using System;
using System.Linq;
using System.Threading.Tasks;
using Epros.Modules.GestaoClientes.Domain.Entities;
using Epros.Modules.GestaoClientes.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Xunit;

namespace Epros.Tests
{
    /// <summary>
    /// Suíte anti-vazamento cross-tenant (1.05 / REG teste). Prova, contra um Postgres real com RLS
    /// ligada, que dados criados no tenant A NÃO são visíveis ao operar como tenant B — para as
    /// entidades-chave (Plano custom, Cliente, Fatura, Cupom).
    ///
    /// IMPORTANTE: o usuário de MANUTENÇÃO do container ("epros") é SUPERUSER/BYPASSRLS — a RLS não
    /// se aplica a ele (é assim que as migrations/seed rodam). Portanto a suíte cria e conecta como um
    /// papel de APLICAÇÃO sem privilégio (NOSUPERUSER/NOBYPASSRLS), exatamente como a API deve rodar
    /// em produção, para que a RLS de fato seja exercida. As leituras usam <c>IgnoreQueryFilters()</c>
    /// de propósito: desligam o QueryFilter da aplicação e isolam a proteção do BANCO (RLS).
    /// </summary>
    public class IsolamentoCrossTenantTests
    {
        private const string TenantA = "tenant-iso-a";
        private const string TenantB = "tenant-iso-b";
        private const string AppRole = "epros_rls_test";
        private const string AppPwd = "epros_rls_test";

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
            public string? GetUserName() => "Iso Test";
            public string? GetUserEmail() => "iso@epros.com";
        }

        private static ContextGestaoClientes ContextoPara(string connectionString, string tenantId)
        {
            var provider = new FixedTenantProvider(tenantId);
            var options = PostgresTestDb.BuildOptions<ContextGestaoClientes>(connectionString, provider);
            return new ContextGestaoClientes(options, provider, new FixedCurrentUser(tenantId + "-user"));
        }

        /// <summary>
        /// Garante um papel de aplicação SEM privilégio (RLS aplicável) e devolve uma connection string
        /// conectando como ele, sobre o mesmo banco de teste. O seed continua sendo feito com o usuário
        /// de manutenção (superuser) via <paramref name="superConn"/>.
        /// </summary>
        private static string PrepararConexaoAppRole(string superConn)
        {
            using (var conn = new NpgsqlConnection(superConn))
            {
                conn.Open();
                // Papel compartilhado (cluster-wide), criado de forma idempotente e tolerante a corrida.
                TryExec(conn,
                    $"DO $$ BEGIN IF NOT EXISTS (SELECT FROM pg_roles WHERE rolname='{AppRole}') THEN " +
                    $"CREATE ROLE {AppRole} LOGIN PASSWORD '{AppPwd}' NOSUPERUSER NOBYPASSRLS; END IF; END $$;");
                // Privilégios (por banco) no schema usado por esta suíte.
                TryExec(conn, $"GRANT USAGE ON SCHEMA plataforma TO {AppRole};");
                TryExec(conn, $"GRANT SELECT, INSERT, UPDATE, DELETE ON ALL TABLES IN SCHEMA plataforma TO {AppRole};");
                TryExec(conn, $"GRANT USAGE, SELECT ON ALL SEQUENCES IN SCHEMA plataforma TO {AppRole};");
            }

            var builder = new NpgsqlConnectionStringBuilder(superConn)
            {
                Username = AppRole,
                Password = AppPwd,
                Pooling = false
            };
            return builder.ConnectionString;
        }

        private static void TryExec(NpgsqlConnection conn, string sql)
        {
            try
            {
                using var cmd = conn.CreateCommand();
                cmd.CommandText = sql;
                cmd.ExecuteNonQuery();
            }
            catch (PostgresException)
            {
                // Corrida na criação do papel compartilhado entre classes de teste paralelas: tolerável.
            }
        }

