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
    // ===================== Planejamento de Demanda (VEN-PDM) =====================

    public record ListarDemandaPrevisoesQuery(EDemandaStatus? Status = null, int Pagina = 1, int TamanhoPagina = 20) : IQuery<CommandResult>;

    public class ListarDemandaPrevisoesQueryHandler : IRequestHandler<ListarDemandaPrevisoesQuery, CommandResult>
    {
        private readonly ContextVendas _context;
        private readonly ITenantProvider _tenantProvider;

        public ListarDemandaPrevisoesQueryHandler(ContextVendas context, ITenantProvider tenantProvider)
        {
            _context = context; _tenantProvider = tenantProvider;
        }

        public async Task<CommandResult> Handle(ListarDemandaPrevisoesQuery request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var query = _context.DemandaPrevisoes.AsNoTracking().Where(p => p.TenantId == tenantId);
            if (request.Status.HasValue) query = query.Where(p => p.Status == request.Status.Value);
            var total = await query.CountAsync(cancellationToken);
            var itens = await query
                .OrderByDescending(p => p.CriadoEm)
                .Skip((request.Pagina - 1) * request.TamanhoPagina)
                .Take(request.TamanhoPagina)
                .Select(p => new { p.Id, p.Codigo, p.Nome, p.PeriodoInicio, p.PeriodoFim, Status = p.Status.ToString() })
                .ToListAsync(cancellationToken);
            return CommandResult.Ok("Previsões listadas.", new { total, request.Pagina, request.TamanhoPagina, itens });
        }
    }

    public record ObterDemandaPrevisaoPorIdQuery(Guid Id) : IQuery<CommandResult>;

    public class ObterDemandaPrevisaoPorIdQueryHandler : IRequestHandler<ObterDemandaPrevisaoPorIdQuery, CommandResult>
    {
        private readonly ContextVendas _context;
        private readonly ITenantProvider _tenantProvider;

        public ObterDemandaPrevisaoPorIdQueryHandler(ContextVendas context, ITenantProvider tenantProvider)
        {
            _context = context; _tenantProvider = tenantProvider;
        }

        public async Task<CommandResult> Handle(ObterDemandaPrevisaoPorIdQuery request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var previsao = await _context.DemandaPrevisoes.AsNoTracking().FirstOrDefaultAsync(p => p.TenantId == tenantId && p.Id == request.Id, cancellationToken);
            if (previsao == null) return CommandResult.Falha("Previsão não encontrada.");
            var itens = await _context.DemandaItens.AsNoTracking().Where(i => i.TenantId == tenantId && i.PrevisaoId == request.Id)
                .Select(i => new { i.Id, i.ProdutoId, i.Periodo, i.QuantidadeHistorica, i.QuantidadePrevista, i.QuantidadeAjustada, i.QuantidadeAprovada })
                .ToListAsync(cancellationToken);
            return CommandResult.Ok("Previsão encontrada.", new { previsao.Id, previsao.Codigo, previsao.Nome, previsao.PeriodoInicio, previsao.PeriodoFim, Status = previsao.Status.ToString(), itens });
        }
    }
}
