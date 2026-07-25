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
    public class ObterPlanosGarantiaQueryHandler : IQueryHandler<ObterPlanosGarantiaQuery, CommandResult>
    {
        private readonly ContextDMS _context;

        public ObterPlanosGarantiaQueryHandler(ContextDMS context)
        {
            _context = context;
        }

        public async Task<CommandResult> Handle(ObterPlanosGarantiaQuery request, CancellationToken cancellationToken)
        {
            var planos = await _context.PlanosGarantia
                .OrderByDescending(p => p.CriadoEm)
                .ToListAsync(cancellationToken);

            return CommandResult.Ok("Planos de garantia listados com sucesso!", planos);
        }
    }

    public class ObterVeiculosGarantiaQueryHandler : IQueryHandler<ObterVeiculosGarantiaQuery, CommandResult>
    {
        private readonly ContextDMS _context;

        public ObterVeiculosGarantiaQueryHandler(ContextDMS context)
        {
            _context = context;
        }

        public async Task<CommandResult> Handle(ObterVeiculosGarantiaQuery request, CancellationToken cancellationToken)
        {
            var veiculos = await _context.VeiculosGarantia
                .OrderByDescending(v => v.CriadoEm)
                .ToListAsync(cancellationToken);

            return CommandResult.Ok("Garantias de veículos listadas com sucesso!", veiculos);
        }
    }

    public class ObterSolicitacoesGarantiaQueryHandler : IQueryHandler<ObterSolicitacoesGarantiaQuery, CommandResult>
    {
        private readonly ContextDMS _context;

        public ObterSolicitacoesGarantiaQueryHandler(ContextDMS context)
        {
            _context = context;
        }

        public async Task<CommandResult> Handle(ObterSolicitacoesGarantiaQuery request, CancellationToken cancellationToken)
        {
            var solicitacoes = await _context.SolicitacoesGarantia
                .OrderByDescending(s => s.CriadoEm)
                .ToListAsync(cancellationToken);

            return CommandResult.Ok("Solicitações de garantia listadas com sucesso!", solicitacoes);
        }
    }
}
