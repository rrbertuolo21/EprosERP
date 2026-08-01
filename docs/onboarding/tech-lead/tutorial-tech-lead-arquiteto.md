---
title: "Tutorial — Tech Lead / Arquiteto"
confluence_id: "200671233"
confluence_url: "https://rafaelbertuolo.atlassian.net/wiki/spaces/EprosWeb/pages/200671233/Tutorial+Tech+Lead+Arquiteto"
last_updated: "2026-07-13"
---

**O que você entrega:** garantir que o desenho respeita os padrões — breakdown estimado, tech design/ADR quando necessário, e aprovação final do PR.

**Índice:** [Tutoriais Dev Framework — uso por função (índice)](../indice-tutoriais.md)

**Operação semanal (Git/Jira/release):** [Rotina de segunda — Tech Lead](rotina-segunda-feira.md) — complementa o [fluxo de desenvolvimento (artigo 10)](../10-fluxo-de-desenvolvimento.md).

---

## Quando executar

| Gatilho | O que fazer |
| --- | --- |
| US aprovadas, início da sprint | Fase 05 Planning |
| Decisão técnica nova, spike ou desenho fora do padrão | Fase 06 Architect |
| Task simples que segue padrão existente | **Não precisa** de Architect — o padrão já é a decisão |
| PR aberto (Code Review Agent já rodou no diff) | Revisão final humana + gate do merge |
| Spike / dúvida técnica isolada | Architect → ADR → volta ao Planning |

---

## Pré-requisitos

* **Repositório:** abra o **EprosERP** no Cursor; para revisão, foque em `src/` ou `Epros.App/` conforme o PR.
* **Context Agent:** ativo automaticamente.
* **Artefatos:** US aprovadas com DoR ok (05); US + contexto técnico (06); diff do PR (gate).

---

## Passo a passo

### Fase 05 — Planning

1. Abra um **chat novo**.
2. Execute `/planning`.
3. Cole o prompt abaixo.
4. **Anexe** as User Stories aprovadas (fase 03).
5. Preencha velocity da sprint e restrições do time.
6. **Saída esperada:** breakdown em tasks estimadas (Fibonacci), por camada.
7. **Gate:** total ≤ velocity (cabe na sprint) ou replaneje.

**Prompt — Quebrar US em tasks:**

```
Quebre em tasks técnicas as US anexas de {submódulo}.

Use o método por camada: migration → domínio → handler → endpoint → front → testes.
Para cada task: descrição objetiva, estimativa Fibonacci, dependência e responsável sugerido.

Atenção:
- {a feature toca fiscal/DFe? sim/não} — se sim, aplique o multiplicador de complexidade oculta.
- Incerteza técnica real? Proponha spike com timebox, não estimativa chutada.

Feche com: pontuação total, velocity de referência ({X} pts/sprint), cabe na sprint?,
e a ordem de execução recomendada.
```

**→ Handoff:** tasks estimadas → Dev (07) ou Architect (06) se houver decisão pendente.

---

### Fase 06 — Architect (só quando precisa)

1. Abra um **chat novo**.
2. Execute `/architect`.
3. Cole o prompt abaixo.
4. **Anexe** a US, tasks e qualquer spike anterior.
5. Descreva a decisão ou o desenho em questão.
6. **Saída esperada:** tech design + **ADR** se houver decisão nova.
7. **Gate:** zero violação de padrão (tenancy, Outbox, hexagonal); spikes resolvidos.

> **Importante:** toda decisão nova que você tomar **vira ADR** e entra no registro de decisões do projeto.

**→ Handoff:** tech design aprovado → Dev (07).

---

### Gate do PR — revisão final (Tech Lead)

