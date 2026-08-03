using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Vendas.Application.Commands;
using Epros.Modules.Vendas.Application.Handlers;
using Epros.Modules.Vendas.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Epros.Tests
{
    /// <summary>
    /// VEN-CRM (T-09/EC-05): o CrmHistorico passa a ser EFETIVAMENTE gravado nos eventos de ciclo de
    /// vida de lead e oportunidade (antes mapeado/migrado mas nunca escrito).
    /// </summary>
    public class CrmHistoricoTests
    {
        private const string TenantId = "tenant-crm-hist-001";
        private const string UserId = "user-crm-hist-001";

        private static ContextVendas CreateContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<ContextVendas>().UseInMemoryDatabase(dbName).Options;
            return new ContextVendas(options, new TestTenantProvider(TenantId), new TestCurrentUser(UserId));
        }

        [Fact(DisplayName = "CRM | Criar lead grava histórico de Criação (T-09/EC-05)")]
        public async Task CriarLead_GravaHistorico()
        {
            var ctx = CreateContext(nameof(CriarLead_GravaHistorico));
            var tp = new TestTenantProvider(TenantId); var cu = new TestCurrentUser(UserId);
            var r = await new CriarCrmLeadCommandHandler(ctx, tp, cu).Handle(new CriarCrmLeadCommand(
                null, null, "Lead Teste", null, null, null, null, null, null, null, null, null, Guid.NewGuid(), null, null, null),
                CancellationToken.None);
            Assert.True(r.Sucesso);
            var hist = await ctx.CrmHistoricos.SingleAsync();
            Assert.Equal("Lead", hist.EntidadeTipo);
            Assert.Equal("Criacao", hist.Evento);
        }

        [Fact(DisplayName = "CRM | Ganhar oportunidade grava histórico de Ganho (T-09/EC-05)")]
        public async Task GanharOportunidade_GravaHistorico()
        {
            var ctx = CreateContext(nameof(GanharOportunidade_GravaHistorico));
            var tp = new TestTenantProvider(TenantId); var cu = new TestCurrentUser(UserId);
            await new CriarCrmOportunidadeCommandHandler(ctx, tp, cu).Handle(new CriarCrmOportunidadeCommand(
                Guid.NewGuid(), Guid.NewGuid(), "Op Teste", 1000m, null, null, null, null, null, Guid.NewGuid()),
                CancellationToken.None);
            var op = await ctx.CrmOportunidades.FirstAsync();
            await new GanharCrmOportunidadeCommandHandler(ctx, tp, cu).Handle(new GanharCrmOportunidadeCommand(op.Id), CancellationToken.None);

            var eventos = await ctx.CrmHistoricos.Where(h => h.EntidadeId == op.Id).Select(h => h.Evento).ToListAsync();
            Assert.Contains("Criacao", eventos);
            Assert.Contains("Ganho", eventos);
        }

        private class TestTenantProvider : Epros.Shared.Application.Contracts.ITenantProvider
        {
            private readonly string _t;
            public TestTenantProvider(string t) => _t = t;
            public string GetTenantId() => _t;
        }

        private class TestCurrentUser : Epros.Shared.Application.Contracts.ICurrentUser
        {
            private readonly string _u;
            public TestCurrentUser(string u) => _u = u;
            public string? GetUserId() => _u;
            public string? GetUserName() => "Test User";
            public string? GetUserEmail() => "test@epros.com";
        }
    }
}
