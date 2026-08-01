# Molde de Porte — Legado Epros → EprosERP

> **Este arquivo é só o MOLDE campo a campo.** As convenções gerais estão em **[`CONVENCAO_CODIGO.md`](../../CONVENCAO_CODIGO.md)** (fonte canônica na raiz). Em qualquer divergência, o CONVENCAO prevalece.

Regra mestra: **porte fiel, campo a campo. Não inventar, não simplificar, não remover.** Todo campo/entidade do legado precisa ter destino no novo. Só se traduz de plataforma (SQL Server/long/Entity/Controller) para a plataforma nova (PostgreSQL/Guid/EntidadeSaaSBase/CQRS). Tradução de plataforma resumida: `long Id`+`SequenciaTenantId` → `EntidadeSaaSBase` (+ `SequenciaExibicao long?` quando a UX legada exibia a sequência); FK `long` → `Guid`; Controller → CQRS; enum legado → enum portado (nunca string/int); `DateTime.Now` → base UTC. Detalhes em CONVENCAO §5.4.

## 2. Namespaces/base a usar (não recriar)
- Entidade base: `Epros.Shared.Domain.Entities.EntidadeSaaSBase` — ctor `(string tenantId, string criadoPor)`; métodos `MarcarAlterado(alteradoPor)`, `Deletar(userId)`, `IsValid`, `Notifications`, `Clear()`, `AddNotifications(...)`.
- Enums: `Epros.Shared.Domain.Enums`
- Resultado de comando: `Epros.Shared.Application.Models.CommandResult` → `CommandResult.Ok(msg, data?)`, `CommandResult.Falha(msg|IEnumerable<string>)`
- Contratos DI: `Epros.Shared.Application.Contracts` → `ICurrentUser.GetUserId()`, `ITenantProvider.GetTenantId()`
- DbContext base: `Epros.Infrastructure.Data.ContextBase` (aplica snake_case, filtro de tenant e soft-delete por reflection)
- Outbox: `Epros.Shared.Domain.Events.OutboxMessage`
- CQRS: `MediatR` (`IRequest<CommandResult>`, `IRequestHandler<,>`) — handlers são auto-registrados por assembly scan; **não** editar Program.cs.

## 3. Molde de ENTIDADE
```csharp
public class ContaBancaria : EntidadeSaaSBase
{
    public Guid BancoId { get; private set; }
    public string Apelido { get; private set; } = string.Empty;
    public DateTime? DataEncerramento { get; private set; }
    public Banco Banco { get; private set; } = null!; // navegação intra-módulo

    protected ContaBancaria() { } // EF Core

    public ContaBancaria(Guid bancoId, string apelido, DateTime? dataEncerramento,
                         string tenantId, string criadoPor) : base(tenantId, criadoPor)
    {
        BancoId = bancoId; Apelido = apelido; DataEncerramento = dataEncerramento;
        Validar();
    }

    public void Alterar(Guid bancoId, string apelido, DateTime? dataEncerramento, string alteradoPor)
    {
        BancoId = bancoId; Apelido = apelido; DataEncerramento = dataEncerramento;
        MarcarAlterado(alteradoPor); Validar();
    }

    public void Validar()
    {
        Clear();
        AddNotifications(new Contract<ContaBancaria>().Requires()
            .AreNotEquals(BancoId, Guid.Empty, nameof(BancoId), "O banco é obrigatório")
            .IsNotNullOrEmpty(Apelido, nameof(Apelido), "O apelido é obrigatório"));
    }
}
```
- Propriedades com `private set`. Ctor protegido p/ EF. Ctor público com todos os campos + `tenantId` + `criadoPor`. Método `Alterar(...)` + `Validar()`. Mesmas validações do legado.

