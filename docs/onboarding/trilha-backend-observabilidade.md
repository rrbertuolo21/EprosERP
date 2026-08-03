---
title: "Trilha Backend — observabilidade, eventos e CI"
confluence_id: "192282631"
confluence_url: "https://rafaelbertuolo.atlassian.net/wiki/spaces/EprosWeb/pages/192282631/Trilha+Backend+observabilidade+eventos+e+CI"
last_updated: "2026-07-06"
---

> [!IMPORTANT]
> **O que você vai aprender:** PostgreSQL e EF Core avançado, segurança multi-tenant, observabilidade e testes com Testcontainers — continuação da trilha backend.

Continuação da [Trilha Backend — CQRS, DDD e EF Core](trilha-backend-cqrs-ddd.md). Ao concluir estas etapas, você domina o ciclo completo: código → testes de segurança → observabilidade → CI.

---

## Etapa 3 — PostgreSQL e EF Core 8

### O que estudar

* [PostgreSQL 16 — banco de dados](backend/postgresql-16.md)
* [EF Core 8 + Npgsql — persistência](backend/ef-core-8-npgsql.md)
* Schemas por módulo — `financas.*`, `estoque.*`, `vendas.*`
* `IEntityTypeConfiguration` — snake_case, precision decimal, schema
* QueryFilter automático — ler `ContextBase`, entender reflection
* Migrations por módulo — criar, aplicar, reverter com segurança

### Padrões obrigatórios

```csharp
// Mapping exemplo
builder.ToTable("contas_pagar", "financas");
builder.Property(e => e.Valor).HasPrecision(18, 2);
builder.Property(e => e.FornecedorId).HasColumnName("fornecedor_id");
```

### Exercício prático

**Criar migration** para o agregado `FornecedorPrincipal` no schema `estoque.*`:

1. `FornecedorMapping.cs` com snake_case
2. `dotnet ef migrations add FornecedorInicial`
3. Revisar SQL gerado — conferir schema e tipos

### Critério de conclusão

- [ ] Migration aplica sem erro
- [ ] Colunas em snake_case
- [ ] `HasPrecision(18, 2)` em decimais

---

## Etapa 4 — Segurança e multi-tenancy

### O que estudar

* [Keycloak 24 — identidade e autenticação](backend/keycloak-24.md)
* [HashiCorp Vault 1.16 — gestão de segredos](backend/vault-1-16.md)
* [Valkey 7 — cache e locks distribuídos](backend/valkey-7.md)
* `TenantSaaSMiddleware` — como `tenantId` é resolvido
* `TenantLeakTest` — o teste mais importante
* `DataMaskingMiddleware` — atributo `[Sensitive]`

### Leitura obrigatória

* [Multi-tenancy e os 8 testes que o CI bloqueia](05-multi-tenancy-8-testes.md)
* `tests/.../ContasAPagarSecurityTests.cs`

### Exercício prático

**Escrever os 8 testes de segurança** para um módulo novo (sugestão: Fornecedor):

Copie a estrutura de `ContasAPagarSecurityTests` — todo módulo novo precisa dos 8 testes antes do primeiro merge.

1. Copiar estrutura de `ContasAPagarSecurityTests`
2. Adaptar para duas entidades do módulo
3. Rodar com Testcontainers localmente
4. Garantir CI verde no PR

### Critério de conclusão

- [ ] 8 testes passando
- [ ] TenantLeakTest com assertion clara
- [ ] PR com Code Review Agent no comentário

---

## Etapa 5 — Observabilidade e testes (opcional)

Se o time estiver no Bloco 6 (Compras), aprofunde nestes tópicos:

* [Serilog + OpenTelemetry + Grafana Stack — observabilidade](backend/serilog-opentelemetry-grafana.md)
* Serilog 3 — enrichers de `tenantId`, mascaramento PII
* OpenTelemetry — traces correlacionados com Grafana
* Testcontainers — ciclo de vida do container PostgreSQL
* Suite de integração para módulo Compras

### Exercício prático

**Criar suite de integração** para `Estoque/Compras`:

* Testcontainers com PostgreSQL 16
* Pelo menos 3 cenários de fluxo (criar, confirmar, cancelar)
* Integrar no GitHub Actions

---

## Mapa de competências

```
Etapa 1 → CQRS + Handlers
Etapa 2 → DDD + Domain Events
Etapa 3 → EF Core + Migrations
Etapa 4 → 8 testes de segurança
Etapa 5 → Observabilidade + CI
```

---

## Agentes IA para backend

| Momento | Agente |
| --- | --- |
| Coding | Dev Agent + Context Agent |
| Antes do PR | Code Review Agent (obrigatório) |
| Endpoint fiscal | Security Agent |
| Feature nova | Architect Agent |

---

**Trilha backend concluída.**

**Próximo passo →** [Tutorial — Dev Backend](backend/tutorial-dev-backend.md) — passo a passo hands-on de Dev, Code Review e Security no Cursor.

Consulte o [Índice do Onboarding](README.md) para revisar artigos base ou aprofundar em ADRs na [Trilha Tech Lead](trilha-tech-lead.md).
