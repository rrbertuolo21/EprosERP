using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Aplicativo.Application.Plataforma.Assinatura;
using Epros.Modules.Aplicativo.Infrastructure.Data;
using Epros.Modules.Aplicativo.Infrastructure.Services;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Domain.Entities;
using Epros.Shared.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Epros.Tests.Plataforma
{
    /// <summary>PLT · Assinatura eletrônica ICP — fluxo assinar/validar sem forjar validade jurídica.</summary>
    public class AssinaturaEletronicaPlataformaTests
    {
        private static ContextAplicativo Novo(string db)
        {
            var options = new DbContextOptionsBuilder<ContextAplicativo>().UseInMemoryDatabase(db).Options;
            return new ContextAplicativo(options, T(), U());
        }

        private static ITenantProvider T() => new PlataformaTestFixtures.TestTenantProvider("tenant-1");
        private static ICurrentUser U() => new PlataformaTestFixtures.TestCurrentUser("user-1");

        private static async Task<Guid> CriarDocumento(ContextAplicativo ctx)
        {
            var doc = new DocumentoGed("Contrato.pdf", "contrato", "hash1", 100, "tenant-1", "user-1", requerAssinatura: true);
            ctx.DocumentosGed.Add(doc);
            await ctx.SaveChangesAsync();
            return doc.Id;
        }

        [Fact]
        public async Task Deve_Criar_Solicitacao_E_Marcar_Documento_Pendente()
        {
            using var ctx = Novo(nameof(Deve_Criar_Solicitacao_E_Marcar_Documento_Pendente));
            var docId = await CriarDocumento(ctx);
            var h = new SolicitarAssinaturasCommandHandler(ctx, T(), U());
            var r = await h.Handle(new SolicitarAssinaturasCommand(docId, "contrato", null, false,
                new List<SignatarioInput> { new("Ana", "ana@x.com", false, true, 1) }), CancellationToken.None);

            Assert.True(r.Sucesso);
            Assert.Single(await ctx.SolicitacoesAssinatura.ToListAsync());
            var doc = await ctx.DocumentosGed.FirstAsync();
            Assert.Equal(EStatusAssinaturaDocumento.PendenteAssinatura, doc.StatusAssinatura);
        }

        [Fact]
        public async Task Sem_Provedor_ICP_Nao_Assina_E_Documento_Continua_Pendente()
        {
            using var ctx = Novo(nameof(Sem_Provedor_ICP_Nao_Assina_E_Documento_Continua_Pendente));
            var docId = await CriarDocumento(ctx);
            await new SolicitarAssinaturasCommandHandler(ctx, T(), U()).Handle(
                new SolicitarAssinaturasCommand(docId, null, null, false,
                    new List<SignatarioInput> { new("Ana", "ana@x.com", false, true, 1) }), CancellationToken.None);
            var sol = await ctx.SolicitacoesAssinatura.FirstAsync();
            var sig = await ctx.SignatariosAssinatura.FirstAsync();

            // provider PADRÃO = pendente (dependência externa)
            var h = new RegistrarAssinaturaCommandHandler(ctx, T(), U(), new AssinaturaDigitalPendenteService());
            var r = await h.Handle(new RegistrarAssinaturaCommand(sol.Id, sig.Id), CancellationToken.None);

            Assert.True(r.Sucesso); // aguardando provedor
            Assert.Empty(await ctx.RegistrosAssinatura.ToListAsync()); // nada assinado
            var doc = await ctx.DocumentosGed.FirstAsync();
            Assert.Equal(EStatusAssinaturaDocumento.PendenteAssinatura, doc.StatusAssinatura);
        }

        [Fact]
        public async Task Com_Provedor_Todos_Obrigatorios_Concluem_E_Documento_Fica_Assinado()
        {
            using var ctx = Novo(nameof(Com_Provedor_Todos_Obrigatorios_Concluem_E_Documento_Fica_Assinado));
            var docId = await CriarDocumento(ctx);
            await new SolicitarAssinaturasCommandHandler(ctx, T(), U()).Handle(
                new SolicitarAssinaturasCommand(docId, null, null, false, new List<SignatarioInput>
                {
                    new("Ana", "ana@x.com", false, true, 1),
                    new("Bob", "bob@x.com", false, true, 2)
                }), CancellationToken.None);
            var sol = await ctx.SolicitacoesAssinatura.FirstAsync();
            var sigs = await ctx.SignatariosAssinatura.OrderBy(s => s.Ordem).ToListAsync();

            var h = new RegistrarAssinaturaCommandHandler(ctx, T(), U(), new ProvedorOkFake());
            await h.Handle(new RegistrarAssinaturaCommand(sol.Id, sigs[0].Id), CancellationToken.None);
            var r2 = await h.Handle(new RegistrarAssinaturaCommand(sol.Id, sigs[1].Id), CancellationToken.None);

            Assert.True(r2.Sucesso);
            var solFinal = await ctx.SolicitacoesAssinatura.FirstAsync();
            Assert.Equal("Concluida", solFinal.Estado);
            var doc = await ctx.DocumentosGed.FirstAsync();
            Assert.Equal(EStatusAssinaturaDocumento.Assinado, doc.StatusAssinatura);
            Assert.Equal(2, await ctx.EvidenciasAssinatura.CountAsync());
        }

        [Fact]
        public async Task Ordem_Sequencial_Bloqueia_Fora_De_Ordem()
        {
            using var ctx = Novo(nameof(Ordem_Sequencial_Bloqueia_Fora_De_Ordem));
            var docId = await CriarDocumento(ctx);
            await new SolicitarAssinaturasCommandHandler(ctx, T(), U()).Handle(
                new SolicitarAssinaturasCommand(docId, null, null, true, new List<SignatarioInput>
                {
                    new("Ana", "ana@x.com", false, true, 1),
                    new("Bob", "bob@x.com", false, true, 2)
                }), CancellationToken.None);
            var sol = await ctx.SolicitacoesAssinatura.FirstAsync();
            var sigs = await ctx.SignatariosAssinatura.OrderBy(s => s.Ordem).ToListAsync();

            var h = new RegistrarAssinaturaCommandHandler(ctx, T(), U(), new ProvedorOkFake());
            var r = await h.Handle(new RegistrarAssinaturaCommand(sol.Id, sigs[1].Id), CancellationToken.None); // ordem 2 antes da 1
            Assert.False(r.Sucesso);
        }

        [Fact]
        public async Task Link_Publico_Revogado_Nao_Assina()
        {
            using var ctx = Novo(nameof(Link_Publico_Revogado_Nao_Assina));
            var docId = await CriarDocumento(ctx);
            await new SolicitarAssinaturasCommandHandler(ctx, T(), U()).Handle(
                new SolicitarAssinaturasCommand(docId, null, null, false,
                    new List<SignatarioInput> { new("Externo", "ext@x.com", true, true, 1) }), CancellationToken.None);
            var sig = await ctx.SignatariosAssinatura.FirstAsync();
            Assert.NotNull(sig.LinkToken);

            await new RevogarLinkPublicoCommandHandler(ctx, T(), U()).Handle(
                new RevogarLinkPublicoCommand(sig.Id), CancellationToken.None);

            var h = new AssinarPorLinkPublicoCommandHandler(ctx, T(), U(), new ProvedorOkFake());
            var r = await h.Handle(new AssinarPorLinkPublicoCommand(sig.LinkToken!), CancellationToken.None);
            Assert.False(r.Sucesso);
        }

        [Fact]
        public async Task Recusar_Marca_Documento_Recusado()
        {
            using var ctx = Novo(nameof(Recusar_Marca_Documento_Recusado));
            var docId = await CriarDocumento(ctx);
            await new SolicitarAssinaturasCommandHandler(ctx, T(), U()).Handle(
                new SolicitarAssinaturasCommand(docId, null, null, false,
                    new List<SignatarioInput> { new("Ana", "ana@x.com", false, true, 1) }), CancellationToken.None);
            var sol = await ctx.SolicitacoesAssinatura.FirstAsync();
            var sig = await ctx.SignatariosAssinatura.FirstAsync();

            var r = await new RecusarAssinaturaCommandHandler(ctx, T(), U()).Handle(
                new RecusarAssinaturaCommand(sol.Id, sig.Id, "Discordo dos termos"), CancellationToken.None);

            Assert.True(r.Sucesso);
            var doc = await ctx.DocumentosGed.FirstAsync();
            Assert.Equal(EStatusAssinaturaDocumento.Recusado, doc.StatusAssinatura);
        }

        private sealed class ProvedorOkFake : IAssinaturaDigitalService
        {
            public Task<ResultadoAssinatura> SolicitarAssinaturaAsync(Guid documentoId, CancellationToken ct = default)
                => Task.FromResult(ResultadoAssinatura.Ok($"evidencia-{documentoId:N}"));
        }
    }
}
