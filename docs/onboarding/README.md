---
title: "Onboarding Epros ERP — Índice"
confluence_id: "192610317"
confluence_url: "https://rafaelbertuolo.atlassian.net/wiki/spaces/EprosWeb/pages/192610317/Onboarding+Epros+ERP+ndice"
last_updated: "2026-08-01"
---

> **EprosERP:** trilha trazida de `epros/docs/onboarding`. Quickstart: [QUICKSTART-LOCAL.md](../QUICKSTART-LOCAL.md). Roteiro: [ROTEIRO-ONBOARDING.md](../ROTEIRO-ONBOARDING.md). Layout front: [estrutura-pastas-front.md](estrutura-pastas-front.md). Índice: [docs/README.md](../README.md).

# **Série de Artigos — Onboarding Epros ERP**

Cada artigo é autônomo, mas a ordem abaixo foi pensada para construir contexto progressivamente.

**Tempo total da trilha base:** ~1h45 · **11 artigos** + **ramificações por função** (conceito + tutorial prático)

## **Trilha base (todos os devs)**


| #   | Artigo                                                                                                                        | Tempo   | Público              |
| --- | ----------------------------------------------------------------------------------------------------------------------------- | ------- | -------------------- |
| 00  | [Boas-vindas ao Epros](00-boas-vindas-ao-epros.md)                         | ~5 min  | Todos                |
| 01  | [Mapa do produto — 17 módulos, 132 submódulos](01-mapa-do-produto-17-modulos.md) | ~25 min | Todos                |
| 02  | [Monólito modular — a arquitetura](02-monolito-modular.md)             | ~15 min | Todos                |
| 03  | [A stack completa](03-a-stack-completa.md)                             | ~15 min | Todos                |
| 04  | [Do Command ao PR — Contas a Pagar](04-do-command-ao-pr.md)            | ~18 min | Todos (foco backend) |
| 05  | [Multi-tenancy e os 8 testes do CI](05-multi-tenancy-8-testes.md)            | ~12 min | Todos                |
| 06  | [16 agentes no Cursor](06-16-agentes-cursor.md)                         | ~12 min | Todos                |
| 07  | [Como o time opera](07-squads-cerimonias.md)                            | ~10 min | Todos                |
| 08  | [Slack — guia dos canais](08-slack-guia-canais.md)                      | ~8 min  | Todos                |
| 09  | [Slack — comunicação no dia a dia](09-slack-comunicacao-dia-a-dia.md)             | ~7 min  | Todos                |
| 10  | [Fluxo de desenvolvimento — branches, ciclo e processo](10-fluxo-de-desenvolvimento.md) | ~15 min | Todos                |


---

## **Ramificações pós-artigo 10**

Após concluir a trilha base, siga a sequência da **sua função**. Cada ramificação combina artigos de conceito com o **tutorial prático** de uso dos agentes no Cursor.

Referência rápida de prompts e agentes: [índice de tutoriais](indice-tutoriais.md).

### Backend

| # | Artigo | Tipo |
| --- | --- | --- |
| 1 | [Trilha Backend — CQRS, DDD e EF Core](trilha-backend-cqrs-ddd.md) | Conceito (pode iniciar em paralelo com artigos 04–05) |
| 2 | [Trilha Backend — observabilidade, eventos e CI](trilha-backend-observabilidade.md) | Conceito |
| 3 | [Tutorial — Dev Backend](backend/tutorial-dev-backend.md) | Prática — Dev · Code Review · Security |

### Frontend

| # | Artigo | Tipo |
| --- | --- | --- |
| 1 | [Estrutura de pastas do EprosApp](estrutura-pastas-front.md) | Conceito — **ler primeiro** (layout atual) |
| 2 | [Trilha Frontend — Nuxt (três superfícies)](trilha-frontend-nuxt.md) | Conceito (material original pode citar Nuxt 4; runtime = Nuxt 3) |
| 3 | [Tutorial — Dev Frontend](frontend/tutorial-dev-frontend.md) | Prática — UX · Dev · Code Review |

### QA / SDET

| # | Artigo | Tipo |
| --- | --- | --- |
| 1 | [Trilha QA — testes e plano a partir dos ACs](trilha-qa.md) | Conceito (priorizar artigos 05 e 06 da base) |
| 2 | [Tutorial — QA / SDET](qa/tutorial-qa-sdet.md) | Prática — QA · regressão · catálogo de edge cases |

### Tech Lead

| # | Artigo | Tipo |
| --- | --- | --- |
| 1 | [Trilha Tech Lead — ADRs, fases e guardião](trilha-tech-lead.md) | Conceito (todos os artigos base + esta trilha) |
| 2 | [Tutorial — Tech Lead / Arquiteto](tech-lead/tutorial-tech-lead-arquiteto.md) | Prática — Planning · Architect · gate do PR |
| 3 | [Rotina de segunda — Tech Lead](tech-lead/rotina-segunda-feira.md) | Prática — operação semanal (reunião, reverts, merges, deploy) |

### PO / Facilitador

| # | Artigo | Tipo |
| --- | --- | --- |
| 1 | [Tutorial — PO / Facilitador](po-facilitador/tutorial-po-facilitador.md) | Prática — Strategy · Discovery · Requirements (fases 01–03) |

### Guardião Fiscal (transversal)

| # | Artigo | Tipo |
| --- | --- | --- |
| 1 | [Tutorial — Guardião de Domínio (Fiscal)](fiscal/tutorial-guardiao-fiscal.md) | Prática — Dúvida · Rejeição SEFAZ · SPED (obrig. nas fases 03 · 07 · 08) |

### Suporte / Migração (operação)

| # | Artigo | Tipo |
| --- | --- | --- |
| 1 | [Tutorial — Suporte / Migração](suporte/tutorial-suporte-migracao.md) | Prática — Triagem · migração de clientes legados (fora da esteira de produto) |

---

## **Referências transversais**

Documentos de consulta fora da trilha numerada — use quando precisar no dia a dia:

| Documento | Público | Uso |
| --- | --- | --- |
| [Code Review — checklist do autor](code-review-checklists-e-boas-praticas.md) | Dev backend/frontend | Antes de abrir o PR · retrocompat PWA |
| [Tutoriais Dev Framework — índice](indice-tutoriais.md) | Todos (pós-artigo 10) | Prompts e agentes por função |
| [Tutorial Tech Lead — gate do PR](tech-lead/tutorial-tech-lead-arquiteto.md) | Tech Lead | Checklist humano de merge após o Agent |
