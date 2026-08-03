---
name: S23-code-review-epros
description: >-
  Code review estruturado do Epros: severidades bloqueante/aviso/sugestão, checklist por dimensão (multi-tenancy, testes, complexidade, performance, segurança) e reviews reais anotados. Use ao revisar qualquer PR ou diff — referencia as skills de convenções, tenancy, testes e segurança em vez de duplicá-las.
---

# code-review-epros

> **S23 · Camada 2 — Fases de produto** — Epros Dev Framework

## Quando usar

Ative esta skill quando a tarefa envolver: **review, PR, pull request, revisar código, diff, aprovar, merge**.

## O que esta skill cobre

O método de code review do Epros: severidades (bloqueante/aviso/sugestão), o checklist por dimensão (padrões, multi-tenancy, testes, complexidade, performance, segurança) e exemplos anotados de reviews reais mostrando como apontar problema COM a correção.

Padronizar a régua de review antes da revisão final do Tech Lead: violação de tenant sempre bloqueia, PR sem teste de lógica nova sempre avisa, e toda crítica vem com orientação — review eleva o time, não humilha.

## Instruções para o agente

1. Estruture o review no formato: resumo → bloqueantes (🔴) → avisos (🟡) → sugestões (🔵) → cobertura → veredito.
2. Referencie as skills-fonte em vez de reexplicar: multi-tenancy → S03, convenções → S02, segurança → S14, testes → S10.
3. Todo item bloqueante DEVE vir com sugestão de correção.
4. Violação de multi-tenancy: bloqueante sem exceção. Lógica de negócio nova sem teste: aviso por padrão.
5. PR pequeno (< 100 linhas): use o checklist rápido de 5 minutos.

## Recursos desta skill

> Legenda: ✅ pronto · ⬜ a construir (ver "Como completar" abaixo)

- ✅ `SKILL.md` — este arquivo (semente v1)
- ⬜ `exemplos/reviews-anotados/` — reviews reais comentados: o problema, o comentário ideal, o porquê
- ⬜ `checklists/review-rapido.md` — versão 5 minutos para PRs pequenos

## Como completar esta skill (do v1-semente à versão completa)

1. Mova formato e severidades do prompt do agente para a skill.
2. Selecione 3 PRs reais (com violação de tenant, sem testes, com N+1) e anote os reviews ideais.
3. Derive o checklist rápido do completo.

## Regras de manutenção

- Detalhe profundo vai para `exemplos/` e `checklists/` — este arquivo fica abaixo de 500 linhas.
- Todo conteúdo deve ser específico do Epros (código real, casos reais, dados de homologação).
- Ao concluir os recursos, mude `status:` para `completa` e atualize a data de revisão.
