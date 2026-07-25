using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MediatR;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Modules.Estoque.Domain.Enums;
using Epros.Modules.Estoque.Infrastructure.Data;

namespace Epros.Modules.Estoque.Application.Queries
{
    public record ListarLdeEntradasQuery(Guid? CompraId = null, Guid? FornecedorId = null, ESituacaoEntradaLogistica? Situacao = null, int Pagina = 1, int TamanhoPagina = 20) : IQuery<CommandResult>;
    public record ObterLdeEntradaPorIdQuery(Guid Id) : IQuery<CommandResult>;

    public class ListarLdeEntradasQueryHandler : IRequestHandler<ListarLdeEntradasQuery, CommandResult>
    {
        private readonly ContextEstoque _context;
        public ListarLdeEntradasQueryHandler(ContextEstoque context) => _context = context;

        public async Task<CommandResult> Handle(ListarLdeEntradasQuery request, CancellationToken cancellationToken)
        {
            var query = _context.LdeEntradas.AsNoTracking().Where(e => e.DeletadoEm == null);
            if (request.CompraId.HasValue) query = query.Where(e => e.CompraId == request.CompraId.Value);
            if (request.FornecedorId.HasValue) query = query.Where(e => e.FornecedorId == request.FornecedorId.Value);
            if (request.Situacao.HasValue) query = query.Where(e => e.Situacao == request.Situacao.Value);

            var total = await query.CountAsync(cancellationToken);
            var itens = await query
                .OrderByDescending(e => e.CriadoEm)
                .Skip((request.Pagina - 1) * request.TamanhoPagina)
                .Take(request.TamanhoPagina)
                .Select(e => new { e.Id, e.CompraId, e.FornecedorId, e.LocalEntregaId, e.DocumentoEntradaId, e.Situacao, e.DataConfirmacao, e.CriadoEm })
                .ToListAsync(cancellationToken);

            return CommandResult.Ok("OK", new { Total = total, request.Pagina, Itens = itens });
        }
    }

    public class ObterLdeEntradaPorIdQueryHandler : IRequestHandler<ObterLdeEntradaPorIdQuery, CommandResult>
    {
        private readonly ContextEstoque _context;
        public ObterLdeEntradaPorIdQueryHandler(ContextEstoque context) => _context = context;

        public async Task<CommandResult> Handle(ObterLdeEntradaPorIdQuery request, CancellationToken cancellationToken)
        {
            var e = await _context.LdeEntradas.AsNoTracking().FirstOrDefaultAsync(x => x.Id == request.Id && x.DeletadoEm == null, cancellationToken);
            if (e == null)
                return CommandResult.Falha("Entrada não encontrada.");

            var historico = await _context.LdeHistoricos.AsNoTracking()
                .Where(h => h.EntradaId == e.Id && h.DeletadoEm == null)
                .OrderBy(h => h.CriadoEm)
                .Select(h => new { h.Id, h.Evento, h.SituacaoAnterior, h.SituacaoNova, h.Motivo, h.UsuarioId, h.CriadoEm })
                .ToListAsync(cancellationToken);

            return CommandResult.Ok("OK", new
            {
                e.Id,
                e.CompraId,
                e.FornecedorId,
                e.LocalEntregaId,
                e.DocumentoEntradaId,
                e.Situacao,
                e.DataConfirmacao,
                e.MotivoCancelamentoEstorno,
                Historico = historico
            });
        }
    }
}
