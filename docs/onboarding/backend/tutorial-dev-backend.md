---
title: "Tutorial — Dev Backend"
confluence_id: "200704001"
confluence_url: "https://rafaelbertuolo.atlassian.net/wiki/spaces/EprosWeb/pages/200704001/Tutorial+Dev+Backend"
last_updated: "2026-07-13"
---

> [!NOTE]
> **O que você entrega:** implementar a feature no padrão da casa — código + testes, sem bloqueantes no PR.

**Índice:** [Tutoriais Dev Framework — uso por função (índice)](../indice-tutoriais.md)

---

## Quando executar

| Gatilho | O que fazer |
| --- | --- |
| Task quebrada pelo Planning, pronta para implementar | Fase 07 Dev — Prompt A |
| Refatoração necessária | Fase 07 Dev — Prompt B |
| Bug reportado (produção ou QA) | Fase 07 Dev — Prompt C |
| Dúvida de desenho durante a implementação | Consulta Architect — ver [Tutorial Tech Lead](../tech-lead/tutorial-tech-lead-arquiteto.md) |
| Antes de abrir o PR | Code Review Agent no diff |
| PR toca auth / dados sensíveis | Security Agent |
| Task toca fiscal | Validar com Fiscal Agent — ver [Tutorial Fiscal](../fiscal/tutorial-guardiao-fiscal.md) |

---

## Pré-requisitos

* **Repositório:** abra o **epros-back** no Cursor.
* **Context Agent:** ativo automaticamente.
* **Artefatos:** task + US com critérios + tech design (se houver) + spec do submódulo.

---

## Passo a passo

### Direto do Jira

1. Pegue a task no Jira.
2. Abra um **chat novo**.
3. Execute `/dev` + ID da task.
7. **Saída esperada:** código completo por arquivo + testes + nota de impactos.

### Implementar feature (dia a dia)

1. Pegue a task do Planning.
2. Abra um **chat novo**.
3. Execute `/dev`.
4. Cole o **Prompt A** abaixo.
5. Preencha ID da task, submódulo, schema e critérios de aceite.
6. **Anexe** task, US, tech design e spec do submódulo.
7. **Saída esperada:** código completo por arquivo + testes + nota de impactos.

**Prompt A — Implementar feature:**

```
Implemente a task: {ID e descrição}.

Submódulo: {código — ex: EST-SC-001} · Módulo/schema: {ex: estoque.*}
Critérios de aceite relevantes: {cole os Given/When/Then da US}

Base (backend): convenções de código, multi-tenancy, estrutura CQRS,
mapping/migration se houver, eventos se publicar/consumir,
gere os testes JUNTO — unit da entidade + integration do handler.
Conforme a natureza da task, acione também:
- Sincronização offline/PDV → sync offline
- Job agendado/worker → Quartz multi-tenant
- Toca fiscal → valide o fluxo e sinalize pontos para o Fiscal Agent.

Entregue: código completo por arquivo, testes, e a nota de impactos
(outros módulos afetados, migration incluída?, evento novo?).
```

**Ganchos conforme a task:**

| Tipo de task | Atenção extra |
| --- | --- |
| Sincronização offline / PDV | SyncId, SyncVersion, tombstones |
| Job agendado / worker | Quartz multi-tenant, idempotência |
| Toca fiscal | Validar com [Fiscal Agent](../fiscal/tutorial-guardiao-fiscal.md) |

---

### Refatorar

1. Chat novo → `/dev`.
2. Cole o **Prompt B** abaixo.

```
Refatore {arquivo/classe} que está {problema}. Mantenha o comportamento
(os testes atuais devem continuar verdes), siga as convenções,
e me mostre o diff conceitual antes do código final.
```

---

### Corrigir bug

1. Chat novo → `/dev`.
2. Cole o **Prompt C** abaixo.

```
Bug: {descrição + comportamento esperado vs atual}.
Reprodução: {passos ou teste que falha}. CorrelationId/log: {se houver}.
Encontre a causa raiz (não trate o sintoma), corrija seguindo as convenções,
e escreva o teste de regressão que teria pegado esse bug.
```

---

### Antes do PR — Code Review

1. Confira o [checklist do autor](../code-review-checklists-e-boas-praticas.md).
2. Chat novo → `/code-review`.
3. Cole o prompt de Code Review (ver [Tutorial Tech Lead](../tech-lead/tutorial-tech-lead-arquiteto.md)).
4. **Anexe** o diff.
5. Corrija todos os bloqueantes e avisos.
6. Se tocar **auth / dados sensíveis**, rode também o **Security Agent**:

```
Faça a revisão de segurança de {PR/spec/design anexo}.

Aplique OWASP no stack, dados pessoais/LGPD, secrets e auth (Authorize,
filtro de tenant, RBAC, payload do JWT). Atenção máxima a isolamento de tenant.

Formato: vulnerabilidade → severidade → OWASP/LGPD → localização → correção.
Crítica ou alta: marque como bloqueante de merge.
```

**→ Handoff:** build verde + testes passando → abra o PR (branch, Jira e destino `develop` conforme [Fluxo de desenvolvimento — artigo 10](../10-fluxo-de-desenvolvimento.md)) → Tech Lead faz revisão final.

---

## Seu gate (pronto quando…)

| Momento | Gate |
| --- | --- |
| Implementação | build verde + testes passando |
| Antes do PR | auto-review sem bloqueantes |
| Merge | Tech Lead aprova |

---

## Erros comuns / dicas

| Evite | Prefira |
| --- | --- |
| Pedir só "faça funcionar" | "Implemente conforme a spec, com testes, seguindo as convenções" |
| Abrir PR sem Code Review Agent | Sempre rodar no diff antes do PR |
| Código fiscal sem Fiscal Agent | Validar fluxo fiscal antes do merge |
| Resumir a spec de memória | Anexar task + US + spec |
| Continuar implementação e review no mesmo chat | Chat novo por fase/agente |
