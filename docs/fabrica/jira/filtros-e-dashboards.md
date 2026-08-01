# Filtros JQL e dashboards — EP

> Backlog = planejar e ordenar · Dashboard = ler situação e gargalos

Campo canônico: **Domínio** (Cascading Select). Substituir `"Domínio"` pelo nome exato do campo após criação no Jira.

## Sintaxe JQL — cascading select

| Filtro | JQL |
|---|---|
| Por módulo (todos os submódulos) | `"Domínio" = "FIN — Financeiro"` |
| Por submódulo específico | `"Domínio" = "FIN — Financeiro > FIN-CP-001 — Contas a Pagar"` |
| Transversal | `"Domínio" = "TRV — Trabalho transversal"` |
| Sem classificação | `"Domínio" is EMPTY` |

## Filtros salvos recomendados

| Nome | JQL |
|---|---|
| EP — Backlog ativo | `project = EP AND statusCategory != Done ORDER BY "Domínio", priority DESC` |
| EP — FIN aberto | `project = EP AND "Domínio" = "FIN — Financeiro" AND statusCategory != Done` |
| EP — VEN aberto | `project = EP AND "Domínio" = "VEN — Vendas" AND statusCategory != Done` |
| EP — EST aberto | `project = EP AND "Domínio" = "EST — Estoque" AND statusCategory != Done` |
| EP — PLT aberto | `project = EP AND "Domínio" = "PLT — Plataforma Compartilhada" AND statusCategory != Done` |
| EP — FIN-CP-001 | `project = EP AND "Domínio" = "FIN — Financeiro > FIN-CP-001 — Contas a Pagar" AND statusCategory != Done` |
| EP — FIN-CR-001 | `project = EP AND "Domínio" = "FIN — Financeiro > FIN-CR-001 — Contas a Receber" AND statusCategory != Done` |
| EP — VEN-GPE-001 | `project = EP AND "Domínio" = "VEN — Vendas > VEN-GPE-001 — Gestão de Pedidos" AND statusCategory != Done` |
| EP — Sprint atual | `project = EP AND sprint in openSprints()` |
| EP — Bloqueadas / sem owner | `project = EP AND statusCategory != Done AND (status = Bloqueado OR assignee is EMPTY)` |
| EP — Sem taxonomia | `project = EP AND statusCategory != Done AND "Domínio" is EMPTY` |
| EP — Transversal | `project = EP AND "Domínio" = "TRV — Trabalho transversal" AND statusCategory != Done` |

## Quick Filters (board)

Criar apenas para módulos **prioritários no trimestre** (máx. 5–7):

- FIN · VEN · EST · PLT · Transversal

Não criar 132 quick filters (um por submódulo). Use filtros salvos para submódulos específicos.

## Dashboard geral — "Epros — Visão Produto"

| Gadget | Filtro base | Dimensão |
|---|---|---|
| Gráfico de pizza | EP — Backlog ativo | **Domínio** (nível pai / módulo) |
| Gráfico de pizza | EP — Backlog ativo | **Status** |
| Estatística 2D | EP — Backlog ativo | **Domínio** × **Team** |
| Sprint burndown | EP — Sprint atual | — |
| Filter Results | EP — Bloqueadas / sem owner | lista |
| Filter Results | EP — Sem taxonomia | lista (auditoria) |

> Em gadgets de pizza com cascading, agrupar pelo **nível pai** (macromódulo). Para submódulo, use filtro salvo específico ou estatística 2D.

## Dashboard de módulo (criar sob demanda)

Exemplo: **Epros — Financeiro**

| Gadget | Filtro | Dimensão / conteúdo |
|---|---|---|
| Created vs Resolved | EP — FIN aberto | tendência |
| Pie Chart | EP — FIN aberto | submódulos (`FIN — Financeiro > …`) |
| Pie Chart | EP — FIN aberto | **Status** |
| Pie Chart | EP — FIN aberto | **Team** |
| Filter Results | `... AND sprint in openSprints()` | sprint atual |
| Filter Results | EP — FIN aberto + bloqueadas | exceções |

Repetir o template apenas para módulos **em foco** (Bloco 6 → EST; refatoração NFe → VEN).

## Uso no planning

Durante refinamento e sprint planning:

1. Filtrar backlog por **Domínio** (módulo ou caminho completo).
2. Agrupar visualmente por **Epic** (iniciativa), não por módulo.
3. Conferir carga **Team** (Back-End vs Frontend) no dashboard ou estatística 2D.
4. Usar filtro **Sem taxonomia** até zerar backlog sem classificação.

## Validação pós-migração

- [ ] Quick Filters retornam issues esperadas
- [ ] Dashboard geral reflete distribuição por módulo (nível pai)
- [ ] Filtro `FIN — Financeiro > FIN-CP-001 — Contas a Pagar` lista só CP
- [ ] Card layout mostra Domínio (pai e filho)
- [ ] Filtro "Sem taxonomia" = 0 issues abertas
