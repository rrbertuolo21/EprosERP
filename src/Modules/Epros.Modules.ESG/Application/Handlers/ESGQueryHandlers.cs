using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Modules.ESG.Application.Queries;
using Epros.Modules.ESG.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.ESG.Application.Handlers
{
    public class ObterEmissoesQueryHandler : IQueryHandler<ObterEmissoesQuery, CommandResult>
    {
        private readonly ContextESG _context;

        public ObterEmissoesQueryHandler(ContextESG context)
        {
            _context = context;
        }

        public async Task<CommandResult> Handle(ObterEmissoesQuery request, CancellationToken cancellationToken)
        {
            var emissoes = await _context.EmissoesCarbono
                .OrderByDescending(e => e.DataTransacao)
                .ToListAsync(cancellationToken);

            return CommandResult.Ok("Emissões de carbono listadas com sucesso!", emissoes);
        }
    }

    public class ObterRelatoriosESGQueryHandler : IQueryHandler<ObterRelatoriosESGQuery, CommandResult>
    {
        private readonly ContextESG _context;

        public ObterRelatoriosESGQueryHandler(ContextESG context)
        {
            _context = context;
        }

        public async Task<CommandResult> Handle(ObterRelatoriosESGQuery request, CancellationToken cancellationToken)
        {
            var relatorios = await _context.RelatoriosESG
                .OrderByDescending(r => r.AnoFiscal)
                .ToListAsync(cancellationToken);

            return CommandResult.Ok("Relatórios ESG listados com sucesso!", relatorios);
        }
    }
}
