using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.GRC.Application.Commands;
using Epros.Modules.GRC.Application.Handlers;
using Epros.Modules.GRC.Application.Queries;
using Epros.Modules.GRC.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Epros.Tests
{
    /// <summary>
    /// D-TEC-04 — parametrizacao por tenant nos 5 submodulos que faltavam
    /// (grc_pol/reg/ris/cia/sod_parametro). Upsert por (TenantId, Chave).
    /// </summary>
    public class GrcParametrosTests
    {
        private static ContextGRC NovoContexto(string db, string tenant = "tenant-1", string user = "user-1")
        {
            var options = new DbContextOptionsBuilder<ContextGRC>().UseInMemoryDatabase(db).Options;
            return new ContextGRC(options, new FakeTenant(tenant), new FakeUser(user));
        }

        [Fact]
        public async Task Deve_Definir_Parametro_SOD()
        {
            using var context = NovoContexto(nameof(Deve_Definir_Parametro_SOD));
            var handler = new DefinirParametroGrcCommandHandler(context, new FakeTenant("tenant-1"), new FakeUser("user-1"));

            var result = await handler.Handle(
                new DefinirParametroGrcCommand(SubmoduloGrc.SegregacaoFuncoes, "SOD_MODO_BLOQUEIO", "\"Bloqueia\""),
                CancellationToken.None);

            Assert.True(result.Sucesso);
            Assert.Single(await context.SegregacaoParametros.ToListAsync());
        }

        [Fact]
        public async Task Deve_Fazer_Upsert_Da_Mesma_Chave_Sem_Duplicar()
        {
            using var context = NovoContexto(nameof(Deve_Fazer_Upsert_Da_Mesma_Chave_Sem_Duplicar));
            var handler = new DefinirParametroGrcCommandHandler(context, new FakeTenant("tenant-1"), new FakeUser("user-1"));

            await handler.Handle(new DefinirParametroGrcCommand(SubmoduloGrc.Compliance, "REG_ALERTA", "[30,15,7]"), CancellationToken.None);
            var upsert = await handler.Handle(new DefinirParametroGrcCommand(SubmoduloGrc.Compliance, "REG_ALERTA", "[45,30,15,7]"), CancellationToken.None);

            Assert.True(upsert.Sucesso);
            var todos = await context.ComplianceParametros.ToListAsync();
            Assert.Single(todos);
            Assert.Equal("[45,30,15,7]", todos[0].ValorJson);
        }

        [Fact]
        public async Task Deve_Rejeitar_Submodulo_Invalido()
        {
            using var context = NovoContexto(nameof(Deve_Rejeitar_Submodulo_Invalido));
            var handler = new DefinirParametroGrcCommandHandler(context, new FakeTenant("tenant-1"), new FakeUser("user-1"));
            var result = await handler.Handle(new DefinirParametroGrcCommand("XPTO", "K", "V"), CancellationToken.None);
            Assert.False(result.Sucesso);
        }

        [Fact]
        public async Task Deve_Listar_Parametros_Por_Submodulo()
        {
            using var context = NovoContexto(nameof(Deve_Listar_Parametros_Por_Submodulo));
            var handler = new DefinirParametroGrcCommandHandler(context, new FakeTenant("tenant-1"), new FakeUser("user-1"));
            await handler.Handle(new DefinirParametroGrcCommand(SubmoduloGrc.Riscos, "RIS_ESCALA", "\"5x5\""), CancellationToken.None);
            await handler.Handle(new DefinirParametroGrcCommand(SubmoduloGrc.Riscos, "RIS_FORMULA", "\"PxI\""), CancellationToken.None);

            var query = new ObterParametrosGrcQueryHandler(context);
            var lista = await query.Handle(new ObterParametrosGrcQuery(SubmoduloGrc.Riscos), CancellationToken.None);

            Assert.Equal(2, lista.Count);
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
            public string? GetUserName() => "test";
            public string? GetUserEmail() => "test@epros.com.br";
        }
    }
}
