using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Epros.Modules.Aplicativo.Infrastructure.Data;
using Epros.Modules.Aplicativo.Infrastructure.Services;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Domain.Entities;
using Epros.Shared.Domain.Enums;
using Epros.Shared.Domain.Events;
using Epros.Shared.Domain.StatusCanonico;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Epros.Tests
{
    /// <summary>
    /// Kernel TRANSVERSAL compartilhado (fundação): status canônico (T3), catálogo de eventos (T2),
    /// numeração central (T9), auditoria imutável central (T8), GED/assinatura (T10) e rotação do
    /// cofre (T5). Garante que cada transversal é ÚNICA e reutilizável pelos módulos.
    /// </summary>
    public class TransversaisCompartilhadasTests
    {
        private sealed class FixedTenantProvider : ITenantProvider
        {
            private readonly string _tenantId;
            public FixedTenantProvider(string tenantId) => _tenantId = tenantId;
            public string GetTenantId() => _tenantId;
            public bool EhTenantDemo() => false;
        }

        private sealed class FixedCurrentUser : ICurrentUser
        {
            private readonly string _userId;
            public FixedCurrentUser(string userId) => _userId = userId;
            public string? GetUserId() => _userId;
            public string? GetUserName() => "Transversais Test";
            public string? GetUserEmail() => "transversais@epros.com";
        }

        private static ContextAplicativo NovoContexto(string connectionString, string tenant)
        {
            var tenantProvider = new FixedTenantProvider(tenant);
            var currentUser = new FixedCurrentUser("system-test");
            var opts = PostgresTestDb.BuildOptions<ContextAplicativo>(connectionString, tenantProvider);
            return new ContextAplicativo(opts, tenantProvider, currentUser);
        }

        // ===================== T3 — STATUS CANÔNICO ÚNICO =====================

        [Fact]
        public void StatusCanonico_Transicoes_Validas_E_Invalidas()
        {
            Assert.True(MaquinaSituacaoCanonica.PodeTransicionar(ESituacaoCanonica.Rascunho, ESituacaoCanonica.EmAnalise));
            Assert.True(MaquinaSituacaoCanonica.PodeTransicionar(ESituacaoCanonica.EmAnalise, ESituacaoCanonica.Ativo));
            Assert.True(MaquinaSituacaoCanonica.PodeTransicionar(ESituacaoCanonica.Ativo, ESituacaoCanonica.Suspenso));
            Assert.True(MaquinaSituacaoCanonica.PodeTransicionar(ESituacaoCanonica.Suspenso, ESituacaoCanonica.Ativo));
            Assert.True(MaquinaSituacaoCanonica.PodeTransicionar(ESituacaoCanonica.Inativo, ESituacaoCanonica.Ativo));

            // Salto inválido e auto-transição negados.
            Assert.False(MaquinaSituacaoCanonica.PodeTransicionar(ESituacaoCanonica.Rascunho, ESituacaoCanonica.Encerrado));
            Assert.False(MaquinaSituacaoCanonica.PodeTransicionar(ESituacaoCanonica.Ativo, ESituacaoCanonica.Ativo));
        }

        [Fact]
        public void StatusCanonico_Estados_Terminais_Sem_Saida()
        {
            Assert.True(MaquinaSituacaoCanonica.EhTerminal(ESituacaoCanonica.Encerrado));
            Assert.True(MaquinaSituacaoCanonica.EhTerminal(ESituacaoCanonica.Cancelado));
            Assert.Empty(MaquinaSituacaoCanonica.TransicoesPermitidas(ESituacaoCanonica.Encerrado));

            var r = MaquinaSituacaoCanonica.Transicionar(ESituacaoCanonica.Cancelado, ESituacaoCanonica.Ativo);
            Assert.False(r.Ok);
            Assert.False(string.IsNullOrEmpty(r.Mensagem));

            var ok = MaquinaSituacaoCanonica.Transicionar(ESituacaoCanonica.Ativo, ESituacaoCanonica.Encerrado);
            Assert.True(ok.Ok);
            Assert.Equal(ESituacaoCanonica.Encerrado, ok.Situacao);
        }

        // ===================== T2 — CATÁLOGO DE EVENTOS =====================

        [Fact]
        public void CatalogoEventos_Reconhece_Conhecidos_E_Rejeita_Desconhecidos()
        {
            Assert.True(CatalogoEventosIntegracao.EhEventoConhecido(CatalogoEventosIntegracao.Vendas.VendaFaturada));
            Assert.True(CatalogoEventosIntegracao.EhEventoConhecido(CatalogoEventosIntegracao.Pessoa.Anonimizada));
            Assert.True(CatalogoEventosIntegracao.EhEventoConhecido("est.lde.entrada_criada"));

            Assert.False(CatalogoEventosIntegracao.EhEventoConhecido("evento.que.nao.existe"));
            Assert.False(CatalogoEventosIntegracao.EhEventoConhecido(""));
            Assert.NotEmpty(CatalogoEventosIntegracao.Todos);
        }

        [Fact]
        public void EnvelopeEvento_Inicia_Cadeia_De_Correlacao()
        {
            var env = EnvelopeEvento.Novo(CatalogoEventosIntegracao.Vendas.VendaFaturada, "tenant-x", actorId: "u1");
            Assert.NotEqual(Guid.Empty, env.MessageId);
            Assert.Equal(env.MessageId.ToString(), env.CorrelationId); // sem correlação prévia, o próprio id inicia
            Assert.Equal(CatalogoEventosIntegracao.Versao, env.SchemaVersao);
        }

        // ===================== T10 — GED / ASSINATURA =====================

        [Fact]
        public void DocumentoGed_RequerAssinatura_Fica_Pendente_E_Versiona()
        {
            var doc = new DocumentoGed("contrato.pdf", "contrato", "hash-v1", 100, "tenant-x", "user",
                requerAssinatura: true);
            Assert.Equal(EStatusAssinaturaDocumento.PendenteAssinatura, doc.StatusAssinatura);
            Assert.Equal(1, doc.Versao);

            doc.RegistrarNovaVersao("hash-v2", 200, "storage/2", "user");
            Assert.Equal(2, doc.Versao);
            Assert.Equal("hash-v2", doc.Hash);

            doc.ConfirmarAssinatura("user");
            Assert.Equal(EStatusAssinaturaDocumento.Assinado, doc.StatusAssinatura);
            Assert.NotNull(doc.AssinadoEm);
        }

        [Fact]
        public async Task Assinatura_Sem_Provedor_Fica_Pendente_Nunca_Assina()
        {
            IAssinaturaDigitalService svc = new AssinaturaDigitalPendenteService();
            var r = await svc.SolicitarAssinaturaAsync(Guid.NewGuid());
            Assert.True(r.Pendente);
            Assert.False(r.Assinado);
        }

        // ===================== T5 — COFRE + ROTAÇÃO =====================

        [Fact]
        public async Task Cofre_Rotacao_Reencripta_Preservando_Texto_Plano()
        {
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Cofre:VaultUrl"] = "http://localhost:8200",
                    ["Cofre:VaultToken"] = "token-teste",
                    ["Cofre:KekLocal"] = "kek-de-teste-para-rotacao-32bytes",
                })
                .Build();

            var vault = new Epros.Infrastructure.Services.VaultEncryptionService(
                new HttpClient(), config, NullLogger<Epros.Infrastructure.Services.VaultEncryptionService>.Instance);

            const string segredo = "chave-de-gateway-super-secreta";
            var cipher1 = await ((ISegredoCofreService)vault).CriptografarAsync(segredo); // fallback local (sem Vault)
            var cipher2 = await ((ISegredoRotacaoService)vault).RotacionarAsync(cipher1);

            Assert.NotEqual(cipher1, cipher2); // novo nonce/versão
            var plano = await ((ISegredoCofreService)vault).DescriptografarAsync(cipher2);
            Assert.Equal(segredo, plano); // texto plano preservado após rotação

            var rotacao = (ISegredoRotacaoService)vault;
            Assert.True(rotacao.PrecisaRotacionar(DateTime.UtcNow.AddDays(-100), 90));
            Assert.False(rotacao.PrecisaRotacionar(DateTime.UtcNow.AddDays(-10), 90));
        }

        // ===================== T9 — NUMERAÇÃO CENTRAL (Postgres) =====================

        [Fact]
        public async Task Numeracao_Sequencial_Sem_Gap_E_Isolada_Por_Tenant_E_Tipo()
        {
            var conn = PostgresTestDb.CreateDatabase("db_numeracao_seq");

            using (var ctx = NovoContexto(conn, "tenant-num-A"))
            {
                var svc = new NumeracaoService(ctx, new FixedTenantProvider("tenant-num-A"), new FixedCurrentUser("u"));
                Assert.Equal(1, await svc.ProximoNumeroAsync("pedido"));
                Assert.Equal(2, await svc.ProximoNumeroAsync("pedido"));
                Assert.Equal(3, await svc.ProximoNumeroAsync("pedido"));
                // Tipo diferente = sequência independente.
                Assert.Equal(1, await svc.ProximoNumeroAsync("nfe"));
            }

            // Tenant diferente = sequência independente (isolamento).
            using (var ctx = NovoContexto(conn, "tenant-num-B"))
            {
                var svc = new NumeracaoService(ctx, new FixedTenantProvider("tenant-num-B"), new FixedCurrentUser("u"));
                Assert.Equal(1, await svc.ProximoNumeroAsync("pedido"));
            }
        }

        [Fact]
        public async Task Numeracao_Concorrente_Nunca_Duplica()
        {
            var conn = PostgresTestDb.CreateDatabase("db_numeracao_conc");
            const int n = 40;

            // Cada tarefa usa o SEU próprio DbContext (DbContext não é thread-safe); o UPSERT atômico
            // garante que ninguém recebe o mesmo número, mesmo sob concorrência.
            var tarefas = Enumerable.Range(0, n).Select(async _ =>
            {
                using var ctx = NovoContexto(conn, "tenant-conc");
                var svc = new NumeracaoService(ctx, new FixedTenantProvider("tenant-conc"), new FixedCurrentUser("u"));
                return await svc.ProximoNumeroAsync("os");
            });

            var numeros = await Task.WhenAll(tarefas);

            Assert.Equal(n, numeros.Distinct().Count());          // sem duplicidade
            Assert.Equal(Enumerable.Range(1, n), numeros.OrderBy(x => x).Select(x => (int)x)); // sem gap: 1..n
        }

        // ===================== T8 — AUDITORIA IMUTÁVEL CENTRAL (Postgres) =====================

        [Fact]
        public async Task Auditoria_Central_Grava_Trilha_Imutavel_Isolada_Por_Tenant()
        {
            var conn = PostgresTestDb.CreateDatabase("db_auditoria_central");

            using (var ctx = NovoContexto(conn, "tenant-aud"))
            {
                var svc = new RegistroAuditoriaService(ctx, new FixedTenantProvider("tenant-aud"), new FixedCurrentUser("ana"));
                await svc.RegistrarAsync("Venda", "V-1", "Criado", valoresAntes: null, valoresDepois: "{\"total\":10}");
                await svc.RegistrarAsync("Venda", "V-1", "Cancelado", valoresAntes: "{\"total\":10}", valoresDepois: null);
            }

            using (var ctx = NovoContexto(conn, "tenant-aud"))
            {
                var registros = await ctx.RegistrosAuditoria.OrderBy(r => r.OcorridoEm).ToListAsync();
                Assert.Equal(2, registros.Count);
                Assert.Equal("Criado", registros[0].Acao);
                Assert.Equal("ana", registros[0].Usuario);
                Assert.Equal("Cancelado", registros[1].Acao);
                Assert.All(registros, r => Assert.Equal("tenant-aud", r.TenantId));
            }

            // Outro tenant não enxerga a trilha (RLS + filtro por tenant).
            using (var ctx = NovoContexto(conn, "outro-tenant"))
            {
                Assert.Empty(await ctx.RegistrosAuditoria.ToListAsync());
            }
        }
    }
}
