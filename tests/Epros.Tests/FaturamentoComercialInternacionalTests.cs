using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Vendas.Application.Commands;
using Epros.Modules.Vendas.Application.Handlers;
using Epros.Modules.Vendas.Application.Queries;
using Epros.Modules.Vendas.Domain.Entities;
using Epros.Modules.Vendas.Domain.Enums;
using Epros.Modules.Vendas.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Epros.Tests
{
    /// <summary>
    /// Testes do submódulo Faturamento Comercial Internacional (VEN-FCI).
    /// Fonte: EF_7_VENDAS_FATURAMENTO_COMERCIAL_INTERNACIONAL_V1. Foco: cálculo, validações (FCI-017..021)
    /// e a regra NF-08 de SINAIS OPOSTOS fatura × nota de crédito (razão e estoque).
    /// </summary>
    public class FaturamentoComercialInternacionalTests
    {
        private const string TenantId = "tenant-fci-001";
        private const string UserId = "user-fci-001";

        private static ContextVendas CreateContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<ContextVendas>().UseInMemoryDatabase(dbName).Options;
            return new ContextVendas(options, new TestTenantProvider(TenantId), new TestCurrentUser(UserId));
        }

        private static FciDocumentoComercial NovoDocumento(EFciTipoDocumento tipo, decimal qtd = 2m, decimal preco = 100m, decimal aliquota = 10m)
        {
            var doc = new FciDocumentoComercial(tipo, 1, "INV-000001", DateTime.UtcNow.Date, Guid.NewGuid(), Guid.NewGuid(),
                1, null, 0m, EFciTipoDesconto.Percentual, 0m, "USD", 5.10m, "FOB", null, null, null, TenantId, UserId);
            var item = new FciDocumentoItem(Guid.NewGuid(), Guid.NewGuid(), qtd, preco, null, 0m, null, aliquota, Guid.NewGuid(), null, TenantId, UserId);
            item.VincularDocumento(doc.Id);
            doc.AdicionarItem(item);
            doc.Recalcular();
            return doc;
        }

        // ---------- Cálculo e validação ----------

        [Fact(DisplayName = "FciItem | Bruto/imposto/total calculados (FCI-024/025)")]
        public void Item_Calcula()
        {
            var item = new FciDocumentoItem(Guid.NewGuid(), Guid.NewGuid(), 2m, 100m, null, 0m, null, 10m, Guid.NewGuid(), null, TenantId, UserId);
            Assert.True(item.IsValid);
            Assert.Equal(200m, item.ValorBruto);
            Assert.Equal(20m, item.ValorImposto);   // 10% de 200
            Assert.Equal(220m, item.ValorTotal);
        }

        [Fact(DisplayName = "FciDocumento | Recalcula totais (bruto + imposto + frete)")]
        public void Documento_RecalculaTotais()
        {
            var doc = NovoDocumento(EFciTipoDocumento.Fatura);
            Assert.Equal(200m, doc.TotalBruto);
            Assert.Equal(20m, doc.TotalImposto);
            Assert.Equal(220m, doc.TotalGeral);
            doc.Validar();
            Assert.True(doc.IsValid);
        }

        [Fact(DisplayName = "FciDocumento | Sem itens é rejeitado (FCI-017)")]
        public void Documento_SemItens_Invalido()
        {
            var doc = new FciDocumentoComercial(EFciTipoDocumento.Fatura, 1, "INV-1", DateTime.UtcNow, Guid.NewGuid(), Guid.NewGuid(),
                1, null, 0m, EFciTipoDesconto.Percentual, 0m, null, null, null, null, null, null, TenantId, UserId);
            doc.Recalcular();
            doc.Validar();
            Assert.False(doc.IsValid);
        }

        [Fact(DisplayName = "FciDocumento | Sem cliente é rejeitado (FCI-019)")]
        public void Documento_SemCliente_Invalido()
        {
            var doc = new FciDocumentoComercial(EFciTipoDocumento.Fatura, 1, "INV-1", DateTime.UtcNow, Guid.Empty, Guid.NewGuid(),
                1, null, 0m, EFciTipoDesconto.Percentual, 0m, null, null, null, null, null, null, TenantId, UserId);
            var item = new FciDocumentoItem(Guid.NewGuid(), Guid.NewGuid(), 1m, 10m, null, 0m, null, null, Guid.NewGuid(), null, TenantId, UserId);
            doc.AdicionarItem(item); doc.Recalcular(); doc.Validar();
            Assert.False(doc.IsValid);
        }

        // ---------- NF-08: sinais opostos ----------

        [Fact(DisplayName = "FCI razão | Fatura DEBITA contas a receber e CREDITA receita/imposto (FCI-030..033)")]
        public void Razao_Fatura_Sinais()
        {
            var doc = NovoDocumento(EFciTipoDocumento.Fatura);
            var lanc = doc.GerarLancamentosRazao(UserId).ToList();

            var ar = lanc.Single(l => l.Categoria == EFciRazaoCategoria.ContasReceber);
            Assert.Equal(220m, ar.Debito);
            Assert.Equal(0m, ar.Credito);

            var item = lanc.Single(l => l.Categoria == EFciRazaoCategoria.Item);
            Assert.Equal(0m, item.Debito);
            Assert.Equal(220m, item.Credito); // receita creditada

            var imposto = lanc.Single(l => l.Categoria == EFciRazaoCategoria.Imposto);
            Assert.Equal(0m, imposto.Debito);
            Assert.Equal(20m, imposto.Credito);
        }

        [Fact(DisplayName = "FCI razão | Nota de crédito inverte TODOS os sinais da fatura (FCI-034..037)")]
        public void Razao_NotaCredito_SinaisInvertidos()
        {
            var doc = NovoDocumento(EFciTipoDocumento.NotaCredito);
            var lanc = doc.GerarLancamentosRazao(UserId).ToList();

            var ar = lanc.Single(l => l.Categoria == EFciRazaoCategoria.ContasReceber);
            Assert.Equal(0m, ar.Debito);
            Assert.Equal(220m, ar.Credito); // invertido: credita AR

            var item = lanc.Single(l => l.Categoria == EFciRazaoCategoria.Item);
            Assert.Equal(220m, item.Debito); // invertido: debita a conta da linha
            Assert.Equal(0m, item.Credito);

            var imposto = lanc.Single(l => l.Categoria == EFciRazaoCategoria.Imposto);
            Assert.Equal(20m, imposto.Debito); // invertido
            Assert.Equal(0m, imposto.Credito);
        }

        [Fact(DisplayName = "FCI estoque | Fatura gera SAÍDA; nota de crédito gera ENTRADA (FCI-027/028)")]
        public void Estoque_SinaisOpostos()
        {
            var fatura = NovoDocumento(EFciTipoDocumento.Fatura, qtd: 3m);
            var estFat = fatura.GerarLancamentosEstoque().Single();
            Assert.Equal(3m, estFat.QuantidadeSaida);
            Assert.Equal(0m, estFat.QuantidadeEntrada);

            var nota = NovoDocumento(EFciTipoDocumento.NotaCredito, qtd: 3m);
            var estNota = nota.GerarLancamentosEstoque().Single();
            Assert.Equal(0m, estNota.QuantidadeSaida);
            Assert.Equal(3m, estNota.QuantidadeEntrada);
        }

        // ---------- Handlers ----------

        [Fact(DisplayName = "FCI handler | Criar salva rascunho SEM estoque/razão (FCI-014)")]
        public async Task Handler_Criar_Rascunho_SemEfeitos()
        {
            var ctx = CreateContext(nameof(Handler_Criar_Rascunho_SemEfeitos));
            var tp = new TestTenantProvider(TenantId); var cu = new TestCurrentUser(UserId);
            var r = await new CriarFciDocumentoCommandHandler(ctx, tp, cu).Handle(NovoComando(EFciTipoDocumento.Fatura), CancellationToken.None);
            Assert.True(r.Sucesso);
            Assert.Equal(EFciStatus.Rascunho, (await ctx.FciDocumentos.FirstAsync()).Status);
            Assert.Empty(ctx.FciLancamentosRazao);
            Assert.Empty(ctx.FciLancamentosEstoque);
        }

        [Fact(DisplayName = "FCI handler | Aprovar gera estoque e razão (FCI-015)")]
        public async Task Handler_Aprovar_GeraEfeitos()
        {
            var ctx = CreateContext(nameof(Handler_Aprovar_GeraEfeitos));
            var tp = new TestTenantProvider(TenantId); var cu = new TestCurrentUser(UserId);
            await new CriarFciDocumentoCommandHandler(ctx, tp, cu).Handle(NovoComando(EFciTipoDocumento.Fatura), CancellationToken.None);
            var doc = await ctx.FciDocumentos.FirstAsync();
            var r = await new AprovarFciDocumentoCommandHandler(ctx, tp, cu).Handle(new AprovarFciDocumentoCommand(doc.Id), CancellationToken.None);
            Assert.True(r.Sucesso);
            Assert.Equal(EFciStatus.Aprovada, (await ctx.FciDocumentos.FirstAsync()).Status);
            Assert.NotEmpty(ctx.FciLancamentosRazao);
            Assert.NotEmpty(ctx.FciLancamentosEstoque);
        }

        [Fact(DisplayName = "FCI handler | Serial automático incrementa por tenant/tipo (FCI-006)")]
        public async Task Handler_Serial_Incrementa()
        {
            var ctx = CreateContext(nameof(Handler_Serial_Incrementa));
            var tp = new TestTenantProvider(TenantId); var cu = new TestCurrentUser(UserId);
            var h = new CriarFciDocumentoCommandHandler(ctx, tp, cu);
            await h.Handle(NovoComando(EFciTipoDocumento.Fatura), CancellationToken.None);
            await h.Handle(NovoComando(EFciTipoDocumento.Fatura), CancellationToken.None);
            var seriais = await ctx.FciDocumentos.Select(d => d.Serial).OrderBy(s => s).ToListAsync();
            Assert.Equal(new[] { 1, 2 }, seriais);
        }

        [Fact(DisplayName = "FCI handler | Imposto com nome duplicado é rejeitado (FCI-050)")]
        public async Task Handler_Imposto_Duplicado()
        {
            var ctx = CreateContext(nameof(Handler_Imposto_Duplicado));
            var tp = new TestTenantProvider(TenantId); var cu = new TestCurrentUser(UserId);
            var h = new CriarFciImpostoCommandHandler(ctx, tp, cu);
            var r1 = await h.Handle(new CriarFciImpostoCommand("VAT", 5m), CancellationToken.None);
            var r2 = await h.Handle(new CriarFciImpostoCommand("VAT", 7m), CancellationToken.None);
            Assert.True(r1.Sucesso);
            Assert.False(r2.Sucesso);
        }

        private static CriarFciDocumentoCommand NovoComando(EFciTipoDocumento tipo) => new(
            tipo, null, DateTime.UtcNow.Date, Guid.NewGuid(), Guid.NewGuid(), 1, null, 0m, EFciTipoDesconto.Percentual,
            0m, "USD", 5.1m, "FOB", null, null,
            new List<FciItemInput> { new(Guid.NewGuid(), Guid.NewGuid(), 2m, 100m, null, 0m, null, 10m, Guid.NewGuid(), null) });

        private class TestTenantProvider : Epros.Shared.Application.Contracts.ITenantProvider
        {
            private readonly string _t;
            public TestTenantProvider(string t) => _t = t;
            public string GetTenantId() => _t;
        }

        private class TestCurrentUser : Epros.Shared.Application.Contracts.ICurrentUser
        {
            private readonly string _u;
            public TestCurrentUser(string u) => _u = u;
            public string? GetUserId() => _u;
            public string? GetUserName() => "Test User";
            public string? GetUserEmail() => "test@epros.com";
        }
    }
}
