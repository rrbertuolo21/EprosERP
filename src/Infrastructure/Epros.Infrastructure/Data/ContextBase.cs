using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Epros.Shared.Domain.Entities;
using Epros.Shared.Application.Contracts;

using Flunt.Notifications;

namespace Epros.Infrastructure.Data
{
    public abstract class ContextBase : DbContext
    {
        protected readonly ITenantProvider _tenantProvider;
        protected readonly ICurrentUser _currentUser;

        protected ContextBase(
            DbContextOptions options,
            ITenantProvider tenantProvider,
            ICurrentUser currentUser) : base(options)
        {
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Ignorar classe de notificações do Flunt no banco de dados
            modelBuilder.Ignore<Notification>();

            // 1. Aplicar filtro automático de Tenant e Soft Delete para todas as entidades SaaS
            var aplicarFiltroMetodo = typeof(ContextBase)
                .GetMethod(nameof(AplicarFiltroTenantESoftDelete), BindingFlags.NonPublic | BindingFlags.Static);

            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                if (entityType.ClrType.IsAssignableTo(typeof(EntidadeSaaSBase)))
                {
                    aplicarFiltroMetodo!
                        .MakeGenericMethod(entityType.ClrType)
                        .Invoke(null, new object[] { modelBuilder, this });
                }
            }

            // 2. Aplicar regras globais e convenções (snake_case, decimal precision, indexes)
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                // Nome de tabela em snake_case
                var tableName = entityType.GetTableName();
                var schema = entityType.GetSchema();
                if (!string.IsNullOrEmpty(tableName))
                {
                    entityType.SetTableName(ToSnakeCase(tableName));
                    if (!string.IsNullOrEmpty(schema))
                    {
                        entityType.SetSchema(schema);
                    }
                }

                // Configurações por propriedade
                foreach (var property in entityType.GetProperties())
                {
                    var ownership = entityType.FindOwnership();
                    if (ownership != null)
                    {
                        var propertyIndex = ownership.Properties.ToList().IndexOf(property);
                        if (propertyIndex != -1)
                        {
                            var principalProperty = ownership.PrincipalKey.Properties[propertyIndex];
                            property.SetColumnName(ToSnakeCase(principalProperty.Name));
                        }
                        else
                        {
                            property.SetColumnName(ToSnakeCase(property.Name));
                        }
                    }
                    else
                    {
                        property.SetColumnName(ToSnakeCase(property.Name));
                    }

                    // Precisão decimal 18,2 para todo decimal
                    if (property.ClrType == typeof(decimal) || property.ClrType == typeof(decimal?))
                    {
                        property.SetPrecision(18);
                        property.SetScale(2);
                    }
                }

                // Convenções de nomes para chaves e índices
                foreach (var key in entityType.GetKeys())
                {
                    key.SetName(ToSnakeCase(key.GetName() ?? ""));
                }
                foreach (var key in entityType.GetForeignKeys())
                {
                    key.SetConstraintName(ToSnakeCase(key.GetConstraintName() ?? ""));
                }
                foreach (var index in entityType.GetIndexes())
                {
                    index.SetDatabaseName(ToSnakeCase(index.GetDatabaseName() ?? ""));
                }

