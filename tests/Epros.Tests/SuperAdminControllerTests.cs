using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Epros.API.Controllers;
using Epros.API.Security;
using Epros.Modules.Aplicativo.Application.Commands;
using Epros.Modules.Aplicativo.Application.Handlers;
using Epros.Modules.Aplicativo.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Epros.Tests
{
    public class SuperAdminControllerTests
    {
        private ServiceProvider CreateServiceProvider(string databaseName)
        {
            var services = new ServiceCollection();

            var options = new DbContextOptionsBuilder<ContextAplicativo>()
                .UseInMemoryDatabase(databaseName)
                .Options;

            var tenantProvider = new TestTenantProvider("system");
            var currentUser = new TestCurrentUser("operador-admin");

            services.AddSingleton(new ContextAplicativo(options, tenantProvider, currentUser));
            services.AddSingleton<ITenantProvider>(tenantProvider);
            services.AddSingleton<ICurrentUser>(currentUser);
            services.AddSingleton<IPasswordHasher, Epros.Infrastructure.Services.Pbkdf2PasswordHasher>();

            services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssembly(typeof(CriarUsuarioInternoCommandHandler).Assembly);
            });

            return services.BuildServiceProvider();
        }

        [Fact]
        public void SuperAdminController_Deve_Ter_Atributo_AbacAuthorize_Restrito()
        {
            // Arrange & Act
            var type = typeof(SuperAdminController);
            var abacAttr = type.GetCustomAttribute<AbacAuthorizeAttribute>();

            // Assert
            Assert.NotNull(abacAttr);
            Assert.Equal("SuperAdmin", abacAttr!.Recurso);
            Assert.Equal("Configurar", abacAttr.Acao);
        }

        [Fact]
        public void AreaPublicaController_Nao_Deve_Ter_Atributo_AbacAuthorize_Restrito()
        {
            // Arrange & Act
            var type = typeof(AreaPublicaController);
            var abacAttr = type.GetCustomAttribute<AbacAuthorizeAttribute>();

            // Assert
            Assert.Null(abacAttr); // Aberto a acessos públicos
        }

        [Fact]
        public async Task SuperAdminController_Deve_Criar_UsuarioInterno_Com_Sucesso_Via_Api()
        {
            // Arrange
            var serviceProvider = CreateServiceProvider("db_api_user_create");
            var mediator = serviceProvider.GetRequiredService<IMediator>();
            var controller = new SuperAdminController(mediator);

            var command = new CriarUsuarioInternoCommand("Op Siser C", "op.c@siser.com", "senhaForte123", "America/Sao_Paulo", false);

            // Act
            var result = await controller.CriarUsuarioInterno(command);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var commandResult = Assert.IsType<CommandResult>(okResult.Value);
            Assert.True(commandResult.Sucesso);
        }

        [Fact]
        public async Task AreaPublicaController_Deve_Inscrever_Newsletter_Com_Sucesso_Via_Api()
        {
            // Arrange
            var serviceProvider = CreateServiceProvider("db_api_newsletter");
            var mediator = serviceProvider.GetRequiredService<IMediator>();
            var controller = new AreaPublicaController(mediator);

            var command = new InscreverNewsletterCommand("publico@teste.com");

            // Act
            var result = await controller.InscreverNewsletter(command);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var commandResult = Assert.IsType<CommandResult>(okResult.Value);
            Assert.True(commandResult.Sucesso);
        }

        #region Classes Auxiliares de Teste

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
            public string? GetUserName() => "Super Admin Controller Tester";
            public string? GetUserEmail() => "super.ctrl@siser.com";
        }

        #endregion
    }
}
