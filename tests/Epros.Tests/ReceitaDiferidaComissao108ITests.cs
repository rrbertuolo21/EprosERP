using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.GestaoClientes.Application.Services;
using Epros.Modules.GestaoClientes.Domain.Entities;
using Epros.Modules.GestaoClientes.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Epros.Tests
{
    /// <summary>
    /// 1.08I — MECANISMO de reconhecimento de receita por competência (diferimento anual = 12 avos, CPC 47
    /// RN05) + APURAÇÃO de comissão parametrizável (base/momento = PARÂMETRO, valida contador). Fonte de
    /// negócio: Negocio-acumulado/contabil (RN04/05/07) e Negocio-acumulado/financeiro (RN07/08/09).
    /// ⛔ Nenhum número fiscal é fixado: % vem de Revenda/Vendedor, base/momento de ConfiguracaoGlobal;
    /// o teste só valida o MECANISMO e os defaults seguros.
    /// </summary>
    public class ReceitaDiferidaComissao108ITests
    {
        #region Doubles

        private ContextGestaoClientes CreateContext(string db, string tenantId, string userId)
        {
            var options = new DbContextOptionsBuilder<ContextGestaoClientes>().UseInMemoryDatabase(db).Options;
            return new ContextGestaoClientes(options, new TestTenantProvider(tenantId), new TestCurrentUser(userId));
        }

        private class TestTenantProvider : ITenantProvider
        {
            private readonly string _t; public TestTenantProvider(string t) => _t = t; public string GetTenantId() => _t;
        }
        private class TestCurrentUser : ICurrentUser
        {
            private readonly string _u; public TestCurrentUser(string u) => _u = u;
            public string? GetUserId() => _u; public string? GetUserName() => "Operador"; public string? GetUserEmail() => "op@epros.com";
        }

        #endregion

        // ===== 1) DIFERIMENTO — cronograma anual = 12 competências, soma fecha =====

        [Fact]
        public void FaturaAnual_CalculaCronograma_12Competencias_SomaIgualAoValor()
        {
            var faturaId = Guid.NewGuid();
            var clienteId = Guid.NewGuid();
            var inicio = new DateTime(2026, 1, 15, 0, 0, 0, DateTimeKind.Utc);

            var itens = ReconhecimentoReceitaService.CalcularCronograma(
                faturaId, clienteId, 1200m, PlanoDuration.Anual, inicio, "t", "u");

            Assert.Equal(12, itens.Count);
            Assert.All(itens, i => Assert.Equal(100m, i.Valor));            // 1/12 de 1200
            Assert.Equal(1200m, itens.Sum(i => i.Valor));                   // soma fecha (RN05)
            Assert.Equal(new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), itens.First().Competencia);
            Assert.Equal(new DateTime(2026, 12, 1, 0, 0, 0, DateTimeKind.Utc), itens.Last().Competencia);
            Assert.Equal(Enumerable.Range(1, 12), itens.Select(i => i.Sequencia));
            Assert.All(itens, i => Assert.Equal(ReconhecimentoReceitaStatus.Pendente, i.Status));
        }

        [Fact]
        public void FaturaAnual_ValorNaoDivisivel_ResiduoNaUltimaParcela_SomaFecha()
        {
            // 1000 / 12 = 83,3333… → 11×83,33 + última absorve o resíduo; soma = exatamente 1000.
            var itens = ReconhecimentoReceitaService.CalcularCronograma(
                Guid.NewGuid(), Guid.NewGuid(), 1000m, PlanoDuration.Anual,
                new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), "t", "u");

            Assert.Equal(12, itens.Count);
            Assert.Equal(1000m, itens.Sum(i => i.Valor));
            Assert.Equal(83.33m, itens.First().Valor);
            Assert.Equal(83.37m, itens.Last().Valor); // 1000 - 11*83,33
        }

        // ===== 2) ROTINA — reconhece 1/12 por mês; soma reconhecida = valor da fatura =====

        [Fact]
        public async Task RotinaMensal_Reconhece1DozeAvoPorMes_ESomaReconhecidaFecha()
        {
            const string tenant = "t-recon", user = "u1";
            var db = Guid.NewGuid().ToString();
            using var ctx = CreateContext(db, tenant, user);

            var cliente = new Cliente("Cli", "00.000.000/0001-00", "c@epros.com", Guid.NewGuid(), tenant, user);
            var fatura = new Fatura(cliente.Id, 1200m, DateTime.UtcNow.AddDays(5), tenant, user);
            ctx.Clientes.Add(cliente); ctx.Faturas.Add(fatura);
            await ctx.SaveChangesAsync();

            var svc = new ReconhecimentoReceitaService(ctx);
            var inicio = new DateTime(2026, 1, 10, 0, 0, 0, DateTimeKind.Utc);
            await svc.GerarCronogramaAsync(fatura, PlanoDuration.Anual, inicio, user, CancellationToken.None);
            await ctx.SaveChangesAsync();

            Assert.Equal(12, await ctx.ReconhecimentosReceita.IgnoreQueryFilters().CountAsync());

            // Roda a rotina mês a mês: cada execução reconhece exatamente 1 parcela (1/12).
            for (int i = 0; i < 12; i++)
            {
                var reconhecidas = await svc.ReconhecerCompetenciaAsync(inicio.AddMonths(i), user, CancellationToken.None);
                await ctx.SaveChangesAsync();
                Assert.Equal(1, reconhecidas);
            }

            var todas = await ctx.ReconhecimentosReceita.IgnoreQueryFilters().ToListAsync();
            Assert.All(todas, r => Assert.Equal(ReconhecimentoReceitaStatus.Reconhecido, r.Status));
            Assert.Equal(1200m, todas.Where(r => r.Status == ReconhecimentoReceitaStatus.Reconhecido).Sum(r => r.Valor));

            // Idempotência: rodar de novo não reconhece nada.
            var extra = await svc.ReconhecerCompetenciaAsync(inicio.AddMonths(13), user, CancellationToken.None);
            Assert.Equal(0, extra);
        }

        [Fact]
        public async Task PlanoMensal_ReconheceIntegralNoMes_SemDiferir()
        {
            const string tenant = "t-mensal", user = "u1";
            using var ctx = CreateContext(Guid.NewGuid().ToString(), tenant, user);

            var cliente = new Cliente("Cli", "00.000.000/0001-00", "c@epros.com", Guid.NewGuid(), tenant, user);
            var fatura = new Fatura(cliente.Id, 100m, DateTime.UtcNow.AddDays(5), tenant, user);
            ctx.Clientes.Add(cliente); ctx.Faturas.Add(fatura);
            await ctx.SaveChangesAsync();

            var svc = new ReconhecimentoReceitaService(ctx);
            var mes = new DateTime(2026, 3, 20, 0, 0, 0, DateTimeKind.Utc);
            var itens = await svc.GerarCronogramaAsync(fatura, PlanoDuration.Mensal, mes, user, CancellationToken.None);
            await ctx.SaveChangesAsync();

            Assert.Single(itens);                                   // RN04 — 1 competência, sem diferir
            Assert.Equal(100m, itens[0].Valor);

            var n = await svc.ReconhecerCompetenciaAsync(mes, user, CancellationToken.None);
            await ctx.SaveChangesAsync();
            Assert.Equal(1, n);
            var parcela = await ctx.ReconhecimentosReceita.IgnoreQueryFilters().SingleAsync();
            Assert.Equal(ReconhecimentoReceitaStatus.Reconhecido, parcela.Status);
        }

        // ===== 3) COMISSÃO — base/momento do PARÂMETRO; trocar o parâmetro muda o cálculo =====

        [Fact]
        public async Task Comissao_ApuraComBaseEMomentoDoParametro_TrocaParametroMudaCalculo()
        {
            const string tenant = "t-com", user = "u1";
            using var ctx = CreateContext(Guid.NewGuid().ToString(), tenant, user);

            var revenda = new Revenda("Rev", 10m, tenant, user);           // % = PARÂMETRO da Revenda
            var vendedor = new Vendedor(revenda.Id, "Vend", "v@epros.com", null, 5m, tenant, user);
            ctx.Revendas.Add(revenda); ctx.Vendedores.Add(vendedor);

            var cliente = new Cliente("Cli", "00.000.000/0001-00", "c@epros.com", Guid.NewGuid(),
                revenda.Id, vendedor.Id, 10, StatusSaaS.Ativo, tenant, user);
            ctx.Clientes.Add(cliente);

            var faturaA = new Fatura(cliente.Id, 1000m, DateTime.UtcNow.AddDays(5), tenant, user);
            var faturaB = new Fatura(cliente.Id, 1000m, DateTime.UtcNow.AddDays(5), tenant, user);
            ctx.Faturas.Add(faturaA); ctx.Faturas.Add(faturaB);
            await ctx.SaveChangesAsync();

            var svc = new ApuracaoComissaoService(ctx);

            // (A) Sem configuração → DEFAULT SEGURO Bruto/Caixa (base = 1000).
            var apA = await svc.ApurarAsync(faturaA, valorLiquido: 900m, criadoPor: user, cancellationToken: CancellationToken.None);
            await ctx.SaveChangesAsync();
            Assert.NotNull(apA);
            Assert.Equal(BaseComissao.Bruto, apA!.Base);
            Assert.Equal(MomentoComissao.Caixa, apA.Momento);
            Assert.Equal(1000m, apA.ValorBase);
            Assert.Equal(100m, apA.ValorComissaoRevenda);   // 10% de 1000
            Assert.Equal(50m, apA.ValorComissaoVendedor);   // 5% de 1000
            Assert.Null(apA.Competencia);                   // caixa não fixa competência

            // Troca o PARÂMETRO: base = Líquido, momento = Competência.
            ctx.ConfiguracoesGlobais.Add(new ConfiguracaoGlobal(
                ApuracaoComissaoService.ChaveBaseComissao, "Liquido", false, "base comissao", tenant, user));
            ctx.ConfiguracoesGlobais.Add(new ConfiguracaoGlobal(
                ApuracaoComissaoService.ChaveMomentoComissao, "Competencia", false, "momento comissao", tenant, user));
            await ctx.SaveChangesAsync();

            // (B) Mesmo cenário, parâmetro trocado → base = líquido (900) e momento competência.
            var apB = await svc.ApurarAsync(faturaB, valorLiquido: 900m, criadoPor: user, cancellationToken: CancellationToken.None,
                competencia: new DateTime(2026, 5, 9, 0, 0, 0, DateTimeKind.Utc));
            await ctx.SaveChangesAsync();
            Assert.NotNull(apB);
            Assert.Equal(BaseComissao.Liquido, apB!.Base);
            Assert.Equal(MomentoComissao.Competencia, apB.Momento);
            Assert.Equal(900m, apB.ValorBase);
            Assert.Equal(90m, apB.ValorComissaoRevenda);    // 10% de 900 (trocou o parâmetro → mudou o cálculo)
            Assert.Equal(45m, apB.ValorComissaoVendedor);   // 5% de 900
            Assert.Equal(new DateTime(2026, 5, 1, 0, 0, 0, DateTimeKind.Utc), apB.Competencia);
        }

        // ===== 4) CLAWBACK — cancelamento aciona o gancho de estorno da apuração =====

        [Fact]
        public async Task Cancelamento_AcionaGanchoClawback_EstornaApuracao()
        {
            const string tenant = "t-claw", user = "u1";
            using var ctx = CreateContext(Guid.NewGuid().ToString(), tenant, user);

            var revenda = new Revenda("Rev", 10m, tenant, user);
            var cliente = new Cliente("Cli", "00.000.000/0001-00", "c@epros.com", Guid.NewGuid(),
                revenda.Id, null, 10, StatusSaaS.Ativo, tenant, user);
            var fatura = new Fatura(cliente.Id, 1000m, DateTime.UtcNow.AddDays(5), tenant, user);
            ctx.Revendas.Add(revenda); ctx.Clientes.Add(cliente); ctx.Faturas.Add(fatura);
            await ctx.SaveChangesAsync();

            var svc = new ApuracaoComissaoService(ctx);
            await svc.ApurarAsync(fatura, null, user, CancellationToken.None);
            await ctx.SaveChangesAsync();

            // Cancelamento da fatura aciona o gancho de clawback (RN09 — política = valida contador).
            fatura.Cancelar(user);
            var estornadas = await svc.AcionarClawbackAsync(fatura.Id, "cancelamento", user, CancellationToken.None);
            await ctx.SaveChangesAsync();

            Assert.Equal(1, estornadas);
            var ap = await ctx.ApuracoesComissao.IgnoreQueryFilters().SingleAsync(a => a.FaturaId == fatura.Id);
            Assert.True(ap.Estornada);
            Assert.NotNull(ap.EstornadaEm);
            Assert.Equal("cancelamento", ap.MotivoEstorno);

            // Idempotência: segunda chamada não estorna de novo.
            var denovo = await svc.AcionarClawbackAsync(fatura.Id, "cancelamento", user, CancellationToken.None);
            Assert.Equal(0, denovo);
        }

        // ===== 5) INTEGRAÇÃO — pagamento real de fatura anual gera cronograma + apuração =====

        [Fact]
        public async Task LiquidarFaturaAnual_GeraCronograma12_ERegistraApuracao()
        {
            const string tenant = "t-liq", user = "u1";
            using var ctx = CreateContext(Guid.NewGuid().ToString(), tenant, user);

            var plano = new Plano("Anual", 1200m, null, 10, 2, null, tenant, user, duration: PlanoDuration.Anual);
            ctx.Planos.Add(plano);
            var revenda = new Revenda("Rev", 10m, tenant, user);
            ctx.Revendas.Add(revenda);
            var cliente = new Cliente("Cli", "00.000.000/0001-00", "c@epros.com", plano.Id,
                revenda.Id, null, 10, StatusSaaS.Ativo, tenant, user);
            ctx.Clientes.Add(cliente);
            var assinatura = new AssinaturaCliente(cliente.Id, plano.Id, AssinaturaStatus.AguardandoAprovacao,
                DateTime.UtcNow, DateTime.UtcNow.AddDays(30), null, "PIX", null, null, tenant, user);
            ctx.AssinaturasClientes.Add(assinatura);
            var fatura = new Fatura(cliente.Id, 1200m, DateTime.UtcNow.AddDays(5), tenant, user);
            ctx.Faturas.Add(fatura);
            await ctx.SaveChangesAsync();

            var aprovacao = new DateTime(2026, 2, 10, 0, 0, 0, DateTimeKind.Utc);
            var recibo = await new FaturaLiquidacaoService(ctx).LiquidarAsync(
                fatura, "pix-1", "PIX", "MercadoPago", 1200m, null, null, aprovacao, user, CancellationToken.None);
            await ctx.SaveChangesAsync();

            Assert.NotNull(recibo);

            // Diferimento anual: 12 competências consecutivas a partir da aprovação, soma = 1200.
            var cronograma = await ctx.ReconhecimentosReceita.IgnoreQueryFilters()
                .Where(r => r.FaturaId == fatura.Id).OrderBy(r => r.Sequencia).ToListAsync();
            Assert.Equal(12, cronograma.Count);
            Assert.Equal(1200m, cronograma.Sum(r => r.Valor));
            Assert.Equal(new DateTime(2026, 2, 1, 0, 0, 0, DateTimeKind.Utc), cronograma.First().Competencia);

            // Apuração de comissão registrada (default Bruto/Caixa; % = da revenda).
            var ap = await ctx.ApuracoesComissao.IgnoreQueryFilters().SingleAsync(a => a.FaturaId == fatura.Id);
            Assert.Equal(BaseComissao.Bruto, ap.Base);
            Assert.Equal(1200m, ap.ValorBase);
            Assert.Equal(120m, ap.ValorComissaoRevenda); // 10% de 1200

            // Idempotência do caminho: liquidar de novo não duplica cronograma nem apuração.
            var reciboDup = await new FaturaLiquidacaoService(ctx).LiquidarAsync(
                fatura, "pix-1", "PIX", "MercadoPago", 1200m, null, null, aprovacao, user, CancellationToken.None);
            Assert.Null(reciboDup); // fatura já paga
            Assert.Equal(12, await ctx.ReconhecimentosReceita.IgnoreQueryFilters().CountAsync(r => r.FaturaId == fatura.Id));
            Assert.Equal(1, await ctx.ApuracoesComissao.IgnoreQueryFilters().CountAsync(a => a.FaturaId == fatura.Id));
        }
    }
}
