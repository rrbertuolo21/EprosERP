using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Financeiro.Infrastructure.Data;
using Epros.Shared.Application.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Financeiro.Application.Queries
{
    public record ListarVersoesOrcamentariasQuery(int Pagina = 1, int TamanhoPagina = 20) : IRequest<CommandResult>;
    public record ObterVersaoOrcamentariaPorIdQuery(Guid Id) : IRequest<CommandResult>;
    public record PrevistoRealizadoQuery(Guid VersaoId) : IRequest<CommandResult>;
    public record ListarPeriodosOrcamentariosQuery() : IRequest<CommandResult>;
    public record ListarBudgetsPeriodoQuery(Guid PeriodoId) : IRequest<CommandResult>;
    public record ListarMetasQuery(Guid? CategoriaId) : IRequest<CommandResult>;
    public record ObterMetaPorIdQuery(Guid Id) : IRequest<CommandResult>;

    public class PlanejamentoOrcamentoQueryHandlers :
        IRequestHandler<ListarVersoesOrcamentariasQuery, CommandResult>,
        IRequestHandler<ObterVersaoOrcamentariaPorIdQuery, CommandResult>,
        IRequestHandler<PrevistoRealizadoQuery, CommandResult>,
        IRequestHandler<ListarPeriodosOrcamentariosQuery, CommandResult>,
        IRequestHandler<ListarBudgetsPeriodoQuery, CommandResult>,
        IRequestHandler<ListarMetasQuery, CommandResult>,
        IRequestHandler<ObterMetaPorIdQuery, CommandResult>
    {
        private readonly ContextFinanceiro _context;
        public PlanejamentoOrcamentoQueryHandlers(ContextFinanceiro context) => _context = context;

        public async Task<CommandResult> Handle(ListarVersoesOrcamentariasQuery request, CancellationToken ct)
        {
            var tamanho = request.TamanhoPagina is <= 0 or > 100 ? 20 : request.TamanhoPagina;
            var pagina = request.Pagina <= 0 ? 1 : request.Pagina;
            var query = _context.VersoesOrcamentarias.AsNoTracking();
            var total = await query.CountAsync(ct);
            var itens = await query.OrderByDescending(v => v.PeriodoInicio).Skip((pagina - 1) * tamanho).Take(tamanho)
                .Select(v => new { v.Id, v.Nome, v.PeriodoInicio, v.PeriodoFim, v.Status }).ToListAsync(ct);
            return CommandResult.Ok("Versões orçamentárias listadas.", new { total, pagina, tamanho, itens });
        }

        public async Task<CommandResult> Handle(ObterVersaoOrcamentariaPorIdQuery request, CancellationToken ct)
        {
            var v = await _context.VersoesOrcamentarias.AsNoTracking().Include(x => x.Linhas).FirstOrDefaultAsync(x => x.Id == request.Id, ct);
            return v == null ? CommandResult.Falha("Versão orçamentária não encontrada.") : CommandResult.Ok("Versão encontrada.", v);
        }

        public async Task<CommandResult> Handle(PrevistoRealizadoQuery request, CancellationToken ct)
        {
            var linhas = await _context.LinhasOrcamento.AsNoTracking().Where(l => l.VersaoId == request.VersaoId)
                .Select(l => new { l.Id, l.ContaId, l.CentroCustoId, l.Periodo, l.ValorOrcado, l.ValorRealizado, l.VariacaoValor, l.VariacaoPercentual })
                .ToListAsync(ct);
            var totalOrcado = linhas.Sum(l => l.ValorOrcado);
            var totalRealizado = linhas.Sum(l => l.ValorRealizado ?? 0);
            return CommandResult.Ok("Previsto x realizado apurado.", new { versaoId = request.VersaoId, totalOrcado, totalRealizado, variacao = totalRealizado - totalOrcado, linhas });
        }

        public async Task<CommandResult> Handle(ListarPeriodosOrcamentariosQuery request, CancellationToken ct)
        {
            var itens = await _context.PeriodosOrcamentarios.AsNoTracking().OrderByDescending(p => p.DataInicio)
                .Select(p => new { p.Id, p.DataInicio, p.DataFim, p.Status }).ToListAsync(ct);
            return CommandResult.Ok("Períodos orçamentários listados.", itens);
        }

        public async Task<CommandResult> Handle(ListarBudgetsPeriodoQuery request, CancellationToken ct)
        {
            var itens = await _context.Budgets.AsNoTracking().Where(b => b.PeriodoId == request.PeriodoId)
                .Select(b => new { b.Id, b.Tipo, b.Status, b.Valor }).ToListAsync(ct);
            return CommandResult.Ok("Budgets do período listados.", itens);
        }

        public async Task<CommandResult> Handle(ListarMetasQuery request, CancellationToken ct)
        {
            var query = _context.Metas.AsNoTracking().AsQueryable();
            if (request.CategoriaId.HasValue) query = query.Where(m => m.CategoriaId == request.CategoriaId.Value);
            var itens = await query.OrderByDescending(m => m.DataInicio)
                .Select(m => new { m.Id, m.CategoriaId, m.Tipo, m.Prioridade, m.DataInicio, m.DataAlvo, m.Status }).ToListAsync(ct);
            return CommandResult.Ok("Metas listadas.", itens);
        }

        public async Task<CommandResult> Handle(ObterMetaPorIdQuery request, CancellationToken ct)
        {
            var meta = await _context.Metas.AsNoTracking()
                .Include(m => m.Milestones).Include(m => m.Contribuicoes).Include(m => m.Trackings)
                .FirstOrDefaultAsync(m => m.Id == request.Id, ct);
            return meta == null ? CommandResult.Falha("Meta não encontrada.") : CommandResult.Ok("Meta encontrada.", meta);
        }
    }
}
