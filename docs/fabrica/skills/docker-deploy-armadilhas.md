---
name: docker-deploy-armadilhas
categoria: devops-infra
tags: [docker, buildkit, docker-compose, build, deploy, nuxt, dotnet, migrate, rls, dockerfile, csproj, node, path, macos, timeout, no-deps]
nivel: intermediario
aplica-se-a: [qualquer-projeto, dotnet, nuxt, docker-compose]
fontes:
  - "Lições reais — subida do EprosERP em Docker (build API .NET + front Nuxt + Postgres/migrate) — jul/2026"
status: v1-semente
revisao: semestral
---

# Docker: armadilhas de build e deploy (receitas)

> **Conhecimento agnóstico** — destilado de uma subida real (EprosERP: API .NET + front Nuxt +
> Postgres com RLS), mas as armadilhas e receitas valem para qualquer stack em Docker. O que é
> específico do projeto (nomes de serviço, lista de `.csproj`, vars de ambiente) vive no overlay
> do projeto — ver "Como um projeto aterra isto". Complementa [[containers-git]] (fundamentos de
> imagem/Dockerfile/compose) e [[entrega-continua]] (esteira).

## Quando usar

Ative quando a tarefa envolver: **subir/reconstruir um sistema em Docker, `docker compose build`
falhando, timeout de BuildKit, front que não builda local, `dotnet`/`node` fora do PATH,
Dockerfile que não restaura, `migrate` que sai com erro, rodar migração/RLS em container,
consolidar um repo no git antes do deploy, ou rebuildar um serviço sem derrubar o banco.**

## Princípios

- **A máquina do dev não é o ambiente de build.** PATH, runtimes (`node`, `dotnet`) e ferramentas
  podem estar ausentes ou fora do PATH. A verdade do build é a **imagem Docker**, não o shell local.
  Valide no container; use o shell local só para checagens que a imagem não faz (ex.: type-check).
- **Falha de build tem duas famílias: transitória (rede/timeout) e determinística (Dockerfile/deps).**
  Diagnostique a família antes de agir: transitória → retry/fallback de builder; determinística →
  corrija o arquivo, retry não resolve.
- **Migração é um passo com pré-requisitos próprios** (vars, conexão, ordem). Um migrate que roda
  no localhost do dev pode quebrar no container por conexão hardcoded ou var faltante.
- **`build` e `migrate` são operações distintas.** Rebuildar código não deve rederrubar/remigrar o
  banco. Saiba isolar (`--no-deps`) para não perder dados nem tempo.
- **A forma do repositório é contrato do compose.** Caminhos de contexto e `src/` são acoplamento
  físico; consolidar/mover pastas no git quebra o build silenciosamente.

---

## Receitas

### R1 — BuildKit falha com `DeadlineExceeded: context deadline exceeded`

**Sintoma:** `docker compose build` aborta puxando a imagem base ou enviando o contexto, com
`failed to solve … DeadlineExceeded: context deadline exceeded` (timeout de rede do BuildKit).

**Receita:**
1. Primeiro, **retry** — muitas vezes é rede transitória:
   ```bash
   docker compose build
   ```
2. Se persistir, **caia para o builder legado** (sem BuildKit), que tolera melhor rede lenta:
   ```bash
   DOCKER_BUILDKIT=0 docker compose build
   ```
3. Reduza o contexto enviado (um `.dockerignore` enxuto: sem `node_modules/`, `bin/`, `obj/`,
   `.git/`) para diminuir o tempo de upload que estoura o deadline.

> O builder legado é mais lento e sem cache moderno — use como **fallback**, não como padrão.

### R2 — Front (Nuxt) sem `node` no PATH: não dá para `npm`/typecheck local

**Sintoma:** na máquina do build não há `node`/`npm` no PATH, então não se roda `npm ci`,
`nuxi typecheck` nem lint localmente.

**Receita:**
1. **Valide o front pela imagem Docker.** O build real roda `nuxt generate` dentro do container,
   que **transpila sem type-check** — ou seja, gera o bundle mas *não* garante tipos corretos:
   ```bash
   DOCKER_BUILDKIT=0 docker compose build web
   ```
   Build verde aqui = transpilou e empacotou; **não** significa "sem erro de tipo".
2. **Rode `nuxi typecheck` na máquina do dono do front** (onde há `node`), separadamente, para
   pegar erros de tipo que o `nuxt generate` deixa passar:
   ```bash
   npx nuxi typecheck
   ```
