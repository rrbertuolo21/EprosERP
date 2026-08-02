using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Epros.Modules.Vendas.Application.Commands;
using Epros.Modules.Vendas.Application.Handlers;
using Epros.Modules.Vendas.Application.Queries;
using Epros.Modules.Vendas.Domain.Entities;
using Epros.Modules.Vendas.Infrastructure.Data;
using Epros.Shared.Application.Contracts;

namespace Epros.Tests
{
    /// <summary>
    /// VEN-PCL / T-02 — isolamento usuário↔cliente do Portal do Cliente.
    /// Prova que, para um principal EXTERNO (claim clienteId), o clienteId vem SEMPRE do sujeito
    /// autenticado — nunca do request — e que usuário externo inativo/bloqueado é barrado (PodeAcessar).
    /// </summary>
    public class PortalClienteSegurancaTests
    {
        private const string TenantId = "tenant-pcl";

        private static ContextVendas CreateContext(string dbName, ICurrentUser currentUser)
        {
            var options = new DbContextOptionsBuilder<ContextVendas>().UseInMemoryDatabase("db_pcl_" + dbName).Options;
            return new ContextVendas(options, new TestTenantProvider(TenantId), currentUser);
        }

        [Fact(DisplayName = "PCL | Principal externo NÃO lê solicitações de outro cliente (T-02)")]
        public async Task Externo_NaoLe_OutroCliente()
        {
            var clienteA = Guid.NewGuid();
            var clienteB = Guid.NewGuid();
            var externoA = new TestCurrentUser("user-a", clienteId: clienteA);
            var ctx = CreateContext(nameof(Externo_NaoLe_OutroCliente), externoA);

            // Duas solicitações: uma de A, uma de B (dados semeados no tenant).
            ctx.PortalSolicitacoes.Add(new PortalSolicitacao(clienteA, null, null, "A1", null, null, TenantId, "seed"));
            ctx.PortalSolicitacoes.Add(new PortalSolicitacao(clienteB, null, null, "B1", null, null, TenantId, "seed"));
            await ctx.SaveChangesAsync();

            // Externo de A tenta forçar o clienteId de B no request → deve ser NEGADO.
            var handler = new ListarPortalSolicitacoesQueryHandler(ctx, new TestTenantProvider(TenantId), externoA);
            var rForcado = await handler.Handle(new ListarPortalSolicitacoesQuery(clienteB), CancellationToken.None);
            Assert.False(rForcado.Sucesso);

            // Sem informar cliente, vê apenas o próprio (A), nunca o de B.
            var rProprio = await handler.Handle(new ListarPortalSolicitacoesQuery(Guid.Empty), CancellationToken.None);
            Assert.True(rProprio.Sucesso);
            var total = (int)rProprio.Dados!.GetType().GetProperty("total")!.GetValue(rProprio.Dados)!;
            Assert.Equal(1, total);
        }

        [Fact(DisplayName = "PCL | Abrir solicitação usa o cliente do principal externo, não o do request (T-02)")]
        public async Task Abrir_UsaClienteDoPrincipal()
        {
            var clienteA = Guid.NewGuid();
            var clienteB = Guid.NewGuid();
            var externoA = new TestCurrentUser("user-a", clienteId: clienteA);
            var ctx = CreateContext(nameof(Abrir_UsaClienteDoPrincipal), externoA);

            // Tenta abrir informando o cliente B no corpo → negado (cross-cliente).
            var r = await new AbrirPortalSolicitacaoCommandHandler(ctx, new TestTenantProvider(TenantId), externoA)
                .Handle(new AbrirPortalSolicitacaoCommand(clienteB, null, null, "X", "Y", null), CancellationToken.None);
            Assert.False(r.Sucesso);

            // Abrindo sem informar cliente, a solicitação nasce vinculada ao cliente A (do principal).
            var rOk = await new AbrirPortalSolicitacaoCommandHandler(ctx, new TestTenantProvider(TenantId), externoA)
                .Handle(new AbrirPortalSolicitacaoCommand(null, null, null, "X", "Y", null), CancellationToken.None);
            Assert.True(rOk.Sucesso);
            var sol = await ctx.PortalSolicitacoes.AsNoTracking().FirstAsync();
            Assert.Equal(clienteA, sol.ClienteId);
        }

        [Fact(DisplayName = "PCL | Usuário externo bloqueado é barrado (PodeAcessar/§18)")]
        public async Task UsuarioBloqueado_Barrado()
        {
            var clienteA = Guid.NewGuid();
            // Cria o usuário externo e o bloqueia.
            var seedCtx = CreateContext(nameof(UsuarioBloqueado_Barrado), new TestCurrentUser("seed"));
            var portalUser = new PortalUsuarioCliente(clienteA, "Ana", "ana@a.com", null, false, null, TenantId, "seed");
            portalUser.Bloquear("seed");
            seedCtx.PortalUsuariosCliente.Add(portalUser);
            await seedCtx.SaveChangesAsync();

            // Principal externo identificado por portalUsuarioId (bloqueado) → negado.
            var externo = new TestCurrentUser("user-a", clienteId: clienteA, portalUsuarioId: portalUser.Id);
            var ctx = CreateContext(nameof(UsuarioBloqueado_Barrado), externo);
            var handler = new ListarPortalSolicitacoesQueryHandler(ctx, new TestTenantProvider(TenantId), externo);
            var r = await handler.Handle(new ListarPortalSolicitacoesQuery(clienteA), CancellationToken.None);
            Assert.False(r.Sucesso);
        }

        [Fact(DisplayName = "PCL | Operador interno (sem vínculo) preserva o filtro do request")]
        public async Task Interno_PreservaRequest()
        {
            var clienteA = Guid.NewGuid();
            var interno = new TestCurrentUser("op-interno"); // sem claim clienteId
            var ctx = CreateContext(nameof(Interno_PreservaRequest), interno);
            ctx.PortalSolicitacoes.Add(new PortalSolicitacao(clienteA, null, null, "A1", null, null, TenantId, "seed"));
            await ctx.SaveChangesAsync();

            var handler = new ListarPortalSolicitacoesQueryHandler(ctx, new TestTenantProvider(TenantId), interno);
            var r = await handler.Handle(new ListarPortalSolicitacoesQuery(clienteA), CancellationToken.None);
            Assert.True(r.Sucesso);
            var total = (int)r.Dados!.GetType().GetProperty("total")!.GetValue(r.Dados)!;
            Assert.Equal(1, total);
        }

        private sealed class TestTenantProvider : ITenantProvider
        {
            private readonly string _t; public TestTenantProvider(string t) => _t = t;
            public string GetTenantId() => _t;
        }

        private sealed class TestCurrentUser : ICurrentUser
        {
            private readonly string _u;
            private readonly Guid? _clienteId;
            private readonly Guid? _portalUsuarioId;
            public TestCurrentUser(string u, Guid? clienteId = null, Guid? portalUsuarioId = null)
            { _u = u; _clienteId = clienteId; _portalUsuarioId = portalUsuarioId; }
            public string? GetUserId() => _u;
            public string? GetUserName() => "Test";
            public string? GetUserEmail() => "t@epros.com";
            public string? GetClaim(string claimType) => claimType switch
            {
                "clienteId" => _clienteId?.ToString(),
                "portalUsuarioId" => _portalUsuarioId?.ToString(),
                _ => null
            };
        }
    }
}
