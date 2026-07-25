using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Epros.Modules.Estoque.Application.Commands;
using Epros.Modules.Estoque.Application.Handlers;
using Epros.Modules.Estoque.Application.Services;
using Epros.Modules.Estoque.Domain.Entities;
using Epros.Modules.Estoque.Domain.Enums;
using Epros.Modules.Estoque.Infrastructure.Data;
using Epros.Shared.Domain.Enums;
using Epros.Tests.Integration;
using Xunit;

namespace Epros.Tests
{
    public class CompraFiscalAgregadoTests
    {
        private static ContextEstoque CreateContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<ContextEstoque>()
                .UseInMemoryDatabase(dbName)
                .Options;
            return new ContextEstoque(options, new TestTenantProvider("tenant-1"), new TestCurrentUser("user-1"));
        }

        private static Compra CriarCompraBase() =>
            new Compra("12345678000199", "Fornecedor X", "1", new string('7', 44), 100m, DateTime.UtcNow, "tenant-1", "user-1");

        // ============================ Agregado (domínio) ============================

        [Fact]
        public void DefinirEmitente_ComEmitenteValido_AnexaAoAgregado()
        {
            var compra = CriarCompraBase();
            var emitente = new CompraEmitente(compra.Id, null, null, "12345678000199", null, "Fornecedor X Ltda", null, null, "ISENTO", null, null, 0, ERegimeTributario.SimplesNacional, null, "tenant-1", "user-1");

            compra.DefinirEmitente(emitente);

            Assert.NotNull(compra.Emitente);
            Assert.True(compra.IsValid);
        }

        [Fact]
        public void AdicionarPagamento_DoisPagamentos_AmbosNaColecao()
        {
            var compra = CriarCompraBase();
            var p1 = new CompraPagamento(compra.Id, 0m, ETipoPagamento.Dinheiro, 50m, ETipoIntegracaoPagamentoCartao.NaoUtiliza, null, EBandeiraCartao.NaoUtiliza, null, "tenant-1", "user-1");
            var p2 = new CompraPagamento(compra.Id, 0m, ETipoPagamento.BoletoBancario, 50m, ETipoIntegracaoPagamentoCartao.NaoUtiliza, null, EBandeiraCartao.NaoUtiliza, null, "tenant-1", "user-1");

            compra.AdicionarPagamento(p1);
            compra.AdicionarPagamento(p2);

            Assert.Equal(2, compra.Pagamentos.Count);
            Assert.True(compra.AgregadoValido());
        }

        [Fact]
        public void AdicionarPagamento_PagamentoInvalido_PropagaNotificacaoParaRaiz()
        {
            var compra = CriarCompraBase();
            // ValorPagamento zero é inválido pelo Contract de CompraPagamento.
            var invalido = new CompraPagamento(compra.Id, 0m, ETipoPagamento.Dinheiro, 0m, ETipoIntegracaoPagamentoCartao.NaoUtiliza, null, EBandeiraCartao.NaoUtiliza, null, "tenant-1", "user-1");

            compra.AdicionarPagamento(invalido);

            Assert.Empty(compra.Pagamentos);
            Assert.False(compra.IsValid);
        }

        // ============================ Handler fiscal (persistência) ============================

