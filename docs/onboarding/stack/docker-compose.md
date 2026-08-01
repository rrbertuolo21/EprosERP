---
title: "docker-compose.yml — sobe tudo com um comando"
confluence_id: "193953795"
confluence_url: "https://rafaelbertuolo.atlassian.net/wiki/spaces/EprosWeb/pages/193953795/docker-compose.yml+sobe+tudo+com+um+comando"
last_updated: "2026-07-06"
---

## docker-compose.yml — sobe tudo com um comando

```yaml
# docker compose up -d
# Sobe todos os 5 serviços com healthchecks

services:
  postgresql:
    image: postgres:16-alpine
    ports: ["5432:5432"]
    environment:
      POSTGRES_USER: epros
      POSTGRES_PASSWORD: epros_dev_password
      POSTGRES_DB: epros
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U epros"]
      interval: 5s
      timeout: 5s
      retries: 5

  keycloak:
    image: keycloak/keycloak:24.0
    ports: ["8080:8080"]
    environment:
      KEYCLOAK_ADMIN: admin
      KEYCLOAK_ADMIN_PASSWORD: admin
    command: start-dev
    depends_on:
      postgresql: { condition: service_healthy }

  vault:
    image: vault:1.16
    ports: ["8200:8200"]
    environment:
      VAULT_DEV_ROOT_TOKEN_ID: epros-dev-token
      VAULT_DEV_LISTEN_ADDRESS: 0.0.0.0:8200
    cap_add: [IPC_LOCK]

  minio:
    image: minio/minio:latest
    ports: ["9000:9000", "9001:9001"]
    environment:
      MINIO_ROOT_USER: epros_minio
      MINIO_ROOT_PASSWORD: epros_minio_password
    command: server /data --console-address ":9001"

  valkey:
    image: valkey/valkey:7-alpine
    ports: ["6379:6379"]

  # ─── Observabilidade ─────────────────────────────────────────────────────
  # Grafana, Prometheus, Loki e Tempo NÃO sobem no ambiente local
  # por restrições de hardware das máquinas dos clientes.
  # Para subir a stack de observabilidade no servidor de staging/produção:
  #   docker compose -f docker-compose.observability.yml up -d
  #
  # Em desenvolvimento, use:
  #   - Console do Serilog (logs estruturados visíveis no terminal)
  #   - dotnet trace (profiling pontual quando necessário)
```

## URLs locais após `docker compose up -d`

| Serviço | URL | Credenciais |
| --- | --- | --- |
| API + Swagger | [https://localhost:7000/swagger](https://localhost:7000/swagger) | JWT do Keycloak |
| Keycloak Admin | [http://localhost:8080](http://localhost:8080) | admin / admin |
| MinIO Console | [http://localhost:9001](http://localhost:9001) | epros_minio / epros_minio_password |
| Vault UI | [http://localhost:8200](http://localhost:8200) | token: epros-dev-token |

**Observabilidade (Grafana, Prometheus, Loki, Tempo):** roda no servidor, não no ambiente local. Ver `docker-compose.observability.yml` no repositório.
