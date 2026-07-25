using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Modules.DMS.Application.Queries;
using Epros.Modules.DMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.DMS.Application.Handlers
{
    public class ObterProspectsShowroomQueryHandler : IQueryHandler<ObterProspectsShowroomQuery, CommandResult>
    {
        private readonly ContextDMS _context;

        public ObterProspectsShowroomQueryHandler(ContextDMS context)
        {
            _context = context;
        }

        public async Task<CommandResult> Handle(ObterProspectsShowroomQuery request, CancellationToken cancellationToken)
        {
            var itens = await _context.ProspectsShowroom
                .OrderByDescending(x => x.CriadoEm)
                .ToListAsync(cancellationToken);

            return CommandResult.Ok("Listagem realizada com sucesso!", itens);
        }
    }

    public class ObterOportunidadesConcessionariaQueryHandler : IQueryHandler<ObterOportunidadesConcessionariaQuery, CommandResult>
    {
        private readonly ContextDMS _context;

        public ObterOportunidadesConcessionariaQueryHandler(ContextDMS context)
        {
            _context = context;
        }

        public async Task<CommandResult> Handle(ObterOportunidadesConcessionariaQuery request, CancellationToken cancellationToken)
        {
            var itens = await _context.OportunidadesConcessionaria
                .OrderByDescending(x => x.CriadoEm)
                .ToListAsync(cancellationToken);

            return CommandResult.Ok("Listagem realizada com sucesso!", itens);
        }
    }

    public class ObterTestDrivesQueryHandler : IQueryHandler<ObterTestDrivesQuery, CommandResult>
    {
        private readonly ContextDMS _context;

        public ObterTestDrivesQueryHandler(ContextDMS context)
        {
            _context = context;
        }

        public async Task<CommandResult> Handle(ObterTestDrivesQuery request, CancellationToken cancellationToken)
        {
            var itens = await _context.TestDrives
                .OrderByDescending(x => x.CriadoEm)
                .ToListAsync(cancellationToken);

            return CommandResult.Ok("Listagem realizada com sucesso!", itens);
        }
    }
}
