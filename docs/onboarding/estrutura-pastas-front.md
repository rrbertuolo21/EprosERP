---
title: "Estrutura de pastas do Epros.App — superfícies e domínios"
last_updated: "2026-08-01"
---

> **Adaptado para EprosERP.** Layout real do Nuxt 3 em `Epros.App/`. Rules e skills: [`docs/fabrica/`](../fabrica/).

> [!NOTE]
> **O que você vai aprender:** onde cada arquivo do frontend vive, como as três superfícies se separam e como o IO com a API deve ser feito.

O backend organiza código em `src/Modules/Epros.Modules.<Nome>/` (Clean Arch: Domain / Application / Infrastructure). O frontend **não** espelha 1:1 o assembly — espelha **domínio de UX** em `pages/erp/<área>/`.

Mapa de rotas e fatias para fan-out: [`Epros.App/MAPA_FRONTEND.md`](../../Epros.App/MAPA_FRONTEND.md).

---

## Regra de ouro

| Camada | Caminho | Convenção |
|---|---|---|
| Backend | `src/Modules/Epros.Modules.<Nome>/` | Assembly por módulo |
| Frontend (ERP) | `Epros.App/pages/erp/<domínio>/` | kebab-case nas pastas de rota |
| Frontend (Landlord) | `Epros.App/pages/plataforma/` | Admin SaaS |
| Frontend (Portal) | `Epros.App/pages/area-cliente/` | Área do cliente final |
| Componentes | `Epros.App/components/<feature>/` + `components/shared/` | Parallel às áreas |

```
Negócio              Módulo backend              Rota UI
Contas a Pagar  →    Epros.Modules.Financeiro →  pages/erp/financeiro/...
Produtos        →    Epros.Modules.Estoque    →  pages/erp/estoque/... ou cadastros/produtos
```

> [!IMPORTANT]
> IO **somente** via `useApi` / `useApiList` (`API_PREFIX = '/api/v1'`). Token/tenant via `plugins/api.ts`. Não hardcodar `localhost` em página nova.

---

## Árvore raiz do `Epros.App/`

```
Epros.App/
├── pages/
│   ├── index.vue                 # Login
│   ├── cadastro.vue
│   ├── dashboard.vue
│   ├── erp/                      # Operação do tenant
│   │   ├── cadastros/
│   │   ├── compras/
│   │   ├── vendas/
│   │   ├── estoque/
│   │   ├── financeiro/
│   │   ├── fiscal/
│   │   ├── contabilidade/
│   │   ├── rh/
│   │   ├── producao/
│   │   ├── projetos/
│   │   ├── manutencao/
│   │   ├── qualidade/
│   │   ├── grc/
│   │   ├── esg/
│   │   ├── concessionarias/
│   │   ├── imobiliaria/
│   │   ├── pdv/
│   │   ├── configuracoes/
│   │   ├── relatorios/
│   │   └── integracao/
│   ├── plataforma/               # Landlord / admin SaaS
│   └── area-cliente/             # Portal
├── components/
│   ├── shared/                   # DataTable, FilterBar, PageToolbar, DeleteAlert…
│   └── <feature>/
├── composables/
├── plugins/api.ts
├── layouts/
├── middleware/
├── assets/
└── types/
```

---

## Molde de tela

- `<script setup lang="ts">`
- `definePageMeta({ layout: 'default' })`
- Listagem: `DataTable` + `FilterBar` + `PageToolbar` + `DeleteAlert`
- Envelope: `CommandResult`
- Textos UI: português BR
- UX ERP: `docs/fabrica/cursor/cursor-install/rules/S19-ux-erp-patterns.mdc`

### Fan-out seguro

1 agente por pasta disjunta `pages/erp/<mod>/` — skill [`fan-out-paralelo.md`](../fabrica/skills/fan-out-paralelo.md).

---

## Backend (lembrete rápido)

```
src/Modules/Epros.Modules.<Nome>/
├── Domain/{Entities,ValueObjects,Enums}
├── Application/{Commands,Queries,Handlers[,Models]}
├── Infrastructure/{Data,Jobs,Services}
└── Migrations/
```

API: `src/API/Epros.API/Controllers` — controllers finos, só `_mediator.Send()`.

---

## Próximo

- Conceito front: [Trilha Frontend — Nuxt](trilha-frontend-nuxt.md) (runtime atual = **Nuxt 3**)
- Tutorial: [Tutorial — Dev Frontend](frontend/tutorial-dev-frontend.md)
- Stack: [COMECE-AQUI.md](../../COMECE-AQUI.md)
