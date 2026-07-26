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
    /// Consome o evento FolhaProcessada enviado pelo outbox do módulo de RH.
    /// Cria um título a pagar (modelo FIEL: ContasAPagar + FatoGeradorFinanceiro) para o salário do colaborador.
    /// </summary>
    public class FolhaProcessadaFinanceiroHandler : INotificationHandler<FolhaProcessadaEventNotification>
    {
        private readonly ContextFinanceiro _context;

        public FolhaProcessadaFinanceiroHandler(ContextFinanceiro context)
        {
            _context = context;
        }

        public async Task Handle(FolhaProcessadaEventNotification notification, CancellationToken cancellationToken)
        {
            var vencimento = DateTime.UtcNow.Date.AddDays(5);

            var planoItemId = await FinanceiroFielFactory.GarantirPlanoItemPadraoAsync(
                _context, notification.TenantId, "system_payroll", cancellationToken);

            var fato = FinanceiroFielFactory.CriarFatoGerador(_context, EOrigem.ManualContasAPagar,
                null, null,
                $"Salário - Competência {notification.MesCompetencia:D2}/{notification.AnoCompetencia}",
                notification.TenantId, "system_payroll");

            var documento = $"FOLHA-{notification.FolhaPagamentoId.ToString().Substring(0, 8).ToUpper()}";

            var conta = new ContasAPagar(
                pessoaId: notification.ColaboradorId,
                planoDeContasFinanceiroItemId: planoItemId,
                fatoGeradorFinanceiroId: fato.Id,
                nomePessoa: notification.NomeColaborador,
                situacao: ESituacao.Aberto,
                dataVencimento: vencimento,
                dataEmissao: DateTime.UtcNow.Date,
                dataBaixa: null,
                documento: documento.Length > 30 ? documento.Substring(0, 30) : documento,
                valorTitulo: notification.SalarioLiquido,
                valorInicialDesconto: 0m,
                valorInicialMulta: 0m,
                valorInicialJuros: 0m,
                valorInicialAcrescimo: 0m,
                numeroParcela: 1,
                detalhamento: $"Pagamento referente ao salário líquido de {notification.NomeColaborador} (CPF: {notification.CpfColaborador}).",
                justificativaCancelamento: null,
                tenantId: notification.TenantId,
                criadoPor: "system_payroll");

            if (conta.IsValid)
            {
                _context.ContasAPagarAgregado.Add(conta);
                await _context.SaveChangesAsync(cancellationToken);
            }
        }
    }
}
