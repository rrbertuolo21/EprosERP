using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Modules.GRC.Application.Commands;
using Epros.Modules.GRC.Domain.Entities;
using Epros.Modules.GRC.Infrastructure.Data;

namespace Epros.Modules.GRC.Application.Handlers
{
    public class RegistrarDenunciaCommandHandler : ICommandHandler<RegistrarDenunciaCommand>
    {
        private readonly ContextGRC _context;
        private readonly ITenantProvider _tenantProvider;

        public RegistrarDenunciaCommandHandler(
            ContextGRC context,
            ITenantProvider tenantProvider)
        {
            _context = context;
            _tenantProvider = tenantProvider;
        }

        public async Task<CommandResult> Handle(RegistrarDenunciaCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            // Denúncias são anônimas, então o autor é marcado como "anonymous"
            var usuario = "anonymous";

            var denuncia = new Denuncia(
                request.Relato,
                tenantId,
                usuario
            );

            if (!denuncia.IsValid)
            {
                return CommandResult.Falha(denuncia.Notifications.Select(n => n.Message));
            }

            _context.Denuncias.Add(denuncia);
            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Denúncia registrada anonimamente com sucesso!", new { DenunciaId = denuncia.Id, CodigoAcompanhamento = denuncia.CodigoAcompanhamento });
        }
    }
}
