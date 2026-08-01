---
name: S30-quartz-jobs-epros
description: >-
  Jobs em background com Quartz.NET no Epros multi-tenant: escopo de DI por tenant (a armadilha nº 1 de vazamento em jobs), idempotência, retries e monitoramento de atraso. Use ao criar ou revisar qualquer job, rotina agendada, processador de fila ou processamento em lote.
---

# quartz-jobs-epros

> **S30 · Camada 3 — Especialização** — Epros Dev Framework

## Quando usar

Ative esta skill quando a tarefa envolver: **job, Quartz, agendamento, background, processamento em lote, rotina noturna, scheduler, Outbox processor**.

## O que esta skill cobre

Jobs em background com Quartz.NET 3.x (ADR-015) no contexto multi-tenant: o padrão de job com escopo de tenant correto (a armadilha nº 1 de vazamento — job roda fora do request e o ITenantProvider scoped não existe), idempotência de execução, retry policies e monitoramento de jobs atrasados.

Fechar o maior ponto cego do multi-tenancy: jobs que iteram tenants precisam criar escopo por tenant explicitamente — e um job travado (ex: processador da Outbox) precisa disparar alerta antes do cliente perceber.

## Instruções para o agente

1. Job multi-tenant: use o template — cria um escopo de DI POR TENANT dentro do loop. Nunca resolva ITenantProvider fora do escopo.
2. Todo job deve ser idempotente: rodar duas vezes (retry, redeploy) não pode duplicar efeito.
3. Job novo = checklist: escopo de tenant, idempotência, métrica de última execução, alerta de atraso.
4. Job de efeito fiscal/financeiro: log estruturado com TenantId em cada iteração para auditoria.

## Recursos desta skill

> Legenda: ✅ pronto · ⬜ a construir (ver "Como completar" abaixo)

- ✅ `SKILL.md` — este arquivo (semente v1)
- ⬜ `templates/job-multitenant.cs` — job padrão com escopo por tenant
- ⬜ `checklists/job-novo.md` — checklist de criação de job

## Como completar esta skill (do v1-semente à versão completa)

1. Documente o processador da Outbox existente como exemplo canônico de job multi-tenant.
2. Escreva o template generalizando o padrão de escopo por tenant.
3. Adicione as métricas de job (última execução, duração, atraso) ao dashboard do S12.

## Regras de manutenção

- Detalhe profundo vai para `exemplos/` e `checklists/` — este arquivo fica abaixo de 500 linhas.
- Todo conteúdo deve ser específico do Epros (código real, casos reais, dados de homologação).
- Ao concluir os recursos, mude `status:` para `completa` e atualize a data de revisão.
