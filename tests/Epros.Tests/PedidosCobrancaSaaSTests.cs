using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Microsoft.EntityFrameworkCore;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Modules.GestaoClientes.Domain.Entities;
using Epros.Modules.GestaoClientes.Infrastructure.Data;
using Epros.Modules.Aplicativo.Application.Commands;
using Epros.Modules.Aplicativo.Application.Handlers;
using Epros.Modules.Aplicativo.Application.Dtos;

namespace Epros.Tests
{
    public class PedidosCobrancaSaaSTests
    {
        #region Testes de Pedido e Cupom de Desconto

        [Fact]
        public async Task Deve_Criar_Pedido_Com_Desconto_Percentual_E_Incrementar_Uso_Do_Cupom()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            var tenantId = "tenant-cupom-pct";
            var userId = "user-cupom-pct";

            using var context = CreateInMemoryContext(dbName, tenantId, userId);
            var tenantProvider = new TestTenantProvider(tenantId);
            var currentUser = new TestCurrentUser(userId);

            // Setup
            var cliente = new Cliente("Cliente Teste Cupom Pct", "00.000.000/0001-00", "cliente@pct.com", Guid.NewGuid(), tenantId, userId);
            context.Clientes.Add(cliente);

            var plano = new Plano("Plano Anual", 1000.00m, null, 10, 2, "Fiscal", tenantId, userId);
            context.Planos.Add(plano);

            var cupom = new Cupom("DESC20", "Percentual", 20.00m, 5, null, tenantId, userId);
            context.Cupons.Add(cupom);

            await context.SaveChangesAsync();

            var handler = new CriarPedidoSaaSCommandHandler(context, tenantProvider, currentUser);

            // Act
            var command = new CriarPedidoSaaSCommand(plano.Id, "DESC20", "Gateway");
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.Sucesso);

            var pedido = await context.PedidosSaaS.FirstOrDefaultAsync(p => p.ClienteId == cliente.Id);
            Assert.NotNull(pedido);
            Assert.Equal(1000.00m, pedido.ValorBase);
            Assert.Equal(200.00m, pedido.ValorDesconto);
            Assert.Equal(800.00m, pedido.ValorTotal);
            Assert.Equal(PedidoSaaSStatus.Pending, pedido.Status);

            // Verifica incremento de usos do cupom
            var cupomDB = await context.Cupons.FindAsync(cupom.Id);
            Assert.Equal(1, cupomDB!.QuantidadeUsos);

