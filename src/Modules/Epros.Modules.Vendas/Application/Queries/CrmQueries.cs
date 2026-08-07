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
    // ===================== Leads =====================

    public record ListarCrmLeadsQuery(
        Guid? EtapaId = null,
        ECrmLeadStatus? Status = null,
        Guid? ResponsavelUsuarioId = null,
        bool IncluirArquivados = false,
        string? Localizar = null,
        int Pagina = 1,
        int TamanhoPagina = 20) : IQuery<CommandResult>;

    public class ListarCrmLeadsQueryHandler : IRequestHandler<ListarCrmLeadsQuery, CommandResult>
    {
        private readonly ContextVendas _context;
        private readonly ITenantProvider _tenantProvider;

        public ListarCrmLeadsQueryHandler(ContextVendas context, ITenantProvider tenantProvider)
        {
            _context = context; _tenantProvider = tenantProvider;
        }

        public async Task<CommandResult> Handle(ListarCrmLeadsQuery request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var query = _context.CrmLeads.AsNoTracking().Where(l => l.TenantId == tenantId);

            // CRM-008: lead arquivado não aparece na visão operacional padrão.
            if (!request.IncluirArquivados) query = query.Where(l => l.EstadoArquivo == ECrmEstadoArquivo.Ativo);
            if (request.EtapaId.HasValue) query = query.Where(l => l.EtapaId == request.EtapaId.Value);
            if (request.Status.HasValue) query = query.Where(l => l.Status == request.Status.Value);
            if (request.ResponsavelUsuarioId.HasValue) query = query.Where(l => l.ResponsavelUsuarioId == request.ResponsavelUsuarioId.Value);
            if (!string.IsNullOrWhiteSpace(request.Localizar))
                query = query.Where(l => l.Nome.Contains(request.Localizar) ||
                                         (l.Email != null && l.Email.Contains(request.Localizar)) ||
                                         (l.Telefone != null && l.Telefone.Contains(request.Localizar)));

            var total = await query.CountAsync(cancellationToken);
            var itens = await query
                .OrderBy(l => l.PosicaoKanban).ThenByDescending(l => l.CriadoEm)
                .Skip((request.Pagina - 1) * request.TamanhoPagina)
                .Take(request.TamanhoPagina)
                .Select(l => new { l.Id, l.Nome, l.Email, l.Telefone, l.EtapaId, l.Status, l.ValorEstimado, l.PosicaoKanban, l.Convertido })
                .ToListAsync(cancellationToken);

            return CommandResult.Ok("Leads listados.", new { total, request.Pagina, request.TamanhoPagina, itens });
        }
    }

    public record ObterCrmLeadPorIdQuery(Guid Id) : IQuery<CommandResult>;

    public class ObterCrmLeadPorIdQueryHandler : IRequestHandler<ObterCrmLeadPorIdQuery, CommandResult>
    {
        private readonly ContextVendas _context;
        private readonly ITenantProvider _tenantProvider;

        public ObterCrmLeadPorIdQueryHandler(ContextVendas context, ITenantProvider tenantProvider)
        {
            _context = context; _tenantProvider = tenantProvider;
        }

        public async Task<CommandResult> Handle(ObterCrmLeadPorIdQuery request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var l = await _context.CrmLeads.AsNoTracking().FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == request.Id, cancellationToken);
            if (l == null) return CommandResult.Falha("Lead não encontrado.");
            return CommandResult.Ok("Lead encontrado.", l);
        }
    }

    // ===================== Setup (pipelines/etapas) =====================

    /// <summary>
    /// Lista os pipelines ATIVOS do tenant com suas etapas ativas (ordenadas). Alimenta o seletor de
    /// pipeline/etapa no cadastro de lead — sem pipeline/etapa o lead não gera oportunidade (regra de
    /// domínio ConverterCrmLead). Antes só havia POST de setup; o front não tinha como listar.
    /// </summary>
    public record ListarCrmPipelinesQuery() : IQuery<CommandResult>;

    public class ListarCrmPipelinesQueryHandler : IRequestHandler<ListarCrmPipelinesQuery, CommandResult>
    {
        private readonly ContextVendas _context;
        private readonly ITenantProvider _tenantProvider;

        public ListarCrmPipelinesQueryHandler(ContextVendas context, ITenantProvider tenantProvider)
        {
            _context = context; _tenantProvider = tenantProvider;
        }

        public async Task<CommandResult> Handle(ListarCrmPipelinesQuery request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var pipelines = await _context.CrmPipelines.AsNoTracking()
                .Where(p => p.TenantId == tenantId && p.Ativo)
                .OrderBy(p => p.Nome)
                .Select(p => new { p.Id, p.Nome })
                .ToListAsync(cancellationToken);

            var pipeIds = pipelines.Select(p => p.Id).ToList();
            var etapas = await _context.CrmEtapas.AsNoTracking()
                .Where(e => e.TenantId == tenantId && e.Ativo && pipeIds.Contains(e.PipelineId))
                .OrderBy(e => e.Ordem)
                .Select(e => new { e.Id, e.PipelineId, e.Nome, e.Ordem, TipoEtapa = (int)e.TipoEtapa })
                .ToListAsync(cancellationToken);

            var itens = pipelines.Select(p => new
            {
                p.Id,
                p.Nome,
                etapas = etapas.Where(e => e.PipelineId == p.Id)
                    .Select(e => new { e.Id, e.Nome, e.Ordem, e.TipoEtapa }).ToList()
            }).ToList();

            return CommandResult.Ok("Pipelines listados.", new { itens });
        }
    }

    // ===================== Oportunidades =====================

    public record ListarCrmOportunidadesQuery(
        Guid? EtapaId = null,
        ECrmOportunidadeStatus? Status = null,
        Guid? ClientePrincipalId = null,
        int Pagina = 1,
        int TamanhoPagina = 20) : IQuery<CommandResult>;

    public class ListarCrmOportunidadesQueryHandler : IRequestHandler<ListarCrmOportunidadesQuery, CommandResult>
    {
        private readonly ContextVendas _context;
        private readonly ITenantProvider _tenantProvider;

        public ListarCrmOportunidadesQueryHandler(ContextVendas context, ITenantProvider tenantProvider)
        {
            _context = context; _tenantProvider = tenantProvider;
        }

        public async Task<CommandResult> Handle(ListarCrmOportunidadesQuery request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var query = _context.CrmOportunidades.AsNoTracking().Where(o => o.TenantId == tenantId);

            if (request.EtapaId.HasValue) query = query.Where(o => o.EtapaId == request.EtapaId.Value);
            if (request.Status.HasValue) query = query.Where(o => o.Status == request.Status.Value);
            if (request.ClientePrincipalId.HasValue) query = query.Where(o => o.ClientePrincipalId == request.ClientePrincipalId.Value);

            var total = await query.CountAsync(cancellationToken);
            var itens = await query
                .OrderBy(o => o.PosicaoKanban).ThenByDescending(o => o.CriadoEm)
                .Skip((request.Pagina - 1) * request.TamanhoPagina)
                .Take(request.TamanhoPagina)
                .Select(o => new { o.Id, o.Nome, o.Valor, o.EtapaId, o.Status, o.ClientePrincipalId, o.PosicaoKanban })
                .ToListAsync(cancellationToken);

            return CommandResult.Ok("Oportunidades listadas.", new { total, request.Pagina, request.TamanhoPagina, itens });
        }
    }

    // ===================== Tickets =====================

    public record ListarCrmTicketsQuery(
        Guid? StatusId = null,
        Guid? ClienteId = null,
        bool IncluirArquivados = false,
        int Pagina = 1,
        int TamanhoPagina = 20) : IQuery<CommandResult>;

    public class ListarCrmTicketsQueryHandler : IRequestHandler<ListarCrmTicketsQuery, CommandResult>
    {
        private readonly ContextVendas _context;
        private readonly ITenantProvider _tenantProvider;

        public ListarCrmTicketsQueryHandler(ContextVendas context, ITenantProvider tenantProvider)
        {
            _context = context; _tenantProvider = tenantProvider;
        }

        public async Task<CommandResult> Handle(ListarCrmTicketsQuery request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var query = _context.CrmTickets.AsNoTracking().Where(t => t.TenantId == tenantId);

            if (!request.IncluirArquivados) query = query.Where(t => t.EstadoArquivo == ECrmEstadoArquivo.Ativo);
            if (request.StatusId.HasValue) query = query.Where(t => t.StatusId == request.StatusId.Value);
            if (request.ClienteId.HasValue) query = query.Where(t => t.ClienteId == request.ClienteId.Value);

            var total = await query.CountAsync(cancellationToken);
            var itens = await query
                .OrderByDescending(t => t.UltimaAtualizacao)
                .Skip((request.Pagina - 1) * request.TamanhoPagina)
                .Take(request.TamanhoPagina)
                .Select(t => new { t.Id, t.Numero, t.Titulo, t.StatusId, t.Prioridade, t.ClienteId, t.UltimaAtualizacao })
                .ToListAsync(cancellationToken);

            return CommandResult.Ok("Tickets listados.", new { total, request.Pagina, request.TamanhoPagina, itens });
        }
    }

    // ===================== Campanhas =====================

    public record ListarCrmCampanhasQuery(bool ApenasAtivas = true, int Pagina = 1, int TamanhoPagina = 20) : IQuery<CommandResult>;

    public class ListarCrmCampanhasQueryHandler : IRequestHandler<ListarCrmCampanhasQuery, CommandResult>
    {
        private readonly ContextVendas _context;
        private readonly ITenantProvider _tenantProvider;

        public ListarCrmCampanhasQueryHandler(ContextVendas context, ITenantProvider tenantProvider)
        {
            _context = context; _tenantProvider = tenantProvider;
        }

        public async Task<CommandResult> Handle(ListarCrmCampanhasQuery request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var query = _context.CrmCampanhas.AsNoTracking().Where(c => c.TenantId == tenantId);
            if (request.ApenasAtivas) query = query.Where(c => c.Ativo);

            var total = await query.CountAsync(cancellationToken);
            var itens = await query
                .OrderByDescending(c => c.CriadoEm)
                .Skip((request.Pagina - 1) * request.TamanhoPagina)
                .Take(request.TamanhoPagina)
                .Select(c => new { c.Id, c.Nome, c.Tipo, c.Status, c.DataInicio, c.DataFim, c.Orcamento, c.ReceitaEsperada })
                .ToListAsync(cancellationToken);

            return CommandResult.Ok("Campanhas listadas.", new { total, request.Pagina, request.TamanhoPagina, itens });
        }
    }
}
