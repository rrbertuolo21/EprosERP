---
title: "Trilha Frontend — Nuxt 4 em três superfícies"
confluence_id: "193462273"
confluence_url: "https://rafaelbertuolo.atlassian.net/wiki/spaces/EprosWeb/pages/193462273/Trilha+Frontend+Nuxt+4+em+tr+s+superf+cies"
last_updated: "2026-07-07"
---

> [!NOTE]
> **O que você vai aprender:** Nuxt 4, estado com Pinia, integração com API e padrões Epros para web, desktop e mobile.

**Leitura prévia recomendada:** [A stack completa](03-a-stack-completa.md), [Multi-tenancy e os 8 testes](05-multi-tenancy-8-testes.md) e [Estrutura de pastas do epros-front](estrutura-pastas-front.md).

> [!NOTE]
> Repositório: `epros-front` — pastas `app/` (web), `electron/` (desktop) e `capacitor/` (mobile). Uma stack, três superfícies.

---

## Etapa 1 — Nuxt 4 na prática

### O que estudar

* [Nuxt 4 — o framework unificado](frontend/nuxt-4.md)
* SSR vs SPA vs SSG — por que Epros usa SSR
* Pages, layouts e components — convenções de nomenclatura
* Auto-imports — como o Nuxt elimina imports explícitos

### Exercício prático

**Página de listagem de fornecedores** com paginação:

* `pages/financeiro/fornecedores/index.vue`
* Layout padrão do design system
* Loading e estado vazio

### Critério de conclusão

- [ ] SSR funcionando
- [ ] Paginação consumindo API `GET /fornecedores`
- [ ] TypeScript strict sem `any`

---

## Etapa 2 — Estado e API

### O que estudar

* Pinia — stores por domínio, actions assíncronas
* `useFetch` vs `$fetch` — tratamento de erro
* Autenticação Keycloak — token JWT no `authStore`
* Interceptors — `tenantId` em **toda** requisição

### Padrão de interceptor

```typescript
// Conceito — tenantId nunca hardcoded
$fetch.create({
  onRequest({ options }) {
    const auth = useAuthStore()
    options.headers = {
      ...options.headers,
      Authorization: `Bearer ${auth.token}`,
      'X-Tenant-Id': auth.tenantId,
    }
  },
})
```

Frontend nunca envia tenant fixo em código. Vem do token Keycloak.

### Exercício prático

**Store de autenticação** com refresh token automático:

* `stores/auth.ts` com Pinia
* Refresh antes da expiração
* Redirect para login em 401

---

## Etapa 3 — Padrões Epros Frontend

### O que estudar

* Composables — `use*` para lógica reutilizável
* Design system — tipografia, cores, componentes base
* Formulários com validação e feedback visual

### Exercício prático

**Formulário de cadastro de fornecedor:**

* Validação client-side alinhada com API (422)
* Feedback visual por campo
* Composable `useFornecedores()` para CRUD

---

## Etapa 4 — Qualidade e performance

### O que estudar

* TypeScript strict — DTOs espelhando API OpenAPI
* Vitest — testes de composables e componentes
* Lazy loading de páginas e imagens

### Exercício prático

**Testes para** `useFornecedores`:

* Mock da API
* Cenários: sucesso, erro 422, erro 500
* Cobertura mínima do composable

---

## Três superfícies

| Superfície | Pasta | Status | Notas |
| --- | --- | --- | --- |
| Web SaaS | `app/` | Ativo | Foco atual do time |
| Desktop | `electron/` | Fase 2 | Nuxt + SQLite local offline |
| Mobile / PDV | `capacitor/` | Fase 2 | Mesmo código Nuxt em iOS/Android |

* [Capacitor 6 — mobile e PDV](frontend/capacitor-6.md)
* [Electron + Capacitor — mesmo repositório do front](frontend/electron-capacitor.md)

---

## Agentes IA para frontend

| Momento | Agente |
| --- | --- |
| Coding | Dev Agent + Context Agent |
| Telas novas | UX Agent |
| Antes do PR | Code Review Agent (obrigatório) |

---

## Checklist de PR frontend

- [ ] [Retrocompat e contratos API](./code-review-checklists-e-boas-praticas.md#retrocompatibilidade-e-o-pwa) — especialmente mudanças em endpoints consumidos pelo PWA
- [ ] TypeScript strict — sem any
- [ ] tenantId via interceptor (não hardcoded)
- [ ] Componentes seguem design system
- [ ] Code Review Agent no comentário do PR
- [ ] Testes Vitest para composables alterados

---

**Trilha frontend concluída.**

**Próximo passo →** [Tutorial — Dev Frontend](frontend/tutorial-dev-frontend.md) — passo a passo hands-on de UX, Dev e Code Review no Cursor.

[Índice do Onboarding](README.md)
