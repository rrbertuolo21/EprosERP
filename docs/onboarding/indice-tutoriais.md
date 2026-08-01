---
title: "Tutoriais Dev Framework — uso por função (índice)"
confluence_id: "200605697"
confluence_url: "https://rafaelbertuolo.atlassian.net/wiki/spaces/EprosWeb/pages/200605697/Tutoriais+Dev+Framework+uso+por+fun+o+ndice"
last_updated: "2026-07-13"
---

**Para quem é:** todo o time que já está em uma ramificação pós-artigo 10 e precisa de referência rápida de **quando** e **como** acionar cada agente no Cursor.

**Navegação principal:** comece pela [ramificação da sua função](README.md) no índice do onboarding — cada função já inclui o tutorial prático na sequência de leitura. Este arquivo é um **catálogo operacional** de prompts e agentes, não o ponto de entrada da trilha.

**Leitura recomendada antes:** [16 agentes no Cursor](06-16-agentes-cursor.md) · [Como o time opera](07-squads-cerimonias.md) · [Monólito modular — a arquitetura](02-monolito-modular.md)

---

## As 3 peças (decore só isto)

| Peça | O que é |
| --- | --- |
| **Agente** | _Com quem você fala_ — persona em [`docs/fabrica/agentes/`](../fabrica/agentes/); rules ativas via Cursor |
| **Skill** | _O conhecimento_ — [`rules/`](../fabrica/cursor/cursor-install/rules/) (S01–S30) + [`skills/`](../fabrica/skills/) |
| **Prompt de partida** | _O que você cola_ para começar — modelos embutidos em cada tutorial abaixo |

---

## Passo a passo universal (vale para TODAS as funções)

Sempre a mesma receita, 5 passos:

1. **Abra um chat novo** — um agente por conversa; trocou de fase, chat novo.
2. **Execute o slash-command** da fase no Cursor (ex.: `/strategy`, `/dev`). O **Context Agent** já está ativo automaticamente.
3. **Cole o prompt de partida** da fase (modelo no tutorial da sua função) e **preencha os campos entre chaves**. Campo vazio = resposta genérica.
4. **Anexe o artefato da fase anterior** (US, spec, diff…). Anexe, não resuma de memória — o artefato É o contexto.
5. **Confira o gate** (critério de pronto da fase) e siga o handoff indicado no fim do prompt.

> **3 regras que nunca mudam:** (a) a **skill vence a memória do modelo**; (b) **um humano é dono de cada gate**; (c) **um agente por conversa**.

---

## Mapa de tutoriais (por função)

Cada tutorial faz parte da ramificação pós-artigo 10 da função correspondente. Use esta tabela para localizar o arquivo certo ou consultar agentes e fases.

| Sua função | Ramificação no onboarding | Tutorial | Agentes | Fases / momento |
| --- | --- | --- | --- | --- |
| **PO / Facilitador** | [Índice — PO / Facilitador](README.md) | [Tutorial — PO / Facilitador](po-facilitador/tutorial-po-facilitador.md) | Strategy, Discovery, Requirements | 01 · 02 · 03 |
| **Tech Lead / Arquiteto** | [Índice — Tech Lead](README.md) | [Tutorial — Tech Lead / Arquiteto](tech-lead/tutorial-tech-lead-arquiteto.md) · [Rotina de segunda](tech-lead/rotina-segunda-feira.md) | Planning, Architect, Code Review | 05 · 06 · gate do PR · ciclo semanal |
| **Dev Backend** | [Índice — Backend](README.md) | [Tutorial — Dev Backend](backend/tutorial-dev-backend.md) | Dev, Code Review, (Architect) | 07 |
| **Dev Frontend** | [Índice — Frontend](README.md) | [Tutorial — Dev Frontend](frontend/tutorial-dev-frontend.md) | UX, Dev, Code Review | 04 · 07 |
| **QA / SDET** | [Índice — QA / SDET](README.md) | [Tutorial — QA / SDET](qa/tutorial-qa-sdet.md) | QA, (Fiscal) | 08 |
| **Guardião de Domínio (Fiscal)** | [Índice — Guardião Fiscal](README.md) | [Tutorial — Guardião de Domínio (Fiscal)](fiscal/tutorial-guardiao-fiscal.md) | Fiscal | transversal (obrig. 03 · 07 · 08) |
| **Suporte / Migração** | [Índice — Suporte / Migração](README.md) | [Tutorial — Suporte / Migração](suporte/tutorial-suporte-migracao.md) | Support, Migration | fora da esteira |

_(Transversais que qualquer um aciona quando aplicável: **Security**, **Docs**, **Fiscal**, **Code Review** — prompts nos tutoriais de cada função.)_

---

## Qual repositório abrir no Cursor

Neste projeto há **um** repositório: **EprosERP**.

| Área | Pasta | Público | Onde está o processo |
| --- | --- | --- | --- |
| Tudo | docs/ | Todos | [ROTEIRO-ONBOARDING.md](../ROTEIRO-ONBOARDING.md) |
| Backend | `src/` | Dev Backend, QA, Fiscal | [`docs/fabrica/agentes/`](../fabrica/agentes/) + rules `.mdc` |
| Frontend | `Epros.App/` | Dev Frontend, QA | S11, S19 + [estrutura-pastas-front.md](estrutura-pastas-front.md) |
| Mobile | `Epros.Mobile/` | Mobile | submódulo |
| Processo / IA | `docs/fabrica/` | Todos | PIPELINE, skills, [CONFIGURAR-CURSOR](../fabrica/cursor/CONFIGURAR-CURSOR.md) |

Contexto global: `CLAUDE.md` + `docs/fabrica/cursor/cursor-install/rules/00-context.mdc`.

**Ops (fase 09):** [PIPELINE](../fabrica/processo/PIPELINE.md) · agente [`09-ops-agent.md`](../fabrica/agentes/09-ops-agent.md).

---

## Fluxos curtos

* **Bug de produção (hotfix):** Support tria → Dev corrige → Code Review → Ops deploy → QA regressão → Docs changelog → QA adiciona edge case ao catálogo.
* **Melhoria pequena (sem discovery):** Requirements → Planning → Dev → Code Review → QA → Ops.
* **Spike / dúvida técnica:** Architect → ADR ou nota técnica → volta ao Planning.

---

## Erros comuns (resumo)

| Não faça | Faça |
| --- | --- |
| Misturar duas fases no mesmo chat | Um agente por conversa |
| Resumir de memória o artefato anterior | Anexe o arquivo |
| Deixar campos do prompt em branco | Preencha todos antes de enviar |
| Aceitar resposta que contraria uma skill | A skill vence; reporte o desvio |
| Abrir PR sem Code Review Agent | Rode Code Review no diff antes do PR |
| Pedir código fiscal sem Fiscal Agent | Feature fiscal SEMPRE valida com Fiscal |
| Deixar o agente decidir o gate | O humano dono do gate decide |
