---
title: "Slack — comunicação no dia a dia"
confluence_id: "196313090"
confluence_url: "https://rafaelbertuolo.atlassian.net/wiki/spaces/EprosWeb/pages/196313090/Slack+comunica+o+no+dia+a+dia"
last_updated: "2026-07-07"
---

> [!NOTE]
> **O que você vai aprender:** templates prontos para daily, incidentes, produto e ADRs — e as boas práticas que mantêm o Slack útil em vez de barulhento.

Canal certo é metade do caminho. A outra metade é **como escrever**: formato consistente, threads organizadas e links para a fonte de verdade.

Este artigo reúne os templates que o time usa no dia a dia. Copie, adapte e poste no canal certo.

---

## Daily

Responda na **thread** do lembrete automático das 9h.

### Resposta padrão

```
*Ontem:* finalizei handler de BaixaContaPagar (EP-142)
*Hoje:* abrir PR + testes de integração
*Bloqueios:* nenhum
```

### Com bloqueio

```
*Ontem:* investiguei falha no TenantLeakTest
*Hoje:* corrigir filtro de query no módulo Financeiro
*Bloqueios:* preciso de revisão do @tech-lead no approach de IgnoreQueryFilters — thread no #dev-backend
```

> [!TIP]
> Bloqueio real? Mencione a pessoa certa e indique onde a discussão técnica continua (`#dev-backend`, `#dev-frontend` ou `#dev-geral`).

---

## Incidente

Canal: `#epros-incidentes`. Toda atualização e o fechamento ficam na **mesma thread**.

### Abertura

```
[P0] API de emissão NFe retornando 503 → todos os clientes sem emitir nota → investigando: @joao
```

```
[P1] Relatório de contas a pagar com timeout > 30s → workaround: export CSV manual → investigando: @maria
```

```
[P2] Label errada na tela de PDV → sem impacto fiscal → investigando: @pedro
```

### Atualização (durante investigação)

```
[P0] Causa provável: pool de conexões PostgreSQL esgotado. Reiniciando serviço api. ETA 15 min.
```

### Fechamento

```
✅ Resolvido: pool aumentado de 50→100 conexões. NFe voltou às 14:32. RCA em EP-891.
```

```
✅ Resolvido: deploy revertido. Monitorando 1h. Sem recorrência até agora.
```

> [!IMPORTANT]
> Fechamento é **obrigatório**. Quem entra no canal depois precisa saber que o incidente terminou — não deixe threads abertas sem resolução.

Alertas automáticos (Sentry, falha de deploy) abrem a thread; **você** assume, atualiza e fecha no formato acima.

---

## Produto

Canal: `#epros-produto`. Uma issue = preferencialmente uma thread.

### Proposta de feature (antes da discussão longa)

```
💡 Proposta: filtro por centro de custo na listagem de CP

Issue: EP-203 (criada agora)
Contexto: clientes com +5 CC pedem isso toda semana
Quero validar escopo antes de colocar no sprint.
```

### Decisão encaminhada para o board

```
Decisão da thread: entra no sprint 24 como EP-203.
ACs atualizados no Jira. Board EP reflete.
```

> [!WARNING]
> Não discuta feature longa sem issue Jira. Slack discute; Jira registra.

---

## ADR

Canal: `#dev-geral`. Postado pelo Tech Lead (ou delegado).

```
📐 ADR-016 — Outbox pattern para eventos de domínio

Decisão: manter Outbox no monólito; sem message broker na Fase 1.
Doc: <link Confluence ou path no repo>
PR de referência: <link>
Dúvidas nesta thread.
```

---

## PR

Canal: `#dev-prs`. A maior parte chega **automaticamente** do GitHub — não duplique se o PR já foi notificado.

Para pedido manual de review urgente (exceção):

```
👀 Review urgente — PR #342 (EP-178)
Bloqueia merge do sprint. Fiscal + multi-tenant.
<link PR>
```

Preferência: pedir review na thread do PR no GitHub ou no canal da squad. Use `#dev-prs` só quando precisar de visibilidade extra.

---

## Release

Canal: `#releases`. Mensagem automática após deploy em produção — **não precisa responder**.

Formato de referência (gerado pelo pipeline):

```
🚀 Release em produção

Repo: epros-back
Versão: 2.4.1
Projetos: api, dfe
Workflow: <link>
```

Reação 👍 é opcional. Ninguém precisa confirmar leitura.

---

## Threads, menções e links

### Threads

| Contexto | Regra |
| --- | --- |
| Daily | Responder na thread do lembrete |
| Incidente | Abertura, updates e fechamento na mesma thread |
| Produto | Uma issue = uma thread |

### Menções

| Uso | Quando |
| --- | --- |
| `@here` | Bloqueio de sprint que precisa resposta em <1h |
| `@channel` | Evitar — só emergência P0 confirmada |
| DM | Assunto pessoal ou 1:1; decisão técnica fica no canal |

### Links obrigatórios

| Contexto | Linkar |
| --- | --- |
| Código | PR ou arquivo no GitHub |
| Requisito | Issue Jira (`EP-xxx`) |
| Decisão formal | ADR / Confluence |
| Incidente | Workflow run ou issue Sentry |

---

## Notificações pessoais

Ajuste o Slack ao seu ritmo — sem perder o que importa:

* **Mutar** `#dev-prs` se o volume incomodar; use keywords para seu login
* **Manter** `#epros-incidentes` **ativo** se você está de plantão
* `#releases` pode ficar em "mentions only"

---

## Segurança na comunicação

| Nunca cole | Alternativa |
| --- | --- |
| Tokens, `.env`, connection strings | Vault / secrets do time |
| JWT ou API keys | Referenciar o secret pelo nome, nunca o valor |
| CPF/CNPJ de clientes | Mascarar ou descrever sem dados reais |
| Screenshot de produção sem mascarar | Borrar tenant e dados sensíveis |

---

## Referência rápida

| Situação | Canal | Template neste artigo |
| --- | --- | --- |
| Status diário | `#daily` | Daily |
| Produção fora | `#epros-incidentes` | Incidente |
| Nova feature / backlog | `#epros-produto` | Produto |
| Decisão de arquitetura | `#dev-geral` | ADR |
| Review urgente | `#dev-prs` | PR |
| Deploy em produção | `#releases` | Release |

---

**Próximo passo →** [Fluxo de desenvolvimento — branches, ciclo e processo](10-fluxo-de-desenvolvimento.md)
