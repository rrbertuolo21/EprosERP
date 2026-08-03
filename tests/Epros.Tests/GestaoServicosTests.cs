using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Vendas.Application.Commands;
using Epros.Modules.Vendas.Application.Handlers;
using Epros.Modules.Vendas.Domain.Entities;
using Epros.Modules.Vendas.Domain.Enums;
using Epros.Modules.Vendas.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Epros.Tests
{
    /// <summary>Testes do submódulo Gestão de Serviços (VEN-GSV). Fonte: EF_7_VENDAS_GESTAO_DE_SERVICOS_V1.</summary>
    public class GestaoServicosTests
    {
        private const string TenantId = "tenant-gsv-001";
        private const string UserId = "user-gsv-001";

        // ---------- Catálogo ----------

        [Fact(DisplayName = "ServicoCatalogo | Sem nome deve ser inválido")]
        public void ServicoCatalogo_SemNome_DeveSerInvalido()
        {
            var s = new ServicoCatalogo("", null, 100m, 0m, false, null, null, null, TenantId, UserId);
            Assert.False(s.IsValid);
        }

        [Fact(DisplayName = "ServicoCatalogo | Valor negativo é normalizado para zero (EF §8.5)")]
        public void ServicoCatalogo_ValorNegativo_ViraZero()
        {
            var s = new ServicoCatalogo("Instalação", null, -10m, -5m, false, null, null, null, TenantId, UserId);
            Assert.True(s.IsValid);
            Assert.Equal(0m, s.Valor);
            Assert.Equal(0m, s.Taxa);
        }

        // ---------- Linha ----------

        [Fact(DisplayName = "ServicoFaturaLinha | Total = qtd x preço - desconto% (EF §10.12)")]
        public void Linha_CalculaTotalComDesconto()
        {
            // 3 x 100 = 300; 10% desconto => 30; total = 270
            var linha = new ServicoFaturaLinha(Guid.NewGuid(), "Serviço A", null, 3m, 100m, 10m, TenantId, UserId);
            Assert.True(linha.IsValid);
            Assert.Equal(270m, linha.Total);
            Assert.Equal(30m, linha.DescontoValor());
        }

        [Fact(DisplayName = "ServicoFaturaLinha | Quantidade zero assume 1 (EF §10.4)")]
        public void Linha_QuantidadeZero_Assume1()
        {
            var linha = new ServicoFaturaLinha(Guid.NewGuid(), "Serviço A", null, 0m, 50m, 0m, TenantId, UserId);
            Assert.Equal(1m, linha.Quantidade);
            Assert.Equal(50m, linha.Total);
        }

        // ---------- Fatura: cálculo ----------

        [Fact(DisplayName = "ServicoFatura | Imposto exclusivo soma ao total líquido (EF §11.5/§11.6)")]
        public void Fatura_ImpostoExclusivo_SomaAoLiquido()
        {
            var fatura = NovaFaturaComLinha(quantidade: 1m, preco: 1000m, descontoLinha: 0m, descontoCabecalho: 0m, custoEnvio: 0m, valorPago: 0m);
            // 10% exclusivo sobre 1000 => imposto 100; líquido 1100
            fatura.Recalcular(10m, ETipoImpostoServico.Exclusivo);

            Assert.Equal(1000m, fatura.TotalGeral);
            Assert.Equal(100m, fatura.ValorImposto);
            Assert.Equal(1100m, fatura.TotalLiquido);
            Assert.Equal(1100m, fatura.SaldoDevido);
            Assert.Equal(0m, fatura.Troco);
        }

        [Fact(DisplayName = "ServicoFatura | Imposto inclusivo não soma novamente ao líquido (EF §11.7/§11.8)")]
        public void Fatura_ImpostoInclusivo_NaoSomaAoLiquido()
        {
            var fatura = NovaFaturaComLinha(quantidade: 1m, preco: 1100m, descontoLinha: 0m, descontoCabecalho: 0m, custoEnvio: 0m, valorPago: 0m);
            // 10% inclusivo sobre 1100 => imposto = 1100 * 10/110 = 100; líquido permanece 1100
            fatura.Recalcular(10m, ETipoImpostoServico.Inclusivo);

            Assert.Equal(1100m, fatura.TotalGeral);
            Assert.Equal(100m, fatura.ValorImposto);
            Assert.Equal(1100m, fatura.TotalLiquido);
        }

        [Fact(DisplayName = "ServicoFatura | Pagamento parcial gera saldo devido (EF §11.14)")]
        public void Fatura_PagamentoParcial_GeraSaldoDevido()
        {
            var fatura = NovaFaturaComLinha(1m, 1000m, 0m, 0m, custoEnvio: 0m, valorPago: 400m);
            fatura.Recalcular(0m, ETipoImpostoServico.Exclusivo);
            Assert.Equal(1000m, fatura.TotalLiquido);
            Assert.Equal(600m, fatura.SaldoDevido);
            Assert.Equal(0m, fatura.Troco);
        }

        [Fact(DisplayName = "ServicoFatura | Pagamento acima do total gera troco (EF §11.15)")]
        public void Fatura_PagamentoExcedente_GeraTroco()
        {
            var fatura = NovaFaturaComLinha(1m, 1000m, 0m, 0m, custoEnvio: 0m, valorPago: 1200m);
            fatura.Recalcular(0m, ETipoImpostoServico.Exclusivo);
            Assert.Equal(0m, fatura.SaldoDevido);
            Assert.Equal(200m, fatura.Troco);
        }

        [Fact(DisplayName = "ServicoFatura | Desconto total soma cabeçalho + linhas (EF §11.2)")]
        public void Fatura_DescontoTotal_SomaCabecalhoELinhas()
        {
            // linha: 2 x 100 = 200, 10% => desconto 20, total 180; cabeçalho 30
            var fatura = NovaFaturaComLinha(2m, 100m, descontoLinha: 10m, descontoCabecalho: 30m, custoEnvio: 0m, valorPago: 0m);
            fatura.Recalcular(0m, ETipoImpostoServico.Exclusivo);
            Assert.Equal(180m, fatura.TotalGeral);
            Assert.Equal(50m, fatura.TotalDesconto); // 20 (linha) + 30 (cabeçalho)
        }

        [Fact(DisplayName = "ServicoFatura | Sem cliente é inválida (EF §9.2)")]
        public void Fatura_SemCliente_Invalida()
        {
            var fatura = new ServicoFatura(Guid.NewGuid(), Guid.Empty, null, null, null, string.Empty, null, 0m, 0m, 0m, null, TenantId, UserId);
            Assert.False(fatura.IsValid);
        }

        [Fact(DisplayName = "ServicoFatura | Valor pago sem conta de pagamento é inválida (EF §9.6)")]
        public void Fatura_ValorPagoSemConta_Invalida()
        {
            var fatura = new ServicoFatura(Guid.NewGuid(), Guid.NewGuid(), null, null, null, string.Empty, null, 0m, 0m, 100m, null, TenantId, UserId);
            Assert.False(fatura.IsValid);
        }

        [Fact(DisplayName = "ServicoFatura | Confirmar sem linha falha; com linha confirma (EF §21)")]
        public void Fatura_Confirmar_ExigeLinha()
        {
            var semLinha = new ServicoFatura(Guid.NewGuid(), Guid.NewGuid(), null, null, null, string.Empty, null, 0m, 0m, 0m, null, TenantId, UserId);
            semLinha.Confirmar(UserId);
            Assert.False(semLinha.IsValid);

            var comLinha = NovaFaturaComLinha(1m, 100m, 0m, 0m, 0m, 0m);
            comLinha.Confirmar(UserId);
            Assert.True(comLinha.IsValid);
            Assert.Equal(EServicoFaturaStatus.Confirmado, comLinha.Status);
        }

        // ---------- Fatura: fluxo de faturamento (handler) ----------

        [Fact(DisplayName = "Faturar fatura de serviço gera referências financeiras (EF §12)")]
        public async Task Faturar_GeraLancamentosFinanceiros()
        {
            var context = CreateContext(nameof(Faturar_GeraLancamentosFinanceiros));
            var tp = new TestTenantProvider(TenantId);
            var cu = new TestCurrentUser(UserId);

            var criar = new CriarServicoFaturaCommandHandler(context, tp, cu);
            var empresaId = Guid.NewGuid();
            var clienteId = Guid.NewGuid();
            var contaId = Guid.NewGuid();
            var servicoId = Guid.NewGuid();

            var criarResult = await criar.Handle(new CriarServicoFaturaCommand(
                empresaId, clienteId, null, contaId, null, null, 0m, 0m, 500m,
                "Serviço avulso",
                new List<ServicoFaturaLinhaInput> { new(servicoId, "Serviço A", null, 1m, 1000m, 0m) }),
                CancellationToken.None);
            Assert.True(criarResult.Sucesso);

            var fatura = await context.ServicoFaturas.FirstAsync();
            await new ConfirmarServicoFaturaCommandHandler(context, tp, cu).Handle(new ConfirmarServicoFaturaCommand(fatura.Id), CancellationToken.None);
            var fatResult = await new FaturarServicoFaturaCommandHandler(context, tp, cu).Handle(new FaturarServicoFaturaCommand(fatura.Id), CancellationToken.None);

            Assert.True(fatResult.Sucesso);
            var refs = await context.ServicoLancamentoFinanceiroRefs.Where(r => r.FaturaId == fatura.Id).ToListAsync();
            // Cliente débito + receita crédito + (conta débito + cliente crédito) porque houve valor pago.
            Assert.Equal(4, refs.Count);
            Assert.Contains(refs, r => r.TipoLancamento == ETipoLancamentoServicoFinanceiro.ClienteDebito);
            Assert.Contains(refs, r => r.TipoLancamento == ETipoLancamentoServicoFinanceiro.ReceitaCredito);
            Assert.Contains(refs, r => r.TipoLancamento == ETipoLancamentoServicoFinanceiro.ContaPagamentoDebito);
        }

        [Fact(DisplayName = "Estornar fatura faturada estorna referências financeiras; Cancelar a rejeita (EC-08/EF §13.5)")]
        public async Task Estornar_FaturaFaturada_EstornaReferencias()
        {
            var context = CreateContext(nameof(Estornar_FaturaFaturada_EstornaReferencias));
            var tp = new TestTenantProvider(TenantId);
            var cu = new TestCurrentUser(UserId);

            var servicoId = Guid.NewGuid();
            await new CriarServicoFaturaCommandHandler(context, tp, cu).Handle(new CriarServicoFaturaCommand(
                Guid.NewGuid(), Guid.NewGuid(), null, Guid.NewGuid(), null, null, 0m, 0m, 1000m, null,
                new List<ServicoFaturaLinhaInput> { new(servicoId, "Serviço A", null, 1m, 1000m, 0m) }),
                CancellationToken.None);

            var fatura = await context.ServicoFaturas.FirstAsync();
            await new ConfirmarServicoFaturaCommandHandler(context, tp, cu).Handle(new ConfirmarServicoFaturaCommand(fatura.Id), CancellationToken.None);
            await new FaturarServicoFaturaCommandHandler(context, tp, cu).Handle(new FaturarServicoFaturaCommand(fatura.Id), CancellationToken.None);

            // EC-08: Cancelar não vale para faturada.
            var cancelada = await new CancelarServicoFaturaCommandHandler(context, tp, cu).Handle(new CancelarServicoFaturaCommand(fatura.Id), CancellationToken.None);
            Assert.False(cancelada.Sucesso);

            // EC-08: Estornar é a transição correta e reverte os lançamentos.
            await new EstornarServicoFaturaCommandHandler(context, tp, cu).Handle(new EstornarServicoFaturaCommand(fatura.Id), CancellationToken.None);

            var faturaEstornada = await context.ServicoFaturas.FirstAsync(f => f.Id == fatura.Id);
            Assert.Equal(EServicoFaturaStatus.Estornado, faturaEstornada.Status);
            var refs = await context.ServicoLancamentoFinanceiroRefs.Where(r => r.FaturaId == fatura.Id).ToListAsync();
            Assert.All(refs, r => Assert.Equal(EStatusIntegracaoServico.Estornado, r.StatusIntegracao));
        }

        [Fact(DisplayName = "ISS | Alíquota entra como parâmetro (valida-contador); sem parâmetro = stub 0% (NF-03/INV-02)")]
        public async Task Iss_Parametro_AplicaAliquota_SemParametro_Zero()
        {
            var context = CreateContext(nameof(Iss_Parametro_AplicaAliquota_SemParametro_Zero));
            var tp = new TestTenantProvider(TenantId);
            var cu = new TestCurrentUser(UserId);

            // Sem parâmetro de ISS → 0% (stub, não inventa alíquota).
            await new CriarServicoFaturaCommandHandler(context, tp, cu).Handle(new CriarServicoFaturaCommand(
                Guid.NewGuid(), Guid.NewGuid(), null, Guid.NewGuid(), null, null, 0m, 0m, 0m, null,
                new List<ServicoFaturaLinhaInput> { new(Guid.NewGuid(), "Serviço A", null, 1m, 1000m, 0m) }),
                CancellationToken.None);
            var semIss = await context.ServicoFaturas.FirstAsync();
            Assert.Equal(0m, semIss.ValorImposto);

            // Com parâmetro de ISS (ex.: 5% por município/atividade) → aplica.
            await new CriarServicoFaturaCommandHandler(context, tp, cu).Handle(new CriarServicoFaturaCommand(
                Guid.NewGuid(), Guid.NewGuid(), null, Guid.NewGuid(), null, null, 0m, 0m, 0m, null,
                new List<ServicoFaturaLinhaInput> { new(Guid.NewGuid(), "Serviço B", null, 1m, 1000m, 0m) },
                AliquotaIssPercentual: 5m, TipoImpostoIss: ETipoImpostoServico.Exclusivo),
                CancellationToken.None);
            var comIss = await context.ServicoFaturas.FirstAsync(f => f.ValorImposto == 50m); // 5% de 1000
            Assert.Equal(50m, comIss.ValorImposto);
        }

        // ---------- Helpers ----------

        private static ServicoFatura NovaFaturaComLinha(decimal quantidade, decimal preco, decimal descontoLinha, decimal descontoCabecalho, decimal custoEnvio, decimal valorPago)
        {
            var contaId = valorPago > 0 ? Guid.NewGuid() : (Guid?)null;
            var fatura = new ServicoFatura(Guid.NewGuid(), Guid.NewGuid(), null, contaId, null, string.Empty, null, descontoCabecalho, custoEnvio, valorPago, null, TenantId, UserId);
            fatura.AdicionarLinha(new ServicoFaturaLinha(Guid.NewGuid(), "Serviço A", null, quantidade, preco, descontoLinha, TenantId, UserId));
            return fatura;
        }

        private static ContextVendas CreateContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<ContextVendas>().UseInMemoryDatabase(dbName).Options;
            return new ContextVendas(options, new TestTenantProvider(TenantId), new TestCurrentUser(UserId));
        }

        private class TestTenantProvider : ITenantProvider
        {
            private readonly string _t;
            public TestTenantProvider(string t) => _t = t;
            public string GetTenantId() => _t;
        }

        private class TestCurrentUser : ICurrentUser
        {
            private readonly string _u;
            public TestCurrentUser(string u) => _u = u;
            public string? GetUserId() => _u;
            public string? GetUserName() => "Test User";
            public string? GetUserEmail() => "test@epros.com";
        }
    }
}
