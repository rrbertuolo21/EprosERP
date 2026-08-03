---
title: "Tutorial — QA / SDET"
confluence_id: "200769537"
confluence_url: "https://rafaelbertuolo.atlassian.net/wiki/spaces/EprosWeb/pages/200769537/Tutorial+QA+SDET"
last_updated: "2026-07-13"
---

**O que você entrega:** provar que funciona antes de ir pro ar — plano de testes, regressão e edge cases no catálogo.

**Índice:** [Tutoriais Dev Framework — uso por função (índice)](../indice-tutoriais.md)

---

## Quando executar

| Gatilho | O que fazer |
| --- | --- |
| Build de release pronto para validação | Fase 08 QA |
| Bug P0/P1 em produção | Plano de regressão + edge case no catálogo |
| Cenário fiscal em dúvida | Consulta Fiscal Agent |
| Após hotfix deployado | Regressão obrigatória (fluxo curto) |

---

## Pré-requisitos

* **Repositório:** abra o **EprosERP** no Cursor (`src/` para API/módulos; `EprosApp/` se testar UI).
* **Context Agent:** ativo automaticamente.
* **Artefatos:** critérios de aceite da US; build/release notes; cenários fiscais se aplicável.

---

## Passo a passo

### Fase 08 — Plano de teste e execução

1. Abra um **chat novo**.
2. Execute `/qa`.
3. Cole o **Prompt A** abaixo.
4. **Anexe** os critérios de aceite (Given/When/Then) da US.
5. Preencha submódulo, release e escopo.
6. **Saída esperada:**

    * plano de testes priorizado por risco
    * edge cases do catálogo para verificar — ex.: tenant sem config emitindo NF-e, produto sem NCM, certificado expirado

7. Execute os cenários, priorizando **fiscais** e **multi-tenancy** — são os que mais quebram.

**Prompt A — Plano de testes:**

```
Gere o plano de testes para {feature/submódulo} a partir dos critérios anexos.

- Casos priorizados por risco (fiscal/financeiro = P0 por definição)
- OBRIGATÓRIO: inclua todos os edge cases catalogados de {módulo}
- Cenários de multi-tenancy: usuário do tenant A não vê dados do tenant B
- Separe: unit / integration / E2E / manual
- Dados de teste: use os de homologação — especifique quais

Feche com a cobertura estimada e os gaps que sobram.
```

---

### Cenário fiscal em dúvida

1. Chat novo → `/fiscal`.
2. Consulte o [Tutorial — Guardião de Domínio (Fiscal)](../fiscal/tutorial-guardiao-fiscal.md).
3. Informe UF, regime e contexto do cenário de teste.

---

### Bug de produção → regressão + catálogo

1. Após o Dev corrigir, rode regressão no cenário que quebrou.
2. Abra o **QA Agent** e peça o **teste de regressão** formalizado.
3. **Obrigatório:** adicione o edge case ao **catálogo** — é o que faz a fábrica melhorar.
4. Se P0, também alimente runbook conforme o [fluxo de hotfix](../10-fluxo-de-desenvolvimento.md#hotfix--fluxo-paralelo).

**Prompt C — Bug de produção:**

```
Bug P{0/1} encontrado em produção: {descrição}. Já corrigido no PR {link}.
1. Redija o edge case para o catálogo (módulo {X}), no formato padrão.
2. Proponha o teste de regressão automatizado se ainda não existir.
3. Esse cenário sugere runbook novo? Se sim, esboce.
```

**→ Handoff:** zero P0/P1 → Ops (09) para go-live.

---

## Seu gate (pronto quando…)

| Momento | Gate | Dono |
| --- | --- | --- |
| 08 QA | zero P0/P1; cenários fiscais e de tenancy verdes | QA |
| Pós-hotfix | regressão verde + edge case adicionado ao catálogo | QA |

---

## Erros comuns / dicas

| Evite | Prefira |
| --- | --- |
| Testar só o happy path | Priorizar risco: fiscal + multi-tenancy primeiro |
| Corrigir bug sem regressão | Sempre teste de regressão + edge case no catálogo |
| Dúvida fiscal sem Fiscal Agent | Consultar antes de marcar cenário como ok |
| Aceitar P1 aberto no release | Gate é zero P0/P1 |
| Resumir ACs de memória | Anexar a US com Given/When/Then |