                // Índice Único composto e individual automático para SaaS
                if (entityType.ClrType.IsAssignableTo(typeof(EntidadeSaaSBase)))
                {
                    // Índice único em sync_id
                    modelBuilder.Entity(entityType.ClrType)
                        .HasIndex(nameof(EntidadeSaaSBase.SyncId))
                        .IsUnique()
                        .HasDatabaseName(ToSnakeCase($"ix_{entityType.ClrType.Name}_sync_id"));

                    // Índice composto (tenant_id, id) ou (tenant_id, outro campo) pode ser definido em mappings específicos,
                    // mas garantiremos um índice no tenant_id para otimização de consultas
                    modelBuilder.Entity(entityType.ClrType)
                        .HasIndex(nameof(EntidadeSaaSBase.TenantId))
                        .HasDatabaseName(ToSnakeCase($"ix_{entityType.ClrType.Name}_tenant_id"));

                    // REG-017 — LOCK OTIMISTA global: usa a coluna de sistema xmin do PostgreSQL como
                    // concurrency token (aditivo, sem migration/coluna nova). Todo UPDATE/DELETE passa a
                    // carregar "WHERE ... AND xmin = @original"; se a linha mudou entre a leitura e o save,
                    // o EF lança DbUpdateConcurrencyException (mapeada a 409 no middleware global).
                    // Aplicado SÓ no provider Npgsql: o xmin não existe no InMemory (usado por parte da
                    // suíte), e como é ValueGeneratedOnAddOrUpdate o Postgres o preenche sozinho — inserts
                    // com seeds não precisam setar versão (não há "match em insert").
                    if (Database.IsNpgsql())
                    {
                        // Npgsql 7+: uint + IsRowVersion() mapeia automaticamente para a coluna de sistema xmin.
                        modelBuilder.Entity(entityType.ClrType)
                            .Property<uint>("xmin")
                            .IsRowVersion();
                    }
                }
            }
        }

        private static void AplicarFiltroTenantESoftDelete<TEntity>(ModelBuilder modelBuilder, ContextBase context)
            where TEntity : EntidadeSaaSBase
        {
            if (typeof(IGlobalEntity).IsAssignableFrom(typeof(TEntity)))
            {
                modelBuilder.Entity<TEntity>().HasQueryFilter(e => e.DeletadoEm == null);
            }
            else
            {
                modelBuilder.Entity<TEntity>().HasQueryFilter(e =>
                    e.TenantId == context._tenantProvider.GetTenantId() && e.DeletadoEm == null);
            }
        }

        public override int SaveChanges()
        {
            ProcessarEntidadesSaaS();
            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            ProcessarEntidadesSaaS();
            return base.SaveChangesAsync(cancellationToken);
        }

        private void ProcessarEntidadesSaaS()
        {
            var userId = _currentUser.GetUserId() ?? "system";
            var tenantId = _tenantProvider.GetTenantId();

            if (_tenantProvider.EhTenantDemo())
            {
                foreach (var entry in ChangeTracker.Entries<EntidadeSaaSBase>())
                {
                    if (entry.State == EntityState.Added ||
                        entry.State == EntityState.Modified ||
                        entry.State == EntityState.Deleted)
                    {
                        var entityType = entry.Entity.GetType();
                        if (entityType.Name != "SessaoUsuario" &&
                            entityType.Name != "SessaoImpersonacao" &&
                            entityType.Name != "HistoricoLogin")
                        {
                            throw new Epros.Shared.Domain.Exceptions.OperacaoBloqueadaModoDemoException();
                        }
                    }
                }
            }

            foreach (var entry in ChangeTracker.Entries<EntidadeSaaSBase>())
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        // Garante tenant_id e criado_por
                        if (entry.Entity is IGlobalEntity)
                        {
                            entry.Property(e => e.TenantId).CurrentValue = "system";
                        }
                        else if (tenantId != "system")
                        {
                            entry.Property(e => e.TenantId).CurrentValue = tenantId;
                        }
                        else if (string.IsNullOrEmpty(entry.Entity.TenantId))
                        {
                            entry.Property(e => e.TenantId).CurrentValue = "system";
                        }
                        entry.Property(e => e.CriadoPor).CurrentValue = userId;
                        break;

                    case EntityState.Modified:
                        entry.Entity.MarcarAlterado(userId);
                        break;

                    case EntityState.Deleted:
                        if (entry.Entity is IHardDeletable)
                        {
                            break;
                        }
                        // Cancela delete físico e aplica soft delete
                        entry.State = EntityState.Modified;
                        entry.Entity.Deletar(userId);
                        break;
                }
            }
        }

        private static string ToSnakeCase(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;
            var startUnderscore = input.StartsWith("_");
            return (startUnderscore ? "_" : "") +
                   string.Concat(input.Select((x, i) => i > 0 && char.IsUpper(x) ? "_" + x.ToString() : x.ToString()))
                         .ToLower();
        }
    }
}
