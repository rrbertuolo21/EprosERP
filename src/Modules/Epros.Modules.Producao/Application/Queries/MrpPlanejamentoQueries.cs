using System;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Producao.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Producao.Application.Queries
{
    public record ListarMrpPlanejamentosQuery(string? Status, int Pagina = 1, int TamanhoPagina = 20) : IQuery<CommandResult>;
    public record ObterMrpPlanejamentoPorIdQuery(Guid Id) : IQuery<CommandResult>;

    public class ListarMrpPlanejamentosQueryHandler : IQueryHandler<ListarMrpPlanejamentosQuery, CommandResult>
    {
        private readonly ContextProducao _context;
        public ListarMrpPlanejamentosQueryHandler(ContextProducao context) => _context = context;

        public async Task<CommandResult> Handle(ListarMrpPlanejamentosQuery request, CancellationToken ct)
        {
            var query = _context.MrpPlanejamentos.AsNoTracking().AsQueryable();
            if (!string.IsNullOrWhiteSpace(request.Status) &&
                Enum.TryParse<Domain.Enums.EStatusWorkflowProducao>(request.Status, true, out var status))
            {
                query = query.Where(p => p.Status == status);
            }

            var total = await query.CountAsync(ct);
            var itens = await query
                .OrderByDescending(p => p.CriadoEm)
                .Skip((request.Pagina - 1) * request.TamanhoPagina)
                .Take(request.TamanhoPagina)
                .ToListAsync(ct);

            return CommandResult.Ok("Planejamentos MRP/IBP listados com sucesso.", new { total, itens });
        }
    }

    public class ObterMrpPlanejamentoPorIdQueryHandler : IQueryHandler<ObterMrpPlanejamentoPorIdQuery, CommandResult>
    {
        private readonly ContextProducao _context;
        public ObterMrpPlanejamentoPorIdQueryHandler(ContextProducao context) => _context = context;

        public async Task<CommandResult> Handle(ObterMrpPlanejamentoPorIdQuery request, CancellationToken ct)
        {
            var planejamento = await _context.MrpPlanejamentos.AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == request.Id, ct);

            if (planejamento == null)
                return CommandResult.Falha("Planejamento não encontrado.");

            return CommandResult.Ok("Planejamento obtido com sucesso.", planejamento);
        }
    }
}
