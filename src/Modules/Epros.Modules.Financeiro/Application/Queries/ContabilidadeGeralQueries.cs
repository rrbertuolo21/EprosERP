using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Financeiro.Infrastructure.Data;
using Epros.Shared.Application.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Financeiro.Application.Queries
{
    public record ListarContasContabeisQuery(int Pagina = 1, int TamanhoPagina = 20) : IRequest<CommandResult>;
    public record ObterContaContabilPorIdQuery(Guid Id) : IRequest<CommandResult>;
    public record ListarPeriodosContabeisQuery(int? AnoFiscal) : IRequest<CommandResult>;
    public record ListarLancamentosContabeisQuery(Guid? PeriodoContabilId, int Pagina = 1, int TamanhoPagina = 20) : IRequest<CommandResult>;

    public class ContabilidadeGeralQueryHandlers :
        IRequestHandler<ListarContasContabeisQuery, CommandResult>,
        IRequestHandler<ObterContaContabilPorIdQuery, CommandResult>,
        IRequestHandler<ListarPeriodosContabeisQuery, CommandResult>,
        IRequestHandler<ListarLancamentosContabeisQuery, CommandResult>
    {
        private readonly ContextFinanceiro _context;
        public ContabilidadeGeralQueryHandlers(ContextFinanceiro context) => _context = context;

        public async Task<CommandResult> Handle(ListarContasContabeisQuery request, CancellationToken ct)
        {
            var tamanho = request.TamanhoPagina is <= 0 or > 100 ? 20 : request.TamanhoPagina;
            var pagina = request.Pagina <= 0 ? 1 : request.Pagina;
            var query = _context.ContasContabeis.AsNoTracking().OrderBy(c => c.CodigoConta);
            var total = await query.CountAsync(ct);
            var itens = await query.Skip((pagina - 1) * tamanho).Take(tamanho)
                .Select(c => new { c.Id, c.CodigoConta, c.NomeConta, c.ContaPaiId, c.Nivel, c.TipoConta, c.AceitaLancamento, c.Ativo })
                .ToListAsync(ct);
            return CommandResult.Ok("Contas contábeis listadas.", new { total, pagina, tamanho, itens });
        }

        public async Task<CommandResult> Handle(ObterContaContabilPorIdQuery request, CancellationToken ct)
        {
            var conta = await _context.ContasContabeis.AsNoTracking().FirstOrDefaultAsync(c => c.Id == request.Id, ct);
            return conta == null ? CommandResult.Falha("Conta contábil não encontrada.") : CommandResult.Ok("Conta contábil encontrada.", conta);
        }

        public async Task<CommandResult> Handle(ListarPeriodosContabeisQuery request, CancellationToken ct)
        {
            var query = _context.PeriodosContabeis.AsNoTracking().AsQueryable();
            if (request.AnoFiscal.HasValue) query = query.Where(p => p.AnoFiscal == request.AnoFiscal.Value);
            var itens = await query.OrderByDescending(p => p.AnoFiscal)
                .Select(p => new { p.Id, p.AnoFiscal, p.DataInicio, p.DataFim, p.DataFechamento, p.Estado }).ToListAsync(ct);
            return CommandResult.Ok("Períodos contábeis listados.", itens);
        }

        public async Task<CommandResult> Handle(ListarLancamentosContabeisQuery request, CancellationToken ct)
        {
            var tamanho = request.TamanhoPagina is <= 0 or > 100 ? 20 : request.TamanhoPagina;
            var pagina = request.Pagina <= 0 ? 1 : request.Pagina;
            var query = _context.LancamentosContabeis.AsNoTracking().AsQueryable();
            if (request.PeriodoContabilId.HasValue) query = query.Where(l => l.PeriodoContabilId == request.PeriodoContabilId.Value);
            var total = await query.CountAsync(ct);
            var itens = await query.OrderByDescending(l => l.Data).Skip((pagina - 1) * tamanho).Take(tamanho)
                .Select(l => new { l.Id, l.NumeroLancamento, l.Data, l.Estado, l.Historico, l.PeriodoContabilId })
                .ToListAsync(ct);
            return CommandResult.Ok("Lançamentos contábeis listados.", new { total, pagina, tamanho, itens });
        }
    }
}
