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
    public class CriarContratoFinCommandHandler : ICommandHandler<CriarContratoFinCommand>
    {
        private readonly ContextDMS _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public CriarContratoFinCommandHandler(
            ContextDMS context,
            ITenantProvider tenantProvider,
            ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(CriarContratoFinCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var entidade = new ContratoFin(
                request.PropostaId,
                request.VendaId,
                request.NumeroContrato,
                request.CondicaoFinalJson,
                tenantId,
                usuario);

            if (!entidade.IsValid)
            {
                return CommandResult.Falha(entidade.Notifications.Select(n => n.Message));
            }

            _context.ContratosFin.Add(entidade);

            _context.OutboxMessages.Add(new OutboxMessage(tenantId,
                CatalogoEventosIntegracao.Concessionarias.FinContratoEmitido,
                JsonSerializer.Serialize(new { contratoId = entidade.Id, vendaId = entidade.VendaId, numeroContrato = entidade.NumeroContrato, tenantId })));

            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Contrato de financiamento emitido com sucesso!", new { entidade.Id });
        }
    }
}
