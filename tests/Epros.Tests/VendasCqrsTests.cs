using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MediatR;
using Epros.Modules.Vendas.Application.Commands;
using Epros.Modules.Vendas.Application.Handlers;
using Epros.Modules.Vendas.Application.Queries;
using Epros.Modules.Vendas.Domain.Enums;
using Epros.Modules.Vendas.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Domain.Enums;
using Epros.Tests.Integration;
using Xunit;

namespace Epros.Tests
{
    public class VendasCqrsTests
    {
        private ContextVendas CreateInMemoryContext(string databaseName, string tenantId, string userId)
        {
            var options = new DbContextOptionsBuilder<ContextVendas>()
                .UseInMemoryDatabase(databaseName)
                .Options;

            var tenantProvider = new TestTenantProvider(tenantId);
            var currentUser = new TestCurrentUser(userId);

            return new ContextVendas(options, tenantProvider, currentUser);
        }

        [Fact]
        public async Task Deve_Abrir_Caixa_Registrar_Venda_E_Obter_Status_Com_Sucesso()
        {
            // Arrange
            var context = CreateInMemoryContext("db_venda_test", "tenant-1", "user-1");
            var tenantProvider = new TestTenantProvider("tenant-1");
            var currentUser = new TestCurrentUser("user-1");
            var fakeMediator = new FakeMediator();

            var abrirHandler = new AbrirCaixaCommandHandler(context, tenantProvider, currentUser);
            var fecharHandler = new FecharCaixaCommandHandler(context, tenantProvider, currentUser);
            var registrarHandler = new RegistrarVendaCommandHandler(context, tenantProvider, currentUser, fakeMediator);

            var caixaStatusQueryHandler = new ObterCaixaStatusQueryHandler(context, tenantProvider);
            var caixaDetalhadoQueryHandler = new ObterCaixaDetalhadoQueryHandler(context);
            var listVendasQueryHandler = new ListarVendasQueryHandler(context, tenantProvider);
            var getVendaQueryHandler = new ObterVendaPorIdQueryHandler(context);

            // 1. Abrir Caixa
            var abrirCmd = new AbrirCaixaCommand("operador-1", 100.00m);
            var abrirResult = await abrirHandler.Handle(abrirCmd, CancellationToken.None);
            Assert.True(abrirResult.Sucesso);
            var caixaId = (Guid)abrirResult.Dados!.GetType().GetProperty("CaixaId")!.GetValue(abrirResult.Dados)!;

            // 2. Verificar Status do Caixa (Deve estar Aberto)
            var statusResult = await caixaStatusQueryHandler.Handle(new ObterCaixaStatusQuery("operador-1"), CancellationToken.None);
            Assert.True(statusResult.Sucesso);
            var statusDados = statusResult.Dados!;
            var status = (string)statusDados.GetType().GetProperty("Status")!.GetValue(statusDados)!;
            Assert.Equal("Aberto", status);

            // 3. Registrar Venda
            var registrarCmd = new RegistrarVendaCommand(
                CaixaId: caixaId.ToString(),
                Total: 50.00m,
                Status: "Emitida",
                ModeloFiscal: EModeloDocumento.NFCe,
                NaturezaOperacao: "5102",
                DataVenda: DateTime.UtcNow,
                InformacoesComplementares: "Venda teste PDV",
                InformacoesAdicionaisFisco: null,
                ModalidadeFrete: EModalidadeFrete.mfSemFrete,
                VendaOrigem: EVendaOrigem.NfeSimplificada,
                IncluirFreteNoTotal: false,
                ClienteId: null,
                ValorDesconto: 0m,
                ValorFrete: 0m,
                FormaPagamento: "Dinheiro",
                Itens: new List<VendaItemInput>
                {
                    new VendaItemInput(
                        ProdutoId: Guid.NewGuid(),
                        Quantidade: 2m,
                        PrecoUnitario: 25.00m,
                        CodigoProduto: "SKU-V1",
                        CodigoEan: "7891111111112",
                        DescricaoProduto: "Produto Venda Teste",
                        Ncm: "22030000",
                        CestId: null,
                        Cest: null,
                        CodigoAnpId: null,
                        CodigoAnp: null,
                        Cfop: 5102,
                        UnidadeComercial: "UN"
                    )
                }
            );

            var registrarResult = await registrarHandler.Handle(registrarCmd, CancellationToken.None);
            Assert.True(registrarResult.Sucesso);
            var vendaId = (Guid)registrarResult.Dados!.GetType().GetProperty("VendaId")!.GetValue(registrarResult.Dados)!;

            // Assert - Verificar se o Mediator publicou o evento VendaFaturadaEventNotification
            Assert.Single(fakeMediator.PublishedEvents);
            var publishedEvent = fakeMediator.PublishedEvents.First();
            Assert.IsType<Epros.Shared.Domain.Events.VendaFaturadaEventNotification>(publishedEvent);

            // 4. Obter Caixa Detalhado (Deve ter faturamento em Dinheiro e saldo atualizado)
            var detalhadoResult = await caixaDetalhadoQueryHandler.Handle(new ObterCaixaDetalhadoQuery(caixaId), CancellationToken.None);
            Assert.True(detalhadoResult.Sucesso);
            var detalhadoDados = detalhadoResult.Dados!;
            var saldoCalculado = (decimal)detalhadoDados.GetType().GetProperty("SaldoCalculadoDinheiro")!.GetValue(detalhadoDados)!;
            Assert.Equal(150.00m, saldoCalculado); // 100 Abertura + 50 Venda Dinheiro

            // 5. Listar Vendas
            var listResult = await listVendasQueryHandler.Handle(new ListarVendasQuery(null, 1, 10), CancellationToken.None);
            Assert.True(listResult.Sucesso);
            var listDados = listResult.Dados!;
            var totalVendas = (int)listDados.GetType().GetProperty("Total")!.GetValue(listDados)!;
            Assert.Equal(1, totalVendas);

            // 6. Obter Venda Por Id
            var getVendaResult = await getVendaQueryHandler.Handle(new ObterVendaPorIdQuery(vendaId), CancellationToken.None);
            Assert.True(getVendaResult.Sucesso);

            // 7. Fechar Caixa
            var fecharCmd = new FecharCaixaCommand(caixaId, 150.00m);
            var fecharResult = await fecharHandler.Handle(fecharCmd, CancellationToken.None);
            Assert.True(fecharResult.Sucesso);
        }

