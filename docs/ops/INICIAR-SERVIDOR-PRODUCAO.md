# Como iniciar o servidor VPS de produção

Guia passo a passo para subir o EprosERP em uma **VPS Ubuntu** com Docker Compose. Operação contínua (autodeploy, rollback, backup): [PRODUCAO.md](PRODUCAO.md).

Stack: [`docker-compose.prod.yml`](../../docker-compose.prod.yml) · Bootstrap: [`scripts/server-bootstrap.sh`](../../scripts/server-bootstrap.sh) · Deploy: [`scripts/deploy-prod.sh`](../../scripts/deploy-prod.sh)

---

## Pré-requisitos

| Item | Requisito |
|---|---|
| Servidor | VPS com Ubuntu **24.04 LTS** |
| Recursos mínimos | 2 vCPU, 8 GB RAM, 80 GB disco (recomendado go-live: 4 vCPU, 16 GB RAM) |
| Rede | IP público fixo; portas **22**, **80** e **443** liberadas |
| DNS | Registros **A** (e **AAAA** se usar IPv6) para `app`, `api` e `storage` apontando para o IP da VPS |
| Domínios | Ex.: `app.siser.com.br`, `api.siser.com.br`, `storage.siser.com.br` — ver [endpoints](../processos/endpoints-ambientes-e-nomenclatura.md) |

---

## Passo 1 — Criar e acessar a VPS

1. Provisione uma VPS Ubuntu 24.04 no provedor de sua escolha.
2. Anote o **IP público** da máquina.
3. Configure os registros DNS (TTL **300** no primeiro go-live).
4. Conecte via SSH como root (ou usuário com sudo):

```bash
ssh root@<IP_DA_VPS>
```

---

## Passo 2 — Bootstrap do servidor

Execute o script de bootstrap **como root**. Ele instala Docker, configura firewall (UFW), cria o usuário `deploy`, clona o repositório em `/opt/epros` e agenda backup diário.

```bash
curl -fsSL https://raw.githubusercontent.com/rrbertuolo21/EprosERP/main/scripts/server-bootstrap.sh | bash
```

Ou, se já tiver o repositório clonado:

```bash
sudo bash scripts/server-bootstrap.sh
```

O bootstrap deixa pronto:

- Docker Engine + Compose plugin
- UFW: SSH (22), HTTP (80), HTTPS (443)
- Usuário `deploy` (membro do grupo `docker`)
- Repositório em `/opt/epros`
- Diretório `/backups` para dumps

---

## Passo 3 — Configurar variáveis de produção

Troque para o usuário `deploy` e crie o arquivo de ambiente:

```bash
sudo su - deploy
cd /opt/epros
cp .env.production.example .env.production
nano .env.production
```

Preencha **todos** os valores marcados como `CHANGE_ME`. Os campos obrigatórios:

| Variável | O que colocar |
|---|---|
| `POSTGRES_PASSWORD` | Senha forte do PostgreSQL |
| `MINIO_ROOT_USER` / `MINIO_ROOT_PASSWORD` | Credenciais do MinIO |
| `COFRE_KEK_LOCAL` | `openssl rand -base64 32` |
| `Seguranca__JwtSigningKey` | String aleatória com **≥ 32 caracteres** |
| `DOMAIN_APP` / `DOMAIN_API` / `DOMAIN_STORAGE` | Domínios reais (sem `https://`) |
| `ACME_EMAIL` | E-mail para certificados Let's Encrypt |
| `CORS_ORIGINS` | `https://` + valor de `DOMAIN_APP` |
| `NUXT_PUBLIC_*` | URLs públicas `https://` dos serviços |
| `REGISTRY` | `ghcr.io/<seu-usuario-github>` |

Salve e confira permissões:

```bash
chmod 600 .env.production
```

---

## Passo 4 — Primeiro deploy (build local)

No primeiro start, as imagens são **construídas na VPS** (não vêm do registry ainda):

```bash
cd /opt/epros
./scripts/deploy-prod.sh --build
```

O script:

1. Sobe Postgres, Valkey e MinIO
2. Executa migrations (EF Core + RLS)
3. Sobe API, exporta o front estático e inicia o Caddy (TLS automático)
4. Aguarda `https://<DOMAIN_API>/health` responder OK

Acompanhe os logs se algo demorar:

