using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.GestaoClientes.Application.Commands;
using Epros.Modules.GestaoClientes.Application.Handlers;
using Epros.Modules.GestaoClientes.Domain.Entities;
using Epros.Modules.GestaoClientes.Infrastructure.Data;
using Epros.Modules.GestaoClientes.Infrastructure.Jobs;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Epros.Tests
{
    public class AssinaturasPlanosGapsTests
    {
        #region Helpers

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
            public string? GetUserName() => "User Test";
            public string? GetUserEmail() => "test@epros.com";
        }

        private class TestMediator : IMediator
        {
            private readonly Func<object, Task<object>> _handler;

            public TestMediator(Func<object, Task<object>> handler)
            {
                _handler = handler;
            }

            public async Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
            {
                var result = await _handler(request);
                return (TResponse)result;
            }

            public async Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default) where TRequest : IRequest
            {
                await _handler(request);
            }

            public async Task<object?> Send(object request, CancellationToken cancellationToken = default)
            {
                return await _handler(request);
            }

            public Task Publish(object notification, CancellationToken cancellationToken = default) => Task.CompletedTask;
            public Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default) where TNotification : INotification => Task.CompletedTask;

            public IAsyncEnumerable<TResponse> CreateStream<TResponse>(IStreamRequest<TResponse> request, CancellationToken cancellationToken = default) => throw new NotImplementedException();
            public IAsyncEnumerable<object?> CreateStream(object request, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        }

        private string ComputeHmacSha256(string secret, string payload)
        {
            using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret)))
            {
                var hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            }
        }

        #endregion

        #region Testes de Reajuste Automático por Índices (REG-036)

        [Fact]
        public async Task Deve_Reajustar_Composicao_Faturamento_No_Aniversario_De_12_Meses()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            var tenantId = "tenant-reajuste";
            var userId = "sistema_reajuste";

            using var context = CreateInMemoryContext(dbName, tenantId, userId);
            var httpContextAccessor = new HttpContextAccessor();

            // Configuração do índice no banco
            var configPercentual = new ConfiguracaoGlobal("reajuste_indice_percentual", "5.5", false, "IGP-M Percentual", tenantId, userId);
            var configNome = new ConfiguracaoGlobal("reajuste_indice_nome", "IGP-M", false, "IGP-M Nome", tenantId, userId);
            context.ConfiguracoesGlobais.Add(configPercentual);
            context.ConfiguracoesGlobais.Add(configNome);

            // Composição de faturamento criada há 13 meses atrás
            var clienteId = Guid.NewGuid();
            var dataInicial = DateTime.UtcNow.AddMonths(-13);
            var composicao = new ComposicaoFaturamento(clienteId, "Plano Anual ERP", 100.00m, dataInicial, null, true, tenantId, userId);
            context.ComposicoesFaturamento.Add(composicao);

            await context.SaveChangesAsync();

            var job = new ReajusteContratoJob(context, httpContextAccessor);

            // Act
            await job.Execute(null!);

            // Assert
            var composicaoAtualizada = await context.ComposicoesFaturamento
                .FirstOrDefaultAsync(c => c.Id == composicao.Id);

            Assert.NotNull(composicaoAtualizada);
            // 100.00 + 5.5% = 105.50
            Assert.Equal(105.50m, composicaoAtualizada.Valor);

            var historico = await context.HistoricosReajustes
                .FirstOrDefaultAsync(h => h.ComposicaoId == composicao.Id);

            Assert.NotNull(historico);
            Assert.Equal(100.00m, historico.ValorAtual);
            Assert.Equal(105.50m, historico.ValorNovo);
            Assert.Equal(5.5m, historico.PercentualReajuste);
            Assert.Equal("IGP-M", historico.TipoReajuste);
        }

        #endregion

        #region Testes de Split de Recebíveis e Comissão (REG-037)

        [Fact]
        public async Task Deve_Calcular_E_Gravar_Comissao_Split_Na_Baixa_De_Fatura()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            var tenantId = "tenant-split";
            var userId = "user-split";

            using var context = CreateInMemoryContext(dbName, tenantId, userId);
            var currentUser = new TestCurrentUser(userId);

            // Cria parceiros
            var revenda = new Revenda("Parceiro Revenda S.A.", 10.0m, tenantId, userId);
            var vendedor = new Vendedor(null, "João Vendedor", "joao@epros.com", null, 5.0m, tenantId, userId);
            context.Revendas.Add(revenda);
            context.Vendedores.Add(vendedor);
            await context.SaveChangesAsync();

            // Cria cliente vinculado à revenda e vendedor
            var plano = new Plano("Plano Teste", 500m, tenantId, userId);
            context.Planos.Add(plano);
            var cliente = new Cliente("Empresa Cliente", "12.345.678/0001-00", "cliente@teste.com", plano.Id, revenda.Id, vendedor.Id, 10, StatusSaaS.Ativo, tenantId, userId);
            context.Clientes.Add(cliente);
            await context.SaveChangesAsync();

            // Cria fatura
            var fatura = new Fatura(cliente.Id, 1000.00m, DateTime.UtcNow.AddDays(5), tenantId, userId);
            context.Faturas.Add(fatura);
            await context.SaveChangesAsync();

            var handler = new BaixarFaturaCommandHandler(context, currentUser);
            var command = new BaixarFaturaCommand(fatura.Id);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.Sucesso);

            var faturaPaga = await context.Faturas.FirstOrDefaultAsync(f => f.Id == fatura.Id);
            Assert.NotNull(faturaPaga);
            Assert.Equal(FaturaStatus.Paga, faturaPaga.Status);
            Assert.Equal(10.0m, faturaPaga.PercentualComissaoRevenda);
            Assert.Equal(5.0m, faturaPaga.PercentualComissaoVendedor);
            Assert.Equal(100.00m, faturaPaga.ValorComissaoRevenda); // 10% de 1000
            Assert.Equal(50.00m, faturaPaga.ValorComissaoVendedor); // 5% de 1000

            // Verifica mensagem de outbox
            var outboxMsg = await context.OutboxMessages
                .FirstOrDefaultAsync(o => o.EventType == "ComissaoApuradaEvent");
            Assert.NotNull(outboxMsg);
            Assert.Contains(fatura.Id.ToString(), outboxMsg.Payload);
            Assert.Contains("\"ValorComissaoRevenda\":100", outboxMsg.Payload);
        }

        #endregion

        #region Testes de Régua de Cobrança (REG-039)

        [Fact]
        public async Task Deve_Enfileirar_Alertas_Corretos_Na_Regua_De_Cobranca()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            var tenantId = "tenant-regua";
            var userId = "user-regua";

            using var context = CreateInMemoryContext(dbName, tenantId, userId);
            var httpContextAccessor = new HttpContextAccessor();

            var hoje = DateTime.UtcNow.Date;

            // Fatura D-3 (Vence em 3 dias)
            var faturaD3 = new Fatura(Guid.NewGuid(), 100.00m, hoje.AddDays(3), tenantId, userId);
            // Fatura D-1 (Vence em 1 dia)
            var faturaD1 = new Fatura(Guid.NewGuid(), 200.00m, hoje.AddDays(1), tenantId, userId);
            // Fatura D+1 (Vencida há 1 dia)
            var faturaD_1 = new Fatura(Guid.NewGuid(), 300.00m, hoje.AddDays(-1), tenantId, userId);
            // Fatura D+5 (Vencida há 5 dias)
            var faturaD_5 = new Fatura(Guid.NewGuid(), 400.00m, hoje.AddDays(-5), tenantId, userId);
            // Fatura D+10 (Vencida há 10 dias)
            var faturaD_10 = new Fatura(Guid.NewGuid(), 500.00m, hoje.AddDays(-10), tenantId, userId);
            // Fatura D+14 (Vencida há 14 dias)
            var faturaD_14 = new Fatura(Guid.NewGuid(), 600.00m, hoje.AddDays(-14), tenantId, userId);
            // Fatura Fora da régua (Vence em 10 dias)
            var faturaFora = new Fatura(Guid.NewGuid(), 700.00m, hoje.AddDays(10), tenantId, userId);

            context.Faturas.AddRange(faturaD3, faturaD1, faturaD_1, faturaD_5, faturaD_10, faturaD_14, faturaFora);
            await context.SaveChangesAsync();

            var job = new ReguaCobrancaJob(context, httpContextAccessor);

            // Act
            await job.Execute(null!);

            // Assert
            var alertas = await context.OutboxMessages
                .Where(o => o.EventType == "FaturaAlertaCobrancaEvent")
                .ToListAsync();

            // Deve ter gerado 6 alertas (faturaFora é ignorada)
            Assert.Equal(6, alertas.Count);

            var alertasTipos = alertas.Select(a => {
                using var doc = System.Text.Json.JsonDocument.Parse(a.Payload);
                return doc.RootElement.GetProperty("TipoAlerta").GetString();
            }).ToList();

            Assert.Contains("D-3", alertasTipos);
            Assert.Contains("D-1", alertasTipos);
            Assert.Contains("D+1", alertasTipos);
            Assert.Contains("D+5", alertasTipos);
            Assert.Contains("D+10", alertasTipos);
            Assert.Contains("D+14", alertasTipos);
        }

        #endregion

        #region Testes de Segurança HMAC no Webhook (REG-035)

        [Fact]
        public async Task Webhook_Deve_Falhar_Se_Assinatura_Ausente_Ou_Invalida()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            var tenantId = "tenant-webhook";
            var userId = "user-webhook";

            using var context = CreateInMemoryContext(dbName, tenantId, userId);
            
            // Configura o segredo do webhook no banco
            var configSecret = new ConfiguracaoGlobal("webhook_secret", "meusegredo", false, "Segredo do Webhook", tenantId, userId);
            context.ConfiguracoesGlobais.Add(configSecret);
            await context.SaveChangesAsync();

            var mediatorMock = new TestMediator(_ => Task.FromResult<object>(CommandResult.Ok("Ok")));
            var handler = new ProcessarWebhookPagamentoCommandHandler(context, mediatorMock);

            var commandSemSig = new ProcessarWebhookPagamentoCommand("payment.created", new WebhookData(Guid.NewGuid().ToString()));
            var commandSigInvalida = commandSemSig with { Signature = "hash_incorreto" };

            // Act
            var resultSemSig = await handler.Handle(commandSemSig, CancellationToken.None);
            var resultSigInvalida = await handler.Handle(commandSigInvalida, CancellationToken.None);

            // Assert
            Assert.False(resultSemSig.Sucesso);
            Assert.Contains(resultSemSig.Erros, e => e.Contains("não fornecida"));

            Assert.False(resultSigInvalida.Sucesso);
            Assert.Contains(resultSigInvalida.Erros, e => e.Contains("assinatura do webhook inválida", StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        public async Task Webhook_Deve_Baixar_Fatura_Se_Assinatura_HMAC_For_Valida()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            var tenantId = "tenant-webhook";
            var userId = "user-webhook";

            using var context = CreateInMemoryContext(dbName, tenantId, userId);

            // Configura o segredo do webhook no banco
            var configSecret = new ConfiguracaoGlobal("webhook_secret", "meusegredo", false, "Segredo do Webhook", tenantId, userId);
            context.ConfiguracoesGlobais.Add(configSecret);

            // Cria fatura pendente (o ID é gerado automaticamente)
            var fatura = new Fatura(Guid.NewGuid(), 150.00m, DateTime.UtcNow.AddDays(5), tenantId, userId);
            context.Faturas.Add(fatura);
            await context.SaveChangesAsync();

            var faturaId = fatura.Id;

            // Cria o mediator real chamando o BaixarFaturaCommandHandler
            var currentUser = new TestCurrentUser(userId);
            var baixarHandler = new BaixarFaturaCommandHandler(context, currentUser);
            
            var mediatorMock = new TestMediator(async request =>
            {
                if (request is BaixarFaturaCommand command)
                {
                    return await baixarHandler.Handle(command, CancellationToken.None);
                }
                return CommandResult.Falha("Não suportado");
            });

            var handler = new ProcessarWebhookPagamentoCommandHandler(context, mediatorMock);

            var action = "payment.created";
            var hashValido = ComputeHmacSha256("meusegredo", $"{action}:{faturaId}");

            var command = new ProcessarWebhookPagamentoCommand(action, new WebhookData(faturaId.ToString()), hashValido);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.Sucesso);

            var faturaPaga = await context.Faturas.IgnoreQueryFilters().FirstOrDefaultAsync(f => f.Id == faturaId);
            Assert.NotNull(faturaPaga);
            Assert.Equal(FaturaStatus.Paga, faturaPaga.Status);
        }

        #endregion
    }
}
