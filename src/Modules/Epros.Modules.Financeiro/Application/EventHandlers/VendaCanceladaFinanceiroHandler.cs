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
    /// Consome o evento VendaCancelada publicado pelo módulo Vendas.
    /// Cancela o título a receber (modelo FIEL: ContasAReceber) gerado pela venda de origem.
    /// </summary>
    public class VendaCanceladaFinanceiroHandler : INotificationHandler<VendaCanceladaEventNotification>
    {
        private readonly ContextFinanceiro _context;

        public VendaCanceladaFinanceiroHandler(ContextFinanceiro context)
        {
            _context = context;
        }

        public async Task Handle(VendaCanceladaEventNotification notification, CancellationToken cancellationToken)
        {
            var conta = await _context.ContasAReceberAgregado
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(c => c.TenantId == notification.TenantId
                                       && c.FatoGeradorFinanceiro.VendaId == notification.VendaId
                                       && c.Situacao != ESituacao.Cancelado, cancellationToken);

            if (conta == null)
                return; // Já cancelado ou inexistente.

            if (conta.Situacao == ESituacao.Aberto)
            {
                conta.Cancelar("Cancelamento da venda de origem no PDV/Faturamento.", notification.UserId);

                if (conta.IsValid)
                    await _context.SaveChangesAsync(cancellationToken);
            }
        }
    }
}
