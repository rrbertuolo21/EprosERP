using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Epros.Modules.Estoque.Application.Commands;
using Epros.Modules.Estoque.Application.Handlers;
using Epros.Modules.Estoque.Application.Queries;
using Epros.Modules.Estoque.Domain.Entities;
using Epros.Modules.Estoque.Domain.Enums;
using Epros.Modules.Estoque.Infrastructure.Data;
using Epros.Tests.Integration;
using Xunit;

namespace Epros.Tests
{
    /// <summary>
    /// Testes da alçada de aprovação de compras multi-nível (CD3 / EF SOURCING §5.8, §6.2):
    /// invariantes das regras, casamento por valor/comprador/categoria, e o workflow completo
    /// (solicitar → aprovar em cadeia / reprovar bloqueia) via CQRS InMemory.
    /// </summary>
    public class ComprasAlcadaTests
    {
        private const string TenantId = "tenant-alc-001";
        private const string UserId = "user-alc-001";

        private ContextEstoque CreateContext(string db)
        {
            var options = new DbContextOptionsBuilder<ContextEstoque>().UseInMemoryDatabase(db).Options;
            return new ContextEstoque(options, new TestTenantProvider(TenantId), new TestCurrentUser(UserId));
        }

        // ===================== Domínio: regra =====================

        [Fact(DisplayName = "AlcadaRegra | Sem aprovador (pessoa ou papel) é inválida (ALC-004)")]
        public void Regra_SemAprovador_Invalida()
        {
            var r = new ComprasAlcadaRegra(1, 0m, null, null, null, null, null, true, TenantId, UserId);
            Assert.False(r.IsValid);
        }

        [Fact(DisplayName = "AlcadaRegra | Máximo menor ou igual ao mínimo é inválido (ALC-003)")]
        public void Regra_FaixaInvertida_Invalida()
        {
            var r = new ComprasAlcadaRegra(1, 1000m, 500m, null, null, null, "Gerente", true, TenantId, UserId);
            Assert.False(r.IsValid);
        }

        [Fact(DisplayName = "AlcadaRegra | Aplica respeita faixa, comprador e categoria")]
        public void Regra_Aplica_RespeitaCriterios()
        {
            var comprador = Guid.NewGuid();
            var r = new ComprasAlcadaRegra(1, 1000m, 5000m, comprador, "MRO", null, "Gerente", true, TenantId, UserId);
            Assert.True(r.Aplica(2000m, comprador, "MRO"));
            Assert.False(r.Aplica(200m, comprador, "MRO"));      // abaixo da faixa
            Assert.False(r.Aplica(2000m, Guid.NewGuid(), "MRO")); // outro comprador
            Assert.False(r.Aplica(2000m, comprador, "Insumos"));  // outra categoria
        }

        [Fact(DisplayName = "AlcadaRegra | Inativa nunca aplica")]
        public void Regra_Inativa_NuncaAplica()
        {
            var r = new ComprasAlcadaRegra(1, 0m, null, null, null, null, "Gerente", false, TenantId, UserId);
            Assert.False(r.Aplica(999999m, null, null));
        }

        // ===================== Domínio: workflow =====================

        [Fact(DisplayName = "PedidoAprovacao | Sem níveis nasce Aprovado (não há alçada a exercer)")]
        public void Pedido_SemNiveis_NasceAprovado()
        {
            var p = new ComprasPedidoAprovacao(EOrigemAprovacaoCompra.PedidoCompra, Guid.NewGuid(), 100m, null, null, TenantId, UserId);
            p.FinalizarMontagem(UserId);
            Assert.Equal(EStatusPedidoAprovacaoCompra.Aprovado, p.Status);
            Assert.True(p.FoiAprovado());
        }

        [Fact(DisplayName = "PedidoAprovacao | Dois níveis: aprova em cadeia só conclui no último")]
        public void Pedido_DoisNiveis_ConcluiNoUltimo()
        {
            var p = new ComprasPedidoAprovacao(EOrigemAprovacaoCompra.PedidoCompra, Guid.NewGuid(), 8000m, null, null, TenantId, UserId);
            p.AdicionarNivel(new ComprasPedidoAprovacaoNivel(p.Id, 1, null, "Gerente", 0m, 5000m, TenantId, UserId));
            p.AdicionarNivel(new ComprasPedidoAprovacaoNivel(p.Id, 2, null, "Diretor", 5000m, null, TenantId, UserId));
            p.FinalizarMontagem(UserId);
            Assert.Equal(EStatusPedidoAprovacaoCompra.Pendente, p.Status);
            Assert.Equal(1, p.NivelAtual);

            Assert.True(p.Aprovar("aprovador-1", "ok nível 1"));
            Assert.Equal(EStatusPedidoAprovacaoCompra.Pendente, p.Status); // ainda falta nível 2
            Assert.Equal(2, p.NivelAtual);

            Assert.True(p.Aprovar("aprovador-2", "ok nível 2"));
            Assert.Equal(EStatusPedidoAprovacaoCompra.Aprovado, p.Status);
            Assert.NotNull(p.DecididoEm);
        }

