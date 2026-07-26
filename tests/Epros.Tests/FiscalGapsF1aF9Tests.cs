using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Microsoft.EntityFrameworkCore;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Domain.Enums;
using Epros.Modules.Fiscal.Domain.Entities;
using Epros.Modules.Fiscal.Application.Commands;
using Epros.Modules.Fiscal.Application.Handlers;
using Epros.Modules.Fiscal.Application.Queries;
using Epros.Modules.Fiscal.Application.Services;
using Epros.Modules.Fiscal.Infrastructure.Data;
using Epros.Modules.Fiscal.Infrastructure.Services;

namespace Epros.Tests
{
    /// <summary>
    /// Cobertura das pontas fiscais F1 (QR NFC-e), F3 (salvar-nf-com-xml / cancelamento por XML),
    /// F5 (tabela/lookup IBPT), F8 (Code128 no DANFE) e F9 (cStat 573 duplicidade no cancelamento).
    /// </summary>
    public class FiscalGapsF1aF9Tests
    {
        // ---- F1: QR Code da NFC-e é desenhado a partir do infNFeSupl do XML autorizado ----
        [Fact]
        public void Cupom_Nfce_Autorizado_Com_InfNFeSupl_Gera_Pdf_Nao_Vazio()
        {
            var doc = new DocumentoFiscal("65", 2, 1, 500, 10.00m, "12345678000199", "CONSUMIDOR", "t", "u");
            doc.AdicionarItem("SKU1", "Produto Teste", "102", 5102, "22021000", 1, 10.00m, 0, "u");
            var chave = "35260612345678000199650010000005001000005001";
            var xmlAutorizado =
                "<nfeProc xmlns=\"http://www.portalfiscal.inf.br/nfe\">" +
                "<NFe><infNFe Id=\"NFe" + chave + "\"></infNFe>" +
                "<infNFeSupl><qrCode>https://www.sefaz.uf.gov.br/nfce/qrcode?p=" + chave + "|2|2|1|ABC123</qrCode>" +
                "<urlChave>https://www.sefaz.uf.gov.br/nfce/consulta</urlChave></infNFeSupl></NFe>" +
                "<protNFe><infProt><cStat>100</cStat><nProt>135260000005001</nProt></infProt></protNFe></nfeProc>";
            doc.Autorizar(chave, "135260000005001", 100, xmlAutorizado, xmlAutorizado, "", "");

            var svc = new DanfeQuestPdfService();
            var pdf = svc.GerarPdf(doc);

            Assert.NotNull(pdf);
            Assert.True(pdf.Length > 1000, "PDF do cupom NFC-e com QR deve ter conteúdo real.");
            Assert.Equal(0x25, pdf[0]); // '%' — assinatura de arquivo PDF
        }

        // ---- F8: DANFE NF-e com Code128 da chave gera PDF sem exceção ----
        [Fact]
        public void Danfe_Nfe_Autorizada_Gera_Pdf_Com_Codigo_De_Barras()
        {
            var doc = new DocumentoFiscal("55", 1, 1, 1000, 250.00m, "12345678000199", "Cliente S/A", "t", "u");
            doc.AdicionarItem("SKU1", "Produto", "00", 5102, "84713012", 2, 125.00m, 18, "u");
            var chave = "35260612345678000199550010000010001000010001";
            doc.Autorizar(chave, "135260000010001", 100, "<x/>", "<x/>", "", "");

            var svc = new DanfeQuestPdfService();
            var pdf = svc.GerarPdf(doc);

            Assert.NotNull(pdf);
            Assert.True(pdf.Length > 1000);
            Assert.Equal(0x25, pdf[0]);
        }

        // ---- F5: lookup IBPT por NCM/UF ----
        [Fact]
        public async Task ObterAliquotasIbpt_Retorna_Linha_Da_Tabela()
        {
            var ctx = CriarContexto(nameof(ObterAliquotasIbpt_Retorna_Linha_Da_Tabela));
            ctx.Ibpts.Add(new Ibpt("22021000", EEstado.SP, 0, 0, "Refrigerante", 12.34m, 15.00m, 18.00m, 3.50m,
                "25.1.A", "ABCDEF", DateTime.UtcNow.AddMonths(-1), DateTime.UtcNow.AddMonths(6), "u"));
            await ctx.SaveChangesAsync();

            var handler = new ObterAliquotasIbptPorNcmUfQueryHandler(ctx);
            var res = await handler.Handle(new ObterAliquotasIbptPorNcmUfQuery("2202.10.00", "SP"), CancellationToken.None);

            Assert.True(res.Sucesso);
            Assert.Equal(12.34m, Prop<decimal>(res.Dados!, "AliquotaFederalNacional"));
            Assert.Equal(18.00m, Prop<decimal>(res.Dados!, "AliquotaEstadual"));
        }

        private static T Prop<T>(object obj, string name)
            => (T)obj.GetType().GetProperty(name)!.GetValue(obj)!;

