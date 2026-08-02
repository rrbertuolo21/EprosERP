---
title: "Tutorial — Dev Frontend"
confluence_id: "200736769"
confluence_url: "https://rafaelbertuolo.atlassian.net/wiki/spaces/EprosWeb/pages/200736769/Tutorial+Dev+Frontend"
last_updated: "2026-07-13"
---

**O que você entrega:** telas consistentes com o design system — fluxo validado (UX) e componentes no padrão Nuxt 4.

**Índice:** [Tutoriais Dev Framework — uso por função (índice)](../indice-tutoriais.md)

---

## Quando executar

| Gatilho | O que fazer |
| --- | --- |
| Tela ou fluxo novo | Fase 04 UX |
| Task de frontend pronta | Fase 07 Dev — gancho frontend |
| Antes de abrir o PR | Code Review Agent no componente/diff |
| Componente com dados sensíveis ou auth | Security Agent (se aplicável) |

---

## Pré-requisitos

* **Repositório:** abra o **EprosERP** no Cursor (pasta `EprosApp/` para telas).
* **Context Agent:** ativo automaticamente.
* **Artefatos:** US com critérios; spec do submódulo; tech design se houver.

---

## Passo a passo

### Fase 04 — UX (tela nova)

1. Abra um **chat novo**.
2. Execute `/ux`.
3. Descreva a tela/fluxo e preencha os campos do prompt.
4. **Anexe** a US e referências de fluxo, se houver.
5. **Saída esperada:** fluxo/estrutura de componentes contra o design system (azul/dourado), WCAG e confirmações fiscais.
6. **Gate:** aprovado para dev (consistência + WCAG) — dono: UX/PO.

**→ Handoff:** fluxo aprovado → Dev (07).

---

### Fase 07 — Implementar componente/página

1. Abra um **chat novo**.
2. Execute `/dev`.
3. Cole o prompt de implementação (ver [tutorial Dev Backend](../backend/tutorial-dev-backend.md) — Prompt A).
4. Preencha task, submódulo e critérios de aceite.
5. O **gancho de frontend** aciona automaticamente os padrões Nuxt 4:

    * composable `useApi` com auth + tenant
    * Pinia para estado
    * TypeScript estrito
    * tabelas densas, formulários fiscais

6. **Anexe** US, fluxo UX (se houver) e spec.
7. **Saída esperada:** componente/página completa + testes.

---

### Antes do PR — Code Review

1. Confira o [checklist do autor](../code-review-checklists-e-boas-praticas.md).
2. Chat novo → `/code-review`.
3. Cole o prompt de Code Review (ver [Tutorial Tech Lead](../tech-lead/tutorial-tech-lead-arquiteto.md)).
4. **Anexe** o diff do componente.
5. Checagens típicas:

    * token **nunca** em localStorage
    * chamadas via `useApi`
    * estado no Pinia
    * tratamento de erro no catch

6. Corrija bloqueantes e avisos antes do PR.

**→ Handoff:** componente no padrão → PR para `develop` (ver [Fluxo de desenvolvimento — artigo 10](../10-fluxo-de-desenvolvimento.md)) → Tech Lead revisão final.

---

## Seu gate (pronto quando…)

| Momento | Gate | Dono |
| --- | --- | --- |
| 04 UX | aprovado p/ dev (consistência + WCAG) | UX/PO |
| 07 Dev | build + testes verdes | Dev |
| PR | Code Review sem bloqueantes | Tech Lead |

---

## Erros comuns / dicas

| Evite | Prefira |
| --- | --- |
| Implementar sem passar pelo UX (tela nova) | UX primeiro para fluxo e componentes |
| Token em localStorage | Auth via composable padrão |
| Chamada API direta sem `useApi` | Sempre via `useApi` com tenant |
| Pular Code Review no componente | Rodar no diff antes do PR |
| Misturar UX e Dev no mesmo chat | Chat novo por fase |