            // Verifica registro do uso
            var jaUsou = await context.UsosCupons.AnyAsync(u => u.ClienteId == cliente.Id && u.CupomId == cupom.Id);
            Assert.True(jaUsou);
        }

        [Fact]
        public async Task Nao_Deve_Permitir_Cupom_Acima_Do_Limite_De_Uso()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            var tenantId = "tenant-cupom-limite";
            var userId = "user-cupom-limite";

            using var context = CreateInMemoryContext(dbName, tenantId, userId);
            var tenantProvider = new TestTenantProvider(tenantId);
            var currentUser = new TestCurrentUser(userId);

            var cliente = new Cliente("Cliente Teste Limite", "00.000.000/0001-00", "cliente@limite.com", Guid.NewGuid(), tenantId, userId);
            context.Clientes.Add(cliente);

            var plano = new Plano("Plano Semanal", 100.00m, null, 10, 2, "Fiscal", tenantId, userId);
            context.Planos.Add(plano);

            // Cupom ativo mas com limite de 1 uso, e já usado 1 vez
            var cupom = new Cupom("LIMITE1", "Fixo", 10.00m, 1, null, tenantId, userId);
            cupom.IncrementarUso(userId);
            context.Cupons.Add(cupom);

            await context.SaveChangesAsync();

            var handler = new CriarPedidoSaaSCommandHandler(context, tenantProvider, currentUser);

            // Act
            var command = new CriarPedidoSaaSCommand(plano.Id, "LIMITE1", "Gateway");
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.Sucesso);
            Assert.Contains(result.Erros, e => e.Contains("limite de usos esgotado"));
        }

        #endregion

        #region Testes de Idempotência do Webhook

        [Fact]
        public async Task Webhook_Deve_Ser_Idempotente_E_Nao_Processar_Mesma_Transacao_Duas_Vezes()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            var tenantId = "tenant-webhook-idemp";
            var userId = "user-webhook";

            using var context = CreateInMemoryContext(dbName, tenantId, userId);
            var tenantProvider = new TestTenantProvider(tenantId);
            var currentUser = new TestCurrentUser(userId);

            var cliente = new Cliente("Cliente Webhook", "00.000.000/0001-00", "cliente@webhook.com", Guid.NewGuid(), tenantId, userId);
            context.Clientes.Add(cliente);

            var plano = new Plano("Plano Webhook", 299.90m, null, 10, 2, "Fiscal", tenantId, userId);
            context.Planos.Add(plano);
            await context.SaveChangesAsync();

            var criarPedidoHandler = new CriarPedidoSaaSCommandHandler(context, tenantProvider, currentUser);
            var resultPedido = await criarPedidoHandler.Handle(new CriarPedidoSaaSCommand(plano.Id, null, "Gateway"), CancellationToken.None);
            Assert.True(resultPedido.Sucesso);

            var pedidoId = (Guid)resultPedido.Dados!.GetType().GetProperty("PedidoId")!.GetValue(resultPedido.Dados)!;
            var assinaturaId = (Guid)resultPedido.Dados.GetType().GetProperty("AssinaturaId")!.GetValue(resultPedido.Dados)!;

            // Busca a fatura gerada automaticamente
            var fatura = await context.Faturas.FirstOrDefaultAsync(f => f.ClienteId == cliente.Id);
            Assert.NotNull(fatura);

            var webhookHandler = new ProcessarWebhookPagamentoCommandHandler(context);
            var transactionId = "webhook-tx-123456";

            // Act 1: Processa o webhook pela primeira vez
            var command1 = new ProcessarWebhookPagamentoCommand(
                TransactionId: transactionId,
                Status: "approved",
                Gateway: "MercadoPago",
                Valor: 299.90m,
                AssinaturaId: assinaturaId,
                PedidoId: pedidoId,
                FaturaId: fatura.Id
            );
            var result1 = await webhookHandler.Handle(command1, CancellationToken.None);

            // Assert 1: Deve liquidar com sucesso
            Assert.True(result1.Sucesso);

            var faturaDB1 = await context.Faturas.FindAsync(fatura.Id);
            Assert.Equal(FaturaStatus.Paga, faturaDB1!.Status);

            var pedidoDB1 = await context.PedidosSaaS.FindAsync(pedidoId);
            Assert.Equal(PedidoSaaSStatus.Succeeded, pedidoDB1!.Status);

            var assinaturaDB1 = await context.AssinaturasClientes.FindAsync(assinaturaId);
            Assert.Equal(AssinaturaStatus.Aprovada, assinaturaDB1!.Status);

            var totalPagamentosGlobais = await context.PagamentosGlobais.CountAsync();
            Assert.Equal(1, totalPagamentosGlobais);

            // Act 2: Envia o mesmo webhook com a mesma transação novamente
            var result2 = await webhookHandler.Handle(command1, CancellationToken.None);

            // Assert 2: Deve retornar sucesso (Ok), mas de forma idempotente (sem duplicar liquidação)
            Assert.True(result2.Sucesso);
            Assert.Contains("Idempotente", result2.Mensagem);

            var totalPagamentosGlobaisAposSegundaTentativa = await context.PagamentosGlobais.CountAsync();
            Assert.Equal(1, totalPagamentosGlobaisAposSegundaTentativa); // Não duplicou
        }

        #endregion

        #region Testes de Transferência Offline e Comprovante

        [Fact]
        public async Task Fluxo_Offline_Deve_Registrar_Comprovante_E_Aprovar_Aguardando_Analise()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            var tenantId = "tenant-offline";
            var userId = "user-offline";

            using var context = CreateInMemoryContext(dbName, tenantId, userId);
            var tenantProvider = new TestTenantProvider(tenantId);
            var currentUser = new TestCurrentUser(userId);

            var cliente = new Cliente("Cliente Offline", "00.000.000/0001-00", "cliente@offline.com", Guid.NewGuid(), tenantId, userId);
            context.Clientes.Add(cliente);

            var plano = new Plano("Plano Offline", 150.00m, null, 10, 2, "Fiscal", tenantId, userId);
            context.Planos.Add(plano);
            await context.SaveChangesAsync();

            var criarPedidoHandler = new CriarPedidoSaaSCommandHandler(context, tenantProvider, currentUser);
            var resultPedido = await criarPedidoHandler.Handle(new CriarPedidoSaaSCommand(plano.Id, null, "Transferencia"), CancellationToken.None);
            Assert.True(resultPedido.Sucesso);

            var pedidoId = (Guid)resultPedido.Dados!.GetType().GetProperty("PedidoId")!.GetValue(resultPedido.Dados)!;
            var assinaturaId = (Guid)resultPedido.Dados.GetType().GetProperty("AssinaturaId")!.GetValue(resultPedido.Dados)!;

            var registrarTransfHandler = new RegistrarTransferenciaCommandHandler(context, tenantProvider, currentUser);

            // Act 1: Envia o comprovante
            var cmdRegistrar = new RegistrarTransferenciaCommand(
                PedidoId: pedidoId,
                FaturaId: null,
                Valor: 150.00m,
                NomeArquivo: "comprovante.pdf",
                CaminhoArquivo: "/arquivos/comprovante.pdf",
                TamanhoBytes: 5048,
                DataComprovante: DateTime.UtcNow
            );
            var resultRegistrar = await registrarTransfHandler.Handle(cmdRegistrar, CancellationToken.None);

            // Assert 1: Comprovante registrado com status pendente
            Assert.True(resultRegistrar.Sucesso);
            var pagamentoTransfId = (Guid)resultRegistrar.Dados!.GetType().GetProperty("PagamentoTransferenciaId")!.GetValue(resultRegistrar.Dados)!;

            var pagamentoDB = await context.PagamentosTransferencias.FindAsync(pagamentoTransfId);
            Assert.NotNull(pagamentoDB);
            Assert.Equal(PagamentoTransferenciaStatus.Pending, pagamentoDB.Status);

            var comprovanteDB = await context.ComprovantesPagamentos.FirstOrDefaultAsync(c => c.PagamentoTransferenciaId == pagamentoTransfId);
            Assert.NotNull(comprovanteDB);
            Assert.Equal("Unread", comprovanteDB.StatusLeitura);

            var analisarHandler = new AnalisarTransferenciaCommandHandler(context, currentUser);

            // Act 2: Siser aprova o comprovante
            var cmdAnalisar = new AnalisarTransferenciaCommand(pagamentoTransfId, Aprovado: true, Justificativa: null);
            var resultAnalisar = await analisarHandler.Handle(cmdAnalisar, CancellationToken.None);

            // Assert 2: Pedido e assinatura liquidados
            Assert.True(resultAnalisar.Sucesso);

            var pagamentoDBAposAprovacao = await context.PagamentosTransferencias.FindAsync(pagamentoTransfId);
            Assert.Equal(PagamentoTransferenciaStatus.Approved, pagamentoDBAposAprovacao!.Status);

            var pedidoDB = await context.PedidosSaaS.FindAsync(pedidoId);
            Assert.Equal(PedidoSaaSStatus.Succeeded, pedidoDB!.Status);

            var assinaturaDB = await context.AssinaturasClientes.FindAsync(assinaturaId);
            Assert.Equal(AssinaturaStatus.Aprovada, assinaturaDB!.Status);

            var clienteDB = await context.Clientes.FindAsync(cliente.Id);
            Assert.Equal(plano.Id, clienteDB!.PlanoId); // Plano atualizado no cliente

            var pagGlobal = await context.PagamentosGlobais.FirstOrDefaultAsync(p => p.PedidoId == pedidoId);
            Assert.NotNull(pagGlobal);
            Assert.Equal(150.00m, pagGlobal.Valor);
        }

        [Fact]
        public async Task Fluxo_Offline_Rejeitado_Deve_Exigir_Justificativa_E_Manter_Pedido_Pendente()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            var tenantId = "tenant-offline-rejeitar";
            var userId = "user-offline";

            using var context = CreateInMemoryContext(dbName, tenantId, userId);
            var tenantProvider = new TestTenantProvider(tenantId);
            var currentUser = new TestCurrentUser(userId);

            var cliente = new Cliente("Cliente Offline Rej", "00.000.000/0001-00", "cliente@offline.com", Guid.NewGuid(), tenantId, userId);
            context.Clientes.Add(cliente);

            var plano = new Plano("Plano Offline", 150.00m, null, 10, 2, "Fiscal", tenantId, userId);
            context.Planos.Add(plano);
            await context.SaveChangesAsync();

            var criarPedidoHandler = new CriarPedidoSaaSCommandHandler(context, tenantProvider, currentUser);
            var resultPedido = await criarPedidoHandler.Handle(new CriarPedidoSaaSCommand(plano.Id, null, "Transferencia"), CancellationToken.None);
            Assert.True(resultPedido.Sucesso);

            var pedidoId = (Guid)resultPedido.Dados!.GetType().GetProperty("PedidoId")!.GetValue(resultPedido.Dados)!;

            var registrarTransfHandler = new RegistrarTransferenciaCommandHandler(context, tenantProvider, currentUser);
            var cmdRegistrar = new RegistrarTransferenciaCommand(
                PedidoId: pedidoId,
                FaturaId: null,
                Valor: 150.00m,
                NomeArquivo: "comprovante_fake.pdf",
                CaminhoArquivo: "/arquivos/comprovante_fake.pdf",
                TamanhoBytes: 5048,
                DataComprovante: DateTime.UtcNow
            );
            var resultRegistrar = await registrarTransfHandler.Handle(cmdRegistrar, CancellationToken.None);
            var pagamentoTransfId = (Guid)resultRegistrar.Dados!.GetType().GetProperty("PagamentoTransferenciaId")!.GetValue(resultRegistrar.Dados)!;

            var analisarHandler = new AnalisarTransferenciaCommandHandler(context, currentUser);

            // Act: Rejeita com justificativa
            var cmdAnalisar = new AnalisarTransferenciaCommand(pagamentoTransfId, Aprovado: false, Justificativa: "Comprovante ilegível");
            var resultAnalisar = await analisarHandler.Handle(cmdAnalisar, CancellationToken.None);

            // Assert
            Assert.True(resultAnalisar.Sucesso);

            var pagamentoDB = await context.PagamentosTransferencias.FindAsync(pagamentoTransfId);
            Assert.Equal(PagamentoTransferenciaStatus.Rejected, pagamentoDB!.Status);
            Assert.Equal("Comprovante ilegível", pagamentoDB.Justificativa);

            // O pedido deve continuar pendente
            var pedidoDB = await context.PedidosSaaS.FindAsync(pedidoId);
            Assert.Equal(PedidoSaaSStatus.Pending, pedidoDB!.Status);
        }

        #endregion

        #region Helpers e Doubles de Teste

        private ContextGestaoClientes CreateInMemoryContext(string databaseName, string tenantId, string userId)
        {
            var options = new DbContextOptionsBuilder<ContextGestaoClientes>()
                .UseInMemoryDatabase(databaseName)
                .Options;

            var tenantProvider = new TestTenantProvider(tenantId);
            var currentUser = new TestCurrentUser(userId);

            return new ContextGestaoClientes(options, tenantProvider, currentUser);
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
            public string? GetUserName() => "Siser Financial Operator";
            public string? GetUserEmail() => "financeiro@siser.com.br";
        }

        #endregion
    }
}