3. Trate os dois como gates distintos: transpilação (Docker) e tipos (máquina com `node`).

### R3 — `dotnet` fora do PATH

**Sintoma:** `dotnet: command not found` ao tentar restaurar/buildar/rodar migrate localmente,
mesmo com o SDK instalado (instalação via script fica em `~/.dotnet`).

**Receita:** exporte `DOTNET_ROOT` e adicione ao PATH antes de usar `dotnet`:
```bash
export DOTNET_ROOT="$HOME/.dotnet"
export PATH="$DOTNET_ROOT:$PATH"
dotnet --info   # confirma
```
Persista no `~/.zshrc`/`~/.bashrc` para não repetir a cada sessão.

### R4 — Dockerfile da API não copia todos os `.csproj` antes do `restore`

**Sintoma:** o build da API quebra no `dotnet restore` quando um módulo novo é adicionado à
solution (ex.: `Epros.Imobiliaria.csproj`) mas o Dockerfile ainda não copia esse `.csproj`.

**Causa:** o padrão de cache é copiar só os `.csproj` primeiro, `restore`, depois copiar o resto.
Se a lista de `COPY *.csproj` não inclui o módulo novo, o `restore` não vê o projeto e falha.

**Receita:** **mantenha a lista de `COPY` dos `.csproj` no Dockerfile sincronizada com o `.sln`.**
Ao adicionar um projeto à solution, adicione a linha correspondente no Dockerfile:
```dockerfile
COPY ["src/Epros.Api/Epros.Api.csproj", "src/Epros.Api/"]
COPY ["src/Epros.Imobiliaria/Epros.Imobiliaria.csproj", "src/Epros.Imobiliaria/"]  # módulo novo
# ... um COPY por projeto do .sln ...
RUN dotnet restore
COPY . .
RUN dotnet publish -c Release -o /app
```
> Sinal de que faltou sincronizar: `restore` reclama de projeto referenciado que "não existe".

### R5 — `migrate-all.sh` precisa de `POSTGRES_*` no serviço migrate

**Sintoma:** o serviço de migração sai com erro no passo de **RLS via `psql`** (`Row-Level
Security`) porque as variáveis `POSTGRES_*` não chegam ao container do migrate.

**Causa:** o `migrate-all.sh` aplica as migrations do EF e depois roda um passo de RLS por `psql`,
que precisa de `PGHOST`/`POSTGRES_HOST`, `POSTGRES_USER`, `POSTGRES_PASSWORD`, `POSTGRES_DB`.
Sem elas, o `psql` não conecta.

**Receita:** passe as vars ao serviço `migrate` no compose:
```yaml
migrate:
  environment:
    POSTGRES_HOST: db
    POSTGRES_USER: ${POSTGRES_USER}
    POSTGRES_PASSWORD: ${POSTGRES_PASSWORD}
    POSTGRES_DB: ${POSTGRES_DB}
  depends_on:
    - db
```

### R6 — Design-time factory com connection hardcoded quebra o migrate fora do localhost

**Sintoma:** `dotnet ef` / migrate funciona na máquina do dev mas falha no container, tentando
conectar em `localhost`/`127.0.0.1` em vez do host do serviço de banco (ex.: `db`).

**Causa:** a `IDesignTimeDbContextFactory` tem a connection string **hardcoded** apontando para o
localhost do dev, ignorando a config do ambiente.

**Receita:** a factory de design-time deve **ler a connection do ambiente** (env var/config), com
o localhost apenas como default de dev:
```csharp
var conn = Environment.GetEnvironmentVariable("ConnectionStrings__Default")
           ?? "Host=localhost;Database=epros;Username=postgres;Password=postgres";
```
> Este é um caso clássico de acoplamento a ambiente ao portar/subir um sistema — ver
> [[engenharia-reversa/portabilidade]] (aterramento de conexões e configs ao mudar de ambiente).

### R7 — Estrutura do repo achatada vs aninhada (`src/` na raiz, contexto `.`)

**Sintoma:** após consolidar/mover pastas no git, o build quebra porque o `docker-compose.prod.yml`
espera `src/` **na raiz** do contexto e o `build.context` é `.`.

**Causa:** o compose referencia caminhos físicos (`dockerfile: src/Epros.Api/Dockerfile`,
`context: .`). Achatar ou aninhar a árvore muda esses caminhos e o build não acha o Dockerfile/os
`.csproj`.

