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
    // ===================== Faturamento Comercial Internacional (VEN-FCI) =====================

    /// <summary>FCI-011/012: filtro de faturas (9) ou notas de crédito (10) por período/cliente/número/status.</summary>
    public record ListarFciDocumentosQuery(
        EFciTipoDocumento TipoDocumento,
        DateTime? DataInicial = null,
        DateTime? DataFinal = null,
        Guid? ClienteId = null,
        string? Numero = null,
        EFciStatus? Status = null,
        int Pagina = 1,
        int TamanhoPagina = 20) : IQuery<CommandResult>;

    public class ListarFciDocumentosQueryHandler : IRequestHandler<ListarFciDocumentosQuery, CommandResult>
    {
        private readonly ContextVendas _context;
        private readonly ITenantProvider _tenantProvider;
        public ListarFciDocumentosQueryHandler(ContextVendas context, ITenantProvider tenantProvider)
        { _context = context; _tenantProvider = tenantProvider; }

        public async Task<CommandResult> Handle(ListarFciDocumentosQuery request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var query = _context.FciDocumentos.AsNoTracking()
                .Where(d => d.TenantId == tenantId && d.TipoDocumento == request.TipoDocumento);
            if (request.DataInicial.HasValue) query = query.Where(d => d.DataDocumento >= request.DataInicial.Value);
            if (request.DataFinal.HasValue) query = query.Where(d => d.DataDocumento <= request.DataFinal.Value);
            if (request.ClienteId.HasValue) query = query.Where(d => d.ClienteId == request.ClienteId.Value);
            if (!string.IsNullOrWhiteSpace(request.Numero)) query = query.Where(d => d.Numero == request.Numero);
            if (request.Status.HasValue) query = query.Where(d => d.Status == request.Status.Value);

            var total = await query.CountAsync(cancellationToken);
            var itens = await query
                .OrderByDescending(d => d.DataDocumento).ThenByDescending(d => d.Serial)
                .Skip((request.Pagina - 1) * request.TamanhoPagina).Take(request.TamanhoPagina)
                .Select(d => new { d.Id, d.Numero, d.DataDocumento, d.ClienteId, d.TotalGeral, d.SaldoEmAberto, Status = d.Status.ToString() })
                .ToListAsync(cancellationToken);
            return CommandResult.Ok("Documentos comerciais listados.", new { total, request.Pagina, request.TamanhoPagina, itens });
        }
    }

    /// <summary>FCI-010: consulta por id filtrando documento e tenant, com linhas.</summary>
    public record ObterFciDocumentoPorIdQuery(Guid Id) : IQuery<CommandResult>;

    public class ObterFciDocumentoPorIdQueryHandler : IRequestHandler<ObterFciDocumentoPorIdQuery, CommandResult>
    {
        private readonly ContextVendas _context;
        private readonly ITenantProvider _tenantProvider;
        public ObterFciDocumentoPorIdQueryHandler(ContextVendas context, ITenantProvider tenantProvider)
        { _context = context; _tenantProvider = tenantProvider; }

        public async Task<CommandResult> Handle(ObterFciDocumentoPorIdQuery request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var doc = await _context.FciDocumentos.AsNoTracking().Include(d => d.Itens)
                .FirstOrDefaultAsync(d => d.TenantId == tenantId && d.Id == request.Id, cancellationToken);
            if (doc == null) return CommandResult.Falha("Documento comercial não encontrado.");
            return CommandResult.Ok("Documento comercial encontrado.", new
            {
                doc.Id,
                doc.Numero,
                TipoDocumento = doc.TipoDocumento.ToString(),
                doc.DataDocumento,
                doc.ClienteId,
                doc.Moeda,
                doc.TaxaCambio,
                doc.Incoterm,
                doc.TotalBruto,
                doc.DescontoDocumento,
                doc.TotalImposto,
                doc.ValorFrete,
                doc.TotalLiquido,
                doc.TotalGeral,
                doc.ValorPago,
                doc.SaldoEmAberto,
                Status = doc.Status.ToString(),
                Itens = doc.Itens.Select(i => new { i.Id, i.ProdutoId, i.Quantidade, i.ValorUnitario, i.ValorBruto, i.ValorImposto, i.ValorTotal })
            });
        }
    }

    /// <summary>FCI-013: contas em aberto = documentos com saldo != 0 e status != Rascunho, por cliente.</summary>
    public record ListarFciContasEmAbertoQuery(Guid? ClienteId = null) : IQuery<CommandResult>;

    public class ListarFciContasEmAbertoQueryHandler : IRequestHandler<ListarFciContasEmAbertoQuery, CommandResult>
    {
        private readonly ContextVendas _context;
        private readonly ITenantProvider _tenantProvider;
        public ListarFciContasEmAbertoQueryHandler(ContextVendas context, ITenantProvider tenantProvider)
        { _context = context; _tenantProvider = tenantProvider; }

        public async Task<CommandResult> Handle(ListarFciContasEmAbertoQuery request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var query = _context.FciDocumentos.AsNoTracking()
                .Where(d => d.TenantId == tenantId && d.Status != EFciStatus.Rascunho && d.SaldoEmAberto != 0m);
            if (request.ClienteId.HasValue) query = query.Where(d => d.ClienteId == request.ClienteId.Value);
            var itens = await query
                .OrderBy(d => d.ClienteId).ThenBy(d => d.DataDocumento)
                .Select(d => new { d.Id, d.Numero, d.ClienteId, TipoDocumento = d.TipoDocumento.ToString(), d.TotalGeral, d.SaldoEmAberto, Status = d.Status.ToString() })
                .ToListAsync(cancellationToken);
            return CommandResult.Ok("Contas em aberto listadas.", new { itens });
        }
    }

    /// <summary>Lista impostos comerciais do tenant (FCI-049).</summary>
    public record ListarFciImpostosQuery(bool ApenasAtivos = false) : IQuery<CommandResult>;

    public class ListarFciImpostosQueryHandler : IRequestHandler<ListarFciImpostosQuery, CommandResult>
    {
        private readonly ContextVendas _context;
        private readonly ITenantProvider _tenantProvider;
        public ListarFciImpostosQueryHandler(ContextVendas context, ITenantProvider tenantProvider)
        { _context = context; _tenantProvider = tenantProvider; }

        public async Task<CommandResult> Handle(ListarFciImpostosQuery request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var query = _context.FciImpostos.AsNoTracking().Where(i => i.TenantId == tenantId);
            if (request.ApenasAtivos) query = query.Where(i => i.Ativo);
            var itens = await query.OrderBy(i => i.Nome)
                .Select(i => new { i.Id, i.Nome, i.Aliquota, i.Ativo }).ToListAsync(cancellationToken);
            return CommandResult.Ok("Impostos comerciais listados.", new { itens });
        }
    }
}
