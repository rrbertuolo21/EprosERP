using System;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Producao.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Producao.Application.Queries
{
    public record ListarEscProgramacoesQuery(string? Status, int Pagina = 1, int TamanhoPagina = 20) : IQuery<CommandResult>;
    public record ObterEscProgramacaoPorIdQuery(Guid Id) : IQuery<CommandResult>;

    public class ListarEscProgramacoesQueryHandler : IQueryHandler<ListarEscProgramacoesQuery, CommandResult>
    {
        private readonly ContextProducao _context;
        public ListarEscProgramacoesQueryHandler(ContextProducao context) => _context = context;

        public async Task<CommandResult> Handle(ListarEscProgramacoesQuery request, CancellationToken ct)
        {
            var query = _context.EscProgramacoes.AsNoTracking().AsQueryable();
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

            return CommandResult.Ok("Programações listadas com sucesso.", new { total, itens });
        }
    }

    public class ObterEscProgramacaoPorIdQueryHandler : IQueryHandler<ObterEscProgramacaoPorIdQuery, CommandResult>
    {
        private readonly ContextProducao _context;
        public ObterEscProgramacaoPorIdQueryHandler(ContextProducao context) => _context = context;

        public async Task<CommandResult> Handle(ObterEscProgramacaoPorIdQuery request, CancellationToken ct)
        {
            var programacao = await _context.EscProgramacoes.AsNoTracking()
                .Include(p => p.Operacoes)
                .FirstOrDefaultAsync(p => p.Id == request.Id, ct);

            if (programacao == null)
                return CommandResult.Falha("Programação não encontrada.");

            return CommandResult.Ok("Programação obtida com sucesso.", programacao);
        }
    }
}
