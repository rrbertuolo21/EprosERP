# Roteiro de onboarding — EprosERP

> Este é o **roteiro de leitura**: leia nesta ordem e em ~30 min você entende o projeto, sobe o sistema
> e começa a produzir com o processo.
> Só validar o stack? → [QUICKSTART-LOCAL.md](QUICKSTART-LOCAL.md).
> Vai **codar** (hot reload)? → [ops/AMBIENTE-DEV.md](ops/AMBIENTE-DEV.md).

## Leia nesta ordem

1. **Subir o ambiente** — escolha um:
   - [QUICKSTART-LOCAL.md](QUICKSTART-LOCAL.md) — Docker completo (teste/validação, sem hot reload)
   - [ops/AMBIENTE-DEV.md](ops/AMBIENTE-DEV.md) — Postgres Docker + API/front na máquina (**dev**, hot reload)
   Credenciais de seed, Cursor e o **processo em 5 passos** estão no quickstart; o fluxo diário de código está no AMBIENTE-DEV.
2. **[CLAUDE.md](../CLAUDE.md)** — o "cérebro" do projeto (o Cursor/Claude carrega sozinho, mas **você
   leia**): o produto em uma frase, a estrutura, e as **disciplinas inegociáveis** (o gate de 3 camadas,
   "verde não é prova", negócio vem da skill, módulos sobem desabilitados).
3. **[CONVENCAO_CODIGO.md](../CONVENCAO_CODIGO.md)** — as convenções de código (Modo Porte × Consolidação,
   nomenclatura, multi-tenancy, CQRS). É o que mantém o código coeso.
4. **[fabrica/](fabrica/)** — o processo da fábrica, enxuto, dentro do projeto:
   - [fabrica/cursor/CONFIGURAR-CURSOR.md](fabrica/cursor/CONFIGURAR-CURSOR.md) — **ligue o Cursor** (as regras `.mdc`).
   - [fabrica/processo/](fabrica/processo/) — PIPELINE, MODELO, RETROSPECTIVA.
   - [fabrica/agentes/](fabrica/agentes/) — Dev, QA, Arquiteto, Segurança…
   - [fabrica/skills/](fabrica/skills/) — fan-out, armadilhas de porte/Docker, gateway de pagamento.
5. **[HISTORICO-DESENVOLVIMENTO-IA.md](../HISTORICO-DESENVOLVIMENTO-IA.md)** — o diário: o que já foi
   construído (blocos 0–12), decisões e números. Leia para **retomar de onde paramos**.
6. **[CONSOLIDACAO-GAPS.md](../CONSOLIDACAO-GAPS.md)** — o que ainda falta (o roadmap). É de onde saem as
   próximas tarefas.
7. **[README.md](README.md)** — índice de `docs/`: onboarding longo, orquestração, migração/DePara, processos.

## Quando ler o quê (atalho)

| Momento | Leia |
|---|---|
| **Primeiro dia** | 1 (subir: quickstart **ou** [AMBIENTE-DEV](ops/AMBIENTE-DEV.md)) → 2 (disciplinas) → [CONFIGURAR-CURSOR](fabrica/cursor/CONFIGURAR-CURSOR.md) |
| **Vai codar** | [ops/AMBIENTE-DEV.md](ops/AMBIENTE-DEV.md) |
| **Antes de codar** | 3 (convenções) + o agente/skill do tema em [fabrica/](fabrica/) |
| **Vai fazer muita tela/módulo** | [fan-out-paralelo.md](fabrica/skills/fan-out-paralelo.md) |
| **Mexer em pagamento** | [integracao-gateway-pagamento.md](fabrica/skills/integracao-gateway-pagamento.md) |
| **Travou no build/Docker** | [docker-deploy-armadilhas.md](fabrica/skills/docker-deploy-armadilhas.md) |
| **Retomar o projeto** | 5 (HISTORICO) + [PROMPT_REINICIO.md](../PROMPT_REINICIO.md) |
| **O que fazer agora** | 6 (CONSOLIDACAO-GAPS) |
| **Onboarding longo / DePara** | [README.md](README.md) → `onboarding/`, `migracao/`, `orquestracao/` |

## As 3 regras que você nunca esquece
1. **O "verde" do agente é entrada, não prova** — re-execute o build/test e valide no **ambiente vivo**.
2. **Negócio vem SEMPRE da skill de negócio** — nunca invente regra fiscal/tributária; fiscal fica
   travado até validação de contador.
3. **Commit/push só quando combinado**; nunca direto na `main`.
