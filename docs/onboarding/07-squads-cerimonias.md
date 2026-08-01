---
title: "Squads, cerimônias e como o time opera"
confluence_id: "193363970"
confluence_url: "https://rafaelbertuolo.atlassian.net/wiki/spaces/EprosWeb/pages/193363970/Squads+cerim+nias+e+como+o+time+opera"
last_updated: "2026-07-07"
---

> [!IMPORTANT]
> **O que você vai aprender:** como o time se organiza, as cerimônias do sprint e DoR/DoD.

Ferramentas sem processo viram caos. Este artigo fecha a trilha base com o **modelo operacional** — quem faz o quê, quando nos encontramos e o que significa "pronto".

---

## O time

| Papel | Pessoa | Foco principal |
| --- | --- | --- |
| Tech Lead | @Cesar Vieira | Arquitetura, ADRs, aprovação de PRs |
| Guardião de Domínio | @Marcio Goncalves | Regras fiscais e de negócio — validação pré-merge |
| Dev Backend | Back-End (@Marcio Goncalves, @Rafael Rosa, @Thales Gallasso) | Features no novo sistema, CQRS |
| Dev Frontend | Frontend (@Cesar Vieira, @João Paulo Miranda Dos Santos) | Nuxt 4, API, design system |
| PO / Facilitador | @luciano | Requisitos com ACs, Jira, priorização |
| QA / SDET | @Cesar Vieira | Testes automatizados, CI |

**Princípio:** Agentes IA cobrem gaps de capacidade — não substituem pessoas.

---

## Squads e canais

| Área | Canal | Repositórios / foco |
| --- | --- | --- |
| Daily assíncrona | `#daily` | 3 perguntas diárias (ontem / hoje / bloqueio) |
| Empresa | `#epros-geral` | Comunicação geral |
| Incidentes | `#epros-incidentes` | Produção fora + alertas Sentry |
| Produto / Jira | `#epros-produto` | Backlog, board EP |
| Releases produção | `#releases` | Deploy em `main` |
| Cross-squad / ADRs | `#dev-geral` | Decisões de arquitetura, ADRs |
| Backend | `#dev-backend` | `epros-back`, `epros-api` — dúvidas técnicas da squad |
| Frontend | `#dev-frontend` | `epros-front` — dúvidas técnicas da squad |
| PRs e CI | `#dev-prs` | PR aberto/aprovado, CI falhou |

Projetos Jira: **EP** (features do produto, ativo); **SUP** (suporte e patches — planejado).

> [!NOTE]
> Guia completo de canais e uso do Slack → artigo 08 e artigo 09.

---

## Cerimônias semanais

| Dia | Cerimônia | Duração | Participantes |
| --- | --- | --- | --- |
| Seg 9h | Reunião de segunda — homolog, fila, Sprint Review + Planning | 75–90 min | PO, Tech Lead, time |
| Ter–Sex | Desenvolvimento | Assíncrono | Devs |
| Qui 16h | Refinamento | 45 min | PO, Tech Lead, Dev Sênior |

> [!TIP]
> **Daily:** assíncrona em `#daily` — cada dev posta o que fez ontem, o que vai fazer hoje e bloqueios.

A reunião de segunda é **uma única cerimônia** com pauta unificada: blocos operacionais (homolog, rejeitados, fila de PR) + review e planning do sprint. Detalhe da pauta, checklist de encerramento e pós-reunião Git → [Fluxo de desenvolvimento — artigo 10](10-fluxo-de-desenvolvimento.md).

---

## Definition of Ready (DoR)

Item **entra** no sprint quando:

* User Story com ACs em Given/When/Then
* Sem ambiguidade — time entende o que fazer
* Dependências mapeadas
* Estimativa Fibonacci (1, 2, 3, 5, 8, 13)
* Impacto em multi-tenancy ou fiscal avaliado pelo Tech Lead
* Wireframe disponível (se houver interface nova)

---

## Definition of Done (DoD)

Item **sai** do sprint somente com Code Review Agent executado, CI verde (incluindo os 8 testes de segurança) e aprovação do Tech Lead.

Item **sai** do sprint quando:

* Code Review Agent executado (output no PR)
* Tech Lead aprovou o PR
* Cobertura ≥ 70% nos arquivos alterados
* CI verde (build + testes + 8 testes de segurança)
* TenantLeakTest passou
* QA validou fluxo de negócio
* OpenAPI atualizado (se endpoint novo)
* Changelog atualizado

Checklist do autor (antes do PR): [Code Review — checklist do autor](code-review-checklists-e-boas-praticas.md) · Gate do Tech Lead: [Tutorial Tech Lead](tech-lead/tutorial-tech-lead-arquiteto.md).

---

## Métricas que acompanhamos

| Métrica | Por quê |
| --- | --- |
| Lead time de PR | Velocidade de entrega |
| Cobertura de código | Qualidade sustentável |
| Zero P0 em produção | Estabilidade |
| 100% US com AC | Clareza de requisitos |
| Carryover por sprint | Capacidade real vs planejada |

---

**Aprofundamento:** branches, PR, homologação, hotfix e ciclo semanal → [Fluxo de desenvolvimento — artigo 10](10-fluxo-de-desenvolvimento.md).

**Próximo passo →** [Slack — guia dos canais](08-slack-guia-canais.md)
