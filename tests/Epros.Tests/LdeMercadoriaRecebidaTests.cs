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
using Epros.Modules.Estoque.Infrastructure.Data;

namespace Epros.Tests
{
    /// <summary>
    /// Integração da Logística de Entrada (EST-LDE): a confirmação do recebimento físico deve publicar
    /// o evento de catálogo <see cref="CatalogoEventosIntegracao.Estoque.MercadoriaRecebida"/> para os
    /// consumidores a jusante (Qualidade → inspeção; Financeiro → contas a pagar), com payload rico
    /// (itens conferíveis + duplicatas). Cobre o gap apontado no escopo do submódulo LDE.
    /// </summary>
    public class LdeMercadoriaRecebidaTests
    {
        private const string TenantId = "tenant-lde-mr";
        private const string UserId = "user-lde-mr";

        [Fact(DisplayName = "LDE | Confirmar entrada publica MercadoriaRecebida no catálogo com itens e duplicatas")]
        public async Task Confirmar_Entrada_Publica_MercadoriaRecebida()
        {
            var ctx = CreateContext(nameof(Confirmar_Entrada_Publica_MercadoriaRecebida));
            var tp = new TestTenantProvider(TenantId);
            var cu = new TestCurrentUser(UserId);

            var compraId = Guid.NewGuid();
            var fornecedorId = Guid.NewGuid();
            var produtoId = Guid.NewGuid();

            // 1. Criar entrada em rascunho.
            var criar = new CriarLdeEntradaCommandHandler(ctx, tp, cu);
            var rCriar = await criar.Handle(new CriarLdeEntradaCommand(compraId, fornecedorId), CancellationToken.None);
            Assert.True(rCriar.Sucesso);
            var entradaId = (Guid)rCriar.Dados!.GetType().GetProperty("Id")!.GetValue(rCriar.Dados)!;

            // 2. Vincular documento com item + duplicata + emitente.
            var vincular = new VincularLdeDocumentoCommandHandler(ctx, tp, cu);
            var rVinc = await vincular.Handle(new VincularLdeDocumentoCommand(
                EntradaId: entradaId,
                ChaveAcesso: "11111111111111111111111111111111111111111111",
                Numero: "000999",
                Serie: "1",
                DataEmissao: DateTime.UtcNow,
                NaturezaOperacao: "Compra para revenda",
                ValorTotal: 500m,
                FornecedorId: fornecedorId,
                DestinatarioId: Guid.NewGuid(),
                EmitenteId: Guid.NewGuid(),
                Itens: new List<LdeDocumentoItemInput> { new(produtoId, 10m, 50m, null) },
                Duplicatas: new List<LdeDocumentoDuplicataInput> { new("000999/01", DateTime.UtcNow.AddDays(30), 500m) }
            ), CancellationToken.None);
            Assert.True(rVinc.Sucesso);

            // 3. Confirmar recebimento.
            var confirmar = new ConfirmarLdeEntradaCommandHandler(ctx, tp, cu);
            var rConf = await confirmar.Handle(new ConfirmarLdeEntradaCommand(entradaId), CancellationToken.None);
            Assert.True(rConf.Sucesso, rConf.Mensagem);

            // 4. O evento MercadoriaRecebida foi enfileirado no Outbox e é reconhecido pelo catálogo.
            var eventos = await ctx.OutboxMessages.AsNoTracking().Select(o => o.EventType).ToListAsync();
            Assert.Contains(CatalogoEventosIntegracao.Estoque.MercadoriaRecebida, eventos);
            Assert.True(CatalogoEventosIntegracao.EhEventoConhecido(CatalogoEventosIntegracao.Estoque.MercadoriaRecebida));

            var msg = await ctx.OutboxMessages.AsNoTracking()
                .FirstAsync(o => o.EventType == CatalogoEventosIntegracao.Estoque.MercadoriaRecebida);
            Assert.Contains(produtoId.ToString(), msg.Payload);
            Assert.Contains("000999/01", msg.Payload); // duplicata para o Financeiro
            Assert.Null(msg.ProcessadoEm);
        }

        [Fact(DisplayName = "LDE | Entrada sem documento não confirma e não publica MercadoriaRecebida")]
        public async Task Sem_Documento_Nao_Publica()
        {
            var ctx = CreateContext(nameof(Sem_Documento_Nao_Publica));
            var tp = new TestTenantProvider(TenantId);
            var cu = new TestCurrentUser(UserId);

            var criar = new CriarLdeEntradaCommandHandler(ctx, tp, cu);
            var rCriar = await criar.Handle(new CriarLdeEntradaCommand(Guid.NewGuid(), Guid.NewGuid()), CancellationToken.None);
            var entradaId = (Guid)rCriar.Dados!.GetType().GetProperty("Id")!.GetValue(rCriar.Dados)!;

            var confirmar = new ConfirmarLdeEntradaCommandHandler(ctx, tp, cu);
            var rConf = await confirmar.Handle(new ConfirmarLdeEntradaCommand(entradaId), CancellationToken.None);

            Assert.False(rConf.Sucesso);
            var eventos = await ctx.OutboxMessages.AsNoTracking().Select(o => o.EventType).ToListAsync();
            Assert.DoesNotContain(CatalogoEventosIntegracao.Estoque.MercadoriaRecebida, eventos);
        }

        private ContextEstoque CreateContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<ContextEstoque>()
                .UseInMemoryDatabase("db_lde_mr_" + dbName).Options;
            return new ContextEstoque(options, new TestTenantProvider(TenantId), new TestCurrentUser(UserId));
        }

        private class TestTenantProvider : ITenantProvider
        {
            private readonly string _t;
            public TestTenantProvider(string t) => _t = t;
            public string GetTenantId() => _t;
        }

        private class TestCurrentUser : ICurrentUser
        {
            private readonly string _u;
            public TestCurrentUser(string u) => _u = u;
            public string? GetUserId() => _u;
            public string? GetUserName() => "Test User";
            public string? GetUserEmail() => "test@epros.com";
        }
    }
}
