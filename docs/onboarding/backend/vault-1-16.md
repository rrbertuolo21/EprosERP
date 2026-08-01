---
title: "HashiCorp Vault 1.16 — gestão de segredos"
confluence_id: "191856664"
confluence_url: "https://rafaelbertuolo.atlassian.net/wiki/spaces/EprosWeb/pages/191856664/HashiCorp+Vault+1.16+gest+o+de+segredos"
last_updated: "2026-07-06"
---

**Versão fixada:** `1.16.x`

### O problema que resolve

```
❌ LEGADO: connection string, API keys, JWT secret — tudo em appsettings.json no repositório. Qualquer dev com acesso ao Git vê as credenciais de produção

✅ NOVO: Vault é o único lugar onde segredos existem
         A aplicação pede ao Vault em runtime
         O repositório não tem nenhuma credencial
```

### Tipos de segredo no Epros

```
KV (Key-Value) — segredos estáticos
  vault kv put secret/epros/minio \
    access_key="epros_minio" \
    secret_key="senha_supersecreta"

Database Secrets Engine — segredos dinâmicos (o melhor)
  vault write database/roles/epros-app \
    db_name=postgresql \
    creation_statements="CREATE ROLE '{{name}}' LOGIN PASSWORD '{{password}}' VALID UNTIL '{{expiration}}'; GRANT SELECT, INSERT, UPDATE, DELETE ON ALL TABLES IN SCHEMA financas TO '{{name}}';"

  # Cada instância da aplicação recebe credenciais únicas com TTL de 1 hora
  # Quando o TTL expira, as credenciais são automaticamente revogadas
  # Um ataque que compromete as credenciais tem janela de 1 hora
```

### Integração com ASP.NET Core

```csharp
// Program.cs — busca segredos do Vault na inicialização
builder.Configuration.AddVault(options =>
{
    options.VaultAddress = builder.Configuration["Vault:Address"];
    options.Token = Environment.GetEnvironmentVariable("VAULT_TOKEN");
    options.SecretPaths = new[]
    {
        "secret/epros/postgresql",
        "secret/epros/keycloak",
        "secret/epros/minio"
    };
});

// Depois disso, os segredos aparecem no Configuration como qualquer outra configuração
// Nunca há connection string hardcoded — nem em appsettings, nem em código
```
