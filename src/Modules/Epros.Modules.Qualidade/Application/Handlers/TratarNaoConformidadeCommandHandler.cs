using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Modules.Qualidade.Application.Commands;
using Epros.Modules.Qualidade.Domain.Entities;
using Epros.Modules.Qualidade.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Qualidade.Application.Handlers
{
    public class TratarNaoConformidadeCommandHandler : ICommandHandler<TratarNaoConformidadeCommand>
    {
        private readonly ContextQualidade _context;
        private readonly ICurrentUser _currentUser;

        public TratarNaoConformidadeCommandHandler(
            ContextQualidade context,
            ICurrentUser currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(TratarNaoConformidadeCommand request, CancellationToken cancellationToken)
        {
            var usuario = _currentUser.GetUserId() ?? "system";

            var ncr = await _context.NaoConformidades
                .FirstOrDefaultAsync(n => n.Id == request.NaoConformidadeId, cancellationToken);

            if (ncr == null)
            {
                return CommandResult.Falha("Não Conformidade (NCR) não encontrada.");
            }

            ncr.Tratar(request.CausaRaiz, request.PlanoAcao, request.ResolvidoPor, usuario);

            if (!ncr.IsValid)
            {
                return CommandResult.Falha(ncr.Notifications.Select(n => n.Message));
            }

            _context.NaoConformidades.Update(ncr);
            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Não Conformidade resolvida e encerrada com sucesso!", new { NaoConformidadeId = ncr.Id, Status = ncr.Status });
        }
    }
}
