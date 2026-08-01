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
            // API agora exige autenticação (FallbackPolicy). Em ambiente de testes, o esquema EprosToken
            // autentica via header X-Tenant-Id — necessário para a requisição alcançar o controller e
            // então exercitar a formatação de erro (ProblemDetails) do ExcecaoGlobalMiddleware.
            client.DefaultRequestHeaders.Add("X-Tenant-Id", "tenant-teste-erro");
            client.DefaultRequestHeaders.Add("X-User-Id", "admin-erro");

            // ClientesController agora exige SuperAdmin:Configurar. Cadastra Administrador (ignora ACL no AbacFilter)
            // para a requisição passar da autorização e alcançar o mediator que lança a exceção simulada.
            using (var scope = factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ContextGestaoClientes>();
                var perfil = new PerfilColaborador("admin-erro", "Adm Erro", "adm@erro.com", "Administrador", "TI", 0m, "tenant-teste-erro", "system");
                typeof(Epros.Shared.Domain.Entities.EntidadeSaaSBase)
                    .GetField("<Id>k__BackingField", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!
                    .SetValue(perfil, Guid.NewGuid());
                db.PerfisUsuarios.Add(perfil);
                await db.SaveChangesAsync();
            }

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
            client.DefaultRequestHeaders.Add("X-Tenant-Id", "tenant-teste-customizado");
            client.DefaultRequestHeaders.Add("X-User-Id", "admin-customizado");

            var planoId = Guid.NewGuid();
            using (var scope = factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ContextGestaoClientes>();
                var plano = new Plano("Plano Teste", 99.90m, "tenant-teste-customizado", "user-teste");

                // Força a inserção de ID Guid gerado localmente
                typeof(Epros.Shared.Domain.Entities.EntidadeSaaSBase).GetField("<Id>k__BackingField", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!.SetValue(plano, planoId);

                db.Planos.Add(plano);

                // ClientesController agora exige SuperAdmin:Configurar — cadastra Administrador (ignora ACL no AbacFilter).
                var perfil = new PerfilColaborador("admin-customizado", "Adm Custom", "adm@custom.com", "Administrador", "TI", 0m, "tenant-teste-customizado", "system");
                typeof(Epros.Shared.Domain.Entities.EntidadeSaaSBase)
                    .GetField("<Id>k__BackingField", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!
                    .SetValue(perfil, Guid.NewGuid());
                db.PerfisUsuarios.Add(perfil);

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
                Assert.Equal("tenant-teste-customizado", cliente!.TenantId);
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
        public async Task Deve_Bloquear_Acesso_A_Modulo_Para_Tenant_Bloqueado()
        {
            using var factory = new CustomWebApplicationFactory();
            var client = factory.CreateClient();
            client.DefaultRequestHeaders.Add("X-Tenant-Id", "tenant-teste-bloqueado");

            // Acesso ao financeiro (financas) bloqueado para "tenant-teste-bloqueado" no ModuloTenantMiddleware
            var response = await client.GetAsync("/api/v1/financas/contas-pagar");

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("Módulo desabilitado", content);
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
            client.DefaultRequestHeaders.Add("X-Tenant-Id", "system");
            client.DefaultRequestHeaders.Add("X-User-Id", "admin-siser");

            // Cadastra o PerfilUsuario de Administrador no tenant "system"
            using (var scope = factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ContextGestaoClientes>();
                var perfil = new PerfilColaborador("admin-siser", "Adm Siser", "adm@siser.com", "Administrador", "TI", 0m, "system", "system");

                typeof(Epros.Shared.Domain.Entities.EntidadeSaaSBase)
                    .GetField("<Id>k__BackingField", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!
                    .SetValue(perfil, Guid.NewGuid());

                db.PerfisUsuarios.Add(perfil);
                await db.SaveChangesAsync();
            }

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

            // Retorno esperado do AbacFilter (ForbidResult -> 403 Forbidden)
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

                builder.ConfigureServices(services =>
                {
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
