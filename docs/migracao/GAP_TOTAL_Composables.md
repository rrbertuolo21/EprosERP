# GAP TOTAL — Composables / Lógica de UX (Epros → EprosERP)

> Auditoria de **cobertura funcional** (não 1:1). O legado espalha a lógica em ~113 composables
> reais de negócio (os `._*` são forks AppleDouble do macOS, não código). O novo **consolida**
> essa lógica em composables co-locados nos componentes (`components/<dominio>/use*.ts`),
> em páginas (`pages/erp/**`) e em campos compartilhados (`components/shared/fields/*`).
>
> **Regra de leitura:** "existe" = a lógica funcional está portada em algum lugar (composable,
> componente OU página). "Falta" = fluxo/cálculo/validação/diálogo comprovadamente ausente ou
> degradado.

Data: 2026-07-04 · Evidência: grep + leitura de fonte.

## Contagem real (fonte, sem `._` e sem `/tests/`)

| Domínio legado | Composables reais | Onde vive no novo |
|---|---|---|
| vendas/nfe | 27 (+9 nfe/) | `components/vendas-nfe/*`, `vendas-nfce/*`, `vendas-nfe-simplificada/*`, `vendas-transmissoes/*` + páginas `erp/vendas/emissao/**` |
| fiscal | 13 | páginas `erp/fiscal/**` + `useFiscalReferencias.ts` |
| financeiro | 12 | `financeiro-receber/*`, `financeiro-pagar/*`, `useContasAReceber.ts` + páginas |
| produtos | 10 | `cadastros-produto/*` + páginas `erp/cadastros/produtos/**` |
| cadastros | 6 | `cadastros-empresa/*`, `cadastros-parceiro/*` + páginas |
| estoque | 4 | `estoque/*`, `useEstoqueProduto*.ts` |
| compras | 2 | `compras-entrada/*`, `compras-nfe-entrada/*` |
| pos (PDV) | 1 | `components/pdv/*` + `usePdvNfce.ts` |
| servicos | 2 | páginas `erp/cadastros/servicos/**` (só cadastro; NFSe não existe em nenhum) |
| configuracoes | 2 | páginas `erp/configuracoes/**` |
| area-cliente | 1 | páginas `area-cliente/**` |
| shared | 2 | `useApiList.ts`, `useRouteScoped*` (equivalente Nuxt) |

**A tese "234 → 15" é enganosa.** O novo tem 15 composables *globais* + ~15 composables
co-locados em componentes + lógica nas páginas. A cobertura funcional real é **alta** (~80%),
com gaps pontuais mas alguns críticos.

---

## Cobertura por domínio

### 1. Vendas / NF-e / NFC-e — cobertura ~85%

**Portado (comprovado):**
- Emissão NF-e completa: `useNfeEmissao.ts`, `useNfeProdutos.ts`, `useNfeTotais.ts`,
  `usePagamentos.ts` (423 linhas vs 573 legado), `useNfeTransmissao.ts`.
- Totais + merge do hub SignalR: `useNfeTotais.ts` documenta e implementa o `mesclarTotaisHubComUi`
  (campos UI vs campos do hub) — **portado**.
- Impostos por item: `useImpostosItem.ts` (ICMS/ICMS-ST/DIFAL/FCP/PIS/COFINS/IPI/IBS-CBS) — todos
  os campos presentes. **Mudança arquitetural deliberada:** o cálculo agora é feito no backend REST
  (o front só coleta e exibe). Não é gap.
- Diálogos ricos: `ImpostosTabsDialog`, `NfeProdutoDialog`, `NfeReferenciadasDialog`,
  `NfeCartaCorrecaoDialog`, `NfeTransmissoesDialog`, `CancelamentoDialog`.
- NFC-e/PDV: `usePdvNfce.ts` + `vendas-nfce/*`. NFe simplificada: `vendas-nfe-simplificada/*`.
- Transmissões / devolução-retorno / inutilização: `useTransmissoes.ts`, `useDevolucaoRetorno.ts`
  (finalidade 4, chaves referenciadas), `useInutilizacao.ts`. **Portado.**
- SignalR real-time: `useRealtime.ts` + handlers embutidos (substitui `useSignalr`, `useNfeSignalrHandlers`).