## 4. Molde de MAPPING (dentro de `Context<Modulo>.OnModelCreating`)
```csharp
modelBuilder.HasDefaultSchema("<schema-do-modulo>"); // financas, vendas, estoque, plataforma...
modelBuilder.Entity<ContaBancaria>(e =>
{
    e.HasKey(x => x.Id);
    e.Property(x => x.Apelido).HasMaxLength(150);
    e.HasOne(x => x.Banco).WithMany().HasForeignKey(x => x.BancoId).OnDelete(DeleteBehavior.Restrict);
    e.HasIndex(x => new { x.TenantId, x.Conta });        // índice composto (tenant + negócio)
});
// ...para entidades transacionais: e.HasIndex(x => x.SyncId).IsUnique();
// decimal: e.Property(x => x.Valor).HasPrecision(18, 2);
base.OnModelCreating(modelBuilder); // por último
```
Registrar `DbSet<T>` no `Context<Modulo>`.

## 5. Molde de COMMAND + HANDLER
```csharp
public record CriarContaBancariaCommand(Guid BancoId, string Apelido, DateTime? DataEncerramento)
    : IRequest<CommandResult>;

public class CriarContaBancariaCommandHandler : IRequestHandler<CriarContaBancariaCommand, CommandResult>
{
    private readonly ContextFinanceiro _context;
    private readonly ITenantProvider _tenantProvider;
    private readonly ICurrentUser _currentUser;
    public CriarContaBancariaCommandHandler(ContextFinanceiro c, ITenantProvider t, ICurrentUser u)
    { _context = c; _tenantProvider = t; _currentUser = u; }

    public async Task<CommandResult> Handle(CriarContaBancariaCommand r, CancellationToken ct)
    {
        var tenantId = _tenantProvider.GetTenantId();
        var userId = _currentUser.GetUserId() ?? "system";
        var conta = new ContaBancaria(r.BancoId, r.Apelido, r.DataEncerramento, tenantId, userId);
        if (!conta.IsValid) return CommandResult.Falha(conta.Notifications.Select(n => n.Message));
        _context.ContasBancarias.Add(conta);
        await _context.SaveChangesAsync(ct);
        return CommandResult.Ok("Conta bancária cadastrada com sucesso.", new { conta.Id });
    }
}
```
- Sempre `Criar`, `Atualizar`, `Deletar` (soft delete via `entidade.Deletar(userId)`) por agregado, além das operações de negócio específicas do controller legado.

## 6. Regra de dependência entre módulos
- Referenciar entidades de outro módulo **por Guid FK**, nunca por navegação/projeto cruzado.
- Para leitura cruzada, usar entidade *Lookup* mapeada `ToTable("<tabela>", "<schema-do-outro-modulo>")` (ver `PessoaLookup` no Financeiro). Não criar referência de projeto entre módulos.

## 7. Layout de arquivos por módulo
```
Domain/Entities/<Entidade>.cs
Application/Commands/<Agregado>Commands.cs
Application/Handlers/<Agregado>Handlers.cs
Application/Queries/<Agregado>Queries.cs
Infrastructure/Data/Context<Modulo>.cs   (adicionar DbSet + mapping)
```

## 8. Mapa de domínios legado → módulo novo
| Legado (`Epros.ERP.Domain/Entities`) | Módulo destino | Schema |
|---|---|---|
| Cadastros/Pessoas, Empresas, Enderecos | GestaoClientes | plataforma |
| Cadastros/Produtos, Estoque, Compras | Estoque | estoque |
| Cadastros/Bancos, Financeiros, Importacoes(OFX) | Financeiro | financas |
| Vendas | Vendas (+ doc fiscal em Fiscal) | vendas |
| Fiscais, Tributarios, Configuracoes, Servicos | Fiscal | **plataforma** (ContextFiscal usa `plataforma`, NÃO `fiscal`) |
| Permissoes, Usuarios | Aplicativo | aplicativo |
| DfeCalculos, Dfe.API (motor de cálculo + SEFAZ) | Fiscal (envelopado via IHerculesFiscalService) | plataforma |

**Exceção:** o motor de cálculo tributário e a comunicação SEFAZ (`Epros.ERP.DfeCalculos` + `Epros.ERP.Dfe.API`) NÃO são reescritos — são reaproveitados via adaptador. Não portar essas classes agora.
