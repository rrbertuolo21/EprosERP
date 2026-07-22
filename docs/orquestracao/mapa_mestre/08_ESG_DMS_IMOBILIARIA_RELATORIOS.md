# Mapa Mestre — ESG · CONCESSIONARIAS (DMS) · IMOBILIARIA · RELATORIOS

> Reconciliação spec × código. Agente de mapa 08. Data 2026-07-22.

## Roots confirmados
- **ESG** existe: `src/Modules/Epros.Modules.ESG/` — só 2 entidades (`EmissaoCarbono.cs`, `RelatorioESG.cs`). Controller `ESGController.cs`. Consome eventos horizontais: `CompraLancadaESGHandler.cs`, `VendaFaturadaESGHandler.cs`.
- **DMS** (=CONCESSIONARIAS) existe: `src/Modules/Epros.Modules.DMS/` — só 3 entidades (`VendaVeiculo.cs`, `OrdemServicoDms.cs`, `GarantiaMontadora.cs`). Controller `DMSController.cs`.
- **IMOBILIARIA**: sem módulo, sem controller. AUSENTE (ADR-04 placeholder).
- **RELATORIOS**: sem módulo, sem controller, sem camada BI. Relatórios ad-hoc espalhados (DANFE fiscal, RelatorioESG).

## Tabela resumo

| Submódulo | Módulo | Status | Reuso horizontais | Tier |
|---|---|---|---|---|
| DIVERSIDADE_E_RESPONSABILIDADE_SOCIAL | ESG | AUSENTE | Baixo (RH) | G |
| ECONOMIA_CIRCULAR | ESG | AUSENTE | Médio (Estoque) | M |
| GESTAO_AMBIENTAL_EHS | ESG | AUSENTE | Médio (Manut/Qld/RH) | G |
| PEGADA_DE_CARBONO | ESG | SCAFFOLD | Alto (eventos FIN/VEN cabeados) | G |
| RELATORIOS_ESG | ESG | SCAFFOLD/PARCIAL | Alto | M |
| TRANSPORTE_SUSTENTAVEL | ESG | AUSENTE | Médio | M |
| CRM_DE_CONCESSIONARIA | DMS | AUSENTE | Alto (GestaoClientes) | M |
| DESENVOLVIMENTO_DE_CONCESSIONARIAS | DMS | AUSENTE | Médio | M |
| FINANCAS (F&I) | DMS | AUSENTE | Alto (Financeiro) | G |
| GARANTIAS | DMS | SCAFFOLD/PARCIAL | Médio (Manutencao) | M |
| GESTAO_DE_PECAS_DE_REPOSICAO | DMS | AUSENTE | Alto (Estoque) | M |
| GESTAO_DE_SERVICOS | DMS | AUSENTE | Alto (Servicos) | M |
| MANUTENCAO | DMS | SCAFFOLD/PARCIAL | Alto (Manutencao) | M |
| VENDAS | DMS | SCAFFOLD/PARCIAL | Alto (Vendas/Estoque) | G |
| GESTAO_IMOBILIARIA | IMOBILIARIA | AUSENTE (sem módulo) | N/A | G |
| BI_E_ANALYTICS | RELATORIOS | AUSENTE (sem módulo) | Alto (todos) | G |
| RELATORIOS_OPERACIONAIS | RELATORIOS | AUSENTE (espalhado) | Alto (todos) | G |

## Dependências
- ESG já cabeia `CompraLancadaEventNotification` + `VendaFaturadaEventNotification`; falta EST/PRD.
- DMS: intenção "magra" mas hoje 3 entidades sem wiring de reuso visível.
- RELATORIOS: consumiria todos — inexistente (precisa camada BI nova).
