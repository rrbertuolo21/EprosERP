using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Modules.RH.Application.Commands;
using Epros.Modules.RH.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.RH.Application.Handlers
{
    public class DesligarColaboradorCommandHandler : ICommandHandler<DesligarColaboradorCommand>
    {
        private readonly ContextRH _context;
        private readonly ICurrentUser _currentUser;

        public DesligarColaboradorCommandHandler(
            ContextRH context,
            ICurrentUser currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(DesligarColaboradorCommand request, CancellationToken cancellationToken)
        {
            var usuario = _currentUser.GetUserId() ?? "system";

            var colaborador = await _context.Colaboradores
                .FirstOrDefaultAsync(c => c.Id == request.ColaboradorId, cancellationToken);

            if (colaborador == null)
            {
                return CommandResult.Falha("Colaborador não encontrado.");
            }

            colaborador.Desligar(request.DataDemissao, usuario);

            if (!colaborador.IsValid)
            {
                return CommandResult.Falha(colaborador.Notifications.Select(n => n.Message));
            }

            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Desligamento registrado com sucesso!", new { ColaboradorId = colaborador.Id, Status = colaborador.Status });
        }
    }
}
