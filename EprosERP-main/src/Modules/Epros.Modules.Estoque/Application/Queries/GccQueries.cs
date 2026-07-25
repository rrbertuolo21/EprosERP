using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MediatR;
using Epros.Shared.Application.Models;
using Epros.Modules.Estoque.Domain.Enums;
using Epros.Modules.Estoque.Infrastructure.Data;

namespace Epros.Modules.Estoque.Application.Queries
{
    /// <summary>
    /// Consultas do submódulo Gestão de Contratos de Compra (EST-GCC). Tenant é aplicado pelo filtro global.
    /// </summary>
    public record ListarGccContratosCompraQuery(Guid? FornecedorId = null, ESituacaoContratoCompra? Situacao = null, int Pagina = 1, int TamanhoPagina = 20) : IRequest<CommandResult>;

    public record ObterGccContratoCompraPorIdQuery(Guid Id) : IRequest<CommandResult>;

    public class ListarGccContratosCompraQueryHandler : IRequestHandler<ListarGccContratosCompraQuery, CommandResult>
    {
        private readonly ContextEstoque _context;

        public ListarGccContratosCompraQueryHandler(ContextEstoque context) => _context = context;

        public async Task<CommandResult> Handle(ListarGccContratosCompraQuery request, CancellationToken cancellationToken)
        {
            var query = _context.GccContratosCompra.AsNoTracking().Where(c => c.DeletadoEm == null).AsQueryable();

            if (request.FornecedorId.HasValue)
                query = query.Where(c => c.FornecedorId == request.FornecedorId.Value);
            if (request.Situacao.HasValue)
                query = query.Where(c => c.Situacao == request.Situacao.Value);

            var total = await query.CountAsync(cancellationToken);
            var itens = await query
                .OrderByDescending(c => c.CriadoEm)
                .Skip((request.Pagina - 1) * request.TamanhoPagina)
                .Take(request.TamanhoPagina)
                .Select(c => new { c.Id, c.FornecedorId, c.NumeroContrato, c.VigenciaInicio, c.VigenciaFim, c.ValorTotal, c.Situacao, c.CriadoEm })
                .ToListAsync(cancellationToken);

            return CommandResult.Ok("OK", new { Total = total, Pagina = request.Pagina, Itens = itens });
        }
    }

    public class ObterGccContratoCompraPorIdQueryHandler : IRequestHandler<ObterGccContratoCompraPorIdQuery, CommandResult>
    {
        private readonly ContextEstoque _context;

        public ObterGccContratoCompraPorIdQueryHandler(ContextEstoque context) => _context = context;

        public async Task<CommandResult> Handle(ObterGccContratoCompraPorIdQuery request, CancellationToken cancellationToken)
        {
            var c = await _context.GccContratosCompra.AsNoTracking()
                .Include(x => x.Itens)
                .FirstOrDefaultAsync(x => x.Id == request.Id && x.DeletadoEm == null, cancellationToken);
            if (c == null)
                return CommandResult.Falha("Contrato de compra não encontrado.");

            return CommandResult.Ok("OK", new
            {
                c.Id,
                c.FornecedorId,
                c.NumeroContrato,
                c.VigenciaInicio,
                c.VigenciaFim,
                c.ValorTotal,
                c.Situacao,
                c.Observacao,
                Itens = c.Itens.Where(i => i.DeletadoEm == null).Select(i => new { i.Id, i.ProdutoId, i.PrecoUnitario, i.QuantidadeComprometida, i.QuantidadeConsumida, i.SaldoQuantidade, i.SaldoValor })
            });
        }
    }
}
