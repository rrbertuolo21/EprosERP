---
name: S12-observabilidade-epros
description: >-
  Observabilidade do Epros: Serilog estruturado (TenantId, UserId, CorrelationId obrigatórios), OpenTelemetry, dashboards Grafana, queries Loki e métricas por módulo (fila Outbox, p95 por tenant, jobs atrasados). Use ao instrumentar features, escrever logs, investigar erros de produção, criar alertas ou dashboards.
---

# observabilidade-epros

> **S12 · Camada 1 — Engenharia** — Epros Dev Framework

## Quando usar

Ative esta skill quando a tarefa envolver: **log, logging, métrica, trace, Grafana, Loki, alerta, SLO, investigar erro, produção, instrumentar**.

## O que esta skill cobre

O guia de observabilidade da stack Serilog + OpenTelemetry → Grafana/Prometheus/Loki/Tempo: campos obrigatórios de log (TenantId, UserId, CorrelationId), o que NUNCA logar, como instrumentar handler/job novo e as métricas que importam por módulo (fila da Outbox, latência p95 por tenant, jobs atrasados).

Fazer produção ser visível ANTES do incidente: toda feature nasce instrumentada, todo log é rastreável até o tenant e a investigação de erro segue um caminho conhecido no Grafana em vez de caça às cegas.

## Instruções para o agente

1. Log estruturado sempre: _logger.LogInformation("Descrição {Propriedade}", valor) com TenantId/CorrelationId no escopo.
2. NUNCA logue: CPF/CNPJ de pessoa física em claro, senhas, tokens, payload de certificado (consulte S14).
3. Feature nova = checklist de instrumentação: logs nos pontos de decisão, métricas de negócio, trace no handler.
4. Para investigar erro: comece pelas queries Loki úteis documentadas (por CorrelationId, por tenant, por evento).

## Recursos desta skill

> Legenda: ✅ pronto · ⬜ a construir (ver "Como completar" abaixo)

- ✅ `SKILL.md` — este arquivo (semente v1)
- ⬜ `checklists/instrumentacao-nova-feature.md` — o que instrumentar antes do merge
- ⬜ `exemplos/queries-loki-uteis.md` — as consultas que resolvem 80% das investigações
- ⬜ `templates/dashboard-modulo.json` — dashboard Grafana padrão por módulo

## Como completar esta skill (do v1-semente à versão completa)

1. Documente o enrichment atual do Serilog e padronize os campos obrigatórios.
2. Colete as queries Loki que o time já usa e cure as melhores.
3. Crie o dashboard-modelo a partir do módulo mais maduro e exporte o JSON.
4. Defina as métricas de negócio mínimas por módulo com o Ops.

## Regras de manutenção

- Detalhe profundo vai para `exemplos/` e `checklists/` — este arquivo fica abaixo de 500 linhas.
- Todo conteúdo deve ser específico do Epros (código real, casos reais, dados de homologação).
- Ao concluir os recursos, mude `status:` para `completa` e atualize a data de revisão.
