using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Modules.GestaoClientes.Application.Commands;
using Epros.Modules.GestaoClientes.Domain.Entities;
using Epros.Modules.GestaoClientes.Infrastructure.Data;

namespace Epros.Modules.GestaoClientes.Application.Handlers
{
    /// <summary>
    /// T-02 / P1-3 — Processa um lote de importação de pessoas: cria o registro de lote, materializa uma
    /// linha por entrada (com os dados originais preservados), e delega a criação de cada Pessoa ao
    /// handler canônico (reaproveitando limites/dedup/validação de domínio). Regista o resultado
    /// (aceitas/rejeitadas) e o status final no lote.
    /// </summary>
    public class ImportarPessoasLoteCommandHandler : ICommandHandler<ImportarPessoasLoteCommand>
    {
        private readonly ContextGestaoClientes _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;
        private readonly IValidadorLimitesSaaS _validadorLimites;

        public ImportarPessoasLoteCommandHandler(
            ContextGestaoClientes context,
            ITenantProvider tenantProvider,
            ICurrentUser currentUser,
            IValidadorLimitesSaaS validadorLimites)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
            _validadorLimites = validadorLimites;
        }

        public async Task<CommandResult> Handle(ImportarPessoasLoteCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var linhasInput = request.Linhas ?? new List<PessoaImportacaoLinhaInput>();
            if (linhasInput.Count == 0)
                return CommandResult.Falha(new[] { "O lote não contém linhas para importar." }, "Erro de validação");

            var lote = new PessoaImportacaoLote(request.NomeArquivo, request.LayoutVersao, linhasInput.Count, tenantId, usuario);
            if (!lote.IsValid)
                return CommandResult.Falha(lote.Notifications.Select(n => n.Message), "Dados do lote inválidos.");

            // 1) Validação de mapeamento (não persiste): decide o gate tudo-ou-nada antes de criar Pessoas.
            var errosMapeamento = linhasInput.ToDictionary(l => l.NumeroLinha, ValidarMapeamento);
            var houveErroMapeamento = errosMapeamento.Values.Any(e => e != null);

            // 2) Política tudo-ou-nada: aborta sem criar nenhuma Pessoa.
            if (request.AbortarSeHouverErro && houveErroMapeamento)
            {
                foreach (var input in linhasInput.OrderBy(l => l.NumeroLinha))
                {
                    var linha = NovaLinha(lote.Id, input, tenantId, usuario);
                    var erro = errosMapeamento[input.NumeroLinha] ?? "Lote abortado: outra linha falhou na validação (tudo-ou-nada).";
                    linha.Rejeitar(erro, usuario);
                    lote.AdicionarLinha(linha);
                }
                lote.RegistrarResultado(0, linhasInput.Count, EStatusImportacao.Rejeitada, usuario);
                _context.PessoasImportacaoLote.Add(lote);
                await _context.SaveChangesAsync(cancellationToken);
                return CommandResult.Ok("Lote rejeitado: há linhas inválidas e a política é tudo-ou-nada.",
                    new { LoteId = lote.Id, lote.LinhasAceitas, lote.LinhasRejeitadas, Status = lote.Status.ToString() });
            }

            // 3) Processamento parcial: cria as válidas, rejeita as inválidas. Reaproveita o handler canônico.
            var criarPessoa = new CriarPessoaCommandHandler(_context, _tenantProvider, _currentUser, _validadorLimites);
            int aceitas = 0, rejeitadas = 0;

            foreach (var input in linhasInput.OrderBy(l => l.NumeroLinha))
            {
                var linha = NovaLinha(lote.Id, input, tenantId, usuario);

                var erroMap = errosMapeamento[input.NumeroLinha];
                if (erroMap != null)
                {
                    linha.Rejeitar(erroMap, usuario);
                    rejeitadas++;
                }
                else
                {
                    var resultado = await criarPessoa.Handle(MapearParaCriarPessoa(input), cancellationToken);
                    if (resultado.Sucesso && resultado.Dados != null)
                    {
                        var pessoaId = (Guid)resultado.Dados.GetType().GetProperty("PessoaId")!.GetValue(resultado.Dados)!;
                        linha.Aceitar(pessoaId, usuario);
                        aceitas++;
                    }
                    else
                    {
                        var msg = resultado.Erros?.Any() == true ? string.Join("; ", resultado.Erros) : (resultado.Mensagem ?? "Falha ao criar a pessoa.");
                        linha.Rejeitar(msg, usuario);
                        rejeitadas++;
                    }
                }

                lote.AdicionarLinha(linha);
            }

            var status = rejeitadas == 0
                ? EStatusImportacao.Concluida
                : (aceitas == 0 ? EStatusImportacao.Rejeitada : EStatusImportacao.ConcluidaComErros);
            lote.RegistrarResultado(aceitas, rejeitadas, status, usuario);

            _context.PessoasImportacaoLote.Add(lote);
            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok(
                $"Importação concluída: {aceitas} aceita(s), {rejeitadas} rejeitada(s).",
                new { LoteId = lote.Id, lote.LinhasAceitas, lote.LinhasRejeitadas, Status = lote.Status.ToString() });
        }

