# Epros.Modules.Relatorios — RELATORIOS & BI (RPT)

Modulo **read-side / query-side puro** (CQRS). Nao possui DbContext, entidades nem tabelas
proprias: consome, **somente para leitura/agregacao**, os DbContexts dos modulos de origem
(`ContextEstoque`, `ContextVendas`, `ContextFinanceiro`), ja registrados no `Program.cs`.
Nao altera estado de nenhum outro modulo.

Fonte funcional: `especificacoes/RELATORIOS/` (EF/MC de RELATORIOS_OPERACIONAIS e BI_E_ANALYTICS).
Decisoes de fork aplicadas: `especificacoes/RELATORIOS/DECISOES-PENDENTES-RAFAEL.md`.

## Submodulos

### RPT-OPB — Relatorios Operacionais (Openbook)
Rota: `GET /api/v1/relatorios/operacionais/*` — ABAC `RelatoriosOperacionais/Ler`.
- Estoque: `estoque/posicao`, `estoque/giro`
- Vendas: `vendas/por-periodo`, `vendas/por-produto`, `vendas/por-cliente`
- Compras: `compras/por-item`, `compras/por-parceiro`
- Financeiro: `financeiro/aging-receber`, `financeiro/aging-pagar`, `financeiro/receber`,
  `financeiro/pagar`, `financeiro/pagamentos-recebidos`, `financeiro/pagamentos-efetuados`,
  `financeiro/fluxo-caixa`

### RPT-ONM — BI (OneManager)
Rota: `GET /api/v1/relatorios/bi/*` — ABAC `RelatoriosBi/Ler`.
- KPIs: `kpi/faturamento`, `kpi/margem`, `kpi/inadimplencia`, `kpi/ruptura`
- Series: `serie/faturamento` (granularidade Dia|Mes)
- `top-produtos` (Valor|Quantidade), `painel-gerencial` (consolidado multi-modulo)

## Regras transversais
- **Isolamento por empresa/tenant**: aplicado pelo global query filter do `ContextBase`
  (`TenantId == tenant && DeletadoEm == null`) — EF RN-001 / RN-BI-001. Handlers nao precisam
  filtrar tenant manualmente.
- **ABAC**: submodulos novos sobem **negados** ate a permissao ser semeada (padrao ESG).
- **Vencido** (aging): `DataVencimento < referencia` E `saldo devido > 0` (EF RN-016).
- Formulas gerenciais isoladas em `AgingCalculo` (aging) e `BiCalculo` (margem) para troca barata;
  gaps de negocio registrados em `Conhecimento-acumulado/_ingestao/PEDIDOS.md`.

## Testes
`tests/Epros.Tests/RelatoriosRptTests.cs` e `RelatoriosBiTests.cs` (agregacao com dados semeados,
InMemory). Filtro: `~Relatorio|~Bi`.
