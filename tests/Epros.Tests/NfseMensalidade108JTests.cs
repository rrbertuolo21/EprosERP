using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.GestaoClientes.Application.Handlers;
using Epros.Modules.GestaoClientes.Application.Queries;
using Epros.Modules.GestaoClientes.Application.Security;
using Epros.Modules.GestaoClientes.Application.Services;
using Epros.Modules.GestaoClientes.Domain.Entities;
using Epros.Modules.GestaoClientes.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Epros.Tests
{
    /// <summary>
    /// 1.08J — MECANISMO/HOOK de NFS-e da mensalidade SaaS. Software/SaaS é serviço tributável pelo ISS
    /// (LC 116/2003 item 1.05 e/ou 1.03; STF ADI 1.945/5.659); fato gerador = prestação, 1 NFS-e por
    /// competência mensal (skill Negocio-acumulado/fiscal/nfse RN47–RN52). ⛔ O mecanismo PREPARA e PARA no
    /// provedor: alíquota/subitem/município/certificado/provedor são dependência do overlay `negocio-siser`
    /// (VAZIO) + contador + infra. Estes testes validam o MECANISMO e a GUARDA — NENHUM valor fiscal é inventado.
    /// </summary>
    public class NfseMensalidade108JTests
    {
        #region Doubles

        private ContextGestaoClientes CreateContext(string db, string tenantId, string userId)
        {
            var options = new DbContextOptionsBuilder<ContextGestaoClientes>().UseInMemoryDatabase(db).Options;
            return new ContextGestaoClientes(options, new TestTenantProvider(tenantId), new TestCurrentUser(userId));
        }

        private class TestTenantProvider : ITenantProvider
        {
            private readonly string _t; public TestTenantProvider(string t) => _t = t; public string GetTenantId() => _t;
        }
        private class TestCurrentUser : ICurrentUser
        {
            private readonly string _u; public TestCurrentUser(string u) => _u = u;
            public string? GetUserId() => _u; public string? GetUserName() => "Operador"; public string? GetUserEmail() => "op@epros.com";
        }

        /// <summary>Provedor FAKE que emitiria um número — usado só para provar que a GUARDA FISCAL barra antes de chamá-lo.</summary>
        private sealed class ProvedorQueEmitiria : INfseProvider
        {
            public bool FoiChamado { get; private set; }
            public Task<NfseEmissaoResultado> EmitirAsync(NfseEmissaoDados dados, CancellationToken ct)
            {
                FoiChamado = true;
                return Task.FromResult(NfseEmissaoResultado.Emitida("NFSE-FAKE-0001"));
            }
        }

        private static ConfiguracaoGlobal Cfg(string chave, string valor, string tenant, string user)
            => new ConfiguracaoGlobal(chave, valor, false, chave, tenant, user);

        #endregion

        // ===== 1) MARCAÇÃO — fatura de assinatura paga cria 1 NFS-e Pendente por competência (idempotente) =====

        [Fact]
        public async Task LiquidarFatura_CriaUmaNfseMensalidadePendentePorCompetencia_Idempotente()
        {
            const string tenant = "t-nfse", user = "u1";
            using var ctx = CreateContext(Guid.NewGuid().ToString(), tenant, user);

            var plano = new Plano("Mensal", 100m, null, 10, 2, null, tenant, user, duration: PlanoDuration.Mensal);
            ctx.Planos.Add(plano);
            var cliente = new Cliente("Cli", "00.000.000/0001-00", "c@epros.com", plano.Id,
                null, null, 10, StatusSaaS.Ativo, tenant, user);
            ctx.Clientes.Add(cliente);
            var assinatura = new AssinaturaCliente(cliente.Id, plano.Id, AssinaturaStatus.AguardandoAprovacao,
                DateTime.UtcNow, DateTime.UtcNow.AddDays(30), null, "PIX", null, null, tenant, user);
            ctx.AssinaturasClientes.Add(assinatura);
            var fatura = new Fatura(cliente.Id, 100m, DateTime.UtcNow.AddDays(5), tenant, user);
            ctx.Faturas.Add(fatura);
            await ctx.SaveChangesAsync();

            var aprovacao = new DateTime(2026, 3, 10, 0, 0, 0, DateTimeKind.Utc);
            await new FaturaLiquidacaoService(ctx).LiquidarAsync(
                fatura, "pix-1", "PIX", "MercadoPago", 100m, null, null, aprovacao, user, CancellationToken.None);
            await ctx.SaveChangesAsync();

            var registros = await ctx.NfseMensalidades.IgnoreQueryFilters()
                .Where(n => n.FaturaId == fatura.Id).ToListAsync();
            Assert.Single(registros);
            var nfse = registros[0];
            Assert.Equal(NfseMensalidadeStatus.Pendente, nfse.Status);            // MECANISMO nunca emite sozinho
            Assert.Equal(new DateTime(2026, 3, 1, 0, 0, 0, DateTimeKind.Utc), nfse.Competencia); // competência mensal
            Assert.Equal(100m, nfse.ValorBase);
            Assert.Equal(ETipoAmbiente.Homologacao, nfse.Ambiente);               // default seguro (igual à 1.07)
            Assert.Null(nfse.NumeroNfse);                                         // nenhum número fabricado
            Assert.NotNull(nfse.Motivo);                                         // motivo = config/overlay ausente

            // Idempotência: liquidar de novo (fatura já paga) não duplica o registro da competência.
            await new FaturaLiquidacaoService(ctx).LiquidarAsync(
                fatura, "pix-1", "PIX", "MercadoPago", 100m, null, null, aprovacao, user, CancellationToken.None);
            await ctx.SaveChangesAsync();
            Assert.Equal(1, await ctx.NfseMensalidades.IgnoreQueryFilters().CountAsync(n => n.FaturaId == fatura.Id));
        }

        // ===== 2) SEM PROVEDOR/CONFIG — tentativa de emissão mantém Pendente (não emite, não inventa) =====

        [Fact]
        public async Task TentarEmitir_SemProvedorNemConfig_FicaPendente_NaoEmite()
        {
            const string tenant = "t-nfse2", user = "u1";
            using var ctx = CreateContext(Guid.NewGuid().ToString(), tenant, user);

            var cliente = new Cliente("Cli", "00.000.000/0001-00", "c@epros.com", Guid.NewGuid(), tenant, user);
            var fatura = new Fatura(cliente.Id, 250m, DateTime.UtcNow.AddDays(5), tenant, user);
            ctx.Clientes.Add(cliente); ctx.Faturas.Add(fatura);
            await ctx.SaveChangesAsync();

            // Provedor DEFAULT (NfseProviderNaoConfigurado), sem nenhuma ConfiguracaoGlobal fiscal → overlay vazio.
            var svc = new NfseMensalidadeService(ctx);
            var registro = await svc.RegistrarPendenteAsync(
                fatura, new DateTime(2026, 4, 15, 0, 0, 0, DateTimeKind.Utc), user, CancellationToken.None);
            await ctx.SaveChangesAsync();

            var resultado = await svc.TentarEmitirAsync(registro, user, CancellationToken.None);
            await ctx.SaveChangesAsync();

            Assert.Equal(NfseEmissaoSituacao.NaoConfigurado, resultado.Situacao); // guarda barrou antes do provedor
            Assert.Equal(NfseMensalidadeStatus.Pendente, registro.Status);
            Assert.Null(registro.NumeroNfse);                                    // nenhum número inventado
            Assert.Contains("negocio-siser", registro.Motivo);                   // dependência do overlay explícita
        }

        [Fact]
        public async Task TentarEmitir_ConfigIncompleta_GuardaBarraProvedor_MesmoQueProvedorEmitiria()
        {
            const string tenant = "t-nfse3", user = "u1";
            using var ctx = CreateContext(Guid.NewGuid().ToString(), tenant, user);

            var cliente = new Cliente("Cli", "00.000.000/0001-00", "c@epros.com", Guid.NewGuid(), tenant, user);
            var fatura = new Fatura(cliente.Id, 300m, DateTime.UtcNow.AddDays(5), tenant, user);
            ctx.Clientes.Add(cliente); ctx.Faturas.Add(fatura);
            // Config PARCIAL: só alíquota (falta subitem/município/certificado) → guarda deve barrar.
            ctx.ConfiguracoesGlobais.Add(Cfg(NfseMensalidadeService.ChaveAliquotaIss, "5", tenant, user));
            await ctx.SaveChangesAsync();

            var provedor = new ProvedorQueEmitiria();
            var svc = new NfseMensalidadeService(ctx, provedor);
            var registro = await svc.RegistrarPendenteAsync(
                fatura, new DateTime(2026, 5, 1, 0, 0, 0, DateTimeKind.Utc), user, CancellationToken.None);
            await ctx.SaveChangesAsync();

            var resultado = await svc.TentarEmitirAsync(registro, user, CancellationToken.None);
            await ctx.SaveChangesAsync();

            Assert.False(provedor.FoiChamado);                                   // ⛔ guarda barrou ANTES do provedor
            Assert.Equal(NfseEmissaoSituacao.NaoConfigurado, resultado.Situacao);
            Assert.Equal(NfseMensalidadeStatus.Pendente, registro.Status);
            Assert.Null(registro.NumeroNfse);
        }

        [Fact]
        public async Task TentarEmitir_ConfigCompletaComProvedor_EmiteComNumeroReal_SemInventar()
        {
            const string tenant = "t-nfse4", user = "u1";
            using var ctx = CreateContext(Guid.NewGuid().ToString(), tenant, user);

            var cliente = new Cliente("Cli", "00.000.000/0001-00", "c@epros.com", Guid.NewGuid(), tenant, user);
            var fatura = new Fatura(cliente.Id, 400m, DateTime.UtcNow.AddDays(5), tenant, user);
            ctx.Clientes.Add(cliente); ctx.Faturas.Add(fatura);
            // Overlay/config COMPLETO (alíquota/subitem/município/certificado = PARÂMETRO do contador, aqui só presença).
            ctx.ConfiguracoesGlobais.Add(Cfg(NfseMensalidadeService.ChaveAliquotaIss, "5", tenant, user));
            ctx.ConfiguracoesGlobais.Add(Cfg(NfseMensalidadeService.ChaveSubitemLc116, "1.05", tenant, user));
            ctx.ConfiguracoesGlobais.Add(Cfg(NfseMensalidadeService.ChaveMunicipioIncidencia, "4204202", tenant, user));
            ctx.ConfiguracoesGlobais.Add(Cfg(NfseMensalidadeService.ChaveCertificadoConfigurado, "ok", tenant, user));
            await ctx.SaveChangesAsync();

            var provedor = new ProvedorQueEmitiria();
            var svc = new NfseMensalidadeService(ctx, provedor);
            var registro = await svc.RegistrarPendenteAsync(
                fatura, new DateTime(2026, 6, 1, 0, 0, 0, DateTimeKind.Utc), user, CancellationToken.None);
            await ctx.SaveChangesAsync();

            var resultado = await svc.TentarEmitirAsync(registro, user, CancellationToken.None);
            await ctx.SaveChangesAsync();

            Assert.True(provedor.FoiChamado);                                    // guarda liberou → provedor decide
            Assert.Equal(NfseEmissaoSituacao.Emitida, resultado.Situacao);
            Assert.Equal(NfseMensalidadeStatus.Emitida, registro.Status);
            Assert.Equal("NFSE-FAKE-0001", registro.NumeroNfse);                 // número REAL veio do provedor, não inventado
            Assert.NotNull(registro.EmitidaEm);
        }

        [Fact]
        public async Task ProviderNaoConfigurado_SempreRetornaNaoConfigurado()
        {
            var provider = new NfseProviderNaoConfigurado();
            var dados = new NfseEmissaoDados(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
                DateTime.UtcNow, 100m, "t", "Homologacao");
            var r = await provider.EmitirAsync(dados, CancellationToken.None);
            Assert.Equal(NfseEmissaoSituacao.NaoConfigurado, r.Situacao);
            Assert.Null(r.NumeroNfse);
        }

        // ===== 3) ENDPOINT — operador interno lista as pendentes; não-operador é barrado (fail-closed) =====

        [Fact]
        public async Task ListarPendentes_OperadorInterno_RetornaCompetenciasPendentes()
        {
            const string tenant = "t-nfse5", user = "u1";
            var db = Guid.NewGuid().ToString();
            using var ctx = CreateContext(db, tenant, user);

            var cliente = new Cliente("Acme", "00.000.000/0001-00", "c@epros.com", Guid.NewGuid(), tenant, user);
            var f1 = new Fatura(cliente.Id, 100m, DateTime.UtcNow.AddDays(5), tenant, user);
            var f2 = new Fatura(cliente.Id, 200m, DateTime.UtcNow.AddDays(5), tenant, user);
            ctx.Clientes.Add(cliente); ctx.Faturas.Add(f1); ctx.Faturas.Add(f2);
            await ctx.SaveChangesAsync();

            var svc = new NfseMensalidadeService(ctx);
            await svc.RegistrarPendenteAsync(f1, new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), user, CancellationToken.None);
            await svc.RegistrarPendenteAsync(f2, new DateTime(2026, 2, 1, 0, 0, 0, DateTimeKind.Utc), user, CancellationToken.None);
            await ctx.SaveChangesAsync();

            // Operador interno (tenant "system") consulta consolidada por todos os tenants (mesma InMemory db).
            using var ctxOp = CreateContext(db, GuardaOperadorInterno.TenantSistema, user);
            var handler = new ListarNfseMensalidadesQueryHandler(ctxOp, new TestTenantProvider(GuardaOperadorInterno.TenantSistema));
            var page = await handler.Handle(new ListarNfseMensalidadesQuery(Status: "Pendente"), CancellationToken.None);

            Assert.Equal(2, page.TotalRegistros);
            Assert.All(page.Items, i => Assert.Equal("Pendente", i.Status));
            Assert.All(page.Items, i => Assert.Equal("Acme", i.ClienteRazaoSocial));
        }

        [Fact]
        public async Task ListarPendentes_NaoOperador_Barrado_FailClosed()
        {
            const string tenant = "t-nfse6", user = "u1";
            using var ctx = CreateContext(Guid.NewGuid().ToString(), tenant, user);

            var handler = new ListarNfseMensalidadesQueryHandler(ctx, new TestTenantProvider(tenant)); // NÃO é "system"
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                handler.Handle(new ListarNfseMensalidadesQuery(), CancellationToken.None));
        }
    }
}
