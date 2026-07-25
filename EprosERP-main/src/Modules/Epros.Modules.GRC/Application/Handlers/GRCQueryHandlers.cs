using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Modules.GRC.Application.Queries;
using Epros.Modules.GRC.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.GRC.Application.Handlers
{
    public class ObterRiscosQueryHandler : IQueryHandler<ObterRiscosQuery, CommandResult>
    {
        private readonly ContextGRC _context;

        public ObterRiscosQueryHandler(ContextGRC context)
        {
            _context = context;
        }

        public async Task<CommandResult> Handle(ObterRiscosQuery request, CancellationToken cancellationToken)
        {
            var riscos = await _context.RiscosCorporativos
                .OrderByDescending(r => r.NivelRisco)
                .ToListAsync(cancellationToken);

            return CommandResult.Ok("Riscos corporativos listados com sucesso!", riscos);
        }
    }

    public class ObterDenunciasQueryHandler : IQueryHandler<ObterDenunciasQuery, CommandResult>
    {
        private readonly ContextGRC _context;

        public ObterDenunciasQueryHandler(ContextGRC context)
        {
            _context = context;
        }

        public async Task<CommandResult> Handle(ObterDenunciasQuery request, CancellationToken cancellationToken)
        {
            var denuncias = await _context.Denuncias
                .OrderByDescending(d => d.DataRegistro)
                .ToListAsync(cancellationToken);

            return CommandResult.Ok("Denúncias listadas com sucesso!", denuncias);
        }
    }

    public class ObterIncidentesQueryHandler : IQueryHandler<ObterIncidentesQuery, CommandResult>
    {
        private readonly ContextGRC _context;

        public ObterIncidentesQueryHandler(ContextGRC context)
        {
            _context = context;
        }

        public async Task<CommandResult> Handle(ObterIncidentesQuery request, CancellationToken cancellationToken)
        {
            var incidentes = await _context.IncidentesCompliance
                .OrderByDescending(i => i.DataAbertura)
                .ToListAsync(cancellationToken);

            return CommandResult.Ok("Incidentes de compliance listados com sucesso!", incidentes);
        }
    }
}
