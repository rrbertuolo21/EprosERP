using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Epros.API.Security;
using Epros.Modules.Aplicativo.Application.Commands;
using Epros.Modules.Aplicativo.Application.Queries;
using Epros.Modules.Aplicativo.Application.Handlers;
using Epros.Modules.Aplicativo.Application.Services;
using Epros.Modules.Aplicativo.Domain.Entities;
using Epros.Modules.Aplicativo.Infrastructure.Data;
using Epros.Modules.GestaoClientes.Domain.Entities;
using Epros.Modules.GestaoClientes.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using MediatR;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Epros.Tests
{
    public class PermissoesMenuTests
    {
        private (ServiceProvider Provider, TestTenantProvider TenantProvider, TestCurrentUser CurrentUser) CreateServiceProvider(string databaseName, string tenantId = "tenant-test", string userId = "usr-test")
        {
            var services = new ServiceCollection();

            var optAplicativo = new DbContextOptionsBuilder<ContextAplicativo>()
                .UseInMemoryDatabase(databaseName)
                .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
                .Options;

            var optGestaoClientes = new DbContextOptionsBuilder<ContextGestaoClientes>()
                .UseInMemoryDatabase(databaseName)
                .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
                .Options;

            var tenantProvider = new TestTenantProvider(tenantId);
            var currentUser = new TestCurrentUser(userId);

            services.AddSingleton(new ContextAplicativo(optAplicativo, tenantProvider, currentUser));
            services.AddSingleton(new ContextGestaoClientes(optGestaoClientes, tenantProvider, currentUser));
            services.AddSingleton<ITenantProvider>(tenantProvider);
            services.AddSingleton<ICurrentUser>(currentUser);
            services.AddMemoryCache();
            services.AddSingleton<IPermissaoCacheManager, PermissaoCacheManager>();
            services.AddSingleton<Epros.Shared.Security.IEprosTokenService>(
                new Epros.Shared.Security.EprosTokenService("epros-test-signing-key-0123456789-abcdef"));

            services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssembly(typeof(AutenticarUsuarioCommandHandler).Assembly);
            });

            return (services.BuildServiceProvider(), tenantProvider, currentUser);
        }

        [Fact]
        public async Task CriarPerfilAcesso_Com_Descricao_Duplicada_Deve_Retornar_Falha()
        {
            // Arrange
            var (provider, _, _) = CreateServiceProvider("db_perfil_dup_desc");
            var mediator = provider.GetRequiredService<IMediator>();
            var contextGestao = provider.GetRequiredService<ContextGestaoClientes>();

            var perfilExistente = new PerfilAcesso("Financeiro", "tenant-test", "system");
            contextGestao.PerfisAcessos.Add(perfilExistente);
            await contextGestao.SaveChangesAsync();

            var command = new CriarPerfilAcessoCommand("Financeiro", new List<AcessoInputDto>());

            // Act
            var result = await mediator.Send(command);

            // Assert
            Assert.False(result.Sucesso);
            Assert.Contains(result.Erros, e => e.Contains("Já existe um perfil de acesso cadastrado com esta descrição"));
        }

        [Fact]
        public async Task SincronizarAcessos_Deve_Persistir_E_Atualizar_Corretamente()
        {
            // Arrange
            var (provider, _, _) = CreateServiceProvider("db_perfil_sync_acessos");
            var mediator = provider.GetRequiredService<IMediator>();
            var contextGestao = provider.GetRequiredService<ContextGestaoClientes>();

            var perfil = new PerfilAcesso("Operacional", "tenant-test", "system");
            contextGestao.PerfisAcessos.Add(perfil);
            await contextGestao.SaveChangesAsync();

            var menuId = Guid.NewGuid();
            var command = new AtualizarPerfilAcessoCommand(
                perfil.Id, 
                "Operacional Alterado", 
                new List<AcessoInputDto>
                {
                    new AcessoInputDto(menuId, null, null, true, false, false)
                }
            );

            // Act
            var result = await mediator.Send(command);

            // Assert
            Assert.True(result.Sucesso);

            var perfilDb = await contextGestao.PerfisAcessos
                .Include(p => p.Acessos)
                .FirstOrDefaultAsync(p => p.Id == perfil.Id);

            Assert.NotNull(perfilDb);
            Assert.Equal("Operacional Alterado", perfilDb!.Descricao);
            Assert.Single(perfilDb.Acessos);
            Assert.True(perfilDb.Acessos.First().Ver);
            Assert.False(perfilDb.Acessos.First().Editar);
        }

        [Fact]
        public async Task ObterAcessosSessao_Usuario_Admin_Deve_Ter_Bypass_De_Matriz()
        {
            // Arrange
            var (provider, _, _) = CreateServiceProvider("db_acessos_admin_bypass");
            var mediator = provider.GetRequiredService<IMediator>();
            var contextGestao = provider.GetRequiredService<ContextGestaoClientes>();
            var contextApp = provider.GetRequiredService<ContextAplicativo>();

            // Setup do Inquilino
            var cliente = SetupTenant(contextGestao, "tenant-test");
            var empresa = SetupEmpresa(contextGestao, "tenant-test", false);

            var usuario = new Usuario("tenant-test", "Admin User", "admin@teste.com", "Senha@123", UsuarioTipo.Company, "system");
            contextApp.Usuarios.Add(usuario);

            // Vínculo como Admin
            var vinculo = new UsuarioEmpresa("tenant-test", usuario.Id, empresa.Id, null, true, "system");
            contextApp.UsuariosEmpresas.Add(vinculo);

            // Adiciona um menu global no catálogo
            var menu = new Epros.Modules.GestaoClientes.Domain.Entities.Menu("Estoque", "box", "/estoque", 1, "estoque");
            contextGestao.Menus.Add(menu);

            await contextGestao.SaveChangesAsync();
            await contextApp.SaveChangesAsync();

            var query = new ObterAcessosSessaoQuery(usuario.Id, empresa.Id);

            // Act
            var result = await mediator.Send(query);

            // Assert
            Assert.True(result.Sucesso);
            var response = Assert.IsType<AcessosResponseDto>(result.Dados);
            Assert.True(response.IsAdmin);
            Assert.NotEmpty(response.Acesso);
            Assert.Equal("Estoque", response.Acesso.First().Menu);
        }

        [Fact]
        public async Task ObterAcessosSessao_Usuario_Comum_Com_Ver_False_Deve_Bloquear_Visibilidade()
        {
            // Arrange
            var (provider, _, _) = CreateServiceProvider("db_acessos_comum_ver_false");
            var mediator = provider.GetRequiredService<IMediator>();
            var contextGestao = provider.GetRequiredService<ContextGestaoClientes>();
            var contextApp = provider.GetRequiredService<ContextAplicativo>();

            var cliente = SetupTenant(contextGestao, "tenant-test");
            var empresa = SetupEmpresa(contextGestao, "tenant-test", false);

            // Menu no catálogo
            var menu = new Epros.Modules.GestaoClientes.Domain.Entities.Menu("Vendas", "shopping-cart", "/vendas", 1, "vendas");
            contextGestao.Menus.Add(menu);

            // Perfil com Ver = false
            var perfil = new PerfilAcesso("Operador", "tenant-test", "system");
            var acesso = new PerfilAcessoMenu(perfil.Id, menu.Id, null, null, false, false, false, "tenant-test", "system");
            perfil.SincronizarAcessos(new List<PerfilAcessoMenu> { acesso }, "system");
            contextGestao.PerfisAcessos.Add(perfil);

            var usuario = new Usuario("tenant-test", "Comum User", "comum@teste.com", "Senha@123", UsuarioTipo.Company, "system");
            contextApp.Usuarios.Add(usuario);

            // Vínculo comum
            var vinculo = new UsuarioEmpresa("tenant-test", usuario.Id, empresa.Id, perfil.Id, false, "system");
            contextApp.UsuariosEmpresas.Add(vinculo);

            await contextGestao.SaveChangesAsync();
            await contextApp.SaveChangesAsync();

            var query = new ObterAcessosSessaoQuery(usuario.Id, empresa.Id);

            // Act
            var result = await mediator.Send(query);

            // Assert
            Assert.True(result.Sucesso);
            var response = Assert.IsType<AcessosResponseDto>(result.Dados);
            Assert.False(response.IsAdmin);
            Assert.Empty(response.Acesso); // Como Ver é false, a árvore de acessos de tela deve vir vazia
        }

        [Fact]
        public async Task ObterAcessosSessao_Empresa_Regime_Nao_Mei_Deve_Ocultar_Menu_MEI()
        {
            // Arrange
            var (provider, _, _) = CreateServiceProvider("db_ocultar_mei");
            var mediator = provider.GetRequiredService<IMediator>();
            var contextGestao = provider.GetRequiredService<ContextGestaoClientes>();
            var contextApp = provider.GetRequiredService<ContextAplicativo>();

            SetupTenant(contextGestao, "tenant-test");
            var empresaNormal = SetupEmpresa(contextGestao, "tenant-test", false); // Não é MEI

            // Menu MEI (ID funcional 16 ou Módulo MEI)
            var menu = new Epros.Modules.GestaoClientes.Domain.Entities.Menu("MEI", "user", "/mei", 1, "MEI");
            var item1 = new Epros.Modules.GestaoClientes.Domain.Entities.MenuItemNivel1(menu.Id, "Declaração MEI", "file", "/mei/declaracao", 1, "MEI");
            menu.ItensNivel1.Add(item1);
            contextGestao.Menus.Add(menu);

            var usuario = new Usuario("tenant-test", "Admin User", "admin@teste.com", "Senha@123", UsuarioTipo.Company, "system");
            contextApp.Usuarios.Add(usuario);
            var vinculo = new UsuarioEmpresa("tenant-test", usuario.Id, empresaNormal.Id, null, true, "system");
            contextApp.UsuariosEmpresas.Add(vinculo);

            await contextGestao.SaveChangesAsync();
            await contextApp.SaveChangesAsync();

            var query = new ObterAcessosSessaoQuery(usuario.Id, empresaNormal.Id);

            // Act
            var result = await mediator.Send(query);

            // Assert
            Assert.True(result.Sucesso);
            var response = Assert.IsType<AcessosResponseDto>(result.Dados);
            Assert.Empty(response.Acesso); // O menu MEI foi ocultado porque a empresa não é MEI
        }

        [Fact]
        public async Task ObterAcessosSessao_Empresa_Regime_Mei_Deve_Exibir_Menu_MEI()
        {
            // Arrange
            var (provider, _, _) = CreateServiceProvider("db_exibir_mei");
            var mediator = provider.GetRequiredService<IMediator>();
            var contextGestao = provider.GetRequiredService<ContextGestaoClientes>();
            var contextApp = provider.GetRequiredService<ContextAplicativo>();

            SetupTenant(contextGestao, "tenant-test");
            var empresaMei = SetupEmpresa(contextGestao, "tenant-test", true); // É MEI

            var menu = new Epros.Modules.GestaoClientes.Domain.Entities.Menu("MEI", "user", "/mei", 1, "MEI");
            var item1 = new Epros.Modules.GestaoClientes.Domain.Entities.MenuItemNivel1(menu.Id, "Declaração MEI", "file", "/mei/declaracao", 1, "MEI");
            menu.ItensNivel1.Add(item1);
            contextGestao.Menus.Add(menu);

            var usuario = new Usuario("tenant-test", "Admin User", "admin@teste.com", "Senha@123", UsuarioTipo.Company, "system");
            contextApp.Usuarios.Add(usuario);
            var vinculo = new UsuarioEmpresa("tenant-test", usuario.Id, empresaMei.Id, null, true, "system");
            contextApp.UsuariosEmpresas.Add(vinculo);

            await contextGestao.SaveChangesAsync();
            await contextApp.SaveChangesAsync();

            var query = new ObterAcessosSessaoQuery(usuario.Id, empresaMei.Id);

            // Act
            var result = await mediator.Send(query);

            // Assert
            Assert.True(result.Sucesso);
            var response = Assert.IsType<AcessosResponseDto>(result.Dados);
            Assert.Single(response.Acesso);
            Assert.Equal("MEI", response.Acesso.First().Menu);
        }

        [Fact]
        public async Task ObterAcessosSessao_Tenant_Bloqueado_Por_Inadimplencia_Deve_Impedir_Acesso()
        {
            // Arrange
            var (provider, _, _) = CreateServiceProvider("db_inadimplencia_bloqueio");
            var mediator = provider.GetRequiredService<IMediator>();
            var contextGestao = provider.GetRequiredService<ContextGestaoClientes>();
            var contextApp = provider.GetRequiredService<ContextAplicativo>();

            var cliente = SetupTenant(contextGestao, "tenant-test");
            var empresa = SetupEmpresa(contextGestao, "tenant-test", false);

            var usuario = new Usuario("tenant-test", "Admin User", "admin@teste.com", "Senha@123", UsuarioTipo.Company, "system");
            contextApp.Usuarios.Add(usuario);
            var vinculo = new UsuarioEmpresa("tenant-test", usuario.Id, empresa.Id, null, true, "system");
            contextApp.UsuariosEmpresas.Add(vinculo);

            // Fatura com atraso superior a 15 dias
            var faturaAtrasada = new Fatura(cliente.Id, 150m, DateTime.UtcNow.AddDays(-20), "tenant-test", "system");
            contextGestao.Faturas.Add(faturaAtrasada);

            await contextGestao.SaveChangesAsync();
            await contextApp.SaveChangesAsync();

            var query = new ObterAcessosSessaoQuery(usuario.Id, empresa.Id);

            // Act
            var result = await mediator.Send(query);

            // Assert
            Assert.False(result.Sucesso);
            Assert.True(result.Block); // Retorna a flag de redirecionamento/bloqueio SaaS
        }

        [Fact]
        public async Task IPermissaoCacheManager_Deve_Cachear_Por_30_Minutos_E_Invalidar_Apos_Alterar_Perfil()
        {
            // Arrange
            var (provider, _, _) = CreateServiceProvider("db_cache_test");
            var cacheManager = provider.GetRequiredService<IPermissaoCacheManager>();
            var contextGestao = provider.GetRequiredService<ContextGestaoClientes>();

            var tenantId = "tenant-test";
            var usuarioId = Guid.NewGuid();
            var empresaId = Guid.NewGuid();
            var perfilId = Guid.NewGuid();

            var chamadasFactory = 0;
            Func<Task<List<PerfilAcessoMenu>>> factory = () =>
            {
                chamadasFactory++;
                var lista = new List<PerfilAcessoMenu>
                {
                    new PerfilAcessoMenu(perfilId, Guid.NewGuid(), null, null, true, false, false, tenantId, "system")
                };
                return Task.FromResult(lista);
            };

            // Act 1: Primeira chamada (chama a factory)
            var p1 = await cacheManager.ObterPermissoesEfetivasAsync(tenantId, usuarioId, empresaId, factory);
            Assert.Equal(1, chamadasFactory);

            // Act 2: Segunda chamada (recupera do cache, chamadasFactory não altera)
            var p2 = await cacheManager.ObterPermissoesEfetivasAsync(tenantId, usuarioId, empresaId, factory);
            Assert.Equal(1, chamadasFactory);

            // Act 3: Invalida o perfil
            cacheManager.InvalidarCachePerfil(tenantId, perfilId);

            // Act 4: Terceira chamada (chama a factory novamente após invalidação)
            var p3 = await cacheManager.ObterPermissoesEfetivasAsync(tenantId, usuarioId, empresaId, factory);
            Assert.Equal(2, chamadasFactory);
        }

        [Fact]
        public async Task PermissaoMenuFilter_Usuario_Comum_Sem_Permissao_Editar_Deve_Retornar_403()
        {
            // Arrange
            var (provider, _, _) = CreateServiceProvider("db_filter_test");
            var contextGestao = provider.GetRequiredService<ContextGestaoClientes>();
            var contextApp = provider.GetRequiredService<ContextAplicativo>();
            var cacheManager = provider.GetRequiredService<IPermissaoCacheManager>();

            var tenantId = "tenant-test";
            var menuId = Guid.NewGuid();

            // Setup
            var cliente = SetupTenant(contextGestao, tenantId);
            var empresa = SetupEmpresa(contextGestao, tenantId, false);

            var perfil = new PerfilAcesso("Operador", tenantId, "system");
            // Dá permissão apenas de Ver
            var acesso = new PerfilAcessoMenu(perfil.Id, menuId, null, null, true, false, false, tenantId, "system");
            perfil.SincronizarAcessos(new List<PerfilAcessoMenu> { acesso }, "system");
            contextGestao.PerfisAcessos.Add(perfil);

            var usuario = new Usuario(tenantId, "Comum User", "comum@teste.com", "Senha@123", UsuarioTipo.Company, "system");
            contextApp.Usuarios.Add(usuario);

            var vinculo = new UsuarioEmpresa(tenantId, usuario.Id, empresa.Id, perfil.Id, false, "system");
            contextApp.UsuariosEmpresas.Add(vinculo);

            await contextGestao.SaveChangesAsync();
            await contextApp.SaveChangesAsync();

            // Cria o filtro simulando a ação de Editar
            var tokenService = new Epros.Shared.Security.EprosTokenService("epros-test-signing-key-0123456789-abcdef");
            var filter = new PermissaoMenuFilter(menuId.ToString(), null, null, "Editar", contextApp, contextGestao, cacheManager, tokenService);

            // Contexto simulado do filtro
            var actionContext = new ActionContext(
                new DefaultHttpContext(),
                new Microsoft.AspNetCore.Routing.RouteData(),
                new Microsoft.AspNetCore.Mvc.Abstractions.ActionDescriptor()
            );

            // Injeta headers de teste
            actionContext.HttpContext.Request.Headers["X-Tenant-Id"] = tenantId;
            actionContext.HttpContext.Request.Headers["X-User-Id"] = usuario.Id.ToString();
            actionContext.HttpContext.Request.Headers["X-Empresa-Id"] = empresa.Id.ToString();

            var authorizationFilterContext = new AuthorizationFilterContext(actionContext, new List<IFilterMetadata>());

            // Act
            await filter.OnAuthorizationAsync(authorizationFilterContext);

            // Assert
            Assert.NotNull(authorizationFilterContext.Result);
            var jsonResult = Assert.IsType<JsonResult>(authorizationFilterContext.Result);
            Assert.Equal(403, jsonResult.StatusCode); // 403 Forbidden esperado
        }

        #region Helpers de Setup
        private Cliente SetupTenant(ContextGestaoClientes context, string tenantId)
        {
            var plano = new Plano("Plano Teste", 100m, tenantId, "system");
            plano.AdicionarModulo("estoque", "system");
            plano.AdicionarModulo("vendas", "system");
            plano.AdicionarModulo("financeiro", "system");
            plano.AdicionarModulo("MEI", "system");

            var cliente = new Cliente("Razao Test", "12345678000100", "teste@cliente.com", plano.Id, tenantId, "system");

            context.Planos.Add(plano);
            context.Clientes.Add(cliente);
            return cliente;
        }

        private Empresa SetupEmpresa(ContextGestaoClientes context, string tenantId, bool ehMei)
        {
            var endereco = new Epros.Modules.GestaoClientes.Domain.ValueObjects.Endereco("Rua", "123", "", "Centro", "00000000", "Cidade", "SP");
            var empresa = new Empresa(
                "Empresa Teste LTDA", 
                "Fantasia", 
                "12345678000100", 
                null, null, null, null, 
                RegimeTributario.SimplesNacional, 
                RegimeApuracao.Cumulativo, 
                null, null, null, null, null, null, null, null, null, null, 
                endereco, 
                tenantId, 
                "system",
                ehMei
            );

            context.Empresas.Add(empresa);
            return empresa;
        }
        #endregion

        #region Provedores de Teste
        public class TestTenantProvider : ITenantProvider
        {
            public string TenantId { get; set; }
            public TestTenantProvider(string tenantId) => TenantId = tenantId;
            public string GetTenantId() => TenantId;
        }

        public class TestCurrentUser : ICurrentUser
        {
            private readonly string _userId;
            public TestCurrentUser(string userId) => _userId = userId;
            public string? GetUserId() => _userId;
            public string? GetUserName() => "Test User";
            public string? GetUserEmail() => "test@epros.com";
        }
        #endregion
    }
}
