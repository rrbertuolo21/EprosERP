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
    /// <summary>
    /// Consultas do submódulo Transporte e Frete TMS (EST-TMS) sobre o agregado CompraTransporte.
    /// Tenant é aplicado pelo filtro global do ContextBase.
    /// </summary>
    public record ListarTmsTransportesCompraQuery(Guid? CompraId = null, int Pagina = 1, int TamanhoPagina = 20) : IRequest<CommandResult>;

    public record ObterTmsTransporteCompraPorIdQuery(Guid Id) : IRequest<CommandResult>;

    public class ListarTmsTransportesCompraQueryHandler : IRequestHandler<ListarTmsTransportesCompraQuery, CommandResult>
    {
        private readonly ContextEstoque _context;

        public ListarTmsTransportesCompraQueryHandler(ContextEstoque context) => _context = context;

        public async Task<CommandResult> Handle(ListarTmsTransportesCompraQuery request, CancellationToken cancellationToken)
        {
            var query = _context.CompraTransportes.AsNoTracking().Where(t => t.DeletadoEm == null).AsQueryable();

            if (request.CompraId.HasValue)
                query = query.Where(t => t.CompraId == request.CompraId.Value);

            var total = await query.CountAsync(cancellationToken);
            var itens = await query
                .OrderByDescending(t => t.CriadoEm)
                .Skip((request.Pagina - 1) * request.TamanhoPagina)
                .Take(request.TamanhoPagina)
                .Select(t => new { t.Id, t.CompraId, t.Situacao, t.CriadoEm })
                .ToListAsync(cancellationToken);

            return CommandResult.Ok("OK", new { Total = total, Pagina = request.Pagina, Itens = itens });
        }
    }

    public class ObterTmsTransporteCompraPorIdQueryHandler : IRequestHandler<ObterTmsTransporteCompraPorIdQuery, CommandResult>
    {
        private readonly ContextEstoque _context;

        public ObterTmsTransporteCompraPorIdQueryHandler(ContextEstoque context) => _context = context;

        public async Task<CommandResult> Handle(ObterTmsTransporteCompraPorIdQuery request, CancellationToken cancellationToken)
        {
            var t = await _context.CompraTransportes.AsNoTracking()
                .Include(x => x.Transportadora)
                .Include(x => x.Veiculo)
                .Include(x => x.Volumes)
                .Include(x => x.Reboques)
                .FirstOrDefaultAsync(x => x.Id == request.Id && x.DeletadoEm == null, cancellationToken);
            if (t == null)
                return CommandResult.Falha("Transporte da compra não encontrado.");

            return CommandResult.Ok("OK", new
            {
                t.Id,
                t.CompraId,
                t.Situacao,
                Transportadora = t.Transportadora == null ? (object?)null : new { t.Transportadora.Id, t.Transportadora.Uf },
                Veiculo = t.Veiculo == null ? (object?)null : new { t.Veiculo.Id, t.Veiculo.Placa, t.Veiculo.Uf },
                Volumes = t.Volumes.Select(v => new { v.Id, v.QuantidadeVolumes, v.PesoLiquido, v.PesoBruto }),
                Reboques = t.Reboques.Select(r => new { r.Id, r.Placa, r.Uf })
            });
        }
    }
}