**FALTA / degradado:**
- **[CRÍTICO] Volumes/transporte de transmissão** — `NfeTransporteCard.vue` declara explicitamente
  *"Sem cálculo de volumes/reboques/lacres nesta versão do design novo (mantido enxuto)"*. O legado
  tem `montarVolumesParaTransmissao.ts` + `volumePossuiValores()` (filtra volumes vazios antes de
  transmitir: quantidade, numeração, peso bruto/líquido, espécie, marca). **Ausente no novo** →
  NF-e com carga/frete que exigem volumes não serão montadas corretamente.
- **[CRÍTICO] Decodificação de balança configurável** — o PDV novo (`PdvBusca.vue::extrairDadosBalanca`)
  usa offsets **hardcoded** (`substring(2,7)` código, `substring(7,12)` valor, divisor fixo `/1000`).
  O legado (`useBalancaBarcode.ts`) usa os campos configuráveis da entidade `Balanca`
  (`qntDigitoIdentificador`, `qntDigitoCodigoProduto`, `qntDigitoValorProduto`, `qntCasaDecimal`,
  `encontrarBalanca` por prefixo). **Degradação:** balanças com layout diferente do padrão embutido
  quebram no novo PDV.
- **[MÉDIO] Emissão em lote NFC-e** — `useLoteNfceEmissao.ts` + `useLoteNfceForm.ts` (tolerância,
  pool de itens, shuffle) sem equivalente. Provável ferramenta de QA/teste de carga — validar se é
  necessária em produção antes de aposentar.
- **[BAIXO/UX] Intro tour de emissão** — `useNfeEmissaoIntroTour.ts` (onboarding guiado da tela de
  emissão) não portado. Cosmético.

### 2. Fiscal — cobertura ~90%

**Portado:** telas de CFOP, NCM, NCM-tributação, CEST, ANP, ICMS-interestadual, tipo-operação-fiscal,
observações-nfe, código-benefício-fiscal, classificações-tributárias, XML-contador —
todas presentes como páginas `erp/fiscal/**`. `useFiscalReferencias.ts` centraliza selects.

**FALTA:** nada estrutural detectado. Observação: o cálculo DIFAL/partilha que no legado ficava
próximo de `useIcmsInterestadual` foi movido para backend (coerente com item 1). Verificar que a
tela `icms-interestadual/index.vue` mantém a mesma UX de cadastro de alíquotas por UF.

### 3. Financeiro — cobertura ~90%

**Portado:** contas-a-receber (fetch/post/put/delete/totais → `useContasAReceber.ts` + páginas),
contas-a-pagar, conta-bancaria, banco, natureza-financeira, plano-de-contas. Diálogos de baixa:
`BaixaPagamentoDialog`, `BaixaPagamentoFormaDialog`, `BaixaContaPagarDialog`. Utils
`contas-a-receber.ts` (cálculos de parcelas) têm equivalente na lógica dos componentes.

**FALTA:** validar se os **cálculos de utils/contas-a-receber** (geração de parcelas, rateio,
vencimentos) estão 100% replicados nos diálogos de baixa — não há composable util dedicado no novo,
a lógica foi absorvida nos `.vue`. Risco baixo-médio; requer diff funcional dos cálculos de parcela.

### 4. Cadastros (pessoa/empresa/parceiro) — cobertura ~90%

**Portado:** `useParceiroForm.ts` (co-locado, com endereços), `cadastros-empresa/*`
(contatos, painel DFe), grupos (pessoa/produto/tributário), contadores. Máscaras CPF/CNPJ/CEP/
telefone/placa em `useMask.ts` (inclui CNPJ alfanumérico da nova especificação). Autocomplete de
cliente (`ClienteAutocomplete.vue`).

**FALTA:** validar `useParceiroFormEnderecos` (múltiplos endereços + busca CEP) — o novo tem
`useParceiroForm.ts` mas confirmar se cobre o fluxo de N endereços do legado.

### 5. Produtos — cobertura ~90%

**Portado:** abas dados/adicionais/combustível (`ProdutoAba*`), categoria, marca, grupo, unidade,
unidade tributável, adicional, balança (cadastro). `useProdutoUnidadeSelect` → lógica em componente.

**FALTA:** nada estrutural. (A degradação da balança é no *consumo* pelo PDV, não no cadastro.)

### 6. Estoque — cobertura ~95%

**Portado:** `useEstoqueProduto.ts`, `useEstoqueProdutoForm.ts`, movimento manual
(`MovimentoManualDialog.vue` + página), enums de movimento embutidos.

**FALTA:** confirmar que `useEstoqueMovimentoEnums` (tipos de movimento) está com a mesma lista.

