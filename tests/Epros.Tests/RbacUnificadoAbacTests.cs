using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Epros.API.Security;
using Epros.API.Seed;
using Epros.Shared.Application.Contracts;
using Epros.Modules.GestaoClientes.Domain.Entities;
using Epros.Modules.GestaoClientes.Infrastructure.Data;

namespace Epros.Tests
{
    /// <summary>
    /// 1.09 (USUARIOS_E_PAPEIS) — prova do modelo de autorização UNIFICADO por RBAC no <see cref="AbacFilter"/>:
    /// capacidades efetivas = papéis (por empresa) + grant direto − deny direto (deny sobrepõe). Estes testes
    /// exercitam o CAMINHO NOVO (userId é Guid real). Os testes legados (Cargo=="Administrador", desconto,
    /// ACL fina) seguem em GestaoClientesTests e continuam válidos — o caminho legado foi preservado.
    /// </summary>
    public class RbacUnificadoAbacTests
    {
        private const string Tenant = "tenant-rbac";

        private static ContextGestaoClientes CreateContext(string db, string userId)
        {
            var options = new DbContextOptionsBuilder<ContextGestaoClientes>()
                .UseInMemoryDatabase(db)
                .Options;
            return new ContextGestaoClientes(options, new FakeTenant(Tenant), new FakeUser(userId));
        }

        private static AuthorizationFilterContext BuildFilterContext(Guid? empresaId)
        {
            var httpContext = new DefaultHttpContext();
            if (empresaId.HasValue)
            {
                var identity = new ClaimsIdentity(new[] { new Claim("empresaId", empresaId.Value.ToString()) }, "test");
                httpContext.User = new ClaimsPrincipal(identity);
            }
            var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
            return new AuthorizationFilterContext(actionContext, new List<IFilterMetadata>());
        }

        /// <summary>Semeia Capacidade + Papel + vínculo papel-capacidade, retornando os Ids.</summary>
        private static async Task<(Guid CapId, Guid PapelId)> SemearCapacidadeEPapelAsync(
            ContextGestaoClientes ctx, string recurso, string acao)
        {
            var cap = new Capacidade(
                name: CapacidadeCatalogoSeeder.NomeCapacidade(recurso, acao),
                label: $"{recurso} {acao}", module: recurso, addOn: null,
                permissionKey: CapacidadeCatalogoSeeder.NomeCapacidade(recurso, acao),
                tenantId: Tenant, criadoPor: "seed");
            ctx.Capacidades.Add(cap);

            var papel = new Papel("Papel Teste", "Papel Teste", null, true, false, null, null, null, Tenant, "seed");
            ctx.Papeis.Add(papel);
            await ctx.SaveChangesAsync();

            ctx.PapeisCapacidades.Add(new PapelCapacidade(papel.Id, cap.Id, Tenant, "seed"));
            await ctx.SaveChangesAsync();

            return (cap.Id, papel.Id);
        }

        // 1) LC-1 CONSERTADO: usuário com papel Administrador (capacidade exigida) recebe 2xx (Result null).
        [Fact]
        public async Task Usuario_Com_Papel_Que_Concede_Capacidade_Deve_Ser_Autorizado()
        {
            var db = Guid.NewGuid().ToString();
            var userId = Guid.NewGuid();
            using var ctx = CreateContext(db, userId.ToString());

            var (_, papelId) = await SemearCapacidadeEPapelAsync(ctx, "Faturas", "Cancelar");
            ctx.UsuariosPapeis.Add(new UsuarioPapel(userId, papelId, "Usuario", Tenant, "seed", empresaId: null));
            await ctx.SaveChangesAsync();

            var filter = new AbacFilter("Faturas", "Cancelar", ctx, new FakeUser(userId.ToString()), new FakeTenant(Tenant));
            var fc = BuildFilterContext(empresaId: null);

            await filter.OnAuthorizationAsync(fc);

            Assert.Null(fc.Result); // autorizado pelo RBAC unificado
        }

        // 2) Usuário sem a capacidade exigida é negado (403).
        [Fact]
        public async Task Usuario_Sem_A_Capacidade_Deve_Receber_403()
        {
            var db = Guid.NewGuid().ToString();
            var userId = Guid.NewGuid();
            using var ctx = CreateContext(db, userId.ToString());

            // Semeia uma capacidade/papel DIFERENTE do exigido e atribui ao usuário.
            var (_, papelId) = await SemearCapacidadeEPapelAsync(ctx, "Clientes", "Ver");
            ctx.UsuariosPapeis.Add(new UsuarioPapel(userId, papelId, "Usuario", Tenant, "seed", empresaId: null));
            await ctx.SaveChangesAsync();

            var filter = new AbacFilter("Faturas", "Cancelar", ctx, new FakeUser(userId.ToString()), new FakeTenant(Tenant));
            var fc = BuildFilterContext(empresaId: null);

            await filter.OnAuthorizationAsync(fc);

            Assert.IsType<ForbidResult>(fc.Result);
        }

