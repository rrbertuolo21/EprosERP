using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Epros.Modules.Estoque.Application.Commands;
using Epros.Modules.Estoque.Application.Handlers;
using Epros.Modules.Estoque.Domain.Entities;
using Epros.Modules.Estoque.Infrastructure.Data;
using Epros.Shared.Domain.Events;
using Epros.Tests.Integration;
using Xunit;

namespace Epros.Tests
{
    /// <summary>
    /// Testes dos gaps de SUBCONTRATAÇÃO: cobrança de serviço de beneficiamento (SUB-009, gera compra do
    /// serviço via evento) e documento fiscal de remessa/retorno com CFOP parametrizado (SUB-008). InMemory.
    /// </summary>
    public class SubcontratacaoServicoTests
    {
        private const string TenantId = "tenant-sub-001";
        private const string UserId = "user-sub-001";

        private ContextEstoque CreateContext(string db)
        {
            var options = new DbContextOptionsBuilder<ContextEstoque>().UseInMemoryDatabase(db).Options;
            return new ContextEstoque(options, new TestTenantProvider(TenantId), new TestCurrentUser(UserId));
        }

        private static async Task<Guid> SeedOrdemAsync(ContextEstoque ctx)
        {
            var ordem = new SubOrdem(null, "OS-1", null, Guid.NewGuid(), DateTime.UtcNow, DateTime.UtcNow.AddDays(15), null, TenantId, UserId);
            ctx.SubOrdens.Add(ordem);
            await ctx.SaveChangesAsync();
            return ordem.Id;
        }

        [Fact(DisplayName = "SUB-009 | Cobrança de serviço com valor positivo gera a compra do serviço (evento)")]
        public async Task ServicoCobranca_Ok()
        {
            var ctx = CreateContext(nameof(ServicoCobranca_Ok));
            var tenant = new TestTenantProvider(TenantId);
            var user = new TestCurrentUser(UserId);
            var ordemId = await SeedOrdemAsync(ctx);

            var res = await new RegistrarSubServicoCobrancaCommandHandler(ctx, tenant, user)
                .Handle(new RegistrarSubServicoCobrancaCommand(ordemId, 250m), CancellationToken.None);
            Assert.True(res.Sucesso);
            Assert.True(await ctx.SubServicosCobranca.AnyAsync(c => c.OrdemId == ordemId));
            Assert.True(await ctx.OutboxMessages.AnyAsync(o => o.EventType == CatalogoEventosIntegracao.Estoque.SubServicoCobrado));
        }

        [Fact(DisplayName = "SUB-009 | Cobrança com valor não-positivo é rejeitada (SUB-030)")]
        public async Task ServicoCobranca_ValorInvalido()
        {
            var ctx = CreateContext(nameof(ServicoCobranca_ValorInvalido));
            var tenant = new TestTenantProvider(TenantId);
            var user = new TestCurrentUser(UserId);
            var ordemId = await SeedOrdemAsync(ctx);

            var res = await new RegistrarSubServicoCobrancaCommandHandler(ctx, tenant, user)
                .Handle(new RegistrarSubServicoCobrancaCommand(ordemId, 0m), CancellationToken.None);
            Assert.False(res.Sucesso);
        }

        [Fact(DisplayName = "SUB-008 | Documento fiscal registra CFOP remessa/retorno parametrizado (valida-contador)")]
        public async Task DocumentoFiscal_CfopParametrizado()
        {
            var ctx = CreateContext(nameof(DocumentoFiscal_CfopParametrizado));
            var tenant = new TestTenantProvider(TenantId);
            var user = new TestCurrentUser(UserId);
            var ordemId = await SeedOrdemAsync(ctx);

            var res = await new RegistrarSubDocumentoFiscalCommandHandler(ctx, tenant, user)
                .Handle(new RegistrarSubDocumentoFiscalCommand(ordemId, CfopRemessa: "5901", CfopRetorno: "5902"), CancellationToken.None);
            Assert.True(res.Sucesso);

            var doc = await ctx.SubDocumentosFiscais.FirstAsync(d => d.OrdemId == ordemId);
            Assert.Equal("5901", doc.CfopRemessa);
            Assert.Equal("5902", doc.CfopRetorno);
            Assert.True(await ctx.OutboxMessages.AnyAsync(o => o.EventType == CatalogoEventosIntegracao.Estoque.SubDocumentoFiscalRegistrado));
        }
    }
}
