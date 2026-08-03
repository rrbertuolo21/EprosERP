using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Aplicativo.Application.Handlers;
using Epros.Modules.Aplicativo.Application.Queries;
using Epros.Modules.Aplicativo.Infrastructure.Data;
using Epros.Modules.GestaoClientes.Application.Handlers;
using Epros.Modules.GestaoClientes.Application.Queries;
using Epros.Modules.GestaoClientes.Domain.Entities;
using Epros.Modules.GestaoClientes.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Epros.Tests
{
    /// <summary>
    /// 1.08G — Métricas SaaS REBASEADAS na AssinaturaCliente (MRR/ARPU/conversão trial→pago/LTV/inadimplência
    /// agregada) e relatórios financeiros AGREGADOS do Landlord (receita/inadimplência/comissão por período).
    /// Todos os valores são FACTUAIS/caixa; competência e apuração fiscal são Onda 2 (não testadas aqui).
    /// </summary>
    public class MetricasRelatorios108GTests
    {
        // ===== Infra de teste =====

        private sealed class TestTenantProvider : ITenantProvider
        {
            private readonly string _tenantId;
            public TestTenantProvider(string tenantId) => _tenantId = tenantId;
            public string GetTenantId() => _tenantId;
        }

        private sealed class TestCurrentUser : ICurrentUser
        {
            private readonly string _userId;
            public TestCurrentUser(string userId) => _userId = userId;
            public string? GetUserId() => _userId;
            public string? GetUserName() => "Landlord Tester";
            public string? GetUserEmail() => "landlord@siser.com";
        }

        private static void SetId(EntidadeSaaSBase entity, Guid id)
        {
            typeof(EntidadeSaaSBase)
                .GetField("<Id>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic)!
                .SetValue(entity, id);
        }

        private static ContextGestaoClientes NovoContextoGestao(string dbName, string tenantId)
        {
            var options = new DbContextOptionsBuilder<ContextGestaoClientes>()
                .UseInMemoryDatabase(dbName)
                .Options;
            return new ContextGestaoClientes(options, new TestTenantProvider(tenantId), new TestCurrentUser("admin-root"));
        }

        private static ContextAplicativo NovoContextoAplicativo(string dbName, string tenantId)
        {
            var options = new DbContextOptionsBuilder<ContextAplicativo>()
                .UseInMemoryDatabase(dbName)
                .Options;
            return new ContextAplicativo(options, new TestTenantProvider(tenantId), new TestCurrentUser("admin-root"));
        }

        private static Guid AddPlano(ContextGestaoClientes ctx, decimal preco, PlanoDuration duration)
        {
            var id = Guid.NewGuid();
            var plano = new Plano("Plano " + duration, preco, null, 10, 10, null, "system", "admin-root", duration: duration);
            SetId(plano, id);
            ctx.Planos.Add(plano);
            return id;
        }

        private static Guid AddCliente(ContextGestaoClientes ctx, string tenantId, Guid planoId, Guid? revendaId = null)
        {
            var id = Guid.NewGuid();
            var cliente = new Cliente("Cliente " + tenantId, "12.345.678/0001-90", $"{tenantId}@corp.com",
                planoId, revendaId, null, 10, StatusSaaS.Ativo, tenantId, "admin-root");
            SetId(cliente, id);
            ctx.Clientes.Add(cliente);
            return id;
        }

        private static AssinaturaCliente NovaAssinatura(Guid clienteId, Guid planoId, AssinaturaStatus status,
            string tenantId, DateTime? trialAte = null)
        {
            var a = new AssinaturaCliente(clienteId, planoId, status, DateTime.UtcNow, null, trialAte,
                "Pix", "tx", "{}", tenantId, "admin-root");
            return a;
        }

        // ===== 1) MRR rebaseado + ARPU + LTV =====

        [Fact]
        public async Task Dashboard_Mrr_Deve_Ser_Rebaseado_Na_Assinatura_Normalizado_E_Derivar_Arpu_E_Ltv()
        {
            var db = "g_mrr_" + Guid.NewGuid();
            using var gestao = NovoContextoGestao(db, "system");
            using var app = NovoContextoAplicativo(db + "_app", "system");
            var tenant = new TestTenantProvider("system");

            var planoMensal = AddPlano(gestao, 100m, PlanoDuration.Mensal);   // 100/mês
            var planoAnual = AddPlano(gestao, 1200m, PlanoDuration.Anual);    // 1200/12 = 100/mês
            var planoVitalicia = AddPlano(gestao, 999m, PlanoDuration.Vitalicia); // 0 (não recorrente)

            var clienteA = AddCliente(gestao, "tenant-a", planoMensal);
            var clienteB = AddCliente(gestao, "tenant-b", planoAnual);

            gestao.AssinaturasClientes.Add(NovaAssinatura(clienteA, planoMensal, AssinaturaStatus.Ativa, "tenant-a"));
            gestao.AssinaturasClientes.Add(NovaAssinatura(clienteB, planoAnual, AssinaturaStatus.Ativa, "tenant-b"));
            gestao.AssinaturasClientes.Add(NovaAssinatura(clienteB, planoVitalicia, AssinaturaStatus.Ativa, "tenant-b"));

            // Cancelada (não conta em MRR nem em churn Expirada/Recusada)
            var cancelada = NovaAssinatura(clienteA, planoMensal, AssinaturaStatus.Ativa, "tenant-a");
            cancelada.RegistrarCancelamento("saiu", "admin-root");
            gestao.AssinaturasClientes.Add(cancelada);

            // Expirada recente -> alimenta churn (1 cancelamento sobre base 3 ativas => 25%)
            var expirada = NovaAssinatura(clienteA, planoMensal, AssinaturaStatus.Ativa, "tenant-a");
            expirada.Expirar("admin-root");
            gestao.AssinaturasClientes.Add(expirada);

            await gestao.SaveChangesAsync();

            var handler = new ObterDashboardGlobalQueryHandler(app, gestao, tenant);
            var dto = await handler.Handle(new ObterDashboardGlobalQuery(), CancellationToken.None);

            Assert.Equal(3, dto.TotalAssinaturasAtivas);
            Assert.Equal(200m, dto.ReceitaEstimadaMRR);   // 100 + 100 + 0
            Assert.Equal(100m, dto.Arpu);                 // 200 / 2 clientes ativos
            Assert.Equal(25.00m, dto.ChurnRate);          // 1 / (3+1) * 100
            Assert.Equal(400m, dto.Ltv);                  // ARPU / (churn/100) = 100 / 0.25
        }

        // ===== 2) Conversão trial -> pago =====

        [Fact]
        public async Task Dashboard_Conversao_Trial_Deve_Ser_Convertidos_Sobre_Total_De_Trials()
        {
            var db = "g_trial_" + Guid.NewGuid();
            using var gestao = NovoContextoGestao(db, "system");
            using var app = NovoContextoAplicativo(db + "_app", "system");
            var tenant = new TestTenantProvider("system");

            var plano = AddPlano(gestao, 100m, PlanoDuration.Mensal);
            var cliente = AddCliente(gestao, "tenant-a", plano);

            // 2 assinaturas que TIVERAM trial (TrialAte != null); 1 convertida (TrialConvertidoEm != null).
            var t1 = NovaAssinatura(cliente, plano, AssinaturaStatus.AguardandoAprovacao, "tenant-a", DateTime.UtcNow.AddDays(7));
            var t2 = NovaAssinatura(cliente, plano, AssinaturaStatus.AguardandoAprovacao, "tenant-a", DateTime.UtcNow.AddDays(7));
            t2.MarcarTrialConvertido("admin-root");
            gestao.AssinaturasClientes.Add(t1);
            gestao.AssinaturasClientes.Add(t2);

            await gestao.SaveChangesAsync();

            var handler = new ObterDashboardGlobalQueryHandler(app, gestao, tenant);
            var dto = await handler.Handle(new ObterDashboardGlobalQuery(), CancellationToken.None);

            Assert.Equal(50.00m, dto.ConversaoTrialParaPago); // 1 de 2
        }

        // ===== 3) Inadimplência agregada =====

        [Fact]
        public async Task Dashboard_Inadimplencia_Agregada_Deve_Somar_Faturas_Vencidas_Nao_Pagas()
        {
            var db = "g_inad_" + Guid.NewGuid();
            using var gestao = NovoContextoGestao(db, "system");
            using var app = NovoContextoAplicativo(db + "_app", "system");
            var tenant = new TestTenantProvider("system");

            var plano = AddPlano(gestao, 100m, PlanoDuration.Mensal);
            var cliente = AddCliente(gestao, "tenant-a", plano);

            var vencida1 = new Fatura(cliente, 100m, DateTime.UtcNow.AddDays(-10), "tenant-a", "admin-root"); // Pendente
            var vencida2 = new Fatura(cliente, 50m, DateTime.UtcNow.AddDays(-5), "tenant-a", "admin-root");
            vencida2.MarcarAtrasada("admin-root");
            var paga = new Fatura(cliente, 999m, DateTime.UtcNow.AddDays(-3), "tenant-a", "admin-root");
            paga.Baixar("admin-root"); // excluída
            var futura = new Fatura(cliente, 777m, DateTime.UtcNow.AddDays(10), "tenant-a", "admin-root"); // não vencida

            gestao.Faturas.AddRange(vencida1, vencida2, paga, futura);
            await gestao.SaveChangesAsync();

            var handler = new ObterDashboardGlobalQueryHandler(app, gestao, tenant);
            var dto = await handler.Handle(new ObterDashboardGlobalQuery(), CancellationToken.None);

            Assert.Equal(2, dto.InadimplenciaQtdFaturas);
            Assert.Equal(150m, dto.InadimplenciaValorTotal);
        }

        // ===== 4) Relatório de receita por período (caixa) =====

        [Fact]
        public async Task Relatorio_Receita_Por_Periodo_Soma_Faturas_Pagas_No_Intervalo_Bruto_Tarifa_Liquido()
        {
            var db = "g_receita_" + Guid.NewGuid();
            using var gestao = NovoContextoGestao(db, "system");
            var tenant = new TestTenantProvider("system");

            var plano = AddPlano(gestao, 100m, PlanoDuration.Mensal);
            var cliente = AddCliente(gestao, "tenant-a", plano);

            var jan = new DateTime(2026, 1, 10, 0, 0, 0, DateTimeKind.Utc);
            var jan2 = new DateTime(2026, 1, 20, 0, 0, 0, DateTimeKind.Utc);
            var fev = new DateTime(2026, 2, 15, 0, 0, 0, DateTimeKind.Utc);
            var mar = new DateTime(2026, 3, 5, 0, 0, 0, DateTimeKind.Utc);

            var f1 = new Fatura(cliente, 100m, jan, "tenant-a", "admin-root"); f1.BaixarManual(jan, "admin-root");
            var f2 = new Fatura(cliente, 200m, jan2, "tenant-a", "admin-root"); f2.BaixarManual(jan2, "admin-root");
            var f3 = new Fatura(cliente, 300m, fev, "tenant-a", "admin-root"); f3.BaixarManual(fev, "admin-root");
            var f4 = new Fatura(cliente, 999m, mar, "tenant-a", "admin-root"); f4.BaixarManual(mar, "admin-root"); // fora do intervalo
            gestao.Faturas.AddRange(f1, f2, f3, f4);

            // Tarifas dos pagamentos liquidados (5 + 10 em janeiro).
            gestao.PagamentosFaturas.Add(new PagamentoFatura(f1.Id, "Pix", PagamentoFaturaStatus.Paid, 100m, 5m, null, false, jan, "tenant-a", "admin-root"));
            gestao.PagamentosFaturas.Add(new PagamentoFatura(f2.Id, "Pix", PagamentoFaturaStatus.Paid, 200m, 10m, null, false, jan2, "tenant-a", "admin-root"));

            await gestao.SaveChangesAsync();

            var handler = new ObterRelatorioReceitaPorPeriodoQueryHandler(gestao, tenant);
            var dto = await handler.Handle(new ObterRelatorioReceitaPorPeriodoQuery(
                new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                new DateTime(2026, 2, 28, 23, 59, 59, DateTimeKind.Utc)), CancellationToken.None);

            Assert.Equal(3, dto.TotalFaturas);       // f4 (março) fora
            Assert.Equal(600m, dto.TotalBruto);      // 100 + 200 + 300
            Assert.Equal(15m, dto.TotalTarifa);      // 5 + 10
            Assert.Equal(585m, dto.TotalLiquido);
            Assert.Equal(2, dto.Meses.Count);

            var janeiro = dto.Meses.Single(m => m.Mes == 1);
            Assert.Equal(300m, janeiro.Bruto);
            Assert.Equal(15m, janeiro.Tarifa);
            Assert.Equal(285m, janeiro.Liquido);
            Assert.Equal(2, janeiro.QuantidadeFaturas);

            var fevereiro = dto.Meses.Single(m => m.Mes == 2);
            Assert.Equal(300m, fevereiro.Bruto);
            Assert.Equal(0m, fevereiro.Tarifa);
            Assert.Equal(300m, fevereiro.Liquido);
        }

        // ===== 5) Relatório de comissão por período (caixa) =====

        [Fact]
        public async Task Relatorio_Comissao_Por_Periodo_Soma_Campos_Por_Mes_E_Revenda()
        {
            var db = "g_comissao_" + Guid.NewGuid();
            using var gestao = NovoContextoGestao(db, "system");
            var tenant = new TestTenantProvider("system");

            var plano = AddPlano(gestao, 100m, PlanoDuration.Mensal);
            var revenda1 = Guid.NewGuid();
            var clienteA = AddCliente(gestao, "tenant-a", plano, revenda1);
            var clienteB = AddCliente(gestao, "tenant-b", plano, revenda1);
            var clienteC = AddCliente(gestao, "tenant-c", plano, null);

            var jan = new DateTime(2026, 1, 10, 0, 0, 0, DateTimeKind.Utc);
            var fev = new DateTime(2026, 2, 10, 0, 0, 0, DateTimeKind.Utc);

            var fa = new Fatura(clienteA, 100m, jan, "tenant-a", "admin-root"); fa.CalcularSplitComissao(10m, 5m); fa.BaixarManual(jan, "admin-root"); // 10 / 5
            var fb = new Fatura(clienteB, 200m, jan, "tenant-b", "admin-root"); fb.CalcularSplitComissao(10m, 5m); fb.BaixarManual(jan, "admin-root"); // 20 / 10
            var fc = new Fatura(clienteC, 100m, fev, "tenant-c", "admin-root"); fc.CalcularSplitComissao(10m, 5m); fc.BaixarManual(fev, "admin-root"); // 10 / 5 (sem revenda)
            gestao.Faturas.AddRange(fa, fb, fc);
            await gestao.SaveChangesAsync();

            var handler = new ObterRelatorioComissaoPorPeriodoQueryHandler(gestao, tenant);
            var dto = await handler.Handle(new ObterRelatorioComissaoPorPeriodoQuery(
                new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                new DateTime(2026, 2, 28, 23, 59, 59, DateTimeKind.Utc)), CancellationToken.None);

            Assert.Equal(3, dto.TotalFaturas);
            Assert.Equal(40m, dto.TotalComissaoRevenda);  // 10 + 20 + 10
            Assert.Equal(20m, dto.TotalComissaoVendedor); // 5 + 10 + 5

            var janR1 = dto.Itens.Single(i => i.Mes == 1 && i.RevendaId == revenda1);
            Assert.Equal(30m, janR1.ComissaoRevenda);
            Assert.Equal(15m, janR1.ComissaoVendedor);
            Assert.Equal(2, janR1.QuantidadeFaturas);

            var fevSemRevenda = dto.Itens.Single(i => i.Mes == 2 && i.RevendaId == null);
            Assert.Equal(10m, fevSemRevenda.ComissaoRevenda);
            Assert.Equal(1, fevSemRevenda.QuantidadeFaturas);
        }

        // ===== 6) Relatório de inadimplência por período =====

        [Fact]
        public async Task Relatorio_Inadimplencia_Por_Periodo_Agrupa_Vencidas_Nao_Pagas_Por_Mes_E_Tenant()
        {
            var db = "g_inad_rel_" + Guid.NewGuid();
            using var gestao = NovoContextoGestao(db, "system");
            var tenant = new TestTenantProvider("system");

            var plano = AddPlano(gestao, 100m, PlanoDuration.Mensal);
            var clienteA = AddCliente(gestao, "tenant-a", plano);
            var clienteB = AddCliente(gestao, "tenant-b", plano);

            var janA1 = new Fatura(clienteA, 100m, new DateTime(2026, 1, 10, 0, 0, 0, DateTimeKind.Utc), "tenant-a", "admin-root");
            var janA2 = new Fatura(clienteA, 50m, new DateTime(2026, 1, 20, 0, 0, 0, DateTimeKind.Utc), "tenant-a", "admin-root");
            janA2.MarcarAtrasada("admin-root");
            var fevB = new Fatura(clienteB, 200m, new DateTime(2026, 2, 5, 0, 0, 0, DateTimeKind.Utc), "tenant-b", "admin-root");
            var pagaJan = new Fatura(clienteA, 999m, new DateTime(2026, 1, 15, 0, 0, 0, DateTimeKind.Utc), "tenant-a", "admin-root");
            pagaJan.Baixar("admin-root"); // excluída (paga)
            var marA = new Fatura(clienteA, 777m, new DateTime(2026, 3, 1, 0, 0, 0, DateTimeKind.Utc), "tenant-a", "admin-root"); // fora do intervalo

            gestao.Faturas.AddRange(janA1, janA2, fevB, pagaJan, marA);
            await gestao.SaveChangesAsync();

            var handler = new ObterRelatorioInadimplenciaPorPeriodoQueryHandler(gestao, tenant);
            var dto = await handler.Handle(new ObterRelatorioInadimplenciaPorPeriodoQuery(
                new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                new DateTime(2026, 2, 28, 23, 59, 59, DateTimeKind.Utc)), CancellationToken.None);

            Assert.Equal(3, dto.TotalFaturas);       // janA1, janA2, fevB
            Assert.Equal(350m, dto.ValorTotal);      // 100 + 50 + 200

            var janTenantA = dto.Itens.Single(i => i.Mes == 1 && i.TenantId == "tenant-a");
            Assert.Equal(150m, janTenantA.ValorTotal);
            Assert.Equal(2, janTenantA.QuantidadeFaturas);

            var fevTenantB = dto.Itens.Single(i => i.Mes == 2 && i.TenantId == "tenant-b");
            Assert.Equal(200m, fevTenantB.ValorTotal);
            Assert.Equal(1, fevTenantB.QuantidadeFaturas);
        }

        // ===== 7) Segurança: relatórios Landlord exigem operador interno (tenant "system") =====

        [Fact]
        public async Task Relatorios_Landlord_Devem_Rejeitar_Tenant_Nao_System()
        {
            var db = "g_seg_" + Guid.NewGuid();
            using var gestao = NovoContextoGestao(db, "tenant-regular");
            var tenantIntruso = new TestTenantProvider("tenant-regular");

            var inicio = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var fim = new DateTime(2026, 12, 31, 23, 59, 59, DateTimeKind.Utc);

            var receita = new ObterRelatorioReceitaPorPeriodoQueryHandler(gestao, tenantIntruso);
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                receita.Handle(new ObterRelatorioReceitaPorPeriodoQuery(inicio, fim), CancellationToken.None));

            var inad = new ObterRelatorioInadimplenciaPorPeriodoQueryHandler(gestao, tenantIntruso);
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                inad.Handle(new ObterRelatorioInadimplenciaPorPeriodoQuery(inicio, fim), CancellationToken.None));

            var comissao = new ObterRelatorioComissaoPorPeriodoQueryHandler(gestao, tenantIntruso);
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                comissao.Handle(new ObterRelatorioComissaoPorPeriodoQuery(inicio, fim), CancellationToken.None));
        }
    }
}