1. Confirme que o dev rodou o **Code Review Agent** — relatório publicado no PR (S23).
2. Revise o diff com foco em **negócio e arquitetura** — padrão e tenancy o Agent já checou.
3. Para re-rodar o Agent: execute `/code-review` e cole o prompt abaixo.
4. **Anexe** o diff (ou aponte o PR).
5. Aplique o [checklist de gate do PR](#checklist-de-gate-do-pr) abaixo.
6. **Gate:** zero bloqueante · decisão registrada · merge na `develop` só se aprovado (fluxo: [artigo 10](../10-fluxo-de-desenvolvimento.md)).

**Prompt — Code Review Agent (re-rodar):**

```
Revise o diff anexo do PR "{título}" ({submódulo/módulo}).

Aplique o checklist em todas as dimensões: padrões de código, multi-tenancy (bloqueante),
migrations/N+1, cobertura de testes, complexidade, nomenclatura, tratamento de erro.

Formato: resumo → bloqueantes (com correção) → avisos → sugestões →
checklist de cobertura → veredito (aprovado / corrigir bloqueantes).
```

---

### Checklist de gate do PR

O Code Review Agent valida **padrão, segurança óbvia, contratos técnicos (incl. PWA/Swagger), tenancy/Outbox, testes (feliz/erro/limite) e checks locais** — ver relatório no PR. Você valida **negócio, ACs e merge**.

#### Entendimento

- [ ] Li a task no Jira antes de abrir o código
- [ ] O implementado corresponde ao pedido e aos ACs
  _dúvida? comente no Jira antes de reprovar_

#### Gate humano — impedem merge

- [ ] Validei localmente seguindo o comentário de teste no Jira (quando aplicável)
- [ ] CI verde no PR (quando o workflow estiver configurado)
- [ ] Relatório do `/code-review` no PR — zero bloqueante pendente e checks locais ✅ (ou ⚠️ tooling/baseline documentado e aceito)
- [ ] ADR referenciada (se decisão nova)
- [ ] Guardião validou (se módulo FIN, VEN, EST ou feature fiscal)
- [ ] UX aprovado (se tela nova ou mudança de fluxo — front)

#### Avisos do Agent — não bloqueiam; registrar comentário no PR ou Jira se discordar ou aceitar com ressalva

- [ ] Comentários 🟡/🔵 do Agent revisados (legibilidade, duplicação, N+1, DS, etc.)

#### Decisão do Tech Lead

* **Aprovado** — autoriza merge na `develop`
* **Aprovado com ressalvas** — merge autorizado; registrar comentário na task no Jira
* **Bloqueado** — comentário no PR + task para **Rejeitado** no Jira (campos obrigatórios: [artigo 10](../10-fluxo-de-desenvolvimento.md))

#### Etiqueta do Tech Lead na revisão

* Critique o código, não o autor
* Diferencie sugestão de bloqueio — qualidade avisa; segurança e contratos (no relatório do Agent) bloqueiam
* PR grande demais → peça ao autor quebrar antes do merge
* Prazo: revisar até o fim do dia útil seguinte ao PR aberto

#### FAQ (Tech Lead)

**Posso aprovar PR em hotfix sem Agent?**
Não. Agent + sua revisão — hotfix não fura review.

**Sou responsável por bugs que passaram?**
Responsabilidade compartilhada — autor escreve, Agent checa padrão, você gateia merge.

**Auto-aprovação do próprio PR?**
Nunca — nem o autor faz merge; gate é sempre Tech Lead (ou TL designado).

---

## Seu gate (pronto quando…)

| Momento | Gate | Dono |
| --- | --- | --- |
| 05 Planning | cabe na sprint (≤ velocity) | Tech Lead |
| 06 Architect | zero violação de padrão; spikes resolvidos | Tech Lead |
| PR | zero bloqueante | Tech Lead |

---

## Erros comuns / dicas

| Evite | Prefira |
| --- | --- |
| Architect em toda task simples | Só quando há decisão nova ou desvio do padrão |
| Decidir o gate sem revisar o diff | Revisão humana após Code Review Agent |
| Decisão sem ADR | Toda decisão nova → ADR documentada |
| Aceitar violação de tenancy/Outbox | Bloqueante — corrigir antes do merge |
| Misturar Planning e Architect no mesmo chat | Chat novo por fase |