        // -----------------------------------------------------------------
        // Helpers
        // -----------------------------------------------------------------

        private static List<VendaItemInput> UmItemValido(decimal quantidade = 2m, decimal precoUnitario = 25.00m)
            => new()
            {
                new VendaItemInput(
                    ProdutoId: Guid.NewGuid(),
                    Quantidade: quantidade,
                    PrecoUnitario: precoUnitario,
                    CodigoProduto: "SKU-V1",
                    CodigoEan: "7891111111112",
                    DescricaoProduto: "Produto Venda Teste",
                    Ncm: "22030000",
                    CestId: null,
                    Cest: null,
                    CodigoAnpId: null,
                    CodigoAnp: null,
                    Cfop: 5102,
                    UnidadeComercial: "UN")
            };

        private RegistrarVendaCommand NovaVendaCmd(string caixaId, string status, List<VendaItemInput> itens, decimal total = 50.00m)
            => new RegistrarVendaCommand(
                CaixaId: caixaId,
                Total: total,
                Status: status,
                ModeloFiscal: EModeloDocumento.NFCe,
                NaturezaOperacao: "5102",
                DataVenda: DateTime.UtcNow,
                InformacoesComplementares: "Venda teste PDV",
                InformacoesAdicionaisFisco: null,
                ModalidadeFrete: EModalidadeFrete.mfSemFrete,
                VendaOrigem: EVendaOrigem.NfeSimplificada,
                IncluirFreteNoTotal: false,
                ClienteId: null,
                ValorDesconto: 0m,
                ValorFrete: 0m,
                FormaPagamento: "Dinheiro",
                Itens: itens);

        private async Task<Guid> AbrirCaixaAsync(ContextVendas context, TestTenantProvider tenant, TestCurrentUser user, string operador = "operador-1")
        {
            var abrirHandler = new AbrirCaixaCommandHandler(context, tenant, user);
            var abrirResult = await abrirHandler.Handle(new AbrirCaixaCommand(operador, 100.00m), CancellationToken.None);
            Assert.True(abrirResult.Sucesso);
            return (Guid)abrirResult.Dados!.GetType().GetProperty("CaixaId")!.GetValue(abrirResult.Dados)!;
        }

        // -----------------------------------------------------------------
        // RegistrarVenda — regras de rejeição / evento
        // -----------------------------------------------------------------

        [Fact]
        public async Task RegistrarVenda_Deve_Rejeitar_Quando_Caixa_Nao_Existe()
        {
            var context = CreateInMemoryContext(Guid.NewGuid().ToString(), "tenant-1", "user-1");
            var tenant = new TestTenantProvider("tenant-1");
            var user = new TestCurrentUser("user-1");
            var handler = new RegistrarVendaCommandHandler(context, tenant, user, new FakeMediator());

            var cmd = NovaVendaCmd(Guid.NewGuid().ToString(), "Emitida", UmItemValido());
            var result = await handler.Handle(cmd, CancellationToken.None);

            Assert.False(result.Sucesso);
            Assert.Contains("Caixa não encontrado.", result.Erros);
            Assert.Empty(context.Vendas);
        }

