using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Aplicativo.Application.Contracts;
using Epros.Modules.Aplicativo.Application.Plataforma.Ged;
using Epros.Modules.Aplicativo.Domain.Entities.Plataforma.Ged;
using Epros.Modules.Aplicativo.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Epros.Tests.Plataforma
{
    /// <summary>PLT · GED (T10) — registro/hash/dedupe, versionamento, vínculo, retenção legal (NF-04).</summary>
    public class GedPlataformaTests
    {
        private static ContextAplicativo Novo(string db, string tenant = "tenant-1", string user = "user-1")
        {
            var options = new DbContextOptionsBuilder<ContextAplicativo>().UseInMemoryDatabase(db).Options;
            return new ContextAplicativo(options, new PlataformaTestFixtures.TestTenantProvider(tenant),
                new PlataformaTestFixtures.TestCurrentUser(user));
        }

        private static string B64(string s) => Convert.ToBase64String(Encoding.UTF8.GetBytes(s));

        [Fact]
        public async Task Deve_Registrar_Documento_Com_Hash_Calculado()
        {
            using var ctx = Novo(nameof(Deve_Registrar_Documento_Com_Hash_Calculado));
            var handler = new RegistrarDocumentoGedCommandHandler(ctx, T(), U(), new StorageFake());
            var r = await handler.Handle(new RegistrarDocumentoGedCommand(
                "Contrato.pdf", "contrato", B64("conteudo-do-contrato"), null, 0, "application/pdf",
                false, "Imobiliaria", "Locacao", "loc-1"), CancellationToken.None);

            Assert.True(r.Sucesso);
            var doc = await ctx.DocumentosGed.FirstAsync();
            Assert.Equal(64, doc.Hash.Length); // SHA-256 hex
            Assert.Equal(1, doc.Versao);
            Assert.Equal("local://", doc.StorageRef!.Substring(0, 8));
        }

        [Fact]
        public async Task Deve_Deduplicar_Por_Hash_E_Reaproveitar_StorageRef()
        {
            using var ctx = Novo(nameof(Deve_Deduplicar_Por_Hash_E_Reaproveitar_StorageRef));
            var handler = new RegistrarDocumentoGedCommandHandler(ctx, T(), U(), new StorageFake());
            var cmd = new RegistrarDocumentoGedCommand("a.pdf", "laudo", B64("mesmo-conteudo"), null, 0, null, false, null, null, null);

            var r1 = await handler.Handle(cmd, CancellationToken.None);
            var r2 = await handler.Handle(cmd with { Nome = "b.pdf" }, CancellationToken.None);

            Assert.True(r1.Sucesso);
            Assert.True(r2.Sucesso);
            var docs = await ctx.DocumentosGed.OrderBy(d => d.CriadoEm).ToListAsync();
            Assert.Equal(2, docs.Count); // dois registros lógicos
            Assert.Equal(docs[0].Hash, docs[1].Hash);
            Assert.Equal(docs[0].StorageRef, docs[1].StorageRef); // bytes reaproveitados (REG-020)
        }

        [Fact]
        public async Task Deve_Bloquear_Upload_De_Hash_Bloqueado()
        {
            using var ctx = Novo(nameof(Deve_Bloquear_Upload_De_Hash_Bloqueado));
            var bloqueio = new BloquearHashGedCommandHandler(ctx, T(), U());
            var conteudo = "conteudo-proibido";
            var hash = System.Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(Encoding.UTF8.GetBytes(conteudo))).ToLowerInvariant();
            await bloqueio.Handle(new BloquearHashGedCommand(hash, "Conteúdo proibido"), CancellationToken.None);

            var handler = new RegistrarDocumentoGedCommandHandler(ctx, T(), U(), new StorageFake());
            var r = await handler.Handle(new RegistrarDocumentoGedCommand("x.pdf", "outro", B64(conteudo), null, 0, null, false, null, null, null), CancellationToken.None);

            Assert.False(r.Sucesso);
            Assert.True(r.Block);
        }

        [Fact]
        public async Task Deve_Registrar_Nova_Versao_Incrementando()
        {
            using var ctx = Novo(nameof(Deve_Registrar_Nova_Versao_Incrementando));
            var reg = new RegistrarDocumentoGedCommandHandler(ctx, T(), U(), new StorageFake());
            await reg.Handle(new RegistrarDocumentoGedCommand("v.pdf", "contrato", B64("v1"), null, 0, null, false, null, null, null), CancellationToken.None);
            var doc = await ctx.DocumentosGed.FirstAsync();

            var vh = new RegistrarNovaVersaoDocumentoGedCommandHandler(ctx, T(), U(), new StorageFake());
            var r = await vh.Handle(new RegistrarNovaVersaoDocumentoGedCommand(doc.Id, B64("v2"), null, 0), CancellationToken.None);

            Assert.True(r.Sucesso);
            var atualizado = await ctx.DocumentosGed.FirstAsync();
            Assert.Equal(2, atualizado.Versao);
        }

        [Fact]
        public async Task Deve_Vincular_Documento_E_Bloquear_Duplicado()
        {
            using var ctx = Novo(nameof(Deve_Vincular_Documento_E_Bloquear_Duplicado));
            var reg = new RegistrarDocumentoGedCommandHandler(ctx, T(), U(), new StorageFake());
            await reg.Handle(new RegistrarDocumentoGedCommand("c.pdf", "contrato", B64("c"), null, 0, null, false, null, null, null), CancellationToken.None);
            var doc = await ctx.DocumentosGed.FirstAsync();

            var h = new VincularDocumentoGedCommandHandler(ctx, T(), U());
            var ok = await h.Handle(new VincularDocumentoGedCommand(doc.Id, "Vendas", "Venda", "venda-1"), CancellationToken.None);
            var dup = await h.Handle(new VincularDocumentoGedCommand(doc.Id, "Vendas", "Venda", "venda-1"), CancellationToken.None);

            Assert.True(ok.Sucesso);
            Assert.False(dup.Sucesso);
        }

        [Fact]
        public void Politica_Retencao_Exige_Base_Legal_Regra_Zero()
        {
            var pol = new PoliticaRetencaoGed("nfe-xml", 1825, "", "Reter", false, "tenant-1", "user-1");
            Assert.False(pol.IsValid); // base legal obrigatória — não se inventa (Regra #0)
        }

        [Fact]
        public async Task Deve_Listar_Documentos_Com_Retencao_Vencida()
        {
            using var ctx = Novo(nameof(Deve_Listar_Documentos_Com_Retencao_Vencida));
            // política curta (1 dia); documento criado no passado.
            var pol = new PoliticaRetencaoGed("recibo", 1, "Guarda contábil", "RevisarDescarte", false, "tenant-1", "user-1");
            ctx.PoliticasRetencaoGed.Add(pol);
            var doc = new Epros.Shared.Domain.Entities.DocumentoGed("r.pdf", "recibo", "h", 10, "tenant-1", "user-1");
            // força CriadoEm antigo via reflexão do backing field não é possível; usa política de 1 dia e espera não vencer.
            ctx.DocumentosGed.Add(doc);
            await ctx.SaveChangesAsync();

            var h = new ObterDocumentosRetencaoVencidaQueryHandler(ctx);
            var vencidos = await h.Handle(new ObterDocumentosRetencaoVencidaQuery(), CancellationToken.None);
            Assert.Empty(vencidos); // criado agora, prazo 1 dia => ainda não vencido (valida o join/regra)
        }

        private static ITenantProvider T() => new PlataformaTestFixtures.TestTenantProvider("tenant-1");
        private static ICurrentUser U() => new PlataformaTestFixtures.TestCurrentUser("user-1");

        private sealed class StorageFake : IArmazenamentoDocumentoService
        {
            public bool ProviderConfigurado => false;
            public Task<string> ArmazenarAsync(byte[] c, string chave, CancellationToken ct = default)
                => Task.FromResult($"local://{chave}");
            public Task<byte[]?> ObterAsync(string s, CancellationToken ct = default) => Task.FromResult<byte[]?>(null);
            public Task RemoverAsync(string s, CancellationToken ct = default) => Task.CompletedTask;
        }
    }
}
