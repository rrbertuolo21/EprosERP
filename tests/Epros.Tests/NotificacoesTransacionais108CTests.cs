using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.GestaoClientes.Application.Commands;
using Epros.Modules.GestaoClientes.Application.Handlers;
using Epros.Modules.GestaoClientes.Domain.Entities;
using Epros.Modules.GestaoClientes.Infrastructure.Data;
using Epros.Modules.GestaoClientes.Infrastructure.Jobs;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Shared.Domain.Events;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Epros.Tests
{
    /// <summary>
    /// 1.08C — Notificações transacionais + conserto do dunning. Prova que a régua de cobrança agora ENTREGA
    /// (havia enfileiramento de FaturaAlertaCobrancaEvent sem NENHUM consumidor), que o fim de trial notifica
    /// e que o recibo é entregue — tudo via INotificacaoService, idempotente pelo Outbox.
    /// </summary>
    public class NotificacoesTransacionais108CTests
    {
        #region Helpers

        private ContextGestaoClientes CreateInMemoryContext(string databaseName, string tenantId, string userId)
        {
            var options = new DbContextOptionsBuilder<ContextGestaoClientes>()
                .UseInMemoryDatabase(databaseName)
                .Options;
            return new ContextGestaoClientes(options, new TestTenantProvider(tenantId), new TestCurrentUser(userId));
        }

        private static GestaoClientesOutboxProcessorJob CreateProcessor(ContextGestaoClientes context, INotificacaoService? notificador)
        {
            var services = notificador == null
                ? Enumerable.Empty<INotificacaoService>()
                : new[] { notificador };
            return new GestaoClientesOutboxProcessorJob(context, services, new HttpContextAccessor());
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
            public TestMediator(Func<object, Task<object>> handler) => _handler = handler;
            public async Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default) => (TResponse)await _handler(request);
            public async Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default) where TRequest : IRequest => await _handler(request!);
            public async Task<object?> Send(object request, CancellationToken cancellationToken = default) => await _handler(request);
            public Task Publish(object notification, CancellationToken cancellationToken = default) => Task.CompletedTask;
            public Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default) where TNotification : INotification => Task.CompletedTask;
            public IAsyncEnumerable<TResponse> CreateStream<TResponse>(IStreamRequest<TResponse> request, CancellationToken cancellationToken = default) => throw new NotImplementedException();
            public IAsyncEnumerable<object?> CreateStream(object request, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        }

        /// <summary>Spy do serviço de notificação: conta chamadas e captura o último e-mail/assunto.</summary>
        public class SpyNotificacaoService : INotificacaoService
        {
            public int EmailCalls { get; private set; }
            public string LastEmail { get; private set; } = string.Empty;
            public string LastAssunto { get; private set; } = string.Empty;
            public string LastCorpo { get; private set; } = string.Empty;
            public bool ShouldThrow { get; set; }

            public Task EnviarEmailAsync(string destinatario, string assunto, string corpoHtml)
            {
                EmailCalls++;
                LastEmail = destinatario;
                LastAssunto = assunto;
                LastCorpo = corpoHtml;
                if (ShouldThrow) throw new InvalidOperationException("Falha simulada de envio.");
                return Task.CompletedTask;
            }

            public Task EnviarSmsAsync(string telefone, string mensagem) => Task.CompletedTask;
            public Task EnviarWhatsAppAsync(string telefone, string mensagem) => Task.CompletedTask;
        }

        private static Cliente NovoCliente(string tenantId, string userId, string email = "cliente@epros.com")
            => new Cliente("Cliente Teste", "00.000.000/0001-00", email, Guid.NewGuid(),
                null, null, 10, StatusSaaS.Ativo, tenantId, userId);

        #endregion

        #region 1) Dunning agora ENTREGA (FaturaAlertaCobrancaEvent)

        [Fact]
        public async Task Dunning_Deve_Entregar_FaturaAlertaCobrancaEvent_Via_NotificacaoService()
        {
            var tenantId = "tenant-dunning";
            var userId = "user";
            using var context = CreateInMemoryContext(Guid.NewGuid().ToString(), tenantId, userId);

            var cliente = NovoCliente(tenantId, userId, "dunning@epros.com");
            context.Clientes.Add(cliente);

            // Espelha o payload que o ReguaCobrancaJob enfileira para um alerta D-3.
            var payload = JsonSerializer.Serialize(new
            {
                FaturaId = Guid.NewGuid(),
                ClienteId = cliente.Id,
                Valor = 199.90m,
                DataVencimento = DateTime.UtcNow.AddDays(3),
                DiasDiferenca = 3,
                TipoAlerta = "D-3",
                PixCopiaECola = "00020101021226830014br.gov.bcb.pix",
                Mensagem = "Sua fatura vence em 3 dias.",
                EnviadoEm = DateTime.UtcNow
            });
            context.OutboxMessages.Add(new OutboxMessage(tenantId, "FaturaAlertaCobrancaEvent", payload));
            await context.SaveChangesAsync();

            var spy = new SpyNotificacaoService();
            await CreateProcessor(context, spy).Execute(null!);

            Assert.Equal(1, spy.EmailCalls);
            Assert.Equal("dunning@epros.com", spy.LastEmail);
            Assert.Contains("D-3", spy.LastAssunto);

            var msg = await context.OutboxMessages.AsNoTracking().FirstAsync(m => m.EventType == "FaturaAlertaCobrancaEvent");
            Assert.NotNull(msg.ProcessadoEm);
        }

        [Fact]
        public async Task Dunning_Processar_Duas_Vezes_Nao_Deve_Notificar_Duas_Vezes_Idempotencia()
        {
            var tenantId = "tenant-idem";
            var userId = "user";
            using var context = CreateInMemoryContext(Guid.NewGuid().ToString(), tenantId, userId);

            var cliente = NovoCliente(tenantId, userId, "idem@epros.com");
            context.Clientes.Add(cliente);
            var payload = JsonSerializer.Serialize(new
            {
                FaturaId = Guid.NewGuid(),
                ClienteId = cliente.Id,
                Valor = 50m,
                DataVencimento = DateTime.UtcNow.AddDays(-1),
                DiasDiferenca = -1,
                TipoAlerta = "D+1",
                PixCopiaECola = "pix",
                Mensagem = "Fatura vencida.",
                EnviadoEm = DateTime.UtcNow
            });
            context.OutboxMessages.Add(new OutboxMessage(tenantId, "FaturaAlertaCobrancaEvent", payload));
            await context.SaveChangesAsync();

            var spy = new SpyNotificacaoService();
            var processor = CreateProcessor(context, spy);

            await processor.Execute(null!); // 1ª passada entrega
            await processor.Execute(null!); // 2ª passada: mensagem já processada → nada a fazer

            Assert.Equal(1, spy.EmailCalls);
        }

        [Fact]
        public async Task Dunning_Falha_De_Envio_Nao_Trava_Job_E_Deixa_Para_Retry()
        {
            var tenantId = "tenant-falha";
            var userId = "user";
            using var context = CreateInMemoryContext(Guid.NewGuid().ToString(), tenantId, userId);

            var cliente = NovoCliente(tenantId, userId, "falha@epros.com");
            context.Clientes.Add(cliente);
            var payload = JsonSerializer.Serialize(new
            {
                FaturaId = Guid.NewGuid(),
                ClienteId = cliente.Id,
                Valor = 10m,
                DataVencimento = DateTime.UtcNow,
                DiasDiferenca = 0,
                TipoAlerta = "D-1",
                PixCopiaECola = "pix",
                Mensagem = "Aviso.",
                EnviadoEm = DateTime.UtcNow
            });
            context.OutboxMessages.Add(new OutboxMessage(tenantId, "FaturaAlertaCobrancaEvent", payload));
            await context.SaveChangesAsync();

            var spy = new SpyNotificacaoService { ShouldThrow = true };

            // Não deve lançar exceção (job resiliente).
            await CreateProcessor(context, spy).Execute(null!);

            var msg = await context.OutboxMessages.AsNoTracking().FirstAsync(m => m.EventType == "FaturaAlertaCobrancaEvent");
            Assert.Null(msg.ProcessadoEm);   // não marcada como processada
            Assert.Equal(1, msg.Tentativas); // registrou a tentativa → fica para retry
        }

        [Fact]
        public async Task Dunning_Sem_Provedor_Deve_Ser_NoOp_E_Consumir_Mensagem()
        {
            var tenantId = "tenant-noprovider";
            var userId = "user";
            using var context = CreateInMemoryContext(Guid.NewGuid().ToString(), tenantId, userId);

            var cliente = NovoCliente(tenantId, userId, "np@epros.com");
            context.Clientes.Add(cliente);
            var payload = JsonSerializer.Serialize(new
            {
                FaturaId = Guid.NewGuid(),
                ClienteId = cliente.Id,
                Valor = 10m,
                DataVencimento = DateTime.UtcNow,
                DiasDiferenca = -5,
                TipoAlerta = "D+5",
                PixCopiaECola = "pix",
                Mensagem = "Aviso.",
                EnviadoEm = DateTime.UtcNow
            });
            context.OutboxMessages.Add(new OutboxMessage(tenantId, "FaturaAlertaCobrancaEvent", payload));
            await context.SaveChangesAsync();

            // Sem provedor (coleção vazia) → no-op resiliente; a mensagem é consumida assim mesmo.
            await CreateProcessor(context, null).Execute(null!);

            var msg = await context.OutboxMessages.AsNoTracking().FirstAsync(m => m.EventType == "FaturaAlertaCobrancaEvent");
            Assert.NotNull(msg.ProcessadoEm);
        }

        #endregion

        #region 2) Fim de trial notifica

        [Fact]
        public async Task Fim_De_Trial_Deve_Enfileirar_E_Entregar_Notificacao()
        {
            var tenantId = "tenant-trial-notif";
            var userId = "user-trial";
            using var context = CreateInMemoryContext(Guid.NewGuid().ToString(), tenantId, userId);

            var plano = new Plano("Plano Pago", 100.00m, tenantId, userId);
            context.Planos.Add(plano);
            var cliente = new Cliente("Cliente Trial", "00.000.000/0001-00", "trial@epros.com", plano.Id,
                null, null, 10, StatusSaaS.TrialGratuito, tenantId, userId);
            context.Clientes.Add(cliente);
            var assinatura = new AssinaturaCliente(cliente.Id, plano.Id, AssinaturaStatus.Ativa,
                DateTime.UtcNow.AddDays(-31), DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddDays(-1),
                "Trial", null, null, tenantId, userId);
            context.AssinaturasClientes.Add(assinatura);
            await context.SaveChangesAsync();

            var cobranca = new Epros.Modules.GestaoClientes.Infrastructure.Gateways.CobrancaRecorrenteGatewayNoop();
            var mediator = new TestMediator(_ => Task.FromResult<object>(CommandResult.Ok("pix ok")));
            var handler = new EncerrarTrialsExpiradosCommandHandler(context, new TestCurrentUser(userId), cobranca, mediator);

            var r = await handler.Handle(new EncerrarTrialsExpiradosCommand(DateTime.UtcNow), CancellationToken.None);
            Assert.True(r.Sucesso);

            // O handler enfileirou o TrialEncerradoEvent.
            var evt = await context.OutboxMessages.FirstOrDefaultAsync(m => m.EventType == "TrialEncerradoEvent");
            Assert.NotNull(evt);

            // O processador entrega a notificação.
            var spy = new SpyNotificacaoService();
            await CreateProcessor(context, spy).Execute(null!);

            Assert.Equal(1, spy.EmailCalls);
            Assert.Equal("trial@epros.com", spy.LastEmail);
            Assert.Contains("avaliação", spy.LastAssunto);
        }

        #endregion

        #region 3) Recibo entregue

        [Fact]
        public async Task Quitacao_Deve_Entregar_Notificacao_De_Recibo()
        {
            var tenantId = "tenant-recibo";
            var userId = "user-recibo";
            using var context = CreateInMemoryContext(Guid.NewGuid().ToString(), tenantId, userId);

            var plano = new Plano("Plano Pago", 120.00m, tenantId, userId);
            context.Planos.Add(plano);
            var cliente = new Cliente("Cliente Recibo", "00.000.000/0001-00", "recibo@epros.com", plano.Id,
                null, null, 10, StatusSaaS.AguardandoPagamento, tenantId, userId);
            context.Clientes.Add(cliente);
            var assinatura = new AssinaturaCliente(cliente.Id, plano.Id, AssinaturaStatus.Ativa,
                DateTime.UtcNow, null, null, "Contratado", null, null, tenantId, userId);
            context.AssinaturasClientes.Add(assinatura);
            var fatura = new Fatura(cliente.Id, 120.00m, DateTime.UtcNow.AddDays(5), tenantId, userId);
            context.Faturas.Add(fatura);
            await context.SaveChangesAsync();

            var svc = new Epros.Modules.GestaoClientes.Application.Services.FaturaLiquidacaoService(context);
            var recibo = await svc.LiquidarAsync(fatura, "pay-123", "PIX", "MercadoPago",
                120.00m, 2.50m, 117.50m, DateTime.UtcNow, userId, CancellationToken.None);
            await context.SaveChangesAsync();

            Assert.NotNull(recibo);

            var evt = await context.OutboxMessages.FirstOrDefaultAsync(m => m.EventType == "ReciboEmitidoEvent");
            Assert.NotNull(evt);

            var spy = new SpyNotificacaoService();
            await CreateProcessor(context, spy).Execute(null!);

            Assert.Equal(1, spy.EmailCalls);
            Assert.Equal("recibo@epros.com", spy.LastEmail);
            Assert.Contains(recibo!.Numero, spy.LastAssunto);
        }

        #endregion

        #region 4) TRANSVERSAL T1 — eventos de ciclo de assinatura que morriam na fila agora são DRENADOS

        [Fact]
        public async Task PlanoAlterado_Deve_Entregar_Notificacao_Ao_Cliente()
        {
            var tenantId = "tenant-plano";
            var userId = "user";
            using var context = CreateInMemoryContext(Guid.NewGuid().ToString(), tenantId, userId);

            var cliente = NovoCliente(tenantId, userId, "plano@epros.com");
            context.Clientes.Add(cliente);
            var payload = JsonSerializer.Serialize(new
            {
                AssinaturaId = Guid.NewGuid(),
                ClienteId = cliente.Id,
                TenantId = tenantId,
                PlanoAnteriorId = Guid.NewGuid(),
                PlanoNovoId = Guid.NewGuid(),
                TipoMudanca = "Upgrade",
                ValorProracao = 49.90m,
                TipoProracao = "Debito",
                FaturaDiferencaId = (Guid?)Guid.NewGuid()
            });
            context.OutboxMessages.Add(new OutboxMessage(tenantId, "PlanoAlteradoEvent", payload));
            await context.SaveChangesAsync();

            var spy = new SpyNotificacaoService();
            await CreateProcessor(context, spy).Execute(null!);

            Assert.Equal(1, spy.EmailCalls);
            Assert.Equal("plano@epros.com", spy.LastEmail);
            Assert.Contains("upgrade", spy.LastAssunto, StringComparison.OrdinalIgnoreCase);
            var msg = await context.OutboxMessages.AsNoTracking().FirstAsync(m => m.EventType == "PlanoAlteradoEvent");
            Assert.NotNull(msg.ProcessadoEm);
        }

        [Fact]
        public async Task AssinaturaCancelada_Deve_Entregar_Notificacao_Ao_Cliente()
        {
            var tenantId = "tenant-cancel";
            var userId = "user";
            using var context = CreateInMemoryContext(Guid.NewGuid().ToString(), tenantId, userId);

            var cliente = NovoCliente(tenantId, userId, "cancel@epros.com");
            context.Clientes.Add(cliente);
            var payload = JsonSerializer.Serialize(new
            {
                AssinaturaId = Guid.NewGuid(),
                ClienteId = cliente.Id,
                TenantId = tenantId,
                Motivo = "Motivo teste",
                CanceladaEm = DateTime.UtcNow,
                CanceladaPor = userId
            });
            context.OutboxMessages.Add(new OutboxMessage(tenantId, "AssinaturaCanceladaEvent", payload));
            await context.SaveChangesAsync();

            var spy = new SpyNotificacaoService();
            await CreateProcessor(context, spy).Execute(null!);

            Assert.Equal(1, spy.EmailCalls);
            Assert.Equal("cancel@epros.com", spy.LastEmail);
            Assert.Contains("cancelada", spy.LastAssunto, StringComparison.OrdinalIgnoreCase);
            var msg = await context.OutboxMessages.AsNoTracking().FirstAsync(m => m.EventType == "AssinaturaCanceladaEvent");
            Assert.NotNull(msg.ProcessadoEm);
        }

        [Fact]
        public async Task AssinaturaReativada_Deve_Entregar_Notificacao_Ao_Cliente()
        {
            var tenantId = "tenant-react";
            var userId = "user";
            using var context = CreateInMemoryContext(Guid.NewGuid().ToString(), tenantId, userId);

            var cliente = NovoCliente(tenantId, userId, "react@epros.com");
            context.Clientes.Add(cliente);
            var payload = JsonSerializer.Serialize(new
            {
                AssinaturaId = Guid.NewGuid(),
                ClienteId = cliente.Id,
                TenantId = tenantId,
                ReativadaPor = userId,
                ReativadaEm = DateTime.UtcNow
            });
            context.OutboxMessages.Add(new OutboxMessage(tenantId, "AssinaturaReativadaEvent", payload));
            await context.SaveChangesAsync();

            var spy = new SpyNotificacaoService();
            await CreateProcessor(context, spy).Execute(null!);

            Assert.Equal(1, spy.EmailCalls);
            Assert.Equal("react@epros.com", spy.LastEmail);
            Assert.Contains("reativada", spy.LastAssunto, StringComparison.OrdinalIgnoreCase);
            var msg = await context.OutboxMessages.AsNoTracking().FirstAsync(m => m.EventType == "AssinaturaReativadaEvent");
            Assert.NotNull(msg.ProcessadoEm);
        }

        [Fact]
        public async Task Comissao_E_DocumentoFiscal_Sao_Drenados_Como_Fallback_Sem_Notificar()
        {
            // Estes eventos (factuais / de efeito síncrono) morriam na fila: agora o leitor único de
            // plataforma.outbox_messages os DRENA (fallback, pendência de regra) sem inventar efeito e sem notificar.
            var tenantId = "tenant-fallback";
            var userId = "user";
            using var context = CreateInMemoryContext(Guid.NewGuid().ToString(), tenantId, userId);

            context.OutboxMessages.Add(new OutboxMessage(tenantId, "ComissaoApuradaEvent", "{}"));
            context.OutboxMessages.Add(new OutboxMessage(tenantId, "DocumentoFiscalAutorizado", "{}"));
            context.OutboxMessages.Add(new OutboxMessage(tenantId, "DocumentoFiscalCancelado", "{}"));
            await context.SaveChangesAsync();

            var spy = new SpyNotificacaoService();
            await CreateProcessor(context, spy).Execute(null!);

            Assert.Equal(0, spy.EmailCalls); // fallback NÃO notifica
            var pendentes = await context.OutboxMessages.AsNoTracking().CountAsync(m => m.ProcessadoEm == null);
            Assert.Equal(0, pendentes); // todos drenados (marcados processados), nada morre na fila
        }

        #endregion
    }
}
