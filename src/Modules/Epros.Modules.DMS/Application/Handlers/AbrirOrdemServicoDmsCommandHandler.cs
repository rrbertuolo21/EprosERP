using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Shared.Domain.Events;
using Epros.Modules.DMS.Application.Commands;
using Epros.Modules.DMS.Domain.Entities;
using Epros.Modules.DMS.Infrastructure.Data;

namespace Epros.Modules.DMS.Application.Handlers
{
    public class AbrirOrdemServicoDmsCommandHandler : ICommandHandler<AbrirOrdemServicoDmsCommand>
    {
        private readonly ContextDMS _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public AbrirOrdemServicoDmsCommandHandler(
            ContextDMS context,
            ITenantProvider tenantProvider,
            ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(AbrirOrdemServicoDmsCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var os = new OrdemServicoDms(
                request.NumeroOs,
                request.VeiculoChassi,
                request.DescricaoInconveniente,
                request.ValorPecas,
                request.ValorMaoDeObra,
                request.ReclamacaoGarantia,
                tenantId,
                usuario
            );

            if (!os.IsValid)
            {
                return CommandResult.Falha(os.Notifications.Select(n => n.Message));
            }

            _context.OrdensServicoDms.Add(os);

            _context.OutboxMessages.Add(new OutboxMessage(tenantId,
                CatalogoEventosIntegracao.Concessionarias.MntOrdemServicoAberta,
                JsonSerializer.Serialize(new { osId = os.Id, numeroOs = os.NumeroOs, chassi = os.VeiculoChassi, garantia = os.ReclamacaoGarantia, tenantId })));

            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Ordem de serviço da oficina aberta com sucesso!", new { OsId = os.Id, Status = os.Status, StatusGarantia = os.StatusGarantia });
        }
    }
}
