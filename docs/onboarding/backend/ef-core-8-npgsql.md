---
title: "EF Core 8 + Npgsql — persistência"
confluence_id: "193691660"
confluence_url: "https://rafaelbertuolo.atlassian.net/wiki/spaces/EprosWeb/pages/193691660/EF+Core+8+Npgsql+persist+ncia"
last_updated: "2026-07-06"
---

**Versão fixada:** `EF Core 8.0.x` + `Npgsql 8.0.x`
**Licença:** MIT / Apache 2.0

### O que é

EF Core é o ORM do .NET — mapeia classes C# para tabelas. Npgsql é o driver PostgreSQL. Juntos, são a camada de persistência de todo o Epros.

### Por que foi escolhido vs Dapper (ADR-001)

Dapper permite SQL livre — o dev escreve o WHERE. Isso significa que o dev pode esquecer de filtrar por tenant. Com EF Core, o `QueryFilter` no `ContextBase` aplica o filtro automaticamente por reflection em **todas** as entidades, sem possibilidade de esquecer. Este foi o bug #2 do legado (TenantIdStatic) — resolvido por arquitetura.

### EntidadeSaaSBase — a base de tudo

```csharp
// Shared/DomainObjects/EntidadeSaaSBase.cs
public abstract class EntidadeSaaSBase : Notifiable<Notification>, ISyncable
{
    // PK gerada localmente — offline-safe, sem precisar do banco
    public Guid Id { get; private set; } = Guid.NewGuid();

    // Chave estável para sync entre dispositivos — nunca muda
    public Guid SyncId { get; private set; } = Guid.NewGuid();

    // Isolamento multi-tenant — aplicado automaticamente pelo ContextBase
    public string TenantId { get; private set; } = string.Empty;

    // Para conflito offline: dois dispositivos editam o mesmo registro → SyncVersion decide
    public int SyncVersion { get; private set; } = 1;

    // Auditoria de tempo — SEMPRE UtcNow, nunca DateTime.Now
    public DateTime CriadoEm { get; private set; } = DateTime.UtcNow;
    public DateTime? AlteradoEm { get; private set; }
    public DateTime? DeletadoEm { get; private set; } // null = ativo; preenchido = soft deleted

    // Auditoria de quem — userId do Keycloak
    public string? CriadoPor { get; private set; }
    public string? AlteradoPor { get; private set; }

    // Constructor protegido para garantir que TenantId sempre é setado
    protected EntidadeSaaSBase(string tenantId, string criadoPor)
    {
        TenantId = tenantId;
        CriadoPor = criadoPor;
    }

    // Métodos de ciclo de vida — nunca manipular campos diretamente
    public void MarcarAlterado(string usuarioId)
    {
        AlteradoEm = DateTime.UtcNow;
        AlteradoPor = usuarioId;
        SyncVersion++;
    }

    public void Deletar(string usuarioId)
    {
        DeletadoEm = DateTime.UtcNow;
        AlteradoPor = usuarioId;
    }

    public bool EstaAtivo() => DeletadoEm == null;

    // Flunt: IsValid = true se não há notificações de erro
    public bool IsValid => !Notifications.Any();

    // Domain Events — registrados na entidade, publicados pelo UnitOfWork
    private readonly List<DomainEvent> _eventos = [];
    public IReadOnlyCollection<DomainEvent> Eventos => _eventos.AsReadOnly();

    protected void AdicionarEvento(DomainEvent evento) => _eventos.Add(evento);
    public void LimparEventos() => _eventos.Clear();
}
```

### ContextBase — QueryFilter automático

```csharp
// Shared/Data/ContextBase.cs
public abstract class ContextBase : DbContext
{
    private readonly ITenantProvider _tenantProvider;

    protected ContextBase(
        DbContextOptions options,
        ITenantProvider tenantProvider) : base(options)
    {
        _tenantProvider = tenantProvider;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Aplica snake_case em todas as colunas (convenção PostgreSQL)
        modelBuilder.UseSnakeCaseNamingConvention();

        // QueryFilter automático por reflection
        // Nenhum dev precisa lembrar — é impossível esquecer
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (!typeof(EntidadeSaaSBase).IsAssignableFrom(entityType.ClrType))
                continue;

            var tenantId = _tenantProvider.GetTenantId();

            // Toda query desta entidade automaticamente terá:
            // WHERE tenant_id = '{tenantId}' AND deletado_em IS NULL
            modelBuilder.Entity(entityType.ClrType)
                .HasQueryFilter(BuildQueryFilter(entityType.ClrType, tenantId));
        }
    }
}
```

### Mapeamento de entidade com schema próprio

```csharp
// Modules/Financeiro/ContasPagar/Infrastructure/Data/Mappings/ContaPagarMapping.cs
public class ContaPagarMapping : IEntityTypeConfiguration<ContaPagar>
{
    public void Configure(EntityTypeBuilder<ContaPagar> builder)
    {
        // Schema separado por macrodomínio — fronteira física entre módulos
        builder.ToTable("conta_pagar", schema: "financas");

        builder.HasKey(x => x.Id);

        // Decimal com precisão explícita — nunca perde centavo
        builder.Property(x => x.Valor)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(x => x.DataVencimento)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasConversion<string>() // enum como string legível no banco
            .IsRequired();

        // Índice composto tenant + campo de negócio — todas as buscas usam isso
        builder.HasIndex(x => new { x.TenantId, x.DataVencimento })
            .HasDatabaseName("idx_conta_pagar_tenant_vencimento");

        // Índice único para sync offline
        builder.HasIndex(x => x.SyncId)
            .IsUnique()
            .HasDatabaseName("idx_conta_pagar_sync_id");
    }
}
```

### Armadilhas

* **Nunca** `context.Remove()` — soft delete via `entidade.Deletar(usuarioId)`
* **Nunca DbContext de outro módulo** — cada módulo tem seu próprio context
* `IgnoreQueryFilters()` **é banido** — único uso permitido é em contexto admin com role específico e log de auditoria
* **Decimais sempre com** `Precision(18,2)` — sem isso o EF Core usa `float`, que tem erro de arredondamento

### Onde aprender

* EF Core: [https://learn.microsoft.com/ef/core/](https://learn.microsoft.com/ef/core/)
* Npgsql + EF: [https://www.npgsql.org/efcore/](https://www.npgsql.org/efcore/)
* PostgreSQL 16: [https://www.postgresql.org/docs/16/](https://www.postgresql.org/docs/16/)
