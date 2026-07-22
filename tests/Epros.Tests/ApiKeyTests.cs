using System;
using System.IO;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Epros.Modules.Aplicativo.Application.Commands;
using Epros.Modules.Aplicativo.Application.Handlers;
using Epros.Modules.Aplicativo.Domain.Entities;
using Epros.Modules.Aplicativo.Infrastructure.Data;
using Epros.API.Middlewares;
using Epros.Shared.Application.Contracts;
using Xunit;

namespace Epros.Tests
{
    public class ApiKeyTests
    {
        [Fact]
        public async Task Deve_Gerar_ApiKey_Com_Sucesso_E_Gravar_Politicas()
        {
            var options = new DbContextOptionsBuilder<ContextAplicativo>()
                .UseInMemoryDatabase("db_apikey_gerar_sucesso")
                .Options;

            var tenantProvider = new TestTenantProvider("tenant-123");
            var currentUser = new TestCurrentUser("user-123");
            using var context = new ContextAplicativo(options, tenantProvider, currentUser);

            var usuario = new Usuario("tenant-123", "User ApiKey", "api@teste.com", "senha123", UsuarioTipo.Company, "user-123");
            context.Usuarios.Add(usuario);
            await context.SaveChangesAsync();

            var handler = new GerarApiKeyCommandHandler(context, currentUser);
            var command = new GerarApiKeyCommand(usuario.Id, RateLimit: 120, ValidadeDias: 45);

            var result = await handler.Handle(command, CancellationToken.None);

            Assert.True(result.Sucesso);
            Assert.NotNull(usuario.ApiKey);
            Assert.NotNull(usuario.ApiKeyCreated);
            Assert.NotNull(usuario.ApiKeyExpiration);
            Assert.Equal(120, usuario.ApiKeyRateLimit);
            Assert.True((DateTime.UtcNow - usuario.ApiKeyCreated.Value).TotalMinutes < 2);
            Assert.Equal(usuario.ApiKeyCreated.Value.AddDays(45).Date, usuario.ApiKeyExpiration.Value.Date);
        }

