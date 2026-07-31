using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Epros.API.Security;
using Epros.API.Seed;
using Epros.Modules.Aplicativo.Application.Handlers;
using Epros.Modules.Aplicativo.Application.Queries;
using Epros.Modules.Aplicativo.Application.Services;
using Epros.Modules.Aplicativo.Domain.Entities;
using Epros.Modules.Aplicativo.Infrastructure.Data;
using Epros.Modules.GestaoClientes.Application.Services;
using Epros.Modules.GestaoClientes.Domain.Entities;
using Epros.Modules.GestaoClientes.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Epros.Tests
{
    /// <summary>
    /// 1.10 (PERMISSOES_DE_MENU) — prova de que o MENU é PROJEÇÃO das capacidades efetivas do RBAC
    /// (decisão 1.09) e que menu × gate são coerentes (REG-002/REG-080/§8.2/CA-007). Cobre: visibilidade por
    /// capacidade, entitlement de plano, papel-por-empresa, ausência de escalonamento por "admin" afirmado, e
    /// o invariante "item visível ⇔ o AbacFilter autorizaria".
    /// </summary>
    public class MenuPorCapacidade110Tests
    {
        private const string Tenant = "tenant-test";

        private (ServiceProvider Provider, TestTenantProvider Tp, TestCurrentUser Cu) CreateProvider(string db, string userId)
        {
            var services = new ServiceCollection();
            var optApp = new DbContextOptionsBuilder<ContextAplicativo>()
                .UseInMemoryDatabase(db)
                .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
                .Options;
            var optGestao = new DbContextOptionsBuilder<ContextGestaoClientes>()
                .UseInMemoryDatabase(db)
                .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
                .Options;

            var tp = new TestTenantProvider(Tenant);
            var cu = new TestCurrentUser(userId);

            services.AddSingleton(new ContextAplicativo(optApp, tp, cu));
            services.AddSingleton(new ContextGestaoClientes(optGestao, tp, cu));
            services.AddSingleton<ITenantProvider>(tp);
            services.AddSingleton<ICurrentUser>(cu);
            services.AddMemoryCache();
            services.AddSingleton<IPermissaoCacheManager, PermissaoCacheManager>();
            services.AddScoped<ICapacidadesEfetivasService, CapacidadesEfetivasService>();
            services.AddSingleton<Epros.Shared.Security.IEprosTokenService>(
                new Epros.Shared.Security.EprosTokenService("epros-test-signing-key-0123456789-abcdef"));
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(AutenticarUsuarioCommandHandler).Assembly));

            return (services.BuildServiceProvider(), tp, cu);
        }

        // ---------- helpers de seed ----------
        private Cliente SetupTenantComPlano(ContextGestaoClientes ctx, params string[] modulos)
        {
            var plano = new Plano("Plano Teste", 100m, Tenant, "system");
            foreach (var m in modulos) plano.AdicionarModulo(m, "system");
            var cliente = new Cliente("Razao Test", "12345678000100", "teste@cliente.com", plano.Id, Tenant, "system");
            ctx.Planos.Add(plano);
            ctx.Clientes.Add(cliente);
            return cliente;
        }

        private Empresa SetupEmpresa(ContextGestaoClientes ctx, bool ehMei = false)
        {
            var endereco = new Epros.Modules.GestaoClientes.Domain.ValueObjects.Endereco("Rua", "123", "", "Centro", "00000000", "Cidade", "SP");
            var empresa = new Empresa("Empresa Teste LTDA", "Fantasia", "12345678000100", null, null, null, null,
                RegimeTributario.SimplesNacional, RegimeApuracao.Cumulativo,
                null, null, null, null, null, null, null, null, null, null, endereco, Tenant, "system", ehMei);
            ctx.Empresas.Add(empresa);
            return empresa;
        }

        private Guid SetupUsuarioComVinculo(ContextAplicativo ctxApp, Guid empresaId)
        {
            var usuario = new Usuario(Tenant, "User", $"u{Guid.NewGuid():N}@teste.com", "Senha@123", UsuarioTipo.Company, "system");
            ctxApp.Usuarios.Add(usuario);
            ctxApp.UsuariosEmpresas.Add(new UsuarioEmpresa(Tenant, usuario.Id, empresaId, null, false, "system"));
            return usuario.Id;
        }

        /// <summary>Semeia Capacidade "recurso:acao" + Papel que a concede + atribui ao usuário na empresa.</summary>
        private async Task ConcederCapacidadeAsync(ContextGestaoClientes ctx, Guid userId, string recurso, string acao, Guid? empresaId)
        {
            var nome = CapacidadeCatalogoSeeder.NomeCapacidade(recurso, acao);
            var cap = new Capacidade(nome, $"{recurso} {acao}", recurso, null, nome, Tenant, "seed");
            ctx.Capacidades.Add(cap);
            var papel = new Papel($"Papel {recurso}", "p", null, true, false, null, null, null, Tenant, "seed");
            ctx.Papeis.Add(papel);
            await ctx.SaveChangesAsync();
            ctx.PapeisCapacidades.Add(new PapelCapacidade(papel.Id, cap.Id, Tenant, "seed"));
            ctx.UsuariosPapeis.Add(new UsuarioPapel(userId, papel.Id, "Usuario", Tenant, "seed", empresaId: empresaId));
            await ctx.SaveChangesAsync();
        }

        // ---------- 1) menu só traz itens cuja capacidade o usuário tem ----------
        [Fact]
        public async Task Menu_Deve_Trazer_Somente_Itens_Cuja_Capacidade_Usuario_Possui()
        {
            var (provider, _, _) = CreateProvider(Guid.NewGuid().ToString(), Guid.NewGuid().ToString());
            var mediator = provider.GetRequiredService<IMediator>();
            var ctx = provider.GetRequiredService<ContextGestaoClientes>();
            var ctxApp = provider.GetRequiredService<ContextAplicativo>();

            SetupTenantComPlano(ctx);
            var empresa = SetupEmpresa(ctx);
            var userId = SetupUsuarioComVinculo(ctxApp, empresa.Id);

            // Item que o usuário PODE ver (tem clientes:ler) e item que NÃO pode (faturas:cancelar).
            var menuOk = new Menu("Clientes", "user", "/clientes", 1, null, "clientes:ler");
            var menuNo = new Menu("Faturas", "file", "/faturas", 2, null, "faturas:cancelar");
            ctx.Menus.AddRange(menuOk, menuNo);
            await ctx.SaveChangesAsync();
            await ctxApp.SaveChangesAsync();

            await ConcederCapacidadeAsync(ctx, userId, "Clientes", "Ler", empresa.Id);

            var result = await mediator.Send(new ObterMenuDoUsuarioQuery(userId, empresa.Id));
            Assert.True(result.Sucesso);
            var resp = Assert.IsType<MenuDoUsuarioResponseDto>(result.Dados);
            Assert.Single(resp.Menu);
            Assert.Equal("Clientes", resp.Menu.First().Menu);
            Assert.False(resp.IsAdmin);
        }

        // ---------- 2) item de módulo não contratado não aparece ----------
        [Fact]
        public async Task Menu_Nao_Deve_Trazer_Item_De_Modulo_Nao_Contratado()
        {
            var (provider, _, _) = CreateProvider(Guid.NewGuid().ToString(), Guid.NewGuid().ToString());
            var mediator = provider.GetRequiredService<IMediator>();
            var ctx = provider.GetRequiredService<ContextGestaoClientes>();
            var ctxApp = provider.GetRequiredService<ContextAplicativo>();

            // Plano contrata só "vendas"; menu RH é do módulo "rh".
            SetupTenantComPlano(ctx, "vendas");
            var empresa = SetupEmpresa(ctx);
            var userId = SetupUsuarioComVinculo(ctxApp, empresa.Id);

            var menuRh = new Menu("RH", "users", "/rh", 1, "rh", "rh:ler");
            ctx.Menus.Add(menuRh);
            await ctx.SaveChangesAsync();
            await ctxApp.SaveChangesAsync();

            // Mesmo TENDO a capacidade, o módulo não contratado esconde o item (entitlement 1.06).
            await ConcederCapacidadeAsync(ctx, userId, "Rh", "Ler", empresa.Id);

            var result = await mediator.Send(new ObterMenuDoUsuarioQuery(userId, empresa.Id));
            Assert.True(result.Sucesso);
            var resp = Assert.IsType<MenuDoUsuarioResponseDto>(result.Dados);
            Assert.Empty(resp.Menu);
        }

        // ---------- 3) menu difere por empresa (papel A × B) ----------
        [Fact]
        public async Task Menu_Deve_Diferir_Por_Empresa()
        {
            var (provider, _, _) = CreateProvider(Guid.NewGuid().ToString(), Guid.NewGuid().ToString());
            var mediator = provider.GetRequiredService<IMediator>();
            var ctx = provider.GetRequiredService<ContextGestaoClientes>();
            var ctxApp = provider.GetRequiredService<ContextAplicativo>();

            SetupTenantComPlano(ctx);
            var empresaA = SetupEmpresa(ctx);
            var empresaB = SetupEmpresa(ctx);
            var usuario = new Usuario(Tenant, "User", "multi@teste.com", "Senha@123", UsuarioTipo.Company, "system");
            ctxApp.Usuarios.Add(usuario);
            ctxApp.UsuariosEmpresas.Add(new UsuarioEmpresa(Tenant, usuario.Id, empresaA.Id, null, false, "system"));
            ctxApp.UsuariosEmpresas.Add(new UsuarioEmpresa(Tenant, usuario.Id, empresaB.Id, null, false, "system"));

            var menu = new Menu("Clientes", "user", "/clientes", 1, null, "clientes:ler");
            ctx.Menus.Add(menu);
            await ctx.SaveChangesAsync();
            await ctxApp.SaveChangesAsync();

            // Capacidade concedida SÓ na empresa A (papel restrito a A).
            await ConcederCapacidadeAsync(ctx, usuario.Id, "Clientes", "Ler", empresaA.Id);

            var respA = Assert.IsType<MenuDoUsuarioResponseDto>((await mediator.Send(new ObterMenuDoUsuarioQuery(usuario.Id, empresaA.Id))).Dados);
            var respB = Assert.IsType<MenuDoUsuarioResponseDto>((await mediator.Send(new ObterMenuDoUsuarioQuery(usuario.Id, empresaB.Id))).Dados);

            Assert.Single(respA.Menu);   // vê Clientes na A
            Assert.Empty(respB.Menu);    // não vê na B
        }

        // ---------- 4) "admin" afirmado não eleva privilégio (furo IsAdmin do corpo) ----------
        // A projeção NÃO tem flag de admin vinda do chamador: sem papel/capacidade, o menu vem vazio,
        // ainda que outrora o corpo pudesse dizer IsAdmin=true. Prova de que a fonte é o RBAC, não o request.
        [Fact]
        public async Task Menu_Sem_Papel_Nem_Capacidade_Deve_Vir_Vazio_Mesmo_Que_Chamador_Se_Diga_Admin()
        {
            var (provider, _, _) = CreateProvider(Guid.NewGuid().ToString(), Guid.NewGuid().ToString());
            var mediator = provider.GetRequiredService<IMediator>();
            var ctx = provider.GetRequiredService<ContextGestaoClientes>();
            var ctxApp = provider.GetRequiredService<ContextAplicativo>();

            SetupTenantComPlano(ctx);
            var empresa = SetupEmpresa(ctx);
            var userId = SetupUsuarioComVinculo(ctxApp, empresa.Id);

            var menu = new Menu("Clientes", "user", "/clientes", 1, null, "clientes:ler");
            ctx.Menus.Add(menu);
            await ctx.SaveChangesAsync();
            await ctxApp.SaveChangesAsync();

            var result = await mediator.Send(new ObterMenuDoUsuarioQuery(userId, empresa.Id));
            var resp = Assert.IsType<MenuDoUsuarioResponseDto>(result.Dados);
            Assert.Empty(resp.Menu);
            Assert.False(resp.IsAdmin);
        }

        // ---------- 5) INVARIANTE: item visível no menu ⇔ o AbacFilter autorizaria (coerência menu × gate) ----------
        [Fact]
        public async Task Invariante_Item_Visivel_No_Menu_Corresponde_Ao_Que_AbacFilter_Autoriza()
        {
            var (provider, _, _) = CreateProvider(Guid.NewGuid().ToString(), Guid.NewGuid().ToString());
            var mediator = provider.GetRequiredService<IMediator>();
            var ctx = provider.GetRequiredService<ContextGestaoClientes>();
            var ctxApp = provider.GetRequiredService<ContextAplicativo>();

            SetupTenantComPlano(ctx);
            var empresa = SetupEmpresa(ctx);
            var userId = SetupUsuarioComVinculo(ctxApp, empresa.Id); // Usuario ativo com Id == userId

            var menuVisivel = new Menu("Clientes", "user", "/clientes", 1, null, "clientes:ler");
            var menuOculto = new Menu("Faturas", "file", "/faturas", 2, null, "faturas:cancelar");
            ctx.Menus.AddRange(menuVisivel, menuOculto);
            await ctx.SaveChangesAsync();
            await ctxApp.SaveChangesAsync();

            await ConcederCapacidadeAsync(ctx, userId, "Clientes", "Ler", empresa.Id);

            // (a) Projeção de menu (fluxo real do endpoint): só "Clientes" aparece.
            var resp = Assert.IsType<MenuDoUsuarioResponseDto>((await mediator.Send(new ObterMenuDoUsuarioQuery(userId, empresa.Id))).Dados);
            Assert.Single(resp.Menu);
            Assert.Equal("Clientes", resp.Menu.First().Menu);

            // (b) Gate real (AbacFilter) concorda item a item — mesma fonte de capacidade.
            var gateClientes = await RodarAbacFilter(ctx, userId, empresa.Id, "Clientes", "Ler");
            var gateFaturas = await RodarAbacFilter(ctx, userId, empresa.Id, "Faturas", "Cancelar");

            Assert.Null(gateClientes);                    // visível ⇒ AbacFilter autoriza
            Assert.IsType<ForbidResult>(gateFaturas);     // oculto ⇒ AbacFilter nega
        }

        /// <summary>Roda o AbacFilter para (recurso,acao) na empresa e devolve o Result (null = autorizado).</summary>
        private async Task<IActionResult?> RodarAbacFilter(ContextGestaoClientes ctx, Guid userId, Guid empresaId, string recurso, string acao)
        {
            var filter = new AbacFilter(recurso, acao, ctx, new TestCurrentUser(userId.ToString()), new TestTenantProvider(Tenant));
            var http = new DefaultHttpContext();
            http.User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim("empresaId", empresaId.ToString()) }, "test"));
            var actionCtx = new ActionContext(http, new RouteData(), new ActionDescriptor());
            var fc = new AuthorizationFilterContext(actionCtx, new List<IFilterMetadata>());
            await filter.OnAuthorizationAsync(fc);
            return fc.Result;
        }

        public class TestTenantProvider : ITenantProvider
        {
            public string TenantId { get; set; }
            public TestTenantProvider(string t) => TenantId = t;
            public string GetTenantId() => TenantId;
        }

        public class TestCurrentUser : ICurrentUser
        {
            private readonly string _u;
            public TestCurrentUser(string u) => _u = u;
            public string? GetUserId() => _u;
            public string? GetUserName() => "Test User";
            public string? GetUserEmail() => "test@epros.com";
        }
    }
}