        // 3) REG-041: deny direto SOBREPÕE o grant (via papel) → 403.
        [Fact]
        public async Task Deny_Direto_Deve_Sobrepor_Grant_Do_Papel()
        {
            var db = Guid.NewGuid().ToString();
            var userId = Guid.NewGuid();
            using var ctx = CreateContext(db, userId.ToString());

            var (capId, papelId) = await SemearCapacidadeEPapelAsync(ctx, "Faturas", "Cancelar");
            ctx.UsuariosPapeis.Add(new UsuarioPapel(userId, papelId, "Usuario", Tenant, "seed", empresaId: null));
            // Deny direto da MESMA capacidade concedida pelo papel.
            ctx.UsuariosCapacidades.Add(new UsuarioCapacidade(userId, capId, granted: false, Tenant, "seed"));
            await ctx.SaveChangesAsync();

            var filter = new AbacFilter("Faturas", "Cancelar", ctx, new FakeUser(userId.ToString()), new FakeTenant(Tenant));
            var fc = BuildFilterContext(empresaId: null);

            await filter.OnAuthorizationAsync(fc);

            Assert.IsType<ForbidResult>(fc.Result);
        }

        // 4) Papel POR EMPRESA: autorizado na empresa A (papel restrito a A), negado na empresa B.
        [Fact]
        public async Task Papel_Deve_Diferir_Por_Empresa()
        {
            var db = Guid.NewGuid().ToString();
            var userId = Guid.NewGuid();
            var empresaA = Guid.NewGuid();
            var empresaB = Guid.NewGuid();
            using var ctx = CreateContext(db, userId.ToString());

            var (_, papelId) = await SemearCapacidadeEPapelAsync(ctx, "Faturas", "Cancelar");
            ctx.UsuariosPapeis.Add(new UsuarioPapel(userId, papelId, "Usuario", Tenant, "seed", empresaId: empresaA));
            await ctx.SaveChangesAsync();

            // Empresa A → autorizado.
            var filterA = new AbacFilter("Faturas", "Cancelar", ctx, new FakeUser(userId.ToString()), new FakeTenant(Tenant));
            var fcA = BuildFilterContext(empresaId: empresaA);
            await filterA.OnAuthorizationAsync(fcA);
            Assert.Null(fcA.Result);

            // Empresa B → negado (o papel só vale para A).
            var filterB = new AbacFilter("Faturas", "Cancelar", ctx, new FakeUser(userId.ToString()), new FakeTenant(Tenant));
            var fcB = BuildFilterContext(empresaId: empresaB);
            await filterB.OnAuthorizationAsync(fcB);
            Assert.IsType<ForbidResult>(fcB.Result);
        }

        // 5) REG-040: grant direto (sem papel) concede a capacidade.
        [Fact]
        public async Task Grant_Direto_Deve_Conceder_Capacidade()
        {
            var db = Guid.NewGuid().ToString();
            var userId = Guid.NewGuid();
            using var ctx = CreateContext(db, userId.ToString());

            var cap = new Capacidade(
                name: CapacidadeCatalogoSeeder.NomeCapacidade("Faturas", "Cancelar"),
                label: "Faturas Cancelar", module: "Faturas", addOn: null,
                permissionKey: CapacidadeCatalogoSeeder.NomeCapacidade("Faturas", "Cancelar"),
                tenantId: Tenant, criadoPor: "seed");
            ctx.Capacidades.Add(cap);
            await ctx.SaveChangesAsync();

            ctx.UsuariosCapacidades.Add(new UsuarioCapacidade(userId, cap.Id, granted: true, Tenant, "seed"));
            await ctx.SaveChangesAsync();

            var filter = new AbacFilter("Faturas", "Cancelar", ctx, new FakeUser(userId.ToString()), new FakeTenant(Tenant));
            var fc = BuildFilterContext(empresaId: null);

            await filter.OnAuthorizationAsync(fc);

            Assert.Null(fc.Result);
        }

        private class FakeTenant : ITenantProvider
        {
            private readonly string _t;
            public FakeTenant(string t) => _t = t;
            public string GetTenantId() => _t;
        }

        private class FakeUser : ICurrentUser
        {
            private readonly string _u;
            public FakeUser(string u) => _u = u;
            public string? GetUserId() => _u;
            public string? GetUserName() => "rbac-test";
            public string? GetUserEmail() => "rbac@epros.com";
        }
    }
}
