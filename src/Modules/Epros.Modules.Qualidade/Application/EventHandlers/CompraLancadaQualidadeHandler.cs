using System;
using System.Threading;
using System.Threading.Tasks;
using Epros.Shared.Domain.Events;
using Epros.Modules.Qualidade.Domain.Entities;
using Epros.Modules.Qualidade.Infrastructure.Data;
using MediatR;

namespace Epros.Modules.Qualidade.Application.EventHandlers
{
    public class CompraLancadaQualidadeHandler : INotificationHandler<CompraLancadaEventNotification>
    {
        private readonly ContextQualidade _context;

        public CompraLancadaQualidadeHandler(ContextQualidade context)
        {
            _context = context;
        }

        public async Task Handle(CompraLancadaEventNotification notification, CancellationToken cancellationToken)
        {
            if (notification.Itens == null || notification.Itens.Count == 0)
                return;

            foreach (var item in notification.Itens)
            {
                var inspecao = new InspecaoLote(
                    notification.CompraId,
                    item.Sku,
                    item.NomeProduto,
                    item.Quantidade,
                    notification.TenantId,
                    "system_event"
                );

                if (inspecao.IsValid)
                {
                    _context.InspecoesLote.Add(inspecao);
                }
            }

            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
