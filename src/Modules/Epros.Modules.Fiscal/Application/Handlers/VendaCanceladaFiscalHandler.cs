using System;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Fiscal.Domain.Entities;
using Epros.Modules.Fiscal.Infrastructure.Data;
using Epros.Shared.Domain.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Fiscal.Application.Handlers
{
    public class VendaCanceladaFiscalHandler : INotificationHandler<VendaCanceladaEventNotification>
    {
        private readonly ContextFiscal _context;

        public VendaCanceladaFiscalHandler(ContextFiscal context)
        {
            _context = context;
        }

        public async Task Handle(VendaCanceladaEventNotification notification, CancellationToken cancellationToken)
        {
            // 1. Localizar o rascunho de documento fiscal correspondente à venda de origem
            var documento = await _context.DocumentosFiscais
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(d => d.TenantId == notification.TenantId &&
                                           d.VendaOrigemId == notification.VendaId &&
                                           d.Status != "Cancelada", cancellationToken);

            if (documento == null)
            {
                return; // Documento já cancelado ou inexistente
            }

            // 2. Se for rascunho, cancelamos direto no banco
            if (documento.Status == "Rascunho")
            {
                documento.Cancelar("Venda de origem cancelada.", string.Empty, notification.UserId);

                if (documento.IsValid)
                {
                    _context.DocumentosFiscais.Update(documento);
                    await _context.SaveChangesAsync(cancellationToken);

                    Console.WriteLine($"[Fiscal] Rascunho de NFC-e ID {documento.Id} cancelado com sucesso devido ao cancelamento da Venda {notification.VendaId}.");
                }
            }
        }
    }
}
