---
title: "Trilha Tech Lead — ADRs, fases e guardião de domínio"
confluence_id: "193495041"
confluence_url: "https://rafaelbertuolo.atlassian.net/wiki/spaces/EprosWeb/pages/193495041/Trilha+Tech+Lead+ADRs+fases+e+guardi+o+de+dom+nio"
last_updated: "2026-07-06"
---

> [!NOTE]
> **O que você vai aprender:** responsabilidades do Tech Lead no Epros — ADRs, as 9 fases de desenvolvimento com gates, aprovação de PRs e coordenação com o Guardião de Domínio.

Leia **todos os artigos base** (00–10) antes desta trilha.

> [!TIP]
> Use as trilhas por função (R1–R4) como referência do que o time está estudando.

---

## Papel do Tech Lead

| Foco | Detalhe |
| --- | --- |
| Arquitetura | Manter coerência do monólito modular |
| Decisões | ADRs para toda mudança relevante de stack ou padrão |
| Qualidade | Aprovação de PRs de alto risco; lead time e cobertura |
| **Não faz mais** | Organizar Jira sozinho, priorizar backlog, planning isolado |

### Agentes IA do Tech Lead

* **Architect Agent** — antes de features novas
* **Code Review Agent** — validação de padrão (time executa; TL revisa lógica)
* **Security Agent** — endpoints fiscais e autenticação

### Métricas de sucesso

* Lead time de PR
* Cobertura de código
* Zero P0 em produção
* 100% PRs com Code Review Agent no comentário

---

## As 20 ADRs — decisões fechadas

O Epros mantém **20 ADRs**: **15 técnicas** (stack e padrões) + **5 de fronteira de módulos** (contratos entre bounded contexts). Toda tecnologia relevante tem ADR.

> [!IMPORTANT]
> Reverter qualquer ADR exige ADR nova aprovada — sem exceção.

As **15 ADRs técnicas** (resumo):

| ADR | Decisão | Motivo resumido |
| --- | --- | --- |
| 001 | EF Core vs Dapper | QueryFilter automático multi-tenant |
| 002 | Keycloak vs Auth0 | Open source, on-premise |
| 003 | Valkey vs Redis | BSL do Redis em 2024 |
| 004 | Vault vs Azure KV | Cloud-agnostic |
| 005 | OpenTofu vs Terraform | BSL do Terraform em 2023 |
| 006 | PostgreSQL vs SQL Server | Open source, RLS nativo |
| 007 | Nuxt 4 vs Blazor | Uma stack para 3 superfícies |
| 008 | Electron vs Tauri | Reutiliza 100% do Nuxt |
| 009 | Capacitor vs React Native | Mesmo código web no mobile |
| 010 | Guid vs long | Segurança e distribuição |
| 011 | SyncId+SyncVersion vs timestamp | Conflitos offline sem clock skew |
| 012 | QueryFilter vs RLS puro | Flexibilidade + segunda barreira opcional |
| 013 | Caddy vs Nginx | HTTPS automático, config simples |
| 014 | Hercules.NET vs alternativas | Homologada em produção |
| 015 | Quartz.NET vs Hangfire | Clustering nativo com PostgreSQL |

As **5 ADRs de fronteira** (contratos entre módulos) estão na skill **S05**.

Lista canônica com alternativas descartadas: [CONTEXT.md §12](../../CLAUDE.md) · skill [S05](../../.cursor/skills/S05-epros-adrs/SKILL.md).

---

## As 9 fases de desenvolvimento

Cada fase tem agente, entregáveis e **gate** — nenhuma avança sem aprovação. Detalhe completo: [PIPELINE.md](../fabrica/processo/PIPELINE.md).

| Fase | Nome | Agente | Gate |
| --- | --- | --- | --- |
| 01 | Estratégia | Strategy | Go aprovado + OKR vinculado |
| 02 | Discovery | Discovery | Problem Statement validado (≥5 entrevistas ou risco assumido) |
| 03 | Requisitos | Requirements | DoR (S18): sem termo vago; fiscal + tenancy respondidos |
| 04 | Design UX | UX | Aprovado p/ dev (consistência + WCAG + confirmações fiscais) |
| 05 | Refinamento | Planning | Cabe na sprint (total ≤ velocity) ou replanejado |
| 06 | Arquitetura | Architect | Zero violação de padrão; spikes resolvidos *(pode pular se padrão existente)* |
| 07 | Desenvolvimento | Dev | Build verde + testes passando |
| 08 | Qualidade | QA | Zero P0/P1; cenários fiscais e de tenancy verdes |
| 09 | Operações | Ops | Checklist go-live 100% + rollback testado |

### Quando escalar para Architect Agent

> [!WARNING]
> Acione o Architect Agent antes de: novo bounded context, integração externa, mudança de schema compartilhado ou qualquer uso de `IgnoreQueryFilters()`.

---

## Coordenação com Guardião de Domínio

O Guardião (dev com 10 anos de experiência fiscal/ERP) valida **regras de negócio** antes do merge.

| Tech Lead | Guardião |
| --- | --- |
| Padrão de código, arquitetura | Regra fiscal, contábil, ERP |
| ADRs técnicos | Glossário de domínio por módulo |
| Aprovação de PR (técnico) | Aprovação de PR (negócio) |

> [!TIP]
> **Ritual sugerido:** toda US de módulo FIN, VEN ou EST passa pelo Guardião no refinamento (qui) e na revisão pré-merge.

---

## Aprovação de PR — checklist de gate

Sequência: dev roda **Code Review Agent** → você aplica o gate abaixo → merge na `develop`.

Checklist completo (passo a passo + decisão): [Tutorial Tech Lead — gate do PR](tech-lead/tutorial-tech-lead-arquiteto.md#checklist-de-gate-do-pr).

Resumo:

- [ ] Code Review Agent no PR — zero bloqueante pendente
- [ ] Task Jira vs implementação — ACs atendidos
- [ ] CI verde · multi-tenancy · contratos retrocompatíveis
- [ ] ADR / OpenAPI / Guardião quando aplicável
- [ ] Decisão registrada: aprovar · aprovar com ressalvas · bloquear (Jira Rejeitado)

O Code Review Agent cuida de **padrão**. Você cuida de **negócio, contratos e autorização de merge**.

---

**Trilha Tech Lead concluída.**

**Próximo passo →** [Tutorial — Tech Lead / Arquiteto](tech-lead/tutorial-tech-lead-arquiteto.md) — passo a passo hands-on de Planning, Architect e gate do PR no Cursor.

**Operação semanal →** [Rotina de segunda — Tech Lead](tech-lead/rotina-segunda-feira.md) — reunião, Git, Jira, release (pós [artigo 10](10-fluxo-de-desenvolvimento.md)).

[Índice do Onboarding](README.md)
