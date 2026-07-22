using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Microsoft.EntityFrameworkCore;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Modules.Fiscal.Domain.Entities;
using Epros.Modules.Fiscal.Application.Commands;
using Epros.Modules.Fiscal.Application.Handlers;
using Epros.Modules.Fiscal.Application.Services;
using Epros.Modules.Fiscal.Infrastructure.Data;
using Epros.Modules.Fiscal.Infrastructure.Services;
using FluentValidation.TestHelper;

namespace Epros.Tests
{
    public class FiscalTests
    {
        #region Testes de Domínio (Entidades e Invariantes)

        [Fact]
        public void Deve_Criar_Documento_Fiscal_Valido()
        {
            // Arrange & Act
            var doc = new DocumentoFiscal(
                modelo: "55",
                ambiente: 2,
                serie: 1,
                numero: 1024,
                total: 250.50m,
                destinatarioCnpjCpf: "12345678000199",
                destinatarioNome: "Cliente Destino S/A",
                tenantId: "tenant-123",
                criadoPor: "user-1"
            );

            // Assert
            Assert.True(doc.IsValid);
            Assert.Equal("55", doc.Modelo);
            Assert.Equal(2, doc.Ambiente);
            Assert.Equal(1, doc.Serie);
            Assert.Equal(1024, doc.Numero);
            Assert.Equal(250.50m, doc.Total);
            Assert.Equal("12345678000199", doc.DestinatarioCnpjCpf);
            Assert.Equal("Cliente Destino S/A", doc.DestinatarioNome);
            Assert.Equal("Rascunho", doc.Status);
        }

        [Fact]
        public void Nao_Deve_Permitir_Documento_Fiscal_Com_Modelo_Invalido()
        {
            // Arrange & Act
            var doc = new DocumentoFiscal(
                modelo: "99", // Modelo inválido (apenas 55 ou 65)
                ambiente: 2,
                serie: 1,
                numero: 1024,
                total: 250.50m,
                destinatarioCnpjCpf: "12345678000199",
                destinatarioNome: "Cliente Destino S/A",
                tenantId: "tenant-123",
                criadoPor: "user-1"
            );

            // Assert
            Assert.False(doc.IsValid);
            Assert.Contains(doc.Notifications, n => n.Key == "Modelo");
        }

        [Fact]
        public void Nao_Deve_Permitir_Documento_Fiscal_Com_Total_Negativo()
        {
            // Arrange & Act
            var doc = new DocumentoFiscal(
                modelo: "55",
                ambiente: 1,
                serie: 1,
                numero: 1024,
                total: -50.00m, // Negativo
                destinatarioCnpjCpf: "12345678000199",
                destinatarioNome: "Cliente Destino S/A",
                tenantId: "tenant-123",
                criadoPor: "user-1"
            );

            // Assert
            Assert.False(doc.IsValid);
            Assert.Contains(doc.Notifications, n => n.Key == "Total");
        }

        [Fact]
        public void Deve_Adicionar_Itens_Ao_Documento_E_Calcular_Total_E_Icms()
        {
            // Arrange
            var doc = new DocumentoFiscal("55", 2, 1, 100, 100.00m, "12345678000199", "Cliente Destino S/A", "tenant-123", "user-1");

            // Act
            doc.AdicionarItem(
                sku: "SKU-UNIT-01",
                nomeProduto: "Teclado Mecanico",
                cst: "000",
                cfop: 5102,
                ncm: "84716052",
                quantidade: 2,
                valorUnitario: 50.00m,
                aliquotaIcms: 18.00m,
                criadoPor: "user-1"
            );

            // Assert
            Assert.True(doc.IsValid);
            Assert.Single(doc.Itens);
            
            var item = doc.Itens.First();
            Assert.Equal("SKU-UNIT-01", item.Sku);
            Assert.Equal(100.00m, item.ValorTotal); // 2 * 50
            Assert.Equal(18.00m, item.ValorIcms);  // 100 * 0.18
        }

        [Fact]
        public void Nao_Deve_Permitir_Item_Com_Quantidade_Zero_Ou_Negativa()
        {
            // Arrange
            var doc = new DocumentoFiscal("55", 2, 1, 100, 100.00m, "12345678000199", "Cliente Destino S/A", "tenant-123", "user-1");

            // Act
            doc.AdicionarItem("SKU-UNIT-01", "Teclado Mecanico", "000", 5102, "84716052", 0m, 50.00m, 18.00m, "user-1");

            // Assert
            Assert.False(doc.IsValid);
            Assert.Empty(doc.Itens);
            Assert.Contains(doc.Notifications, n => n.Key == "Quantidade");
        }

