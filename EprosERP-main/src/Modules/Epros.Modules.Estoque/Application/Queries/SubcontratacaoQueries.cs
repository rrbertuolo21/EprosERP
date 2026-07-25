using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MediatR;
using Epros.Shared.Application.Models;
using Epros.Modules.Estoque.Infrastructure.Data;

namespace Epros.Modules.Estoque.Application.Queries
{
    /// <summary>Consultas do submódulo Subcontratação (EST-SUB). Tenant é aplicado pelo filtro global.</summary>
    public record ListarSubOrdensQuery(Guid? FornecedorId = null, int Pagina = 1, int TamanhoPagina = 20) : IRequest<CommandResult>;

    public record ObterSubOrdemPorIdQuery(Guid Id) : IRequest<CommandResult>;

    public record ListarSubSaldosTerceiroQuery(Guid? FornecedorId = null, Guid? ProdutoId = null) : IRequest<CommandResult>;

    public class ListarSubOrdensQueryHandler : IRequestHandler<ListarSubOrdensQuery, CommandResult>
    {
        private readonly ContextEstoque _context;

        public ListarSubOrdensQueryHandler(ContextEstoque context) => _context = context;

        public async Task<CommandResult> Handle(ListarSubOrdensQuery request, CancellationToken cancellationToken)
        {
            var query = _context.SubOrdens.AsNoTracking().Where(o => o.DeletadoEm == null).AsQueryable();

            if (request.FornecedorId.HasValue)
                query = query.Where(o => o.FornecedorId == request.FornecedorId.Value);

            var total = await query.CountAsync(cancellationToken);
            var itens = await query
                .OrderByDescending(o => o.CriadoEm)
                .Skip((request.Pagina - 1) * request.TamanhoPagina)
                .Take(request.TamanhoPagina)
                .Select(o => new { o.Id, o.NumeroOrdem, o.FornecedorId, o.OrdemProducaoId, o.Status, o.DataEmissao, o.DataPrevistaRetorno })
                .ToListAsync(cancellationToken);

            return CommandResult.Ok("OK", new { Total = total, Pagina = request.Pagina, Itens = itens });
        }
    }

    public class ObterSubOrdemPorIdQueryHandler : IRequestHandler<ObterSubOrdemPorIdQuery, CommandResult>
    {
        private readonly ContextEstoque _context;

        public ObterSubOrdemPorIdQueryHandler(ContextEstoque context) => _context = context;

        public async Task<CommandResult> Handle(ObterSubOrdemPorIdQuery request, CancellationToken cancellationToken)
        {
            var o = await _context.SubOrdens.AsNoTracking()
                .Include(x => x.Itens)
                .FirstOrDefaultAsync(x => x.Id == request.Id && x.DeletadoEm == null, cancellationToken);
            if (o == null)
                return CommandResult.Falha("Ordem de subcontratação não encontrada.");

            return CommandResult.Ok("OK", new
            {
                o.Id,
                o.NumeroOrdem,
                o.FornecedorId,
                o.OrdemProducaoId,
                o.Status,
                o.DataEmissao,
                o.DataPrevistaRetorno,
                o.Observacao,
                Itens = o.Itens.Where(i => i.DeletadoEm == null).Select(i => new { i.Id, i.ProdutoId, i.QuantidadePlanejada, i.Unidade, i.OperacaoTerceirizada })
            });
        }
    }

    public class ListarSubSaldosTerceiroQueryHandler : IRequestHandler<ListarSubSaldosTerceiroQuery, CommandResult>
    {
        private readonly ContextEstoque _context;

        public ListarSubSaldosTerceiroQueryHandler(ContextEstoque context) => _context = context;

        public async Task<CommandResult> Handle(ListarSubSaldosTerceiroQuery request, CancellationToken cancellationToken)
        {
            var query = _context.SubSaldosTerceiro.AsNoTracking().Where(s => s.DeletadoEm == null).AsQueryable();

            if (request.FornecedorId.HasValue)
                query = query.Where(s => s.FornecedorId == request.FornecedorId.Value);
            if (request.ProdutoId.HasValue)
                query = query.Where(s => s.ProdutoId == request.ProdutoId.Value);

            var itens = await query
                .OrderByDescending(s => s.UltimoMovimento)
                .Select(s => new { s.Id, s.FornecedorId, s.ProdutoId, s.OrdemId, s.QuantidadeEnviada, s.QuantidadeRetornada, s.QuantidadeEmPoderTerceiro, s.QuantidadePerda, s.UltimoMovimento })
                .ToListAsync(cancellationToken);

            return CommandResult.Ok("OK", new { Itens = itens });
        }
    }
}
