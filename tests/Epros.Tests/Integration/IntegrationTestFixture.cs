using System;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.GestaoClientes.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Epros.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Testcontainers.PostgreSql;
using Npgsql;
using Xunit;

namespace Epros.Tests.Integration
{
    public class IntegrationTestFixture : IAsyncLifetime
    {
        private static readonly TimeSpan ContainerStartTimeout = TimeSpan.FromMinutes(3);

        private PostgreSqlContainer? _postgreSqlContainer;
        public bool DockerDisponivel { get; private set; }
        public string ConnectionString { get; private set; } = string.Empty;
        public string AppConnectionString { get; private set; } = string.Empty;

        public async Task InitializeAsync()
        {
            try
            {
                _postgreSqlContainer = new PostgreSqlBuilder()
                    .WithImage("postgres:16-alpine")
                    .WithDatabase("epros_test")
                    .WithUsername("epros")
                    .WithPassword("epros_test_password")
                    .Build();

                Console.WriteLine($"[IntegrationTestFixture] Iniciando postgres:16-alpine (timeout {ContainerStartTimeout.TotalMinutes:0} min)...");
                using var cts = new CancellationTokenSource(ContainerStartTimeout);
                await _postgreSqlContainer.StartAsync(cts.Token);
                Console.WriteLine("[IntegrationTestFixture] Container Postgres pronto.");
                ConnectionString = _postgreSqlContainer.GetConnectionString();
                DockerDisponivel = true;

                // Cria o usuário da aplicação que não é superuser para testar RLS
                using (var connection = new NpgsqlConnection(ConnectionString))
                {
                    await connection.OpenAsync();
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = @"
                            CREATE ROLE epros_app_user WITH LOGIN PASSWORD 'epros_app_password';
                            CREATE SCHEMA IF NOT EXISTS plataforma;
                            CREATE SCHEMA IF NOT EXISTS aplicativo;
                            GRANT USAGE, CREATE ON SCHEMA plataforma TO epros_app_user;
                            GRANT USAGE, CREATE ON SCHEMA aplicativo TO epros_app_user;
                            ALTER DEFAULT PRIVILEGES IN SCHEMA plataforma GRANT ALL ON TABLES TO epros_app_user;
                            ALTER DEFAULT PRIVILEGES IN SCHEMA plataforma GRANT ALL ON SEQUENCES TO epros_app_user;
                            ALTER DEFAULT PRIVILEGES IN SCHEMA aplicativo GRANT ALL ON TABLES TO epros_app_user;
                            ALTER DEFAULT PRIVILEGES IN SCHEMA aplicativo GRANT ALL ON SEQUENCES TO epros_app_user;
                        ";
                        await command.ExecuteNonQueryAsync();
                    }
                }

                var connBuilder = new NpgsqlConnectionStringBuilder(ConnectionString)
                {
                    Username = "epros_app_user",
                    Password = "epros_app_password"
                };
                AppConnectionString = connBuilder.ConnectionString;
            }
            catch (Exception ex)
            {
                // Docker não está ativo/disponível no host local
                Console.WriteLine($"[AVISO] Docker está offline ou indisponível. Testes com Testcontainers serão pulados. Erro: {ex.Message}");
                DockerDisponivel = false;
            }
        }

        public async Task DisposeAsync()
        {
            if (_postgreSqlContainer != null)
            {
                await _postgreSqlContainer.DisposeAsync();
            }
        }

        public ContextGestaoClientes CreateDbContext(string tenantId, string userId)
        {
            return CreateDbContext(tenantId, userId, isAdmin: false);
        }

        public ContextGestaoClientes CreateDbContext(string tenantId, string userId, bool isAdmin)
        {
            if (!DockerDisponivel)
            {
                throw new InvalidOperationException("Docker não está disponível no ambiente.");
            }

            var tenantProvider = new TestTenantProvider(tenantId);
            var currentUser = new TestCurrentUser(userId);
            var interceptor = new TenantRlsInterceptor(tenantProvider);

            var connectionString = isAdmin ? ConnectionString : AppConnectionString;

            var options = new DbContextOptionsBuilder<ContextGestaoClientes>()
                .UseNpgsql(connectionString)
                .AddInterceptors(interceptor)
                .ReplaceService<IMigrationsSqlGenerator, EprosMigrationsSqlGenerator>()
                .Options;

            var context = new ContextGestaoClientes(options, tenantProvider, currentUser);
            return context;
        }
    }

    public class TestTenantProvider : ITenantProvider
    {
        private readonly string _tenantId;
        public TestTenantProvider(string tenantId) => _tenantId = tenantId;
        public string GetTenantId() => _tenantId;
    }

    public class TestCurrentUser : ICurrentUser
    {
        private readonly string _userId;
        public TestCurrentUser(string userId) => _userId = userId;
        public string? GetUserId() => _userId;
        public string? GetUserName() => "Test User";
        public string? GetUserEmail() => "test@epros.com";
    }
}
