using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Aplicativo.Application.Contracts;
using Epros.Modules.Aplicativo.Application.Plataforma.Conectores;
using Epros.Modules.Aplicativo.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Domain.Events;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Epros.Tests.Plataforma
{
    /// <summary>PLT · Conectores/Webhooks — registro, fan-out, HMAC, retry/backoff/dead-letter.</summary>
    public class ConectoresPlataformaTests
    {
        private static ContextAplicativo Novo(string db)
        {
            var options = new DbContextOptionsBuilder<ContextAplicativo>().UseInMemoryDatabase(db).Options;
            return new ContextAplicativo(options, T(), U());
        }

        private static ITenantProvider T() => new PlataformaTestFixtures.TestTenantProvider("tenant-1");
        private static ICurrentUser U() => new PlataformaTestFixtures.TestCurrentUser("user-1");

        private static readonly string EventoValido = CatalogoEventosIntegracao.Plataforma.GedDocumentoRegistrado;

        [Fact]
        public async Task Registrar_Endpoint_Rejeita_Evento_Fora_Do_Catalogo()
        {
            using var ctx = Novo(nameof(Registrar_Endpoint_Rejeita_Evento_Fora_Do_Catalogo));
            var h = new RegistrarEndpointWebhookCommandHandler(ctx, T(), U(), new CofreFake());
            var r = await h.Handle(new RegistrarEndpointWebhookCommand("EP", "https://ex.com/hook",
                new List<string> { "evento.inexistente" }, null, null, 3), CancellationToken.None);
            Assert.False(r.Sucesso);
        }

        [Fact]
        public async Task Registrar_Endpoint_Cifra_Segredo_No_Cofre()
        {
            using var ctx = Novo(nameof(Registrar_Endpoint_Cifra_Segredo_No_Cofre));
            var h = new RegistrarEndpointWebhookCommandHandler(ctx, T(), U(), new CofreFake());
            var r = await h.Handle(new RegistrarEndpointWebhookCommand("EP", "https://ex.com/hook",
                new List<string> { EventoValido }, "meu-segredo", null, 3), CancellationToken.None);

            Assert.True(r.Sucesso);
            var ep = await ctx.EndpointsWebhook.FirstAsync();
            Assert.NotNull(ep.SegredoCifrado);
            Assert.DoesNotContain("meu-segredo", ep.SegredoCifrado!); // cifrado, nunca em claro
        }

        [Fact]
        public async Task Publicar_Faz_Fanout_Apenas_Para_Endpoints_Inscritos_E_Ativos()
        {
            using var ctx = Novo(nameof(Publicar_Faz_Fanout_Apenas_Para_Endpoints_Inscritos_E_Ativos));
            var reg = new RegistrarEndpointWebhookCommandHandler(ctx, T(), U(), new CofreFake());
            await reg.Handle(new RegistrarEndpointWebhookCommand("A", "https://a.com/h", new List<string> { EventoValido }, "s", null, 3), CancellationToken.None);
            await reg.Handle(new RegistrarEndpointWebhookCommand("B", "https://b.com/h",
                new List<string> { CatalogoEventosIntegracao.Plataforma.AssinaturaConcluida }, null, null, 3), CancellationToken.None);

            var pub = new PublicarEventoWebhookCommandHandler(ctx, T(), U(), new CofreFake());
            var r = await pub.Handle(new PublicarEventoWebhookCommand(EventoValido, "{\"x\":1}"), CancellationToken.None);

            Assert.True(r.Sucesso);
            var entregas = await ctx.EntregasWebhook.ToListAsync();
            Assert.Single(entregas); // só o endpoint A
            Assert.NotNull(entregas[0].AssinaturaHmac); // HMAC calculado (A tem segredo)
        }

        [Fact]
        public async Task Processar_Sem_Dispatcher_Deixa_Pendente()
        {
            using var ctx = Novo(nameof(Processar_Sem_Dispatcher_Deixa_Pendente));
            var entregaId = await SemearEntrega(ctx);
            var h = new ProcessarEntregaWebhookCommandHandler(ctx, T(), U(), new WebhookNaoConfiguradoFake());
            var r = await h.Handle(new ProcessarEntregaWebhookCommand(entregaId), CancellationToken.None);

            Assert.True(r.Sucesso);
            var e = await ctx.EntregasWebhook.FirstAsync();
            Assert.Equal("Pendente", e.Status);
            Assert.Equal(0, e.Tentativas); // nenhum estado falso gravado
        }

        [Fact]
        public async Task Processar_Sucesso_Marca_Entregue()
        {
            using var ctx = Novo(nameof(Processar_Sucesso_Marca_Entregue));
            var entregaId = await SemearEntrega(ctx);
            var h = new ProcessarEntregaWebhookCommandHandler(ctx, T(), U(), new WebhookFake(sucesso: true));
            await h.Handle(new ProcessarEntregaWebhookCommand(entregaId), CancellationToken.None);
            var e = await ctx.EntregasWebhook.FirstAsync();
            Assert.Equal("Entregue", e.Status);
        }

        [Fact]
        public async Task Processar_Falha_Ate_Max_Vai_Para_DeadLetter()
        {
            using var ctx = Novo(nameof(Processar_Falha_Ate_Max_Vai_Para_DeadLetter));
            var entregaId = await SemearEntrega(ctx, maxTentativas: 2);
            var h = new ProcessarEntregaWebhookCommandHandler(ctx, T(), U(), new WebhookFake(sucesso: false));

            await h.Handle(new ProcessarEntregaWebhookCommand(entregaId), CancellationToken.None); // tentativa 1 -> Pendente (backoff)
            var e1 = await ctx.EntregasWebhook.FirstAsync();
            Assert.Equal("Pendente", e1.Status);
            Assert.Equal(1, e1.Tentativas);
            Assert.NotNull(e1.ProximaTentativaEm);

            // força a entrega a ficar "processável" novamente (limpa o backoff no teste)
            await h.Handle(new ProcessarEntregaWebhookCommand(entregaId), CancellationToken.None); // tentativa 2 -> DeadLetter
            var e2 = await ctx.EntregasWebhook.FirstAsync();
            Assert.Equal("DeadLetter", e2.Status);
            Assert.Equal(2, e2.Tentativas);
        }

        private static async Task<Guid> SemearEntrega(ContextAplicativo ctx, int maxTentativas = 5)
        {
            var reg = new RegistrarEndpointWebhookCommandHandler(ctx, T(), U(), new CofreFake());
            await reg.Handle(new RegistrarEndpointWebhookCommand("EP", "https://ex.com/h",
                new List<string> { EventoValido }, null, null, maxTentativas), CancellationToken.None);
            var pub = new PublicarEventoWebhookCommandHandler(ctx, T(), U(), new CofreFake());
            await pub.Handle(new PublicarEventoWebhookCommand(EventoValido, "{}"), CancellationToken.None);
            return await ctx.EntregasWebhook.Select(e => e.Id).FirstAsync();
        }

        private sealed class CofreFake : ISegredoCofreService
        {
            public Task<string> CriptografarAsync(string valor) => Task.FromResult($"enc::{Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(valor))}");
            public Task<string> DescriptografarAsync(string ciphertext) =>
                Task.FromResult(System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(ciphertext.Replace("enc::", ""))));
        }

        private sealed class WebhookNaoConfiguradoFake : IWebhookDispatchService
        {
            public Task<ResultadoEntregaWebhook> EnviarAsync(string url, string payload, string? a,
                IReadOnlyDictionary<string, string>? h, CancellationToken ct = default)
                => Task.FromResult(ResultadoEntregaWebhook.NaoConfigurado());
        }

        private sealed class WebhookFake : IWebhookDispatchService
        {
            private readonly bool _sucesso;
            public WebhookFake(bool sucesso) => _sucesso = sucesso;
            public Task<ResultadoEntregaWebhook> EnviarAsync(string url, string payload, string? a,
                IReadOnlyDictionary<string, string>? h, CancellationToken ct = default)
                => Task.FromResult(_sucesso ? ResultadoEntregaWebhook.Ok(200) : ResultadoEntregaWebhook.Falha(500, "erro"));
        }
    }
}
