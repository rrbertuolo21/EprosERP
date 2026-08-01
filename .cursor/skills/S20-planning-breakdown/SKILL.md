---
name: S20-planning-breakdown
description: >-
  Quebra técnica e estimativa do Epros: decomposição por camada, calibração Fibonacci com histórico real do time, velocity de referência, política de spikes e heurística de complexidade oculta fiscal/DFe. Use ao quebrar épicos ou US em tasks, estimar, planejar sprints ou avaliar se o escopo cabe.
---

# planning-breakdown

> **S20 · Camada 2 — Fases de produto** — Epros Dev Framework

## Quando usar

Ative esta skill quando a tarefa envolver: **quebrar épico, estimar, pontos, planning, sprint, task, velocity, spike, refinamento**.

## O que esta skill cobre

O método de quebra técnica e estimativa: decomposição por camada (migration → domínio → handler → endpoint → front → testes), calibração Fibonacci com exemplos históricos reais do Epros ('3 pontos = CRUD simples com testes'), a velocity de referência do time e a heurística de complexidade oculta fiscal/DFe.

Tornar as estimativas comparáveis e realistas: todo mundo calibra pelo mesmo histórico, tasks grandes são quebradas por regra objetiva (>8 pts ou >3 dias) e a complexidade fiscal escondida entra na conta antes de estourar a sprint.

## Instruções para o agente

1. Quebre por camada técnica usando o método — cada task estimável e testável isoladamente.
2. Estime por comparação com a tabela de referência histórica, não por intuição.
3. Task > 8 pontos ou > 3 dias: quebra imediata. Incerteza técnica real: proponha spike com timebox.
4. Feature fiscal ou de DFe: aplique o multiplicador de complexidade oculta e diga o porquê.
5. Compare o total com a velocity do time e responda objetivamente: cabe na sprint?

## Recursos desta skill

> Legenda: ✅ pronto · ⬜ a construir (ver "Como completar" abaixo)

- ✅ `SKILL.md` — este arquivo (semente v1)
- ⬜ `exemplos/breakdown-real-bloco5.md` — quebra real do PDV com estimativas vs realizado
- ⬜ `exemplos/referencia-estimativas.md` — 'o que é 1, 2, 3, 5, 8 pontos' com exemplos do Epros

## Como completar esta skill (do v1-semente à versão completa)

1. Reconstitua a quebra do Bloco 5 (PDV) com estimado vs realizado — vira o exemplo canônico.
2. Monte a tabela de referência de pontos com o time (cerimônia de calibração).
3. Registre a velocity histórica e o processo de atualização por sprint.

## Regras de manutenção

- Detalhe profundo vai para `exemplos/` e `checklists/` — este arquivo fica abaixo de 500 linhas.
- Todo conteúdo deve ser específico do Epros (código real, casos reais, dados de homologação).
- Ao concluir os recursos, mude `status:` para `completa` e atualize a data de revisão.
