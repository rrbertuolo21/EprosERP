using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Qualidade.Application.EventHandlers;
using Epros.Modules.Qualidade.Domain.Enums;
using Epros.Modules.Qualidade.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Domain.Events;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Epros.Tests
{
    /// <summary>QLD-ACR inbound: MercadoriaRecebida (Estoque LDE) abre analise de aceite de entrada (idempotente).</summary>
    public class QualidadeInboundTests
    {
        private static readonly ITenantProvider Tenant = new TP("tenant-1");
        private static readonly ICurrentUser User = new CU("user-1");

        private static ContextQualidade Novo(string db)
            => new(new DbContextOptionsBuilder<ContextQualidade>().UseInMemoryDatabase(db).Options, Tenant, User);

        private static MercadoriaRecebidaEventNotification Evento(Guid entradaId) => new(
            EntradaId: entradaId,
            CompraId: Guid.NewGuid(),
            FornecedorId: Guid.NewGuid(),
            DocumentoId: Guid.NewGuid(),
            ChaveAcesso: "chave",
            Numero: "1234",
            ValorTotal: 500m,
            TenantId: "tenant-1",
            Itens: new List<MercadoriaRecebidaItemNotification>
            {
                new(Guid.NewGuid(), 100m, 5m),
                new(Guid.NewGuid(), 50m, 2m)
            });

        [Fact]
        public async Task MercadoriaRecebida_Abre_Analise_De_Recebimento_Com_Itens()
        {
            const string db = nameof(MercadoriaRecebida_Abre_Analise_De_Recebimento_Com_Itens);
            var entradaId = Guid.NewGuid();
            using (var ctx = Novo(db))
                await new MercadoriaRecebidaQualidadeHandler(ctx).Handle(Evento(entradaId), CancellationToken.None);

            using (var ctx = Novo(db))
            {
                var analise = await ctx.AcrAnalises.FirstAsync();
                Assert.Equal(ETipoAnaliseAcr.Recebimento, analise.TipoAnalise);
                Assert.Equal(2, await ctx.AcrItens.CountAsync());
            }
        }

        [Fact]
        public async Task MercadoriaRecebida_Eh_Idempotente_Por_Entrada()
        {
            const string db = nameof(MercadoriaRecebida_Eh_Idempotente_Por_Entrada);
            var entradaId = Guid.NewGuid();
            var ev = Evento(entradaId);

            using (var ctx = Novo(db))
                await new MercadoriaRecebidaQualidadeHandler(ctx).Handle(ev, CancellationToken.None);
            using (var ctx = Novo(db))
                await new MercadoriaRecebidaQualidadeHandler(ctx).Handle(ev, CancellationToken.None);

            using (var ctx = Novo(db))
            {
                Assert.Equal(1, await ctx.AcrAnalises.CountAsync());
                Assert.Equal(2, await ctx.AcrItens.CountAsync());
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