        [Fact]
        public void Deve_Gerar_Pdf_Danfe_NFe_Real()
        {
            // Arrange: documento NF-e autorizado com 1 item.
            var doc = new DocumentoFiscal("55", 2, 1, 1234, 100.00m, "12345678000199", "Cliente Destino S/A", "tenant-123", "user-1");
            doc.AdicionarItem("SKU-01", "Teclado Mecanico", "00", 5102, "84716052", 2, 50.00m, 18.00m, "user-1");
            doc.Submeter();
            doc.Autorizar("35260612345678000199550010000012341000012340", "135260000012345", 100, "<nfe/>", "<nfeProc/>", "", "");

            var service = new DanfeQuestPdfService();

            // Act
            var pdf = service.GerarPdf(doc);

            // Assert: PDF real (assinatura %PDF- no início) e não vazio.
            Assert.NotNull(pdf);
            Assert.True(pdf.Length > 500, "PDF gerado muito pequeno.");
            var header = System.Text.Encoding.ASCII.GetString(pdf, 0, 5);
            Assert.Equal("%PDF-", header);
        }

        [Fact]
        public void Deve_Gerar_Pdf_Cupom_NFCe_Real()
        {
            // Arrange: NFC-e (modelo 65) em rascunho -> sai marcado SEM VALOR FISCAL, mas é PDF válido.
            var doc = new DocumentoFiscal("65", 2, 1, 55, 45.00m, "00000000000", "Consumidor Final", "tenant-123", "user-1");
            doc.AdicionarItem("SKU-02", "Caneta", "102", 5102, "96081000", 3, 15.00m, 0m, "user-1");

            var service = new DanfeQuestPdfService();

            // Act
            var pdf = service.GerarPdf(doc);

            // Assert
            Assert.NotNull(pdf);
            Assert.True(pdf.Length > 300);
            Assert.Equal("%PDF-", System.Text.Encoding.ASCII.GetString(pdf, 0, 5));
        }

        [Fact]
        public async Task Seeder_Deve_Popular_Cfop_E_CstIbsCbs_De_Forma_Idempotente()
        {
            // Arrange
            var context = CreateInMemoryContext("db_seed_catalogos", "tenant-123", "user-1");

            // Act 1: primeira execução popula os catálogos
            await CatalogoFiscalSeeder.SeedAsync(context);

            var totalCfop = await context.Cfops.CountAsync();
            var totalCst = await context.CstsIbsCbs.CountAsync();

            // Assert 1: CFOP em massa (621 códigos) e os 4 CSTs IBS/CBS.
            Assert.True(totalCfop > 600, $"Esperava >600 CFOPs, veio {totalCfop}.");
            Assert.Equal(4, totalCst);
            Assert.Contains(await context.Cfops.ToListAsync(), c => c.CfopCodigo == 5102);

            // Act 2: reexecução não duplica (idempotência)
            await CatalogoFiscalSeeder.SeedAsync(context);

            // Assert 2
            Assert.Equal(totalCfop, await context.Cfops.CountAsync());
            Assert.Equal(totalCst, await context.CstsIbsCbs.CountAsync());
        }

        #endregion

        #region Testes de Validadores de Commando (FluentValidation)

        [Fact]
        public void Validar_Comando_Cancelar_Deve_Falhar_Com_Justificativa_Curta()
        {
            // Arrange
            var validator = new CancelarDocumentoFiscalCommandValidator();
            var command = new CancelarDocumentoFiscalCommand(Guid.NewGuid(), "Curta"); // Menos que 15 chars

            // Act
            var result = validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(c => c.Justificativa)
                .WithErrorMessage("A justificativa de cancelamento deve possuir no mínimo 15 caracteres.");
        }