        [Fact]
        public async Task Plano_Cliente_Fatura_Cupom_Do_Tenant_A_Nao_Vazam_Para_O_Tenant_B()
        {
            var superConn = PostgresTestDb.CreateDatabase("db_iso_cross_tenant");
            var appConn = PrepararConexaoAppRole(superConn);

            Guid planoId, clienteId, faturaId, cupomId;

            // 1) Cria dados operando como TENANT A (seed via usuário de manutenção).
            using (var ctxA = ContextoPara(superConn, TenantA))
            {
                var plano = new Plano("Plano Custom A", 199.90m, TenantA, "seed-a");
                ctxA.Planos.Add(plano);

                var cliente = new Cliente("Cliente A Ltda", "11111111000111", "a@cliente.com", plano.Id, TenantA, "seed-a");
                ctxA.Clientes.Add(cliente);

                var fatura = new Fatura(cliente.Id, 500m, DateTime.UtcNow.AddDays(10), TenantA, "seed-a");
                ctxA.Faturas.Add(fatura);

                var cupom = new Cupom("CUPOM-A", "Percentual", 10m, 100, DateTime.UtcNow.AddDays(30), TenantA, "seed-a");
                ctxA.Cupons.Add(cupom);

                await ctxA.SaveChangesAsync();

                planoId = plano.Id;
                clienteId = cliente.Id;
                faturaId = fatura.Id;
                cupomId = cupom.Id;
            }

            // 2) Sanidade: como TENANT A (papel de aplicação, RLS ligada), os dados ESTÃO visíveis.
            using (var ctxA = ContextoPara(appConn, TenantA))
            {
                Assert.NotNull(await ctxA.Planos.IgnoreQueryFilters().FirstOrDefaultAsync(p => p.Id == planoId));
                Assert.NotNull(await ctxA.Clientes.IgnoreQueryFilters().FirstOrDefaultAsync(c => c.Id == clienteId));
                Assert.NotNull(await ctxA.Faturas.IgnoreQueryFilters().FirstOrDefaultAsync(f => f.Id == faturaId));
                Assert.NotNull(await ctxA.Cupons.IgnoreQueryFilters().FirstOrDefaultAsync(c => c.Id == cupomId));
            }

            // 3) Como TENANT B (papel de aplicação, RLS ligada): NADA do tenant A pode ser lido — nem
            //    por Id, nem em lote. IgnoreQueryFilters garante que só a RLS do banco está protegendo.
            using (var ctxB = ContextoPara(appConn, TenantB))
            {
                Assert.Null(await ctxB.Planos.IgnoreQueryFilters().FirstOrDefaultAsync(p => p.Id == planoId));
                Assert.Null(await ctxB.Clientes.IgnoreQueryFilters().FirstOrDefaultAsync(c => c.Id == clienteId));
                Assert.Null(await ctxB.Faturas.IgnoreQueryFilters().FirstOrDefaultAsync(f => f.Id == faturaId));
                Assert.Null(await ctxB.Cupons.IgnoreQueryFilters().FirstOrDefaultAsync(c => c.Id == cupomId));

                Assert.Empty(await ctxB.Planos.IgnoreQueryFilters().Where(p => p.TenantId == TenantA).ToListAsync());
                Assert.Empty(await ctxB.Clientes.IgnoreQueryFilters().Where(c => c.TenantId == TenantA).ToListAsync());
                Assert.Empty(await ctxB.Faturas.IgnoreQueryFilters().Where(f => f.TenantId == TenantA).ToListAsync());
                Assert.Empty(await ctxB.Cupons.IgnoreQueryFilters().Where(c => c.TenantId == TenantA).ToListAsync());
            }
        }

        [Fact]
        public async Task RLS_Bloqueia_Mover_Linha_Para_Outro_Tenant()
        {
            var superConn = PostgresTestDb.CreateDatabase("db_iso_write_guard");
            var appConn = PrepararConexaoAppRole(superConn);

            // A camada de aplicação (ContextBase) já reescreve o tenant_id para o tenant corrente em
            // toda escrita via EF — então um write cross-tenant não passa nem pela aplicação. Aqui
            // testamos a SEGUNDA linha de defesa (o banco): a política RLS de ESCRITA deve impedir mover
            // uma linha do tenant corrente para OUTRO tenant. Rodado como papel de aplicação (RLS ligada).
            Guid planoId;
            using (var ctx = ContextoPara(superConn, TenantA))
            {
                var plano = new Plano("Plano Guard", 50m, TenantA, "seed-a");
                ctx.Planos.Add(plano);
                await ctx.SaveChangesAsync();
                planoId = plano.Id;
            }

            using var conn = new NpgsqlConnection(appConn);
            await conn.OpenAsync();

            using (var setCmd = conn.CreateCommand())
            {
                setCmd.CommandText = "SET app.current_tenant_id = 'tenant-iso-a';";
                await setCmd.ExecuteNonQueryAsync();
            }

            // Sessão no tenant A: a linha é visível (USING). Tentar gravar tenant_id=B deve ser rejeitado
            // pela checagem de escrita da RLS (WITH CHECK explícito ou o USING reaproveitado).
            using var update = conn.CreateCommand();
            update.CommandText = $"UPDATE plataforma.planos SET tenant_id = 'tenant-iso-b' WHERE id = '{planoId}';";

            await Assert.ThrowsAnyAsync<Exception>(async () => await update.ExecuteNonQueryAsync());
        }
    }
}
