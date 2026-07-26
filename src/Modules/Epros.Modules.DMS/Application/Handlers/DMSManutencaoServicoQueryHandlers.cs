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
    public class ObterTiposServicoConcessionariaQueryHandler : IQueryHandler<ObterTiposServicoConcessionariaQuery, CommandResult>
    {
        private readonly ContextDMS _context;

        public ObterTiposServicoConcessionariaQueryHandler(ContextDMS context)
        {
            _context = context;
        }

        public async Task<CommandResult> Handle(ObterTiposServicoConcessionariaQuery request, CancellationToken cancellationToken)
        {
            var tipos = await _context.TiposServicoConcessionaria
                .OrderByDescending(x => x.CriadoEm)
                .ToListAsync(cancellationToken);

            return CommandResult.Ok("Tipos de serviço da concessionária listados com sucesso!", tipos);
        }
    }

    public class ObterOperacoesServicoQueryHandler : IQueryHandler<ObterOperacoesServicoQuery, CommandResult>
    {
        private readonly ContextDMS _context;

        public ObterOperacoesServicoQueryHandler(ContextDMS context)
        {
            _context = context;
        }

        public async Task<CommandResult> Handle(ObterOperacoesServicoQuery request, CancellationToken cancellationToken)
        {
            var operacoes = await _context.OperacoesServico
                .OrderByDescending(x => x.CriadoEm)
                .ToListAsync(cancellationToken);

            return CommandResult.Ok("Operações de serviço listadas com sucesso!", operacoes);
        }
    }

    public class ObterPacotesServicoQueryHandler : IQueryHandler<ObterPacotesServicoQuery, CommandResult>
    {
        private readonly ContextDMS _context;

        public ObterPacotesServicoQueryHandler(ContextDMS context)
        {
            _context = context;
        }

        public async Task<CommandResult> Handle(ObterPacotesServicoQuery request, CancellationToken cancellationToken)
        {
            var pacotes = await _context.PacotesServico
                .OrderByDescending(x => x.CriadoEm)
                .ToListAsync(cancellationToken);

            return CommandResult.Ok("Pacotes de serviço listados com sucesso!", pacotes);
        }
    }

    public class ObterOrdensServicoManutencaoQueryHandler : IQueryHandler<ObterOrdensServicoManutencaoQuery, CommandResult>
    {
        private readonly ContextDMS _context;

        public ObterOrdensServicoManutencaoQueryHandler(ContextDMS context)
        {
            _context = context;
        }

        public async Task<CommandResult> Handle(ObterOrdensServicoManutencaoQuery request, CancellationToken cancellationToken)
        {
            var ordens = await _context.OrdensServicoManutencao
                .OrderByDescending(x => x.CriadoEm)
                .ToListAsync(cancellationToken);

            return CommandResult.Ok("Ordens de serviço de manutenção listadas com sucesso!", ordens);
        }
    }

    public class ObterOrcamentosManutencaoQueryHandler : IQueryHandler<ObterOrcamentosManutencaoQuery, CommandResult>
    {
        private readonly ContextDMS _context;

        public ObterOrcamentosManutencaoQueryHandler(ContextDMS context)
        {
            _context = context;
        }

        public async Task<CommandResult> Handle(ObterOrcamentosManutencaoQuery request, CancellationToken cancellationToken)
        {
            var orcamentos = await _context.OrcamentosManutencao
                .OrderByDescending(x => x.CriadoEm)
                .ToListAsync(cancellationToken);

            return CommandResult.Ok("Orçamentos de manutenção listados com sucesso!", orcamentos);
        }
    }
}
