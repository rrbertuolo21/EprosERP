---
title: "Trilha Backend — CQRS, DDD e EF Core"
confluence_id: "193363991"
confluence_url: "https://rafaelbertuolo.atlassian.net/wiki/spaces/EprosWeb/pages/193363991/Trilha+Backend+CQRS+DDD+e+EF+Core"
last_updated: "2026-07-06"
---

> [!NOTE]
> **O que você vai aprender:** fundação .NET 8, CQRS com MediatR e DDD na prática Epros — primeiras etapas da trilha backend.

Esta trilha complementa os artigos base, em paralelo com o trabalho. Exercícios usam o código real de `epros-back`. Avance no seu ritmo; use os critérios de conclusão de cada etapa como referência.

> [!TIP]
> Pode iniciar esta trilha **em paralelo** com os artigos 04 e 05 da trilha base. A etapa de CQRS alinha com o conteúdo de código; a etapa de DDD aprofunda domínio antes dos testes de segurança.

---

## Etapa 1 — Fundação .NET 8 e CQRS

### O que estudar

* [ASP.NET Core 8 — o runtime](backend/asp-net-core-8.md) — pipeline de middlewares, quando usar Minimal APIs vs Controllers
* [MediatR 12 — CQRS](backend/mediatr-12.md) — `IRequest`, `IRequestHandler`, Behaviors (logging → validation → handler)
* [Flunt + FluentValidation — validação em duas camadas](backend/flunt-fluentvalidation.md) — validação de Commands antes do handler executar

### Leitura obrigatória

* [Do Command ao PR: implementando Contas a Pagar do zero](04-do-command-ao-pr.md)
* `src/Modules/Financeiro/ContasAPagar/Application/` no repositório

### Exercício prático

**Criar handler completo para** `CriarFornecedor` com:

1. `CriarFornecedorCommand` (record)
2. `CriarFornecedorValidator` (FluentValidation)
3. `CriarFornecedorHandler` (injeta `ITenantProvider`, `IUnitOfWork`)
4. Controller com uma linha: `_mediator.Send(command)`

Use o Cursor Composer (Ctrl+I) com o Context Agent ativo. Revise o output contra o checklist do artigo 04.

### Critério de conclusão

- [ ] Handler compila e passa validação
- [ ] Entidade herda `EntidadeSaaSBase`
- [ ] Zero lógica no Controller

---

## Etapa 2 — DDD na prática Epros

### O que estudar

* `EntidadeSaaSBase` — ler o código, entender cada campo
* [Outbox Pattern + Domain Events — comunicação entre módulos](backend/outbox-domain-events.md) — agregados, Value Objects e Domain Events no contexto Epros
* [Quartz.NET 3 — jobs agendados](backend/quartz-net-3.md) — Outbox Pattern — como o Quartz.NET processa eventos

### Conceitos-chave

| Conceito | No Epros |
| --- | --- |
| Factory method | `ContaPagar.Criar()` — única forma de instanciar |
| Comportamento | `cp.Baixar()` — muda estado + dispara evento |
| Value Object | `ValorMonetario` — imutável, sem identidade |
| Domain Event | `ContaPagarBaixada` — via Outbox para outros módulos |

### Exercício prático

**Criar agregado** `FornecedorPrincipal` com:

* 2 Value Objects (ex.: `Cnpj`, `Endereco`)
* 1 Domain Event (ex.: `FornecedorCadastrado`)
* Factory method com validação Flunt

### Critério de conclusão

- [ ] Agregado com setters privados
- [ ] Evento registrado na Outbox no `CommitAsync`
- [ ] Code Review Agent sem bloqueantes 🔴

---

## Recursos

| Recurso | Link |
| --- | --- |
| MediatR | [github.com/jbogard/MediatR](http://github.com/jbogard/MediatR) |
| EF Core | [learn.microsoft.com/ef/core](http://learn.microsoft.com/ef/core) |
| Módulo referência | `Financeiro/ContasAPagar/` |
| Arquitetura e ADRs | [Monólito modular: a arquitetura do Epros](02-monolito-modular.md) |
| Stack | [A stack completa](03-a-stack-completa.md) |
| Segurança e testes | [Multi-tenancy e os 8 testes](05-multi-tenancy-8-testes.md) |

---

**Próximo passo →** [Trilha Backend — observabilidade, eventos e CI](trilha-backend-observabilidade.md)