        [Fact]
        public async Task Middleware_Deve_Autenticar_E_Definir_Tenant_Com_ApiKey_Valida()
        {
            var tenantProvider = new TestTenantProvider("tenant-456");
            var currentUser = new TestCurrentUser("user-456");

            var serviceProvider = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .AddDbContext<ContextAplicativo>(opt => opt.UseInMemoryDatabase("db_apikey_middleware_auth"))
                .AddSingleton<ITenantProvider>(tenantProvider)
                .AddSingleton<ICurrentUser>(currentUser)
                .BuildServiceProvider();

            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ContextAplicativo>();

            var usuario = new Usuario("tenant-456", "User API Programatico", "program@teste.com", "senha123", UsuarioTipo.Company, "user-456");
            usuario.GerarApiKey(100, 30, "user-456");
            context.Usuarios.Add(usuario);
            await context.SaveChangesAsync();

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers["X-API-Key"] = usuario.ApiKey;
            httpContext.RequestServices = serviceProvider;

            var memoryCache = new MemoryCache(new MemoryCacheOptions());
            var middleware = new ApiKeyMiddleware(next: (innerContext) => Task.CompletedTask);

            await middleware.InvokeAsync(httpContext, memoryCache);

            Assert.NotNull(httpContext.User);
            Assert.True(httpContext.User.Identity?.IsAuthenticated);
            Assert.Equal(usuario.Id.ToString(), httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            Assert.Equal("tenant-456", httpContext.Items["TenantId"] as string);

            // Verifica se atualizou o último uso
            context.ChangeTracker.Clear();
            var dbUsuario = await context.Usuarios.FindAsync(usuario.Id);
            Assert.NotNull(dbUsuario?.ApiKeyLastUsed);
            Assert.True((DateTime.UtcNow - dbUsuario.ApiKeyLastUsed.Value).TotalMinutes < 2);
        }

        [Fact]
        public async Task Middleware_Deve_Rejeitar_Se_ApiKey_Expirada()
        {
            var tenantProvider = new TestTenantProvider("tenant-789");
            var currentUser = new TestCurrentUser("user-789");

            var serviceProvider = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .AddDbContext<ContextAplicativo>(opt => opt.UseInMemoryDatabase("db_apikey_middleware_expired"))
                .AddSingleton<ITenantProvider>(tenantProvider)
                .AddSingleton<ICurrentUser>(currentUser)
                .BuildServiceProvider();

            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ContextAplicativo>();

            var usuario = new Usuario("tenant-789", "Expired User", "expired@teste.com", "senha123", UsuarioTipo.Company, "user-789");

            // Gerando API Key com validade de -1 dias (expirada)
            usuario.GerarApiKey(60, -1, "user-789");
            context.Usuarios.Add(usuario);
            await context.SaveChangesAsync();

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers["X-API-Key"] = usuario.ApiKey;
            httpContext.RequestServices = serviceProvider;

            var responseStream = new MemoryStream();
            httpContext.Response.Body = responseStream;

            var memoryCache = new MemoryCache(new MemoryCacheOptions());
            var middleware = new ApiKeyMiddleware(next: (innerContext) => Task.CompletedTask);

            await middleware.InvokeAsync(httpContext, memoryCache);

            Assert.Equal(StatusCodes.Status401Unauthorized, httpContext.Response.StatusCode);

            responseStream.Seek(0, SeekOrigin.Begin);
            using var reader = new StreamReader(responseStream);
            var responseBody = await reader.ReadToEndAsync();
            Assert.Contains("expirada", responseBody);
        }

        [Fact]
        public async Task Middleware_Deve_Retornar_429_Se_RateLimit_Excedido()
        {
            var tenantProvider = new TestTenantProvider("tenant-limit");
            var currentUser = new TestCurrentUser("user-limit");

            var serviceProvider = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .AddDbContext<ContextAplicativo>(opt => opt.UseInMemoryDatabase("db_apikey_middleware_ratelimit"))
                .AddSingleton<ITenantProvider>(tenantProvider)
                .AddSingleton<ICurrentUser>(currentUser)
                .BuildServiceProvider();

            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ContextAplicativo>();

            var usuario = new Usuario("tenant-limit", "Limited User", "limit@teste.com", "senha123", UsuarioTipo.Company, "user-limit");
            // Limite de apenas 2 requisições por minuto
            usuario.GerarApiKey(2, 30, "user-limit");
            context.Usuarios.Add(usuario);
            await context.SaveChangesAsync();

            var memoryCache = new MemoryCache(new MemoryCacheOptions());
            var middleware = new ApiKeyMiddleware(next: (innerContext) => Task.CompletedTask);

            // Requisição 1 (Ok)
            var context1 = new DefaultHttpContext();
            context1.Request.Headers["X-API-Key"] = usuario.ApiKey;
            context1.RequestServices = serviceProvider;
            await middleware.InvokeAsync(context1, memoryCache);
            Assert.Equal(StatusCodes.Status200OK, context1.Response.StatusCode);

            // Requisição 2 (Ok)
            var context2 = new DefaultHttpContext();
            context2.Request.Headers["X-API-Key"] = usuario.ApiKey;
            context2.RequestServices = serviceProvider;
            await middleware.InvokeAsync(context2, memoryCache);
            Assert.Equal(StatusCodes.Status200OK, context2.Response.StatusCode);

            // Requisição 3 (Excedido -> 429)
            var context3 = new DefaultHttpContext();
            context3.Request.Headers["X-API-Key"] = usuario.ApiKey;
            context3.RequestServices = serviceProvider;
            var responseStream = new MemoryStream();
            context3.Response.Body = responseStream;

            await middleware.InvokeAsync(context3, memoryCache);

            Assert.Equal(StatusCodes.Status429TooManyRequests, context3.Response.StatusCode);

            responseStream.Seek(0, SeekOrigin.Begin);
            using var reader = new StreamReader(responseStream);
            var responseBody = await reader.ReadToEndAsync();
            Assert.Contains("excedido", responseBody);
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
            public string? GetUserName() => "API Key Test User";
            public string? GetUserEmail() => "api-test@epros.com";
        }
    }
}
