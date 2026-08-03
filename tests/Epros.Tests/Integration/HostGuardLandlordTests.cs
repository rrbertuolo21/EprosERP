using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Epros.Modules.GestaoClientes.Infrastructure.Data;
using Epros.Modules.Estoque.Infrastructure.Data;
using Epros.Modules.Fiscal.Infrastructure.Data;
using Epros.Modules.Financeiro.Infrastructure.Data;
using Epros.Modules.Vendas.Infrastructure.Data;
using Xunit;

namespace Epros.Tests.Integration
{
    /// <summary>
    /// Coleção única para os testes baseados em <see cref="WebApplicationFactory{TEntryPoint}"/>.
    /// DisableParallelization: o <c>HostFactoryResolver</c> do WebApplicationFactory usa estado
    /// compartilhado ao interceptar o entry point (Program); construir DUAS factories de <c>Program</c>
    /// em paralelo dispara ObjectDisposedException no provider. Serializando essas classes numa mesma
    /// coleção não-paralela, as factories nunca são construídas concorrentemente.
    /// </summary>
    [CollectionDefinition(IntegrationWebAppCollection.Nome, DisableParallelization = true)]
    public sealed class IntegrationWebAppCollection
    {
        public const string Nome = "IntegrationWebApp";
    }

    /// <summary>
    /// Host-guard do Landlord (1.04) — fecha a topologia de endereçamento: as rotas do painel Siser só
    /// respondem no host do Landlord; num host de cliente devolvem 404 (não revelam a superfície).
    /// FAIL-SAFE: sem Hosts:Landlord configurado, o guard é inativo — comportamento atual preservado
    /// (as rotas landlord seguem batendo no gate 1.11 sem host config, como os ~1034 testes existentes).
    /// </summary>
    [Collection(IntegrationWebAppCollection.Nome)]
    public class HostGuardLandlordTests
    {
        // GET real do Landlord (SuperAdmin -> dashboard, 200 para operador interno). O host-guard roda
        // ANTES da autorização; no host errado devolve 404 independentemente da auth. O 404 é o ÚNICO
        // sinal do host-guard nesta rota (a rota existe), o que torna as asserções precisas.
        private const string RotaLandlord = "/api/v1/plataforma/superadmin/dashboard";

        /// <summary>
        /// Host de teste isolado. <paramref name="hostLandlordConfigurado"/> liga/desliga Hosts:Landlord;
        /// cada instância usa um banco InMemory próprio para não sofrer contaminação da suíte paralela.
        /// </summary>
        private sealed class HostGuardWebAppFactory : WebApplicationFactory<Program>
        {
            private readonly bool _hostLandlordConfigurado;
            private readonly string _dbName;

            public HostGuardWebAppFactory(bool hostLandlordConfigurado, string dbName)
            {
                _hostLandlordConfigurado = hostLandlordConfigurado;
                _dbName = dbName;
            }

