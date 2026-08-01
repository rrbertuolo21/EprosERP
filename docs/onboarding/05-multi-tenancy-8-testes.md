---
title: "Multi-tenancy e os 8 testes que o CI bloqueia"
confluence_id: "193429505"
confluence_url: "https://rafaelbertuolo.atlassian.net/wiki/spaces/EprosWeb/pages/193429505/Multi-tenancy+e+os+8+testes+que+o+CI+bloqueia"
last_updated: "2026-07-06"
---

> [!NOTE]
> **O que você vai aprender:** como o Epros garante isolamento entre tenants por design, quais são os 8 testes obrigatórios e o que acontece quando um deles falha no CI.

Um PR que vaza dados de outro tenant **nunca chega ao merge** — por design e por teste.

Multi-tenancy no Epros não é um filtro `WHERE tenant_id = @x` espalhado pelo código. É uma camada de infraestrutura que o desenvolvedor não precisa lembrar de aplicar — e que o CI verifica automaticamente.

---

## Os três pilares do isolamento

### 1. EntidadeSaaSBase

Toda entidade de domínio herda campos comuns:

* `TenantId` — identificador do cliente
* `CriadoEm`, `CriadoPor`, `AlteradoEm`, `AlteradoPor`
* `DeletadoEm` — soft delete
* `SyncVersion` — controle de sincronização offline

### 2. QueryFilter automático (EF Core)

O `ContextBase` aplica filtro por reflection em todas as entidades que herdam `EntidadeSaaSBase`:

```csharp
// Conceito — implementação em ContextBase
modelBuilder.Entity<T>()
    .HasQueryFilter(e => e.TenantId == _tenantProvider.GetTenantId()
                      && e.DeletadoEm == null);
```

Toda query já filtra tenant e soft delete. Esquecer o filtro manualmente é impossível — a menos que alguém use `IgnoreQueryFilters()` sem justificativa documentada.

> [!WARNING]
> `IgnoreQueryFilters()` desliga o isolamento automático. Só use com ADR e revisão do Tech Lead.

### 3. TenantSaaSMiddleware

Resolve o `tenantId` do claim JWT, valida no Catalog DB (cache Valkey 5 min) e enriquece o contexto de log. Request sem tenant válido → 403.

---

## Os 8 testes obrigatórios

Rode com Testcontainers — PostgreSQL **real**, não mock. Se um destes falhar, o CI bloqueia o merge.

| # | Teste | O que protege |
| --- | --- | --- |
| 1 | **TenantLeakTest** | Tenant A não vê dados do Tenant B |
| 2 | **SoftDeleteFilterTest** | Entidade deletada não aparece em queries |
| 3 | **LedgerAppendOnlyTest** | Lançamentos contábeis são imutáveis (trigger PG) |
| 4 | **AuditTrailTest** | Ações sensíveis geram registro em audit_trail |
| 5 | **OutboxDeliveryTest** | Domain Events são entregues via Outbox |
| 6 | **PCIDataMaskingTest** | PAN/CPF/CNPJ mascarados nos logs |
| 7 | **EntitlementGateTest** | Tenant sem módulo ativo recebe 403 |
| 8 | **PerformanceSLOTest** | P95 leitura <200ms, escrita <500ms |

---

## TenantLeakTest — o mais crítico

```csharp
[Fact]
public async Task TenantLeak_TenantA_NaoVeDadosDeTenantB()
{
    // Seed: conta criada no tenant B
    var cpTenantB = ContaPagar.Criar(
        "tenant-empresa-b", "sistema",
        Guid.NewGuid(), "Conta do Tenant B",
        100.00m, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30)));
    _ctxTenantB.ContasAPagar.Add(cpTenantB);
    await _ctxTenantB.SaveChangesAsync();

    // Tenant A lista suas contas
    var contasTenantA = await _ctxTenantA.ContasAPagar.ToListAsync();

    // Deve estar vazio — QueryFilter garante isolamento
    contasTenantA.Should().BeEmpty(
        "Tenant A não deve ver dados do Tenant B.");
}
```

> [!IMPORTANT]
> Todo módulo novo precisa dos 8 testes antes do primeiro merge em produção.

---

## SoftDeleteFilterTest

```csharp
cp.Deletar("usuario-teste");
await _ctxTenantA.SaveChangesAsync();

var encontrada = await _ctxTenantA.ContasAPagar
    .FirstOrDefaultAsync(c => c.Id == cp.Id);

encontrada.Should().BeNull(
    "Entidade com soft delete não deve aparecer em queries normais.");
```

> [!WARNING]
> Nunca use `context.Remove()`. Sempre `entidade.Deletar(userId)`.

---

## O que o CI faz quando um teste falha

```
1. Build compila
2. Testes unitários rodam
3. Testes de integração sobem PostgreSQL via Testcontainers
4. 8 testes de segurança executam
5. Se qualquer um falhar → PR bloqueado, sem override
```

O DoD do sprint exige CI verde incluindo os 8 testes. Tech Lead não aprova PR com falha de segurança.

> [!WARNING]
> Falha em qualquer um dos 8 testes bloqueia o merge — sem override manual.

---

## Checklist de segurança por PR

- [ ] Entidade herda EntidadeSaaSBase
- [ ] ITenantProvider scoped via DI
- [ ] Sem IgnoreQueryFilters() sem ADR
- [ ] Sem secrets no diff
- [ ] DataMasking em campos [Sensitive]
- [ ] TenantLeakTest passa para o módulo alterado
- [ ] Code Review Agent executado (próximo artigo)

---

## Para QA e frontend

> [!TIP]
> **QA:** reproduza os 8 testes em todo módulo novo — use `Financeiro` / `ContasAPagar` como template.

> [!TIP]
> **Frontend:** garanta que `tenantId` vai em todo request via interceptor; nunca hardcode tenant em dev.

---

**Próximo passo →** [16 agentes no Cursor: IA como copiloto, não como atalho](06-16-agentes-cursor.md)
