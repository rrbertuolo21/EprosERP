using System;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Producao.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Producao.Application.Queries
{
    public record ListarEstimativasQuery(string? Status, int Pagina = 1, int TamanhoPagina = 20) : IQuery<CommandResult>;
    public record ObterEstimativaPorIdQuery(Guid Id) : IQuery<CommandResult>;

    public class ListarEstimativasQueryHandler : IQueryHandler<ListarEstimativasQuery, CommandResult>
    {
        private readonly ContextProducao _context;
        public ListarEstimativasQueryHandler(ContextProducao context) => _context = context;

        public async Task<CommandResult> Handle(ListarEstimativasQuery request, CancellationToken ct)
        {
            var query = _context.Estimativas.AsNoTracking().AsQueryable();
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

            return CommandResult.Ok("Estimativas listadas com sucesso.", new { total, itens });
        }
    }

    public class ObterEstimativaPorIdQueryHandler : IQueryHandler<ObterEstimativaPorIdQuery, CommandResult>
    {
        private readonly ContextProducao _context;
        public ObterEstimativaPorIdQueryHandler(ContextProducao context) => _context = context;

        public async Task<CommandResult> Handle(ObterEstimativaPorIdQuery request, CancellationToken ct)
        {
            var estimativa = await _context.Estimativas.AsNoTracking()
                .Include(e => e.Componentes)
                .FirstOrDefaultAsync(e => e.Id == request.Id, ct);

            if (estimativa == null)
                return CommandResult.Falha("Estimativa não encontrada.");

            return CommandResult.Ok("Estimativa obtida com sucesso.", estimativa);
        }
    }
}
