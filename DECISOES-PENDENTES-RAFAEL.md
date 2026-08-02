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

---

# Decisões pendentes — módulo FINANCEIRO (motores de cálculo)

Os 10 submódulos do Financeiro (FIN-CGL/SF/AFX/CMG/PO/TS/CAM/CON/GCF/SBF) já estavam construídos
(domínio + CQRS + controllers + testes) em rodadas anteriores. Esta rodada adicionou os **motores de
cálculo contábil/financeiro universais** que ainda faltavam, cada um com testes e citando a skill de
negócio. Nenhum inventa número legal — todos recebem o valor factual como parâmetro (valida-contador).

## 5. Fiscais/contábeis — valida-contador (motores prontos; valor factual é do contador)

| Item | Onde | Como ficou |
|---|---|---|
| Taxa/vida útil de depreciação (RFB IN 1.700/2017 Anexo III) | FIN-AFX `CalculoDepreciacao` | Motor aplica linear/saldos decrescentes/soma dos dígitos; **taxa/vida útil chega informada** no ativo. |
| Alíquotas de IOF de crédito (Decreto 6.306/2007; majoração 2025) | FIN-GCF `CalculoAmortizacao.IofCredito` | Estrutura diário(teto 365)+adicional; **alíquotas são parâmetro** por vigência/PF-PJ, nunca hardcode. |
| Metodologia/divulgação do CET (Res. CMN 3.517/2007) | FIN-GCF `CalculoAmortizacao.CetAnual` | TIR do fluxo por bisseção; componentes (tarifas/seguros) entram como parâmetro; divulgação PF a homologar. |
| Cotação de câmbio (PTAX/fechamento) | FIN-CAM `CalculoVariacaoCambial` | Mark-to-market aplica a cotação; **cotação é fato de mercado por data**, informada. |
| Mapeamento evento→conta do plano (qual débito × qual crédito) | FIN-CGL `MotorContabilizacao` | Motor garante a partida dobrada; **as contas vêm na `RegraContabilizacao`** (config do contador). |

## 6. Wiring pendente (anotado, NÃO feito overnight para não arriscar a suíte)

- **Contabilização automática evento→ledger:** hoje os handlers de integração
  (`CompraLancadaEventHandler`, `VendaFaturadaEventHandler`, folha, projeto) geram **título financeiro**
  (ContasAPagar/Receber), mas **não** geram lançamento no ledger. O `MotorContabilizacao` está pronto e
  testado; ligá-lo aos eventos exige uma **tabela de mapeamento evento→conta** (config por cliente/contador)
  + migration. Isso depende de definição contábil (quais contas do plano) → **valida-contador**; não criado
  overnight para não inventar contas-padrão. Decisão do Rafael/contador: definir o de-para e habilitar.
- **Motores de Price/SAC/CET e variação cambial** são bibliotecas de domínio testadas, ainda **não
  consumidas** por handler/endpoint (dívida estruturada e reavaliação usam valores informados). Expor
  como endpoint de simulação é passo natural quando houver a demanda de produto.

## 7. Outbox — dispatcher central (T1) e consumidores pendentes

Fechada a causa-raiz "Outbox publica mas não despacha/consome": há agora um **dispatcher central**
(`Epros.Infrastructure/Outbox/OutboxDispatcher` + `OutboxDispatcherJob<TContext>`) que roteia mensagens
não-processadas por `EventType` do catálogo para `IOutboxConsumer` registrados, idempotente e com retry.

**Ligado agora (consumidor REAL):**
- `imo.aluguel.cobranca_gerada` → título em **Contas a Receber** (Financeiro, modelo FIEL).

**Roteado ao FALLBACK (logado como pendência, NÃO inventa efeito) — precisa de consumidor real + regra:**
- `con.*` (Concessionárias/DMS): venda de veículo faturada, contrato F&I, OS de oficina, reserva de peças
  → efeitos financeiros/estoque dependem de regra de negócio/fiscal a definir.
