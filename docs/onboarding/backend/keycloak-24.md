---
title: "Keycloak 24 — identidade e autenticação"
confluence_id: "193429561"
confluence_url: "https://rafaelbertuolo.atlassian.net/wiki/spaces/EprosWeb/pages/193429561/Keycloak+24+identidade+e+autentica+o"
last_updated: "2026-07-06"
---

**Versão fixada:** `24.0`

### O que o Keycloak gerencia (e o Epros não precisa mais implementar)

```
✅ Login / logout / sessão
✅ Cadastro de usuário
✅ MFA (TOTP, SMS, WebAuthn)
✅ Recuperação de senha
✅ Hash de senha (Argon2id)
✅ Refresh token rotativo
✅ SSO entre sistemas
✅ Revogação de token
✅ Rotação automática de chaves JWT
✅ Claim customizado (tenantId, roles)
✅ Audit log de autenticação
```

### Fluxo de autenticação

```
1. Usuário → POST /realms/epros/protocol/openid-connect/token
             { username, password, client_id }

2. Keycloak → valida credenciais (Argon2id)
            → verifica MFA se configurado
            → emite JWT com claims:
              {
                "sub": "uuid-do-usuario",
                "tenantId": "tenant-empresa-xpto",
                "realm_access": {
                  "roles": ["financeiro.contas_pagar.write", "vendas.read"]
                },
                "exp": 1716900000  // 15 minutos
              }

3. Frontend → guarda token (cookie HttpOnly — nunca localStorage)
            → envia Authorization: Bearer {token} em toda requisição

4. Backend → TenantSaaSMiddleware extrai tenantId do claim
           → sem validar senha, sem "checar banco de usuários"
           → confia no Keycloak
```

### Configuração de claims customizados (tenantId)

```
No Keycloak Admin:
1. Clients → epros-api → Client Scopes → Mapper
2. Add mapper → User Attribute
   - Name: tenantId
   - User Attribute: tenantId
   - Token Claim Name: tenantId
   - Claim JSON Type: String
   - Add to access token: ON
   - Add to ID token: ON

Ao cadastrar usuário:
  User → Attributes → tenantId = "tenant-empresa-xpto"
```

### Multi-tenancy com Keycloak

```
Opção A (usada agora): Realm único + claim tenantId
  - Todos os tenants no mesmo realm
  - tenantId diferencia via claim
  - Mais simples de operar

Opção B (Enterprise, futuro): Realm por tenant
  - Isolamento total no nível do IAM
  - Permite personalização de login por cliente
  - Mais complexo de operar (um Keycloak por realm ou instâncias separadas)
```