        [Fact]
        public async Task ObterAliquotasIbpt_Sem_Cadastro_Falha_Honestamente()
        {
            var ctx = CriarContexto(nameof(ObterAliquotasIbpt_Sem_Cadastro_Falha_Honestamente));
            var handler = new ObterAliquotasIbptPorNcmUfQueryHandler(ctx);
            var res = await handler.Handle(new ObterAliquotasIbptPorNcmUfQuery("99999999", "RJ"), CancellationToken.None);
            Assert.False(res.Sucesso);
        }

        // ---- F3: importar NF-e autorizada por XML ----
        [Fact]
        public async Task SalvarNfComXml_Importa_Documento_Autorizado()
        {
            var ctx = CriarContexto(nameof(SalvarNfComXml_Importa_Documento_Autorizado));
            var chave = "35260612345678000199550010000001001000001001";
            var xml =
                "<nfeProc xmlns=\"http://www.portalfiscal.inf.br/nfe\"><NFe><infNFe Id=\"NFe" + chave + "\">" +
                "<ide><mod>55</mod><serie>1</serie><nNF>100</nNF><tpAmb>1</tpAmb></ide>" +
                "<dest><CNPJ>99999999000191</CNPJ><xNome>CLIENTE XML</xNome></dest>" +
                "<det><prod><cProd>P1</cProd><xProd>PROD XML</xProd><NCM>84713012</NCM><CFOP>5102</CFOP><qCom>1.00</qCom><vUnCom>50.00</vUnCom></prod>" +
                "<imposto><ICMS><ICMS00><CST>00</CST><vBC>50.00</vBC><pICMS>18.00</pICMS><vICMS>9.00</vICMS></ICMS00></ICMS></imposto></det>" +
                "<total><ICMSTot><vNF>50.00</vNF></ICMSTot></total></infNFe></NFe>" +
                "<protNFe><infProt><cStat>100</cStat><nProt>135260000001001</nProt></infProt></protNFe></nfeProc>";

            var handler = new SalvarDocumentoFiscalComXmlCommandHandler(
                ctx, new TP("t"), new CU("u"), new ArmazenamentoNulo(), new DanfeQuestPdfService());

            var res = await handler.Handle(new SalvarDocumentoFiscalComXmlCommand(xml), CancellationToken.None);

            Assert.True(res.Sucesso, string.Join(";", res.Erros ?? new string[0]));
            var salvo = await ctx.DocumentosFiscais.Include(d => d.Itens).FirstOrDefaultAsync(d => d.ChaveAcesso == chave);
            Assert.NotNull(salvo);
            Assert.Equal("Autorizado", salvo!.Status);
            Assert.Equal("135260000001001", salvo.Protocolo);
            Assert.Single(salvo.Itens);
        }

        [Fact]
        public async Task SalvarNfComXml_Rejeita_Xml_Nao_Autorizado()
        {
            var ctx = CriarContexto(nameof(SalvarNfComXml_Rejeita_Xml_Nao_Autorizado));
            var chave = "35260612345678000199550010000002001000002001";
            var xml = "<NFe xmlns=\"http://www.portalfiscal.inf.br/nfe\"><infNFe Id=\"NFe" + chave + "\">" +
                      "<ide><mod>55</mod><serie>1</serie><nNF>200</nNF></ide></infNFe></NFe>";

            var handler = new SalvarDocumentoFiscalComXmlCommandHandler(
                ctx, new TP("t"), new CU("u"), new ArmazenamentoNulo(), new DanfeQuestPdfService());
            var res = await handler.Handle(new SalvarDocumentoFiscalComXmlCommand(xml), CancellationToken.None);

            Assert.False(res.Sucesso); // sem protNFe/cStat=100 => não importa
        }

        // ---- F3: registrar cancelamento por XML de evento externo ----
        [Fact]
        public async Task RegistrarCancelamentoPorXml_Cancela_Documento_Existente()
        {
            var ctx = CriarContexto(nameof(RegistrarCancelamentoPorXml_Cancela_Documento_Existente));
            var chave = "35260612345678000199550010000003001000003001";
            var doc = new DocumentoFiscal("55", 1, 1, 300, 100m, "99999999000191", "CLI", "t", "u");
            doc.Autorizar(chave, "135260000003001", 100, "<x/>", "<x/>", "", "");
            ctx.DocumentosFiscais.Add(doc);
            await ctx.SaveChangesAsync();

            var xmlEvento =
                "<procEventoNFe xmlns=\"http://www.portalfiscal.inf.br/nfe\"><evento><infEvento>" +
                "<chNFe>" + chave + "</chNFe><tpEvento>110111</tpEvento></infEvento></evento>" +
                "<retEvento><infEvento><chNFe>" + chave + "</chNFe><cStat>135</cStat>" +
                "<xMotivo>Evento registrado e vinculado a NF-e</xMotivo><nProt>135260000009999</nProt></infEvento></retEvento></procEventoNFe>";

            var handler = new RegistrarCancelamentoPorXmlCommandHandler(ctx, new TP("t"), new CU("u"));
            var res = await handler.Handle(new RegistrarCancelamentoPorXmlCommand(xmlEvento), CancellationToken.None);

            Assert.True(res.Sucesso, string.Join(";", res.Erros ?? new string[0]));
            var salvo = await ctx.DocumentosFiscais.FirstAsync(d => d.ChaveAcesso == chave);
            Assert.Equal("Cancelada", salvo.Status);
            var evt = await ctx.EventosDocumentosFiscais.FirstAsync(e => e.DocumentoFiscalId == doc.Id);
            Assert.Equal("Cancelamento", evt.TipoEvento);
        }