- `imo.*` restantes (LocacaoFormalizada, Reajustada, Rescindida, ReciboEmitido, PropostaConvertida, etc.)
  → sem efeito cross-módulo claro hoje.

**Órfãos ainda NÃO cobertos pelo dispatcher (schema já tem job legado — evitar colisão no flag de
processado da mesma tabela física):** `qld.*` (Qualidade), `prd.*` (Produção), `man.*` (Manutenção),
`prj.*` (Projetos), `PlanoAlterado`/`AssinaturaCancelada`/`Reativada`/`ComissaoApurada` (plataforma/
GestãoClientes), `PedidoEcommerceParaVenda`/`ExpedicaoConfirmada`/`DemandaPlanejadaPublicada` (Vendas),
`LdeEntradaConfirmada`/`MercadoriaRecebida` (Estoque), `DocumentoFiscalAutorizado/Cancelado` (Fiscal).
**Migração recomendada:** trocar cada job por-módulo por um `OutboxDispatcherJob<Context>` + consumidores
por evento, um schema de cada vez, com teste. Só então registrar o dispatcher nesses schemas (senão dois
leitores competem pelo mesmo `outbox_messages`).

**⚠️ Anti-dupla-contagem de estoque:** `LdeEntradaConfirmada`/`MercadoriaRecebida` **NÃO** ganharam
consumidor que credite estoque — `LancarCompra` já credita (motor único D1). Um futuro consumidor desses
eventos deve tratar só inspeção/financeiro, nunca re-creditar saldo.
## 7. Segurança — resíduos da rodada de fixes P0/P1 (branch wt/fix-seguranca)

- **Portais externos (Cliente/Fornecedor) — wiring de login externo (`// valida-ambiente`).** O
  isolamento passou a derivar `clienteId`/`fornecedorId` das claims do principal autenticado
  (`ICurrentUser.GetClaim/GetClienteId/GetFornecedorId/GetPortalUsuarioId`). Falta o **host emitir essas
  claims** no token do portal externo (SSO/login do cliente/fornecedor). Enquanto não houver login
  externo, os endpoints operam pelo caminho interno (ABAC), e o enforcement usuário↔cliente/fornecedor já
  está pronto para quando o token trouxer as claims. Guards: `PortalClienteAcesso`, `PortalFornecedorAcesso`.
- **GRC SoD — interceptor GLOBAL pré-gravação (RN-SOD-040) é resíduo.** O bloqueio SoD foi ligado no
  **fluxo de concessão RBAC** (`AtribuirPapelUsuarioCommandHandler` → `ISoDAvaliadorConcessao`), que é o
  ponto de maior alavancagem e menor risco à suíte. Um **interceptor global** que barre QUALQUER caminho de
  concessão de permissão (grant direto de capacidade, seeds, imports) fica como próximo passo. Além disso,
  o **seed ABAC dos novos recursos SoD** (`SegregacaoFuncoes:Avaliar/SolicitarExcecao/AprovarExcecao/
  RegistrarBypass`) é o resíduo T-D — sem seed, os endpoints negam por padrão (fail-closed).
- **Alçada de Compras (CD3/SRC-008) — referência da origem sob alçada.** O gate `AlcadaCompraGate` foi
  ligado nos handlers de lançamento/faturamento de compra (`LancarCompra`, `LancarCompraFiscal`,
  `EntradaPropria`, `EntradaFornecedor`): quando o comando informa `AprovacaoOrigemId`, a efetivação só
  ocorre com o `ComprasPedidoAprovacao` **Aprovado** (pendente/reprovado → bloqueia). **Resíduo:** o
  **front precisa enviar `AprovacaoOrigemId`** (id do pedido de compra/compra sob alçada) e a avaliação
  automática por VALOR no momento do lançamento exigiria comprador/categoria/valor no próprio comando de
  compra — decisão de produto (hoje a alçada é avaliada no fluxo de pedido de compra via `SolicitarAprovacao`).
