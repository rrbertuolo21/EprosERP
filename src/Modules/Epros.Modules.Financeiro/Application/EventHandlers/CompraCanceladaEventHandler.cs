using System;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Financeiro.Infrastructure.Data;
using Epros.Shared.Domain.Enums;
using Epros.Shared.Domain.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Financeiro.Application.EventHandlers
{
    /// <summary>
    /// Consome o evento CompraCancelada publicado pelo módulo de Estoque.
    /// Cancela o título a pagar (modelo FIEL: ContasAPagar) gerado pela compra de origem.
    /// </summary>
    public class CompraCanceladaEventHandler : INotificationHandler<CompraCanceladaEventNotification>
    {
        private readonly ContextFinanceiro _context;

        public CompraCanceladaEventHandler(ContextFinanceiro context)
        {
            _context = context;
        }

        public async Task Handle(CompraCanceladaEventNotification notification, CancellationToken cancellationToken)
        {
            // Localiza o título a pagar ativo gerado por esta compra (via FatoGerador.CompraId).
            var conta = await _context.ContasAPagarAgregado
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(c => c.TenantId == notification.TenantId
                                       && c.FatoGeradorFinanceiro.CompraId == notification.CompraId
                                       && c.Situacao != ESituacao.Cancelado, cancellationToken);

            if (conta == null)
                return; // Já cancelado, inexistente ou já pago.

            // Só é possível cancelar títulos ainda em aberto (não pagos).
            if (conta.Situacao == ESituacao.Aberto)
            {
                conta.Cancelar("Cancelamento da compra de origem no Estoque.", notification.UserId);

                if (conta.IsValid)
                {
                    // Wiring evento→ledger (TEC-8): estorno contábil do cancelamento da compra
                    // (de-para = valida-contador). Idempotente por (evento + compraId).
                    await Services.ContabilizacaoEventoService.GerarLancamentoAsync(
                        _context, notification.TenantId, "system",
                        Epros.Shared.Domain.Events.CatalogoEventosIntegracao.Financeiro.CompraCanceladaEstorno,
                        notification.CompraId, conta.ValorTitulo,
                        $"Estorno do cancelamento da compra {notification.CompraId}", cancellationToken);

                    await _context.SaveChangesAsync(cancellationToken);
                }
            }
        }
    }
}
