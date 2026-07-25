using Xunit;
using Epros.Modules.Fiscal.Domain.Entities;
using Epros.Modules.Fiscal.Domain.Enums;

namespace Epros.Tests
{
    public class ContingenciaFiscalTests
    {
        private const string TenantId = "tenant-test-001";
        private const string UserId = "user-test-001";

        private static DocumentoFiscal CriarDocumento(string modelo = "55") =>
            new DocumentoFiscal(modelo, 2, 1, 100, 250m, "14200166000187", "Cliente Exemplo", TenantId, UserId);

        [Fact(DisplayName = "DocumentoFiscal | Recém-criado | Deve estar em emissão Normal (caminho normal intacto)")]
        public void Criar_Documento_DeveIniciarNormal()
        {
            var doc = CriarDocumento();

            Assert.Equal(ETipoEmissaoFiscal.Normal, doc.TipoEmissao);
            Assert.False(doc.EmContingencia);
            Assert.Equal(1, doc.Finalidade);
            Assert.Null(doc.ChaveReferenciada);
        }

        [Fact(DisplayName = "DocumentoFiscal | EntrarContingencia SVC-AN | Deve marcar tpEmis e justificativa")]
        public void EntrarContingencia_SvcAn_DeveMarcar()
        {
            var doc = CriarDocumento();

            doc.EntrarContingencia(ETipoEmissaoFiscal.ContingenciaSvcAn, "SEFAZ da UF indisponível para autorização");

            Assert.True(doc.EmContingencia);
            Assert.Equal(ETipoEmissaoFiscal.ContingenciaSvcAn, doc.TipoEmissao);
            Assert.Equal("SEFAZ da UF indisponível para autorização", doc.JustificativaContingencia);
            Assert.NotNull(doc.DataHoraContingencia);
        }

        [Fact(DisplayName = "DocumentoFiscal | EntrarContingencia com Normal | Não deve alterar (permanece Normal)")]
        public void EntrarContingencia_Normal_NaoAltera()
        {
            var doc = CriarDocumento();

            doc.EntrarContingencia(ETipoEmissaoFiscal.Normal, "qualquer justificativa aqui");

            Assert.False(doc.EmContingencia);
            Assert.Null(doc.JustificativaContingencia);
        }

        [Fact(DisplayName = "DocumentoFiscal | RetornarEmissaoNormal | Deve limpar contingência")]
        public void RetornarEmissaoNormal_DeveLimpar()
        {
            var doc = CriarDocumento();
            doc.EntrarContingencia(ETipoEmissaoFiscal.ContingenciaSvcRs, "SEFAZ indisponível por manutenção programada");

            doc.RetornarEmissaoNormal();

            Assert.False(doc.EmContingencia);
            Assert.Equal(ETipoEmissaoFiscal.Normal, doc.TipoEmissao);
            Assert.Null(doc.JustificativaContingencia);
            Assert.Null(doc.DataHoraContingencia);
        }

        [Fact(DisplayName = "DocumentoFiscal | MarcarPendenteContingencia | Deve ficar com Status PendenteContingencia (offline)")]
        public void MarcarPendenteContingencia_DeveAlterarStatus()
        {
            var doc = CriarDocumento("65");
            doc.EntrarContingencia(ETipoEmissaoFiscal.ContingenciaOffline, "Contingência offline por queda de comunicação");

            doc.MarcarPendenteContingencia();

            Assert.Equal("PendenteContingencia", doc.Status);
            Assert.Equal(ETipoEmissaoFiscal.ContingenciaOffline, doc.TipoEmissao);
        }

        [Fact(DisplayName = "DocumentoFiscal | DefinirDevolucao | Deve marcar finalidade 4 e chave referenciada")]
        public void DefinirDevolucao_DeveMarcarFinalidadeERef()
        {
            var doc = CriarDocumento();
            var chave = "35200114200166000187550010000000015123456789";

            doc.DefinirDevolucao(chave);

            Assert.Equal(4, doc.Finalidade);
            Assert.Equal(chave, doc.ChaveReferenciada);
        }

        [Fact(DisplayName = "ETipoEmissaoFiscal | Valores | Devem coincidir com o código oficial tpEmis da SEFAZ")]
        public void EnumTipoEmissao_ValoresOficiais()
        {
            Assert.Equal(1, (int)ETipoEmissaoFiscal.Normal);
            Assert.Equal(4, (int)ETipoEmissaoFiscal.ContingenciaEpec);
            Assert.Equal(6, (int)ETipoEmissaoFiscal.ContingenciaSvcAn);
            Assert.Equal(7, (int)ETipoEmissaoFiscal.ContingenciaSvcRs);
            Assert.Equal(9, (int)ETipoEmissaoFiscal.ContingenciaOffline);
        }
    }
}
