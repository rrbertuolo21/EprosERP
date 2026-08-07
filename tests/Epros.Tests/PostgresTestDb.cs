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
    /// Provisiona um banco PostgreSQL real e isolado por teste via Testcontainers.
    /// Os fluxos de autenticação/RLS usam métodos relacionais (ExecuteSqlRaw, transações,
    /// current_setting) que o provider InMemory não suporta; por isso estes testes precisam
    /// de um Postgres de verdade.
    ///
    /// Estratégia: um container Postgres sobe uma vez por processo de teste; um "template" é
    /// migrado uma única vez (schemas/tabelas + políticas RLS); cada teste recebe um database
    /// próprio criado por cópia do template (CREATE DATABASE ... TEMPLATE), garantindo
    /// isolamento total sem re-rodar as migrations a cada teste.
    /// </summary>
    public static class PostgresTestDb
    {
        private const string MaintenanceDb = "epros";
        private const string TemplateDb = "epros_rlstpl";
        private static readonly TimeSpan ContainerStartTimeout = TimeSpan.FromMinutes(3);

        // Init via Task compartilhada (sem lock durante await): evita starvation/deadlock
        // quando N testes paralelos fazem sync-over-async em lock + StartAsync.GetResult().
        private static readonly object _initGate = new object();
        private static readonly object _createGate = new object();
        private static Task? _initTask;
        private static PostgreSqlContainer? _container;
        private static bool _templateReady;

        /// <summary>
        /// Dispara o start do container o mais cedo possível (carga do assembly),
        /// para o init ocorrer em paralelo com testes InMemory.
        /// </summary>
        [ModuleInitializer]
        internal static void KickoffWarmup()
        {
            try
            {
                _ = GetOrStartInitTask();
            }
            catch
            {
                // Warmup best-effort; CreateDatabase propaga o erro real.
            }
        }

        private static string ConnFor(string database)
        {
            if (_container is null)
            {
                throw new InvalidOperationException("Container Postgres de teste ainda não foi iniciado.");
            }

            var builder = new NpgsqlConnectionStringBuilder(_container.GetConnectionString())
            {
                Database = database,
                Pooling = false
            };
            return builder.ConnectionString;
        }

        /// <summary>
        /// Cria (recriando se já existir) um database isolado para o teste e devolve a connection string.
        /// A criação é serializada para evitar conflitos de acesso ao template entre classes paralelas.
        /// </summary>
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

        /// <summary>Bloqueia até container + template estarem prontos (útil em testes de sanidade).</summary>
        public static void Warmup() => EnsureReady();

        private static void EnsureReady()
        {
            GetOrStartInitTask().ConfigureAwait(false).GetAwaiter().GetResult();
        }

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
            // Nenhum lock detido durante os awaits — libera o thread pool para o Testcontainers.
            Console.WriteLine($"[PostgresTestDb] Iniciando postgres:16-alpine (timeout {ContainerStartTimeout.TotalMinutes:0} min)...");
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
                Console.WriteLine("[PostgresTestDb] Container Postgres pronto.");
                Console.Out.Flush();
            }
            catch (Exception ex)
            {
                try { await container.DisposeAsync().ConfigureAwait(false); } catch { /* ignore */ }
                throw new InvalidOperationException(
                    "Não foi possível iniciar o PostgreSQL via Testcontainers em " +
                    $"{ContainerStartTimeout.TotalMinutes:0} min. " +
                    "Verifique se o Docker está em execução. Detalhe: " + ex.Message,
                    ex);
            }

            Console.WriteLine("[PostgresTestDb] Aplicando migrations no template...");
            Console.Out.Flush();
            await Task.Run(EnsureTemplate).ConfigureAwait(false);
            Console.WriteLine("[PostgresTestDb] Template RLS pronto.");
            Console.Out.Flush();
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

            // GestaoClientes primeiro (schema "plataforma" com tenants/empresas), depois Aplicativo
            // (schema "aplicativo" com identity), respeitando a ordem de dependência.
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

        /// <summary>
        /// Constrói as options de um DbContext apontando para o banco de teste, com o interceptor de RLS
        /// e o gerador de SQL customizado — idêntico ao registro de produção em Program.cs.
        /// </summary>
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
