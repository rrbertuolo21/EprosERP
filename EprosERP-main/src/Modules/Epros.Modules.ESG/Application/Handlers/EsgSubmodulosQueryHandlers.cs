using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.ESG.Application.Queries;
using Epros.Modules.ESG.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.ESG.Application.Handlers
{
    public class ListarInventariosGeeQueryHandler : IQueryHandler<ListarInventariosGeeQuery, CommandResult>
    {
        private readonly ContextESG _context;
        public ListarInventariosGeeQueryHandler(ContextESG context) => _context = context;
        public async Task<CommandResult> Handle(ListarInventariosGeeQuery request, CancellationToken cancellationToken)
        {
            var dados = await _context.InventariosGee.OrderByDescending(i => i.PeriodoInicio).ToListAsync(cancellationToken);
            return CommandResult.Ok("Inventarios GEE listados.", dados);
        }
    }

    public class ListarFatoresEmissaoGeeQueryHandler : IQueryHandler<ListarFatoresEmissaoGeeQuery, CommandResult>
    {
        private readonly ContextESG _context;
        public ListarFatoresEmissaoGeeQueryHandler(ContextESG context) => _context = context;
        public async Task<CommandResult> Handle(ListarFatoresEmissaoGeeQuery request, CancellationToken cancellationToken)
        {
            var dados = await _context.FatoresEmissaoGee.OrderBy(f => f.Codigo).ToListAsync(cancellationToken);
            return CommandResult.Ok("Fatores de emissao listados.", dados);
        }
    }

    public class ListarRegistrosEhsQueryHandler : IQueryHandler<ListarRegistrosEhsQuery, CommandResult>
    {
        private readonly ContextESG _context;
        public ListarRegistrosEhsQueryHandler(ContextESG context) => _context = context;
        public async Task<CommandResult> Handle(ListarRegistrosEhsQuery request, CancellationToken cancellationToken)
        {
            var dados = await _context.RegistrosEhs.OrderByDescending(r => r.CriadoEm).ToListAsync(cancellationToken);
            return CommandResult.Ok("Registros EHS listados.", dados);
        }
    }

    public class ListarIncidentesQueryHandler : IQueryHandler<ListarIncidentesQuery, CommandResult>
    {
        private readonly ContextESG _context;
        public ListarIncidentesQueryHandler(ContextESG context) => _context = context;
        public async Task<CommandResult> Handle(ListarIncidentesQuery request, CancellationToken cancellationToken)
        {
            var dados = await _context.Incidentes.OrderByDescending(i => i.DataHora).ToListAsync(cancellationToken);
            return CommandResult.Ok("Incidentes listados.", dados);
        }
    }

    public class ListarLicencasAmbientaisQueryHandler : IQueryHandler<ListarLicencasAmbientaisQuery, CommandResult>
    {
        private readonly ContextESG _context;
        public ListarLicencasAmbientaisQueryHandler(ContextESG context) => _context = context;
        public async Task<CommandResult> Handle(ListarLicencasAmbientaisQuery request, CancellationToken cancellationToken)
        {
            var dados = await _context.LicencasAmbientais.OrderByDescending(l => l.DataValidade).ToListAsync(cancellationToken);
            return CommandResult.Ok("Licencas ambientais listadas.", dados);
        }
    }

    public class ListarFrameworksRelQueryHandler : IQueryHandler<ListarFrameworksRelQuery, CommandResult>
    {
        private readonly ContextESG _context;
        public ListarFrameworksRelQueryHandler(ContextESG context) => _context = context;
        public async Task<CommandResult> Handle(ListarFrameworksRelQuery request, CancellationToken cancellationToken)
        {
            var dados = await _context.FrameworksRel.OrderBy(f => f.Codigo).ToListAsync(cancellationToken);
            return CommandResult.Ok("Frameworks listados.", dados);
        }
    }

    public class ListarItensRelatorioQueryHandler : IQueryHandler<ListarItensRelatorioQuery, CommandResult>
    {
        private readonly ContextESG _context;
        public ListarItensRelatorioQueryHandler(ContextESG context) => _context = context;
        public async Task<CommandResult> Handle(ListarItensRelatorioQuery request, CancellationToken cancellationToken)
        {
            var dados = await _context.ItensRelatorioEsg.Where(i => i.RelatorioId == request.RelatorioId).OrderBy(i => i.Sequencia).ToListAsync(cancellationToken);
            return CommandResult.Ok("Itens do relatorio listados.", dados);
        }
    }

    public class ListarDevolucoesEcoQueryHandler : IQueryHandler<ListarDevolucoesEcoQuery, CommandResult>
    {
        private readonly ContextESG _context;
        public ListarDevolucoesEcoQueryHandler(ContextESG context) => _context = context;
        public async Task<CommandResult> Handle(ListarDevolucoesEcoQuery request, CancellationToken cancellationToken)
        {
            var dados = await _context.DevolucoesEco.OrderByDescending(d => d.CriadoEm).ToListAsync(cancellationToken);
            return CommandResult.Ok("Devolucoes listadas.", dados);
        }
    }

    public class ListarFluxosCircularesQueryHandler : IQueryHandler<ListarFluxosCircularesQuery, CommandResult>
    {
        private readonly ContextESG _context;
        public ListarFluxosCircularesQueryHandler(ContextESG context) => _context = context;
        public async Task<CommandResult> Handle(ListarFluxosCircularesQuery request, CancellationToken cancellationToken)
        {
            var dados = await _context.FluxosCirculares.OrderBy(f => f.Codigo).ToListAsync(cancellationToken);
            return CommandResult.Ok("Fluxos circulares listados.", dados);
        }
    }
}