        [Fact(DisplayName = "PedidoAprovacao | Reprova em qualquer nível bloqueia o pedido (SRC-008)")]
        public void Pedido_Reprova_Bloqueia()
        {
            var p = new ComprasPedidoAprovacao(EOrigemAprovacaoCompra.PedidoCompra, Guid.NewGuid(), 8000m, null, null, TenantId, UserId);
            p.AdicionarNivel(new ComprasPedidoAprovacaoNivel(p.Id, 1, null, "Gerente", 0m, 5000m, TenantId, UserId));
            p.AdicionarNivel(new ComprasPedidoAprovacaoNivel(p.Id, 2, null, "Diretor", 5000m, null, TenantId, UserId));
            p.FinalizarMontagem(UserId);

            Assert.True(p.Reprovar("aprovador-1", "fora do orçamento"));
            Assert.Equal(EStatusPedidoAprovacaoCompra.Reprovado, p.Status);
            Assert.False(p.Aprovar("aprovador-2", null)); // já decidido, não pode
        }

        // ===================== CQRS InMemory =====================

        [Fact(DisplayName = "Alçada CQRS | Solicitar monta níveis das regras e aprovar em cadeia conclui")]
        public async Task Cqrs_Solicitar_Aprovar_FluxoCompleto()
        {
            var context = CreateContext("db_alcada_fluxo");
            var tenant = new TestTenantProvider(TenantId);
            var user = new TestCurrentUser(UserId);

            // Duas regras: nível 1 (0–5000) e nível 2 (5000+).
            await new CriarComprasAlcadaRegraCommandHandler(context, tenant, user)
                .Handle(new CriarComprasAlcadaRegraCommand(1, 0m, 5000m, null, null, null, "Gerente", true), CancellationToken.None);
            await new CriarComprasAlcadaRegraCommandHandler(context, tenant, user)
                .Handle(new CriarComprasAlcadaRegraCommand(2, 5000m, null, null, null, null, "Diretor", true), CancellationToken.None);

            // Pedido de 8000 → casa as duas regras (2 níveis).
            var solic = await new SolicitarAprovacaoCompraCommandHandler(context, tenant, user)
                .Handle(new SolicitarAprovacaoCompraCommand(EOrigemAprovacaoCompra.PedidoCompra, Guid.NewGuid(), 8000m), CancellationToken.None);
            Assert.True(solic.Sucesso);
            var pedidoId = (Guid)solic.Dados!.GetType().GetProperty("Id")!.GetValue(solic.Dados)!;

            var pedido = await context.ComprasPedidosAprovacao.Include(x => x.Niveis).FirstAsync(x => x.Id == pedidoId);
            Assert.Equal(2, pedido.QuantidadeNiveis);
            Assert.Equal(EStatusPedidoAprovacaoCompra.Pendente, pedido.Status);

            await new AprovarNivelAprovacaoCompraCommandHandler(context, tenant, user)
                .Handle(new AprovarNivelAprovacaoCompraCommand(pedidoId, "nível 1 ok"), CancellationToken.None);
            var apr2 = await new AprovarNivelAprovacaoCompraCommandHandler(context, tenant, user)
                .Handle(new AprovarNivelAprovacaoCompraCommand(pedidoId, "nível 2 ok"), CancellationToken.None);
            Assert.True(apr2.Sucesso);

            var final = await context.ComprasPedidosAprovacao.FirstAsync(x => x.Id == pedidoId);
            Assert.Equal(EStatusPedidoAprovacaoCompra.Aprovado, final.Status);

            // Outbox: solicitada + concluída (aprovada).
            var eventos = await context.OutboxMessages.Where(o => o.TenantId == TenantId).Select(o => o.EventType).ToListAsync();
            Assert.Contains("AprovacaoSolicitada", eventos);
            Assert.Contains("AprovacaoConcluida", eventos);
        }

        [Fact(DisplayName = "Alçada CQRS | Pedido abaixo de toda faixa nasce aprovado sem níveis")]
        public async Task Cqrs_SemRegraAplicavel_NasceAprovado()
        {
            var context = CreateContext("db_alcada_sem_regra");
            var tenant = new TestTenantProvider(TenantId);
            var user = new TestCurrentUser(UserId);

            await new CriarComprasAlcadaRegraCommandHandler(context, tenant, user)
                .Handle(new CriarComprasAlcadaRegraCommand(1, 5000m, null, null, null, null, "Diretor", true), CancellationToken.None);

            var solic = await new SolicitarAprovacaoCompraCommandHandler(context, tenant, user)
                .Handle(new SolicitarAprovacaoCompraCommand(EOrigemAprovacaoCompra.Compra, Guid.NewGuid(), 100m), CancellationToken.None);
            Assert.True(solic.Sucesso);
            var pedidoId = (Guid)solic.Dados!.GetType().GetProperty("Id")!.GetValue(solic.Dados)!;

            var pedido = await context.ComprasPedidosAprovacao.FirstAsync(x => x.Id == pedidoId);
            Assert.Equal(0, pedido.QuantidadeNiveis);
            Assert.Equal(EStatusPedidoAprovacaoCompra.Aprovado, pedido.Status);
        }
    }
}
