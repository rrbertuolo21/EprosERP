---
title: "Nuxt 4 — o framework unificado"
confluence_id: "194281473"
confluence_url: "https://rafaelbertuolo.atlassian.net/wiki/spaces/EprosWeb/pages/194281473/Nuxt+4+o+framework+unificado"
last_updated: "2026-07-06"
---

> [!NOTE]
> **Versão fixada:** `4.x`

### Por que Nuxt 4 e não outros

| Critério | Nuxt 4 | Blazor WASM | Next.js |
| --- | --- | --- | --- |
| Reusa no Desktop (Electron) | ✅ 100% | ❌ | ❌ |
| Reusa no Mobile (Capacitor) | ✅ 100% | ❌ | Parcial |
| Time já conhece | ✅ Sim | ❌ Stack nova | Parcial |
| SSR nativo | ✅ | ❌ | ✅ |
| Blazor era uma 3ª stack | — | ❌ Problema | — |

### O que muda do Nuxt 3 para Nuxt 4

```
Nuxt 4 introduz o "Nuxt Application Structure" (NAS):
- Pastas reorganizadas: app/ em vez de tudo na raiz
- Tipagem mais forte por padrão
- Melhorias de performance no SSR
- API completamente compatível — migração incremental

Para o Epros:
- Mantemos os composables, stores Pinia, pages, components
- Atualizamos a estrutura de pasta conforme NAS
- Melhoramos a tipagem das chamadas à API
```

### Estrutura de projeto Nuxt 4

```
epros-front/
├── app/
│   ├── pages/              ← rotas automáticas
│   │   ├── index.vue       → /
│   │   ├── auth/
│   │   │   └── login.vue   → /auth/login
│   │   ├── financeiro/
│   │   │   ├── contas-pagar/
│   │   │   │   ├── index.vue   → listagem
│   │   │   │   └── [id].vue    → detalhe
│   │   │   └── contas-receber/
│   │   ├── vendas/
│   │   ├── estoque/
│   │   └── admin/          ← painel do tenant (migrado do Blazor)
│   │       ├── clientes/
│   │       ├── planos/
│   │       └── faturas/
│   ├── components/         ← componentes reutilizáveis
│   │   ├── ui/             ← base (botões, inputs, tabelas)
│   │   ├── financeiro/     ← específicos do domínio
│   │   └── shared/
│   ├── composables/        ← lógica reutilizável (useApi, useTenant)
│   ├── stores/             ← Pinia stores por domínio
│   │   ├── auth.store.ts
│   │   ├── contas-pagar.store.ts
│   │   └── tenant.store.ts
│   ├── layouts/
│   │   ├── default.vue     ← layout principal do ERP
│   │   ├── auth.vue        ← layout de login
│   │   └── admin.vue       ← layout do painel de gestão
│   └── middleware/
│       ├── auth.ts         ← redireciona para login se não autenticado
│       └── tenant.ts       ← valida contexto de tenant
├── public/
├── server/                 ← server-side do Nuxt (BFF leve)
│   └── api/               ← endpoints server-side se necessário
├── nuxt.config.ts
└── package.json
```

### Autenticação com Keycloak no Nuxt 4

```typescript
// composables/useAuth.ts
export const useAuth = () => {
  const config = useRuntimeConfig()

  const login = async (username: string, password: string) => {
    const response = await $fetch('/api/auth/token', {
      method: 'POST',
      body: { username, password }
      // Token retorna via cookie HttpOnly — nunca localStorage
    })
    return response
  }

  const logout = async () => {
    await $fetch('/api/auth/logout', { method: 'POST' })
    navigateTo('/auth/login')
  }

  return { login, logout }
}

// server/api/auth/token.post.ts — BFF que troca credenciais por token
// O token fica no cookie HttpOnly — JavaScript nunca tem acesso
export default defineEventHandler(async (event) => {
  const body = await readBody(event)

  const tokenResponse = await $fetch(
    `${process.env.KEYCLOAK_URL}/realms/epros/protocol/openid-connect/token`,
    {
      method: 'POST',
      body: new URLSearchParams({
        grant_type: 'password',
        client_id: 'epros-front',
        username: body.username,
        password: body.password
      })
    }
  )

  // Define cookie HttpOnly — impossível roubar via XSS
  setCookie(event, 'epros_token', tokenResponse.access_token, {
    httpOnly: true,
    secure: process.env.NODE_ENV === 'production',
    sameSite: 'strict',
    maxAge: 900 // 15 minutos
  })

  return { ok: true }
})
```
