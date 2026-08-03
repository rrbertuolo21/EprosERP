using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Aplicativo.Infrastructure.Data;
using Epros.Modules.Aplicativo.Infrastructure.Jobs;
using Epros.Modules.GestaoClientes.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Domain.Events;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Epros.Tests
{
    /// <summary>
    /// TRANSVERSAL T1 — Prova que os eventos da PLATAFORMA (plt.*), antes PUBLICADOS SEM CONSUMIDOR no
    /// schema "aplicativo" (aplicativo.outbox_messages), agora são DRENADOS pelo leitor único
    /// (<see cref="AplicativoOutboxProcessorJob"/>) como FALLBACK (pendência de regra): marcados como
    /// processados, sem efeito inventado e sem notificação. Não se adiciona um segundo leitor (dispatcher)
    /// na mesma tabela — evita corrida no flag processado.
    /// </summary>
    public class PlataformaOutboxFallbackTests
    {
        private sealed class TestTenantProvider : ITenantProvider
        {
            private readonly string _t;
            public TestTenantProvider(string t) => _t = t;
            public string GetTenantId() => _t;
        }

        private sealed class TestCurrentUser : ICurrentUser
        {
            private readonly string _u;
            public TestCurrentUser(string u) => _u = u;
            public string? GetUserId() => _u;
            public string? GetUserName() => "User Test";
            public string? GetUserEmail() => "test@epros.com";
        }

        private sealed class SpyNotificacao : INotificacaoService
        {
            public int Calls { get; private set; }
            public Task EnviarEmailAsync(string destinatario, string assunto, string corpoHtml) { Calls++; return Task.CompletedTask; }
            public Task EnviarSmsAsync(string telefone, string mensagem) { Calls++; return Task.CompletedTask; }
            public Task EnviarWhatsAppAsync(string telefone, string mensagem) { Calls++; return Task.CompletedTask; }
        }

        [Fact]
        public async Task PltEventos_Sao_Drenados_Como_Fallback_Sem_Notificar()
        {
            var db = Guid.NewGuid().ToString();
            var tenant = new TestTenantProvider("tenant-plt");
            var user = new TestCurrentUser("user-plt");

            var optApp = new DbContextOptionsBuilder<ContextAplicativo>().UseInMemoryDatabase(db).Options;
            using var contextApp = new ContextAplicativo(optApp, tenant, user);
            var optGestao = new DbContextOptionsBuilder<ContextGestaoClientes>().UseInMemoryDatabase(db + "-g").Options;
            using var contextGestao = new ContextGestaoClientes(optGestao, tenant, user);

            // Amostra de plt.* de submódulos distintos (GED, IoT, Conector, SDK, Assinatura documental).
            contextApp.OutboxMessages.Add(new OutboxMessage("tenant-plt", CatalogoEventosIntegracao.Plataforma.GedDocumentoRegistrado, "{}"));
            contextApp.OutboxMessages.Add(new OutboxMessage("tenant-plt", CatalogoEventosIntegracao.Plataforma.IotLeituraForaFaixa, "{}"));
            contextApp.OutboxMessages.Add(new OutboxMessage("tenant-plt", CatalogoEventosIntegracao.Plataforma.ConectorEntregaFalhou, "{}"));
            contextApp.OutboxMessages.Add(new OutboxMessage("tenant-plt", CatalogoEventosIntegracao.Plataforma.SdkChaveApiGerada, "{}"));
            contextApp.OutboxMessages.Add(new OutboxMessage("tenant-plt", CatalogoEventosIntegracao.Plataforma.AssinaturaSolicitada, "{}"));
            await contextApp.SaveChangesAsync();

            var spy = new SpyNotificacao();
            var job = new AplicativoOutboxProcessorJob(contextApp, contextGestao, null!, new HttpContextAccessor(), spy);
            await job.Execute(null!);

            var pendentes = await contextApp.OutboxMessages.AsNoTracking().CountAsync(m => m.ProcessadoEm == null);
            Assert.Equal(0, pendentes); // todos drenados (nada morre na fila)
            Assert.Equal(0, spy.Calls); // fallback NÃO notifica nem inventa efeito
        }
    }
}
