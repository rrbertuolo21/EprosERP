using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Qualidade.Application.Commands.Rst;
using Epros.Modules.Qualidade.Application.Handlers.Rst;
using Epros.Modules.Qualidade.Application.Queries.Rst;
using Epros.Modules.Qualidade.Domain.Enums;
using Epros.Modules.Qualidade.Domain.Services.Rst;
using Epros.Modules.Qualidade.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Domain.Events;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Epros.Tests
{
    /// <summary>QLD-RST: campanha de recall (fluxo/estados), contencao via Outbox e motor de genealogia.</summary>
    public class QualidadeRstTests
    {
        private static readonly ITenantProvider Tenant = new TP("tenant-1");
        private static readonly ICurrentUser User = new CU("user-1");

        private static ContextQualidade Novo(string db)
            => new(new DbContextOptionsBuilder<ContextQualidade>().UseInMemoryDatabase(db).Options, Tenant, User);

        private static async Task<Guid> CriarCampanha(string db, string codigo)
        {
            using var ctx = Novo(db);
            var r = await new CriarCampanhaRecallCommandHandler(ctx, Tenant, User).Handle(
                new CriarCampanhaRecallCommand(codigo, "Recall lote 42", ERstGravidade.Alta, Guid.NewGuid(),
                    "trinca em campo", Guid.NewGuid(), null), CancellationToken.None);
            Assert.True(r.Sucesso);
            return (await ctx.RstCampanhas.FirstAsync(c => c.Codigo == codigo)).Id;
        }

        // ================= Motor de genealogia =================
        [Fact]
        public void MotorGenealogia_Monta_Arvore_Com_Niveis()
        {
            var motor = new MotorGenealogia();
            var mp = Guid.NewGuid();
            var wip = Guid.NewGuid();
            var pa = Guid.NewGuid();
            var arvore = motor.MontarArvore(new[]
            {
                new NoGenealogia(mp, null, "MP", false),
                new NoGenealogia(wip, mp, "WIP", false),
                new NoGenealogia(pa, wip, "PA", false)
            });
            Assert.Single(arvore.Raizes);
            Assert.Equal(0, arvore.Raizes[0].Nivel);
            Assert.Equal(1, arvore.Raizes[0].Filhos[0].Nivel);
            Assert.Equal(2, arvore.Raizes[0].Filhos[0].Filhos[0].Nivel);
            Assert.False(arvore.TemLacuna);
        }

        [Fact]
        public void MotorGenealogia_Sinaliza_Lacuna()
        {
            var motor = new MotorGenealogia();
            var arvore = motor.MontarArvore(new[] { new NoGenealogia(Guid.NewGuid(), null, "MP", true) });
            Assert.True(arvore.TemLacuna);
        }

        // ================= Fluxo da campanha =================
        [Fact]
        public async Task Criar_Emite_RecallAberto()
        {
            const string db = nameof(Criar_Emite_RecallAberto);
            await CriarCampanha(db, "RST-1");
            using var ctx = Novo(db);
            var eventos = await ctx.OutboxMessages.Select(o => o.EventType).ToListAsync();
            Assert.Contains(CatalogoEventosIntegracao.Qualidade.RstRecallAberto, eventos);
        }

        [Fact]
        public async Task Bloqueio_De_Contencao_Emite_Evento_Ao_Estoque()
        {
            const string db = nameof(Bloqueio_De_Contencao_Emite_Evento_Ao_Estoque);
            var id = await CriarCampanha(db, "RST-2");
            using (var ctx = Novo(db))
            {
                var r = await new SolicitarBloqueioRecallCommandHandler(ctx, Tenant, User).Handle(
                    new SolicitarBloqueioRecallCommand(id, 100m, "L42", null, "contencao"), CancellationToken.None);
                Assert.True(r.Sucesso);
            }
            using (var ctx = Novo(db))
            {
                var eventos = await ctx.OutboxMessages.Select(o => o.EventType).ToListAsync();
                Assert.Contains(CatalogoEventosIntegracao.Qualidade.RstBloqueioSolicitado, eventos);
                Assert.Equal(1, await ctx.RstBloqueios.CountAsync(b => b.Ativo));
            }
        }

        [Fact]
        public async Task Encerrar_Muda_Status_E_Emite_Evento()
        {
            const string db = nameof(Encerrar_Muda_Status_E_Emite_Evento);
            var id = await CriarCampanha(db, "RST-3");
            using (var ctx = Novo(db))
            {
                var r = await new EncerrarRecallCommandHandler(ctx, Tenant, User).Handle(
                    new EncerrarRecallCommand(id, "recolhido e disposto"), CancellationToken.None);
                Assert.True(r.Sucesso);
            }
            using (var ctx = Novo(db))
            {
                Assert.Equal(EStatusRegistroQualidade.Encerrado, (await ctx.RstCampanhas.FirstAsync()).Status);
                var eventos = await ctx.OutboxMessages.Select(o => o.EventType).ToListAsync();
                Assert.Contains(CatalogoEventosIntegracao.Qualidade.RstRecallEncerrado, eventos);
            }
        }

        [Fact]
        public async Task Genealogia_Com_Lacuna_Sem_Justificativa_Falha()
        {
            const string db = nameof(Genealogia_Com_Lacuna_Sem_Justificativa_Falha);
            var id = await CriarCampanha(db, "RST-4");
            using var ctx = Novo(db);
            var r = await new RegistrarGenealogiaNoCommandHandler(ctx, Tenant, User).Handle(
                new RegistrarGenealogiaNoCommand(id, ERstTipoNoGenealogia.MateriaPrima, 0, null, null, "L1", null, true, null),
                CancellationToken.None);
            Assert.False(r.Sucesso);
        }

        [Fact]
        public async Task Genealogia_Query_Monta_Arvore()
        {
            const string db = nameof(Genealogia_Query_Monta_Arvore);
            var id = await CriarCampanha(db, "RST-5");
            using (var ctx = Novo(db))
            {
                await new RegistrarGenealogiaNoCommandHandler(ctx, Tenant, User).Handle(
                    new RegistrarGenealogiaNoCommand(id, ERstTipoNoGenealogia.MateriaPrima, 0, null, null, "MP-1", null, false, null),
                    CancellationToken.None);
            }
            using (var ctx = Novo(db))
            {
                var r = await new ObterGenealogiaRecallQueryHandler(ctx, new MotorGenealogia()).Handle(
                    new ObterGenealogiaRecallQuery(id), CancellationToken.None);
                Assert.True(r.Sucesso);
            }
        }

        private sealed class TP : ITenantProvider
        {
            private readonly string _t; public TP(string t) => _t = t;
            public string GetTenantId() => _t;
        }
        private sealed class CU : ICurrentUser
        {
            private readonly string _u; public CU(string u) => _u = u;
            public string? GetUserId() => _u;
            public string? GetUserName() => "test_user";
            public string? GetUserEmail() => "test@epros.com.br";
        }
    }
}