**Receita:**
1. Antes de consolidar no git, confira o que o compose espera:
   ```bash
   grep -nE "context:|dockerfile:|src/" docker-compose.prod.yml
   ```
2. Mantenha a árvore que o compose contrata (`src/` na raiz, contexto `.`) **ou** atualize
   `context`/`dockerfile` junto com o `git mv`, no mesmo commit.
3. Valide com um build limpo após qualquer reorganização de pastas.

### R8 — Pasta terminando em `.app` (ex.: `EprosApp`) tratada como aplicativo pelo macOS

**Sintoma:** no Finder do macOS, a pasta `EprosApp` aparece como um aplicativo (ícone de app) e,
ao clicar, dá "aplicativo corrompido / não pode ser aberto". Parece que o diretório sumiu.

**Causa:** o Finder trata qualquer diretório com sufixo `.app` como *bundle* de aplicativo, não
como pasta comum.

**Receita:** o conteúdo está intacto — acesse por fora do duplo-clique:
- Abra pelo **editor/IDE** ou pelo **terminal** (`cd EprosApp && ls`), onde é uma pasta normal.
- No Finder: botão direito → **"Mostrar Conteúdo do Pacote"**.
- Não é corrupção; nada a "consertar" no arquivo.

### R9 — Rebuildar api/web sem re-rodar migrate nem derrubar o banco (`--no-deps`)

**Sintoma:** `docker compose up --build web` re-executa dependências (migrate) e/ou reinicia o
banco, custando tempo e arriscando os dados a cada rebuild de código.

**Receita:** use `--no-deps` para reconstruir só o serviço alvo, sem tocar em `db`/`migrate`:
```bash
docker compose build api web
docker compose up -d --no-deps api web
```
Assim o banco fica de pé, o migrate não roda de novo, e só o código da API/front é trocado.

---

## Armadilhas comuns

- Tratar todo build vermelho como "problema de código": timeout de BuildKit é **rede**, não Dockerfile.
- Confiar em "build do front passou" como garantia de tipos — `nuxt generate` **não** faz type-check.
- Assumir que o que roda no localhost do dev roda no container: PATH, conexões e vars diferem.
- Esquecer de sincronizar a lista de `.csproj` do Dockerfile ao adicionar um módulo ao `.sln`.
- Rodar `up --build` sem `--no-deps` e re-derrubar/remigrar o banco sem querer.
- Reorganizar pastas no git sem atualizar `context`/`dockerfile` do compose no mesmo commit.

## Checklist — subir/reconstruir em Docker

- [ ] `DOTNET_ROOT` e `PATH` exportados; `dotnet --info` OK (se build local)
- [ ] `.dockerignore` enxuto (sem `node_modules`, `bin`, `obj`, `.git`)
- [ ] `docker compose build`; se `DeadlineExceeded` → retry, depois `DOCKER_BUILDKIT=0`
- [ ] Lista de `COPY *.csproj` do Dockerfile == projetos do `.sln`
- [ ] Front validado via imagem (`nuxt generate`) **e** `nuxi typecheck` na máquina com `node`
- [ ] Serviço `migrate` recebe `POSTGRES_*` no compose (passo RLS via `psql`)
- [ ] Design-time factory lê a connection do ambiente (não hardcoded no localhost)
- [ ] `context`/`dockerfile` do compose batem com a árvore real do repo (`src/` onde esperado)
- [ ] Rebuild de código com `--no-deps` para não re-migrar/derrubar o banco
- [ ] Pasta `*.app` acessada por editor/terminal, não por duplo-clique no Finder

## Como um projeto aterra isto

O projeto define no seu overlay: os **nomes dos serviços** do compose (`api`, `web`, `db`,
`migrate`), a **lista real de `.csproj`** que o Dockerfile deve copiar, as **vars de ambiente**
(`POSTGRES_*`, connection strings) e a **árvore de pastas** contratada. Os overlays de projeto
(ex.: `projetos/epros/skills/…`) preenchem isto com os comandos e nomes reais do produto —
**referenciando** esta skill, sem copiá-la.

## Fontes

- Lições reais da subida do EprosERP em Docker (build API .NET + front Nuxt + Postgres/RLS) — jul/2026.

> Rascunhos de extração acumulam em `EXTRACOES.md` nesta mesma pasta até amadurecerem para cá.
