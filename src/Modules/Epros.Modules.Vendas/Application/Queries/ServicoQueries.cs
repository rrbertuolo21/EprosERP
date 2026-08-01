using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Vendas.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Vendas.Application.Queries
{
    // ===================== Catálogo de serviços =====================

    public record ListarServicoCatalogoQuery(string? Localizar = null, bool ApenasAtivos = true, int Pagina = 1, int TamanhoPagina = 20) : IQuery<CommandResult>;
    public record ObterServicoCatalogoPorIdQuery(Guid Id) : IQuery<CommandResult>;

    public class ListarServicoCatalogoQueryHandler : IRequestHandler<ListarServicoCatalogoQuery, CommandResult>
    {
        private readonly ContextVendas _context;
        private readonly ITenantProvider _tenantProvider;

        public ListarServicoCatalogoQueryHandler(ContextVendas context, ITenantProvider tenantProvider)
        {
            _context = context;
            _tenantProvider = tenantProvider;
        }

        public async Task<CommandResult> Handle(ListarServicoCatalogoQuery request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var query = _context.ServicoCatalogos.AsNoTracking().Where(s => s.TenantId == tenantId);

            if (request.ApenasAtivos) query = query.Where(s => s.Ativo);
            if (!string.IsNullOrWhiteSpace(request.Localizar))
                query = query.Where(s => s.Nome.Contains(request.Localizar));

            var total = await query.CountAsync(cancellationToken);
            var itens = await query
                .OrderBy(s => s.Nome)
                .Skip((request.Pagina - 1) * request.TamanhoPagina)
                .Take(request.TamanhoPagina)
                .Select(s => new { s.Id, s.Nome, s.Descricao, s.Valor, s.Taxa, s.PermiteCamposCustomizados, s.Ativo })
                .ToListAsync(cancellationToken);

            return CommandResult.Ok("Serviços listados.", new { total, request.Pagina, request.TamanhoPagina, itens });
        }
    }

    public class ObterServicoCatalogoPorIdQueryHandler : IRequestHandler<ObterServicoCatalogoPorIdQuery, CommandResult>
    {
        private readonly ContextVendas _context;
        private readonly ITenantProvider _tenantProvider;

        public ObterServicoCatalogoPorIdQueryHandler(ContextVendas context, ITenantProvider tenantProvider)
        {
            _context = context;
            _tenantProvider = tenantProvider;
        }

        public async Task<CommandResult> Handle(ObterServicoCatalogoPorIdQuery request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var s = await _context.ServicoCatalogos.AsNoTracking()
                .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == request.Id, cancellationToken);
            if (s == null) return CommandResult.Falha("Serviço não encontrado.");
            return CommandResult.Ok("Serviço encontrado.", s);
        }
    }

    // ===================== Faturas de serviço =====================

    public record ListarServicoFaturaQuery(Guid? ClienteId = null, Guid? FuncionarioId = null, DateTime? DataInicio = null, DateTime? DataFim = null, int Pagina = 1, int TamanhoPagina = 20) : IQuery<CommandResult>;
    public record ObterServicoFaturaPorIdQuery(Guid Id) : IQuery<CommandResult>;

    public class ListarServicoFaturaQueryHandler : IRequestHandler<ListarServicoFaturaQuery, CommandResult>
    {
        private readonly ContextVendas _context;
        private readonly ITenantProvider _tenantProvider;

        public ListarServicoFaturaQueryHandler(ContextVendas context, ITenantProvider tenantProvider)
        {
            _context = context;
            _tenantProvider = tenantProvider;
        }

        public async Task<CommandResult> Handle(ListarServicoFaturaQuery request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var query = _context.ServicoFaturas.AsNoTracking().Where(f => f.TenantId == tenantId);

            if (request.ClienteId.HasValue) query = query.Where(f => f.ClienteId == request.ClienteId.Value);
            if (request.FuncionarioId.HasValue) query = query.Where(f => f.FuncionarioId == request.FuncionarioId.Value);
            if (request.DataInicio.HasValue) query = query.Where(f => f.DataFatura >= request.DataInicio.Value);
            if (request.DataFim.HasValue) query = query.Where(f => f.DataFatura <= request.DataFim.Value);

            var total = await query.CountAsync(cancellationToken);
            var itens = await query
                .OrderByDescending(f => f.DataFatura)
                .Skip((request.Pagina - 1) * request.TamanhoPagina)
                .Take(request.TamanhoPagina)
                .Select(f => new
                {
                    f.Id,
                    f.NumeroFatura,
                    f.ClienteId,
                    f.FuncionarioId,
                    f.DataFatura,
                    f.Status,
                    f.ValorImposto,
                    f.TotalImposto,
                    f.TotalGeral,
                    f.TotalLiquido,
                    f.ValorPago,
                    f.SaldoDevido,
                    f.Troco
                })
                .ToListAsync(cancellationToken);

            return CommandResult.Ok("Faturas de serviço listadas.", new { total, request.Pagina, request.TamanhoPagina, itens });
        }
    }

    public class ObterServicoFaturaPorIdQueryHandler : IRequestHandler<ObterServicoFaturaPorIdQuery, CommandResult>
    {
        private readonly ContextVendas _context;
        private readonly ITenantProvider _tenantProvider;

        public ObterServicoFaturaPorIdQueryHandler(ContextVendas context, ITenantProvider tenantProvider)
        {
            _context = context;
            _tenantProvider = tenantProvider;
        }

        public async Task<CommandResult> Handle(ObterServicoFaturaPorIdQuery request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var f = await _context.ServicoFaturas.AsNoTracking().Include(x => x.Linhas)
                .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == request.Id, cancellationToken);
            if (f == null) return CommandResult.Falha("Fatura de serviço não encontrada.");

            // EF §9.14: a consulta da fatura deve recuperar as linhas vinculadas.
            return CommandResult.Ok("Fatura encontrada.", new
            {
                f.Id,
                f.NumeroFatura,
                f.EmpresaId,
                f.ClienteId,
                f.FuncionarioId,
                f.ContaPagamentoId,
                f.DataFatura,
                f.Status,
                f.DescontoCabecalho,
                f.TotalDesconto,
                f.ValorImposto,
                f.TotalImposto,
                f.CustoEnvioEntrega,
                f.TotalGeral,
                f.TotalLiquido,
                f.ValorPago,
                f.SaldoDevido,
                f.Troco,
                f.Detalhes,
                Linhas = f.Linhas.Select(l => new { l.Id, l.ServicoId, l.NomeServico, l.Descricao, l.Quantidade, l.PrecoUnitario, l.DescontoPercentual, l.Total })
            });
        }
    }
}
