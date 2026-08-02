using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Aplicativo.Application.Plataforma.Analytics;
using Epros.Modules.Aplicativo.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Epros.Tests.Plataforma
{
    /// <summary>PLT · Analytics — catálogo de KPIs versionado, snapshots read-side, dashboards.</summary>
    public class AnalyticsPlataformaTests
    {
        private static ContextAplicativo Novo(string db)
        {
            var options = new DbContextOptionsBuilder<ContextAplicativo>().UseInMemoryDatabase(db).Options;
            return new ContextAplicativo(options, T(), U());
        }

        private static ITenantProvider T() => new PlataformaTestFixtures.TestTenantProvider("tenant-1");
        private static ICurrentUser U() => new PlataformaTestFixtures.TestCurrentUser("user-1");

        [Fact]
        public async Task Definir_Kpi_Duas_Vezes_Versiona_E_Desativa_Anterior()
        {
            using var ctx = Novo(nameof(Definir_Kpi_Duas_Vezes_Versiona_E_Desativa_Anterior));
            var h = new DefinirKpiCommandHandler(ctx, T(), U());
            await h.Handle(new DefinirKpiCommand("FAT", "Faturamento", null, "R$", "Financeiro", "Financeiro", "q1"), CancellationToken.None);
            var r2 = await h.Handle(new DefinirKpiCommand("FAT", "Faturamento v2", null, "R$", "Financeiro", "Financeiro", "q2"), CancellationToken.None);

            Assert.True(r2.Sucesso);
            var todos = await ctx.DefinicoesKpi.OrderBy(k => k.Versao).ToListAsync();
            Assert.Equal(2, todos.Count);
            Assert.False(todos[0].Ativo);
            Assert.True(todos[1].Ativo);
            Assert.Equal(2, todos[1].Versao);
        }

        [Fact]
        public async Task Snapshot_Exige_Kpi_Existente()
        {
            using var ctx = Novo(nameof(Snapshot_Exige_Kpi_Existente));
            var h = new RegistrarSnapshotMetricaCommandHandler(ctx, T(), U());
            var r = await h.Handle(new RegistrarSnapshotMetricaCommand("INEXISTENTE", "2026-07", 100m, null, "teste"), CancellationToken.None);
            Assert.False(r.Sucesso);
        }

        [Fact]
        public async Task Snapshot_Idempotente_Atualiza_Valor_Por_Competencia()
        {
            using var ctx = Novo(nameof(Snapshot_Idempotente_Atualiza_Valor_Por_Competencia));
            await new DefinirKpiCommandHandler(ctx, T(), U()).Handle(
                new DefinirKpiCommand("HORAS", "Horas", null, "h", null, null, null), CancellationToken.None);
            var h = new RegistrarSnapshotMetricaCommandHandler(ctx, T(), U());
            await h.Handle(new RegistrarSnapshotMetricaCommand("HORAS", "2026-07", 100m, "filial-1", "app"), CancellationToken.None);
            await h.Handle(new RegistrarSnapshotMetricaCommand("HORAS", "2026-07", 150m, "filial-1", "app"), CancellationToken.None);

            var snaps = await ctx.SnapshotsMetrica.ToListAsync();
            Assert.Single(snaps);
            Assert.Equal(150m, snaps[0].Valor);
        }

        [Fact]
        public async Task Serie_Retorna_Snapshots_Ordenados_Por_Competencia()
        {
            using var ctx = Novo(nameof(Serie_Retorna_Snapshots_Ordenados_Por_Competencia));
            await new DefinirKpiCommandHandler(ctx, T(), U()).Handle(
                new DefinirKpiCommand("REC", "Receita", null, "R$", null, null, null), CancellationToken.None);
            var reg = new RegistrarSnapshotMetricaCommandHandler(ctx, T(), U());
            await reg.Handle(new RegistrarSnapshotMetricaCommand("REC", "2026-08", 200m, null, null), CancellationToken.None);
            await reg.Handle(new RegistrarSnapshotMetricaCommand("REC", "2026-06", 100m, null, null), CancellationToken.None);
            await reg.Handle(new RegistrarSnapshotMetricaCommand("REC", "2026-07", 150m, null, null), CancellationToken.None);

            var serie = await new ObterSerieMetricaQueryHandler(ctx).Handle(new ObterSerieMetricaQuery("REC"), CancellationToken.None);
            Assert.Equal(new[] { "2026-06", "2026-07", "2026-08" }, serie.Select(p => p.Competencia).ToArray());
        }

        [Fact]
        public async Task Widget_Exige_Dashboard_E_Kpi()
        {
            using var ctx = Novo(nameof(Widget_Exige_Dashboard_E_Kpi));
            await new DefinirKpiCommandHandler(ctx, T(), U()).Handle(
                new DefinirKpiCommand("K1", "K", null, "un", null, null, null), CancellationToken.None);
            var dr = await new CriarDashboardCommandHandler(ctx, T(), U()).Handle(
                new CriarDashboardCommand("D1", "Dash", null, null, false), CancellationToken.None);
            var dashId = await ctx.Dashboards.Select(d => d.Id).FirstAsync();

            var wh = new AdicionarWidgetDashboardCommandHandler(ctx, T(), U());
            var ok = await wh.Handle(new AdicionarWidgetDashboardCommand(dashId, "K1", "Meu KPI", "numero", 1, null), CancellationToken.None);
            var falha = await wh.Handle(new AdicionarWidgetDashboardCommand(dashId, "INEXISTENTE", "x", "numero", 2, null), CancellationToken.None);

            Assert.True(dr.Sucesso);
            Assert.True(ok.Sucesso);
            Assert.False(falha.Sucesso);
        }

        [Fact]
        public async Task Exportacao_Rejeita_Formato_Invalido()
        {
            using var ctx = Novo(nameof(Exportacao_Rejeita_Formato_Invalido));
            var h = new SolicitarExportacaoAnalyticsCommandHandler(ctx, T(), U());
            var r = await h.Handle(new SolicitarExportacaoAnalyticsCommand("faturamento", "PDF", null), CancellationToken.None);
            Assert.False(r.Sucesso);
        }
    }
}
