using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Modules.DMS.Application.Commands;
using Epros.Modules.DMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.DMS.Application.Handlers
{
    public class JulgarGarantiaMontadoraCommandHandler : ICommandHandler<JulgarGarantiaMontadoraCommand>
    {
        private readonly ContextDMS _context;
        private readonly ICurrentUser _currentUser;

        public JulgarGarantiaMontadoraCommandHandler(
            ContextDMS context,
            ICurrentUser currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(JulgarGarantiaMontadoraCommand request, CancellationToken cancellationToken)
        {
            var usuario = _currentUser.GetUserId() ?? "system";

            var garantia = await _context.GarantiasMontadora.FindAsync(request.Id);
            if (garantia == null)
            {
                return CommandResult.Falha("Solicitação de reembolso de garantia não encontrada.");
            }

            garantia.Julgar(request.NovoStatus, request.Parecer, usuario);

            if (!garantia.IsValid)
            {
                return CommandResult.Falha(garantia.Notifications.Select(n => n.Message));
            }

            var os = await _context.OrdensServicoDms.FindAsync(garantia.OrdemServicoDmsId);
            if (os != null)
            {
                os.AtualizarStatusGarantia(request.NovoStatus, usuario);
                _context.OrdensServicoDms.Update(os);
            }

            _context.GarantiasMontadora.Update(garantia);
            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Julgamento de garantia da montadora registrado com sucesso!", new { GarantiaId = garantia.Id, Status = garantia.Status });
        }
    }
}
