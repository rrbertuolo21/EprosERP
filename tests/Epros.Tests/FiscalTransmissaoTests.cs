using System.Linq;
using Xunit;
using Epros.Modules.Fiscal.Application.Services;
using Epros.Modules.Fiscal.Domain.Entities;
using Epros.Modules.Fiscal.Infrastructure.Services;

// Isolamento do namespace legado por alias (evita colisão de enums Epros.ERP.Shared vs Epros.Shared).
using MotorGeraXml = Epros.ERP.DfeCalculos.Models.GeraXmlDfe;
using MotorVenda = Epros.ERP.DfeCalculos.Models.Vendas.Venda;
using MotorVersaoServico = DFe.Classes.Flags.VersaoServico;
using MotorModeloDocumento = DFe.Classes.Flags.ModeloDocumento;

namespace Epros.Tests
{
    /// <summary>
    /// Testes OFFLINE do adapter de transmissão: exercitam o mapeamento
    /// DocumentoFiscal -> Venda (motor) -> objeto NFe via GeraXmlDfe.ObterNf,
    /// SEM chamar a SEFAZ (transmissão real precisa de certificado/rede).
    /// </summary>
    public class FiscalTransmissaoTests
    {
        private const string TenantId = "tenant-fiscal-001";
        private const string UserId = "user-fiscal-001";

        private static ContextoTransmissaoFiscal ContextoValido() => new()
        {
            Emitente = new EmitenteFiscalDto
            {
                Documento = "11444777000161", // CNPJ válido
                RegimeTributario = 3,          // Regime Normal
                RazaoSocial = "Empresa Emitente Teste LTDA",
                NomeFantasia = "Emitente Teste",
                InscricaoEstadual = "110042490114",
                Cnae = "4751201",
                Telefone = "1133334444",
                Uf = "SP",
                Logradouro = "Rua das Flores",
                Numero = "100",
                Bairro = "Centro",
                CodMunicipioIbge = 3550308, // São Paulo
                NomeMunicipio = "Sao Paulo",
                Cep = "01001000"
            },
            Destinatario = new DestinatarioFiscalDto
            {
                Documento = "12345678000199",
                Nome = "Cliente Destino S/A",
                IndicadorIeDestinatario = 9, // Não contribuinte
                ConsumidorFinal = 1,
                Uf = "SP",
                Logradouro = "Av Paulista",
                Numero = "1000",
                Bairro = "Bela Vista",
                CodMunicipioIbge = 3550308,
                NomeMunicipio = "Sao Paulo",
                Cep = "01310100"
            }
        };

        private static DocumentoFiscal DocumentoNfeComItem()
        {
            var doc = new DocumentoFiscal(
                modelo: "55",
                ambiente: 2, // Homologação
                serie: 1,
                numero: 1024,
                total: 100.00m,
                destinatarioCnpjCpf: "12345678000199",
                destinatarioNome: "Cliente Destino S/A",
                tenantId: TenantId,
                criadoPor: UserId);

            doc.AdicionarItem(
                sku: "SKU-01",
                nomeProduto: "Teclado Mecanico",
                cst: "00",
                cfop: 5102,
                ncm: "84716052",
                quantidade: 2,
                valorUnitario: 50.00m,
                aliquotaIcms: 18.00m,
                criadoPor: UserId);

            return doc;
        }

        [Fact(DisplayName = "Transmissao | Mapper monta Venda de motor com emitente/destinatário/itens/pagamento")]
        public void Mapper_DocumentoValido_MontaVendaCompleta()
        {
            var doc = DocumentoNfeComItem();
            var contexto = ContextoValido();

            MotorVenda venda = DocumentoFiscalVendaMapper.Mapear(doc, contexto);

            Assert.NotNull(venda);
            Assert.Single(venda.Itens);
            Assert.Single(venda.Pagamentos);
            Assert.NotNull(venda.Emitente);
            Assert.NotNull(venda.Destinatario);
            Assert.Equal("11444777000161", venda.Emitente.Documento);
            // Observação: o Notifiable legado acumula avisos de tamanho de campos (validadores
            // com semântica invertida no legado); eles NÃO impedem GeraXmlDfe.ObterNf de montar a NFe.
            Assert.Empty(venda.Emitente.Notifications.Where(n => n.Key == "Documento"));
        }

        [Fact(DisplayName = "Transmissao | GeraXmlDfe.ObterInf preenche ide/emit/det a partir de DocumentoFiscal mapeado")]
        public void ObterInf_APartirDeDocumentoFiscal_PreencheCamposChave()
        {
            var doc = DocumentoNfeComItem();
            var contexto = ContextoValido();

            var venda = DocumentoFiscalVendaMapper.Mapear(doc, contexto);

            // Ato central do mapeamento domínio -> modelo NFe (ide/emit/det/total), SEM transmitir e
            // SEM assinar. Usamos ObterInf porque ObterNf chama nfe.Assina(), que exige certificado
            // real (a transmissão real fica fora do teste offline — precisa de cert/rede).
            var infNFe = MotorGeraXml.ObterInf(venda, MotorVersaoServico.Versao400);

            Assert.NotNull(infNFe);

            // ide
            Assert.NotNull(infNFe.ide);
            Assert.Equal(MotorModeloDocumento.NFe, infNFe.ide.mod);
            Assert.Equal(1, infNFe.ide.serie);
            Assert.Equal(1024, infNFe.ide.nNF);

            // emit
            Assert.NotNull(infNFe.emit);
            Assert.Equal("11444777000161", infNFe.emit.CNPJ);
            Assert.Equal("Empresa Emitente Teste LTDA", infNFe.emit.xNome);
            Assert.NotNull(infNFe.emit.enderEmit);

            // det (itens)
            Assert.NotNull(infNFe.det);
            Assert.Single(infNFe.det);
            var det = infNFe.det.First();
            Assert.Equal("SKU-01", det.prod.cProd);
            Assert.Equal("84716052", det.prod.NCM);
            Assert.Equal(5102, det.prod.CFOP);
            Assert.NotNull(det.imposto);

            // total apurado pelo motor
            Assert.NotNull(infNFe.total);
            Assert.True(infNFe.total.ICMSTot.vProd > 0);
        }

        [Fact(DisplayName = "Transmissao | Sem emitente/certificado o adapter NÃO simula autorização (Sucesso=false, sem chave)")]
        public async System.Threading.Tasks.Task EmitirAsync_SemEmitenteConfigurado_DegradaControlado()
        {
            var provider = new EmitenteFiscalProviderNaoConfigurado(); // sempre null
            var service = new MotorLegadoFiscalService(provider, new HerculesConfiguracaoFactory());

            var doc = DocumentoNfeComItem();
            var retorno = await service.EmitirAsync(doc);

            Assert.False(retorno.Sucesso);
            Assert.Equal(0, retorno.StatusSefaz); // nunca 100 fake
            Assert.True(string.IsNullOrEmpty(retorno.ChaveAcesso));
            Assert.Contains("não configurado", retorno.Motivo, System.StringComparison.OrdinalIgnoreCase);
        }
    }
}
