# Decisões pendentes — módulo COMPRAS (para o Rafael)

Registro do que ficou **aberto** ou **encaminhado** durante a construção do módulo COMPRAS (submódulos
DEVOLUCAO, COMERCIO_EXTERIOR, SOURCING/CD2, CONTRATOS_GCC/CD5, SUBCONTRATACAO, TMS, relatórios CD7).
Nada aqui bloqueia go-live técnico; são decisões de produto/fiscais e refactors futuros.

## 1. COM-FCI (Faturamento de Compra Internacional) — FECHADO POR DECISÃO (não construído)

O submódulo FCI **não foi construído** — e isso é **intencional**, conforme a própria EF reenquadrada
(`especificacoes/COMPRAS/FATURAMENTO_COMPRA_INTERNACIONAL/EF_*` §0) e a decisão CD1:

- A **importação real** (a "via de importação" que o FCI descrevia) foi construída em **COMERCIO_EXTERIOR**
  (incoterm/moeda/câmbio na `Compra`, rateio landed parametrizável, nacionalização por evento). ✅ entregue.
- A **devolução** que o FCI duplicava virou submódulo próprio **DEVOLUCAO_DE_COMPRA** (CD4). ✅ entregue.
- As entidades `fci_*` **não existem e não devem ser criadas** (EC-01/EC-03, código-é-verdade).
- **Parte independente do FCI = fechada** pelos dois submódulos acima. **Nada mais a construir no FCI.**

### Dependência anotada — FIN-CAM (câmbio) [para decidir/priorizar]
O campo `Compra.TaxaCambio` (e `Moeda`/`Incoterm`) já existe e é **factual** (informado). O que **depende de
um módulo financeiro de câmbio (FIN-CAM), ainda não construído**:
- Conversão automática moeda estrangeira → BRL por cotação de câmbio da data (hoje o câmbio é informado à mão).
- Variação cambial (ganho/perda) entre emissão, desembaraço e pagamento.
- **Encaminhamento:** quando FIN-CAM existir, ligar `Compra.TaxaCambio` à cotação oficial e gerar o fato
  gerador de variação cambial. Até lá, câmbio manual atende (não bloqueia importação).

## 2. Fiscais — valida-contador (NÃO inventados no código; estrutura pronta, valor factual)

Todos os itens abaixo têm **estrutura implementada** (campos/eventos/parametrização), mas o **valor/base/alíquota
NÃO é calculado pelo sistema** — vem do contador (`Negocio-acumulado/fiscal` + overlay Siser) antes do go-live:

| Item | Onde | Como ficou |
|---|---|---|
| Sentido (saída) e CFOP da devolução de compra | DEVOLUCAO_DE_COMPRA | CFOP parametrizado (`DevolucaoCompra.Cfop`), evento de saída; sentido a ratificar (NF-06). |
| Base dos tributos de importação (II/IPI/PIS/COFINS/ICMS) | COMERCIO_EXTERIOR | Montantes entram factualmente na nacionalização; sistema só apropria (NF-02). |
| Base do rateio landed | COMERCIO_EXTERIOR | Config `ComprasImportacaoRateioConfig` **desligada por padrão**; contador define base (NF-02). |
| Obrigatoriedade de DI por CFOP/NCM | COMERCIO_EXTERIOR | DI/adições opcionais; regra vem do contador (NF-03). |
| Frete de entrada: custo × despesa | COMEX (landed) + TMS (`RatearFreteCompra`) | Frete compõe custo por padrão (NF-04); contador confirma tratamento. |
| CFOP remessa/retorno da subcontratação | SUBCONTRATACAO | `RegistrarSubDocumentoFiscal` grava CFOP **parametrizado**, não calculado (SUB-008/NF-05). |
| Crédito/estorno de impostos na devolução | DEVOLUCAO_DE_COMPRA | Efeito financeiro por evento idempotente; política tributária a validar. |

## 3. Refactor futuro (técnico — anotado, NÃO feito overnight)

- **TEC-05 — extrair `Epros.Modules.Compras`:** hoje COMPRAS reside fisicamente no módulo Estoque
  (schema `estoque`, clusters `Compra*`/`Sc*`/`Gcc*`/`Sub*`/`CompraTransporte*`/`DevolucaoCompra*`), honrando
  o módulo **logicamente** (controllers/namespaces/rotas `compras-*`). A extração para assembly próprio é um
  refactor grande (mover entidades + migrations + DI) — **não** feito overnight para não arriscar a suíte.
- **Consumidores dos eventos `com.*`/`est.sc|gcc|sub|tms.*`:** os handlers de Estoque (motor único D1) e
  Financeiro (fato gerador único) para os novos eventos de devolução/nacionalização/frete/serviço/aditivo
  devem ser homologados no lado consumidor (aqui só publicamos no Outbox, idempotente).

## 4. Entregue nesta rodada (resumo)

DEVOLUCAO_DE_COMPRA (CD4) · COMERCIO_EXTERIOR (CD1) · SOURCING mapa comparativo + vencedor (CD2) ·
CONTRATOS_GCC aditivo + performance (CD5) · SUBCONTRATACAO serviço + doc fiscal · TMS frete rateado (NF-04) ·
COM-GC relatórios (CD7). Suíte 1127 verdes, build 0 erros. Detalhe no `HISTORICO-DESENVOLVIMENTO-IA.md`.