        /// <summary>Validação de mapeamento (presença dos campos mínimos por tipo). null = ok.</summary>
        private static string? ValidarMapeamento(PessoaImportacaoLinhaInput input)
        {
            if (input.NumeroLinha <= 0)
                return "Número da linha deve ser maior que zero.";
            if (string.IsNullOrWhiteSpace(input.Nome))
                return "Nome/razão social é obrigatório.";
            if (input.TipoPessoa == ETipoPessoa.PessoaFisica && string.IsNullOrWhiteSpace(input.Documento))
                return "CPF é obrigatório para pessoa física.";
            if (input.TipoPessoa == ETipoPessoa.PessoaJuridica && string.IsNullOrWhiteSpace(input.Documento))
                return "CNPJ é obrigatório para pessoa jurídica.";
            return null;
        }

        private static PessoaImportacaoLinha NovaLinha(Guid loteId, PessoaImportacaoLinhaInput input, string tenantId, string usuario)
        {
            var dadosOriginais = JsonSerializer.Serialize(input);
            return new PessoaImportacaoLinha(loteId, input.NumeroLinha, dadosOriginais, tenantId, usuario);
        }

        private static CriarPessoaCommand MapearParaCriarPessoa(PessoaImportacaoLinhaInput input)
        {
            List<ContatoDto>? contatos = null;
            if (!string.IsNullOrWhiteSpace(input.Email) || !string.IsNullOrWhiteSpace(input.Telefone))
            {
                contatos = new List<ContatoDto>
                {
                    new ContatoDto(
                        Nome: input.Nome,
                        TipoContatoTelefonico: string.IsNullOrWhiteSpace(input.Telefone) ? ETipoContatoTelefonico.NaoUtiliza : ETipoContatoTelefonico.Comercial,
                        NumeroTelefone: input.Telefone,
                        TipoContatoEmail: string.IsNullOrWhiteSpace(input.Email) ? ETipoContatoEmail.NaoUtiliza : ETipoContatoEmail.EnvioNFe,
                        Email: input.Email,
                        EhPrincipal: true)
                };
            }

            var ehFisica = input.TipoPessoa == ETipoPessoa.PessoaFisica;
            var ehJuridica = input.TipoPessoa == ETipoPessoa.PessoaJuridica;
            var ehEstrangeira = input.TipoPessoa == ETipoPessoa.PessoaEstrangeira;

            return new CriarPessoaCommand(
                TipoPessoa: input.TipoPessoa,
                TipoIndicadorIe: ETipoIndicadorIe.NaoContribuinte,
                PessoaGrupoId: null,
                InscricaoSuframa: null,
                TitularContaBancaria: null,
                AgenciaContaBancaria: null,
                NumeroContaBancaria: null,
                TipoPix: null,
                ChavePix: null,
                Observacoes: null,

                // Física
                FisicaCpf: ehFisica ? input.Documento : null,
                FisicaNome: ehFisica ? input.Nome : null,
                FisicaSobrenome: ehFisica ? input.Sobrenome : null,
                RgNumero: null,
                RgOrgaoEmissor: null,
                TipoGenero: null,
                DataNascimento: null,

                // Jurídica
                JuridicaCnpj: ehJuridica ? input.Documento : null,
                RazaoSocial: ehJuridica ? input.Nome : null,
                NomeFantasia: ehJuridica ? input.NomeFantasia : null,
                InscricaoEstadual: null,
                InscricaoMunicipal: null,
                Cnae: null,

                // Estrangeira
                EstrangeiroNome: ehEstrangeira ? input.Nome : null,
                IdentificacaoEstrangeiro: ehEstrangeira ? input.Documento : null,

                // Papéis
                EhCliente: input.EhCliente,
                EhFornecedor: false,
                EhTransportadora: false,
                EhMotorista: false,
                EhPrestadorServico: false,
                EhFuncionario: false,
                EhProdutorRural: false,

                // Dados de papel
                ClienteEhConsumidorFinal: null,
                ClienteTipoContribuinte: null,
                FuncionarioTipoCargo: null,
                FuncionarioComissao: null,
                MotoristaTipoVinculo: null,
                MotoristaCategoriaCnh: null,
                MotoristaDataEmissaoCnh: null,
                MotoristaDataVencimentoCnh: null,
                MotoristaRntrc: null,
                TransportadoraCiot: null,
                TransportadoraRntrc: null,
                PrestadorCei: null,

                // Coleções
                Enderecos: null,
                Contatos: contatos,
                Veiculos: null);
        }
    }
}
