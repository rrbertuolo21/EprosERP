using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Microsoft.EntityFrameworkCore;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Domain.Events;
using Epros.Modules.Estoque.Application.Commands;
using Epros.Modules.Estoque.Application.Handlers;
using Epros.Modules.Estoque.Application.Queries;
using Epros.Modules.Estoque.Domain.Enums;
using Epros.Modules.Estoque.Infrastructure.Data;

namespace Epros.Tests
{
    /// <summary>
    /// EST-PFO — Portal do Fornecedor. Cobre o isolamento por fornecedor (PFO-002 / VAL-PFO-001), a rejeição
    /// de resposta a cotação encerrada (VAL-PFO-003), o fluxo convite→acesso→cotação→resposta e o pré-aviso/
    /// documento com seus eventos de catálogo.
    /// </summary>
    public class PortalFornecedorTests
    {
        private const string TenantId = "tenant-pfo";
        private const string UserId = "user-pfo";

        [Fact(DisplayName = "PFO | Convite → ativar acesso emite eventos e cria acesso")]
        public async Task Convite_AtivarAcesso()
        {
            var ctx = CreateContext(nameof(Convite_AtivarAcesso));
            var tp = new TestTenantProvider(TenantId); var cu = new TestCurrentUser(UserId);
            var fornecedorId = Guid.NewGuid();

            var rConvite = await new ConvidarFornecedorCommandHandler(ctx, tp, cu).Handle(
                new ConvidarFornecedorCommand(fornecedorId, "fornecedor@acme.com"), CancellationToken.None);
            Assert.True(rConvite.Sucesso);
            var conviteId = (Guid)rConvite.Dados!.GetType().GetProperty("Id")!.GetValue(rConvite.Dados)!;

            var rAtivar = await new AtivarAcessoFornecedorCommandHandler(ctx, tp, cu).Handle(
                new AtivarAcessoFornecedorCommand(conviteId, Guid.NewGuid()), CancellationToken.None);
            Assert.True(rAtivar.Sucesso);

            var eventos = await ctx.OutboxMessages.AsNoTracking().Select(o => o.EventType).ToListAsync();
            Assert.Contains(CatalogoEventosIntegracao.Estoque.PfoConviteEnviado, eventos);
            Assert.Contains(CatalogoEventosIntegracao.Estoque.PfoAcessoAtivado, eventos);
            Assert.Single(await ctx.PfoUsuariosFornecedor.AsNoTracking().Where(u => u.FornecedorId == fornecedorId).ToListAsync());
        }

        [Fact(DisplayName = "PFO | Responder cotação de OUTRO fornecedor é bloqueado (VAL-PFO-001)")]
        public async Task Responder_Cotacao_OutroFornecedor_Bloqueado()
        {
            var ctx = CreateContext(nameof(Responder_Cotacao_OutroFornecedor_Bloqueado));
            var tp = new TestTenantProvider(TenantId); var cu = new TestCurrentUser(UserId);
            var fornecedorA = Guid.NewGuid(); var fornecedorB = Guid.NewGuid();

            var rPub = await new PublicarCotacaoFornecedorCommandHandler(ctx, tp, cu).Handle(
                new PublicarCotacaoFornecedorCommand(Guid.NewGuid(), fornecedorA), CancellationToken.None);
            var cotacaoId = (Guid)rPub.Dados!.GetType().GetProperty("Id")!.GetValue(rPub.Dados)!;

            // Fornecedor B tenta responder a cotação do A.
            var r = await new ResponderCotacaoFornecedorCommandHandler(ctx, tp, cu).Handle(
                new ResponderCotacaoFornecedorCommand(cotacaoId, fornecedorB, 100m, new List<RespostaCotacaoItemInput>()), CancellationToken.None);
            Assert.False(r.Sucesso);
        }

        [Fact(DisplayName = "PFO | Responder cotação encerrada é bloqueado (VAL-PFO-003)")]
        public async Task Responder_Cotacao_Encerrada_Bloqueado()
        {
            var ctx = CreateContext(nameof(Responder_Cotacao_Encerrada_Bloqueado));
            var tp = new TestTenantProvider(TenantId); var cu = new TestCurrentUser(UserId);
            var fornecedor = Guid.NewGuid();

            // Prazo já vencido → não aceita resposta.
            var rPub = await new PublicarCotacaoFornecedorCommandHandler(ctx, tp, cu).Handle(
                new PublicarCotacaoFornecedorCommand(Guid.NewGuid(), fornecedor, DateTime.UtcNow.AddDays(-1)), CancellationToken.None);
            var cotacaoId = (Guid)rPub.Dados!.GetType().GetProperty("Id")!.GetValue(rPub.Dados)!;

            var r = await new ResponderCotacaoFornecedorCommandHandler(ctx, tp, cu).Handle(
                new ResponderCotacaoFornecedorCommand(cotacaoId, fornecedor, 100m, new List<RespostaCotacaoItemInput>()), CancellationToken.None);
            Assert.False(r.Sucesso);
        }

