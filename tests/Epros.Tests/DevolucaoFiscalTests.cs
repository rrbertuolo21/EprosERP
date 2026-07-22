using Xunit;
using Epros.Modules.Fiscal.Domain.Entities;
using Epros.Modules.Fiscal.Domain.Enums;

namespace Epros.Tests
{
    public class DevolucaoFiscalTests
    {
        private const string TenantId = "tenant-test-001";
        private const string UserId = "user-test-001";
        private const string ChaveValida = "35200114200166000187550010000000015123456789";

        private static DevolucaoFiscal CriarDevolucaoValida(string? chave = null)
        {
            var dev = new DevolucaoFiscal(
                modelo: "55",
                ambiente: 2,
                serie: 1,
                chaveNfEntrada: chave ?? ChaveValida,
                motivo: "Devolução de mercadoria avariada",
                destinatarioCnpjCpf: "14200166000187",
                destinatarioNome: "Fornecedor Exemplo Ltda",
                total: 100m,
                empresaId: System.Guid.NewGuid(),
                documentoOrigemId: null,
                xmlEntrada: null,
                tenantId: TenantId,
                criadoPor: UserId);

            dev.AdicionarItem(null, "SKU-1", "Produto A", "12345678", 1202, "00", 2m, 50m, 18m, UserId);
            return dev;
        }

        [Fact(DisplayName = "DevolucaoFiscal | Dados válidos com chave de entrada e item | Deve ser válida em estado NOVO")]
        public void Criar_DadosValidos_DeveSerValidaEmNovo()
        {
            var dev = CriarDevolucaoValida();

            Assert.True(dev.IsValid);
            Assert.Equal(EEstadoDevolucaoFiscal.Novo, dev.Estado);
            Assert.Single(dev.Itens);
        }

        [Fact(DisplayName = "DevolucaoFiscal | Sem chave da NF de entrada | Deve ser inválida (REG-DEV-001)")]
        public void Criar_SemChaveEntrada_DeveSerInvalida()
        {
            var dev = new DevolucaoFiscal("55", 2, 1, "", "motivo", "14200166000187", "Fornecedor", 100m,
                System.Guid.NewGuid(), null, null, TenantId, UserId);

            Assert.False(dev.IsValid);
        }

        [Fact(DisplayName = "DevolucaoFiscal | Chave de entrada com tamanho diferente de 44 | Deve ser inválida")]
        public void Criar_ChaveTamanhoInvalido_DeveSerInvalida()
        {
            var dev = new DevolucaoFiscal("55", 2, 1, "123", "motivo", "14200166000187", "Fornecedor", 100m,
                System.Guid.NewGuid(), null, null, TenantId, UserId);

            Assert.False(dev.IsValid);
        }

        [Fact(DisplayName = "DevolucaoFiscal | NOVO com itens e referência | PodeTransmitir deve ser verdadeiro (REG-DEV-013)")]
        public void PodeTransmitir_NovoComItens_DeveSerVerdadeiro()
        {
            var dev = CriarDevolucaoValida();

            var pode = dev.PodeTransmitir(out var motivo);

            Assert.True(pode);
            Assert.Equal(string.Empty, motivo);
        }

        [Fact(DisplayName = "DevolucaoFiscal | Sem itens | Não deve transmitir (REG-DEV-018)")]
        public void PodeTransmitir_SemItens_DeveBloquear()
        {
            var dev = new DevolucaoFiscal("55", 2, 1, ChaveValida, "motivo", "14200166000187", "Fornecedor", 100m,
                System.Guid.NewGuid(), null, null, TenantId, UserId);

            var pode = dev.PodeTransmitir(out var motivo);

            Assert.False(pode);
            Assert.Contains("sem itens", motivo, System.StringComparison.OrdinalIgnoreCase);
        }

        [Fact(DisplayName = "DevolucaoFiscal | Aprovada | Não deve retransmitir pelo fluxo normal (REG-DEV-012)")]
        public void PodeTransmitir_Aprovada_DeveBloquear()
        {
            var dev = CriarDevolucaoValida();
            dev.Aprovar(ChaveValida, 10, "PROT-1", "<xml/>", UserId);

            var pode = dev.PodeTransmitir(out _);

            Assert.False(pode);
            Assert.Equal(EEstadoDevolucaoFiscal.Aprovado, dev.Estado);
        }

        [Fact(DisplayName = "DevolucaoFiscal | Aprovar | Deve gravar chave gerada, número gerado e protocolo (REG-DEV-004/008/009)")]
        public void Aprovar_DeveGravarChaveNumeroProtocolo()
        {
            var dev = CriarDevolucaoValida();

            dev.Aprovar("35990114200166000187550010000000025987654321", 25, "135200000012345", "<nfeProc/>", UserId);

            Assert.Equal(EEstadoDevolucaoFiscal.Aprovado, dev.Estado);
            Assert.Equal("35990114200166000187550010000000025987654321", dev.ChaveGerada);
            Assert.Equal(25, dev.NumeroGerado);
            Assert.Equal("135200000012345", dev.Protocolo);
            Assert.NotNull(dev.DataTransmissao);
        }

        [Fact(DisplayName = "DevolucaoFiscal | Cancelar aprovada | Deve preservar chaves rastreáveis (REG-DEV-019)")]
        public void Cancelar_DevePreservarChaves()
        {
            var dev = CriarDevolucaoValida();
            dev.Aprovar("35990114200166000187550010000000025987654321", 25, "PROT", "<xml/>", UserId);

            dev.Cancelar("Cancelamento homologado", "<evento/>", UserId);

            Assert.Equal(EEstadoDevolucaoFiscal.Cancelado, dev.Estado);
            Assert.Equal(ChaveValida, dev.ChaveNfEntrada);
            Assert.Equal("35990114200166000187550010000000025987654321", dev.ChaveGerada);
        }

        [Fact(DisplayName = "DevolucaoFiscal | Rejeitar | Deve permitir nova transmissão (REG-DEV-005/014)")]
        public void Rejeitar_DevePermitirRetransmissao()
        {
            var dev = CriarDevolucaoValida();

            dev.Rejeitar("[204] Duplicidade", "<ret/>", UserId);

            Assert.Equal(EEstadoDevolucaoFiscal.Rejeitado, dev.Estado);
            Assert.True(dev.PodeTransmitir(out _));
        }

        [Fact(DisplayName = "DevolucaoFiscalItem | Sem NCM | Deve ser inválido (REG-DEV-011)")]
        public void Item_SemNcm_DeveSerInvalido()
        {
            var item = new DevolucaoFiscalItem(System.Guid.NewGuid(), null, "SKU", "Produto", "", 1202, "00", 1m, 10m, 18m, TenantId, UserId);

            Assert.False(item.IsValid);
        }
    }
}
