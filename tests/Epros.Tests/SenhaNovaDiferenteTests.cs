using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Microsoft.EntityFrameworkCore;
using Epros.Shared.Application.Contracts;
using Epros.Modules.Aplicativo.Application.Commands;
using Epros.Modules.Aplicativo.Application.Handlers;
using Epros.Modules.Aplicativo.Domain.Entities;
using Epros.Modules.Aplicativo.Infrastructure.Data;

namespace Epros.Tests
{
    /// <summary>
    /// 1.09 — REG-046 / CT-008: a nova senha não pode ser igual à senha atual (troca administrativa).
    /// </summary>
    public class SenhaNovaDiferenteTests
    {
        private const string Tenant = "tenant-senha";

        private static ContextAplicativo CreateContext(string db)
        {
            var options = new DbContextOptionsBuilder<ContextAplicativo>()
                .UseInMemoryDatabase(db)
                .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
                .Options;
            return new ContextAplicativo(options, new FakeTenant(Tenant), new FakeUser("admin"));
        }

        [Fact]
        public async Task AlterarSenhaAdministrativa_Com_Nova_Igual_A_Atual_Deve_Falhar()
        {
            var db = Guid.NewGuid().ToString();
            var hasher = new Epros.Infrastructure.Services.Pbkdf2PasswordHasher();
            using var ctx = CreateContext(db);

            var usuario = new Usuario(Tenant, "Fulano", "fulano@epros.com", hasher.Hash("SenhaAtual@1"), UsuarioTipo.Company, "admin");
            ctx.Usuarios.Add(usuario);
            await ctx.SaveChangesAsync();

            var handler = new AlterarSenhaAdministrativaCommandHandler(ctx, new FakeUser("admin"), hasher);
            var result = await handler.Handle(new AlterarSenhaAdministrativaCommand(usuario.Id, "SenhaAtual@1"), CancellationToken.None);

            Assert.False(result.Sucesso);
        }

        [Fact]
        public async Task AlterarSenhaAdministrativa_Com_Nova_Diferente_Deve_Suceder()
        {
            var db = Guid.NewGuid().ToString();
            var hasher = new Epros.Infrastructure.Services.Pbkdf2PasswordHasher();
            using var ctx = CreateContext(db);

            var usuario = new Usuario(Tenant, "Fulano", "fulano2@epros.com", hasher.Hash("SenhaAtual@1"), UsuarioTipo.Company, "admin");
            ctx.Usuarios.Add(usuario);
            await ctx.SaveChangesAsync();

            var handler = new AlterarSenhaAdministrativaCommandHandler(ctx, new FakeUser("admin"), hasher);
            var result = await handler.Handle(new AlterarSenhaAdministrativaCommand(usuario.Id, "SenhaNova@2"), CancellationToken.None);

            Assert.True(result.Sucesso);
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
            public string? GetUserName() => "t";
            public string? GetUserEmail() => "t@epros.com";
        }
    }
}