        [Fact]
        public void Validar_Comando_Cancelar_Deve_Passar_Com_Justificativa_Valida()
        {
            // Arrange
            var validator = new CancelarDocumentoFiscalCommandValidator();
            var command = new CancelarDocumentoFiscalCommand(Guid.NewGuid(), "Justificativa valida com mais de quinze caracteres");

            // Act
            var result = validator.TestValidate(command);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        #endregion

        #region Testes de Handlers (CQRS)

        [Fact]
        public async Task Deve_Emitir_Documento_Fiscal_Com_Sucesso_E_Gravar_Outbox()
        {
            // Arrange
            var tenantProvider = new TestTenantProvider("tenant-123");
            var currentUser = new TestCurrentUser("user-1");
            var context = CreateInMemoryContext("db_emitir_sucesso", "tenant-123", "user-1");
            var fiscalService = new TestHerculesFiscalService
            {
                Sucesso = true,
                StatusSefaz = 100,
                Motivo = "Autorizado o uso da NF-e",
                ChaveAcesso = "35260612345678000199550010000001001000001000"
            };

            var handler = new EmitirDocumentoFiscalCommandHandler(context, tenantProvider, currentUser, fiscalService, Calculadora(CalculoNeutro(), EmitenteNaoConfigurado()), new DanfeQuestPdfService(), new ArmazenamentoArquivoFiscalLocal());

            var itensDto = new List<EmitirDocumentoFiscalItemDto>
            {
                new EmitirDocumentoFiscalItemDto("SKU-01", "Produto 1", "000", 5102, "84716052", 2, 50.00m, 18.00m)
            };

            var command = new EmitirDocumentoFiscalCommand(
                Modelo: "55",
                Ambiente: 2,
                Serie: 1,
                Numero: 100,
                DestinatarioCnpjCpf: "12345678000199",
                DestinatarioNome: "Cliente Destino S/A",
                Total: 100.00m,
                Itens: itensDto
            );

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.Sucesso);
            Assert.NotNull(result.Dados);

            // Verificar se salvou no banco
            var docSalvo = await context.DocumentosFiscais.Include(d => d.Itens).FirstOrDefaultAsync();
            Assert.NotNull(docSalvo);
            Assert.Equal("Autorizado", docSalvo.Status);
            Assert.Equal("35260612345678000199550010000001001000001000", docSalvo.ChaveAcesso);
            Assert.Equal(100, docSalvo.StatusSefaz);
            Assert.Single(docSalvo.Itens);

            // Verificar outbox
            var outboxMsg = await context.OutboxMessages.FirstOrDefaultAsync();
            Assert.NotNull(outboxMsg);
            Assert.Equal("DocumentoFiscalAutorizado", outboxMsg.EventType);
            Assert.Contains("35260612345678000199550010000001001000001000", outboxMsg.Payload);
            Assert.Equal("tenant-123", outboxMsg.TenantId);
        }

        [Fact(DisplayName = "EmitirDocumentoFiscal | item sem imposto | cálculo do provider é aplicado no item")]
        public async Task Deve_Plugar_Calculo_Fiscal_E_Aplicar_Impostos_No_Item()
        {
            // Arrange: mock de cálculo devolve impostos não-zero; item nasce sem impostos calculados.
            var tenantProvider = new TestTenantProvider("tenant-123");
            var currentUser = new TestCurrentUser("user-1");
            var context = CreateInMemoryContext("db_wiring_calculo", "tenant-123", "user-1");
            var fiscalService = new TestHerculesFiscalService { Sucesso = true, StatusSefaz = 100 };

            var calculo = new FakeCalculoFiscalService
            {
                ResultadoFixo = new CalculoFiscalResultado
                {
                    Sucesso = true,
                    Icms = new CalculoImpostoIcms { BaseCalculo = 100m, Aliquota = 18m, Valor = 18m, ValorSt = 5m, ValorFcp = 2m },
                    Ipi = new CalculoImpostoIpi { Valor = 10m },
                    Pis = new CalculoImpostoPisCofins { Valor = 1.65m },
                    Cofins = new CalculoImpostoPisCofins { Valor = 7.60m }
                }
            };
            // Provider com emitente configurado (regime normal, UF SP) para o handler montar a requisição.
            var emitente = new FakeEmitenteFiscalProvider
            {
                Contexto = new ContextoTransmissaoFiscal
                {
                    Emitente = new EmitenteFiscalDto { RegimeTributario = 3, Uf = "SP" }
                }
            };

            var handler = new EmitirDocumentoFiscalCommandHandler(context, tenantProvider, currentUser, fiscalService, Calculadora(calculo, emitente), new DanfeQuestPdfService(), new ArmazenamentoArquivoFiscalLocal());

            var command = new EmitirDocumentoFiscalCommand(
                Modelo: "55",
                Ambiente: 2,
                Serie: 1,
                Numero: 500,
                DestinatarioCnpjCpf: "12345678000199",
                DestinatarioNome: "Cliente Destino S/A",
                Total: 100.00m,
                Itens: new List<EmitirDocumentoFiscalItemDto>
                {
                    new EmitirDocumentoFiscalItemDto("SKU-01", "Produto 1", "00", 5102, "84716052", 1, 100.00m, 0m)
                });

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.Sucesso);
            Assert.Single(calculo.RequisicoesRecebidas);
            Assert.Equal(3, calculo.RequisicoesRecebidas[0].RegimeTributario);
            Assert.Equal("SP", calculo.RequisicoesRecebidas[0].UfOrigem);