```bash
docker compose -f docker-compose.prod.yml logs -f api caddy migrate
```

---

## Passo 5 — Validar que o servidor está no ar

Execute na VPS ou na sua máquina (após DNS propagado):

```bash
curl -fsS https://api.<seu-dominio>/health
```

Abra no navegador:

- Front: `https://app.<seu-dominio>`
- API (Swagger, se exposto): `https://api.<seu-dominio>/swagger`

Confirme certificado TLS válido (cadeado verde). Se o ACME falhar, veja [Troubleshooting](#troubleshooting) abaixo.

---

## Passo 6 — Preparar autodeploy (opcional, recomendado)

Para deploys automáticos a cada merge em `main`:

### 6.1 Login no GHCR na VPS

```bash
docker login ghcr.io
# Usuário: seu login GitHub
# Senha: PAT com permissão read:packages
```

### 6.2 Chave SSH para GitHub Actions

Na sua máquina local:

```bash
ssh-keygen -t ed25519 -f epros-deploy -N ""
```

- Chave **pública** (`epros-deploy.pub`) → `/home/deploy/.ssh/authorized_keys` na VPS
- Chave **privada** → secret `PROD_SSH_KEY` no GitHub

### 6.3 Secrets e variables no GitHub

**Secrets** (Settings → Secrets → Actions):

| Secret | Valor |
|---|---|
| `PROD_SSH_HOST` | IP ou hostname da VPS |
| `PROD_SSH_USER` | `deploy` |
| `PROD_SSH_KEY` | Conteúdo da chave privada |
| `PROD_SSH_PORT` | (opcional) padrão 22 |
| `GHCR_READ_TOKEN` | (opcional) PAT read:packages |

**Variables** (Settings → Variables → Actions):

| Variable | Exemplo |
|---|---|
| `PROD_NUXT_PUBLIC_API_BASE_URL` | `https://api.siser.com.br` |
| `PROD_NUXT_PUBLIC_REALTIME_URL` | `https://api.siser.com.br` |
| `PROD_NUXT_PUBLIC_STORAGE_URI` | `https://storage.siser.com.br` |

Crie também o **Environment** `production` se quiser gate de aprovação manual antes do deploy.

Após configurar, o workflow [`.github/workflows/deploy-prod.yml`](../../.github/workflows/deploy-prod.yml) fará pull das imagens GHCR e executará `./scripts/deploy-prod.sh` a cada push em `main`.

---

## Comandos úteis após o start

```bash
# Status dos containers
docker compose -f docker-compose.prod.yml ps

# Reiniciar só a API (sem derrubar banco)
docker compose -f docker-compose.prod.yml up -d --no-deps api

# Parar tudo (cuidado — derruba o serviço)
docker compose -f docker-compose.prod.yml down

# Subir novamente (dados persistem nos volumes)
docker compose -f docker-compose.prod.yml up -d

# Backup manual
./scripts/backup-prod.sh
```

---

## Troubleshooting

| Problema | Solução |
|---|---|
| Certificado TLS não emite | DNS ainda propagando ou portas 80/443 bloqueadas; confira `dig app.<dominio>` e `ufw status` |
| `POSTGRES_PASSWORD is required` | `.env.production` ausente ou incompleto |
| API reinicia em loop | JWT ou KEK vazio — veja `docker compose logs api` |
| Migrate falha | Postgres não healthy; rode `docker compose logs postgres migrate` |
| BuildKit timeout no build | `DOCKER_BUILDKIT=0 ./scripts/deploy-prod.sh --build` |
| Pull GHCR negado | `docker login ghcr.io` na VPS |

Validação local antes do go-live (na sua máquina de dev, com Docker instalado):

```bash
./scripts/validate-deploy-setup.sh
```

---

## Checklist rápido

- [ ] VPS Ubuntu 24.04 com IP fixo
- [ ] DNS `app` / `api` / `storage` → IP da VPS
- [ ] Bootstrap executado (`server-bootstrap.sh`)
- [ ] `.env.production` preenchido e protegido (`chmod 600`)
- [ ] `./scripts/deploy-prod.sh --build` concluído com sucesso
- [ ] `https://api.<dominio>/health` retorna OK
- [ ] Front acessível em `https://app.<dominio>`
- [ ] (Opcional) Secrets GitHub + chave SSH para autodeploy
