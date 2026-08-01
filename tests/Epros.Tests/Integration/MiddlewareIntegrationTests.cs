using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Epros.API.Middlewares;
using Epros.Modules.GestaoClientes.Infrastructure.Data;
using Epros.Modules.GestaoClientes.Application.Commands;
using Epros.Modules.GestaoClientes.Domain.Entities;
using Epros.Modules.Estoque.Infrastructure.Data;
using Epros.Modules.Fiscal.Infrastructure.Data;
using Epros.Modules.Financeiro.Infrastructure.Data;
using Epros.Modules.Vendas.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using MediatR;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Epros.Tests.Integration
{
    public class MiddlewareIntegrationTests
    {
        [Fact]
        public async Task Deve_Formatar_Erro_Interno_Com_ProblemDetails_Sob_Excecao()
        {
            using var factory = new CustomWebApplicationFactory().WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IMediator));
                    if (descriptor != null) services.Remove(descriptor);
                    services.AddScoped<IMediator, ExceptionThrowingMediator>();
                });
            });

            var client = factory.CreateClient();
            // 1.11 fix #1 — ClientesController (SuperAdmin) só autoriza operador interno REAL. O antigo
            // atalho "Administrador de tenant comum" foi fechado; autenticamos como operador interno
            // (tenant="system" + perfilId="interno", PrimaryAdmin por default) para a requisição passar
            // da autorização e alcançar o mediator que lança a exceção simulada (ProblemDetails).
            client.DefaultRequestHeaders.Add("X-Tenant-Id", "system");
            client.DefaultRequestHeaders.Add("X-User-Id", "operador-interno-erro");
            client.DefaultRequestHeaders.Add("X-Perfil-Id", "interno");

            var command = new CriarClienteCommand("Cliente Teste Ltda", "12345678000100", "teste@cliente.com", Guid.NewGuid());

            var response = await client.PostAsJsonAsync("/api/v1/plataforma/clientes", command);

            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
            Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

            var problem = await response.Content.ReadFromJsonAsync<System.Text.Json.Nodes.JsonObject>();
            Assert.NotNull(problem);
            Assert.Equal("Erro interno do servidor", problem["title"]?.ToString());
            Assert.Equal("Ocorreu um erro inesperado no processamento da sua solicitação.", problem["detail"]?.ToString());
            Assert.NotNull(problem["traceId"]?.ToString());
        }

        [Fact]
        public async Task Deve_Resolver_TenantId_Corretamente_A_Partir_De_Header()
        {
            using var factory = new CustomWebApplicationFactory();
            var client = factory.CreateClient();
            // 1.11 fix #1/#2 — criar cliente é operação landlord: autentica como operador interno
            // (tenant="system"). O pipeline header->ITenantProvider->entidade continua sendo exercitado
            // (o X-Tenant-Id="system" flui até o TenantId do Cliente criado).
            client.DefaultRequestHeaders.Add("X-Tenant-Id", "system");
            client.DefaultRequestHeaders.Add("X-User-Id", "operador-interno-custom");
            client.DefaultRequestHeaders.Add("X-Perfil-Id", "interno");

            var planoId = Guid.NewGuid();
            using (var scope = factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ContextGestaoClientes>();
                var plano = new Plano("Plano Teste", 99.90m, "system", "user-teste");

                // Força a inserção de ID Guid gerado localmente
                typeof(Epros.Shared.Domain.Entities.EntidadeSaaSBase).GetField("<Id>k__BackingField", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!.SetValue(plano, planoId);

                db.Planos.Add(plano);
                await db.SaveChangesAsync();
            }

            var command = new CriarClienteCommand("Cliente Teste", "12345678000100", "teste@cliente.com", planoId);
            var response = await client.PostAsJsonAsync("/api/v1/plataforma/clientes", command);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            // Verifica se o cliente foi cadastrado com o Tenant correto no banco de dados em memória
            using (var scope = factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ContextGestaoClientes>();
                var cliente = await db.Clientes.IgnoreQueryFilters().FirstOrDefaultAsync(c => c.Cnpj == "12345678000100");
                Assert.NotNull(cliente);
                Assert.Equal("system", cliente!.TenantId);
            }
        }

        [Fact]
        public async Task Endpoint_Protegido_Sem_Credencial_Deve_Retornar_401()
        {
            // B2/S1: com a FallbackPolicy (RequireAuthenticatedUser), qualquer endpoint sem
            // [AllowAnonymous] rejeita requisições anônimas (sem token nem X-Tenant-Id) com 401.
            using var factory = new CustomWebApplicationFactory();
            var client = factory.CreateClient();

            var response = await client.GetAsync("/api/v1/plataforma/clientes");

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task Endpoint_Publico_Deve_Permitir_Anonimo()
        {
            // Rotas públicas ([AllowAnonymous]) continuam acessíveis sem credencial: o pipeline de
            // autorização não as bloqueia com 401. Usa-se um GET público de instalação (sem escrita).
            using var factory = new CustomWebApplicationFactory();
            var client = factory.CreateClient();

            var response = await client.GetAsync("/api/v1/installation/state");

            Assert.NotEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task Requisicao_Autenticada_Por_Header_Deve_Alcancar_O_Controller()
        {
            // Em ambiente de testes, o esquema EprosToken autentica via X-Tenant-Id — a requisição
            // passa da FallbackPolicy e chega ao controller (não retorna 401).
            using var factory = new CustomWebApplicationFactory();
            var client = factory.CreateClient();
            client.DefaultRequestHeaders.Add("X-Tenant-Id", "tenant-isolamento-a");

            var response = await client.GetAsync("/api/v1/plataforma/clientes");

            Assert.NotEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task Deve_Bloquear_Acesso_A_Modulo_Nao_Contratado_No_Plano()
        {
            // 1.06 — entitlement REAL: o stub demonstrativo (tenant hardcoded + /financas) foi
            // removido. O bloqueio agora cruza a ROTA do módulo com a FLAG do plano do tenant.
            // Semeamos um plano SEM o módulo Financeiro (flags nascem false) + cliente ativo nele;
            // a rota do Financeiro deve responder 403 "modulo_nao_contratado".
            using var factory = new CustomWebApplicationFactory();

            const string tenant = "tenant-sem-financeiro";
            Guid planoId;
            using (var scope = factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ContextGestaoClientes>();
                var plano = new Plano("Plano Básico Sem Financeiro", 99.90m, tenant, "seed"); // ModuloFinanceiro=false
                db.Planos.Add(plano);
                var cliente = new Cliente("Cliente Sem Financeiro Ltda", "22222222000122", "sf@cliente.com", plano.Id, tenant, "seed");
                db.Clientes.Add(cliente);
                await db.SaveChangesAsync();
                planoId = plano.Id;
            }

            var client = factory.CreateClient();
            client.DefaultRequestHeaders.Add("X-Tenant-Id", tenant);

            var response = await client.GetAsync("/api/v1/financeiro/contas-pagar");

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("modulo_nao_contratado", content);
        }

        [Fact]
        public async Task Deve_Permitir_Acesso_A_Modulo_Contratado_No_Plano()
        {
            // Contrapartida: plano COM a flag Financeiro → o ModuloTenantMiddleware NÃO barra por
            // entitlement. Isolamos o middleware: o corpo NÃO deve conter o código do gate de módulo
            // ("modulo_nao_contratado"). O que vier depois (401/403 de ABAC, 404, 200) é de outra camada.
            using var factory = new CustomWebApplicationFactory();

            const string tenant = "tenant-com-financeiro";
            using (var scope = factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ContextGestaoClientes>();
                var plano = new Plano("Plano Completo", 499.90m, null, 999, 99, null, tenant, "seed",
                    moduloCrm: true, moduloProjetos: true, moduloRh: true, moduloFinanceiro: true, moduloPdv: true);
                db.Planos.Add(plano);
                var cliente = new Cliente("Cliente Com Financeiro Ltda", "33333333000133", "cf@cliente.com", plano.Id, tenant, "seed");
                db.Clientes.Add(cliente);
                await db.SaveChangesAsync();
            }

            var client = factory.CreateClient();
            client.DefaultRequestHeaders.Add("X-Tenant-Id", tenant);

            var response = await client.GetAsync("/api/v1/financeiro/contas-pagar");

            var content = await response.Content.ReadAsStringAsync();
            Assert.DoesNotContain("modulo_nao_contratado", content);
        }

        [Fact]
        public void Deve_Mascarar_Dados_Sensiveis_Pelo_Metodo_Estatico()
        {
            var cpfOriginal = "Meu CPF é 123.456.789-00.";
            var cpfMascarado = DataMaskingMiddleware.MascararDadosSensiveis(cpfOriginal);
            Assert.Equal("Meu CPF é 123.***.***-00.", cpfMascarado);

            var cartaoOriginal = "O cartão 1234567812345678 foi digitado.";
            var cartaoMascarado = DataMaskingMiddleware.MascararDadosSensiveis(cartaoOriginal);
            Assert.Equal("O cartão ****-****-****-5678 foi digitado.", cartaoMascarado);
        }

        [Fact]
        public async Task Deve_Permitir_Acesso_A_Configuracoes_Para_Administrador_Siser()
        {
            using var factory = new CustomWebApplicationFactory();
            var client = factory.CreateClient();
            // 1.11 fix #1 — o caminho legítimo do landlord é o OPERADOR INTERNO real (perfilId="interno"),
            // não o cargo "Administrador" de um PerfilColaborador. Autentica como operador interno.
            client.DefaultRequestHeaders.Add("X-Tenant-Id", "system");
            client.DefaultRequestHeaders.Add("X-User-Id", "operador-interno-siser");
            client.DefaultRequestHeaders.Add("X-Perfil-Id", "interno");

            var command = new DefinirConfiguracaoGlobalCommand("trial_days", "30", false, "Dias de Trial");
            var response = await client.PostAsJsonAsync("/api/v1/plataforma/configuracoes", command);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            // Valida se salvou no banco
            using (var scope = factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ContextGestaoClientes>();
                var config = await db.ConfiguracoesGlobais.FirstOrDefaultAsync(c => c.Chave == "trial_days");
                Assert.NotNull(config);
                Assert.Equal("30", config!.Valor);
            }
        }

        [Fact]
        public async Task Deve_Bloquear_Acesso_A_Configuracoes_Para_Usuario_Siser_Sem_Permissao()
        {
            using var factory = new CustomWebApplicationFactory();
            var client = factory.CreateClient();
            client.DefaultRequestHeaders.Add("X-Tenant-Id", "system");
            client.DefaultRequestHeaders.Add("X-User-Id", "operator-siser");

            // Cadastra Perfil de Operador (sem permissão explícita para SuperAdmin:Configurar)
            using (var scope = factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ContextGestaoClientes>();
                var perfil = new PerfilColaborador("operator-siser", "Op Siser", "op@siser.com", "Operador", "TI", 0m, "system", "system");

                typeof(Epros.Shared.Domain.Entities.EntidadeSaaSBase)
                    .GetField("<Id>k__BackingField", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!
                    .SetValue(perfil, Guid.NewGuid());

                db.PerfisUsuarios.Add(perfil);
                await db.SaveChangesAsync();
            }

            var command = new DefinirConfiguracaoGlobalCommand("trial_days", "30", false, "Dias de Trial");
            var response = await client.PostAsJsonAsync("/api/v1/plataforma/configuracoes", command);

            // 1.11 fix #1 — sem perfilId="interno" a identidade NÃO é operador interno; recurso SuperAdmin
            // exige operador interno real. O AbacFilter responde ForbidResult (403).
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        private class ExceptionThrowingMediator : IMediator
        {
            public Task Publish(object notification, CancellationToken cancellationToken = default) => Task.CompletedTask;
            public Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default) where TNotification : INotification => Task.CompletedTask;

            public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
            {
                throw new InvalidOperationException("Erro de negócio simulado na API.");
            }

            public Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default) where TRequest : IRequest
            {
                throw new InvalidOperationException("Erro de negócio simulado na API.");
            }

            public Task<object?> Send(object request, CancellationToken cancellationToken = default)
            {
                throw new InvalidOperationException("Erro de negócio simulado na API.");
            }

            public IAsyncEnumerable<TResponse> CreateStream<TResponse>(IStreamRequest<TResponse> request, CancellationToken cancellationToken = default) => throw new NotImplementedException();
            public IAsyncEnumerable<object?> CreateStream(object request, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        }

        private class TestCurrentUser : ICurrentUser
        {
            private readonly Microsoft.AspNetCore.Http.IHttpContextAccessor _httpContextAccessor;

            public TestCurrentUser(Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor)
            {
                _httpContextAccessor = httpContextAccessor;
            }

            public string? GetUserId()
            {
                var httpContext = _httpContextAccessor.HttpContext;
                if (httpContext != null && httpContext.Request.Headers.TryGetValue("X-User-Id", out var userId))
                {
                    return userId.ToString();
                }
                return "system"; // fallback
            }

            public string? GetUserName() => "Test Landlord User";
            public string? GetUserEmail() => "landlord@epros.com";
        }

        public class CustomWebApplicationFactory : WebApplicationFactory<Program>
        {
            protected override void ConfigureWebHost(IWebHostBuilder builder)
            {
                builder.UseEnvironment("Testing");

                // "Testing" é tratado como ambiente deployado (fail-closed): o segredo de assinatura
                // do token NÃO cai mais no valor fixo de dev por causa do ambiente. O host de teste
                // fornece a chave explicitamente — como um deploy real faria via env/secret.
                builder.UseSetting("Seguranca:JwtSigningKey", "chave-de-teste-de-assinatura-jwt-com-mais-de-32-chars-0123456789");

                builder.ConfigureServices(services =>
                {
                    // Injeta a identidade pelo HOST DE TESTE (fechamento do "gato"): remapeia o
                    // esquema "EprosToken" para um handler que lê X-Tenant-Id/X-User-Id/etc. Assim os
                    // testes de integração continuam autenticando por header SEM que o runtime
                    // deployado tenha qualquer caminho de header. A FallbackPolicy autentica pelo
                    // esquema "EprosToken", então o remap do HandlerType é suficiente.
                    services.PostConfigure<Microsoft.AspNetCore.Authentication.AuthenticationOptions>(options =>
                    {
                        if (options.SchemeMap.TryGetValue(
                                Epros.API.Security.EprosTokenAuthenticationHandler.SchemeName, out var scheme))
                        {
                            scheme.HandlerType = typeof(HeaderTestAuthHandler);
                        }
                    });

                    // Remove as opções e contextos default PostgreSQL
                    RemoveDbContext<ContextGestaoClientes>(services);
                    RemoveDbContext<ContextEstoque>(services);
                    RemoveDbContext<ContextFiscal>(services);
                    RemoveDbContext<ContextFinanceiro>(services);
                    RemoveDbContext<ContextVendas>(services);

                    // Registra com InMemoryDb
                    services.AddDbContext<ContextGestaoClientes>(options => options.UseInMemoryDatabase("InMemoryDbForTesting"));
                    services.AddDbContext<ContextEstoque>(options => options.UseInMemoryDatabase("InMemoryDbForTesting"));
                    services.AddDbContext<ContextFiscal>(options => options.UseInMemoryDatabase("InMemoryDbForTesting"));
                    services.AddDbContext<ContextFinanceiro>(options => options.UseInMemoryDatabase("InMemoryDbForTesting"));
                    services.AddDbContext<ContextVendas>(options => options.UseInMemoryDatabase("InMemoryDbForTesting"));

                    // Substitui ICurrentUser pelo dynamic mock local
                    var descriptorUser = services.SingleOrDefault(d => d.ServiceType == typeof(ICurrentUser));
                    if (descriptorUser != null) services.Remove(descriptorUser);
                    services.AddScoped<ICurrentUser, TestCurrentUser>();
                });
            }

            private static void RemoveDbContext<TContext>(IServiceCollection services) where TContext : DbContext
            {
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<TContext>));
                if (descriptor != null) services.Remove(descriptor);
            }
        }
    }
}
