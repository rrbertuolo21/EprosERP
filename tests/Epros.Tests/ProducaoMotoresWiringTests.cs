using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Producao.Application.Commands;
using Epros.Modules.Producao.Application.Handlers;
using Epros.Modules.Producao.Domain.Entities;
using Epros.Modules.Producao.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Epros.Tests
{
    /// <summary>
    /// Fiação (T2): os motores de Produção antes órfãos agora têm handler/rota. Estes testes
    /// exercitam os handlers que disparam BomExplosao, CustoRollup, Capacidade e Sequenciamento.
    /// </summary>
    public class ProducaoMotoresWiringTests
    {
        private const string TenantId = "tenant-motores";
        private const string UserId = "user-motores";

        private static ContextProducao NovoContexto(string dbName)
        {
            var options = new DbContextOptionsBuilder<ContextProducao>().UseInMemoryDatabase(dbName).Options;
            return new ContextProducao(options, new TP(TenantId), new CU(UserId));
        }

        private static BomEstrutura EstruturaAtiva(Guid produtoId, decimal rendimento, (Guid comp, decimal qtd)[] componentes)
        {
            var e = new BomEstrutura(produtoId, Guid.NewGuid(), null, 0m, 0m, 0m, rendimento, TenantId, UserId);
            foreach (var c in componentes)
                e.AdicionarComponente(c.comp, c.qtd, UserId);
            e.SubmeterParaAnalise(UserId);
            e.Aprovar(UserId);
            return e;
        }

        [Fact]
        public async Task ExplodirBom_Handler_Retorna_Itens()
        {
            using var ctx = NovoContexto(nameof(ExplodirBom_Handler_Retorna_Itens));
            var produto = Guid.NewGuid();
            var comp = Guid.NewGuid();
            var estrutura = EstruturaAtiva(produto, 1m, new[] { (comp, 2m) });
            ctx.BomEstruturas.Add(estrutura);
            await ctx.SaveChangesAsync();

            var handler = new ExplodirBomCommandHandler(ctx);
            var r = await handler.Handle(new ExplodirBomCommand(estrutura.Id, 100m), CancellationToken.None);

            Assert.True(r.Sucesso);
            Assert.NotNull(r.Dados);
        }

        [Fact]
        public async Task ExplodirBom_Handler_Estrutura_Inexistente_Falha()
        {
            using var ctx = NovoContexto(nameof(ExplodirBom_Handler_Estrutura_Inexistente_Falha));
            var handler = new ExplodirBomCommandHandler(ctx);
            var r = await handler.Handle(new ExplodirBomCommand(Guid.NewGuid(), 10m), CancellationToken.None);
            Assert.False(r.Sucesso);
        }

        [Fact]
        public async Task CustoRollup_Handler_Calcula()
        {
            using var ctx = NovoContexto(nameof(CustoRollup_Handler_Calcula));
            var produto = Guid.NewGuid();
            var comp = Guid.NewGuid();
            var estrutura = EstruturaAtiva(produto, 1m, new[] { (comp, 2m) });
            ctx.BomEstruturas.Add(estrutura);
            await ctx.SaveChangesAsync();

            var handler = new CalcularCustoRollupCommandHandler(ctx);
            var r = await handler.Handle(new CalcularCustoRollupCommand(estrutura.Id, TaxaMaoDeObraPorHora: 0m, TaxaCifPorHoraMaquina: 0m), CancellationToken.None);

            Assert.True(r.Sucesso);
        }

        [Fact]
        public async Task Capacidade_Handler_Sinaliza_Gargalo()
        {
            using var ctx = NovoContexto(nameof(Capacidade_Handler_Sinaliza_Gargalo));
            var centro = new CentroTrabalho("CT-01", "Usinagem", 480m, 1, 5, TenantId, UserId);
            ctx.CentrosTrabalho.Add(centro);
            await ctx.SaveChangesAsync();

            var handler = new AvaliarCapacidadeCommandHandler(ctx);

            // 1 dia útil = 480 min de capacidade. Carga 1000 > 480 => PENDENTE_CAPACIDADE.
            var gargalo = await handler.Handle(new AvaliarCapacidadeCommand(centro.Id, 1000m, 1), CancellationToken.None);
            Assert.True(gargalo.Sucesso);
            Assert.Contains("PENDENTE", gargalo.Mensagem);

            // Carga 300 <= 480 => Ok.
            var ok = await handler.Handle(new AvaliarCapacidadeCommand(centro.Id, 300m, 1), CancellationToken.None);
            Assert.True(ok.Sucesso);
            Assert.DoesNotContain("PENDENTE", ok.Mensagem);
        }

        [Fact]
        public async Task Sequenciar_Handler_Respeita_Precedencia()
        {
            var centro = Guid.NewGuid();
            var op1 = Guid.NewGuid();
            var op2 = Guid.NewGuid();

            var command = new SequenciarOperacoesCommand(
                new List<OperacaoSequenciamentoDto>
                {
                    new(op1, centro, 1, 60m, 0m),
                    new(op2, centro, 2, 60m, 0m, Precedencias: new List<Guid> { op1 })
                },
                InicioHorizonte: new DateTime(2026, 6, 1, 8, 0, 0, DateTimeKind.Utc));

            var handler = new SequenciarOperacoesCommandHandler();
            var r = await handler.Handle(command, CancellationToken.None);

            Assert.True(r.Sucesso);
            Assert.NotNull(r.Dados);
        }

        [Fact]
        public async Task Sequenciar_Handler_Ciclo_Falha()
        {
            var centro = Guid.NewGuid();
            var a = Guid.NewGuid();
            var b = Guid.NewGuid();

            // a depende de b e b depende de a => ciclo indispachável.
            var command = new SequenciarOperacoesCommand(
                new List<OperacaoSequenciamentoDto>
                {
                    new(a, centro, 1, 30m, 0m, Precedencias: new List<Guid> { b }),
                    new(b, centro, 1, 30m, 0m, Precedencias: new List<Guid> { a })
                });

            var handler = new SequenciarOperacoesCommandHandler();
            var r = await handler.Handle(command, CancellationToken.None);
            Assert.False(r.Sucesso);
        }

        private sealed class TP : ITenantProvider
        {
            private readonly string _t;
            public TP(string t) => _t = t;
            public string GetTenantId() => _t;
        }

        private sealed class CU : ICurrentUser
        {
            private readonly string _u;
            public CU(string u) => _u = u;
            public string? GetUserId() => _u;
            public string? GetUserName() => "test_user";
            public string? GetUserEmail() => "test@epros.com.br";
        }
    }
}