        [Fact]
        public async Task RegistrarVenda_Deve_Rejeitar_Quando_Caixa_Fechado()
        {
            var context = CreateInMemoryContext(Guid.NewGuid().ToString(), "tenant-1", "user-1");
            var tenant = new TestTenantProvider("tenant-1");
            var user = new TestCurrentUser("user-1");

            var caixaId = await AbrirCaixaAsync(context, tenant, user);
            var fecharHandler = new FecharCaixaCommandHandler(context, tenant, user);
            Assert.True((await fecharHandler.Handle(new FecharCaixaCommand(caixaId, 100.00m), CancellationToken.None)).Sucesso);

            var handler = new RegistrarVendaCommandHandler(context, tenant, user, new FakeMediator());
            var cmd = NovaVendaCmd(caixaId.ToString(), "Emitida", UmItemValido());
            var result = await handler.Handle(cmd, CancellationToken.None);

            Assert.False(result.Sucesso);
            Assert.Contains("Não é possível registrar vendas em um caixa fechado.", result.Erros);
        }

        [Fact]
        public async Task RegistrarVenda_Nao_Deve_Publicar_Evento_Quando_Status_Salvar()
        {
            var context = CreateInMemoryContext(Guid.NewGuid().ToString(), "tenant-1", "user-1");
            var tenant = new TestTenantProvider("tenant-1");
            var user = new TestCurrentUser("user-1");
            var fakeMediator = new FakeMediator();

            var caixaId = await AbrirCaixaAsync(context, tenant, user);
            var handler = new RegistrarVendaCommandHandler(context, tenant, user, fakeMediator);

            // Status "Salvar" => venda apenas gravada (não emitida) => NÃO deve faturar/publicar
            var cmd = NovaVendaCmd(caixaId.ToString(), "Salvar", UmItemValido());
            var result = await handler.Handle(cmd, CancellationToken.None);

            Assert.True(result.Sucesso);
            Assert.Empty(fakeMediator.PublishedEvents);
            Assert.Single(context.Vendas);
        }

        [Fact]
        public async Task RegistrarVenda_Deve_Publicar_Evento_Faturamento_Quando_Emitida()
        {
            var context = CreateInMemoryContext(Guid.NewGuid().ToString(), "tenant-1", "user-1");
            var tenant = new TestTenantProvider("tenant-1");
            var user = new TestCurrentUser("user-1");
            var fakeMediator = new FakeMediator();

            var caixaId = await AbrirCaixaAsync(context, tenant, user);
            var handler = new RegistrarVendaCommandHandler(context, tenant, user, fakeMediator);

            var cmd = NovaVendaCmd(caixaId.ToString(), "Emitida", UmItemValido());
            var result = await handler.Handle(cmd, CancellationToken.None);

            Assert.True(result.Sucesso);
            var evt = Assert.Single(fakeMediator.PublishedEvents);
            var notif = Assert.IsType<Epros.Shared.Domain.Events.VendaFaturadaEventNotification>(evt);
            Assert.Equal("tenant-1", notif.TenantId);
            Assert.Equal(50.00m, notif.Total);
            Assert.Single(notif.Itens);
        }

        // -----------------------------------------------------------------
        // CancelarVenda — regras
        // -----------------------------------------------------------------

        [Fact]
        public async Task CancelarVenda_Deve_Rejeitar_Quando_Venda_Nao_Existe()
        {
            var context = CreateInMemoryContext(Guid.NewGuid().ToString(), "tenant-1", "user-1");
            var tenant = new TestTenantProvider("tenant-1");
            var user = new TestCurrentUser("user-1");
            var handler = new CancelarVendaCommandHandler(context, tenant, user);

            var result = await handler.Handle(new CancelarVendaCommand(Guid.NewGuid(), "engano"), CancellationToken.None);

            Assert.False(result.Sucesso);
            Assert.Contains("Venda não encontrada.", result.Erros);
        }

