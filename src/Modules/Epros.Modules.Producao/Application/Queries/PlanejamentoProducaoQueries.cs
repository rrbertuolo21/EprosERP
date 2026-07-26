using System;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Producao.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Producao.Application.Queries
{
    public record ListarPlanejamentosProducaoQuery(string? Status, int Pagina = 1, int TamanhoPagina = 20) : IQuery<CommandResult>;
    public record ObterPlanejamentoProducaoPorIdQuery(Guid Id) : IQuery<CommandResult>;

    public class ListarPlanejamentosProducaoQueryHandler : IQueryHandler<ListarPlanejamentosProducaoQuery, CommandResult>
    {
        private readonly ContextProducao _context;
        public ListarPlanejamentosProducaoQueryHandler(ContextProducao context) => _context = context;

        public async Task<CommandResult> Handle(ListarPlanejamentosProducaoQuery request, CancellationToken ct)
        {
            var query = _context.Planejamentos.AsNoTracking().AsQueryable();
            if (!string.IsNullOrWhiteSpace(request.Status) &&
                Enum.TryParse<Domain.Enums.EStatusWorkflowProducao>(request.Status, true, out var status))
            {
                query = query.Where(e => e.Status == status);
            }

            var total = await query.CountAsync(ct);
            var itens = await query
                .OrderByDescending(e => e.CriadoEm)
                .Skip((request.Pagina - 1) * request.TamanhoPagina)
                .Take(request.TamanhoPagina)
                .ToListAsync(ct);

            return CommandResult.Ok("Planejamentos listados com sucesso.", new { total, itens });
        }
    }

    public class ObterPlanejamentoProducaoPorIdQueryHandler : IQueryHandler<ObterPlanejamentoProducaoPorIdQuery, CommandResult>
    {
        private readonly ContextProducao _context;
        public ObterPlanejamentoProducaoPorIdQueryHandler(ContextProducao context) => _context = context;

        public async Task<CommandResult> Handle(ObterPlanejamentoProducaoPorIdQuery request, CancellationToken ct)
        {
            var plano = await _context.Planejamentos.AsNoTracking()
                .Include(p => p.Snapshots)
                .FirstOrDefaultAsync(p => p.Id == request.Id, ct);

            if (plano == null)
                return CommandResult.Falha("Planejamento não encontrado.");

            return CommandResult.Ok("Planejamento obtido com sucesso.", plano);
        }
    }
}
