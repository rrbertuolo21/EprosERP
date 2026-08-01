using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Epros.API.Security;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Modules.GestaoClientes.Application.Commands;
using Epros.Modules.GestaoClientes.Application.Handlers;
using Epros.Modules.GestaoClientes.Application.Security;
using Epros.Modules.GestaoClientes.Domain.Entities;
using Epros.Modules.GestaoClientes.Infrastructure.Data;
using Epros.Modules.Aplicativo.Application.Commands;
using Epros.Modules.Aplicativo.Application.Handlers;
using Epros.Modules.Aplicativo.Application.Queries;
using Epros.Modules.Aplicativo.Domain.Entities;
using Epros.Modules.Aplicativo.Infrastructure.Data;
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
    /// 1.11 (OPERACAO_SUPER_ADMIN) — prova dos 4 fixes de segurança + diferenciação de perfis de
    /// suporte (Técnico × Negócio) por menor privilégio. Ver DECISOES_IMPLANTACAO_V1.md.
    /// </summary>
    public class SuperAdminSeguranca111Tests
    {
        // ===================== helpers =====================

        private static ContextGestaoClientes CtxGestao(string tenant, string userId)
            => new ContextGestaoClientes(
                new DbContextOptionsBuilder<ContextGestaoClientes>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options,
                new FakeTenant(tenant), new FakeUser(userId));

        private static ContextAplicativo CtxAplicativo(string tenant, string userId)
            => new ContextAplicativo(
                new DbContextOptionsBuilder<ContextAplicativo>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options,
                new FakeTenant(tenant), new FakeUser(userId));

        private static AuthorizationFilterContext FilterCtxComClaims(IEnumerable<Claim> claims)
        {
            var httpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(claims, "test"))
            };
            var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
            return new AuthorizationFilterContext(actionContext, new List<IFilterMetadata>());
        }

        private static IEnumerable<Claim> ClaimsInterno(bool primaryAdmin, string perfilSuporte)
            => new[]
            {
                new Claim("perfilId", "interno"),
                new Claim(SuperAdminSeguranca.ClaimPrimaryAdmin, primaryAdmin ? "true" : "false"),
                new Claim(SuperAdminSeguranca.ClaimPerfilSuporte, perfilSuporte)
            };

        // ===================== FIX #1: escalonamento p/ SuperAdmin fechado =====================

        [Fact]
        public async Task Fix1_Admin_De_Tenant_Comum_Recebe_403_Em_Recurso_SuperAdmin()
        {
            var userId = Guid.NewGuid();
            using var ctx = CtxGestao("tenant-comum", userId.ToString());
            // Cargo "Administrador" (fallback legado) NÃO pode mais autorizar recurso SuperAdmin.
            ctx.PerfisUsuarios.Add(new PerfilColaborador(userId.ToString(), "Adm", "a@t.com", "Administrador", "TI", 0m, "tenant-comum", "seed"));
            await ctx.SaveChangesAsync();

            var filter = new AbacFilter("SuperAdmin", "Configurar", ctx, new FakeUser(userId.ToString()), new FakeTenant("tenant-comum"));
            var fc = FilterCtxComClaims(Array.Empty<Claim>());

            await filter.OnAuthorizationAsync(fc);

            Assert.IsType<ForbidResult>(fc.Result);
        }

        [Fact]
        public async Task Fix1_Fallback_Cargo_Administrador_Ainda_Vale_Para_Recurso_Nao_Super()
        {
            // Preserva 1.05/1.09: o fallback legado continua para recursos NÃO-super (não quebra testes).
            var userId = Guid.NewGuid();
            using var ctx = CtxGestao("tenant-comum", userId.ToString());
            ctx.PerfisUsuarios.Add(new PerfilColaborador(userId.ToString(), "Adm", "a@t.com", "Administrador", "TI", 0m, "tenant-comum", "seed"));
            await ctx.SaveChangesAsync();

            var filter = new AbacFilter("Faturas", "Cancelar", ctx, new FakeUser(userId.ToString()), new FakeTenant("tenant-comum"));
            var fc = FilterCtxComClaims(Array.Empty<Claim>());

            await filter.OnAuthorizationAsync(fc);

            Assert.Null(fc.Result); // autorizado pelo fallback legado (recurso não-super)
        }

        [Fact]
        public async Task Fix1_Operador_Interno_Real_Autoriza_Recurso_SuperAdmin()
        {
            using var ctx = CtxGestao("system", "op-interno");
            var filter = new AbacFilter("SuperAdmin", "Configurar", ctx, new FakeUser("op-interno"), new FakeTenant("system"));
            var fc = FilterCtxComClaims(ClaimsInterno(primaryAdmin: true, perfilSuporte: "Nenhum"));

            await filter.OnAuthorizationAsync(fc);

            Assert.Null(fc.Result);
        }

        // ===================== DECISÃO #5: Técnico × Negócio (menor privilégio) =====================

        [Theory]
        // perfilSuporte, recurso, deveAutorizar
        [InlineData("SuporteNegocio", SuperAdminSeguranca.RecursoSuporteComercial, true)]
        [InlineData("SuporteTecnico", SuperAdminSeguranca.RecursoSuporteComercial, false)]
        [InlineData("SuporteTecnico", SuperAdminSeguranca.RecursoSuporteInfra, true)]
        [InlineData("SuporteNegocio", SuperAdminSeguranca.RecursoSuporteInfra, false)]
        [InlineData("SuporteNegocio", "SuperAdmin", true)]  // faixa Ambos
        [InlineData("SuporteTecnico", "SuperAdmin", true)]  // faixa Ambos
        public async Task Decisao5_Faixa_De_Suporte_Diferencia_Tecnico_E_Negocio(string perfil, string recurso, bool deveAutorizar)
        {
            using var ctx = CtxGestao("system", "op-interno");
            var filter = new AbacFilter(recurso, "Configurar", ctx, new FakeUser("op-interno"), new FakeTenant("system"));
            var fc = FilterCtxComClaims(ClaimsInterno(primaryAdmin: false, perfilSuporte: perfil));

            await filter.OnAuthorizationAsync(fc);

            if (deveAutorizar) Assert.Null(fc.Result);
            else Assert.IsType<ForbidResult>(fc.Result);
        }

        [Theory]
        [InlineData(SuperAdminSeguranca.RecursoSuporteComercial)]
        [InlineData(SuperAdminSeguranca.RecursoSuporteInfra)]
        [InlineData("SuperAdmin")]
        public async Task Decisao5_PrimaryAdmin_Passa_Em_Ambas_As_Faixas(string recurso)
        {
            using var ctx = CtxGestao("system", "lord");
            var filter = new AbacFilter(recurso, "Configurar", ctx, new FakeUser("lord"), new FakeTenant("system"));
            var fc = FilterCtxComClaims(ClaimsInterno(primaryAdmin: true, perfilSuporte: "Nenhum"));

            await filter.OnAuthorizationAsync(fc);

            Assert.Null(fc.Result);
        }

        [Fact]
        public async Task Decisao5_Interno_Sem_Perfil_De_Suporte_E_Nao_Primary_E_Negado()
        {
            using var ctx = CtxGestao("system", "op-sem-perfil");
            var filter = new AbacFilter("SuperAdmin", "Configurar", ctx, new FakeUser("op-sem-perfil"), new FakeTenant("system"));
            var fc = FilterCtxComClaims(ClaimsInterno(primaryAdmin: false, perfilSuporte: "Nenhum"));

            await filter.OnAuthorizationAsync(fc);

            Assert.IsType<ForbidResult>(fc.Result);
        }

        // ===================== FIX #2: guard uniforme nos 5 handlers landlord =====================

        [Fact]
        public void Fix2_GuardaOperadorInterno_Bloqueia_Tenant_Nao_System()
        {
            Assert.NotNull(GuardaOperadorInterno.Exigir(new FakeTenant("tenant-x")));
            Assert.Null(GuardaOperadorInterno.Exigir(new FakeTenant("system")));
        }

        [Fact]
        public async Task Fix2_CriarCliente_Rejeita_Tenant_Nao_System()
        {
            using var ctx = CtxGestao("tenant-x", "u");
            var handler = new CriarClienteCommandHandler(ctx, new FakeTenant("tenant-x"), new FakeUser("u"));
            var result = await handler.Handle(new CriarClienteCommand("Razao", "12345678000199", "e@e.com", Guid.NewGuid()), CancellationToken.None);
            AssertBloqueadoLandlord(result);
        }

        [Fact]
        public async Task Fix2_SuspenderCliente_Rejeita_Tenant_Nao_System()
        {
            using var ctx = CtxGestao("tenant-x", "u");
            var handler = new SuspenderClienteCommandHandler(ctx, new FakeUser("u"), new FakeTenant("tenant-x"));
            var result = await handler.Handle(new SuspenderClienteCommand(Guid.NewGuid()), CancellationToken.None);
            AssertBloqueadoLandlord(result);
        }

        [Fact]
        public async Task Fix2_CriarPlano_Rejeita_Tenant_Nao_System()
        {
            using var ctx = CtxGestao("tenant-x", "u");
            var handler = new CriarPlanoRicoCommandHandler(ctx, new FakeTenant("tenant-x"), new FakeUser("u"));
            var result = await handler.Handle(new CriarPlanoRicoCommand("Plano", 10m), CancellationToken.None);
            AssertBloqueadoLandlord(result);
        }

        [Fact]
        public async Task Fix2_AlterarFaturaLandlord_Rejeita_Tenant_Nao_System()
        {
            using var ctx = CtxGestao("tenant-x", "u");
            var handler = new AlterarFaturaCommandHandler(ctx, new FakeUser("u"), new FakeTenant("tenant-x"));
            var result = await handler.Handle(new AlterarFaturaCommand(Guid.NewGuid(), 10m, DateTime.UtcNow.AddDays(5)), CancellationToken.None);
            AssertBloqueadoLandlord(result);
        }

        [Fact]
        public async Task Fix2_CriarGateway_Rejeita_Tenant_Nao_System()
        {
            using var ctx = CtxGestao("tenant-x", "u");
            var handler = new CriarGatewayPagamentoCommandHandler(ctx, new FakeUser("u"), new TrivialCofre(), new FakeTenant("tenant-x"));
            var cmd = new CriarGatewayPagamentoCommand(default(EProvedorGateway), default(EAmbienteGateway), "token-secreto");
            var result = await handler.Handle(cmd, CancellationToken.None);
            AssertBloqueadoLandlord(result);
        }

        [Fact]
        public async Task Fix2_SuspenderCliente_Como_Interno_Passa_Do_Guard()
        {
            // Com tenant="system" o guard NÃO barra: o handler segue e responde "não encontrado".
            using var ctx = CtxGestao("system", "u");
            var handler = new SuspenderClienteCommandHandler(ctx, new FakeUser("u"), new FakeTenant("system"));
            var result = await handler.Handle(new SuspenderClienteCommand(Guid.NewGuid()), CancellationToken.None);

            Assert.False(result.Sucesso);
            Assert.DoesNotContain(result.Erros, e => e.Contains("operador interno"));
            Assert.Contains(result.Erros, e => e.Contains("não encontrado"));
        }

        // ===================== FIX #3: governança/atualização exige operador interno =====================

        [Fact]
        public async Task Fix3_GovernancaVersao_Solicitar_Rejeita_Nao_Interno()
        {
            using var ctx = CtxAplicativo("tenant-x", "u");
            var handler = new GovernancaVersaoHandlers(ctx, new FakeUser("u"), new ServiceCollection().BuildServiceProvider(), new FakeTenant("tenant-x"));
            var result = await handler.Handle(new SolicitarUpgradeVersaoCommand("v1", "v2", "motivo", true), CancellationToken.None);

            Assert.False(result.Sucesso);
            Assert.Contains(result.Erros, e => e.Contains("operador interno"));
        }

        [Fact]
        public async Task Fix3_ExecutarAtualizacao_Rejeita_Nao_Interno()
        {
            using var ctx = CtxAplicativo("tenant-x", "u");
            var handler = new ExecutarAtualizacaoCommandHandler(ctx, new ServiceCollection().BuildServiceProvider(), new FakeUser("u"), new FakeTenant("tenant-x"));
            var result = await handler.Handle(new ExecutarAtualizacaoCommand("v2026.01"), CancellationToken.None);

            Assert.False(result.Sucesso);
            Assert.Contains(result.Erros, e => e.Contains("operador interno"));
        }

        // ===================== FIX #4: segredo não vaza na listagem =====================

        [Fact]
        public async Task Fix4_Listagem_De_SystemSettings_Nao_Vaza_Segredo()
        {
            using var ctx = CtxAplicativo("system", "op");
            ctx.SystemSettings.Add(new SystemSetting("smtp.host", "mail.epros.com", "global", false, "system", "op"));
            ctx.SystemSettings.Add(new SystemSetting("smtp.password", "S3nh4-Sup3r-Secreta", "global", true, "system", "op"));
            await ctx.SaveChangesAsync();

            var handler = new ListarSystemSettingsQueryHandler(ctx, new FakeTenant("system"));
            var lista = (await handler.Handle(new ListarSystemSettingsQuery(), CancellationToken.None)).ToList();

            var segredo = lista.Single(s => s.Chave == "smtp.password");
            Assert.True(segredo.IsSecret);
            Assert.NotEqual("S3nh4-Sup3r-Secreta", segredo.Valor); // NUNCA o valor integral (REG-030/§19)

            var naoSegredo = lista.Single(s => s.Chave == "smtp.host");
            Assert.Equal("mail.epros.com", naoSegredo.Valor); // não-segredo inalterado
        }

        // ===================== test doubles =====================

        private static void AssertBloqueadoLandlord(CommandResult result)
        {
            Assert.False(result.Sucesso);
            Assert.Contains(result.Erros, e => e.Contains("operador interno"));
        }

        private sealed class FakeTenant : ITenantProvider
        {
            private readonly string _t;
            public FakeTenant(string t) => _t = t;
            public string GetTenantId() => _t;
        }

        private sealed class FakeUser : ICurrentUser
        {
            private readonly string _u;
            public FakeUser(string u) => _u = u;
            public string? GetUserId() => _u;
            public string? GetUserName() => "sec-111";
            public string? GetUserEmail() => "sec111@epros.com";
        }

        private sealed class TrivialCofre : ISegredoCofreService
        {
            public Task<string> CriptografarAsync(string valor) => Task.FromResult(valor);
            public Task<string> DescriptografarAsync(string ciphertext) => Task.FromResult(ciphertext);
        }
    }
}
