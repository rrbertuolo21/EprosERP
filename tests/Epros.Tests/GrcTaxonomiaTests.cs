using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.GRC.Application.Commands;
using Epros.Modules.GRC.Application.Handlers;
using Epros.Modules.GRC.Domain.Entities;
using Epros.Modules.GRC.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Epros.Tests
{
    /// <summary>
    /// D-TEC-05 — taxonomia normativa única (Política↔Obrigação↔Controle↔Risco): catálogo
    /// compartilhado, classificação por FK opcional e arestas de rastreabilidade.
    /// </summary>
    public class GrcTaxonomiaTests
    {
        private static ContextGRC NovoContexto(string db)
        {
            var options = new DbContextOptionsBuilder<ContextGRC>().UseInMemoryDatabase(db).Options;
            return new ContextGRC(options, new FakeTenant("tenant-1"), new FakeUser("user-1"));
        }

        [Fact]
        public async Task Deve_Criar_No_E_Bloquear_Codigo_Duplicado()
        {
            using var context = NovoContexto(nameof(Deve_Criar_No_E_Bloquear_Codigo_Duplicado));
            var handler = new CriarNoTaxonomiaCommandHandler(context, new FakeTenant("tenant-1"), new FakeUser("user-1"));
            var cmd = new CriarNoTaxonomiaCommand("OBR-LGPD-48", "Obrigacao", "Comunicacao de incidente", null);

            var ok = await handler.Handle(cmd, CancellationToken.None);
            var dup = await handler.Handle(cmd, CancellationToken.None);

            Assert.True(ok.Sucesso);
            Assert.False(dup.Sucesso);
        }

        [Fact]
        public async Task Deve_Rejeitar_Tipo_Invalido()
        {
            using var context = NovoContexto(nameof(Deve_Rejeitar_Tipo_Invalido));
            var handler = new CriarNoTaxonomiaCommandHandler(context, new FakeTenant("tenant-1"), new FakeUser("user-1"));
            var result = await handler.Handle(new CriarNoTaxonomiaCommand("X-1", "Coisa", "Nome", null), CancellationToken.None);
            Assert.False(result.Sucesso);
        }

        [Fact]
        public async Task Deve_Classificar_Controle_Na_Taxonomia()
        {
            using var context = NovoContexto(nameof(Deve_Classificar_Controle_Na_Taxonomia));
            var criarNo = new CriarNoTaxonomiaCommandHandler(context, new FakeTenant("tenant-1"), new FakeUser("user-1"));
            var no = await criarNo.Handle(new CriarNoTaxonomiaCommand("CTRL-1", "Controle", "Aprovacao dupla", null), CancellationToken.None);
            var noId = await context.TaxonomiasNormativas.Select(t => t.Id).FirstAsync();

            var controle = new ControleInterno("CI-1", "Controle X", "Desc", "Mensal", "tenant-1", "user-1");
            context.ControlesInternos.Add(controle);
            await context.SaveChangesAsync();

            var handler = new ClassificarAgregadoTaxonomiaCommandHandler(context, new FakeUser("user-1"));
            var result = await handler.Handle(new ClassificarAgregadoTaxonomiaCommand("Controle", controle.Id, noId), CancellationToken.None);

            Assert.True(result.Sucesso);
            Assert.Equal(noId, (await context.ControlesInternos.FirstAsync()).TaxonomiaNormativaId);
        }

        [Fact]
        public async Task Deve_Criar_Aresta_De_Rastreabilidade_Controle_Mitiga_Risco()
        {
            using var context = NovoContexto(nameof(Deve_Criar_Aresta_De_Rastreabilidade_Controle_Mitiga_Risco));
            var handler = new VincularTaxonomiaCommandHandler(context, new FakeTenant("tenant-1"), new FakeUser("user-1"));
            var controleId = Guid.NewGuid();
            var riscoId = Guid.NewGuid();

            var ok = await handler.Handle(new VincularTaxonomiaCommand("Controle", controleId, "Risco", riscoId, "mitiga"), CancellationToken.None);
            var dup = await handler.Handle(new VincularTaxonomiaCommand("Controle", controleId, "Risco", riscoId, "mitiga"), CancellationToken.None);

            Assert.True(ok.Sucesso);
            Assert.False(dup.Sucesso); // vínculo idempotente
            Assert.Single(await context.TaxonomiaVinculos.ToListAsync());
        }

        [Fact]
        public void Vinculo_Nao_Pode_Apontar_Para_Si_Mesmo()
        {
            var id = Guid.NewGuid();
            var vinculo = new TaxonomiaVinculo("Risco", id, "Risco", id, "cobre", "tenant-1", "user-1");
            Assert.False(vinculo.IsValid);
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
