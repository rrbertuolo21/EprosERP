---
title: "Tutorial — PO / Facilitador"
confluence_id: "200638465"
confluence_url: "https://rafaelbertuolo.atlassian.net/wiki/spaces/EprosWeb/pages/200638465/Tutorial+PO+Facilitador"
last_updated: "2026-07-13"
---

**O que você entrega:** transformar demanda em User Story pronta, com critérios de aceite testáveis e DoR ok.

**Índice:** [Tutoriais Dev Framework — uso por função (índice)](../indice-tutoriais.md)

---

## Quando executar

| Gatilho | O que fazer |
| --- | --- |
| Demanda nova (cliente, interno, prospecção) | Fase 01 Strategy |
| Go aprovado pela liderança | Fase 02 Discovery |
| Problem Statement pronto | Fase 03 Requirements |
| Melhoria pequena, sem discovery | Pule direto para Requirements (fluxo curto) |
| Time precisa de requisito claro | Requirements (revisar ou criar US) |

---

## Pré-requisitos

* **Repositório:** abra o **EprosERP** no Cursor.
* **Context Agent:** ativo automaticamente em todo chat.
* **Artefatos:** descrição da demanda (01); notas/entrevistas (02); Problem Statement (03).
* **Skills que o agente consulta:** business case, discovery, US + ACs, impacto fiscal.

---

## Passo a passo

### Fase 01 — Strategy

1. Abra um **chat novo**.
2. Execute `/strategy`.
3. Cole o **Prompt A** abaixo (ou Prompt B para comparar demandas).
4. Preencha os campos entre chaves.
5. **Anexe** a descrição da demanda (e-mail, nota de reunião).
6. **Saída esperada:** Business Case com go/no-go, OKR vinculado, esforço e riscos.
7. **Gate humano:** leve o go/no-go para a liderança decidir. Sem go, não avance.

**Prompt A — Avaliar demanda nova:**

```
Analise esta demanda e produza o Business Case.

DEMANDA: {descreva em 2-5 linhas o que foi pedido e por quem}
SOLICITANTE: {cliente X / interno / prospecção comercial}
URGÊNCIA DECLARADA: {prazo mencionado, se houver}

Considere:
- Roadmap atual: estamos no Bloco {N} ({o que está em andamento}).
- Cruze com o inventário: isso já existe como submódulo mapeado? Qual código?
- OKRs vigentes — a qual objetivo isso se vincula?

Quero: resumo executivo, viabilidade, impacto no roadmap, OKR, esforço (A/M/B),
top 3 riscos com mitigação, e recomendação go/no-go com próximo passo.
```

**→ Handoff:** com go aprovado → Fase 02 Discovery.

---

### Fase 02 — Discovery

1. Abra um **chat novo** (não reutilize o do Strategy).
2. Execute `/discovery`.
3. Cole o **Prompt A** abaixo.
4. Preencha os campos com o contexto da demanda aprovada.
5. **Anexe** as notas de entrevistas, gravações transcritas ou materiais de discovery.
6. **Saída esperada:** Problem Statement validado (personas, JTBD, problema central).
7. **Gate:** Problem Statement validado pelo PO.

**Prompt A — Sintetizar entrevistas:**

```
Analise as {N} entrevistas anexas sobre {tema}.

Contexto: Business Case aprovado para {submódulo/demanda}, anexo.

Quero a síntese:
1. Padrões de dor agrupados por frequência × severidade, com citações como evidência
2. Causa raiz provável por padrão (aplique os 3 porquês)
3. Personas: enriqueça as existentes; só crie persona nova se não houver correspondente
4. JTBDs no formato padrão
5. Gaps de informação e perguntas para a próxima rodada

{N} < 5? Trate tudo como hipótese e diga explicitamente o que falta para concluir.
```

**→ Handoff:** Problem Statement → Fase 03 Requirements.

---

### Fase 03 — Requirements

1. Abra um **chat novo**.
2. Execute `/requirements`.
3. Cole o **Prompt A** abaixo.
4. **Anexe** o Problem Statement da fase anterior.
5. Preencha os campos (submódulo, personas, restrições).
6. **Saída esperada:** User Stories com critérios **Given/When/Then**.
7. Antes de passar adiante, rode o check de **DoR**:

    * Sem termo vago ("rápido", "fácil" → quantificado)
    * Impacto fiscal respondido
    * Impacto multi-tenancy respondido

8. Se a feature for fiscal, sinalize para o [Guardião Fiscal](../fiscal/tutorial-guardiao-fiscal.md) validar na spec.

**Prompt A — Gerar User Stories:**

```
Gere as User Stories para {submódulo} a partir do material anexo.

Para CADA US:
- Persona da biblioteca: {ex: gestor de compras, operador de estoque}
- Critérios Given/When/Then testáveis
- NFRs herdados pelo tipo de feature — inclua explicitamente
- Dependências: cruze com módulos/eventos existentes
- Responda SEMPRE: qual o impacto fiscal? qual o impacto de multi-tenancy?

Regras:
- Detectou termo vago ("rápido", "fácil")? Pare e me pergunte a especificação objetiva.
- US maior que uma sprint de 2 semanas: entregue já quebrada.
- Feature toca documento fiscal? Sinalize os pontos que precisam validação do Fiscal Agent.
```

**→ Handoff:** US com ACs testáveis + DoR ok → entra na esteira (Planning).

---

## Seu gate (pronto quando…)

| Fase | Gate | Dono |
| --- | --- | --- |
| 01 Strategy | go aprovado + OKR vinculado | Liderança |
| 02 Discovery | Problem Statement validado | PO |
| 03 Requirements | DoR: sem termo vago, fiscal+tenancy respondidos | PO |

**Entrega final:** US com ACs testáveis + DoR ok → Planning (Tech Lead).

---

## Erros comuns / dicas

| Evite | Prefira |
| --- | --- |
| Pular Strategy em demanda grande | Strategy sempre no começo |
| Resumir entrevistas de memória | Anexar as notas completas |
| US com termos vagos | Quantificar no DoR |
| Esquecer impacto fiscal/tenancy | Responder no prompt de Requirements |
| Continuar no mesmo chat entre fases | Chat novo por fase |

> **Dica:** melhoria pequena sem discovery? Vá direto para Requirements (fluxo curto no [índice](../indice-tutoriais.md)).
