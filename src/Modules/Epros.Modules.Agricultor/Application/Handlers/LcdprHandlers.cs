using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Agricultor.Application.Commands;
using Epros.Modules.Agricultor.Application.Queries;
using Epros.Modules.Agricultor.Domain.Entities;
using Epros.Modules.Agricultor.Domain.Events;
using Epros.Modules.Agricultor.Domain.Services;
using Epros.Modules.Agricultor.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Agricultor.Application.Handlers
{
    /// <summary>
    /// Handlers do submódulo LIVRO CAIXA DIGITAL (LCDPR). Montam a escrituração (0000→0050 + Q100),
    /// validam contra o leiaute 1.3 e geram o arquivo .txt determinístico (AGR-D19/D20).
    /// </summary>
    public class LcdprCommandHandlers : HandlerAgricultorBase,
        ICommandHandler<AbrirEscrituracaoCommand>,
        ICommandHandler<DefinirDadosCadastraisCommand>,
        ICommandHandler<AdicionarImovelLcdprCommand>,
        ICommandHandler<AdicionarContaLcdprCommand>,
        ICommandHandler<RegistrarLancamentoCommand>,
        ICommandHandler<FecharEscrituracaoCommand>,
        ICommandHandler<ReabrirRetificadoraCommand>,
        ICommandHandler<GerarArquivoLcdprCommand>,
        ICommandHandler<DefinirParamObrigatoriedadeCommand>
    {
        private readonly GeradorArquivoLcdprService _gerador;
        private readonly ValidadorLcdprService _validador;

        public LcdprCommandHandlers(ContextAgricultor c, ITenantProvider t, ICurrentUser u,
            GeradorArquivoLcdprService gerador, ValidadorLcdprService validador) : base(c, t, u)
        { _gerador = gerador; _validador = validador; }

        public async Task<CommandResult> Handle(AbrirEscrituracaoCommand r, CancellationToken ct)
        {
            var esc = new LcdprEscrituracao(r.Cpf, r.Nome, r.DtIni, r.DtFin,
                r.IndSituacaoInicioPeriodo, r.SituacaoEspecial, r.FormaApuracao, Tenant, Usuario);
            if (!esc.IsValid) return CommandResult.Falha(esc.Notifications.Select(n => n.Message));
            Context.Escrituracoes.Add(esc);
            Publicar(EventosAgricultor.EscrituracaoAberta, new { escrituracaoId = esc.Id, esc.Cpf, Tenant });
            await Context.SaveChangesAsync(ct);
            return CommandResult.Ok("Escrituração LCDPR aberta!", new { EscrituracaoId = esc.Id });
        }

        // Nota: as operações de "adicionar filho" carregam o agregado SEM rastreamento (só para rodar a
        // validação de domínio contra 0040/0050/status) e persistem o FILHO diretamente no seu DbSet.
        // Assim não mutamos as coleções de um agregado rastreado (o que o provider InMemory não suporta
        // bem) e a inserção é limpa também no Npgsql.

        public async Task<CommandResult> Handle(DefinirDadosCadastraisCommand r, CancellationToken ct)
        {
            var esc = await CarregarNoTracking(r.EscrituracaoId, ct);
            if (esc is null) return CommandResult.Falha("Escrituração não encontrada.");

            var existente = await Context.LcdprDadosCadastrais.FirstOrDefaultAsync(d => d.EscrituracaoId == esc.Id, ct);
            if (existente is not null) Context.LcdprDadosCadastrais.Remove(existente); // 1:1 — substitui

            var dados = new LcdprDadosCadastrais(r.Endereco, r.Uf, r.CodMunicipio, r.Cep, r.Email, Tenant, Usuario);
            esc.DefinirDadosCadastrais(dados); // vincula ao 0000
            Context.LcdprDadosCadastrais.Add(dados);
            await Context.SaveChangesAsync(ct);
            return CommandResult.Ok("Dados cadastrais (0030) definidos!", new { EscrituracaoId = esc.Id });
        }

        public async Task<CommandResult> Handle(AdicionarImovelLcdprCommand r, CancellationToken ct)
        {
            var esc = await CarregarNoTracking(r.EscrituracaoId, ct);
            if (esc is null) return CommandResult.Falha("Escrituração não encontrada.");

            var imovel = new LcdprImovel(r.CodImovel, r.NomeImovel, r.CadItrCafir, r.Caepf,
                r.Uf, r.CodMunicipio, r.TipoExploracao, r.Participacao, Tenant, Usuario);
            foreach (var ti in r.Terceiros ?? Enumerable.Empty<TerceiroInput>())
                imovel.AdicionarTerceiro(new LcdprTerceiro(ti.TipoContraparte, ti.IdContraparte, ti.NomeContraparte, ti.PercContraparte, Tenant, Usuario));

            esc.AdicionarImovel(imovel); // valida status/duplicidade e vincula
            if (!esc.IsValid) return CommandResult.Falha(esc.Notifications.Select(n => n.Message));
            Context.LcdprImoveis.Add(imovel); // terceiros cascateiam pela navegação
            await Context.SaveChangesAsync(ct);
            return CommandResult.Ok("Imóvel (0040) adicionado!", new { ImovelId = imovel.Id });
        }

        public async Task<CommandResult> Handle(AdicionarContaLcdprCommand r, CancellationToken ct)
        {
            var esc = await CarregarNoTracking(r.EscrituracaoId, ct);
            if (esc is null) return CommandResult.Falha("Escrituração não encontrada.");
            var conta = new LcdprConta(r.CodConta, r.Banco, r.Agencia, r.NumConta, Tenant, Usuario);
            if (!conta.IsValid) return CommandResult.Falha(conta.Notifications.Select(n => n.Message));
            esc.AdicionarConta(conta); // valida duplicidade e vincula
            if (!esc.IsValid) return CommandResult.Falha(esc.Notifications.Select(n => n.Message));
            Context.LcdprContas.Add(conta);
            await Context.SaveChangesAsync(ct);
            return CommandResult.Ok("Conta (0050) adicionada!", new { ContaId = conta.Id });
        }

        public async Task<CommandResult> Handle(RegistrarLancamentoCommand r, CancellationToken ct)
        {
            var esc = await CarregarNoTracking(r.EscrituracaoId, ct);
            if (esc is null) return CommandResult.Falha("Escrituração não encontrada.");

            var lanc = new LcdprLancamento(r.CodImovel, r.CodConta, r.Data, r.TipoDoc, r.NumDoc,
                r.Historico, r.IdPartic, r.TipoLanc, r.VlEntrada, r.VlSaida, Tenant, Usuario);
            esc.AdicionarLancamento(lanc); // valida referências 0040/0050 e domínios
            if (!esc.IsValid) return CommandResult.Falha(esc.Notifications.Select(n => n.Message));

            Context.LcdprLancamentos.Add(lanc);
            Publicar(EventosAgricultor.LancamentoRegistrado, new { escrituracaoId = esc.Id, lancamentoId = lanc.Id, Tenant });
            await Context.SaveChangesAsync(ct);
            return CommandResult.Ok("Lançamento (Q100) registrado!", new { LancamentoId = lanc.Id });
        }

        public async Task<CommandResult> Handle(FecharEscrituracaoCommand r, CancellationToken ct)
        {
            var esc = await CarregarCompleta(r.EscrituracaoId, ct);
            if (esc is null) return CommandResult.Falha("Escrituração não encontrada.");

            var validacao = _validador.Validar(esc);
            if (!validacao.Valido)
                return CommandResult.Falha(validacao.Bloqueantes.Select(b => $"[{b.Codigo}] {b.Mensagem}"),
                    "Escrituração possui pendências bloqueantes.", block: true);

            esc.Fechar(r.IdentificacaoNome, r.IdentificacaoCpfCnpj, Usuario);
            if (!esc.IsValid) return CommandResult.Falha(esc.Notifications.Select(n => n.Message));
            Publicar(EventosAgricultor.EscrituracaoFechada, new { escrituracaoId = esc.Id, Tenant });
            await Context.SaveChangesAsync(ct);
            return CommandResult.Ok("Escrituração fechada e pronta para exportar!",
                new { EscrituracaoId = esc.Id, Alertas = validacao.Alertas });
        }

        public async Task<CommandResult> Handle(ReabrirRetificadoraCommand r, CancellationToken ct)
        {
            var esc = await CarregarCompleta(r.EscrituracaoId, ct);
            if (esc is null) return CommandResult.Falha("Escrituração não encontrada.");
            esc.ReabrirComoRetificadora(Usuario);
            await Context.SaveChangesAsync(ct);
            return CommandResult.Ok("Escrituração reaberta como retificadora (regenera arquivo íntegro).", new { EscrituracaoId = esc.Id });
        }

        public async Task<CommandResult> Handle(GerarArquivoLcdprCommand r, CancellationToken ct)
        {
            var esc = await CarregarCompleta(r.EscrituracaoId, ct);
            if (esc is null) return CommandResult.Falha("Escrituração não encontrada.");

            var validacao = _validador.Validar(esc);
            if (!validacao.Valido)
                return CommandResult.Falha(validacao.Bloqueantes.Select(b => $"[{b.Codigo}] {b.Mensagem}"),
                    "Não é possível gerar o arquivo: há pendências bloqueantes.", block: true);

            var arquivo = _gerador.Gerar(esc);
            esc.MarcarExportada(Usuario);
            Publicar(EventosAgricultor.ArquivoExportado,
                new { escrituracaoId = esc.Id, arquivo.HashSha256, arquivo.QtdLinhas, Tenant });
            await Context.SaveChangesAsync(ct);

            return CommandResult.Ok("Arquivo LCDPR gerado!", new
            {
                NomeArquivo = ArquivoLcdprGerado.NomeArquivo(esc.Cpf, esc.DtFin.Year, DateTime.UtcNow),
                arquivo.Conteudo,
                arquivo.QtdLinhas,
                arquivo.HashSha256,
                Alertas = validacao.Alertas
            });
        }

        public async Task<CommandResult> Handle(DefinirParamObrigatoriedadeCommand r, CancellationToken ct)
        {
            var existente = await Context.LcdprParamsObrigatoriedade.FirstOrDefaultAsync(p => p.Ano == r.Ano, ct);
            if (existente is not null)
            {
                Context.LcdprParamsObrigatoriedade.Remove(existente); // substitui o parâmetro do ano
            }
            var param = new LcdprParamObrigatoriedade(r.Ano, r.LimiteValor, r.Origem, Tenant, Usuario);
            if (!param.IsValid) return CommandResult.Falha(param.Notifications.Select(n => n.Message));
            Context.LcdprParamsObrigatoriedade.Add(param);
            await Context.SaveChangesAsync(ct);
            return CommandResult.Ok("Limite de obrigatoriedade definido!", new { r.Ano, r.LimiteValor });
        }

        private Task<LcdprEscrituracao?> CarregarCompleta(Guid id, CancellationToken ct)
            => Context.Escrituracoes
                .Include(e => e.DadosCadastrais)
                .Include(e => e.Imoveis).ThenInclude(i => i.Terceiros)
                .Include(e => e.Contas)
                .Include(e => e.Lancamentos)
                .FirstOrDefaultAsync(e => e.Id == id, ct);

        private Task<LcdprEscrituracao?> CarregarNoTracking(Guid id, CancellationToken ct)
            => Context.Escrituracoes.AsNoTracking()
                .Include(e => e.DadosCadastrais)
                .Include(e => e.Imoveis).ThenInclude(i => i.Terceiros)
                .Include(e => e.Contas)
                .Include(e => e.Lancamentos)
                .FirstOrDefaultAsync(e => e.Id == id, ct);
    }

    // ===================== Queries LCDPR =====================

    public class LcdprQueryHandlers :
        IQueryHandler<ListarEscrituracoesQuery, CommandResult>,
        IQueryHandler<ObterEscrituracaoQuery, CommandResult>,
        IQueryHandler<ValidarEscrituracaoQuery, CommandResult>,
        IQueryHandler<PreviewArquivoLcdprQuery, CommandResult>
    {
        private readonly ContextAgricultor _c;
        private readonly GeradorArquivoLcdprService _gerador;
        private readonly ValidadorLcdprService _validador;

        public LcdprQueryHandlers(ContextAgricultor c, GeradorArquivoLcdprService gerador, ValidadorLcdprService validador)
        { _c = c; _gerador = gerador; _validador = validador; }

        public async Task<CommandResult> Handle(ListarEscrituracoesQuery r, CancellationToken ct)
            => CommandResult.Ok("Escriturações listadas!", await _c.Escrituracoes.AsNoTracking()
                .OrderByDescending(e => e.DtFin).ToListAsync(ct));

        public async Task<CommandResult> Handle(ObterEscrituracaoQuery r, CancellationToken ct)
        {
            var esc = await Carregar(r.EscrituracaoId, ct);
            return esc is null ? CommandResult.Falha("Escrituração não encontrada.") : CommandResult.Ok("OK", esc);
        }

        public async Task<CommandResult> Handle(ValidarEscrituracaoQuery r, CancellationToken ct)
        {
            var esc = await Carregar(r.EscrituracaoId, ct);
            if (esc is null) return CommandResult.Falha("Escrituração não encontrada.");
            var limite = await _c.LcdprParamsObrigatoriedade.AsNoTracking()
                .Where(p => p.Ano == esc.DtFin.Year).Select(p => (decimal?)p.LimiteValor).FirstOrDefaultAsync(ct);
            var res = _validador.Validar(esc, limite);
            return CommandResult.Ok(res.Valido ? "Escrituração válida." : "Escrituração com pendências.",
                new { res.Valido, res.Bloqueantes, res.Alertas });
        }

        public async Task<CommandResult> Handle(PreviewArquivoLcdprQuery r, CancellationToken ct)
        {
            var esc = await Carregar(r.EscrituracaoId, ct);
            if (esc is null) return CommandResult.Falha("Escrituração não encontrada.");
            var res = _validador.Validar(esc);
            if (!res.Valido)
                return CommandResult.Falha(res.Bloqueantes.Select(b => $"[{b.Codigo}] {b.Mensagem}"),
                    "Preview indisponível: pendências bloqueantes.");
            var arquivo = _gerador.Gerar(esc);
            return CommandResult.Ok("Preview gerado!", new { arquivo.Conteudo, arquivo.QtdLinhas, arquivo.HashSha256 });
        }

        private Task<LcdprEscrituracao?> Carregar(Guid id, CancellationToken ct)
            => _c.Escrituracoes.AsNoTracking()
                .Include(e => e.DadosCadastrais)
                .Include(e => e.Imoveis).ThenInclude(i => i.Terceiros)
                .Include(e => e.Contas)
                .Include(e => e.Lancamentos)
                .FirstOrDefaultAsync(e => e.Id == id, ct);
    }
}