            var docSalvo = await context.DocumentosFiscais.Include(d => d.Itens).FirstOrDefaultAsync();
            Assert.NotNull(docSalvo);
            var item = Assert.Single(docSalvo!.Itens);
            Assert.True(item.ImpostosCalculados);
            Assert.Equal(18m, item.ValorIcms);
            Assert.Equal(5m, item.ValorIcmsSt);
            Assert.Equal(2m, item.ValorFcp);
            Assert.Equal(10m, item.ValorIpi);
            Assert.Equal(1.65m, item.ValorPis);
            Assert.Equal(7.60m, item.ValorCofins);
        }

        [Fact]
        public async Task Deve_Rejeitar_Documento_Fiscal_Se_Sefaz_Retornar_Erro()
        {
            // Arrange
            var tenantProvider = new TestTenantProvider("tenant-123");
            var currentUser = new TestCurrentUser("user-1");
            var context = CreateInMemoryContext("db_emitir_rejeicao", "tenant-123", "user-1");
            
            var fiscalService = new TestHerculesFiscalService
            {
                Sucesso = false,
                StatusSefaz = 204, // Rejeição duplicidade
                Motivo = "Rejeicao: Duplicidade de NF-e"
            };

            var handler = new EmitirDocumentoFiscalCommandHandler(context, tenantProvider, currentUser, fiscalService, Calculadora(CalculoNeutro(), EmitenteNaoConfigurado()), new DanfeQuestPdfService(), new ArmazenamentoArquivoFiscalLocal());

            var itensDto = new List<EmitirDocumentoFiscalItemDto>
            {
                new EmitirDocumentoFiscalItemDto("SKU-01", "Produto 1", "000", 5102, "84716052", 1, 100.00m, 18.00m)
            };

            var command = new EmitirDocumentoFiscalCommand(
                Modelo: "55",
                Ambiente: 2,
                Serie: 1,
                Numero: 101,
                DestinatarioCnpjCpf: "12345678000199",
                DestinatarioNome: "Cliente Destino S/A",
                Total: 100.00m,
                Itens: itensDto
            );

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.Sucesso);
            Assert.Contains(result.Erros, e => e.Contains("Rejeicao"));

            var docSalvo = await context.DocumentosFiscais.FirstOrDefaultAsync();
            Assert.NotNull(docSalvo);
            Assert.Equal("Rejeitado", docSalvo.Status);
            Assert.Equal(204, docSalvo.StatusSefaz);
            Assert.Equal("Rejeicao: Duplicidade de NF-e", docSalvo.MotivoRejeicaoSefaz);

