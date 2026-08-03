using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Agricultor.Application.Commands;
using Epros.Modules.Agricultor.Application.Queries;
using Epros.Modules.Agricultor.Domain.Entities;
using Epros.Modules.Agricultor.Domain.Events;
using Epros.Modules.Agricultor.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Shared.Domain.Events;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Agricultor.Application.Handlers
{
    /// <summary>Base com acesso a tenant/usuário e helper de publicação de evento via Outbox (T2).</summary>
    public abstract class HandlerAgricultorBase
    {
        protected readonly ContextAgricultor Context;
        protected readonly ITenantProvider TenantProvider;
        protected readonly ICurrentUser CurrentUser;

        protected HandlerAgricultorBase(ContextAgricultor context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        { Context = context; TenantProvider = tenantProvider; CurrentUser = currentUser; }

        protected string Tenant => TenantProvider.GetTenantId();
        protected string Usuario => CurrentUser.GetUserId() ?? "system";

        protected void Publicar(string evento, object payload)
            => Context.OutboxMessages.Add(new OutboxMessage(Tenant, evento, JsonSerializer.Serialize(payload)));
    }

    // ===================== Propriedade =====================

    public class CriarPropriedadeCommandHandler : HandlerAgricultorBase, ICommandHandler<CriarPropriedadeCommand>
    {
        public CriarPropriedadeCommandHandler(ContextAgricultor c, ITenantProvider t, ICurrentUser u) : base(c, t, u) { }

        public async Task<CommandResult> Handle(CriarPropriedadeCommand r, CancellationToken ct)
        {
            var prop = new PropriedadeRural(r.NomeImovel, r.Matricula, r.Car, r.CadItrCafir, r.Caepf,
                r.TipoExploracao, r.Participacao, r.Uf, r.CodigoMunicipioSped, r.Cep, r.Endereco, Tenant, Usuario);

            foreach (var ti in r.Talhoes ?? Enumerable.Empty<TalhaoInput>())
                prop.AdicionarTalhao(new Talhao(ti.Nome, ti.CulturaId, ti.PoligonoGeoJson, ti.AreaM2, Tenant, Usuario));

            prop.Validar();
            if (!prop.IsValid) return CommandResult.Falha(prop.Notifications.Select(n => n.Message));

            Context.Propriedades.Add(prop);
            Publicar(EventosAgricultor.PropriedadeCadastrada, new { propriedadeId = prop.Id, Tenant });
            await Context.SaveChangesAsync(ct);
            return CommandResult.Ok("Propriedade cadastrada com sucesso!", new { PropriedadeId = prop.Id });
        }
    }

    public class AlterarPropriedadeCommandHandler : HandlerAgricultorBase, ICommandHandler<AlterarPropriedadeCommand>
    {
        public AlterarPropriedadeCommandHandler(ContextAgricultor c, ITenantProvider t, ICurrentUser u) : base(c, t, u) { }

        public async Task<CommandResult> Handle(AlterarPropriedadeCommand r, CancellationToken ct)
        {
            var prop = await Context.Propriedades.FirstOrDefaultAsync(p => p.Id == r.PropriedadeId, ct);
            if (prop is null) return CommandResult.Falha("Propriedade não encontrada.");
            prop.AtualizarDados(r.NomeImovel, r.Matricula, r.Car, r.CadItrCafir, r.Caepf, r.TipoExploracao,
                r.Participacao, r.Uf, r.CodigoMunicipioSped, r.Cep, r.Endereco, Usuario);
            if (!prop.IsValid) return CommandResult.Falha(prop.Notifications.Select(n => n.Message));
            await Context.SaveChangesAsync(ct);
            return CommandResult.Ok("Propriedade alterada com sucesso!", new { PropriedadeId = prop.Id });
        }
    }

    public class ExcluirPropriedadeCommandHandler : HandlerAgricultorBase, ICommandHandler<ExcluirPropriedadeCommand>
    {
        public ExcluirPropriedadeCommandHandler(ContextAgricultor c, ITenantProvider t, ICurrentUser u) : base(c, t, u) { }

        public async Task<CommandResult> Handle(ExcluirPropriedadeCommand r, CancellationToken ct)
        {
            var prop = await Context.Propriedades.FirstOrDefaultAsync(p => p.Id == r.PropriedadeId, ct);
            if (prop is null) return CommandResult.Falha("Propriedade não encontrada.");
            Context.Propriedades.Remove(prop);
            await Context.SaveChangesAsync(ct);
            return CommandResult.Ok("Propriedade excluída com sucesso!");
        }
    }

    public class AdicionarTalhaoCommandHandler : HandlerAgricultorBase, ICommandHandler<AdicionarTalhaoCommand>
    {
        public AdicionarTalhaoCommandHandler(ContextAgricultor c, ITenantProvider t, ICurrentUser u) : base(c, t, u) { }

        public async Task<CommandResult> Handle(AdicionarTalhaoCommand r, CancellationToken ct)
        {
            var prop = await Context.Propriedades.Include(p => p.Talhoes)
                .FirstOrDefaultAsync(p => p.Id == r.PropriedadeId, ct);
            if (prop is null) return CommandResult.Falha("Propriedade não encontrada.");
            var talhao = new Talhao(r.Nome, r.CulturaId, r.PoligonoGeoJson, r.AreaM2, Tenant, Usuario);
            if (!talhao.IsValid) return CommandResult.Falha(talhao.Notifications.Select(n => n.Message));
            prop.AdicionarTalhao(talhao);
            Context.Talhoes.Add(talhao);
            await Context.SaveChangesAsync(ct);
            return CommandResult.Ok("Talhão adicionado com sucesso!", new { TalhaoId = talhao.Id, talhao.AreaM2, talhao.AreaHectares });
        }
    }

    // ===================== Cadastros =====================

    public class CadastroSimplesHandlers :
        ICommandHandler<CriarCulturaCommand>,
        ICommandHandler<CriarCategoriaDespesaCommand>,
        ICommandHandler<CriarFornecedorCommand>,
        ICommandHandler<CriarColaboradorCommand>,
        ICommandHandler<CriarAnotacaoCommand>
    {
        private readonly ContextAgricultor _c;
        private readonly ITenantProvider _t;
        private readonly ICurrentUser _u;
        public CadastroSimplesHandlers(ContextAgricultor c, ITenantProvider t, ICurrentUser u) { _c = c; _t = t; _u = u; }
        private string Tenant => _t.GetTenantId();
        private string Usuario => _u.GetUserId() ?? "system";

        public async Task<CommandResult> Handle(CriarCulturaCommand r, CancellationToken ct)
        {
            var e = new Cultura(r.Nome, r.Codigo, Tenant, Usuario);
            if (!e.IsValid) return CommandResult.Falha(e.Notifications.Select(n => n.Message));
            _c.Culturas.Add(e); await _c.SaveChangesAsync(ct);
            return CommandResult.Ok("Cultura cadastrada!", new { CulturaId = e.Id });
        }

        public async Task<CommandResult> Handle(CriarCategoriaDespesaCommand r, CancellationToken ct)
        {
            var e = new CategoriaDespesa(r.Nome, r.Codigo, r.EhFolhaMaoDeObra, Tenant, Usuario);
            if (!e.IsValid) return CommandResult.Falha(e.Notifications.Select(n => n.Message));
            _c.CategoriasDespesa.Add(e); await _c.SaveChangesAsync(ct);
            return CommandResult.Ok("Categoria cadastrada!", new { CategoriaId = e.Id });
        }

        public async Task<CommandResult> Handle(CriarFornecedorCommand r, CancellationToken ct)
        {
            var e = new Fornecedor(r.Nome, r.CpfCnpj, r.Codigo, Tenant, Usuario);
            if (!e.IsValid) return CommandResult.Falha(e.Notifications.Select(n => n.Message));
            _c.Fornecedores.Add(e); await _c.SaveChangesAsync(ct);
            return CommandResult.Ok("Fornecedor cadastrado!", new { FornecedorId = e.Id });
        }

        public async Task<CommandResult> Handle(CriarColaboradorCommand r, CancellationToken ct)
        {
            var e = new Colaborador(r.PropriedadeId, r.Nome, r.Email, r.Papel, Tenant, Usuario);
            if (!e.IsValid) return CommandResult.Falha(e.Notifications.Select(n => n.Message));
            _c.Colaboradores.Add(e); await _c.SaveChangesAsync(ct);
            return CommandResult.Ok("Colaborador cadastrado!", new { ColaboradorId = e.Id });
        }

        public async Task<CommandResult> Handle(CriarAnotacaoCommand r, CancellationToken ct)
        {
            var e = new AnotacaoCampo(r.Titulo, r.Descricao, r.Tipo, r.TalhaoId, r.DesignadoA, r.LatLng, r.IsPublic, Tenant, Usuario);
            if (!e.IsValid) return CommandResult.Falha(e.Notifications.Select(n => n.Message));
            _c.Anotacoes.Add(e); await _c.SaveChangesAsync(ct);
            return CommandResult.Ok("Anotação criada!", new { AnotacaoId = e.Id });
        }
    }

    // ===================== Safra =====================

    public class SafraCommandHandlers : HandlerAgricultorBase,
        ICommandHandler<CriarSafraCommand>,
        ICommandHandler<IniciarSafraCommand>,
        ICommandHandler<RegistrarColheitaCommand>,
        ICommandHandler<EncerrarSafraCommand>
    {
        public SafraCommandHandlers(ContextAgricultor c, ITenantProvider t, ICurrentUser u) : base(c, t, u) { }

        public async Task<CommandResult> Handle(CriarSafraCommand r, CancellationToken ct)
        {
            var s = new Safra(r.TalhaoId, r.CulturaId, r.DataPlantio, r.DataColheita, Tenant, Usuario);
            if (!s.IsValid) return CommandResult.Falha(s.Notifications.Select(n => n.Message));
            Context.Safras.Add(s);
            Publicar(EventosAgricultor.SafraAberta, new { safraId = s.Id, Tenant });
            await Context.SaveChangesAsync(ct);
            return CommandResult.Ok("Safra criada!", new { SafraId = s.Id });
        }

        public async Task<CommandResult> Handle(IniciarSafraCommand r, CancellationToken ct)
            => await Transicionar(r.SafraId, s => s.Iniciar(Usuario), "Safra iniciada!", null, ct);

        public async Task<CommandResult> Handle(RegistrarColheitaCommand r, CancellationToken ct)
            => await Transicionar(r.SafraId, s => s.RegistrarColheita(r.DataColheita, Usuario), "Colheita registrada!", null, ct);

        public async Task<CommandResult> Handle(EncerrarSafraCommand r, CancellationToken ct)
            => await Transicionar(r.SafraId, s => s.Encerrar(Usuario), "Safra encerrada!", EventosAgricultor.SafraEncerrada, ct);

        private async Task<CommandResult> Transicionar(Guid id, Action<Safra> acao, string msg, string? evento, CancellationToken ct)
        {
            var s = await Context.Safras.FirstOrDefaultAsync(x => x.Id == id, ct);
            if (s is null) return CommandResult.Falha("Safra não encontrada.");
            acao(s);
            if (!s.IsValid) return CommandResult.Falha(s.Notifications.Select(n => n.Message));
            if (evento != null) Publicar(evento, new { safraId = s.Id, Tenant });
            await Context.SaveChangesAsync(ct);
            return CommandResult.Ok(msg, new { SafraId = s.Id, Status = s.Status.ToString() });
        }
    }

    // ===================== Despesa / Receita =====================

    public class DespesaReceitaCommandHandlers : HandlerAgricultorBase,
        ICommandHandler<RegistrarDespesaCommand>,
        ICommandHandler<ClassificarDespesaLcdprCommand>,
        ICommandHandler<RegistrarReceitaCommand>,
        ICommandHandler<ClassificarReceitaLcdprCommand>
    {
        public DespesaReceitaCommandHandlers(ContextAgricultor c, ITenantProvider t, ICurrentUser u) : base(c, t, u) { }

        public async Task<CommandResult> Handle(RegistrarDespesaCommand r, CancellationToken ct)
        {
            var d = new Despesa(r.PropriedadeId, r.TalhaoId, r.SafraId, r.CategoriaId, r.FornecedorId,
                r.Data, r.Valor, r.Referencia, Tenant, Usuario);
            if (!d.IsValid) return CommandResult.Falha(d.Notifications.Select(n => n.Message));
            Context.Despesas.Add(d);
            Publicar(EventosAgricultor.DespesaRegistrada, new { despesaId = d.Id, d.Valor, Tenant });
            await Context.SaveChangesAsync(ct);
            return CommandResult.Ok("Despesa registrada!", new { DespesaId = d.Id });
        }

        public async Task<CommandResult> Handle(ClassificarDespesaLcdprCommand r, CancellationToken ct)
        {
            var d = await Context.Despesas.FirstOrDefaultAsync(x => x.Id == r.DespesaId, ct);
            if (d is null) return CommandResult.Falha("Despesa não encontrada.");
            d.ClassificarLcdpr(r.CodImovel, r.CodConta, r.TipoDoc, r.TipoLanc, r.IdPartic, Usuario);
            if (!d.IsValid) return CommandResult.Falha(d.Notifications.Select(n => n.Message));
            await Context.SaveChangesAsync(ct);
            return CommandResult.Ok("Despesa classificada para o LCDPR!", new { DespesaId = d.Id });
        }

        public async Task<CommandResult> Handle(RegistrarReceitaCommand r, CancellationToken ct)
        {
            var rec = new ReceitaProducao(r.PropriedadeId, r.TalhaoId, r.SafraId, r.Data,
                r.Quantidade, r.Preco, r.Comprador, r.NumNf, Tenant, Usuario);
            if (!rec.IsValid) return CommandResult.Falha(rec.Notifications.Select(n => n.Message));
            Context.Receitas.Add(rec);
            Publicar(EventosAgricultor.ReceitaRegistrada, new { receitaId = rec.Id, rec.Valor, Tenant });
            await Context.SaveChangesAsync(ct);
            return CommandResult.Ok("Receita registrada!", new { ReceitaId = rec.Id, rec.Valor });
        }

        public async Task<CommandResult> Handle(ClassificarReceitaLcdprCommand r, CancellationToken ct)
        {
            var rec = await Context.Receitas.FirstOrDefaultAsync(x => x.Id == r.ReceitaId, ct);
            if (rec is null) return CommandResult.Falha("Receita não encontrada.");
            rec.ClassificarLcdpr(r.CodImovel, r.CodConta, r.IdPartic, Usuario);
            if (!rec.IsValid) return CommandResult.Falha(rec.Notifications.Select(n => n.Message)); // AGR-D12 bloqueia 000
            await Context.SaveChangesAsync(ct);
            return CommandResult.Ok("Receita classificada para o LCDPR!", new { ReceitaId = rec.Id });
        }
    }

    // ===================== Queries do Painel =====================

    public class PainelQueryHandlers :
        IQueryHandler<ListarPropriedadesQuery, CommandResult>,
        IQueryHandler<ObterPropriedadeQuery, CommandResult>,
        IQueryHandler<ListarSafrasQuery, CommandResult>,
        IQueryHandler<ListarDespesasQuery, CommandResult>,
        IQueryHandler<ListarReceitasQuery, CommandResult>,
        IQueryHandler<ListarCulturasQuery, CommandResult>,
        IQueryHandler<ListarFornecedoresQuery, CommandResult>,
        IQueryHandler<ListarCategoriasDespesaQuery, CommandResult>,
        IQueryHandler<PainelConsolidadoQuery, CommandResult>
    {
        private readonly ContextAgricultor _c;
        public PainelQueryHandlers(ContextAgricultor c) => _c = c;

        public async Task<CommandResult> Handle(ListarPropriedadesQuery r, CancellationToken ct)
            => CommandResult.Ok("Propriedades listadas!", await _c.Propriedades.AsNoTracking()
                .Include(p => p.Talhoes).OrderByDescending(p => p.CriadoEm).ToListAsync(ct));

        public async Task<CommandResult> Handle(ObterPropriedadeQuery r, CancellationToken ct)
        {
            var p = await _c.Propriedades.AsNoTracking().Include(x => x.Talhoes)
                .FirstOrDefaultAsync(x => x.Id == r.PropriedadeId, ct);
            return p is null ? CommandResult.Falha("Propriedade não encontrada.") : CommandResult.Ok("OK", p);
        }

        public async Task<CommandResult> Handle(ListarSafrasQuery r, CancellationToken ct)
        {
            var q = _c.Safras.AsNoTracking().AsQueryable();
            if (r.TalhaoId.HasValue) q = q.Where(s => s.TalhaoId == r.TalhaoId);
            return CommandResult.Ok("Safras listadas!", await q.OrderByDescending(s => s.CriadoEm).ToListAsync(ct));
        }

        public async Task<CommandResult> Handle(ListarDespesasQuery r, CancellationToken ct)
        {
            var q = _c.Despesas.AsNoTracking().AsQueryable();
            if (r.PropriedadeId.HasValue) q = q.Where(d => d.PropriedadeId == r.PropriedadeId);
            if (r.SafraId.HasValue) q = q.Where(d => d.SafraId == r.SafraId);
            return CommandResult.Ok("Despesas listadas!", await q.OrderByDescending(d => d.Data).ToListAsync(ct));
        }

        public async Task<CommandResult> Handle(ListarReceitasQuery r, CancellationToken ct)
        {
            var q = _c.Receitas.AsNoTracking().AsQueryable();
            if (r.PropriedadeId.HasValue) q = q.Where(d => d.PropriedadeId == r.PropriedadeId);
            if (r.SafraId.HasValue) q = q.Where(d => d.SafraId == r.SafraId);
            return CommandResult.Ok("Receitas listadas!", await q.OrderByDescending(d => d.Data).ToListAsync(ct));
        }

        public async Task<CommandResult> Handle(ListarCulturasQuery r, CancellationToken ct)
            => CommandResult.Ok("Culturas listadas!", await _c.Culturas.AsNoTracking().OrderBy(x => x.Nome).ToListAsync(ct));

        public async Task<CommandResult> Handle(ListarFornecedoresQuery r, CancellationToken ct)
            => CommandResult.Ok("Fornecedores listados!", await _c.Fornecedores.AsNoTracking().OrderBy(x => x.Nome).ToListAsync(ct));

        public async Task<CommandResult> Handle(ListarCategoriasDespesaQuery r, CancellationToken ct)
            => CommandResult.Ok("Categorias listadas!", await _c.CategoriasDespesa.AsNoTracking().OrderBy(x => x.Nome).ToListAsync(ct));

        /// <summary>Painel do agricultor: consolida saldos, resultado por atividade (safra) e indicadores.</summary>
        public async Task<CommandResult> Handle(PainelConsolidadoQuery r, CancellationToken ct)
        {
            var despesas = _c.Despesas.AsNoTracking().AsQueryable();
            var receitas = _c.Receitas.AsNoTracking().AsQueryable();
            if (r.PropriedadeId.HasValue)
            {
                despesas = despesas.Where(d => d.PropriedadeId == r.PropriedadeId);
                receitas = receitas.Where(d => d.PropriedadeId == r.PropriedadeId);
            }
            if (r.Ano.HasValue)
            {
                despesas = despesas.Where(d => d.Data.Year == r.Ano);
                receitas = receitas.Where(d => d.Data.Year == r.Ano);
            }

            var listaDespesas = await despesas.ToListAsync(ct);
            var listaReceitas = await receitas.ToListAsync(ct);

            var totalDespesa = listaDespesas.Sum(d => d.Valor);
            var totalReceita = listaReceitas.Sum(d => d.Valor);

            var resultadoPorSafra = listaReceitas
                .Where(x => x.SafraId.HasValue)
                .GroupBy(x => x.SafraId!.Value)
                .Select(g => new
                {
                    SafraId = g.Key,
                    Receita = g.Sum(x => x.Valor),
                    Despesa = listaDespesas.Where(d => d.SafraId == g.Key).Sum(d => d.Valor)
                })
                .Select(x => new { x.SafraId, x.Receita, x.Despesa, Resultado = x.Receita - x.Despesa })
                .ToList();

            var painel = new
            {
                r.PropriedadeId,
                r.Ano,
                TotalReceita = totalReceita,
                TotalDespesa = totalDespesa,
                Saldo = totalReceita - totalDespesa,
                QtdReceitas = listaReceitas.Count,
                QtdDespesas = listaDespesas.Count,
                ResultadoPorSafra = resultadoPorSafra
            };
            return CommandResult.Ok("Painel consolidado!", painel);
        }
    }
}
