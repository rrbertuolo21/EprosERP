using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Qualidade.Application.Commands;
using Epros.Modules.Qualidade.Application.Commands.Acr;
using Epros.Modules.Qualidade.Application.Handlers;
using Epros.Modules.Qualidade.Application.Handlers.Acr;
using Epros.Modules.Qualidade.Domain.Enums;
using Epros.Modules.Qualidade.Domain.Services.Aql;
using Epros.Modules.Qualidade.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Domain.Events;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Epros.Tests
{
    /// <summary>
    /// Testes do wiring QLD-ACR: decisao de disposicao, avaliacao por amostragem AQL e emissao de
    /// eventos Outbox (bloquear/liberar/quarentena, sugestao de NCR, intencao de devolucao).
    /// </summary>
    public class QualidadeAcrTests
    {
        private static readonly ITenantProvider Tenant = new TP("tenant-1");
        private static readonly ICurrentUser User = new CU("user-1");

        private static ContextQualidade Novo(string db)
            => new(new DbContextOptionsBuilder<ContextQualidade>().UseInMemoryDatabase(db).Options, Tenant, User);

        private static async Task<Guid> CriarAnalise(string db, string codigo)
        {
            using var ctx = Novo(db);
            var r = await new CriarAnaliseAcrCommandHandler(ctx, Tenant, User).Handle(
                new CriarAnaliseAcrCommand(codigo, "Recebimento fornecedor", ETipoAnaliseAcr.Recebimento,
                    Guid.NewGuid(), null, null), CancellationToken.None);
            Assert.True(r.Sucesso);
            return (await ctx.AcrAnalises.FirstAsync(a => a.Codigo == codigo)).Id;
        }

        [Fact]
        public async Task Rejeitar_Sem_Motivo_Falha()
        {
            const string db = nameof(Rejeitar_Sem_Motivo_Falha);
            var id = await CriarAnalise(db, "ACR-R1");
            using var ctx = Novo(db);
            var r = await new DecidirAnaliseAcrCommandHandler(ctx, Tenant, User).Handle(
                new DecidirAnaliseAcrCommand(id, EResultadoAcr.Rejeitado, null, "Alta", 10m, "trinca", null, false, "L1"),
                CancellationToken.None);
            Assert.False(r.Sucesso);
        }

        [Fact]
        public async Task Rejeitar_Com_Motivo_Emite_Bloqueio_Ncr_E_Devolucao()
        {
            const string db = nameof(Rejeitar_Com_Motivo_Emite_Bloqueio_Ncr_E_Devolucao);
            var id = await CriarAnalise(db, "ACR-R2");
            using (var ctx = Novo(db))
            {
                var r = await new DecidirAnaliseAcrCommandHandler(ctx, Tenant, User).Handle(
                    new DecidirAnaliseAcrCommand(id, EResultadoAcr.Rejeitado, Guid.NewGuid(), "Alta", 10m, "trinca", null, false, "L1"),
                    CancellationToken.None);
                Assert.True(r.Sucesso);
            }
            using (var ctx = Novo(db))
            {
                var eventos = await ctx.OutboxMessages.Select(o => o.EventType).ToListAsync();
                Assert.Contains(CatalogoEventosIntegracao.Qualidade.AcrLoteBloqueado, eventos);
                Assert.Contains(CatalogoEventosIntegracao.Qualidade.AcrNcrSolicitada, eventos);
                Assert.Contains(CatalogoEventosIntegracao.Qualidade.AcrDevolucaoSolicitada, eventos);
                Assert.Equal(1, await ctx.AcrEventosEstoque.CountAsync());
                Assert.Equal(1, await ctx.AcrEventosNcr.CountAsync());
                Assert.Equal(EStatusRegistroQualidade.Encerrado, (await ctx.AcrAnalises.FirstAsync()).Status);
            }
        }

        [Fact]
        public async Task Aceitar_Emite_Liberacao_Sem_Ncr()
        {
            const string db = nameof(Aceitar_Emite_Liberacao_Sem_Ncr);
            var id = await CriarAnalise(db, "ACR-A1");
            using (var ctx = Novo(db))
                await new DecidirAnaliseAcrCommandHandler(ctx, Tenant, User).Handle(
                    new DecidirAnaliseAcrCommand(id, EResultadoAcr.Aceito, null, null, 100m, null, null, false, "L2"),
                    CancellationToken.None);
            using (var ctx = Novo(db))
            {
                var eventos = await ctx.OutboxMessages.Select(o => o.EventType).ToListAsync();
                Assert.Contains(CatalogoEventosIntegracao.Qualidade.AcrLoteLiberado, eventos);
                Assert.DoesNotContain(CatalogoEventosIntegracao.Qualidade.AcrNcrSolicitada, eventos);
                Assert.Equal(0, await ctx.AcrEventosNcr.CountAsync());
            }
        }

        [Fact]
        public async Task Avaliar_Amostragem_Rejeita_Quando_Defeituosos_Acima_De_Ac()
        {
            const string db = nameof(Avaliar_Amostragem_Rejeita_Quando_Defeituosos_Acima_De_Ac);
            var id = await CriarAnalise(db, "ACR-AM1");
            using var ctx = Novo(db);
            // L @ 1.0 -> Ac=5, Re=6. 6 defeituosos -> rejeita.
            var r = await new AvaliarAcrPorAmostragemCommandHandler(ctx, Tenant, User, new MotorAql()).Handle(
                new AvaliarAcrPorAmostragemCommand(id, 5000, ENivelInspecao.II, 1.0m, ESeveridadeAql.Normal,
                    6, Guid.NewGuid(), "Alta", null, "L3"), CancellationToken.None);
            Assert.True(r.Sucesso);
            var res = await ctx.AcrResultados.FirstAsync();
            Assert.Equal(EResultadoAcr.Rejeitado, res.ResultadoInspecao);
        }

        [Fact]
        public async Task Avaliar_Amostragem_Aceita_Quando_Defeituosos_Ate_Ac()
        {
            const string db = nameof(Avaliar_Amostragem_Aceita_Quando_Defeituosos_Ate_Ac);
            var id = await CriarAnalise(db, "ACR-AM2");
            using var ctx = Novo(db);
            var r = await new AvaliarAcrPorAmostragemCommandHandler(ctx, Tenant, User, new MotorAql()).Handle(
                new AvaliarAcrPorAmostragemCommand(id, 5000, ENivelInspecao.II, 1.0m, ESeveridadeAql.Normal,
                    5, null, null, null, "L4"), CancellationToken.None);
            Assert.True(r.Sucesso);
            var res = await ctx.AcrResultados.FirstAsync();
            Assert.Equal(EResultadoAcr.Aceito, res.ResultadoInspecao);
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
