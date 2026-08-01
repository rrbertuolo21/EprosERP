---
title: "Tutorial — Suporte / Migração"
confluence_id: "200835073"
confluence_url: "https://rafaelbertuolo.atlassian.net/wiki/spaces/EprosWeb/pages/200835073/Tutorial+Suporte+Migra+o"
last_updated: "2026-07-13"
---

**O que você entrega:** operar tickets fora da esteira e executar migrações de clientes legados — trazendo aprendizado de volta para a fábrica.

**Índice:** [Tutoriais Dev Framework — uso por função (índice)](../indice-tutoriais.md)

---

## Quando executar

### Suporte

| Gatilho | O que fazer |
| --- | --- |
| Ticket de cliente chegou | Support Agent — triagem |
| Triagem concluiu: é bug | Alimentar esteira como hotfix/melhoria |
| Triagem concluiu: é config/dúvida | Resolver e documentar; não entra na esteira |
| Pós-corte de migração | Support monitora por 2 semanas |

### Migração

| Gatilho | O que fazer |
| --- | --- |
| Novo cliente legado no Bloco 7 | Migration Agent — plano do cliente |
| Plano aprovado | Sequência: staging → janela → convivência → corte |

---

## Pré-requisitos

* **Repositório:** abra o **EprosERP** no Cursor.
* **Context Agent:** ativo automaticamente.
* **Suporte:** ticket, logs, tenant identificado.
* **Migração:** dados do cliente legado, mapeamento de módulos, janela de corte.

---

## Passo a passo — Suporte

### Triagem de ticket

1. Abra um **chat novo**.
2. Execute `/support`.
3. Cole o prompt abaixo.
4. **Anexe** o ticket, prints, logs ou CorrelationId.
5. Preencha tenant, módulo e sintoma.
6. Siga o roteiro: reproduzir → classificar → identificar tenant → decidir (bug / config / dúvida).
7. **Saída esperada:** classificação + próximo passo (resolver, escalar ou entrar na esteira).

```
Ticket #{id} do cliente {nome} (tenant {id}): "{texto do cliente, colado na íntegra}".
Módulo aparente: {se souber}.

Aplique o roteiro de triagem:
1. O que precisamos para reproduzir? (se já dá, descreva a reprodução)
2. Severidade pela matriz (impacto na operação do cliente)
3. Bug, configuração ou dúvida de uso?
4a. Config/dúvida → resposta pronta para o cliente
4b. Bug → pacote de escalonamento completo: reprodução, tenant, CorrelationId,
    evidências, severidade
```

---

### Ticket virou bug

1. Documente a triagem.
2. Alimente a esteira como **hotfix** ou **melhoria** (detalhe em [Fluxo de desenvolvimento — artigo 10](../10-fluxo-de-desenvolvimento.md)):

    * **Hotfix:** Support tria → Dev corrige → Code Review → Ops deploy → QA regressão → Docs changelog → QA adiciona edge case ao catálogo. Merge obrigatório em `homolog` e `develop` após correção.
    * **Melhoria pequena:** Requirements → Planning → Dev → Code Review → QA → Ops.

**→ Handoff:** bug entra na esteira com contexto da triagem anexado.

---

## Passo a passo — Migração

### Plano do cliente

1. Abra um **chat novo**.
2. Execute `/migration`.
3. Cole o prompt abaixo.
4. Informe cliente, módulos no escopo e volume de dados.
5. **Saída esperada:** plano com de-para long → Guid, ETL por módulo, conciliação de saldos, migração de XMLs históricos.

```
Vamos migrar o cliente {nome/id} do Epros legado para a nova plataforma.
Perfil: {porte, módulos que usa no legado, volume aproximado de dados, sazonalidade}.
Gere o plano: escopo por módulo → de-para necessário → janela proposta →
validações → critérios de corte → rollback.
Este é o cliente nº {N} — incorpore os aprendizados das migrações anteriores.
```

---

### Sequência de execução

1. **Ensaio em staging** — ETL completo, sem corte.
2. **Janela** — execução em ambiente controlado.
3. **Convivência** — novo ativo, legado read-only.
4. **Gate:** saldos batem (financeiro + estoque) — bloqueante para o corte.
5. **Corte** — desliga legado, ativa produção.
6. **Support monitora 2 semanas** pós-corte.

**→ Handoff:** cliente em produção estável → Support assume monitoramento.

---

## Seu gate (pronto quando…)

| Fluxo | Gate |
| --- | --- |
| Suporte — triagem | classificação clara + tenant identificado + próximo passo definido |
| Suporte — bug | entrada na esteira com contexto completo |
| Migração | saldos batem antes do corte |
| Pós-corte | 2 semanas de monitoramento sem P0 |

---

## Erros comuns / dicas

| Evite | Prefira |
| --- | --- |
| Abrir bug na esteira sem triagem | Sempre Support Agent primeiro |
| Corte sem conciliação de saldos | Gate bloqueante: saldos devem bater |
| Resolver ticket sem identificar tenant | Tenant é obrigatório na triagem |
| Migração sem ensaio em staging | Sempre ensaio antes da janela |
| Bug P0 sem edge case no catálogo | QA deve adicionar após hotfix |
