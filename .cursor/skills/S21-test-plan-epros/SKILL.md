---
name: S21-test-plan-epros
description: >-
  Planejamento de testes do Epros: planos priorizados por risco e o catálogo vivo de edge cases fiscais/financeiros/multi-tenant por módulo (tenant sem config emitindo NF-e, produto sem NCM, certificado expirado...). Use ao criar planos de teste, analisar cobertura, priorizar cenários ou converter bugs de produção em testes.
---

# test-plan-epros

> **S21 · Camada 2 — Fases de produto** — Epros Dev Framework

## Quando usar

Ative esta skill quando a tarefa envolver: **plano de teste, caso de teste, edge case, cenário, cobertura, priorizar teste, QA, regressão**.

## O que esta skill cobre

O método de planejamento de testes do Epros: formato de plano com casos priorizados por risco, a separação automatizável/manual, e o ativo mais valioso do QA — o catálogo vivo de edge cases por módulo, que cresce a cada bug de produção (todo P0 vira edge case catalogado).

Garantir que os cenários que derrubam ERP fiscal (tenant sem configuração emitindo NF-e, produto sem NCM, certificado expirado, lançamento retroativo) sejam testados por padrão, não por sorte — e que falha fiscal/financeira seja sempre P0.

## Instruções para o agente

1. Plano de teste: casos com pré-condição/ação/resultado, tipo (unit/integration/E2E) e prioridade por risco.
2. Consulte SEMPRE o catálogo de edge cases do módulo — os cenários lá são obrigatórios no plano.
3. Falha com impacto fiscal ou financeiro = P0 por definição. Priorize acima de tudo.
4. Dados de teste realistas: CNPJs, NCMs e CFOPs de homologação (S04/S10).
5. Ao analisar bug de produção: proponha o novo edge case para o catálogo — é assim que ele cresce.

## Recursos desta skill

> Legenda: ✅ pronto · ⬜ a construir (ver "Como completar" abaixo)

- ✅ `SKILL.md` — este arquivo (semente v1)
- ⬜ `exemplos/catalogo-edge-cases.md` — edge cases por módulo — ativo vivo, alimentar sempre
- ⬜ `templates/plano-testes.md` — template do plano

## Como completar esta skill (do v1-semente à versão completa)

1. Mova os 8 edge cases do prompt do QA Agent para o catálogo, organizando por módulo.
2. Varra os bugs históricos do legado e converta os relevantes em edge cases.
3. Formalize a regra: post-mortem de P0 inclui atualização do catálogo.

## Regras de manutenção

- Detalhe profundo vai para `exemplos/` e `checklists/` — este arquivo fica abaixo de 500 linhas.
- Todo conteúdo deve ser específico do Epros (código real, casos reais, dados de homologação).
- Ao concluir os recursos, mude `status:` para `completa` e atualize a data de revisão.
