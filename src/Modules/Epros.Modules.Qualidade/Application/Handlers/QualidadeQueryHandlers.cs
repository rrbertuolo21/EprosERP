using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Modules.Qualidade.Application.Queries;
using Epros.Modules.Qualidade.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Qualidade.Application.Handlers
{
    public class ObterInspecoesPendentesQueryHandler : IQueryHandler<ObterInspecoesPendentesQuery, CommandResult>
    {
        private readonly ContextQualidade _context;

        public ObterInspecoesPendentesQueryHandler(ContextQualidade context)
        {
            _context = context;
        }

        public async Task<CommandResult> Handle(ObterInspecoesPendentesQuery request, CancellationToken cancellationToken)
        {
            var inspecoes = await _context.InspecoesLote
                .Where(i => i.Status == "Pendente")
                .OrderByDescending(i => i.CriadoEm)
                .ToListAsync(cancellationToken);

            return CommandResult.Ok("Inspeções pendentes listadas com sucesso!", inspecoes);
        }
    }

    public class ObterNaoConformidadesAtivasQueryHandler : IQueryHandler<ObterNaoConformidadesAtivasQuery, CommandResult>
    {
        private readonly ContextQualidade _context;

        public ObterNaoConformidadesAtivasQueryHandler(ContextQualidade context)
        {
            _context = context;
        }

        public async Task<CommandResult> Handle(ObterNaoConformidadesAtivasQuery request, CancellationToken cancellationToken)
        {
            var naoConformidades = await _context.NaoConformidades
                .Where(n => n.Status == "Aberta")
                .OrderByDescending(n => n.CriadoEm)
                .ToListAsync(cancellationToken);

            return CommandResult.Ok("Não Conformidades ativas listadas com sucesso!", naoConformidades);
        }
    }
}
