using System;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Producao.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Producao.Application.Queries
{
    public record ListarBomEstruturasQuery(string? Status, int Pagina = 1, int TamanhoPagina = 20) : IQuery<CommandResult>;
    public record ObterBomEstruturaPorIdQuery(Guid Id) : IQuery<CommandResult>;

    public class ListarBomEstruturasQueryHandler : IQueryHandler<ListarBomEstruturasQuery, CommandResult>
    {
        private readonly ContextProducao _context;
        public ListarBomEstruturasQueryHandler(ContextProducao context) => _context = context;

        public async Task<CommandResult> Handle(ListarBomEstruturasQuery request, CancellationToken ct)
        {
            var query = _context.BomEstruturas.AsNoTracking().AsQueryable();
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

            return CommandResult.Ok("Estruturas listadas com sucesso.", new { total, itens });
        }
    }

    public class ObterBomEstruturaPorIdQueryHandler : IQueryHandler<ObterBomEstruturaPorIdQuery, CommandResult>
    {
        private readonly ContextProducao _context;
        public ObterBomEstruturaPorIdQueryHandler(ContextProducao context) => _context = context;

        public async Task<CommandResult> Handle(ObterBomEstruturaPorIdQuery request, CancellationToken ct)
        {
            var estrutura = await _context.BomEstruturas.AsNoTracking()
                .Include(e => e.Componentes)
                .FirstOrDefaultAsync(e => e.Id == request.Id, ct);

            if (estrutura == null)
                return CommandResult.Falha("Estrutura não encontrada.");

            return CommandResult.Ok("Estrutura obtida com sucesso.", estrutura);
        }
    }
}
