using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Epros.Infrastructure.Data;
using Epros.Modules.Aplicativo.Infrastructure.Data;
using Epros.Modules.GestaoClientes.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql;
using Testcontainers.PostgreSql;

namespace Epros.Tests
{
    /// <summary>
    /// Provisiona um banco PostgreSQL real e isolado por teste.
    ///
    /// No CI: preferir Postgres via service container (`EPROS_TEST_PG`) — evita Testcontainers
    /// (Docker-in-Docker), que vinha pendurando o testhost no GitHub Actions.
    /// Local: se a env não existir, sobe postgres:16-alpine via Testcontainers.
    ///
    /// Um "template" é migrado uma vez; cada teste recebe `CREATE DATABASE ... TEMPLATE`.
    /// </summary>
    public static class PostgresTestDb
    {
        public const string ExternalConnectionEnv = "EPROS_TEST_PG";

        private const string MaintenanceDb = "epros";
        private const string TemplateDb = "epros_rlstpl";
        private static readonly TimeSpan ContainerStartTimeout = TimeSpan.FromMinutes(3);
        private static readonly TimeSpan ExternalReadyTimeout = TimeSpan.FromMinutes(2);

        private static readonly object _initGate = new object();
        private static readonly object _createGate = new object();
        private static Task? _initTask;
        private static PostgreSqlContainer? _container;
        private static string? _baseConnectionString;
        private static bool _templateReady;

        [ModuleInitializer]
        internal static void KickoffWarmup()
        {
            try { _ = GetOrStartInitTask(); }
            catch { /* CreateDatabase propaga o erro real */ }
        }

        private static string ConnFor(string database)
        {
            if (string.IsNullOrWhiteSpace(_baseConnectionString))
            {
                throw new InvalidOperationException("Postgres de teste ainda não foi iniciado.");
            }

            var builder = new NpgsqlConnectionStringBuilder(_baseConnectionString)
            {
                Database = database,
                Pooling = false
            };
            return builder.ConnectionString;
        }

        public static string CreateDatabase(string logicalName)
        {
            var dbName = Sanitize(logicalName);
            EnsureReady();

            lock (_createGate)
            {
                using var conn = new NpgsqlConnection(ConnFor(MaintenanceDb));
                conn.Open();
                Execute(conn, $"DROP DATABASE IF EXISTS \"{dbName}\" WITH (FORCE);");
                Execute(conn, $"CREATE DATABASE \"{dbName}\" TEMPLATE \"{TemplateDb}\";");
            }

            return ConnFor(dbName);
        }

        public static void Warmup() => EnsureReady();

        private static void EnsureReady()
            => GetOrStartInitTask().ConfigureAwait(false).GetAwaiter().GetResult();

        private static Task GetOrStartInitTask()
        {
            if (_initTask is not null) return _initTask;
            lock (_initGate)
            {
                if (_initTask is not null) return _initTask;
                _initTask = InitializeAsync();
                return _initTask;
            }
        }

        private static async Task InitializeAsync()
        {
            var external = Environment.GetEnvironmentVariable(ExternalConnectionEnv);
            if (!string.IsNullOrWhiteSpace(external))
            {
                Console.WriteLine($"[PostgresTestDb] Usando Postgres externo ({ExternalConnectionEnv}).");
                Console.Out.Flush();
                _baseConnectionString = NormalizeMaintenanceDb(external);
                await WaitUntilAcceptsConnectionsAsync(_baseConnectionString, ExternalReadyTimeout).ConfigureAwait(false);
                Console.WriteLine("[PostgresTestDb] Postgres externo pronto.");
                Console.Out.Flush();
            }
            else
            {
                await StartTestcontainersAsync().ConfigureAwait(false);
            }

            Console.WriteLine("[PostgresTestDb] Aplicando migrations no template...");
            Console.Out.Flush();
            await Task.Run(EnsureTemplate).ConfigureAwait(false);
            Console.WriteLine("[PostgresTestDb] Template RLS pronto.");
            Console.Out.Flush();
        }