        [Fact]
        public async Task LancarCompraFiscal_ComAgregadoCompleto_PersisteTudo()
        {
            var context = CreateContext(nameof(LancarCompraFiscal_ComAgregadoCompleto_PersisteTudo));
            var handler = new LancarCompraFiscalCommandHandler(context, new TestTenantProvider("tenant-1"), new TestCurrentUser("user-1"));

            var command = new LancarCompraFiscalCommand(
                FornecedorCnpj: "12345678000199",
                FornecedorNome: "Fornecedor X Ltda",
                NumeroNota: "555",
                ChaveAcesso: "35210712345678000199550010000005551234567890",
                ValorTotal: 100m,
                DataEmissao: DateTime.UtcNow,
                Itens: new List<ItemCompraFiscalInput>
                {
                    new("SKU-F1", "Produto Fiscal 1", 10m, 10m, 1.8m, 0.5m, Ncm: "12345678", Cfop: 1102, UnidadeComercial: "UN")
                },
                Emitente: new EmitenteInput("12345678000199", null, "Fornecedor X Ltda", "FornX", null, "ISENTO"),
                Destinatario: new DestinatarioInput(null, "98765432000188", null, "Comprador Y Ltda", null, "ISENTO", "c@y.com"),
                Imposto: new ImpostoCompraInput(18m),
                Total: new TotalCompraInput(100m, 0m, 0m, 0m, 0.5m, 1.8m, 100m),
                Transporte: new TransporteCompraInput(null, "11122233000144", null, "Transportes Z", "ISENTO", EEstado.SP),
                Pagamentos: new List<PagamentoCompraInput> { new(ETipoPagamento.BoletoBancario, 100m) });

            var result = await handler.Handle(command, CancellationToken.None);

            Assert.True(result.Sucesso);
            var compraId = (Guid)result.Dados!.GetType().GetProperty("CompraId")!.GetValue(result.Dados)!;

            var compra = await context.Compras
                .Include(c => c.Emitente)
                .Include(c => c.Destinatario)
                .Include(c => c.Imposto)
                .Include(c => c.Total)
                .Include(c => c.Transporte).ThenInclude(t => t!.Transportadora)
                .Include(c => c.Pagamentos)
                .Include(c => c.Itens)
                .FirstAsync(c => c.Id == compraId);

            Assert.NotNull(compra.Emitente);
            Assert.NotNull(compra.Destinatario);
            Assert.NotNull(compra.Imposto);
            Assert.Equal(18m, compra.Imposto!.ValorAliquotaCreditoIcms);
            Assert.NotNull(compra.Total);
            Assert.NotNull(compra.Transporte);
            Assert.NotNull(compra.Transporte!.Transportadora);
            Assert.Single(compra.Pagamentos);
            Assert.Single(compra.Itens);
        }

        [Fact]
        public async Task LancarCompraFiscal_ChaveDuplicada_Falha()
        {
            var context = CreateContext(nameof(LancarCompraFiscal_ChaveDuplicada_Falha));
            var handler = new LancarCompraFiscalCommandHandler(context, new TestTenantProvider("tenant-1"), new TestCurrentUser("user-1"));

            LancarCompraFiscalCommand Cmd() => new(
                "12345678000199", "Fornecedor X", "9", "35210712345678000199550010000009991234567890",
                50m, DateTime.UtcNow,
                new List<ItemCompraFiscalInput> { new("SKU-D1", "Prod", 1m, 50m, 0m, 0m) });

            var r1 = await handler.Handle(Cmd(), CancellationToken.None);
            var r2 = await handler.Handle(Cmd(), CancellationToken.None);

            Assert.True(r1.Sucesso);
            Assert.False(r2.Sucesso);
        }

        // ============================ Parser de XML ============================

