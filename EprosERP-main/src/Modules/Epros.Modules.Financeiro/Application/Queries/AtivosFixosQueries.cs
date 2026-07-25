using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Financeiro.Domain.Enums;
using Epros.Modules.Financeiro.Infrastructure.Data;
using Epros.Shared.Application.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Financeiro.Application.Queries
{
    public record ListarAtivosFixosQuery(EStatusAtivoFixo? Status, Guid? GrupoBemId, int Pagina = 1, int TamanhoPagina = 20) : IRequest<CommandResult>;
    public record ObterAtivoFixoPorIdQuery(Guid Id) : IRequest<CommandResult>;
    public record ListarGruposBemQuery(int Pagina = 1, int TamanhoPagina = 20) : IRequest<CommandResult>;
    public record ListarDepreciacoesAtivoQuery(Guid AtivoId) : IRequest<CommandResult>;
    public record ListarMovimentacoesAtivoQuery(Guid AtivoId) : IRequest<CommandResult>;

    public class AtivosFixosQueryHandlers :
        IRequestHandler<ListarAtivosFixosQuery, CommandResult>,
        IRequestHandler<ObterAtivoFixoPorIdQuery, CommandResult>,
        IRequestHandler<ListarGruposBemQuery, CommandResult>,
        IRequestHandler<ListarDepreciacoesAtivoQuery, CommandResult>,
        IRequestHandler<ListarMovimentacoesAtivoQuery, CommandResult>
    {
        private readonly ContextFinanceiro _context;
        public AtivosFixosQueryHandlers(ContextFinanceiro context) => _context = context;

        private static (int pagina, int tamanho) Normaliza(int pagina, int tamanho)
            => (pagina <= 0 ? 1 : pagina, tamanho is <= 0 or > 100 ? 20 : tamanho);

        public async Task<CommandResult> Handle(ListarAtivosFixosQuery request, CancellationToken ct)
        {
            var (pagina, tamanho) = Normaliza(request.Pagina, request.TamanhoPagina);
            var query = _context.AtivosFixos.AsNoTracking().AsQueryable();
            if (request.Status.HasValue) query = query.Where(a => a.Status == request.Status.Value);
            if (request.GrupoBemId.HasValue) query = query.Where(a => a.GrupoBemId == request.GrupoBemId.Value);
            var total = await query.CountAsync(ct);
            var itens = await query.OrderBy(a => a.Descricao).Skip((pagina - 1) * tamanho).Take(tamanho)
                .Select(a => new { a.Id, a.NumeroNb, a.Nome, a.Descricao, a.ValorCompra, a.ValorAtualizado, a.Status, a.GrupoBemId, a.DataAquisicao })
                .ToListAsync(ct);
            return CommandResult.Ok("Ativos fixos listados.", new { total, pagina, tamanho, itens });
        }

        public async Task<CommandResult> Handle(ObterAtivoFixoPorIdQuery request, CancellationToken ct)
        {
            var ativo = await _context.AtivosFixos.AsNoTracking().FirstOrDefaultAsync(a => a.Id == request.Id, ct);
            return ativo == null ? CommandResult.Falha("Ativo fixo não encontrado.") : CommandResult.Ok("Ativo fixo encontrado.", ativo);
        }

        public async Task<CommandResult> Handle(ListarGruposBemQuery request, CancellationToken ct)
        {
            var (pagina, tamanho) = Normaliza(request.Pagina, request.TamanhoPagina);
            var query = _context.GruposBem.AsNoTracking();
            var total = await query.CountAsync(ct);
            var itens = await query.OrderBy(g => g.Nome).Skip((pagina - 1) * tamanho).Take(tamanho)
                .Select(g => new { g.Id, g.Codigo, g.Nome, g.ContaAtivoId, g.ContaDepreciacaoId, g.ContaBaixaId, g.Ativo }).ToListAsync(ct);
            return CommandResult.Ok("Grupos de bem listados.", new { total, pagina, tamanho, itens });
        }

        public async Task<CommandResult> Handle(ListarDepreciacoesAtivoQuery request, CancellationToken ct)
        {
            var itens = await _context.DepreciacoesMensais.AsNoTracking().Where(d => d.AtivoId == request.AtivoId)
                .OrderByDescending(d => d.Competencia)
                .Select(d => new { d.Id, d.Competencia, d.Valor, d.MetodoDepreciacao, d.TaxaAplicada, d.Contabilizado }).ToListAsync(ct);
            return CommandResult.Ok("Depreciações do ativo listadas.", itens);
        }

        public async Task<CommandResult> Handle(ListarMovimentacoesAtivoQuery request, CancellationToken ct)
        {
            var itens = await _context.MovimentacoesAtivo.AsNoTracking().Where(m => m.AtivoId == request.AtivoId)
                .OrderByDescending(m => m.DataMovimentacao)
                .Select(m => new { m.Id, m.TipoMovimentacao, m.DataMovimentacao, m.Valor, m.Observacao }).ToListAsync(ct);
            return CommandResult.Ok("Movimentações do ativo listadas.", itens);
        }
    }
}
