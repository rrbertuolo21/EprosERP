# Ambiente de desenvolvimento — EprosERP

> Guia para **codar com hot reload** (API + front na sua máquina).
>
> O stack Docker completo (`docker-compose.local.yml` + seed) é o ambiente de **teste/validação**
> (sem hot reload). Esse fluxo permanece em [QUICKSTART-LOCAL.md](../QUICKSTART-LOCAL.md) e
> [INSTALACAO_LOCAL.md](INSTALACAO_LOCAL.md).

## Diferença rápida

| | Teste / validação (Docker) | Desenvolvimento (este guia) |
|---|---|---|
| Como sobe | `docker compose ... up -d --build` | Postgres no Docker; API e front na máquina |
| Hot reload | Não — precisa rebuild | Sim (`dotnet watch` + `npm run dev`) |
| Front | http://localhost:3000 (nginx da imagem) | http://127.0.0.1:3000 (Nuxt HMR) |
| API | container `epros-novo-api` | processo local na porta 8080 |
| Quando usar | validar stack “como sobe”, demo, smoke | dia a dia de código |

---

## Pré-requisitos (uma vez)

- **Docker Desktop** (só para o Postgres)
- **.NET 8 SDK** (`dotnet --version` → 8.x)
- **Node 20 + npm** (`node -v` → v20.x)
- Git + clone deste repositório

---

## Passo a passo (primeira vez)

### 1. Subir só o banco (+ migrations)

Na raiz do repositório:

```bash
# Opcional: copiar defaults de porta/senha
cp .env.example .env

# Sobe Postgres e aplica migrations; NÃO sobe api/web (libera as portas 8080 e 3000)
docker compose -f docker-compose.local.yml up -d postgres
docker compose -f docker-compose.local.yml run --rm migrate
```

Confira:

```bash
docker compose -f docker-compose.local.yml ps
# epros-novo-db deve estar Up (healthy) na porta host 55432
```

> Já tinha o stack Docker completo rodando? Pare só API e front para liberar as portas:
> `docker compose -f docker-compose.local.yml stop api web`
> O Postgres (e o volume com dados/seed) continua.

### 2. Subir a API com hot reload

Abra um terminal na raiz. A connection string **precisa** apontar para o Postgres do compose
(porta **55432**, senha **epros_dev** — ver `.env.example`).

**PowerShell (Windows):**

```powershell
$env:ASPNETCORE_ENVIRONMENT = "Development"
$env:ConnectionStrings__DefaultConnection = "Host=localhost;Port=55432;Database=epros;Username=epros;Password=epros_dev"
$env:CORS_ORIGINS = "http://localhost:3000,http://127.0.0.1:3000"

dotnet watch run --project src/API/Epros.API/Epros.API.csproj --urls "http://localhost:8080"
```

**Bash (macOS / Linux / Git Bash):**

```bash
export ASPNETCORE_ENVIRONMENT=Development
export ConnectionStrings__DefaultConnection="Host=localhost;Port=55432;Database=epros;Username=epros;Password=epros_dev"
export CORS_ORIGINS="http://localhost:3000,http://127.0.0.1:3000"

dotnet watch run --project src/API/Epros.API/Epros.API.csproj --urls "http://localhost:8080"
```

Pronto quando o Swagger responder: http://localhost:8080/swagger

Em Development a API também aplica migrations na subida (além do container `migrate`).

### 3. Seed (só se o banco estiver vazio)

Com a API local no ar:

```powershell
# Windows
./scripts/seed-local.ps1
```

```bash
# Unix / macOS / Git Bash
./scripts/seed-local.sh
```

Credenciais criadas:

| Perfil | E-mail | Senha |
|--------|--------|-------|
| Admin plataforma (Landlord) | `admin@epros.local` | `Admin@12345` |
| Cliente demo (ERP) | `cliente@demo.local` | `Cliente@12345` |

### 4. Subir o front com hot reload

Em **outro** terminal:

```bash
cd EprosApp
cp .env.example .env
```

Edite `.env` para apontar à API local (não use a URL de exemplo de produção):

```env
NUXT_PUBLIC_API_BASE_URL=http://localhost:8080
NUXT_PUBLIC_REALTIME_URL=http://localhost:8080
NUXT_PUBLIC_STORAGE_URI=http://localhost:8080
```

Depois:

```bash
npm install
npm run dev
```

Front: http://127.0.0.1:3000 (o script usa `--host 127.0.0.1`).

---

## Dia a dia (já tem banco + seed)

1. Garantir Postgres: `docker compose -f docker-compose.local.yml up -d postgres`
2. Garantir que containers `api`/`web` **não** estão ocupando 8080/3000 (`stop api web` se preciso)
3. Terminal 1 → `dotnet watch run ...` (com as env vars do passo 2)
4. Terminal 2 → `cd EprosApp && npm run dev`
5. Abrir http://127.0.0.1:3000 e logar

Parar o banco (mantém dados):

```bash
docker compose -f docker-compose.local.yml stop postgres
```

Reset total do banco local:

```bash
docker compose -f docker-compose.local.yml down -v
# depois refaça os passos 1–4
```

---

## URLs

| Serviço | URL |
|---------|-----|
| Front (Nuxt HMR) | http://127.0.0.1:3000 |
| API / Swagger | http://localhost:8080/swagger |
| Postgres (host) | `localhost:55432` — user/senha/db: `epros` / `epros_dev` / `epros` |

---

## Problemas comuns

| Sintoma | Solução |
|---------|---------|
| Porta 8080 ou 3000 em uso | `docker compose -f docker-compose.local.yml stop api web` (ou mate o processo local) |
| API não conecta no Postgres | Confira `Port=55432` e senha `epros_dev`; `docker ps` → `epros-novo-db` healthy |
| `appsettings.json` “não pega” | O default do arquivo usa outra senha/porta; **obrigatório** exportar `ConnectionStrings__DefaultConnection` como acima |
| Front chama API errada | `.env` em `EprosApp` com `NUXT_PUBLIC_API_BASE_URL=http://localhost:8080`; reinicie `npm run dev` |
| Front tela branca / rota quebrada | Pare o Nuxt, apague `EprosApp/.nuxt`, suba de novo (`npm run dev`) |
| Seed “API não respondeu” | Swagger em `:8080` precisa estar no ar antes do `seed-local` |
| Quer validar o stack “empacotado” | Use [QUICKSTART-LOCAL.md](../QUICKSTART-LOCAL.md) (rebuild Docker; sem HMR) |

---

## Relação com outros docs

| Doc | Papel |
|-----|--------|
| [QUICKSTART-LOCAL.md](../QUICKSTART-LOCAL.md) | Stack Docker completo — teste/validação em 1 comando |
| [INSTALACAO_LOCAL.md](INSTALACAO_LOCAL.md) | Detalhe/expansão do stack Docker e variações |
| **Este arquivo** | Fluxo diário de desenvolvimento com hot reload |

*Atualizado: 2026-08-01.*
