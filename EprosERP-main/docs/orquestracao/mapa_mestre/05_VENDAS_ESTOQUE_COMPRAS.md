# Mapa Mestre — VENDAS · ESTOQUE · COMPRAS

> Reconciliação spec × código. Agente de mapa 05. Data 2026-07-22.
> 4 DONE · 5 PARCIAL · 4 SCAFFOLD · 13 AUSENTE.

## Constatação central
- **VENDAS** (`Modules.Vendas`, 37 ent.): `Venda*` implementa GESTAO_DE_PEDIDOS integralmente; `Caixa/CaixaMovimento` cobrem núcleo do PDV. Sem CRM, e-commerce, demanda, contratos, garantias, serviços, portal, expedição.
- **ESTOQUE** (`Modules.Estoque`, 54 ent.): `Produto*` = PRODUTOS DONE; `Compra*` (21 ent.) implementa COMPRAS **dentro do Estoque**; TMS e Comércio Exterior parciais.
- **COMPRAS**: sem módulo próprio (ADR-01 confirmado). Operacional/fiscal vive em Estoque; COM-GC estratégico (`sc_*`) parcial. `ComprasController` roteia p/ Estoque.

## Contratos Outbox (confirmados)
- `VendaFaturadaEventNotification`: publicado por Vendas → consumido por Estoque (baixa), Financeiro (CR), Fiscal, ESG.
- `CompraLancadaEventNotification`: publicado por Estoque → consumido por Financeiro (CP), Qualidade, ESG. (Nome do contrato é `CompraLancada`, não CompraRecebida.)
- Também `VendaCancelada`/`CompraCancelada`.

## Tabela resumo

| Submódulo | Módulo-código | Status | Faltantes (nº) | Tier |
|---|---|---|---|---|
| VEN GESTAO_DE_PEDIDOS | Vendas (`Venda*`) | DONE | 0 | G |
| VEN PONTO_DE_VENDA_PDV | Vendas (`Caixa*`) | DONE (parcial) | ~4 | M |
| VEN COMERCIO_ELETRONICO | — | AUSENTE | 12 | G |
| VEN CRM | — | AUSENTE | 25 | G |
| VEN FATURAMENTO_COMERCIAL_INTERNACIONAL | reuso VendaNfeExportacao | SCAFFOLD | ~8 | M |
| VEN GESTAO_DE_CONTRATOS_DE_VENDA | — | AUSENTE | 10 | M |
| VEN GESTAO_DE_SERVICOS | — | AUSENTE | 5 | M |
| VEN LOGISTICA_DE_SAIDA | reuso VendaTransporte* | SCAFFOLD | ~7 | M |
| VEN PLANEJAMENTO_DE_DEMANDA | — | AUSENTE | 7 | M |
| VEN PORTAL_DO_CLIENTE | — | AUSENTE | 6 | M |
| VEN GARANTIAS | — | AUSENTE | 3 | P |
| EST PRODUTOS | Estoque (`Produto*`) | DONE | 0 | G |
| EST SOURCING_E_COMPRAS | Estoque (`Compra*`) | PARCIAL (andamento) | ~4 | G |
| EST MOVIMENTACAO_MANUAL_E_AJUSTES | Estoque | PARCIAL | ~10 | G |
| EST COMERCIO_EXTERIOR | Estoque (CompraItemImportacao) | PARCIAL | 0–1 | M |
| EST TRANSPORTE_E_FRETE_TMS | Estoque (CompraTransporte*) | PARCIAL | 1 | M |
| EST LOGISTICA_DE_ENTRADA | Estoque (ImportacaoXml) | SCAFFOLD | ~6 | G |
| EST ANALISE_E_PLANEJAMENTO_DE_ESTOQUE | — | AUSENTE | 3 | M |
| EST GESTAO_DE_ARMAZEM_WMS | — | AUSENTE | 4 | G |
| EST GESTAO_DE_CONTRATOS_DE_COMPRA | — | AUSENTE | 4 | M |
| EST INVENTARIO_FISICO_E_CONTAGEM_CICLICA | — | AUSENTE | 5 | G |
| EST PORTAL_DO_FORNECEDOR | — | AUSENTE | 9 | M |
| EST RASTREABILIDADE_DE_LOTE_SERIALIZACAO | — | AUSENTE | 7 | G |
| EST SUBCONTRATACAO | — | AUSENTE | 7 | M |
| COM GESTAO_DE_COMPRAS | Estoque (`Compra*`) | DONE | 0–1 | G |
| COM FATURAMENTO_COMPRA_INTERNACIONAL | reuso Compra* import | SCAFFOLD | ~6 | M |