        [Fact]
        public async Task CancelarVenda_Deve_Cancelar_E_Gravar_Outbox_Com_Sucesso()
        {
            var dbName = Guid.NewGuid().ToString();
            var context = CreateInMemoryContext(dbName, "tenant-1", "user-1");
            var tenant = new TestTenantProvider("tenant-1");
            var user = new TestCurrentUser("user-1");

            var caixaId = await AbrirCaixaAsync(context, tenant, user);
            var registrarHandler = new RegistrarVendaCommandHandler(context, tenant, user, new FakeMediator());
            var registrarResult = await registrarHandler.Handle(
                NovaVendaCmd(caixaId.ToString(), "Emitida", UmItemValido()), CancellationToken.None);
            var vendaId = (Guid)registrarResult.Dados!.GetType().GetProperty("VendaId")!.GetValue(registrarResult.Dados)!;

            var cancelarHandler = new CancelarVendaCommandHandler(context, tenant, user);
            var result = await cancelarHandler.Handle(new CancelarVendaCommand(vendaId, "cliente desistiu"), CancellationToken.None);

            Assert.True(result.Sucesso);
            var venda = await context.Vendas.FirstAsync(v => v.Id == vendaId);
            Assert.True(venda.Cancelada);
            // Deve ter enfileirado a mensagem de integração no Outbox
            Assert.Contains(context.OutboxMessages, m => m.EventType == "VendaCancelada");
        }

        [Fact]
        public async Task CancelarVenda_Deve_Rejeitar_Quando_Ja_Cancelada()
        {
            var context = CreateInMemoryContext(Guid.NewGuid().ToString(), "tenant-1", "user-1");
            var tenant = new TestTenantProvider("tenant-1");
            var user = new TestCurrentUser("user-1");

            var caixaId = await AbrirCaixaAsync(context, tenant, user);
            var registrarHandler = new RegistrarVendaCommandHandler(context, tenant, user, new FakeMediator());
            var registrarResult = await registrarHandler.Handle(
                NovaVendaCmd(caixaId.ToString(), "Emitida", UmItemValido()), CancellationToken.None);
            var vendaId = (Guid)registrarResult.Dados!.GetType().GetProperty("VendaId")!.GetValue(registrarResult.Dados)!;

            var cancelarHandler = new CancelarVendaCommandHandler(context, tenant, user);
            Assert.True((await cancelarHandler.Handle(new CancelarVendaCommand(vendaId, "1"), CancellationToken.None)).Sucesso);

            // Segunda tentativa deve falhar
            var result = await cancelarHandler.Handle(new CancelarVendaCommand(vendaId, "2"), CancellationToken.None);
            Assert.False(result.Sucesso);
            Assert.Contains("Esta venda já se encontra cancelada.", result.Erros);
        }

        // -----------------------------------------------------------------
        // VendaFiscalHandlers — CriarVendaFiscal
        // -----------------------------------------------------------------

        [Fact]
        public async Task CriarVendaFiscal_Deve_Criar_Cabecalho_Com_Sucesso()
        {
            var context = CreateInMemoryContext(Guid.NewGuid().ToString(), "tenant-1", "user-1");
            var tenant = new TestTenantProvider("tenant-1");
            var user = new TestCurrentUser("user-1");
            var handler = new CriarVendaFiscalCommandHandler(context, tenant, user);

            var cmd = new CriarVendaFiscalCommand(
                CaixaId: Guid.NewGuid().ToString(),
                ModeloFiscal: EModeloDocumento.NFe,
                NaturezaOperacao: "5102",
                DataVenda: DateTime.UtcNow,
                InformacoesComplementares: null,
                InformacoesAdicionaisFisco: null,
                Status: "Salvar",
                ModalidadeFrete: EModalidadeFrete.mfSemFrete,
                VendaOrigem: EVendaOrigem.NfeSimplificada,
                IncluirFreteNoTotal: false,
                ClienteId: null,
                Total: 200.00m,
                ValorDesconto: 0m,
                ValorFrete: 0m,
                FormaPagamento: "Boleto");

            var result = await handler.Handle(cmd, CancellationToken.None);

            Assert.True(result.Sucesso);
            Assert.Single(context.Vendas);
            var venda = await context.Vendas.FirstAsync();
            Assert.Equal(200.00m, venda.Total);
            Assert.Equal("tenant-1", venda.TenantId);
        }
    }

    public class FakeMediator : IMediator
    {
        public List<object> PublishedEvents { get; } = new();

        public Task Publish(object notification, CancellationToken cancellationToken = default)
        {
            PublishedEvents.Add(notification);
            return Task.CompletedTask;
        }

        public Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default) where TNotification : INotification
        {
            PublishedEvents.Add(notification!);
            return Task.CompletedTask;
        }

        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default) where TRequest : IRequest
        {
            throw new NotImplementedException();
        }

        public Task<object?> Send(object request, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public IAsyncEnumerable<TResponse> CreateStream<TResponse>(IStreamRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public IAsyncEnumerable<object?> CreateStream(object request, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
