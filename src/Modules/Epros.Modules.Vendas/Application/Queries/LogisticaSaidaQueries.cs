using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Vendas.Domain.Enums;
using Epros.Modules.Vendas.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Vendas.Application.Queries
{
    // ===================== Logística de Saída (VEN-LDS) =====================

    public record ListarExpedicoesQuery(
        Guid? PedidoId = null,
        EExpedicaoStatus? Status = null,
        int Pagina = 1,
        int TamanhoPagina = 20) : IQuery<CommandResult>;

    public class ListarExpedicoesQueryHandler : IRequestHandler<ListarExpedicoesQuery, CommandResult>
    {
        private readonly ContextVendas _context;
        private readonly ITenantProvider _tenantProvider;

        public ListarExpedicoesQueryHandler(ContextVendas context, ITenantProvider tenantProvider)
        {
            _context = context; _tenantProvider = tenantProvider;
        }

        public async Task<CommandResult> Handle(ListarExpedicoesQuery request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var query = _context.Expedicoes.AsNoTracking().Where(e => e.TenantId == tenantId);
            if (request.PedidoId.HasValue) query = query.Where(e => e.PedidoId == request.PedidoId.Value);
            if (request.Status.HasValue) query = query.Where(e => e.Status == request.Status.Value);
            var total = await query.CountAsync(cancellationToken);
            var itens = await query
                .OrderByDescending(e => e.CriadoEm)
                .Skip((request.Pagina - 1) * request.TamanhoPagina)
                .Take(request.TamanhoPagina)
                .Select(e => new { e.Id, e.PedidoId, e.DocumentoFiscalId, Status = (int)e.Status, e.DataExpedicao, e.DataConfirmacao })
                .ToListAsync(cancellationToken);
            return CommandResult.Ok("Expedições listadas.", new { total, request.Pagina, request.TamanhoPagina, itens });
        }
    }

    public record ObterExpedicaoPorIdQuery(Guid Id) : IQuery<CommandResult>;

    public class ObterExpedicaoPorIdQueryHandler : IRequestHandler<ObterExpedicaoPorIdQuery, CommandResult>
    {
        private readonly ContextVendas _context;
        private readonly ITenantProvider _tenantProvider;

        public ObterExpedicaoPorIdQueryHandler(ContextVendas context, ITenantProvider tenantProvider)
        {
            _context = context; _tenantProvider = tenantProvider;
        }

        public async Task<CommandResult> Handle(ObterExpedicaoPorIdQuery request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var expedicao = await _context.Expedicoes.AsNoTracking().FirstOrDefaultAsync(e => e.TenantId == tenantId && e.Id == request.Id, cancellationToken);
            if (expedicao == null) return CommandResult.Falha("Expedição não encontrada.");
            var local = await _context.ExpedicaoLocaisEntrega.AsNoTracking().Where(l => l.TenantId == tenantId && l.ExpedicaoId == request.Id)
                .Select(l => new { l.CpfCnpj, l.Logradouro, l.Numero, l.Bairro, l.NomeMunicipio, l.Uf }).FirstOrDefaultAsync(cancellationToken);
            var itens = await _context.ExpedicaoItensEntrega.AsNoTracking().Where(i => i.TenantId == tenantId && i.ExpedicaoId == request.Id)
                .Select(i => new { i.Id, i.PedidoItemId, i.ProdutoId, i.QuantidadeVendida, i.QuantidadeEntregue, i.SaldoEntrega }).ToListAsync(cancellationToken);
            return CommandResult.Ok("Expedição encontrada.", new { expedicao.Id, expedicao.PedidoId, Status = (int)expedicao.Status, local, itens });
        }
    }
}