        [Fact(DisplayName = "PFO | Responder cotação válida emite est.pfo.cotacao_respondida e marca respondida")]
        public async Task Responder_Cotacao_Valida()
        {
            var ctx = CreateContext(nameof(Responder_Cotacao_Valida));
            var tp = new TestTenantProvider(TenantId); var cu = new TestCurrentUser(UserId);
            var fornecedor = Guid.NewGuid();

            var rPub = await new PublicarCotacaoFornecedorCommandHandler(ctx, tp, cu).Handle(
                new PublicarCotacaoFornecedorCommand(Guid.NewGuid(), fornecedor, DateTime.UtcNow.AddDays(5)), CancellationToken.None);
            var cotacaoId = (Guid)rPub.Dados!.GetType().GetProperty("Id")!.GetValue(rPub.Dados)!;

            var r = await new ResponderCotacaoFornecedorCommandHandler(ctx, tp, cu).Handle(
                new ResponderCotacaoFornecedorCommand(cotacaoId, fornecedor, 250m,
                    new List<RespostaCotacaoItemInput> { new(Guid.NewGuid(), Guid.NewGuid(), 10m, 25m, 7) }), CancellationToken.None);
            Assert.True(r.Sucesso);

            var cot = await ctx.PfoCotacoesPublicadas.AsNoTracking().FirstAsync(c => c.Id == cotacaoId);
            Assert.Equal(EStatusCotacaoPublicada.Respondida, cot.Status);
            var eventos = await ctx.OutboxMessages.AsNoTracking().Select(o => o.EventType).ToListAsync();
            Assert.Contains(CatalogoEventosIntegracao.Estoque.PfoCotacaoRespondida, eventos);
            Assert.Single(await ctx.PfoRespostaCotacaoItens.AsNoTracking().ToListAsync());
        }

        [Fact(DisplayName = "PFO | Pré-aviso emite est.pfo.pre_aviso_enviado (alimenta LDE)")]
        public async Task PreAviso_Enviado()
        {
            var ctx = CreateContext(nameof(PreAviso_Enviado));
            var tp = new TestTenantProvider(TenantId); var cu = new TestCurrentUser(UserId);
            var fornecedor = Guid.NewGuid();

            var r = await new EnviarPreAvisoEmbarqueCommandHandler(ctx, tp, cu).Handle(
                new EnviarPreAvisoEmbarqueCommand(Guid.NewGuid(), fornecedor, DateTime.UtcNow.AddDays(3),
                    new List<PreAvisoItemInput> { new(Guid.NewGuid(), Guid.NewGuid(), 20m, "LOTE-9") }), CancellationToken.None);
            Assert.True(r.Sucesso);

            var eventos = await ctx.OutboxMessages.AsNoTracking().Select(o => o.EventType).ToListAsync();
            Assert.Contains(CatalogoEventosIntegracao.Estoque.PfoPreAvisoEnviado, eventos);
            Assert.True(CatalogoEventosIntegracao.EhEventoConhecido(CatalogoEventosIntegracao.Estoque.PfoPreAvisoEnviado));
        }

        [Fact(DisplayName = "PFO | Documento sem referência é inválido; consulta é isolada por fornecedor")]
        public async Task Documento_E_Isolamento_Consulta()
        {
            var ctx = CreateContext(nameof(Documento_E_Isolamento_Consulta));
            var tp = new TestTenantProvider(TenantId); var cu = new TestCurrentUser(UserId);
            var fornecedorA = Guid.NewGuid(); var fornecedorB = Guid.NewGuid();

            // Documento válido de A.
            var rDoc = await new EnviarDocumentoFornecedorCommandHandler(ctx, tp, cu).Handle(
                new EnviarDocumentoFornecedorCommand(fornecedorA, EReferenciaDocumentoFornecedor.Pedido, Guid.NewGuid(), Guid.NewGuid(), "NF-e"), CancellationToken.None);
            Assert.True(rDoc.Sucesso);

            // Documento inválido (referência vazia).
            var rInval = await new EnviarDocumentoFornecedorCommandHandler(ctx, tp, cu).Handle(
                new EnviarDocumentoFornecedorCommand(fornecedorA, EReferenciaDocumentoFornecedor.Pedido, Guid.Empty, Guid.NewGuid()), CancellationToken.None);
            Assert.False(rInval.Sucesso);

            // Consulta do fornecedor B não vê documentos de A (isolamento).
            var rListaB = await new ListarDocumentosFornecedorQueryHandler(ctx).Handle(new ListarDocumentosFornecedorQuery(fornecedorB), CancellationToken.None);
            var totalB = (int)rListaB.Dados!.GetType().GetProperty("Total")!.GetValue(rListaB.Dados)!;
            Assert.Equal(0, totalB);

            var rListaA = await new ListarDocumentosFornecedorQueryHandler(ctx).Handle(new ListarDocumentosFornecedorQuery(fornecedorA), CancellationToken.None);
            var totalA = (int)rListaA.Dados!.GetType().GetProperty("Total")!.GetValue(rListaA.Dados)!;
            Assert.Equal(1, totalA);
        }

        private ContextEstoque CreateContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<ContextEstoque>().UseInMemoryDatabase("db_pfo_" + dbName).Options;
            return new ContextEstoque(options, new TestTenantProvider(TenantId), new TestCurrentUser(UserId));
        }

        private class TestTenantProvider : ITenantProvider
        {
            private readonly string _t; public TestTenantProvider(string t) => _t = t;
            public string GetTenantId() => _t;
        }

        private class TestCurrentUser : ICurrentUser
        {
            private readonly string _u; public TestCurrentUser(string u) => _u = u;
            public string? GetUserId() => _u;
            public string? GetUserName() => "Test User";
            public string? GetUserEmail() => "test@epros.com";
        }
    }
}
