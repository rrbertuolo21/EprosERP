using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Aplicativo.Domain.Entities.Plataforma.Analytics;
using Epros.Modules.Aplicativo.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Shared.Domain.Events;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Aplicativo.Application.Plataforma.Analytics
{
    /// <summary>
    /// PLT · ANALYTICS (PD-06) — catálogo de KPIs versionado, snapshots (read-side), dashboards
    /// configuráveis e exportação auditada. A plataforma orquestra; a fórmula de negócio é do dono.
    /// </summary>
    public class DefinirKpiCommandHandler : ICommandHandler<DefinirKpiCommand>
    {
        private readonly ContextAplicativo _context;
        private readonly ITenantProvider _tenant;
        private readonly ICurrentUser _user;

        public DefinirKpiCommandHandler(ContextAplicativo context, ITenantProvider tenant, ICurrentUser user)
        {
            _context = context;
            _tenant = tenant;
            _user = user;
        }

        public async Task<CommandResult> Handle(DefinirKpiCommand request, CancellationToken ct)
        {
            var tenantId = _tenant.GetTenantId();
            var usuario = _user.GetUserId() ?? "system";

            var ativo = await _context.DefinicoesKpi
                .Where(k => k.Codigo == request.Codigo && k.Ativo)
                .OrderByDescending(k => k.Versao)
                .FirstOrDefaultAsync(ct);

            var novaVersao = (ativo?.Versao ?? 0) + 1;
            ativo?.Desativar(usuario);

            var kpi = new DefinicaoKpi(request.Codigo, request.Nome, request.Descricao, request.Unidade,
                request.Categoria, request.FonteModulo, request.ChaveConsulta, novaVersao, tenantId, usuario);
            if (!kpi.IsValid) return CommandResult.Falha(kpi.Notifications.Select(n => n.Message));

            _context.DefinicoesKpi.Add(kpi);
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok(novaVersao == 1 ? "KPI criado." : "Nova versão do KPI publicada.",
                new { kpi.Id, kpi.Codigo, kpi.Versao });
        }
    }

    public class RegistrarSnapshotMetricaCommandHandler : ICommandHandler<RegistrarSnapshotMetricaCommand>
    {
        private readonly ContextAplicativo _context;
        private readonly ITenantProvider _tenant;
        private readonly ICurrentUser _user;

        public RegistrarSnapshotMetricaCommandHandler(ContextAplicativo context, ITenantProvider tenant, ICurrentUser user)
        {
            _context = context;
            _tenant = tenant;
            _user = user;
        }

        public async Task<CommandResult> Handle(RegistrarSnapshotMetricaCommand request, CancellationToken ct)
        {
            var tenantId = _tenant.GetTenantId();
            var usuario = _user.GetUserId() ?? "system";

            var kpiExiste = await _context.DefinicoesKpi.AnyAsync(k => k.Codigo == request.KpiCodigo, ct);
            if (!kpiExiste) return CommandResult.Falha("KPI não encontrado no catálogo.");

            var dim = request.Dimensao;
            // Idempotência: um snapshot por (kpi, competência, dimensão) — atualiza o valor se já existir.
            var existente = await _context.SnapshotsMetrica.FirstOrDefaultAsync(s =>
                s.KpiCodigo == request.KpiCodigo && s.Competencia == request.Competencia && s.Dimensao == dim, ct);

            if (existente != null)
            {
                existente.AtualizarValor(request.Valor, request.Origem, usuario);
            }
            else
            {
                var snap = new SnapshotMetrica(request.KpiCodigo, request.Competencia, request.Valor,
                    dim, request.Origem, tenantId, usuario);
                if (!snap.IsValid) return CommandResult.Falha(snap.Notifications.Select(n => n.Message));
                _context.SnapshotsMetrica.Add(snap);
            }

            _context.OutboxMessages.Add(new OutboxMessage(tenantId,
                CatalogoEventosIntegracao.Plataforma.AnalyticsSnapshotGerado,
                JsonSerializer.Serialize(new { request.KpiCodigo, request.Competencia, request.Valor, request.Dimensao })));

            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Snapshot registrado.");
        }
    }

    public class CriarDashboardCommandHandler : ICommandHandler<CriarDashboardCommand>
    {
        private readonly ContextAplicativo _context;
        private readonly ITenantProvider _tenant;
        private readonly ICurrentUser _user;

        public CriarDashboardCommandHandler(ContextAplicativo context, ITenantProvider tenant, ICurrentUser user)
        {
            _context = context;
            _tenant = tenant;
            _user = user;
        }

        public async Task<CommandResult> Handle(CriarDashboardCommand request, CancellationToken ct)
        {
            var tenantId = _tenant.GetTenantId();
            var usuario = _user.GetUserId() ?? "system";

            if (await _context.Dashboards.AnyAsync(d => d.Codigo == request.Codigo, ct))
                return CommandResult.Falha("Já existe dashboard com este código.");

            var dash = new Dashboard(request.Codigo, request.Nome, request.Descricao, request.LayoutJson, request.Publico, tenantId, usuario);
            if (!dash.IsValid) return CommandResult.Falha(dash.Notifications.Select(n => n.Message));

            _context.Dashboards.Add(dash);
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Dashboard criado.", new { dash.Id });
        }
    }

    public class AdicionarWidgetDashboardCommandHandler : ICommandHandler<AdicionarWidgetDashboardCommand>
    {
        private readonly ContextAplicativo _context;
        private readonly ITenantProvider _tenant;
        private readonly ICurrentUser _user;

        public AdicionarWidgetDashboardCommandHandler(ContextAplicativo context, ITenantProvider tenant, ICurrentUser user)
        {
            _context = context;
            _tenant = tenant;
            _user = user;
        }

        public async Task<CommandResult> Handle(AdicionarWidgetDashboardCommand request, CancellationToken ct)
        {
            var tenantId = _tenant.GetTenantId();
            var usuario = _user.GetUserId() ?? "system";

            if (!await _context.Dashboards.AnyAsync(d => d.Id == request.DashboardId, ct))
                return CommandResult.Falha("Dashboard não encontrado.");
            if (!await _context.DefinicoesKpi.AnyAsync(k => k.Codigo == request.KpiCodigo, ct))
                return CommandResult.Falha("KPI não encontrado no catálogo.");

            var widget = new DashboardWidget(request.DashboardId, request.KpiCodigo, request.Titulo,
                request.TipoVisualizacao, request.Ordem, request.ConfigJson, tenantId, usuario);
            if (!widget.IsValid) return CommandResult.Falha(widget.Notifications.Select(n => n.Message));

            _context.DashboardWidgets.Add(widget);
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Widget adicionado.", new { widget.Id });
        }
    }

    public class SolicitarExportacaoAnalyticsCommandHandler : ICommandHandler<SolicitarExportacaoAnalyticsCommand>
    {
        private readonly ContextAplicativo _context;
        private readonly ITenantProvider _tenant;
        private readonly ICurrentUser _user;

        public SolicitarExportacaoAnalyticsCommandHandler(ContextAplicativo context, ITenantProvider tenant, ICurrentUser user)
        {
            _context = context;
            _tenant = tenant;
            _user = user;
        }

        public async Task<CommandResult> Handle(SolicitarExportacaoAnalyticsCommand request, CancellationToken ct)
        {
            var tenantId = _tenant.GetTenantId();
            var usuario = _user.GetUserId() ?? "system";

            var exp = new ExportacaoAnalytics(request.Recurso, request.Formato, request.FiltroJson, tenantId, usuario);
            if (!exp.IsValid) return CommandResult.Falha(exp.Notifications.Select(n => n.Message));

            // A geração do arquivo em si depende do ambiente (worker/storage). // valida-ambiente
            _context.ExportacoesAnalytics.Add(exp);
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Exportação solicitada (arquivo gerado no ambiente).", new { exp.Id, exp.Status });
        }
    }

    // ===================== Queries =====================

    public class ObterKpisQueryHandler : IQueryHandler<ObterKpisQuery, IReadOnlyList<KpiDto>>
    {
        private readonly ContextAplicativo _context;
        public ObterKpisQueryHandler(ContextAplicativo context) => _context = context;

        public async Task<IReadOnlyList<KpiDto>> Handle(ObterKpisQuery request, CancellationToken ct)
        {
            var q = _context.DefinicoesKpi.AsNoTracking().AsQueryable();
            if (request.ApenasAtivos) q = q.Where(k => k.Ativo);
            if (!string.IsNullOrWhiteSpace(request.Categoria)) q = q.Where(k => k.Categoria == request.Categoria);
            return await q.OrderBy(k => k.Codigo)
                .Select(k => new KpiDto(k.Id, k.Codigo, k.Nome, k.Descricao, k.Unidade, k.Categoria, k.FonteModulo, k.Versao, k.Ativo))
                .ToListAsync(ct);
        }
    }

    public class ObterDashboardsQueryHandler : IQueryHandler<ObterDashboardsQuery, IReadOnlyList<DashboardDto>>
    {
        private readonly ContextAplicativo _context;
        public ObterDashboardsQueryHandler(ContextAplicativo context) => _context = context;

        public async Task<IReadOnlyList<DashboardDto>> Handle(ObterDashboardsQuery request, CancellationToken ct)
            => await _context.Dashboards.AsNoTracking().OrderBy(d => d.Nome)
                .Select(d => new DashboardDto(d.Id, d.Codigo, d.Nome, d.Descricao, d.Publico, d.CriadoEm)).ToListAsync(ct);
    }

    public class ObterDashboardPorIdQueryHandler : IQueryHandler<ObterDashboardPorIdQuery, DashboardDetalheDto?>
    {
        private readonly ContextAplicativo _context;
        public ObterDashboardPorIdQueryHandler(ContextAplicativo context) => _context = context;

        public async Task<DashboardDetalheDto?> Handle(ObterDashboardPorIdQuery request, CancellationToken ct)
        {
            var d = await _context.Dashboards.AsNoTracking().FirstOrDefaultAsync(x => x.Id == request.Id, ct);
            if (d == null) return null;
            var widgets = await _context.DashboardWidgets.AsNoTracking()
                .Where(w => w.DashboardId == d.Id).OrderBy(w => w.Ordem)
                .Select(w => new WidgetDto(w.Id, w.KpiCodigo, w.Titulo, w.TipoVisualizacao, w.Ordem, w.ConfigJson))
                .ToListAsync(ct);
            return new DashboardDetalheDto(new DashboardDto(d.Id, d.Codigo, d.Nome, d.Descricao, d.Publico, d.CriadoEm), widgets);
        }
    }

    public class ObterSerieMetricaQueryHandler : IQueryHandler<ObterSerieMetricaQuery, IReadOnlyList<PontoSerieDto>>
    {
        private readonly ContextAplicativo _context;
        public ObterSerieMetricaQueryHandler(ContextAplicativo context) => _context = context;

        public async Task<IReadOnlyList<PontoSerieDto>> Handle(ObterSerieMetricaQuery request, CancellationToken ct)
        {
            var q = _context.SnapshotsMetrica.AsNoTracking().Where(s => s.KpiCodigo == request.KpiCodigo);
            if (!string.IsNullOrWhiteSpace(request.Dimensao)) q = q.Where(s => s.Dimensao == request.Dimensao);
            return await q.OrderBy(s => s.Competencia)
                .Select(s => new PontoSerieDto(s.Competencia, s.Valor, s.Dimensao, s.CapturadoEm)).ToListAsync(ct);
        }
    }

    public class ObterExportacoesAnalyticsQueryHandler : IQueryHandler<ObterExportacoesAnalyticsQuery, IReadOnlyList<ExportacaoAnalyticsDto>>
    {
        private readonly ContextAplicativo _context;
        public ObterExportacoesAnalyticsQueryHandler(ContextAplicativo context) => _context = context;

        public async Task<IReadOnlyList<ExportacaoAnalyticsDto>> Handle(ObterExportacoesAnalyticsQuery request, CancellationToken ct)
            => await _context.ExportacoesAnalytics.AsNoTracking().OrderByDescending(e => e.CriadoEm)
                .Select(e => new ExportacaoAnalyticsDto(e.Id, e.Recurso, e.Formato, e.Status, e.UrlRef, e.CriadoEm)).ToListAsync(ct);
    }
}
