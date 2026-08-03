using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MediatR;
using Epros.Modules.Estoque.Domain.Enums;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Modules.Estoque.Infrastructure.Data;

namespace Epros.Modules.Estoque.Application.Queries
{
    public record ListarLotesQuery(Guid? ProdutoId = null, EStatusLoteRastreabilidade? Status = null, int Pagina = 1, int TamanhoPagina = 20) : IQuery<CommandResult>;
    public record ObterLotePorIdQuery(Guid Id) : IQuery<CommandResult>;
    public record ListarNumerosSeriaisQuery(Guid? ProdutoId = null, EStatusNumeroSerial? Status = null, int Pagina = 1, int TamanhoPagina = 20) : IQuery<CommandResult>;

    /// <summary>[DECIDIDO D11] FEFO SUGERE o lote de menor validade na saída (não trava; operador pode override justificado).</summary>
    public record SugerirLoteFefoQuery(Guid EmpresaId, Guid ProdutoId, decimal Quantidade) : IQuery<CommandResult>;

    /// <summary>RLT-020: genealogia — do lote para seriais, bloqueios e recalls (navegação entrada↔saída).</summary>
    public record GenealogiaLoteQuery(Guid LoteId) : IQuery<CommandResult>;

    public class ListarLotesQueryHandler : IRequestHandler<ListarLotesQuery, CommandResult>
    {
        private readonly ContextEstoque _context;
        public ListarLotesQueryHandler(ContextEstoque context) => _context = context;

        public async Task<CommandResult> Handle(ListarLotesQuery request, CancellationToken cancellationToken)
        {
            var query = _context.LotesEstoque.AsNoTracking().Where(l => l.DeletadoEm == null);
            if (request.ProdutoId.HasValue) query = query.Where(l => l.ProdutoId == request.ProdutoId.Value);
            if (request.Status.HasValue) query = query.Where(l => l.Status == request.Status.Value);

            var total = await query.CountAsync(cancellationToken);
            var itens = await query
                .OrderBy(l => l.DataValidade)
                .Skip((request.Pagina - 1) * request.TamanhoPagina).Take(request.TamanhoPagina)
                .Select(l => new { l.Id, l.ProdutoId, l.CodigoLote, l.DataValidade, l.Status, l.QuantidadeDisponivel, l.QuantidadeBloqueada, l.QuantidadeConsumida })
                .ToListAsync(cancellationToken);

            return CommandResult.Ok("OK", new { Total = total, request.Pagina, Itens = itens });
        }
    }

    public class ObterLotePorIdQueryHandler : IRequestHandler<ObterLotePorIdQuery, CommandResult>
    {
        private readonly ContextEstoque _context;
        public ObterLotePorIdQueryHandler(ContextEstoque context) => _context = context;

        public async Task<CommandResult> Handle(ObterLotePorIdQuery request, CancellationToken cancellationToken)
        {
            var l = await _context.LotesEstoque.AsNoTracking().FirstOrDefaultAsync(x => x.Id == request.Id && x.DeletadoEm == null, cancellationToken);
            if (l == null) return CommandResult.Falha("Lote não encontrado.");
            return CommandResult.Ok("OK", new
            {
                l.Id,
                l.EmpresaId,
                l.ProdutoId,
                l.LocalId,
                l.CodigoLote,
                l.DataFabricacao,
                l.DataValidade,
                l.DataRecebimento,
                l.Origem,
                l.Status,
                l.QuantidadeRecebida,
                l.QuantidadeDisponivel,
                l.QuantidadeBloqueada,
                l.QuantidadeConsumida
            });
        }
    }

    public class ListarNumerosSeriaisQueryHandler : IRequestHandler<ListarNumerosSeriaisQuery, CommandResult>
    {
        private readonly ContextEstoque _context;
        public ListarNumerosSeriaisQueryHandler(ContextEstoque context) => _context = context;

        public async Task<CommandResult> Handle(ListarNumerosSeriaisQuery request, CancellationToken cancellationToken)
        {
            var query = _context.NumerosSeriais.AsNoTracking().Where(s => s.DeletadoEm == null);
            if (request.ProdutoId.HasValue) query = query.Where(s => s.ProdutoId == request.ProdutoId.Value);
            if (request.Status.HasValue) query = query.Where(s => s.Status == request.Status.Value);

            var total = await query.CountAsync(cancellationToken);
            var itens = await query
                .OrderBy(s => s.Numero)
                .Skip((request.Pagina - 1) * request.TamanhoPagina).Take(request.TamanhoPagina)
                .Select(s => new { s.Id, s.ProdutoId, s.Numero, s.Status, s.LoteId })
                .ToListAsync(cancellationToken);

            return CommandResult.Ok("OK", new { Total = total, request.Pagina, Itens = itens });
        }
    }