        private static async Task StartTestcontainersAsync()
        {
            Console.WriteLine($"[PostgresTestDb] Iniciando postgres:16-alpine via Testcontainers (timeout {ContainerStartTimeout.TotalMinutes:0} min)...");
            Console.Out.Flush();

            var container = new PostgreSqlBuilder()
                .WithImage("postgres:16-alpine")
                .WithDatabase(MaintenanceDb)
                .WithUsername("epros")
                .WithPassword("epros")
                .Build();

            try
            {
                using var cts = new CancellationTokenSource(ContainerStartTimeout);
                await container.StartAsync(cts.Token).ConfigureAwait(false);
                _container = container;
                _baseConnectionString = container.GetConnectionString();
                Console.WriteLine("[PostgresTestDb] Container Postgres pronto.");
                Console.Out.Flush();
            }
            catch (Exception ex)
            {
                try { await container.DisposeAsync().ConfigureAwait(false); } catch { /* ignore */ }
                throw new InvalidOperationException(
                    "Não foi possível iniciar o PostgreSQL via Testcontainers em " +
                    $"{ContainerStartTimeout.TotalMinutes:0} min. " +
                    $"No CI, defina {ExternalConnectionEnv}. Detalhe: " + ex.Message,
                    ex);
            }
        }

        private static string NormalizeMaintenanceDb(string connectionString)
        {
            var builder = new NpgsqlConnectionStringBuilder(connectionString)
            {
                Database = MaintenanceDb,
                Pooling = false
            };
            return builder.ConnectionString;
        }

        private static async Task WaitUntilAcceptsConnectionsAsync(string connectionString, TimeSpan timeout)
        {
            var deadline = DateTime.UtcNow + timeout;
            Exception? last = null;
            while (DateTime.UtcNow < deadline)
            {
                try
                {
                    await using var conn = new NpgsqlConnection(connectionString);
                    await conn.OpenAsync().ConfigureAwait(false);
                    return;
                }
                catch (Exception ex)
                {
                    last = ex;
                    await Task.Delay(500).ConfigureAwait(false);
                }
            }

            throw new InvalidOperationException(
                $"Postgres externo não aceitou conexões em {timeout.TotalSeconds:0}s. Detalhe: {last?.Message}",
                last);
        }

        private static void EnsureTemplate()
        {
            if (_templateReady) return;

            using (var conn = new NpgsqlConnection(ConnFor(MaintenanceDb)))
            {
                conn.Open();
                Execute(conn, $"DROP DATABASE IF EXISTS \"{TemplateDb}\" WITH (FORCE);");
                Execute(conn, $"CREATE DATABASE \"{TemplateDb}\";");
            }

            var templateConn = ConnFor(TemplateDb);
            var tenantProvider = new NoopTenantProvider();
            var currentUser = new NoopCurrentUser();

            var optGestao = new DbContextOptionsBuilder<ContextGestaoClientes>()
                .UseNpgsql(templateConn)
                .ReplaceService<IMigrationsSqlGenerator, EprosMigrationsSqlGenerator>()
                .Options;
            using (var ctx = new ContextGestaoClientes(optGestao, tenantProvider, currentUser))
            {
                ctx.Database.Migrate();
            }

            var optApp = new DbContextOptionsBuilder<ContextAplicativo>()
                .UseNpgsql(templateConn)
                .ReplaceService<IMigrationsSqlGenerator, EprosMigrationsSqlGenerator>()
                .Options;
            using (var ctx = new ContextAplicativo(optApp, tenantProvider, currentUser))
            {
                ctx.Database.Migrate();
            }

            _templateReady = true;
        }

        public static DbContextOptions<TContext> BuildOptions<TContext>(string connectionString, ITenantProvider tenantProvider)
            where TContext : DbContext
        {
            return new DbContextOptionsBuilder<TContext>()
                .UseNpgsql(connectionString)
                .AddInterceptors(new TenantRlsInterceptor(tenantProvider))
                .ReplaceService<IMigrationsSqlGenerator, EprosMigrationsSqlGenerator>()
                .Options;
        }

        private static void Execute(NpgsqlConnection conn, string sql)
        {
            using var cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            cmd.ExecuteNonQuery();
        }

        private static string Sanitize(string logicalName)
        {
            var span = logicalName.ToLowerInvariant().ToCharArray();
            for (int i = 0; i < span.Length; i++)
            {
                var c = span[i];
                if (!(c >= 'a' && c <= 'z') && !(c >= '0' && c <= '9') && c != '_')
                {
                    span[i] = '_';
                }
            }
            var name = "t_" + new string(span);
            return name.Length > 60 ? name.Substring(0, 60) : name;
        }

        private sealed class NoopTenantProvider : ITenantProvider
        {
            public string GetTenantId() => "system";
        }

        private sealed class NoopCurrentUser : ICurrentUser
        {
            public string? GetUserId() => "system-test";
            public string? GetUserName() => "System Test";
            public string? GetUserEmail() => "system@epros.com";
        }
    }
}