        private const string XmlNfe = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<nfeProc xmlns=""http://www.portalfiscal.inf.br/nfe"">
  <NFe>
    <infNFe Id=""NFe35210712345678000199550010000005551234567890"">
      <ide><nNF>555</nNF><dhEmi>2026-07-01T10:00:00-03:00</dhEmi></ide>
      <emit>
        <CNPJ>12345678000199</CNPJ><xNome>Fornecedor X Ltda</xNome><xFant>FornX</xFant>
        <IE>1234567</IE><CRT>1</CRT>
        <enderEmit><fone>1133334444</fone><UF>SP</UF></enderEmit>
      </emit>
      <dest>
        <CNPJ>98765432000188</CNPJ><xNome>Comprador Y Ltda</xNome><IE>ISENTO</IE>
        <email>compras@y.com</email><indIEDest>1</indIEDest>
      </dest>
      <det nItem=""1"">
        <prod>
          <cProd>SKU-F1</cProd><xProd>Produto Fiscal 1</xProd><NCM>12345678</NCM>
          <CFOP>1102</CFOP><uCom>UN</uCom><qCom>10.0000</qCom><vUnCom>10.0000000000</vUnCom>
        </prod>
        <imposto>
          <ICMS><ICMS00><vICMS>18.00</vICMS></ICMS00></ICMS>
          <IPI><IPITrib><vIPI>5.00</vIPI></IPITrib></IPI>
        </imposto>
      </det>
      <total>
        <ICMSTot>
          <vProd>100.00</vProd><vFrete>0.00</vFrete><vSeg>0.00</vSeg><vDesc>0.00</vDesc>
          <vIPI>5.00</vIPI><vICMS>18.00</vICMS><vNF>105.00</vNF>
        </ICMSTot>
      </total>
      <transp>
        <transporta><CNPJ>11122233000144</CNPJ><xNome>Transportes Z</xNome><IE>ISENTO</IE><UF>SP</UF></transporta>
      </transp>
      <pag>
        <detPag><tPag>15</tPag><vPag>105.00</vPag></detPag>
      </pag>
    </infNFe>
  </NFe>
</nfeProc>";

        [Fact]
        public void Parse_XmlNfeValido_ExtraiCamposDoAgregado()
        {
            var cmd = NfeXmlParser.Parse(XmlNfe);

            Assert.Equal("12345678000199", cmd.FornecedorCnpj);
            Assert.Equal("Fornecedor X Ltda", cmd.FornecedorNome);
            Assert.Equal("555", cmd.NumeroNota);
            Assert.Equal("35210712345678000199550010000005551234567890", cmd.ChaveAcesso);
            Assert.Equal(44, cmd.ChaveAcesso.Length);
            Assert.Equal(105.00m, cmd.ValorTotal);

            Assert.NotNull(cmd.Emitente);
            Assert.Equal(ERegimeTributario.SimplesNacional, cmd.Emitente!.RegimeTributario);
            Assert.NotNull(cmd.Destinatario);
            Assert.Equal("Comprador Y Ltda", cmd.Destinatario!.RazaoSocial);

            Assert.Single(cmd.Itens);
            var item = cmd.Itens[0];
            Assert.Equal("SKU-F1", item.Sku);
            Assert.Equal(10m, item.Quantidade);
            Assert.Equal(10m, item.PrecoUnitario);
            Assert.Equal(18m, item.ValorIms);
            Assert.Equal(5m, item.ValorIpi);
            Assert.Equal(1102, item.Cfop);

            Assert.NotNull(cmd.Total);
            Assert.Equal(105.00m, cmd.Total!.ValorNotaFiscal);
            Assert.NotNull(cmd.Transporte);
            Assert.Equal("Transportes Z", cmd.Transporte!.TransportadoraRazaoSocialOuNome);
            Assert.NotNull(cmd.Pagamentos);
            Assert.Single(cmd.Pagamentos!);
            Assert.Equal(ETipoPagamento.BoletoBancario, cmd.Pagamentos![0].TipoPagamento);
        }

        [Fact]
        public void Parse_XmlVazio_LancaArgumentException()
        {
            Assert.Throws<ArgumentException>(() => NfeXmlParser.Parse(""));
        }

        [Fact]
        public void Parse_XmlSemInfNFe_LancaArgumentException()
        {
            Assert.Throws<ArgumentException>(() => NfeXmlParser.Parse("<raiz><a/></raiz>"));
        }

        [Fact]
        public async Task Parse_E_Persistir_ViaHandlerFiscal_GravaCompra()
        {
            var context = CreateContext(nameof(Parse_E_Persistir_ViaHandlerFiscal_GravaCompra));
            var handler = new LancarCompraFiscalCommandHandler(context, new TestTenantProvider("tenant-1"), new TestCurrentUser("user-1"));

            var comando = NfeXmlParser.Parse(XmlNfe);
            var result = await handler.Handle(comando, CancellationToken.None);

            Assert.True(result.Sucesso);
            Assert.Equal(1, await context.Compras.CountAsync());
        }
    }
}
