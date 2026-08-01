# De/para — Componentes Jira legados → Módulo / Submódulo

> **Gerado por:** [generate-de-para.py](generate-de-para.py)  
> **CSV:** [de-para-componentes.csv](de-para-componentes.csv) · **JSON:** [de-para-componentes.json](de-para-componentes.json)

## Regras

1. **Componente legado funcional** (ex.: `Financeiro - Contas a Pagar`) → preenche **Módulo** e **Submódulo**.
2. **Componente técnico** (ex.: `Backend - DFe`) → permanece em **Componente**; não duplica domínio.
3. Issues com **dois componentes** (funcional + técnico): migrar domínio para campos; manter só o técnico em Componente.
4. Componentes genéricos (`Financeiro`, `Vendas`, `Estoque`) → Módulo preenchido + Submódulo `TRV — Trabalho transversal` até refinamento.

## Ambiguidades (revisão humana)

| Componente legado | Sugestão | Motivo |
|---|---|---|
| Cadastros - Balança | CAD-POP-001 | Pode ser EST-PRO-001 se for cadastro de produto |
| Cadastros - Caixa PDV | CAD-POP-001 | Pode ser VEN-PDV-001 se for operação PDV |
| Vendas - Emissão NFe | VEN-GPE-001 + Backend - DFe | UI em vendas; motor em PLT-FFE-001 |
| Fiscal - * | PLT-CFG-001 | Cadastros fiscais transversais |

Marcados com `REVISAR` no CSV quando a confiança for baixa.

## Exemplos frequentes no backlog ativo

| Componente legado | Módulo | Submódulo | Componente técnico |
|---|---|---|---|
| Financeiro - Contas a Receber | FIN — Financeiro | FIN-CR-001 — Contas a Receber | — |
| Financeiro - Contas a Pagar | FIN — Financeiro | FIN-CP-001 — Contas a Pagar | — |
| Vendas - Emissão NFe | VEN — Vendas | VEN-GPE-001 — Gestão de Pedidos | Backend - DFe |
| Vendas - Emissão Cupom NFCe | VEN — Vendas | VEN-PDV-001 — Ponto de Venda PDV | Backend - DFe |
| Compras - Entrada de Mercadorias | COM — Compras | COM-GCO-001 — Gestão de Compras | — |
| Cadastros - Produtos | EST — Estoque | EST-PRO-001 — Produtos | — |
| Fiscal - NCM Tributação | PLT — Plataforma Compartilhada | PLT-CFG-001 — Configuração | — |
| Frontend - Componentes Compartilhados | TRV — Trabalho transversal | TRV — Trabalho transversal | Frontend - Componentes Compartilhados |
| Engenharia - CI/CD | TRV — Trabalho transversal | TRV — Trabalho transversal | Engenharia - CI/CD |

## Migração em lote

1. Exportar backlog ativo: `project = EP AND statusCategory != Done`
2. Para cada issue, lookup do primeiro componente funcional em [de-para-componentes.csv](de-para-componentes.csv)
3. Preencher Módulo, Submódulo; ajustar Componente para técnico apenas
4. Issues sem componente: classificar manualmente ou usar épico pai

Ver [migracao-backlog-ativo.md](migracao-backlog-ativo.md).
