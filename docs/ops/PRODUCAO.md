# Produção — VPS + autodeploy

Guia operacional para o stack [`docker-compose.prod.yml`](../../docker-compose.prod.yml) em VPS com deploy automático via GitHub Actions em `main`.

**Primeiro start do servidor:** [INICIAR-SERVIDOR-PRODUCAO.md](INICIAR-SERVIDOR-PRODUCAO.md)

---

## Arquitetura

| Componente | Função |
|---|---|
| Caddy | TLS (ACME), front estático, proxy API e MinIO |
| API (.NET 8) | Backend monolito modular |
| Frontend (Nuxt static) | PWA exportado para volume compartilhado |
| PostgreSQL 16 | Banco multi-tenant |
| Valkey 7 | Cache / filas |
| MinIO | Storage fiscal (XMLs, certificados) |

Domínios de produção (padrão): `app.siser.com.br`, `api.siser.com.br`, `storage.siser.com.br` — ver [endpoints](../processos/endpoints-ambientes-e-nomenclatura.md).

Auth V1: JWT local (`Seguranca__JwtSigningKey`). Keycloak/Vault não fazem parte deste stack.

---

## Dimensionamento (referência)

| Perfil | vCPU | RAM | Disco | Uso |
|---|---|---|---|---|
| MVP / baixo tráfego | 2 | 8 GB | 80 GB | Poucos tenants |
| Go-live recomendado | 4 | 16 GB | 160 GB | Produção inicial |
| Crescimento | 8+ | 32 GB+ | 240 GB+ | Mais tenants / fiscal pesado |

Ubuntu **24.04 LTS**. Prefira região/datacenter próximo aos usuários finais.

---

## Variáveis de ambiente

Template commitável: [`.env.production.example`](../../.env.production.example).

Arquivo real na VPS: `/opt/epros/.env.production` — **nunca commitar**.

Segredos críticos:

| Variável | Descrição |
|---|---|
| `POSTGRES_PASSWORD` | Senha do banco |
| `MINIO_ROOT_PASSWORD` | Admin MinIO |
| `COFRE_KEK_LOCAL` | KEK base64 (`openssl rand -base64 32`) |
| `Seguranca__JwtSigningKey` | Assinatura JWT (≥ 32 caracteres) |
| `CORS_ORIGINS` | Deve ser `https://` + `DOMAIN_APP` |

Registry (CD):

```bash
REGISTRY=ghcr.io/rrbertuolo21
IMAGE_TAG=<sha-do-commit>   # definido pelo workflow
```

---

## Autodeploy (GitHub Actions)

Workflow: [`.github/workflows/deploy-prod.yml`](../../.github/workflows/deploy-prod.yml).

Fluxo em cada push em `main` (exceto merge UI `web-flow`):

1. **CI** — format, build Release, testes
2. **Build & push** — imagens `epros-api` e `epros-frontend` no GHCR com tag `$GITHUB_SHA`
3. **Deploy SSH** — `scripts/deploy-prod.sh` na VPS: checkout SHA, pull, migrate, up, healthcheck

Disparo manual: Actions → **Deploy Production** → **Run workflow**.

Validação local (antes do go-live):

```bash
./scripts/validate-deploy-setup.sh
```

---

## Operação manual

### Deploy manual na VPS

```bash
cd /opt/epros
export IMAGE_TAG=<sha>
export REGISTRY=ghcr.io/rrbertuolo21
./scripts/deploy-prod.sh
```

### Bootstrap / rebuild completo (sem registry)

```bash
./scripts/deploy-prod.sh --build
```

### Rollback

```bash
export IMAGE_TAG=<sha_anterior_bom>
export DEPLOY_GIT_REF=<sha_anterior_bom>
./scripts/deploy-prod.sh
```

Ou re-executar o workflow no commit desejado via `workflow_dispatch`.

### Rebuild só API (sem derrubar banco)

```bash
docker compose -f docker-compose.prod.yml build api
docker compose -f docker-compose.prod.yml up -d --no-deps api
```

Ver armadilhas: [`docs/fabrica/skills/docker-deploy-armadilhas.md`](../fabrica/skills/docker-deploy-armadilhas.md) (BuildKit timeout → `DOCKER_BUILDKIT=0`, migrate vs build, `--no-deps`).

### Backup

Cron diário 02:00 UTC (instalado no bootstrap):

```bash
./scripts/backup-prod.sh
```

Arquivos em `/backups/` — retenção 7 dias (Postgres gzip + snapshots MinIO/DFe).

Restaurar Postgres:

```bash
gunzip -c /backups/postgres-YYYYMMDDTHHMMSSZ.sql.gz | \
  docker compose -f docker-compose.prod.yml exec -T postgres \
  psql -U epros -d epros
```

### Logs

```bash
docker compose -f docker-compose.prod.yml logs -f api caddy
```

---

## Migrations

Sempre via container `migrate` (incluído no `deploy-prod.sh`). Nunca rodar `dotnet ef` direto na VPS.

O serviço `migrate` exige `POSTGRES_*` para o passo RLS (`psql`) — já configurado no compose prod.

---

## Limitações V1

- **Canary 10%** (S22): não aplicável em nó único — estratégia é deploy full + healthgate.
- **Observabilidade**: logs Docker `json-file` apenas; stack Grafana fora de escopo.
- **Homolog/dev auto**: não incluídos; apenas produção em `main`.

---

## Troubleshooting

| Sintoma | Ação |
|---|---|
| ACME / certificado falha | Conferir DNS apontando para VPS; portas 80/443 abertas (UFW) |
| API não sobe | `docker compose logs api` — JWT vazio ou KEK inválido |
| Migrate falha no RLS | Verificar `POSTGRES_PASSWORD` e conectividade postgres |
| Pull GHCR negado | `docker login ghcr.io` na VPS ou secret `GHCR_READ_TOKEN` |
| BuildKit timeout | `DOCKER_BUILDKIT=0 docker compose build` |
