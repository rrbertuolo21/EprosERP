using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Qualidade.Application.Commands;
using Epros.Modules.Qualidade.Application.Commands.Ncr;
using Epros.Modules.Qualidade.Application.Handlers;
using Epros.Modules.Qualidade.Application.Handlers.Ncr;
using Epros.Modules.Qualidade.Domain.Enums;
using Epros.Modules.Qualidade.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Domain.Events;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Epros.Tests
{
    /// <summary>QLD-NCR: maquina de estados operavel (causa raiz -> CAPA -> verificacao -> encerramento) com guardas.</summary>
    public class QualidadeNcrFluxoTests
    {
        private static readonly ITenantProvider Tenant = new TP("tenant-1");
        private static readonly ICurrentUser User = new CU("user-1");

        private static ContextQualidade Novo(string db)
            => new(new DbContextOptionsBuilder<ContextQualidade>().UseInMemoryDatabase(db).Options, Tenant, User);

        private static async Task<Guid> CriarNcr(string db, string codigo)
        {
            using var ctx = Novo(db);
            var r = await new CriarNcrCommandHandler(ctx, Tenant, User).Handle(
                new CriarNcrCommand(codigo, "Lote fora de spec", "Trinca", ENcrOrigem.Inspecao, ENcrPrioridade.Alta,
                    Guid.NewGuid(), "Alta", DateTime.UtcNow), CancellationToken.None);
            Assert.True(r.Sucesso);
            return (await ctx.NcrRegistros.FirstAsync(n => n.Codigo == codigo)).Id;
        }

        [Fact]
        public async Task Capa_Exige_Causa_Raiz_Antes()
        {
            const string db = nameof(Capa_Exige_Causa_Raiz_Antes);
            var id = await CriarNcr(db, "NCR-F1");
            using var ctx = Novo(db);
            var r = await new AdicionarAcaoCapaNcrCommandHandler(ctx, Tenant, User).Handle(
                new AdicionarAcaoCapaNcrCommand(id, ENcrTipoAcao.Corretiva, "Trocar fornecedor", Guid.NewGuid(),
                    DateTime.UtcNow.AddDays(5), false), CancellationToken.None);
            Assert.False(r.Sucesso);
        }

        [Fact]
        public async Task Encerrar_Bloqueado_Com_Acao_Aberta_E_Sem_Verificacao()
        {
            const string db = nameof(Encerrar_Bloqueado_Com_Acao_Aberta_E_Sem_Verificacao);
            var id = await CriarNcr(db, "NCR-F2");
            Guid acaoId;

            using (var ctx = Novo(db))
                await new AdicionarCausaRaizNcrCommandHandler(ctx, Tenant, User).Handle(
                    new AdicionarCausaRaizNcrCommand(id, ENcrMetodoCausa.CincoPorques, "analise", "causa", "conclusao"),
                    CancellationToken.None);

            using (var ctx = Novo(db))
            {
                var r = await new AdicionarAcaoCapaNcrCommandHandler(ctx, Tenant, User).Handle(
                    new AdicionarAcaoCapaNcrCommand(id, ENcrTipoAcao.Corretiva, "Trocar fornecedor", Guid.NewGuid(),
                        DateTime.UtcNow.AddDays(5), true), CancellationToken.None);
                Assert.True(r.Sucesso);
                acaoId = (await ctx.NcrAcoesCapa.FirstAsync()).Id;
            }

            // Acao ainda aberta -> encerrar bloqueado.
            using (var ctx = Novo(db))
            {
                var r = await new EncerrarNcrCommandHandler(ctx, Tenant, User).Handle(
                    new EncerrarNcrCommand(id, "resolvido"), CancellationToken.None);
                Assert.False(r.Sucesso);
            }

            using (var ctx = Novo(db))
                await new ConcluirAcaoCapaNcrCommandHandler(ctx, User).Handle(
                    new ConcluirAcaoCapaNcrCommand(acaoId, "evidencia ok"), CancellationToken.None);

            // Acao concluida mas sem verificacao de eficacia -> ainda bloqueado.
            using (var ctx = Novo(db))
            {
                var r = await new EncerrarNcrCommandHandler(ctx, Tenant, User).Handle(
                    new EncerrarNcrCommand(id, "resolvido"), CancellationToken.None);
                Assert.False(r.Sucesso);
            }

            using (var ctx = Novo(db))
                await new RegistrarVerificacaoEficaciaNcrCommandHandler(ctx, Tenant, User).Handle(
                    new RegistrarVerificacaoEficaciaNcrCommand(id, acaoId, "criterio", ENcrResultadoVerificacao.Aprovada,
                        "eficaz", Guid.NewGuid(), null), CancellationToken.None);

            // Agora encerra e emite evento.
            using (var ctx = Novo(db))
            {
                var r = await new EncerrarNcrCommandHandler(ctx, Tenant, User).Handle(
                    new EncerrarNcrCommand(id, "resolvido"), CancellationToken.None);
                Assert.True(r.Sucesso);
            }

            using (var ctx = Novo(db))
            {
                Assert.Equal(EStatusRegistroQualidade.Encerrado, (await ctx.NcrRegistros.FirstAsync()).StatusRegistro);
                var eventos = await ctx.OutboxMessages.Select(o => o.EventType).ToListAsync();
                Assert.Contains(CatalogoEventosIntegracao.Qualidade.NcrEncerrada, eventos);
            }
        }

        [Fact]
        public async Task Verificacao_Reprovada_Reabre_Para_Capa()
        {
            const string db = nameof(Verificacao_Reprovada_Reabre_Para_Capa);
            var id = await CriarNcr(db, "NCR-F3");
            using (var ctx = Novo(db))
                await new RegistrarVerificacaoEficaciaNcrCommandHandler(ctx, Tenant, User).Handle(
                    new RegistrarVerificacaoEficaciaNcrCommand(id, null, "criterio", ENcrResultadoVerificacao.Reprovada,
                        "ineficaz", Guid.NewGuid(), "refazer CAPA"), CancellationToken.None);
            using (var ctx = Novo(db))
                Assert.Equal(ENcrEtapa.CAPA, (await ctx.NcrRegistros.FirstAsync()).EtapaNcr);
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
