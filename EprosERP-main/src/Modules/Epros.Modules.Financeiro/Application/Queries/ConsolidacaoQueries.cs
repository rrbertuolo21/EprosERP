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
    public record ListarGruposConsolidacaoQuery(int Pagina = 1, int TamanhoPagina = 20) : IRequest<CommandResult>;
    public record ObterGrupoConsolidacaoPorIdQuery(Guid Id) : IRequest<CommandResult>;
    public record ListarDemonstrativosQuery(Guid GrupoConsolidacaoId) : IRequest<CommandResult>;
    public record ObterDemonstrativoPorIdQuery(Guid Id) : IRequest<CommandResult>;
    public record ListarBalancetesQuery(Guid GrupoConsolidacaoId) : IRequest<CommandResult>;
    public record ObterBalancetePorIdQuery(Guid Id) : IRequest<CommandResult>;
    public record ListarEliminacoesQuery(Guid GrupoConsolidacaoId, string? Periodo) : IRequest<CommandResult>;

    public class ConsolidacaoQueryHandlers :
        IRequestHandler<ListarGruposConsolidacaoQuery, CommandResult>,
        IRequestHandler<ObterGrupoConsolidacaoPorIdQuery, CommandResult>,
        IRequestHandler<ListarDemonstrativosQuery, CommandResult>,
        IRequestHandler<ObterDemonstrativoPorIdQuery, CommandResult>,
        IRequestHandler<ListarBalancetesQuery, CommandResult>,
        IRequestHandler<ObterBalancetePorIdQuery, CommandResult>,
        IRequestHandler<ListarEliminacoesQuery, CommandResult>
    {
        private readonly ContextFinanceiro _context;
        public ConsolidacaoQueryHandlers(ContextFinanceiro context) => _context = context;

        public async Task<CommandResult> Handle(ListarGruposConsolidacaoQuery request, CancellationToken ct)
        {
            var tamanho = request.TamanhoPagina is <= 0 or > 100 ? 20 : request.TamanhoPagina;
            var pagina = request.Pagina <= 0 ? 1 : request.Pagina;
            var query = _context.GruposConsolidacao.AsNoTracking();
            var total = await query.CountAsync(ct);
            var itens = await query.OrderBy(g => g.Nome).Skip((pagina - 1) * tamanho).Take(tamanho)
                .Select(g => new { g.Id, g.Codigo, g.Nome, g.Descricao, g.Situacao }).ToListAsync(ct);
            return CommandResult.Ok("Grupos de consolidação listados.", new { total, pagina, tamanho, itens });
        }

        public async Task<CommandResult> Handle(ObterGrupoConsolidacaoPorIdQuery request, CancellationToken ct)
        {
            var grupo = await _context.GruposConsolidacao.AsNoTracking().Include(g => g.Empresas).FirstOrDefaultAsync(g => g.Id == request.Id, ct);
            return grupo == null ? CommandResult.Falha("Grupo de consolidação não encontrado.") : CommandResult.Ok("Grupo encontrado.", grupo);
        }

        public async Task<CommandResult> Handle(ListarDemonstrativosQuery request, CancellationToken ct)
        {
            var itens = await _context.Demonstrativos.AsNoTracking().Where(d => d.GrupoConsolidacaoId == request.GrupoConsolidacaoId)
                .OrderByDescending(d => d.Periodo)
                .Select(d => new { d.Id, d.Periodo, d.Estado, d.DataPublicacao, d.TotalAgregado }).ToListAsync(ct);
            return CommandResult.Ok("Demonstrativos listados.", itens);
        }

        public async Task<CommandResult> Handle(ObterDemonstrativoPorIdQuery request, CancellationToken ct)
        {
            var demo = await _context.Demonstrativos.AsNoTracking().Include(d => d.Linhas).FirstOrDefaultAsync(d => d.Id == request.Id, ct);
            return demo == null ? CommandResult.Falha("Demonstrativo não encontrado.") : CommandResult.Ok("Demonstrativo encontrado.", demo);
        }

        public async Task<CommandResult> Handle(ListarBalancetesQuery request, CancellationToken ct)
        {
            var itens = await _context.BalancetesConsolidados.AsNoTracking().Where(b => b.GrupoConsolidacaoId == request.GrupoConsolidacaoId)
                .OrderByDescending(b => b.Periodo)
                .Select(b => new { b.Id, b.Periodo, b.Estado, b.TotalDebito, b.TotalCredito, b.SaldoFinal }).ToListAsync(ct);
            return CommandResult.Ok("Balancetes consolidados listados.", itens);
        }

        public async Task<CommandResult> Handle(ObterBalancetePorIdQuery request, CancellationToken ct)
        {
            var bal = await _context.BalancetesConsolidados.AsNoTracking().Include(b => b.Linhas).FirstOrDefaultAsync(b => b.Id == request.Id, ct);
            return bal == null ? CommandResult.Falha("Balancete consolidado não encontrado.") : CommandResult.Ok("Balancete encontrado.", bal);
        }

        public async Task<CommandResult> Handle(ListarEliminacoesQuery request, CancellationToken ct)
        {
            var query = _context.EliminacoesIntercompany.AsNoTracking().Where(e => e.GrupoConsolidacaoId == request.GrupoConsolidacaoId);
            if (!string.IsNullOrWhiteSpace(request.Periodo)) query = query.Where(e => e.Periodo == request.Periodo);
            var itens = await query.Select(e => new { e.Id, e.Periodo, e.EmpresaOrigemId, e.EmpresaDestinoId, e.Valor, e.Descricao }).ToListAsync(ct);
            return CommandResult.Ok("Eliminações intercompany listadas.", itens);
        }
    }
}
