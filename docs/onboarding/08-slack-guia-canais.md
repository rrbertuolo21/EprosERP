---
title: "Slack — guia dos canais"
confluence_id: "196476929"
confluence_url: "https://rafaelbertuolo.atlassian.net/wiki/spaces/EprosWeb/pages/196476929/Slack+guia+dos+canais"
last_updated: "2026-07-07"
---

> [!NOTE]
> **O que você vai aprender:** os 10 canais do Slack Epros, o que postar em cada um e o que nunca deve ir para o chat.

O Slack é a principal ferramenta de comunicação do time — mas canal errado gera ruído, informação perdida e ansiedade desnecessária.

Este artigo traduz a arquitetura de canais em regras práticas: **onde postar**, **o que evitar** e **como as automações ajudam** sem você precisar configurar nada.

---

## Os 10 canais em um relance

| Canal | Público | Uso principal |
| --- | --- | --- |
| `#daily` | Devs | Daily assíncrona — status diário |
| `#epros-geral` | Empresa | Comunicação geral (eventos, avisos) |
| `#epros-incidentes` | Time + stakeholders | Incidentes de **produção** |
| `#epros-produto` | PO + time | Backlog, Jira, discussão de features |
| `#releases` | Todos | Releases em produção |
| `#dev-prs` | Devs | PRs abertos/aprovados e CI falhou |
| `#dev-backend` | Squad backend | Dúvidas técnicas da squad |
| `#dev-frontend` | Squad frontend | Dúvidas técnicas da squad |
| `#dev-geral` | Devs | ADRs e decisões cross-squad |
| `#random` | Todos | Coisas aleatórias fora de escopo |

> [!TIP]
> Visão operacional do time (squads, cerimônias, DoR/DoD) → [Squads, cerimônias e como o time opera](07-squads-cerimonias.md). Este artigo aprofunda **só o Slack**.

---

## `#daily` — status sem reunião

**Objetivo:** substituir a daily síncrona por status escrito, visível para todo o time de dev.

Todo dev responde **diariamente** (ter–sex) às 3 perguntas:

1. O que fez ontem?
2. O que vai fazer hoje?
3. Algum bloqueio?

Um lembrete automático dispara às **9h** (horário de Brasília) com essas perguntas. Responda na **thread** do lembrete.

> [!IMPORTANT]
> **Não postar aqui:** discussões longas de código (use a squad), incidentes (`#epros-incidentes`), propostas de produto (`#epros-produto`).

---

## `#epros-geral` — comunicação da empresa

**Objetivo:** avisos transversais — não restritos ao time de desenvolvimento.

**Exemplos:** comunicados, eventos, novidades comerciais, avisos institucionais.

**Não postar aqui:** dúvidas técnicas, PRs, incidentes, backlog.

---

## `#epros-incidentes` — produção fora do ar

**Objetivo:** rastrear incidentes de **produção** com formato padronizado e fechamento explícito.

### Abertura (obrigatório)

```
[SEVERIDADE] o que quebrou → impacto → quem tá investigando
```

| Severidade | Critério |
| --- | --- |
| **P0** | Sistema indisponível ou perda de dados; clientes afetados agora |
| **P1** | Funcionalidade crítica degradada; workaround difícil |
| **P2** | Impacto limitado ou workaround viável |

### Fechamento (obrigatório)

Toda thread de incidente termina com mensagem de resolução — mesmo que curta. Canal sem "resolvido" gera ansiedade para quem entra depois.

> [!WARNING]
> Bugs de homolog/dev **não** vão aqui. Erros de deploy em ambientes internos são investigados no GitHub Actions — só produção entra neste canal.

Alertas automáticos (Sentry, falha de deploy em produção) **abrem a thread**; um humano assume, atualiza o status e **fecha** com resolução.

---

## `#epros-produto` — backlog com fonte de verdade

**Objetivo:** discutir produto **antes** do board refletir — sem decisões escondidas no Slack.

### Regras

1. Toda proposta de feature/melhoria **precisa referenciar issue Jira** (criar se não existir) **antes** de virar discussão longa.
2. Board **EP** (chave Jira `EP` — nova plataforma Epros) é fonte de verdade.
3. Slack é onde se **discute**; Jira é onde a decisão **fica registrada**.

**Não postar aqui:** código, PRs, incidentes de produção.

---

## `#releases` — deploy em produção

**Objetivo:** avisar dev e empresa quando algo sobe em produção.

* A mensagem automática do deploy é **suficiente** — ninguém precisa "confirmar recebido".
* Reação 👍 é **opcional**, não obrigatória.

**Não postar aqui:** deploy de dev/homolog, discussão de changelog (usar PR ou `#dev-geral`).

---

## `#dev-prs` — visibilidade de PRs e CI

**Objetivo:** centralizar o que importa sobre pull requests e pipelines.

### O que chega automaticamente

| Evento | Origem |
| --- | --- |
| PR aberto | GitHub |
| PR aprovado | GitHub |
| CI falhou no PR | GitHub Actions |

### O que **não** notifica

* Cada push no PR
* PR mergeado (release em produção já aparece em `#releases`)

**Não postar aqui:** discussão longa de arquitetura (use `#dev-geral` ou squad).

---

## `#dev-backend` e `#dev-frontend` — dúvidas da squad

**Objetivo:** discussões técnicas da squad respectiva.

**Exemplos:** padrão CQRS, dúvida de EF Core, componente Nuxt, pair informal.

**Não postar aqui:** PRs/CI (`#dev-prs`), incidentes, backlog de produto.

---

## `#dev-geral` — decisões cross-squad

**Objetivo:** decisões que afetam mais de uma squad e anúncio de ADRs.

**Exemplos:** nova convenção de código, decisão de arquitetura cross-módulo, link para ADR no repositório ou Confluence.

**Quem posta ADRs:** Tech Lead (ou delegado).

**Não substitui:** `#epros-geral` (empresa) nem `#epros-produto` (backlog).

---

## O que nunca vai no Slack

| Conteúdo | Onde fica |
| --- | --- |
| Acceptance Criteria definitivos | Jira (issue) |
| Decisão de arquitetura formal | ADR no repositório / Confluence |
| Código e diff | GitHub (PR) |
| Tokens, `.env`, credenciais | Vault / secrets — **nunca** Slack |
| Dados de tenant/cliente | Sistemas internos — **mascarar se inevitável** |

> [!CAUTION]
> Nunca cole tokens, connection strings, JWT ou dados de CPF de clientes. Screenshots de produção devem mascarar tenant e dados sensíveis.

---

## Automações — o que existe

Parte do fluxo já roda sozinha. Você só precisa **saber o que esperar** em cada canal:

| Canal | Automação | O que faz |
| --- | --- | --- |
| `#daily` | Lembrete diário | Posta as 3 perguntas às 9h (ter–sex) |
| `#dev-prs` | GitHub + CI | PR aberto, aprovado e pipeline falhou |
| `#releases` | Deploy produção | Aviso quando `main` sobe com sucesso |
| `#epros-incidentes` | Deploy + Sentry | Falha de deploy em produção; alertas P0/P1 |
| `#epros-produto` | Jira | Sprint criada/fechada; resumo semanal de triagem |

> [!NOTE]
> Responsabilidade do Tech Lead e PO.

---

**Próximo passo →** [Slack — comunicação no dia a dia](09-slack-comunicacao-dia-a-dia.md)
