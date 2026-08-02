using System;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Financeiro.Domain.Entities;
using Epros.Modules.Financeiro.Infrastructure.Data;
using Epros.Shared.Domain.Enums;
using Epros.Shared.Domain.Events;
using MediatR;

namespace Epros.Modules.Financeiro.Application.Handlers
{
    /// <summary>
    /// Consome o evento ProjetoFaturado enviado pelo outbox do módulo de Projetos.
    /// Cria um título a receber (modelo FIEL: ContasAReceber + FatoGeradorFinanceiro) para o marco faturado.
    /// </summary>
    public class ProjetoFaturadoFinanceiroHandler : INotificationHandler<ProjetoFaturadoEventNotification>
    {
        private readonly ContextFinanceiro _context;

        public ProjetoFaturadoFinanceiroHandler(ContextFinanceiro context)
        {
            _context = context;
        }

        public async Task Handle(ProjetoFaturadoEventNotification notification, CancellationToken cancellationToken)
        {
            var vencimento = DateTime.UtcNow.Date.AddDays(15);

            var planoItemId = await FinanceiroFielFactory.GarantirPlanoItemPadraoAsync(
                _context, notification.TenantId, "system_billing", cancellationToken);

            var fato = FinanceiroFielFactory.CriarFatoGerador(_context, EOrigem.ManualContasAReceber,
                null, null,
                $"Faturamento Projeto {notification.NomeProjeto} - Marco {notification.Milestone}%",
                notification.TenantId, "system_billing");

            var documento = $"PRJ-{notification.ProjetoId.ToString().Substring(0, 8).ToUpper()}-{notification.Milestone}";

            var conta = new ContasAReceber(
                pessoaId: notification.ClienteId,
                planoDeContasFinanceiroItemId: planoItemId,
                fatoGeradorFinanceiroId: fato.Id,
                nomePessoa: null,
                situacao: ESituacao.Aberto,
                dataVencimento: vencimento,
                dataEmissao: DateTime.UtcNow.Date,
                dataBaixa: null,
                documento: documento.Length > 30 ? documento.Substring(0, 30) : documento,
                valorTitulo: notification.ValorFaturamento,
                valorInicialDesconto: 0m,
                valorInicialMulta: 0m,
                valorInicialJuros: 0m,
                valorInicialAcrescimo: 0m,
                numeroParcela: 1,
                detalhamento: $"Faturamento automático referente ao marco de conclusão de {notification.Milestone}% atingido pelo projeto.",
                justificativaCancelamento: null,
                tenantId: notification.TenantId,
                criadoPor: "system_billing");

            if (conta.IsValid)
            {
                _context.ContasAReceberAgregado.Add(conta);

                // Wiring evento→ledger (TEC-8): lançamento contábil automático do faturamento do marco.
                await Epros.Modules.Financeiro.Application.Services.ContabilizacaoEventoService.GerarLancamentoAsync(
                    _context, notification.TenantId, "system_billing",
                    Epros.Shared.Domain.Events.CatalogoEventosIntegracao.Vendas.ProjetoFaturado,
                    notification.ProjetoId, notification.ValorFaturamento,
                    $"Projeto {notification.NomeProjeto} marco {notification.Milestone}%", cancellationToken);

                await _context.SaveChangesAsync(cancellationToken);
            }
        }
    }
}