    public class SugerirLoteFefoQueryHandler : IRequestHandler<SugerirLoteFefoQuery, CommandResult>
    {
        private readonly ContextEstoque _context;
        public SugerirLoteFefoQueryHandler(ContextEstoque context) => _context = context;

        public async Task<CommandResult> Handle(SugerirLoteFefoQuery request, CancellationToken cancellationToken)
        {
            // [D11] FEFO: ordena por menor validade, ignora bloqueados/indisponíveis; SUGERE (não trava).
            var candidatos = await _context.LotesEstoque.AsNoTracking()
                .Where(l => l.DeletadoEm == null
                    && l.EmpresaId == request.EmpresaId
                    && l.ProdutoId == request.ProdutoId
                    && l.Status == EStatusLoteRastreabilidade.Ativo
                    && l.QuantidadeDisponivel > 0m)
                .OrderBy(l => l.DataValidade == null) // válidos com validade primeiro
                .ThenBy(l => l.DataValidade)
                .ThenBy(l => l.CriadoEm)
                .Select(l => new { l.Id, l.CodigoLote, l.DataValidade, l.QuantidadeDisponivel })
                .ToListAsync(cancellationToken);

            // Monta a sugestão acumulando até cobrir a quantidade solicitada (informativo — não consome).
            decimal restante = request.Quantidade;
            var sugestao = candidatos.Select(c =>
            {
                var alocar = restante > 0m ? Math.Min(c.QuantidadeDisponivel, restante) : 0m;
                restante -= alocar;
                return new { c.Id, c.CodigoLote, c.DataValidade, c.QuantidadeDisponivel, QuantidadeSugerida = alocar };
            }).ToList();

            var atendido = restante <= 0m;
            return CommandResult.Ok("OK", new
            {
                request.ProdutoId,
                request.Quantidade,
                Atendido = atendido,
                FaltaCobrir = atendido ? 0m : restante,
                Observacao = "FEFO sugere o lote de menor validade; o operador pode escolher outro com justificativa registrada [D11].",
                Sugestao = sugestao
            });
        }
    }

    public class GenealogiaLoteQueryHandler : IRequestHandler<GenealogiaLoteQuery, CommandResult>
    {
        private readonly ContextEstoque _context;
        public GenealogiaLoteQueryHandler(ContextEstoque context) => _context = context;

        public async Task<CommandResult> Handle(GenealogiaLoteQuery request, CancellationToken cancellationToken)
        {
            var lote = await _context.LotesEstoque.AsNoTracking().FirstOrDefaultAsync(l => l.Id == request.LoteId && l.DeletadoEm == null, cancellationToken);
            if (lote == null) return CommandResult.Falha("Lote não encontrado.");

            var seriais = await _context.NumerosSeriais.AsNoTracking()
                .Where(s => s.LoteId == lote.Id && s.DeletadoEm == null)
                .Select(s => new { s.Id, s.Numero, s.Status, s.FichaEntradaId, s.FichaSaidaId })
                .ToListAsync(cancellationToken);
            var bloqueios = await _context.BloqueiosLote.AsNoTracking()
                .Where(b => b.LoteId == lote.Id && b.DeletadoEm == null)
                .Select(b => new { b.Id, b.TipoBloqueio, b.Motivo, b.DataBloqueio, b.DataDesbloqueio })
                .ToListAsync(cancellationToken);
            var recalls = await _context.RecallsLote.AsNoTracking()
                .Where(r => r.LoteId == lote.Id && r.DeletadoEm == null)
                .Select(r => new { r.Id, r.CodigoRecall, r.Motivo, r.Status, r.DataAbertura, r.DataEncerramento })
                .ToListAsync(cancellationToken);

            return CommandResult.Ok("OK", new
            {
                Lote = new { lote.Id, lote.ProdutoId, lote.CodigoLote, lote.FichaEntradaId, lote.DataValidade, lote.Status },
                Seriais = seriais,
                Bloqueios = bloqueios,
                Recalls = recalls
            });
        }
    }
}