### 7. Compras / Entrada — cobertura ~85%

**Portado:** entrada de mercadorias (`compras-entrada/*` + `AdicionarProdutoDialog`), NF-e de entrada
(`compras-nfe-entrada/useNfeEntrada.ts` + cards fornecedor/produtos/totais/referenciadas),
devolução-retorno de entrada, importação de XML (`erp/integracao/importar-xml.vue`).

**FALTA:** validar `useEntradaPropriaTransmissoes` (lista de transmissões de entrada própria) —
confirmar equivalência funcional na tela de compras.

### 8. Área-cliente — cobertura ~70%

**Portado:** páginas minhas-faturas, faturas-vencidas, planos. Bloqueio por status "Atrasado".

**FALTA:** **`useFaturasNotificacoes.ts`** (badge de notificação com `diasAtraso` calculado via
`date-fns differenceInDays`/`isBefore`, statusEfetivo ATRASADO/AGUARDANDO). Grep confirma que
**nada** no novo computa `diasAtraso`/`differenceInDays`. O `AppHeader` novo não exibe o alerta de
faturas vencidas — apenas link "Minhas Faturas". **Gap de UX** (notificação proativa perdida).

### 9. Serviços / NFSe — cobertura N/A

`useServicos` (legado) é **só cadastro** de serviços — portado como páginas `erp/cadastros/servicos/**`.
Não há emissão de NFS-e em nenhum dos dois. Sem gap.

---

## Composables globais / utilitários

| Legado | Novo | Status |
|---|---|---|
| useApiFetch | useApi + useApiList | OK (consolidado) |
| useAuth | useAuth | OK |
| useDocumento (CPF/CNPJ valid) | useDocumento | OK — confirmar dígito verificador |
| useEnum | useEnum | OK |
| useHelper (141 lin) | useHelper (103 lin) | **Verificar** — 38 linhas a menos; conferir se algum helper (formatação/parse) sumiu |
| useModalidadesPagamento | embutido em usePagamentos | OK |
| useSwal | ConfirmDialog / DeleteAlert / useToast | OK (troca de lib) |
| useValidateNumericFields | fields/*Input.vue | OK (validação nos campos) |
| useValidateTransporte | — | **FALTA** (ligado ao gap de volumes/transporte) |
| useTransmissaoList | useTransmissoes | OK |
| useTokenValidation / useDecodeJWT | useAuth/useTenant | OK |
| useRegimeTributario / useNFCe | embutidos | OK |
| useBase64 / useVersion / usePwaUpdateLifecycle | — | Verificar (infra; baixo risco) |

---

## Lista priorizada do que falta portar (para aposentar o legado com segurança)

1. **Volumes/reboques/lacres de transporte** na transmissão NF-e (`montarVolumesParaTransmissao`
   + `volumePossuiValores` + `useValidateTransporte`) → `NfeTransporteCard.vue`. **[CRÍTICO]**
2. **Balança configurável no PDV** — substituir offsets hardcoded por leitura de
   `qntDigito*`/`qntCasaDecimal` da entidade `Balanca` (portar `useBalancaBarcode.ts`). **[CRÍTICO]**
3. **Notificação de faturas vencidas** (`useFaturasNotificacoes` — badge `diasAtraso` no header). **[MÉDIO]**
4. **Emissão em lote NFC-e** (`useLoteNfceEmissao/Form`) — decidir se é QA-only ou produção. **[MÉDIO]**
5. **Auditoria de cálculos de parcela** em contas-a-receber (utils absorvidos nos `.vue`). **[MÉDIO]**
6. **Diff de `useHelper`** (38 linhas a menos) — confirmar que nenhum helper de formatação sumiu. **[BAIXO]**
7. **Intro tour de emissão** (`useNfeEmissaoIntroTour`). **[BAIXO/UX]**
8. Confirmar equivalência: `useParceiroFormEnderecos` (N endereços + CEP), `useEntradaPropriaTransmissoes`,
   `useEstoqueMovimentoEnums`, `useValidateTransporte`. **[BAIXO — verificação]**

## Estimativa de cobertura funcional global: **~80%**

A maior parte da lógica de UX (máscaras, validações, cálculos de imposto por item, fluxos de
emissão/transmissão, diálogos ricos) está portada. Os gaps são pontuais mas dois são **bloqueantes
para cenários reais** (volumes de transporte e balança configurável) e precisam ser fechados antes
do cutover.
