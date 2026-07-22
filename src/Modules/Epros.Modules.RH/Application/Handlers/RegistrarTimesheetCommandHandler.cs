using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Modules.RH.Application.Commands;
using Epros.Modules.RH.Domain.Entities;
using Epros.Modules.RH.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.RH.Application.Handlers
{
    public class RegistrarTimesheetCommandHandler : ICommandHandler<RegistrarTimesheetCommand>
    {
        private readonly ContextRH _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public RegistrarTimesheetCommandHandler(
            ContextRH context,
            ITenantProvider tenantProvider,
            ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(RegistrarTimesheetCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            // Validar se o colaborador existe e está ativo
            var colaborador = await _context.Colaboradores
                .FirstOrDefaultAsync(c => c.Id == request.ColaboradorId, cancellationToken);

            if (colaborador == null)
            {
                return CommandResult.Falha("Colaborador não encontrado.");
            }

            if (colaborador.Status != "Ativo")
            {
                return CommandResult.Falha("Não é possível registrar horas (timesheet) para um colaborador desligado.");
            }

            var timesheet = new Timesheet(
                request.ColaboradorId,
                request.Data,
                request.HorasTrabalhadas,
                request.DescricaoAtividade,
                tenantId,
                usuario
            );

            if (!timesheet.IsValid)
            {
                return CommandResult.Falha(timesheet.Notifications.Select(n => n.Message));
            }

            _context.Timesheets.Add(timesheet);
            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Horas de trabalho (timesheet) registradas com sucesso!", new { TimesheetId = timesheet.Id });
        }
    }
}
