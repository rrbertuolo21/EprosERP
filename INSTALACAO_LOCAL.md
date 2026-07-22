# Instalação Local — EprosERP (Docker + API + Frontend)

Guia para subir o ambiente completo e navegar no sistema atual.

---

## Pré-requisitos

- Docker Desktop (rodando)
- .NET 8 SDK
- Node.js 18+ e npm

---

## 1. Infraestrutura (Docker)

Na pasta `EprosERP`:

```powershell
docker compose up -d
```

| Serviço | Porta | Credenciais |
|---------|-------|-------------|
| PostgreSQL | 5432 | `epros` / `epros_dev_password` / DB `epros` |
| Keycloak | 8080 | `admin` / `admin` — realm `epros-tenant` |
| Vault | 8200 | token `epros-dev-token` |
| MinIO | 9000 (API), 9001 (console) | `epros_minio` / `epros_minio_password` |
| Valkey (Redis) | 6379 | — |

Verificar:

```powershell
docker ps --filter "name=epros-"
```

---

## 2. API (.NET)

```powershell
cd EprosERP
dotnet run --project src/API/Epros.API/Epros.API.csproj --urls "http://localhost:5000"
```

- Swagger: http://localhost:5000/swagger  
- Em **Development**, as migrations de todos os módulos rodam automaticamente na subida.  
- Connection string: `appsettings.json` → `Host=localhost;Database=epros;Username=epros;Password=epros_dev_password`

---

## 3. Seed (instalação + tenant demo)

Com a API no ar, em outro terminal:

```powershell
cd EprosERP
powershell -ExecutionPolicy Bypass -File scripts/seed-ambiente-demo.ps1
```

Isso executa:

1. **Instalação** (`POST /api/v1/installation/execute`) — super-admin da plataforma  
2. **Tenant demo** (`POST /api/v1/public/auth/registrar-tenant`) — empresa ERP de teste  

### Credenciais após o seed

| Perfil | E-mail | Senha | Onde entra |
|--------|--------|-------|------------|
| **Admin tenant demo (recomendado)** | `demo@epros.local` | `Demo@123456` | Login em `/` → `/erp/acesso-rapido` |
| Super Admin plataforma | `admin@epros.com.br` | `SenhaSuperSegura123` | Tabela `usuarios_internos` — login público ainda não roteia para este perfil |

No login ERP, o campo **Tenant** pode ficar **vazio** (usa `tenant-padrao` internamente).

> **Nota:** Se `registrar-tenant` falhar (bug de transação entre DbContexts), o script aplica automaticamente `scripts/seed-demo-tenant.sql` no PostgreSQL.

---

## 4. Frontend (Nuxt)

```powershell
cd EprosERP/Epros.App
copy .env.example .env   # se ainda não existir — .env já aponta para localhost:5000
npm install
npm run dev
```

- App: **http://127.0.0.1:3000** (use este endereço no Windows — `localhost` pode falhar por IPv6)
- CORS da API libera `http://localhost:3000`  

### Rotas úteis

| Rota | Descrição |
|------|-----------|
| `/` | Login |
| `/cadastro` | Onboarding novo tenant |
| `/erp/acesso-rapido` | Atalhos ERP |
| `/erp/cadastros/parceiros` | Parceiros |
| `/erp/vendas/emissao/nfe` | Emissão NF-e |
| `/erp/pdv` | PDV / NFC-e |
| `/erp/financeiro/contas-a-pagar` | Contas a pagar |
| `/plataforma/admin` | Admin SaaS |

---

## Status atual (03/07/2026 — ambiente já subido)

| Componente | URL / status |
|--------------|--------------|
| Docker infra | ✅ `epros-postgres`, `keycloak`, `minio`, `vault`, `valkey` |
| API | ✅ http://localhost:5000 — Swagger http://localhost:5000/swagger |
| Frontend | ✅ http://localhost:3000 |
| Instalação | ✅ Concluída |
| Login ERP demo | ✅ `demo@epros.local` / `Demo@123456` |

---

## 5. Script único (atalho)

```powershell
# Terminal 1 — infra
docker compose up -d

# Terminal 2 — API
dotnet run --project src/API/Epros.API/Epros.API.csproj --urls "http://localhost:5000"

# Terminal 3 — seed (após API subir)
powershell -ExecutionPolicy Bypass -File scripts/seed-ambiente-demo.ps1

# Terminal 4 — frontend
cd Epros.App && npm run dev
```

---

## 6. O que esperar (honestidade técnica)

- **Build backend:** verde (344 testes).  
- **Migrations:** aplicadas na subida da API em Development.  
- **Login:** usa API real (`/public/auth/login`) quando disponível; fallback simulação se API offline.  
- **Telas ERP:** ~66 páginas; várias chamam API real, mas há **mismatches de rota** documentados em `PLANO_EQUALIZACAO.md` (Onda 4).  
- **DFe operacional:** emissão parcial via `EmitirDocumentoFiscal`; transmissão/inutilização/importação XML ainda incompletas.

---

## 7. Parar o ambiente

```powershell
docker compose down          # mantém volumes
docker compose down -v       # apaga dados PostgreSQL (reset total)
```

---

## 8. Problemas comuns

| Sintoma | Solução |
|---------|---------|
| API não conecta no Postgres | `docker ps` — `epros-postgres` Up? |
| Porta 5432 ocupada | Parar outro Postgres ou alterar porta no compose |
| Porta 5000 ocupada | `--urls "http://localhost:5001"` + atualizar `.env` do frontend |
| Login cai em simulação | API offline — verificar terminal da API |
| Migration falha (`relation already exists`) | Banco parcial de execução anterior — rodar `scripts/fix-migrations-history.sql` ou reset: `docker compose down -v` |
| `registrar-tenant` 500 | Bug conhecido de transação; use `scripts/seed-demo-tenant.sql` ou reexecute o seed |
| Keycloak lento na 1ª subida | Aguardar ~30s após `docker compose up` |

---

*Atualizado: 03/07/2026*
