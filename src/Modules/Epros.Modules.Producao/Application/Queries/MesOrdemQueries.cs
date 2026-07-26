using System;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Producao.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Producao.Application.Queries
{
    public record ListarMesOrdensQuery(string? Status, int Pagina = 1, int TamanhoPagina = 20) : IQuery<CommandResult>;
    public record ObterMesOrdemPorIdQuery(Guid Id) : IQuery<CommandResult>;

    public class ListarMesOrdensQueryHandler : IQueryHandler<ListarMesOrdensQuery, CommandResult>
    {
        private readonly ContextProducao _context;
        public ListarMesOrdensQueryHandler(ContextProducao context) => _context = context;

        public async Task<CommandResult> Handle(ListarMesOrdensQuery request, CancellationToken ct)
        {
            var query = _context.MesOrdens.AsNoTracking().AsQueryable();
            if (!string.IsNullOrWhiteSpace(request.Status) &&
                Enum.TryParse<Domain.Enums.EStatusOrdemMes>(request.Status, true, out var status))
            {
                query = query.Where(o => o.Status == status);
            }

            var total = await query.CountAsync(ct);
            var itens = await query
                .OrderByDescending(o => o.CriadoEm)
                .Skip((request.Pagina - 1) * request.TamanhoPagina)
                .Take(request.TamanhoPagina)
                .ToListAsync(ct);

            return CommandResult.Ok("Ordens de produção listadas com sucesso.", new { total, itens });
        }
    }

    public class ObterMesOrdemPorIdQueryHandler : IQueryHandler<ObterMesOrdemPorIdQuery, CommandResult>
    {
        private readonly ContextProducao _context;
        public ObterMesOrdemPorIdQueryHandler(ContextProducao context) => _context = context;

        public async Task<CommandResult> Handle(ObterMesOrdemPorIdQuery request, CancellationToken ct)
        {
            var ordem = await _context.MesOrdens.AsNoTracking()
                .Include(o => o.Itens)
                .FirstOrDefaultAsync(o => o.Id == request.Id, ct);

            if (ordem == null)
                return CommandResult.Falha("Ordem não encontrada.");

            return CommandResult.Ok("Ordem obtida com sucesso.", ordem);
        }
    }
}
