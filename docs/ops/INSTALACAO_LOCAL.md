# Instalação Local — EprosERP (detalhe)

> **Dois fluxos distintos:**
>
> | Objetivo | Doc |
> |----------|-----|
> | Teste / validação (Docker completo, sem hot reload) | [QUICKSTART-LOCAL.md](../QUICKSTART-LOCAL.md) — canônico |
> | **Desenvolvimento** (hot reload) | [AMBIENTE-DEV.md](AMBIENTE-DEV.md) |
>
> Este arquivo expande o stack Docker e variações. Para o dia a dia de código, vá direto ao
> [AMBIENTE-DEV.md](AMBIENTE-DEV.md).

---

## Pré-requisitos

- Docker Desktop (rodando)
- .NET 8 SDK (API na máquina / migrations fora do Docker)
- Node 20 + npm (front na máquina; opcional se validar só via Docker)

---

## 1. Stack completo Docker (teste / validação)

Na raiz do repositório:

```bash
docker compose -f docker-compose.local.yml up -d --build
./scripts/seed-local.sh          # Unix / macOS / Git Bash
# Windows PowerShell: ./scripts/seed-local.ps1
```

Se o build da API der `DeadlineExceeded` (BuildKit):

```bash
DOCKER_BUILDKIT=0 docker compose -f docker-compose.local.yml build && docker compose -f docker-compose.local.yml up -d
./scripts/seed-local.sh          # ou ./scripts/seed-local.ps1 no Windows
```

| Serviço | URL / porta |
|---------|-------------|
| Front ERP | http://localhost:3000 |
| API / Swagger | http://localhost:8080/swagger |
| PostgreSQL (host) | `localhost:55432` — user/senha/db: `epros` / `epros_dev` / `epros` |

> Portas e senhas vêm de [`.env.example`](../../.env.example). Não confundir com o
> `docker-compose.yml` da raiz (Postgres em `5432` / senha `epros_dev_password` — infra completa).

### Credenciais após o seed (`scripts/seed-local.sh` / `scripts/seed-local.ps1`)

| Perfil | E-mail | Senha |
|--------|--------|-------|
| Admin plataforma (Landlord) | `admin@epros.local` | `Admin@12345` |
| Cliente demo (ERP) | `cliente@demo.local` | `Cliente@12345` |

---

## 2. Infraestrutura só (sem API/front)

```bash
docker compose up -d
```

Usa [docker-compose.yml](../../docker-compose.yml) na raiz (Postgres, Keycloak, Vault, MinIO, Valkey).

---

## 3. Desenvolvimento com hot reload

Passo a passo canônico: **[AMBIENTE-DEV.md](AMBIENTE-DEV.md)**  
(Postgres do `docker-compose.local.yml` + `dotnet watch` + `npm run dev`).

Resumo:

```bash
# banco + migrate
docker compose -f docker-compose.local.yml up -d postgres
docker compose -f docker-compose.local.yml run --rm migrate

# API (com ConnectionStrings__DefaultConnection → localhost:55432 / epros_dev)
dotnet watch run --project src/API/Epros.API/Epros.API.csproj --urls "http://localhost:8080"

# front (outro terminal)
cd EprosApp && npm install && npm run dev
```

Seed: `./scripts/seed-local.ps1` ou `./scripts/seed-local.sh` com a API local no ar.

Seed alternativo (legado/demo):

```powershell
powershell -ExecutionPolicy Bypass -File scripts/seed-ambiente-demo.ps1
```

Ou bash: `./scripts/seed-ambiente-demo.sh`

---

## 4. Problemas comuns

| Sintoma | Solução |
|---------|---------|
| BuildKit timeout | `DOCKER_BUILDKIT=0` (ver [QUICKSTART-LOCAL.md](../QUICKSTART-LOCAL.md)) |
| API não conecta no Postgres | `docker ps` — container `epros-novo-db` Up? Porta host **55432**? |
| Migration falha | `scripts/fix-migrations-history.sql` ou reset: `docker compose -f docker-compose.local.yml down -v` |
| Front tela branca em dev | Apague `EprosApp/.nuxt` e reinicie `npm run dev` |
| Porta 8080/3000 em uso no fluxo dev | `docker compose -f docker-compose.local.yml stop api web` |
| Rebuild só API/web (Docker) | `docker compose -f docker-compose.local.yml up -d --build --no-deps api web` |

---

## 5. Parar o ambiente

```bash
docker compose -f docker-compose.local.yml down      # mantém volumes
docker compose -f docker-compose.local.yml down -v   # apaga dados (reset)
```

---

*Atualizado: 2026-08-01 — distingue teste Docker × ambiente dev (hot reload).*
