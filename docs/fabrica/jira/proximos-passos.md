# Próximos passos — Padronização Jira EP

Checklist operacional após adoção do campo **Domínio** (cascading select).

## Fase 1 — Admin Jira (Tech Lead / admin)

| # | Ação | Referência | Responsável |
|---|---|---|---|
| 1 | Criar campo **Domínio** (Cascading Select) no projeto EP | [configuracao-campos-jira.md](configuracao-campos-jira.md) §1 | Admin Jira |
| 2 | Cadastrar hierarquia a partir de [opcoes-campo-cascata.csv](opcoes-campo-cascata.csv) | 133 pares pai→filho | Admin Jira |
| 3 | Adicionar campo às telas (criar, editar, transição) | §2 | Admin Jira |
| 4 | Configurar card layout do board (Domínio + Team) | §2 | PO / Tech Lead |
| 5 | Criar Quick Filters (FIN, VEN, EST, PLT, Transversal) | §5 | PO / Tech Lead |
| 6 | Anotar `customfield_id` do Domínio e atualizar publish-jira-ep | §7 | Tech Lead |
| 7 | Ocultar componentes funcionais da tela de criação | §4 | Admin Jira |

**Critério de saída:** issue de teste criada com `FIN — Financeiro > FIN-CP-001 — Contas a Pagar`.

## Fase 2 — Piloto (PO + dev)

| # | Ação | Referência |
|---|---|---|
| 8 | Migrar 8–10 issues abertas do piloto | [migracao-piloto-sugestoes.csv](migracao-piloto-sugestoes.csv) |
| 9 | Validar JQL por módulo e submódulo | [filtros-e-dashboards.md](filtros-e-dashboards.md) |
| 10 | Refinar ambiguidades do de/para | [de-para-componentes-jira.md](de-para-componentes-jira.md) |
| 11 | Time valida em 1 refinamento de sprint | [governanca-taxonomia.md](governanca-taxonomia.md) DoR |

**Critério de saída:** piloto aprovado; filtro `"Domínio" is EMPTY` = 0 nas issues piloto.

## Fase 3 — Saneamento de épicos

| # | Ação | Referência |
|---|---|---|
| 12 | Renomear EP-1004 e EP-733 | [auditoria-epicos-ativos.md](auditoria-epicos-ativos.md) |
| 13 | Fechar épicos DECOMMISSION (EP-1292, EP-1278, EP-1117) | [auditoria-epicos-acoes.csv](auditoria-epicos-acoes.csv) |
| 14 | Preencher Domínio nos épicos KEEP/RENAME | auditoria |

**Critério de saída:** nenhum épico aberto com nome de módulo genérico (`Estoque`, `Contas a Pagar`).

## Fase 4 — Migração do backlog ativo

| # | Ação | Referência |
|---|---|---|
| 15 | Migrar sprint atual + próxima | [migracao-backlog-ativo.md](migracao-backlog-ativo.md) |
| 16 | Migrar bugs abertos | de/para CSV |
| 17 | Zerar filtro `"Domínio" is EMPTY` no backlog aberto | filtros |

**Escopo:** apenas `statusCategory != Done`. Histórico fechado intacto.

## Fase 5 — Dashboards e governança

| # | Ação | Referência |
|---|---|---|
| 18 | Criar dashboard **Epros — Visão Produto** | [filtros-e-dashboards.md](filtros-e-dashboards.md) |
| 19 | Criar dashboard do módulo em foco (ex.: VEN durante refatoração NFe) | idem |
| 20 | Comunicar nova taxonomia no #epros-produto | [governanca-taxonomia.md](governanca-taxonomia.md) |
| 21 | Agendar revisão trimestral da taxonomia | idem |

## Fase 6 — Automação via MCP (opcional, após Fase 1)

Com `customfield_id` definido e confirmação explícita do time:

1. Aplicar migração piloto via `editJiraIssue`
2. Renomear épicos KEEP/RENAME
3. Publicar novos tickets do Planning Agent já com Domínio preenchido

## Ordem recomendada (resumo)

```
Admin cria Domínio (cascading)
  → Piloto 8–10 issues
    → Saneamento épicos
      → Migração backlog aberto
        → Dashboards
          → Governança contínua
```

## Arquivos-chave

| Arquivo | Uso |
|---|---|
| [opcoes-campo-cascata.csv](opcoes-campo-cascata.csv) | Cadastro hierárquico no Jira |
| [taxonomia-modulos-submodulos.json](taxonomia-modulos-submodulos.json) | Fonte machine-readable |
| [de-para-componentes.csv](de-para-componentes.csv) | Migração componente → Domínio |
| [auditoria-epicos-acoes.csv](auditoria-epicos-acoes.csv) | Ações por épico aberto |
