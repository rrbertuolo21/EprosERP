using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Shared.Application.Contracts;
using Epros.Modules.Aplicativo.Application.Dtos;
using Epros.Modules.Aplicativo.Application.Queries;
using Epros.Modules.GestaoClientes.Domain.Entities;
using Epros.Modules.GestaoClientes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Aplicativo.Application.Handlers
{
    public class ListarPedidosClienteQueryHandler : IQueryHandler<ListarPedidosClienteQuery, IEnumerable<PedidoSaaSDto>>
    {
        private readonly ContextGestaoClientes _context;
        private readonly ITenantProvider _tenantProvider;

        public ListarPedidosClienteQueryHandler(
            ContextGestaoClientes context,
            ITenantProvider tenantProvider)
        {
            _context = context;
            _tenantProvider = tenantProvider;
        }

        public async Task<IEnumerable<PedidoSaaSDto>> Handle(ListarPedidosClienteQuery request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();

            var cliente = await _context.Clientes
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(c => c.TenantId == tenantId && c.Ativo && c.DeletadoEm == null, cancellationToken);

            if (cliente == null) return Enumerable.Empty<PedidoSaaSDto>();

            var pedidos = await _context.PedidosSaaS
                .IgnoreQueryFilters()
                .Where(p => p.ClienteId == cliente.Id && p.DeletadoEm == null)
                .OrderByDescending(p => p.CriadoEm)
                .Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            var planoIds = pedidos.Select(p => p.PlanoId).Distinct().ToList();
            var planos = await _context.Planos
                .IgnoreQueryFilters()
                .Where(p => planoIds.Contains(p.Id))
                .ToDictionaryAsync(p => p.Id, p => p.Nome, cancellationToken);

            return pedidos.Select(p => new PedidoSaaSDto(
                Id: p.Id,
                ClienteId: p.ClienteId,
                PlanoId: p.PlanoId,
                PlanoNome: planos.TryGetValue(p.PlanoId, out var nome) ? nome : "Desconhecido",
                CupomId: p.CupomId,
                ValorBase: p.ValorBase,
                ValorDesconto: p.ValorDesconto,
                ValorTotal: p.ValorTotal,
                Moeda: p.Moeda,
                MetodoPagamento: p.MetodoPagamento,
                Status: p.Status.ToString(),
                CriadoEm: p.CriadoEm
            )).ToList();
        }
    }

    public class ObterTransferenciasPendentesQueryHandler : IQueryHandler<ObterTransferenciasPendentesQuery, IEnumerable<TransferenciaPendenteDto>>
    {
        private readonly ContextGestaoClientes _context;

        public ObterTransferenciasPendentesQueryHandler(ContextGestaoClientes context)
        {
            _context = context;
        }

        public async Task<IEnumerable<TransferenciaPendenteDto>> Handle(ObterTransferenciasPendentesQuery request, CancellationToken cancellationToken)
        {
            var transferencias = await _context.PagamentosTransferencias
                .IgnoreQueryFilters()
                .Where(p => p.Status == PagamentoTransferenciaStatus.Pending && p.DeletadoEm == null)
                .OrderBy(p => p.CriadoEm)
                .Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            var pagTransferenciaIds = transferencias.Select(t => t.Id).ToList();
            var comprovantes = await _context.ComprovantesPagamentos
                .IgnoreQueryFilters()
                .Where(c => pagTransferenciaIds.Contains(c.PagamentoTransferenciaId) && c.DeletadoEm == null)
                .ToDictionaryAsync(c => c.PagamentoTransferenciaId, cancellationToken);

            return transferencias.Select(t => {
                comprovantes.TryGetValue(t.Id, out var comp);
                return new TransferenciaPendenteDto(
                    Id: t.Id,
                    FaturaId: t.FaturaId,
                    PedidoId: t.PedidoId,
                    Valor: t.Valor,
                    Moeda: t.Moeda,
                    Status: t.Status.ToString(),
                    CriadoEm: t.CriadoEm,
                    NomeArquivo: comp?.NomeArquivo ?? string.Empty,
                    CaminhoArquivo: comp?.CaminhoArquivo ?? string.Empty,
                    TamanhoBytes: comp?.TamanhoBytes ?? 0
                );
            }).ToList();
        }
    }
}