        // ---- F9: cStat 573 (duplicidade) → reconsulta 101 → cancelamento efetivo ----
        [Fact]
        public async Task Cancelamento_Com_573_Reconsulta_E_Confirma_Cancelado()
        {
            var ctx = CriarContexto(nameof(Cancelamento_Com_573_Reconsulta_E_Confirma_Cancelado));
            var chave = "35260612345678000199550010000004001000004001";
            var doc = new DocumentoFiscal("55", 1, 1, 400, 100m, "99999999000191", "CLI", "t", "u");
            doc.Autorizar(chave, "135260000004001", 100, "<x/>", "<x/>", "", "");
            ctx.DocumentosFiscais.Add(doc);
            await ctx.SaveChangesAsync();

            // Cancelamento retorna 573 (duplicidade); reconsulta de protocolo devolve 101 (já cancelada).
            var fiscal = new FiscalDuplicidade573();
            var handler = new CancelarDocumentoFiscalCommandHandler(ctx, new TP("t"), new CU("u"), fiscal);
            var res = await handler.Handle(new CancelarDocumentoFiscalCommand(doc.Id, "Justificativa valida com mais de quinze caracteres"), CancellationToken.None);

            Assert.True(res.Sucesso);
            var salvo = await ctx.DocumentosFiscais.FirstAsync(d => d.Id == doc.Id);
            Assert.Equal("Cancelada", salvo.Status);
        }

        // ------------------------------------------------------------------ helpers/doubles

        private static ContextFiscal CriarContexto(string db)
        {
            var options = new DbContextOptionsBuilder<ContextFiscal>().UseInMemoryDatabase(db).Options;
            return new ContextFiscal(options, new TP("t"), new CU("u"));
        }

        private class TP : ITenantProvider
        {
            private readonly string _t;
            public TP(string t) => _t = t;
            public string GetTenantId() => _t;
        }

        private class CU : ICurrentUser
        {
            private readonly string _u;
            public CU(string u) => _u = u;
            public string? GetUserId() => _u;
            public string? GetUserName() => "Test";
            public string? GetUserEmail() => "t@e.com";
        }

        private class ArmazenamentoNulo : IArmazenamentoArquivoFiscal
        {
            public Task<string> SalvarAsync(string chaveLogica, string nomeArquivo, byte[] conteudo, string contentType, CancellationToken ct = default)
                => Task.FromResult($"mem://{chaveLogica}/{nomeArquivo}");
            public Task<byte[]?> LerAsync(string caminho, CancellationToken ct = default) => Task.FromResult<byte[]?>(null);
        }

        /// <summary>Double que reproduz o cenário cStat 573: cancelamento falha (573), reconsulta devolve 101.</summary>
        private class FiscalDuplicidade573 : IHerculesFiscalService
        {
            public Task<RetornoEmissaoDto> EmitirAsync(DocumentoFiscal d) => Task.FromResult(new RetornoEmissaoDto { Sucesso = true, StatusSefaz = 100 });
            public Task<RetornoCancelamentoDto> CancelarAsync(DocumentoFiscal d, string j)
                => Task.FromResult(new RetornoCancelamentoDto { Sucesso = false, StatusSefaz = 573, Motivo = "Duplicidade de evento" });
            public Task<RetornoEventoDto> CartaCorrecaoAsync(DocumentoFiscal d, string t, int s) => Task.FromResult(new RetornoEventoDto { Sucesso = true, StatusSefaz = 135 });
            public Task<RetornoInutilizacaoDto> InutilizarAsync(InutilizacaoFiscalRequest r) => Task.FromResult(new RetornoInutilizacaoDto { Sucesso = true, StatusSefaz = 102 });
            public Task<RetornoConsultaSefazDto> VerificarStatusServicoAsync(ConsultaStatusServicoRequest r) => Task.FromResult(new RetornoConsultaSefazDto { Sucesso = true, StatusSefaz = 107 });
            public Task<RetornoConsultaSefazDto> ConsultarProtocoloAsync(ConsultaProtocoloRequest r)
                => Task.FromResult(new RetornoConsultaSefazDto { Sucesso = true, StatusSefaz = 101, Motivo = "Cancelamento de NF-e homologado" });
        }
    }
}