            protected override void ConfigureWebHost(IWebHostBuilder builder)
            {
                builder.UseEnvironment("Testing");
                builder.UseSetting("Seguranca:JwtSigningKey",
                    "chave-de-teste-de-assinatura-jwt-com-mais-de-32-chars-0123456789");

                // Guard ATIVO só quando configurado: painel do Landlord em painel.siser.local.
                if (_hostLandlordConfigurado)
                {
                    builder.UseSetting("Hosts:Landlord:0", "painel.siser.local");
                }

                builder.ConfigureServices(services =>
                {
                    // Mesma injeção de identidade por header dos demais testes de integração.
                    services.PostConfigure<Microsoft.AspNetCore.Authentication.AuthenticationOptions>(options =>
                    {
                        if (options.SchemeMap.TryGetValue(
                                Epros.API.Security.EprosTokenAuthenticationHandler.SchemeName, out var scheme))
                        {
                            scheme.HandlerType = typeof(HeaderTestAuthHandler);
                        }
                    });

                    RemoveDbContext<ContextGestaoClientes>(services);
                    RemoveDbContext<ContextEstoque>(services);
                    RemoveDbContext<ContextFiscal>(services);
                    RemoveDbContext<ContextFinanceiro>(services);
                    RemoveDbContext<ContextVendas>(services);

                    // EnableServiceProviderCaching(false): cada factory recebe seu PRÓPRIO service
                    // provider interno do EF (dono do ciclo de vida), em vez do provider cacheado
                    // globalmente e compartilhado entre TODAS as factories InMemory do processo (o nome
                    // do banco só seleciona o store, não o provider). Sem isso, quando uma factory é
                    // descartada ela derruba o provider compartilhado e uma requisição em voo de outra
                    // factory lança ObjectDisposedException sob execução paralela.
                    services.AddDbContext<ContextGestaoClientes>(o => o.UseInMemoryDatabase(_dbName).EnableServiceProviderCaching(false));
                    services.AddDbContext<ContextEstoque>(o => o.UseInMemoryDatabase(_dbName).EnableServiceProviderCaching(false));
                    services.AddDbContext<ContextFiscal>(o => o.UseInMemoryDatabase(_dbName).EnableServiceProviderCaching(false));
                    services.AddDbContext<ContextFinanceiro>(o => o.UseInMemoryDatabase(_dbName).EnableServiceProviderCaching(false));
                    services.AddDbContext<ContextVendas>(o => o.UseInMemoryDatabase(_dbName).EnableServiceProviderCaching(false));
                });
            }

            private static void RemoveDbContext<TContext>(IServiceCollection services) where TContext : DbContext
            {
                var d = services.SingleOrDefault(x => x.ServiceType == typeof(DbContextOptions<TContext>));
                if (d != null) services.Remove(d);
            }
        }

        private static HttpClient ClienteInterno(HostGuardWebAppFactory factory)
        {
            var client = factory.CreateClient();
            client.DefaultRequestHeaders.Add("X-Tenant-Id", "system");
            client.DefaultRequestHeaders.Add("X-User-Id", "op-interno");
            client.DefaultRequestHeaders.Add("X-Perfil-Id", "interno"); // passaria o gate 1.11
            return client;
        }

        [Fact]
        public async Task Com_Hosts_Landlord_Configurado_Rota_Landlord_No_Host_Errado_Retorna_404()
        {
            using var factory = new HostGuardWebAppFactory(hostLandlordConfigurado: true, dbName: "HostGuardDb_HostErrado");
            var client = ClienteInterno(factory);

            var req = new HttpRequestMessage(HttpMethod.Get, RotaLandlord);
            req.Headers.Host = "app.cliente.local"; // host de cliente, fora de Hosts:Landlord

            var response = await client.SendAsync(req);

            // Mesmo sendo operador interno REAL, o host de cliente não expõe a superfície do Landlord.
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task Com_Hosts_Landlord_Configurado_Rota_Landlord_No_Host_Certo_Passa_Do_Host_Guard()
        {
            using var factory = new HostGuardWebAppFactory(hostLandlordConfigurado: true, dbName: "HostGuardDb_HostCerto");
            var client = ClienteInterno(factory);

            var req = new HttpRequestMessage(HttpMethod.Get, RotaLandlord);
            req.Headers.Host = "painel.siser.local"; // host do Landlord

            var response = await client.SendAsync(req);

            // No host certo o guard NÃO barra: a requisição chega ao controller (dashboard -> 200).
            // Faz par com o host errado (== 404), provando que o guard decide pelo host.
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task Sem_Hosts_Landlord_Configurado_Nao_Bloqueia_Comportamento_Atual_Preservado()
        {
            using var factory = new HostGuardWebAppFactory(hostLandlordConfigurado: false, dbName: "HostGuardDb_FailSafe");
            var client = ClienteInterno(factory);

            // Sem Hosts:Landlord, mesmo com Host de "cliente" a rota Landlord NÃO é bloqueada.
            var req = new HttpRequestMessage(HttpMethod.Get, RotaLandlord);
            req.Headers.Host = "app.cliente.local";

            var response = await client.SendAsync(req);

            // Fail-safe: o guard não emite o 404 de host; a requisição segue o pipeline normal (gate 1.11)
            // e o dashboard responde 200 — exatamente como antes da mudança (suíte preservada).
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }
}
