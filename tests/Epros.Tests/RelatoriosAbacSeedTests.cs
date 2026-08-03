using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Epros.API.Seed;
using Epros.Modules.GestaoClientes.Infrastructure.Data;
using Epros.Shared.Application.Contracts;

namespace Epros.Tests
{
    // RPT (auditoria RELATORIOS, P0): os endpoints RelatoriosOperacionais/RelatoriosBi subiam 100% negados
    // (403) por falta de seed da capacidade ABAC. Estes testes travam o fechamento: as capacidades
    // "relatoriosoperacionais:ler" e "relatoriosbi:ler" são descobertas por reflexão nos controllers e
    // materializadas no catálogo do sistema, ligadas ao papel Administrador (que o admin do tenant recebe).
    public class RelatoriosAbacSeedTests
    {
        private static Assembly ApiAssembly => typeof(CapacidadeCatalogoSeeder).Assembly;

        private static ContextGestaoClientes NovoContexto(string db)
        {
            var options = new DbContextOptionsBuilder<ContextGestaoClientes>()
                .UseInMemoryDatabase(db)
                .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
                .Options;
            return new ContextGestaoClientes(options, new FakeTenantSys(), new FakeUserSys());
        }

        [Fact(DisplayName = "RPT | Descoberta por reflexão inclui os recursos de Relatórios")]
        public void Descoberta_Inclui_Relatorios()
        {
            var pares = CapacidadeCatalogoSeeder.DescobrirCapacidades(ApiAssembly);
            Assert.Contains(pares, p => p.Recurso == "RelatoriosOperacionais" && p.Acao == "Ler");
            Assert.Contains(pares, p => p.Recurso == "RelatoriosBi" && p.Acao == "Ler");
        }

        [Fact(DisplayName = "RPT | Seed materializa as capacidades de Relatórios e liga ao Administrador")]
        public async Task Seed_Materializa_Capacidades_De_Relatorios()
        {
            var ctx = NovoContexto(nameof(Seed_Materializa_Capacidades_De_Relatorios));

            var adminId = await CapacidadeCatalogoSeeder.SeedAsync(ctx, ApiAssembly);
            Assert.NotEqual(Guid.Empty, adminId);

            var capOper = CapacidadeCatalogoSeeder.NomeCapacidade("RelatoriosOperacionais", "Ler");
            var capBi = CapacidadeCatalogoSeeder.NomeCapacidade("RelatoriosBi", "Ler");

            var capsSistema = await ctx.Capacidades.IgnoreQueryFilters()
                .Where(c => c.TenantId == CapacidadeCatalogoSeeder.TenantSistema)
                .Select(c => c.Name).ToListAsync();
            Assert.Contains(capOper, capsSistema);
            Assert.Contains(capBi, capsSistema);

            // Administrador ligado às duas capacidades de Relatórios.
            var idsRelatorios = await ctx.Capacidades.IgnoreQueryFilters()
                .Where(c => c.Name == capOper || c.Name == capBi)
                .Select(c => c.Id).ToListAsync();
            var ligadas = await ctx.PapeisCapacidades.IgnoreQueryFilters()
                .Where(pc => pc.PapelId == adminId && idsRelatorios.Contains(pc.CapacidadeId))
                .CountAsync();
            Assert.Equal(2, ligadas);

            // Idempotência: rodar de novo não duplica as capacidades.
            await CapacidadeCatalogoSeeder.SeedAsync(ctx, ApiAssembly);
            var totalOper = await ctx.Capacidades.IgnoreQueryFilters().CountAsync(c => c.Name == capOper);
            Assert.Equal(1, totalOper);
        }

        private sealed class FakeTenantSys : ITenantProvider
        {
            public string GetTenantId() => CapacidadeCatalogoSeeder.TenantSistema;
        }

        private sealed class FakeUserSys : ICurrentUser
        {
            public string? GetUserId() => "seed-test";
            public string? GetUserName() => "seed-test";
            public string? GetUserEmail() => "seed@test.local";
        }
    }
}
