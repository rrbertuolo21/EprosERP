# Taxonomia Jira — Módulos e Submódulos (EP)

> **Fonte canônica:** [01-mapa-do-produto-17-modulos.md](../../onboarding/01-mapa-do-produto-17-modulos.md)  
> **Machine-readable:** [taxonomia-modulos-submodulos.json](taxonomia-modulos-submodulos.json)  
> **Import Jira (cascading):** [opcoes-campo-cascata.csv](opcoes-campo-cascata.csv)  
> **Próximos passos:** [proximos-passos.md](proximos-passos.md)

## Objetivo

Padronizar o projeto **EP (EprosWeb)** com taxonomia permanente de produto separada de épicos temporários e componentes técnicos.

| Dimensão | Campo Jira | Cardinalidade | Exemplo |
|---|---|---|---|
| Macromódulo + Submódulo | **Domínio** (cascading) | 1 obrigatório | `FIN — Financeiro > FIN-CP-001 — Contas a Pagar` |
| Iniciativa | **Epic Link / parent** | 0–1 | `[FIN-CP-001] Automatizar conciliação` |
| Capacidade técnica | **Componente** | 0–N | `Backend - DFe` |
| Execução | **Team** | 1 | `Back-End` |

\* Trabalho transversal: `TRV — Trabalho transversal > TRV — Trabalho transversal`

## Formato dos valores

- **Campo Domínio (nível 1):** `{PREFIX} — {Nome}` — ex.: `VEN — Vendas`
- **Campo Domínio (nível 2):** `{CODE} — {Nome de negócio}` — ex.: `VEN-GPE-001 — Gestão de Pedidos`
- **JQL caminho completo:** `"Domínio" = "VEN — Vendas > VEN-GPE-001 — Gestão de Pedidos"`
- **Pasta no código:** campo `folder` no JSON (ex.: `ContasAPagar`)

## Inventário

| Prefixo | Módulo | Submódulos | Schema PostgreSQL |
|---|---|---:|---|
| APP | Aplicativo | 11 | plataforma |
| CAD | Cadastros Base | 3 | plataforma |
| FIN | Financeiro | 12 | financas |
| VEN | Vendas | 11 | vendas |
| EST | Estoque | 13 | estoque |
| COM | Compras | 2 | estoque |
| PRD | Produção | 8 | producao |
| RH | Recursos Humanos | 9 | rh |
| MNT | Manutenção | 7 | manutencao |
| QA | Qualidade | 7 | qualidade |
| PRJ | Projetos | 8 | projetos |
| GOV | Governança | 6 | grc |
| ESG | ESG | 6 | esg |
| CON | Concessionárias | 8 | concessionarias |
| IMO | Imobiliária | 1 | — |
| REL | Relatórios | 2 | — |
| PLT | Plataforma Compartilhada | 18 | plataforma |

**Total:** 17 macromódulos · 132 submódulos (+ opção TRV)

## Códigos de referência (implementados)

| Código | Submódulo | Status no CONTEXT |
|---|---|---|
| FIN-CP-001 | Contas a Pagar | Concluído |
| FIN-CR-001 | Contas a Receber | Concluído |
| EST-PRO-001 | Produtos | Concluído |
| EST-SC-001 | Sourcing e Compras | Bloco 6 em andamento |
| EST-APE-001 | Análise e Planejamento de Estoque | Inventariado |
| VEN-PDV-001 | Ponto de Venda PDV | Concluído |

## Regenerar catálogo

```bash
python docs/dev-framework/jira/generate-taxonomy.py
```

Alterações de módulo/submódulo devem partir do mapa de produto e propagar para este JSON, CSVs e Jira.

## Documentos relacionados

- [configuracao-campos-jira.md](configuracao-campos-jira.md) — setup admin no Jira
- [de-para-componentes-jira.md](de-para-componentes-jira.md) — migração dos componentes legados
- [filtros-e-dashboards.md](filtros-e-dashboards.md) — JQL, backlog e dashboards
- [proximos-passos.md](proximos-passos.md) — checklist operacional
- [governanca-taxonomia.md](governanca-taxonomia.md) — regras de manutenção
