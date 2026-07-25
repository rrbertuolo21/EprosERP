using System;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Producao.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Producao.Application.Queries
{
    public record ListarCustosProducaoQuery(string? Status, int Pagina = 1, int TamanhoPagina = 20) : IQuery<CommandResult>;
    public record ObterCustoProducaoPorIdQuery(Guid Id) : IQuery<CommandResult>;

    public class ListarCustosProducaoQueryHandler : IQueryHandler<ListarCustosProducaoQuery, CommandResult>
    {
        private readonly ContextProducao _context;
        public ListarCustosProducaoQueryHandler(ContextProducao context) => _context = context;

        public async Task<CommandResult> Handle(ListarCustosProducaoQuery request, CancellationToken ct)
        {
            var query = _context.CustosProducao.AsNoTracking().AsQueryable();
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

            return CommandResult.Ok("Custos listados com sucesso.", new { total, itens });
        }
    }

    public class ObterCustoProducaoPorIdQueryHandler : IQueryHandler<ObterCustoProducaoPorIdQuery, CommandResult>
    {
        private readonly ContextProducao _context;
        public ObterCustoProducaoPorIdQueryHandler(ContextProducao context) => _context = context;

        public async Task<CommandResult> Handle(ObterCustoProducaoPorIdQuery request, CancellationToken ct)
        {
            var custo = await _context.CustosProducao.AsNoTracking()
                .Include(c => c.Referencias)
                .FirstOrDefaultAsync(c => c.Id == request.Id, ct);

            if (custo == null)
                return CommandResult.Falha("Registro de custo não encontrado.");

            return CommandResult.Ok("Custo obtido com sucesso.", custo);
        }
    }
}
