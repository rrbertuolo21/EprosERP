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
    // ===================== Garantias (VEN-GAR) =====================

    public record ListarGarantiaPoliticasQuery(bool ApenasAtivas = true, int Pagina = 1, int TamanhoPagina = 20) : IQuery<CommandResult>;

    public class ListarGarantiaPoliticasQueryHandler : IRequestHandler<ListarGarantiaPoliticasQuery, CommandResult>
    {
        private readonly ContextVendas _context;
        private readonly ITenantProvider _tenantProvider;

        public ListarGarantiaPoliticasQueryHandler(ContextVendas context, ITenantProvider tenantProvider)
        {
            _context = context; _tenantProvider = tenantProvider;
        }

        public async Task<CommandResult> Handle(ListarGarantiaPoliticasQuery request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var query = _context.GarantiaPoliticas.AsNoTracking().Where(p => p.TenantId == tenantId);
            if (request.ApenasAtivas) query = query.Where(p => p.Ativo);
            var total = await query.CountAsync(cancellationToken);
            var itens = await query
                .OrderBy(p => p.Nome)
                .Skip((request.Pagina - 1) * request.TamanhoPagina)
                .Take(request.TamanhoPagina)
                .Select(p => new { p.Id, p.Nome, p.Descricao, p.Duracao, p.TipoDuracao, p.Ativo })
                .ToListAsync(cancellationToken);
            return CommandResult.Ok("Políticas de garantia listadas.", new { total, request.Pagina, request.TamanhoPagina, itens });
        }
    }

    public record ObterGarantiaPoliticaPorIdQuery(Guid Id) : IQuery<CommandResult>;

    public class ObterGarantiaPoliticaPorIdQueryHandler : IRequestHandler<ObterGarantiaPoliticaPorIdQuery, CommandResult>
    {
        private readonly ContextVendas _context;
        private readonly ITenantProvider _tenantProvider;

        public ObterGarantiaPoliticaPorIdQueryHandler(ContextVendas context, ITenantProvider tenantProvider)
        {
            _context = context; _tenantProvider = tenantProvider;
        }

        public async Task<CommandResult> Handle(ObterGarantiaPoliticaPorIdQuery request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var politica = await _context.GarantiaPoliticas.AsNoTracking()
                .FirstOrDefaultAsync(p => p.TenantId == tenantId && p.Id == request.Id, cancellationToken);
            if (politica == null) return CommandResult.Falha("Política de garantia não encontrada.");
            return CommandResult.Ok("Política de garantia encontrada.", new { politica.Id, politica.Nome, politica.Descricao, politica.Duracao, politica.TipoDuracao, politica.Ativo });
        }
    }

    /// <summary>Consulta de cobertura no pós-venda (EF §6.5): retorna situação vigente/vencida/indeterminada.</summary>
    public record ConsultarGarantiaCoberturaQuery(Guid? VendaId = null, Guid? ProdutoId = null, Guid? ClienteId = null, string? NumeroSerieLote = null) : IQuery<CommandResult>;

    public class ConsultarGarantiaCoberturaQueryHandler : IRequestHandler<ConsultarGarantiaCoberturaQuery, CommandResult>
    {
        private readonly ContextVendas _context;
        private readonly ITenantProvider _tenantProvider;

        public ConsultarGarantiaCoberturaQueryHandler(ContextVendas context, ITenantProvider tenantProvider)
        {
            _context = context; _tenantProvider = tenantProvider;
        }

        public async Task<CommandResult> Handle(ConsultarGarantiaCoberturaQuery request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var query = _context.GarantiaCoberturas.AsNoTracking().Where(c => c.TenantId == tenantId);
            if (request.VendaId.HasValue) query = query.Where(c => c.VendaId == request.VendaId.Value);
            if (request.ProdutoId.HasValue) query = query.Where(c => c.ProdutoId == request.ProdutoId.Value);
            if (request.ClienteId.HasValue) query = query.Where(c => c.ClienteId == request.ClienteId.Value);
            if (!string.IsNullOrWhiteSpace(request.NumeroSerieLote)) query = query.Where(c => c.NumeroSerieLote == request.NumeroSerieLote);

            var itens = await query
                .OrderByDescending(c => c.CriadoEm)
                .Select(c => new { c.Id, c.GarantiaPoliticaId, c.VendaId, c.ProdutoId, c.ClienteId, c.NumeroSerieLote, c.DataOrigem, c.DataVencimento, Situacao = c.Situacao.ToString() })
                .ToListAsync(cancellationToken);
            return CommandResult.Ok("Coberturas consultadas.", new { itens });
        }
    }
}
