# Instalação Local — EprosERP (detalhe)

> **Quickstart canônico:** [QUICKSTART-LOCAL.md](../QUICKSTART-LOCAL.md) — um comando + seed.

Guia expandido para quem prefere subir por partes ou depurar o stack.

---

## Pré-requisitos

- Docker Desktop (rodando)
- .NET 8 SDK (migrations/build fora do Docker)
- Node 20 + npm (typecheck do front na máquina; opcional se validar só via Docker)

---

## 1. Stack completo (recomendado)

Na raiz do repositório:

```bash
docker compose -f docker-compose.local.yml up -d --build
./scripts/seed-local.sh
```

Se o build da API der `DeadlineExceeded` (BuildKit):

```bash
DOCKER_BUILDKIT=0 docker compose -f docker-compose.local.yml build && docker compose -f docker-compose.local.yml up -d
./scripts/seed-local.sh
```

| Serviço | URL / porta |
|---------|-------------|
| Front ERP | http://localhost:3000 |
| API / Swagger | http://localhost:8080/swagger |
| PostgreSQL | `localhost:5432` — `epros` / `epros_dev_password` / DB `epros` |

### Credenciais após o seed (`scripts/seed-local.sh`)

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

## 3. API fora do Docker (opcional)

```bash
dotnet run --project src/API/Epros.API/Epros.API.csproj --urls "http://localhost:8080"
```

Em **Development**, migrations rodam na subida. Connection string em `appsettings.json` / variáveis de ambiente.

Seed alternativo (PowerShell):

```powershell
powershell -ExecutionPolicy Bypass -File scripts/seed-ambiente-demo.ps1
```

Ou bash: `./scripts/seed-ambiente-demo.sh`

---

## 4. Frontend fora do Docker (opcional)

```bash
cd Epros.App
cp .env.example .env   # NUXT_PUBLIC_API_BASE_URL → http://localhost:8080
npm install
npm run dev
```

App: http://localhost:3000 (ou http://127.0.0.1:3000 no Windows se IPv6 falhar).

---

## 5. Problemas comuns

| Sintoma | Solução |
|---------|---------|
| BuildKit timeout | `DOCKER_BUILDKIT=0` (ver [QUICKSTART-LOCAL.md](../QUICKSTART-LOCAL.md)) |
| API não conecta no Postgres | `docker ps` — container `epros-novo-db` Up? |
| Migration falha | `scripts/fix-migrations-history.sql` ou reset: `docker compose down -v` |
| Front tela branca em dev | `rm -rf Epros.App/.nuxt` e rebuild |
| Rebuild só API/web | `docker compose -f docker-compose.local.yml up -d --build --no-deps api web` |

---

## 6. Parar o ambiente

```bash
docker compose -f docker-compose.local.yml down      # mantém volumes
docker compose -f docker-compose.local.yml down -v   # apaga dados (reset)
```

---

*Atualizado: 2026-08-01 — alinhado a `docker-compose.local.yml` + `scripts/seed-local.sh`.*
