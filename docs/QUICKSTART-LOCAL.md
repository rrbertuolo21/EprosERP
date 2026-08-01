# Quickstart local — EprosERP

> Em ~15 min você sobe tudo local e trabalha com o processo da fábrica (IA dirigida).
> Roteiro de leitura completo: [ROTEIRO-ONBOARDING.md](ROTEIRO-ONBOARDING.md).

## 1. Pré-requisitos (instale uma vez)
- **Docker Desktop** (obrigatório — sobe todo o stack)
- **.NET 8 SDK** (para migrations/build fora do Docker) · **Node 20** (para `nuxi typecheck` do front)
- **Cursor** (ou VS Code + Claude Code) — é como você conversa com os agentes
- Git

## 2. Subir o sistema (1 comando)
```bash
docker compose -f docker-compose.local.yml up -d --build
./scripts/seed-local.sh          # Unix / macOS / Git Bash
# Windows PowerShell: ./scripts/seed-local.ps1
```
Se o build da API der `DeadlineExceeded` (timeout do BuildKit):
```bash
DOCKER_BUILDKIT=0 docker compose -f docker-compose.local.yml build && docker compose -f docker-compose.local.yml up -d
```
Pronto:
| | |
|---|---|
| Front (ERP) | http://localhost:3000 |
| API / Swagger | http://localhost:8080/swagger |
| Login **admin** | `admin@epros.local` / `Admin@12345` → painel Landlord |
| Login **cliente** | `cliente@demo.local` / `Cliente@12345` → ERP |

> ⚠️ A pasta se chama `Epros.App` — o Finder do macOS a trata como aplicativo. Abra pelo **editor** ou
> **terminal** (ou botão direito → "Mostrar Conteúdo do Pacote").

## 3. Configurar o Cursor (o processo na sua máquina)
Siga [fabrica/cursor/CONFIGURAR-CURSOR.md](fabrica/cursor/CONFIGURAR-CURSOR.md). Em resumo: as **regras `.mdc`** em
`docs/fabrica/cursor/cursor-install/rules/` fazem o Cursor seguir as convenções do EprosERP
automaticamente (multi-tenancy, CQRS, EF, fiscal, segurança…). O [`CLAUDE.md`](../CLAUDE.md) da raiz é carregado
sozinho e traz as **disciplinas** (o gate, o fan-out, "verde não é prova").

## 4. Como trabalhar (o processo em 5 passos)
1. **Especifica** a tarefa (com a EF / regra de negócio — negócio vem SEMPRE da skill de negócio).
2. **Executa** com o agente Dev ([fabrica/agentes/07-dev-agent.md](fabrica/agentes/07-dev-agent.md)) carregando as skills certas.
3. **Auto-valida** (o agente roda o checklist da skill) — isso é entrada, não prova.
4. **RE-EXECUTA:** você (ou o orquestrador) roda o build/test de novo e **valida no ambiente vivo**
   (banco real, chamada real). *"Build verde" ≠ "funciona".*
5. **Code Review → você libera.** Commit em branch; manter paridade com `origin/main`.

Para trabalho grande (muitas telas/módulos): **fan-out** — 1 agente por pasta disjunta, molde primeiro.
Ver [fabrica/skills/fan-out-paralelo.md](fabrica/skills/fan-out-paralelo.md).

## 5. Onde achar as coisas
| Preciso de… | Onde |
|---|---|
| Cérebro do projeto (a IA lê) | [CLAUDE.md](../CLAUDE.md) |
| Rodar/deploy (detalhe) | [ops/INSTALACAO_LOCAL.md](ops/INSTALACAO_LOCAL.md) · `docker-compose.prod.yml` (deploy) |
| Convenções de código | [CONVENCAO_CODIGO.md](../CONVENCAO_CODIGO.md) |
| Diário / retomar | [HISTORICO-DESENVOLVIMENTO-IA.md](../HISTORICO-DESENVOLVIMENTO-IA.md) · [PROMPT_REINICIO.md](../PROMPT_REINICIO.md) |
| O que falta (roadmap) | [CONSOLIDACAO-GAPS.md](../CONSOLIDACAO-GAPS.md) |
| Agentes / processo / skills | [fabrica/](fabrica/) |
| Integração de pagamento (Mercado Pago) | [fabrica/skills/integracao-gateway-pagamento.md](fabrica/skills/integracao-gateway-pagamento.md) — plugar o access token em **Operação → Integrações / Gateways** |

## 6. Regras de ouro (não esqueça)
- **Negócio vem da skill de negócio**; fiscal travado até contador; nunca invente regra fiscal.
- **Módulos sobem desabilitados** (ABAC) — liberar por plano.
- **O "verde" do agente é entrada, não prova** — re-execute e valide no vivo.
- **Commit/push só quando combinado**; nunca direto na `main`.
