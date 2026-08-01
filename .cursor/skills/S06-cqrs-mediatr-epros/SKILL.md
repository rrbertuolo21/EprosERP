---
name: S06-cqrs-mediatr-epros
description: >-
  Padrão CQRS do Epros com MediatR 12: anatomia completa de uma feature (Command → FluentValidation → Handler → entidade Flunt → CommandResult → Controller fino), pipeline behaviors e validação em duas camadas. Use ao criar qualquer feature, command, query, handler ou endpoint novo no backend — inclui pasta-template completa para copiar.
---

# cqrs-mediatr-epros

> **S06 · Camada 1 — Engenharia** — Epros Dev Framework

## Quando usar

Ative esta skill quando a tarefa envolver: **criar feature, novo command, novo handler, nova query, endpoint novo, MediatR, FluentValidation, Flunt, pipeline behavior**.

## O que esta skill cobre

A anatomia completa de uma feature no padrão Epros: Command → Validator (FluentValidation) → Handler (MediatR) → Entidade (Flunt) → CommandResult → Controller fino, incluindo pipeline behaviors e a divisão de responsabilidade entre validação de comando e validação de domínio.

Fazer com que toda feature nova nasça com a mesma estrutura, testável e sem lógica vazando para o controller — e reduzir o tempo de onboarding: o dev copia a pasta-template e preenche.

## Instruções para o agente

1. Para feature nova, comece SEMPRE pela pasta templates/feature-completa/ — copie os 6 arquivos e renomeie.
2. Validação de formato/obrigatoriedade → FluentValidation no Command. Regra de negócio → Flunt na Entidade. Nunca misture.
3. Controller: recebe request, despacha via MediatR, traduz CommandResult em HTTP. NADA além disso.
4. Query e Command são separados: query não muda estado, usa AsNoTracking e retorna DTO.

## Recursos desta skill

> Legenda: ✅ pronto · ⬜ a construir (ver "Como completar" abaixo)

- ✅ `SKILL.md` — este arquivo (semente v1)
- ⬜ `templates/feature-completa/` — os 6 arquivos de uma feature exemplo prontos para copiar
- ⬜ `exemplos/handler-antes-depois.md` — handler gordo refatorado passo a passo

## Como completar esta skill (do v1-semente à versão completa)

1. Escolha a melhor feature existente (ex: BaixarContaPagar) como referência canônica.
2. Generalize-a em templates com placeholders claros ({{Entidade}}, {{Acao}}).
3. Documente os pipeline behaviors existentes (logging, validação, transação) e quando criar um novo.
4. Escreva o antes/depois usando um handler real que foi refatorado.

## Regras de manutenção

- Detalhe profundo vai para `exemplos/` e `checklists/` — este arquivo fica abaixo de 500 linhas.
- Todo conteúdo deve ser específico do Epros (código real, casos reais, dados de homologação).
- Ao concluir os recursos, mude `status:` para `completa` e atualize a data de revisão.
