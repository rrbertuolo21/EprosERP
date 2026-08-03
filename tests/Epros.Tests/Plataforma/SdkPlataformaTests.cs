using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Aplicativo.Application.Plataforma.Sdk;
using Epros.Modules.Aplicativo.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Epros.Tests.Plataforma
{
    /// <summary>PLT · SDK/Extensões — chaves de API com escopos: geração, hash-only, validação, revogação.</summary>
    public class SdkPlataformaTests
    {
        private static ContextAplicativo Novo(string db)
        {
            var options = new DbContextOptionsBuilder<ContextAplicativo>().UseInMemoryDatabase(db).Options;
            return new ContextAplicativo(options, T(), U());
        }

        private static ITenantProvider T() => new PlataformaTestFixtures.TestTenantProvider("tenant-1");
        private static ICurrentUser U() => new PlataformaTestFixtures.TestCurrentUser("user-1");

        private static string ExtrairSegredo(Epros.Shared.Application.Models.CommandResult r)
            => (string)r.Dados!.GetType().GetProperty("Segredo")!.GetValue(r.Dados)!;

        [Fact]
        public async Task Gerar_Rejeita_Escopo_Invalido()
        {
            using var ctx = Novo(nameof(Gerar_Rejeita_Escopo_Invalido));
            var r = await new GerarChaveApiCommandHandler(ctx, T(), U()).Handle(
                new GerarChaveApiCommand("k", new List<string> { "escopo:inexistente" }, null), CancellationToken.None);
            Assert.False(r.Sucesso);
        }

        [Fact]
        public async Task Gerar_Nao_Persiste_Segredo_Em_Claro()
        {
            using var ctx = Novo(nameof(Gerar_Nao_Persiste_Segredo_Em_Claro));
            var r = await new GerarChaveApiCommandHandler(ctx, T(), U()).Handle(
                new GerarChaveApiCommand("k", new List<string> { "ged:ler" }, null), CancellationToken.None);
            Assert.True(r.Sucesso);
            var segredo = ExtrairSegredo(r);

            var chave = await ctx.ChavesApi.FirstAsync();
            Assert.NotEqual(segredo, chave.HashChave);       // guarda hash, não o segredo
            Assert.StartsWith("epk_", segredo);
            Assert.StartsWith("epk_", chave.Prefixo);
        }

        [Fact]
        public async Task Validar_Aceita_Segredo_Correto_Com_Escopo()
        {
            using var ctx = Novo(nameof(Validar_Aceita_Segredo_Correto_Com_Escopo));
            var r = await new GerarChaveApiCommandHandler(ctx, T(), U()).Handle(
                new GerarChaveApiCommand("k", new List<string> { "ged:ler", "analytics:ler" }, null), CancellationToken.None);
            var segredo = ExtrairSegredo(r);

            var res = await new ValidarChaveApiQueryHandler(ctx).Handle(
                new ValidarChaveApiQuery(segredo, "analytics:ler"), CancellationToken.None);
            Assert.True(res.Valida);

            var semEscopo = await new ValidarChaveApiQueryHandler(ctx).Handle(
                new ValidarChaveApiQuery(segredo, "iot:ingerir"), CancellationToken.None);
            Assert.False(semEscopo.Valida); // escopo não concedido
        }

        [Fact]
        public async Task Validar_Rejeita_Segredo_Errado()
        {
            using var ctx = Novo(nameof(Validar_Rejeita_Segredo_Errado));
            await new GerarChaveApiCommandHandler(ctx, T(), U()).Handle(
                new GerarChaveApiCommand("k", new List<string> { "ged:ler" }, null), CancellationToken.None);
            var res = await new ValidarChaveApiQueryHandler(ctx).Handle(
                new ValidarChaveApiQuery("epk_chave-falsa", "ged:ler"), CancellationToken.None);
            Assert.False(res.Valida);
        }

        [Fact]
        public async Task Revogar_Invalida_A_Chave()
        {
            using var ctx = Novo(nameof(Revogar_Invalida_A_Chave));
            var r = await new GerarChaveApiCommandHandler(ctx, T(), U()).Handle(
                new GerarChaveApiCommand("k", new List<string> { "ged:ler" }, null), CancellationToken.None);
            var segredo = ExtrairSegredo(r);
            var chaveId = await ctx.ChavesApi.Select(c => c.Id).FirstAsync();

            await new RevogarChaveApiCommandHandler(ctx, T(), U()).Handle(new RevogarChaveApiCommand(chaveId), CancellationToken.None);

            var res = await new ValidarChaveApiQueryHandler(ctx).Handle(
                new ValidarChaveApiQuery(segredo, "ged:ler"), CancellationToken.None);
            Assert.False(res.Valida);
        }

        [Fact]
        public async Task Validar_Rejeita_Chave_Expirada()
        {
            using var ctx = Novo(nameof(Validar_Rejeita_Chave_Expirada));
            var r = await new GerarChaveApiCommandHandler(ctx, T(), U()).Handle(
                new GerarChaveApiCommand("k", new List<string> { "ged:ler" }, DateTime.UtcNow.AddSeconds(-1)), CancellationToken.None);
            var segredo = ExtrairSegredo(r);
            var res = await new ValidarChaveApiQueryHandler(ctx).Handle(
                new ValidarChaveApiQuery(segredo, "ged:ler"), CancellationToken.None);
            Assert.False(res.Valida);
        }
    }
}
