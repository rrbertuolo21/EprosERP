# Mapa Mestre — PRODUCAO · QUALIDADE · RH (módulos em quarentena)

> Reconciliação spec × código. Agente de mapa 06. Data 2026-07-22.
> 0 DONE · 4 PARCIAL · 4 SCAFFOLD · 16 AUSENTE. 10 entidades no código vs ~230 exigidas.

## Entidades presentes
- **Producao**: `OrdemProducao`, `ApontamentoProducao`, `ListaMateriais`, `BomItem`. Controller `ProducaoController`.
- **Qualidade**: `InspecaoLote`, `NaoConformidade`. Controllers `InspecoesController`, `NaoConformidadesController`.
- **RH**: `Colaborador`, `Timesheet`, `FolhaPagamento`, `FolhaPagamentoVerba`. Controller `RHController`.
- Nenhum nome de tabela exigido (`prd_*`/`qld_*`/`rh_*`) existe; entidades achatadas cobrem superficialmente vários submódulos.

## Contratos Outbox (wired end-to-end)
- **RH publica `FolhaProcessada`** → `Financeiro/FolhaProcessadaFinanceiroHandler` (Job RHOutboxProcessorJob).
- **PRD publica `OrdemProducaoEncerrada`** (InsumoConsumidoItem) → `Estoque/OrdemProducaoEncerradaEstoqueHandler`.
- **QLD publica `InspecaoReprovada`** → `Estoque/InspecaoReprovadaEstoqueHandler`; **consome `CompraLancada`** → cria InspecaoLote.
- Contratos em `Shared/Domain/Events/`.

## Tabela resumo

| Submódulo | Módulo | Status | Presentes/Exigidas | Tier |
|---|---|---|---|---|
| CUSTOS_DE_PRODUCAO | PRD | AUSENTE | 0/5 | M |
| ESCALONAMENTO_PROGRAMACAO | PRD | AUSENTE | 0/5 | M |
| ESTIMATIVA | PRD | AUSENTE | 0/5 | M |
| ESTRUTURA_DE_PRODUTO_BOM | PRD | PARCIAL | 2/7 | M |
| EXECUCAO_DE_MANUFATURA_MES | PRD | PARCIAL | 2/9 | G |
| GESTAO_DE_ORDENS_DE_SERVICO | PRD | SCAFFOLD | 0/5 | M |
| MRP_PLANEJAMENTO_INTEGRADO_IBP | PRD | AUSENTE | 0/5 | G |
| PLANEJAMENTO_DE_PRODUCAO | PRD | AUSENTE | 0/4 | M |
| ADMINISTRACAO_DA_QUALIDADE | QLD | AUSENTE | 0/9 | M |
| ANALISE_DE_ACEITACAO_E_REJEICAO | QLD | SCAFFOLD | 1/10 | G |
| GESTAO_DE_ATRIBUTOS | QLD | AUSENTE | 0/8 | M |
| NAO_CONFORMIDADES_NCR | QLD | PARCIAL | 1/9 | G |
| PLANOS_DE_INSPECAO_E_AMOSTRAGEM | QLD | SCAFFOLD | 1/11 | G |
| QUALIDADE_DE_FORNECEDOR | QLD | AUSENTE | 0/12 | G |
| RASTREABILIDADE_E_RECALL | QLD | AUSENTE | 0/12 | G |
| DESENVOLVIMENTO_DE_FUNCIONARIOS | RH | AUSENTE | 0/16 | M |
| FOLHA_DE_PAGAMENTO_E_BENEFICIOS | RH | PARCIAL | 2/31 | G |
| GESTAO_DA_FORCA_DE_TRABALHO | RH | SCAFFOLD | 1/18 | G |
| GESTAO_DE_TALENTOS | RH | AUSENTE | 0/17 | G |
| PLANEJAMENTO_DE_RH | RH | AUSENTE | 0/10 | M |
| PONTO_E_JORNADA | RH | SCAFFOLD | 1/16 | G |
| RECRUTAMENTO | RH | AUSENTE | 0/18 | G |
| SAUDE_E_SEGURANCA_OCUPACIONAL | RH | AUSENTE | 0/12 | M |
| TREINAMENTO_E_CERTIFICACOES_LMS | RH | AUSENTE | 0/8 | M |

## Direcionamento
- Folha BR/eSocial (RH-FP) e MES/BOM (PRD) são G — F3.
- QLD depende de EST (recebimento) já wired — bom ponto de partida F3.
