---
title: "Boas-vindas ao Epros: o ERP SaaS que você vai construir"
confluence_id: "193003523"
confluence_url: "https://rafaelbertuolo.atlassian.net/wiki/spaces/EprosWeb/pages/193003523/Boas-vindas+ao+Epros+o+ERP+SaaS+que+voc+vai+construir"
last_updated: ""
---

> [!NOTE]
> **O que você vai aprender:** o que é o Epros, por que ele é um SaaS multi-tenant, como esta série de artigos se organiza e qual o papel de cada pessoa no time.

Você entrou no time que está construindo o **Epros** — um ERP SaaS multi-tenant. Não é um projeto de manutenção pontual: é a construção de uma plataforma completa, com fiscal, financeiro, estoque, vendas e dezenas de outros domínios, pensada para escalar com dezenas de clientes em produção.

Este artigo é o ponto de partida. Leia-o antes de abrir o repositório.


## **O que é o Epros**

O Epros é um **ERP completo** entregue como **software como serviço**. Cada cliente (tenant) opera de forma isolada — seus dados, usuários e configurações não se misturam com os de outros clientes.


| Dimensão    | Detalhe                                      |
| ----------- | -------------------------------------------- |
| Modelo      | SaaS multi-tenant                            |
| Escopo      | 17 macro-domínios, 132 submódulos            |
| Superfícies | Web, desktop (Electron) e mobile (Capacitor) |

> [!IMPORTANT]
> Multi-tenancy não é um recurso opcional no Epros — é a fundação. Toda decisão de arquitetura, código e teste parte do princípio de que um cliente nunca pode ver dados de outro.

---

## **O que estamos construindo**

Três superfícies no **mesmo repositório** `EprosERP`:


| Pasta / projeto | Responsabilidade |
| --- | --- |
| `src/` (`Epros.API` + `Modules`) | Backend — monólito modular, CQRS, DDD, PostgreSQL |
| `EprosApp/` | Frontend — Nuxt 3 (web; Electron no mesmo app) |
| `Epros.Mobile/` | Mobile — React Native (submódulo git) |


O princípio técnico que guia tudo: **cloud-agnostic + open source**. O sistema roda em VPS de baixo custo ou no data center do cliente Enterprise — sem alterar código.

---

## **Esta série de artigos**

O onboarding está dividido em **11 artigos base** (00–10; todos leem) e **5 trilhas por função** (após a base).

`Produto → Arquitetura → Stack → Código → Segurança → IA → Squads → Slack → Fluxo Git → Sua função`

Cada artigo segue formato de leitura rápida: gancho, subtítulos curtos, código com contexto e link para o próximo. Tempo total da trilha base: cerca de **1h45** (ver [README](README.md)).

> [!NOTE]
> **Fora de escopo desta série:** setup hands-on de ambiente local. Isso acontece em sessão prática presencial ou documentação técnica separada.

---

## **O time e os papéis**


| Papel                   | Pessoa                                                                                                                                                  | Foco                                                      |
| ----------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------- | --------------------------------------------------------- |
| **Tech Lead**           | @Cesar Vieira                                                                                                                                           | Arquitetura, ADRs, aprovação de PRs de alto risco         |
| **Guardião de Domínio** | @Marcio Goncalves                                                                                                                                       | Regras de negócio fiscal e ERP — validação antes do merge |
| **Dev Backend**         | [Back-End](https://rafaelbertuolo.atlassian.net/people/team/0f6f1d7d-03b1-4773-85cc-3ccedbca5d50) (@Marcio Goncalves @Rafael Rosa @Thales Gallasso) | Features no novo sistema, padrões CQRS                    |
| **Dev Frontend**        | [Frontend](https://rafaelbertuolo.atlassian.net/people/team/df789072-96d5-4f58-9ad8-1f7b600b9122) (@Cesar Vieira @João Paulo Miranda Dos Santos)    | Nuxt 3, integração com API, design system                 |
| **PO / Facilitador**    | @luciano                                                                                                                                                | Requisitos com ACs, priorização, Jira                     |
| **QA / SDET**           | @Cesar Vieira                                                                                                                                           | Testes automatizados, 8 testes de segurança, CI           |


---

## **Onde buscar mais**

- **Índice desta série:** [Onboarding Epros ERP — Índice](README.md)

- **Mapa do produto:** [17 módulos, 132 submódulos: o mapa do Epros ERP | Catálogo completo por módulo](01-mapa-do-produto-17-modulos.md)

- **Trilha da sua função:** [Onboarding Epros ERP — Índice | Ramificações por função (após artigo 10)](README.md)

---

**Próximo passo →** [17 módulos, 132 submódulos: o mapa do Epros ERP](01-mapa-do-produto-17-modulos.md)