            // Outbox não deve ser criado para rejeição
            var outboxMsg = await context.OutboxMessages.FirstOrDefaultAsync();
            Assert.Null(outboxMsg);
        }

        [Fact]
        public async Task Deve_Cancelar_Documento_Fiscal_Autorizado_Com_Sucesso()
        {
            // Arrange
            var tenantProvider = new TestTenantProvider("tenant-123");
            var currentUser = new TestCurrentUser("user-1");
            var context = CreateInMemoryContext("db_cancelar_sucesso", "tenant-123", "user-1");

            var doc = new DocumentoFiscal("55", 2, 1, 100, 100.00m, "12345678000199", "Cliente Destino S/A", "tenant-123", "user-1");
            doc.Submeter();
            doc.Autorizar(
                chaveAcesso: "35260612345678000199550010000001001000001000",
                protocolo: "135260000012345",
                statusSefaz: 100,
                xmlEnvio: "<xmlEnvio />",
                xmlRetorno: "<xmlRetorno />",
                pdfCaminho: "/pdf.pdf",
                xmlCaminho: "/xml.xml"
            );
            context.DocumentosFiscais.Add(doc);
            await context.SaveChangesAsync();

            var fiscalService = new TestHerculesFiscalService
            {
                Sucesso = true,
                StatusSefaz = 135, // Homologado cancelamento
                Motivo = "Cancelamento de NF-e homologado"
            };

            var handler = new CancelarDocumentoFiscalCommandHandler(context, tenantProvider, currentUser, fiscalService);
            var command = new CancelarDocumentoFiscalCommand(doc.Id, "Justificativa valida de cancelamento com mais de 15 caracteres");

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.Sucesso);

            // Verificar se mudou status para Cancelada
            var docSalvo = await context.DocumentosFiscais.FirstOrDefaultAsync(d => d.Id == doc.Id);
            Assert.NotNull(docSalvo);
            Assert.Equal("Cancelada", docSalvo.Status);
            Assert.NotNull(docSalvo.DataCancelamento);

            // Verificar se gerou evento fiscal de cancelamento
            var evento = await context.EventosDocumentosFiscais.FirstOrDefaultAsync(e => e.DocumentoFiscalId == doc.Id);
            Assert.NotNull(evento);
            Assert.Equal("Cancelamento", evento.TipoEvento);
            Assert.Equal(135, evento.StatusSefaz);
            Assert.Equal("Justificativa valida de cancelamento com mais de 15 caracteres", evento.XCorrecao); // Salvo como justificativa
        }

        [Fact]
        public async Task Nao_Deve_Cancelar_Se_Documento_Nao_Estiver_Autorizado()
        {
            // Arrange
            var tenantProvider = new TestTenantProvider("tenant-123");
            var currentUser = new TestCurrentUser("user-1");
            var context = CreateInMemoryContext("db_cancelar_falha_status", "tenant-123", "user-1");

            var doc = new DocumentoFiscal("55", 2, 1, 100, 100.00m, "12345678000199", "Cliente Destino S/A", "tenant-123", "user-1");
            // Mantém status Rascunho
            context.DocumentosFiscais.Add(doc);
            await context.SaveChangesAsync();

            var fiscalService = new TestHerculesFiscalService();
            var handler = new CancelarDocumentoFiscalCommandHandler(context, tenantProvider, currentUser, fiscalService);
            var command = new CancelarDocumentoFiscalCommand(doc.Id, "Justificativa valida de cancelamento com mais de 15 caracteres");

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.Sucesso);
            Assert.Contains(result.Erros, e => e.Contains("Apenas documentos com status 'Autorizado' podem ser cancelados"));

            var docSalvo = await context.DocumentosFiscais.FirstOrDefaultAsync(d => d.Id == doc.Id);
            Assert.NotNull(docSalvo);
            Assert.Equal("Rascunho", docSalvo.Status); // Não deve mudar
        }

        #endregion

        #region Helpers e Doubles de Teste

        private ContextFiscal CreateInMemoryContext(string databaseName, string tenantId, string userId)
        {
            var options = new DbContextOptionsBuilder<ContextFiscal>()
                .UseInMemoryDatabase(databaseName)
                .Options;

            var tenantProvider = new TestTenantProvider(tenantId);
            var currentUser = new TestCurrentUser(userId);

            return new ContextFiscal(options, tenantProvider, currentUser);
        }

        private class TestTenantProvider : ITenantProvider
        {
            private readonly string _tenantId;
            public TestTenantProvider(string tenantId) => _tenantId = tenantId;
            public string GetTenantId() => _tenantId;
        }

        private class TestCurrentUser : ICurrentUser
        {
            private readonly string _userId;
            public TestCurrentUser(string userId) => _userId = userId;
            public string? GetUserId() => _userId;
            public string? GetUserName() => "Test User";
            public string? GetUserEmail() => "test@epros.com";
        }

        private class TestHerculesFiscalService : IHerculesFiscalService
        {
            public bool Sucesso { get; set; } = true;
            public int StatusSefaz { get; set; } = 100;
            public string Motivo { get; set; } = "Autorizado o uso da NF-e";
            public string ChaveAcesso { get; set; } = "35260600000000000000550010000000011000000000";
            public string Protocolo { get; set; } = "135260000000000";

            public Task<RetornoEmissaoDto> EmitirAsync(DocumentoFiscal documento)
            {
                return Task.FromResult(new RetornoEmissaoDto
                {
                    Sucesso = Sucesso,
                    ChaveAcesso = ChaveAcesso,
                    Protocolo = Protocolo,
                    StatusSefaz = StatusSefaz,
                    Motivo = Motivo,
                    XmlEnvio = "<xmlEnvio />",
                    XmlRetorno = $"<xmlRetorno><cStat>{StatusSefaz}</cStat><xMotivo>{Motivo}</xMotivo></xmlRetorno>",
                    PdfCaminho = $"/Danfes/{documento.TenantId}/{ChaveAcesso}.pdf",
                    XmlCaminho = $"/Xmls/{documento.TenantId}/{ChaveAcesso}.xml"
                });
            }

            public Task<RetornoCancelamentoDto> CancelarAsync(DocumentoFiscal documento, string justificativa)
            {
                return Task.FromResult(new RetornoCancelamentoDto
                {
                    Sucesso = Sucesso,
                    StatusSefaz = StatusSefaz == 100 ? 135 : StatusSefaz,
                    Motivo = Motivo == "Autorizado o uso da NF-e" ? "Cancelamento homologado" : Motivo,
                    Protocolo = Protocolo,
                    XmlRetorno = $"<xmlRetorno><cStat>{StatusSefaz}</cStat></xmlRetorno>"
                });
            }

            public Task<RetornoEventoDto> CartaCorrecaoAsync(DocumentoFiscal documento, string textoCorrecao, int sequenciaEvento)
            {
                return Task.FromResult(new RetornoEventoDto
                {
                    Sucesso = Sucesso,
                    StatusSefaz = 135,
                    Motivo = "Evento registrado e vinculado a NF-e",
                    XmlRetorno = "<xmlRetorno><cStat>135</cStat></xmlRetorno>"
                });
            }

            public Task<RetornoInutilizacaoDto> InutilizarAsync(InutilizacaoFiscalRequest request)
            {
                return Task.FromResult(new RetornoInutilizacaoDto
                {
                    Sucesso = Sucesso,
                    StatusSefaz = 102,
                    Motivo = "Inutilizacao de numero homologada",
                    Protocolo = Protocolo,
                    XmlRetorno = "<xmlRetorno><cStat>102</cStat></xmlRetorno>"
                });
            }

            public Task<RetornoConsultaSefazDto> VerificarStatusServicoAsync(ConsultaStatusServicoRequest request)
            {
                return Task.FromResult(new RetornoConsultaSefazDto
                {
                    Sucesso = Sucesso,
                    StatusSefaz = 107,
                    Motivo = "Servico em Operacao",
                    XmlRetorno = "<xmlRetorno><cStat>107</cStat></xmlRetorno>"
                });
            }

            public Task<RetornoConsultaSefazDto> ConsultarProtocoloAsync(ConsultaProtocoloRequest request)
            {
                return Task.FromResult(new RetornoConsultaSefazDto
                {
                    Sucesso = Sucesso,
                    StatusSefaz = StatusSefaz,
                    Motivo = Motivo,
                    XmlRetorno = $"<xmlRetorno><cStat>{StatusSefaz}</cStat><xMotivo>{Motivo}</xMotivo></xmlRetorno>"
                });
            }
        }

        /// <summary>Double de cálculo: devolve valores fixos de imposto, registrando as requisições recebidas.</summary>
        private class FakeCalculoFiscalService : ICalculoFiscalService
        {
            public List<CalculoFiscalRequest> RequisicoesRecebidas { get; } = new();
            public CalculoFiscalResultado ResultadoFixo { get; set; } = new();

            public CalculoFiscalResultado Calcular(CalculoFiscalRequest request)
            {
                RequisicoesRecebidas.Add(request);
                return ResultadoFixo;
            }
        }

        /// <summary>Provider de emitente configurável; null por padrão (sem emitente configurado).</summary>
        private class FakeEmitenteFiscalProvider : IEmitenteFiscalProvider
        {
            public ContextoTransmissaoFiscal? Contexto { get; set; }
            public ContextoTransmissaoFiscal? ObterContexto(DocumentoFiscal documento) => Contexto;
            public ContextoTransmissaoFiscal? ObterContextoPorDocumento(string documentoEmitente, string modelo, int ambiente) => Contexto;
        }

        private static ICalculoFiscalService CalculoNeutro() => new FakeCalculoFiscalService();
        private static IEmitenteFiscalProvider EmitenteNaoConfigurado() => new FakeEmitenteFiscalProvider();

        private static CalculadoraImpostosDocumentoFiscal Calculadora(ICalculoFiscalService calculo, IEmitenteFiscalProvider emitente)
            => new CalculadoraImpostosDocumentoFiscal(calculo, emitente);

        #endregion
    }
}
