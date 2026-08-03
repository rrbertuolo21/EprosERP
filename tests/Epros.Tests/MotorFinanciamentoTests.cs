using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.DMS.Application.Commands;
using Epros.Modules.DMS.Application.Handlers;
using Epros.Modules.DMS.Domain.Financiamento;
using Epros.Modules.DMS.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Domain.Events;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Epros.Tests
{
    /// <summary>
    /// CON-FIN / NF-01 — Motor de cálculo F&amp;I (Price/SAC + IOF + CET).
    /// Os números conferidos batem com os EXEMPLOS TRABALHADOS da skill AGNÓSTICA
    /// <c>Negocio-acumulado/financeiro/credito</c> (Receitas A, B, C, D).
    /// </summary>
    public class MotorFinanciamentoTests
    {
        // ---------- Receita A — Price (exemplo da skill: PV=10.000, i=2% a.m., n=12) ----------

        [Fact(DisplayName = "F&I | Price | PMT = 945,60 (exemplo da skill)")]
        public void Price_Prestacao_Constante_Bate_Exemplo_Skill()
        {
            var r = MotorFinanciamento.Simular(10000m, 0.02m, 12, SistemaAmortizacao.Price);

            // PMT = 10.000 · 0,02 / (1 − 1,02^-12) = 945,60
            Assert.Equal(945.60m, r.PrimeiraPrestacao);
            // Na Price todas as prestações (fora seguro) são iguais, exceto ajuste da última.
            Assert.All(r.Parcelas.Take(11), p => Assert.Equal(945.60m, p.Prestacao));
        }

        [Fact(DisplayName = "F&I | Price | Total de juros ≈ 1.347,20 e saldo final zera")]
        public void Price_Total_Juros_E_Saldo_Zero()
        {
            var r = MotorFinanciamento.Simular(10000m, 0.02m, 12, SistemaAmortizacao.Price);

            Assert.InRange(r.TotalJuros, 1346.50m, 1348.00m); // skill: 1.347,20
            Assert.Equal(0m, r.Parcelas.Last().SaldoDevedor); // saldo fecha em zero
            // Primeira parcela: juros = 200,00; amortização = 745,60 (skill)
            Assert.Equal(200.00m, r.Parcelas[0].Juros);
            Assert.Equal(745.60m, r.Parcelas[0].Amortizacao);
        }

        // ---------- Receita B — SAC (mesmo PV/i/n) ----------

        [Fact(DisplayName = "F&I | SAC | Amortização constante 833,33 e juros totais = 1.300,00 (skill)")]
        public void Sac_Amortizacao_Constante_E_Juros_Totais()
        {
            var r = MotorFinanciamento.Simular(10000m, 0.02m, 12, SistemaAmortizacao.Sac);

            // amortização = 10.000/12 = 833,33 em todas (última ajusta o resíduo)
            Assert.Equal(833.33m, r.Parcelas[0].Amortizacao);
            // Conferência da skill: juros_totais_SAC = i·PV·(n+1)/2 = 0,02·10.000·13/2 = 1.300,00
            Assert.InRange(r.TotalJuros, 1299.50m, 1300.50m);
            Assert.Equal(0m, r.Parcelas.Last().SaldoDevedor);
            // Primeira prestação SAC (1.033,33) > primeira Price (945,60)
            Assert.Equal(1033.33m, r.Parcelas[0].Prestacao);
        }

        [Fact(DisplayName = "F&I | SAC paga menos juros que Price (identidade aritmética da skill)")]
        public void Sac_Paga_Menos_Juros_Que_Price()
        {
            var price = MotorFinanciamento.Simular(10000m, 0.02m, 12, SistemaAmortizacao.Price);
            var sac = MotorFinanciamento.Simular(10000m, 0.02m, 12, SistemaAmortizacao.Sac);

            Assert.True(sac.TotalJuros < price.TotalJuros,
                $"SAC {sac.TotalJuros} deveria ser < Price {price.TotalJuros}");
        }

        // ---------- Princípio 1 — conversão de taxa composta ----------

        [Fact(DisplayName = "F&I | Taxa anual→mensal é composta, não linear (a/12)")]
        public void Conversao_Taxa_Anual_Para_Mensal_Composta()
        {
            // (1+0,26824)^(1/12)-1 ≈ 0,02 ; linear seria 0,26824/12 = 0,02235 (errado)
            var mensal = MotorFinanciamento.TaxaAnualParaMensal(0.268241m);
            Assert.InRange(mensal, 0.0199m, 0.0201m);
        }

        // ---------- Receita C — IOF ----------

        [Fact(DisplayName = "F&I | IOF = diário (teto 365d) + adicional; > 0 para PF")]
        public void Iof_Estrutura_Diario_Mais_Adicional()
        {
            // 12 meses ≈ 360 dias. PF: 0,0082%/dia · 360 + 0,38% adicional, sobre 10.000.
            var iof = TabelaIof.Calcular(10000m, 360, TipoMutuario.PessoaFisica, new DateTime(2024, 1, 1));
            // adicional sozinho = 38,00; diário ≈ 0,000082·360·10000 = 295,20 → ~333,20
            Assert.InRange(iof, 300m, 360m);

            // PJ tem alíquota diária menor → IOF menor que PF no mesmo cenário
            var iofPj = TabelaIof.Calcular(10000m, 360, TipoMutuario.PessoaJuridica, new DateTime(2024, 1, 1));
            Assert.True(iofPj < iof);
        }

        [Fact(DisplayName = "F&I | IOF diário tem teto de 365 dias")]
        public void Iof_Diario_Tem_Teto_365()
        {
            var iof720 = TabelaIof.Calcular(10000m, 720, TipoMutuario.PessoaFisica, new DateTime(2024, 1, 1));
            var iof365 = TabelaIof.Calcular(10000m, 365, TipoMutuario.PessoaFisica, new DateTime(2024, 1, 1));
            Assert.Equal(iof365, iof720); // além de 365 dias, o diário não cresce
        }

        // ---------- Receita D — CET ----------

        [Fact(DisplayName = "F&I | CET ≥ taxa de juros quando há IOF/tarifas (sanidade da skill)")]
        public void Cet_Maior_Ou_Igual_Juros_Com_Custos()
        {
            var custos = new CustosOperacao(TarifasDescontadas: 500m, SeguroPorParcela: 20m, IofFinanciado: false);
            var r = MotorFinanciamento.Simular(10000m, 0.02m, 12, SistemaAmortizacao.Price, custos);

            Assert.True(r.CetAnual >= r.TaxaJurosAnual,
                $"CET {r.CetAnual} deveria ser ≥ juros {r.TaxaJurosAnual}");
            Assert.True(r.CetAnual > r.TaxaJurosAnual, "com custos além do juro, CET deve ser estritamente maior");
        }

        [Fact(DisplayName = "F&I | Sem custo extra, CET ≈ taxa de juros anual")]
        public void Cet_Igual_Juros_Sem_Custos()
        {
            var r = MotorFinanciamento.Simular(10000m, 0.02m, 12, SistemaAmortizacao.Price);
            // 2% a.m. → (1,02)^12 - 1 ≈ 26,82% a.a.
            Assert.InRange(r.TaxaJurosAnual, 0.2680m, 0.2685m);
            Assert.InRange(r.CetAnual, r.TaxaJurosAnual - 0.001m, r.TaxaJurosAnual + 0.02m);
        }

        // ---------- Handler (persistência + idempotência + evento) ----------

        private static ContextDMS NovoContexto(string db) =>
            new ContextDMS(
                new DbContextOptionsBuilder<ContextDMS>().UseInMemoryDatabase(db).Options,
                new FakeTenant("tenant-1"), new FakeUser("user-1"));

        [Fact(DisplayName = "F&I | Handler calcula, persiste resultado e emite evento")]
        public async Task Handler_Calcula_Persiste_E_Emite_Evento()
        {
            using var ctx = NovoContexto("db_fin_calc");
            var handler = new SimularFinanciamentoCommandHandler(ctx, new FakeTenant("tenant-1"), new FakeUser("user-1"));
            var cmd = new SimularFinanciamentoCommand(
                JornadaId: Guid.NewGuid(), ChaveIdempotencia: "sim-1",
                PrecoVeiculo: 60000m, Entrada: 10000m, PrazoQuantidade: 48,
                TaxaJurosMensal: 0.015m, Sistema: "Price");

            var result = await handler.Handle(cmd, CancellationToken.None);

            Assert.True(result.Sucesso);
            var sim = await ctx.SimulacoesFin.SingleAsync();
            Assert.True(sim.Calculada);
            Assert.Equal("Price", sim.Sistema);
            Assert.Equal(50000m, sim.Saldo);
            Assert.NotNull(sim.ValorParcela);
            Assert.True(sim.ValorParcela > 0);
            Assert.True(sim.CetAnual >= 0);
            Assert.NotNull(sim.MemoriaJson);

            var evt = await ctx.OutboxMessages.SingleAsync();
            Assert.Equal(CatalogoEventosIntegracao.Concessionarias.FinSimulacaoCalculada, evt.EventType);
            Assert.True(CatalogoEventosIntegracao.EhEventoConhecido(evt.EventType));
        }

        [Fact(DisplayName = "F&I | Handler é idempotente por chave")]
        public async Task Handler_Idempotente_Por_Chave()
        {
            using var ctx = NovoContexto("db_fin_idem");
            var handler = new SimularFinanciamentoCommandHandler(ctx, new FakeTenant("tenant-1"), new FakeUser("user-1"));
            var jornada = Guid.NewGuid();
            var cmd = new SimularFinanciamentoCommand(jornada, "sim-idem", 60000m, 10000m, 48, 0.015m, "Sac");

            await handler.Handle(cmd, CancellationToken.None);
            await handler.Handle(cmd, CancellationToken.None); // segunda vez

            Assert.Equal(1, await ctx.SimulacoesFin.CountAsync());
        }

        [Fact(DisplayName = "F&I | Handler rejeita sistema inválido e entrada >= preço")]
        public async Task Handler_Rejeita_Invalidos()
        {
            using var ctx = NovoContexto("db_fin_inval");
            var handler = new SimularFinanciamentoCommandHandler(ctx, new FakeTenant("tenant-1"), new FakeUser("user-1"));

            var sistemaRuim = await handler.Handle(
                new SimularFinanciamentoCommand(Guid.NewGuid(), "s1", 60000m, 10000m, 48, 0.015m, "Xpto"),
                CancellationToken.None);
            Assert.False(sistemaRuim.Sucesso);

            var semSaldo = await handler.Handle(
                new SimularFinanciamentoCommand(Guid.NewGuid(), "s2", 60000m, 60000m, 48, 0.015m, "Price"),
                CancellationToken.None);
            Assert.False(semSaldo.Sucesso);
        }

        private sealed class FakeTenant : ITenantProvider
        {
            private readonly string _t;
            public FakeTenant(string t) => _t = t;
            public string GetTenantId() => _t;
        }

        private sealed class FakeUser : ICurrentUser
        {
            private readonly string _u;
            public FakeUser(string u) => _u = u;
            public string? GetUserId() => _u;
            public string? GetUserName() => "test";
            public string? GetUserEmail() => "t@epros.com";
        }
    }
}
