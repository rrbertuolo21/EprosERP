using System;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.GRC.Domain.Entities;
using Epros.Modules.GRC.Infrastructure.Data;
using Epros.Shared.Domain.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.GRC.Application.EventHandlers
{
    /// <summary>
    /// Consome o evento DenunciaProcedente enviado pelo outbox do módulo de GRC.
    /// Cria automaticamente um novo Incidente de Compliance para investigação do compliance officer.
    /// </summary>
    public class DenunciaProcedenteComplianceHandler : INotificationHandler<DenunciaProcedenteEventNotification>
    {
        private readonly ContextGRC _context;

        public DenunciaProcedenteComplianceHandler(ContextGRC context)
        {
            _context = context;
        }

        public async Task Handle(DenunciaProcedenteEventNotification notification, CancellationToken cancellationToken)
        {
            var identificador = $"Denúncia procedente id {notification.DenunciaId}";

            // 1. Evitar reprocessamento (idempotência)
            var jaExiste = await _context.IncidentesCompliance
                .IgnoreQueryFilters()
                .AnyAsync(i => i.TenantId == notification.TenantId &&
                               i.Descricao.Contains(identificador), cancellationToken);

            if (jaExiste)
            {
                return;
            }

            // 2. Criar novo Incidente de Compliance
            var incidente = new IncidenteCompliance(
                titulo: "Incidente GRC — Investigação de Denúncia Procedente",
                descricao: $"{identificador}. Relato: {notification.Relato}. Parecer Final: {notification.ParecerFinal}",
                origem: "Denuncia",
                gravidade: "Critica",
                tenantId: notification.TenantId,
                criadoPor: "system_grc"
            );

            if (incidente.IsValid)
            {
                _context.IncidentesCompliance.Add(incidente);
                await _context.SaveChangesAsync(cancellationToken);
            }
        }
    }
}
