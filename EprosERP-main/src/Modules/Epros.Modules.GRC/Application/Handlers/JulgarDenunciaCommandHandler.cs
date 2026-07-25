using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Shared.Domain.Events;
using Epros.Modules.GRC.Application.Commands;
using Epros.Modules.GRC.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.GRC.Application.Handlers
{
    public class JulgarDenunciaCommandHandler : ICommandHandler<JulgarDenunciaCommand>
    {
        private readonly ContextGRC _context;
        private readonly ICurrentUser _currentUser;
        private readonly ITenantProvider _tenantProvider;

        public JulgarDenunciaCommandHandler(
            ContextGRC context,
            ICurrentUser currentUser,
            ITenantProvider tenantProvider)
        {
            _context = context;
            _currentUser = currentUser;
            _tenantProvider = tenantProvider;
        }

        public async Task<CommandResult> Handle(JulgarDenunciaCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var denuncia = await _context.Denuncias
                .FirstOrDefaultAsync(d => d.Id == request.DenunciaId, cancellationToken);

            if (denuncia == null)
            {
                return CommandResult.Falha("Denúncia não encontrada.");
            }

            denuncia.Julgar(request.StatusFinal, request.ParecerFinal, usuario);

            if (!denuncia.IsValid)
            {
                return CommandResult.Falha(denuncia.Notifications.Select(n => n.Message));
            }

            if (request.StatusFinal == "Procedente")
            {
                var payload = new
                {
                    DenunciaId = denuncia.Id,
                    Relato = denuncia.Relato,
                    ParecerFinal = denuncia.ParecerFinal,
                    TenantId = tenantId
                };

                var payloadJson = JsonSerializer.Serialize(payload);
                var outboxMsg = new OutboxMessage(tenantId, "DenunciaProcedente", payloadJson);
                _context.OutboxMessages.Add(outboxMsg);
            }

            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok($"Denúncia julgada como {request.StatusFinal} com sucesso!", new { DenunciaId = denuncia.Id, Status = denuncia.Status });
        }
    }
}
