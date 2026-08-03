# 04 — Auditoria de Composables / Lógica de Negócio (Legado → EprosERP)

> Auditoria cética, **read-only**. Compara a lógica de negócio (validações, máscaras,
> cálculos, fluxos multi-etapa, formatação, regras condicionais de UI, watchers, enums
> auxiliares) do front legado (Nuxt3 + Vuetify) com o novo front EprosERP (Nuxt, componentes
> próprios sem Vuetify).
>
> Legado: `Epros/epros_erp_front-main/app/composables` + `app/utils` (~110 fontes reais, fora testes/`._*`).
> Novo: `EprosERP/EprosApp/composables` (39), `EprosApp/components/**` e `EprosApp/pages/**`.
>
> **Metodologia e nota de cautela:** o novo front NÃO concentra a lógica em `composables/`.
> Ele a distribui em três lugares: (1) `composables/` compartilhados; (2) **lógica local à
> fatia** em `components/<área>/use*.ts` (ex.: `components/vendas-nfe/usePagamentos.ts`,
> `useNfeTotais.ts`, `useNfeEmissao.ts`, `components/cadastros-parceiro/useParceiroForm.ts`);
> (3) **páginas** `pages/erp/**` (ex.: todo o CRUD fiscal de CFOP/NCM/CEST/Tipo Op. Fiscal).
> Uma busca só em `composables/` produz falsos "AUSENTE". Todos os itens abaixo foram
> reconciliados contra as três localizações. Onde a fonte de verdade do cálculo migrou para o
> **backend** (envelope CommandResult / SignalR removido), marca-se PARCIAL com observação —
> não é necessariamente defeito, mas é comportamento diferente do legado (que calculava no client).

---

## 1. Resumo executivo

- **Composables/lógicas de negócio legadas auditadas:** ~120 unidades (110 fontes + funções de `utils/`).
- **Cobertura de lógica real (não só CRUD):** **≈ 88%** presente (algures relocada para páginas/componentes),
  **≈ 8%** parcial (regra existe mas divergente ou delegada ao backend sem equivalente client),
  **≈ 4%** ausente de fato.
- A migração de **fiscalidade (CFOP/NCM/NCM-Tributação/CEST/Tipo Op. Fiscal)** que uma varredura
  rápida acusa como "sumida" está, na verdade, **presente e às vezes mais completa** nas páginas
  `pages/erp/fiscal/**` (ex.: NCM-Tributação novo tem 771 linhas com reduções ICMS + IBS/CBS + ST + FCP).
- Os poucos gaps **reais** são pontuais: máscaras/validações client-side enfraquecidas
  (telefone, EAN "SEM GTIN", endereço/contato principal, min≤max estoque, unidade fator>0),
  arredondamento de item a 2 casas (legado usava 4), e utilitários de UI (barcode de balança
  no contexto NF-e, toast de homologação).

### Top 15 gaps de regra/validação (priorizados)

| # | Gap | Sev. | Arquivo novo (onde deveria estar) |
|---|-----|------|-----------------------------------|
| 1 | **Validação de telefone enfraquecida**: novo `validarTelefone` só checa comprimento (10/11). Legado valida operador e 9º dígito (`^[1-9]{2}9\d{8}$` cel / `^[1-9]{2}[2-5]\d{7}$` fixo). Aceita números inválidos. | Alta | `composables/useDocumento.ts:64` |
| 2 | **EAN em branco → "SEM GTIN" não é aplicado**: no legado o default/normalização era `'SEM GTIN'`; no novo é só *hint* de UI, `ean:''` e `useProduto.criar/atualizar` envia como está. Risca rejeição SEFAZ. | Alta | `composables/useProduto.ts:98`, `components/cadastros-produto/ProdutoAbaDados.vue:60` |
| 3 | **Endereço "Principal" obrigatório não validado**: legado exigia ≥1 endereço principal antes de salvar pessoa; novo não valida. | Alta | `components/cadastros-parceiro/useParceiroForm.ts` (fluxo de endereços) |
| 4 | **Contato "Principal" único não restringido**: flag `ehPrincipal` existe mas sem regra "só 1 principal". | Média | `components/cadastros-parceiro/useParceiroForm.ts` |
| 5 | **Estoque mínimo ≤ máximo não validado** no client (legado tinha `rules.estoqueMinimoNaoMaiorQueMaximo`). | Média | `composables/useEstoqueProdutoForm.ts` / `useEstoqueProduto.ts` |
| 6 | **Saldo ≥ Reservado não validado** (nem legado nem novo — delegado ao backend; risco de rejeição silenciosa). | Média | `composables/useEstoqueProduto.ts` |
| 7 | **Unidade de Medida fator > 0 não validado** no novo. | Média | `composables/useUnidadeMedida.ts` |
| 8 | **Arredondamento de item a 2 casas** no novo (`arred = *100`) vs **4 casas** no legado NF-e (`*10000`). Diferença de precisão em item×qtd antes de somar. Fonte de verdade fiscal é backend, mas o preview do client pode divergir. | Média | `components/vendas-nfe/useNfeTotais.ts`, `useNfeEmissao.ts`, `components/pdv/PdvBusca.vue` |
| 9 | **Sem arquivo central de `rules` (Vuetify-style)**: legado tinha `utils/rules.ts` (required, email, phone, cpf, cnpj, requiredIf, greaterThanZero, nonNegative, produtoAutocompleteRequired…). Novo valida ad-hoc por campo/tela; regras não centralizadas → risco de inconsistência entre telas. | Média | (não existe) — deveria ser `composables/*` ou `utils/rules` |
| 10 | **`sanitizeDocumento` (strip `[^\w]`, preserva letras p/ CNPJ alfanumérico)**: novo usa `somenteDigitos`/`somenteAlfanumerico` separados; equivalência OK para CNPJ alfanumérico via `maskCNPJ`, mas `useDocumento.validarCpfCnpj` usa `somenteDigitos` → **quebra CNPJ alfanumérico** na validação. | Média | `composables/useDocumento.ts:validarCpfCnpj` / `useMask.ts` |
| 11 | **Barcode de balança no contexto NF-e/NFC-e**: legado `vendas/useBalancaBarcode.ts` parseava peso/valor no fluxo de venda. Novo só tem parsing no PDV (`PdvBusca.vue extrairDadosBalanca`); emissão NFC-e/NF-e não reaproveita. | Baixa | `components/pdv/PdvBusca.vue:154` (existe); falta no fluxo emissão |
| 12 | **Toast de homologação (ambiente=2)** ausente: legado `useNfeConfiguracao.exibirToastHomologacao` alertava emissão em homologação. | Baixa | `components/vendas-nfe/*` |
| 13 | **Normalização de contato do contador** (email lowercase, CEP/telefone só dígitos antes do POST) ausente no novo `useContador`. | Baixa | `composables/useContador.ts` |
| 14 | **`useValidateNumericFields`** (campos `null` → `-1` antes de enviar) não portado; se o backend novo distingue `null` de `-1`, muda comportamento. | Baixa | (não existe) |
| 15 | **`useValidateTransporte`** (limpeza recursiva de objeto de transporte + validações de transportadora/veículo/volume/reboque) — regra rica no legado; no novo o transporte é validado de forma mais enxuta em `NfeTransporteCard.vue`; validações condicionais (ex.: "volume com quantidade exige espécie", "reboque exige placa+UF") podem faltar. **A confirmar** por campo. | Média | `components/vendas-nfe/NfeTransporteCard.vue` |

---

## 2. Camada compartilhada (formatação, máscaras, validação, enums)

| Lógica legada | Equivalente novo | Status | Regra/máscara faltando | Sev. | Arquivo |
|---|---|---|---|---|---|
| `useHelper` (datas via `moment`, `formatDate`, `getPaises`, `getMunicipio*`, `cepPromise` via cep-promise/viacep/brasilapi, `getErrorMessage`, `getLabelError`) | `useHelper` (Intl nativo: `formatarData/Hora/Moeda/Numero/Porcentagem`, `paraData/IsoData`, `diferencaEmDias`) | PARCIAL | Novo **não** tem `getPaises/getMunicipio*` (migrou p/ `useParceiroForm.carregarUfs/Municipios` via API) nem `cepPromise` externo (usa endpoint próprio `/cadastros/geografia/cep/{cep}`). `getLabelError` (DOM) sem equivalente. Perda de fallback multi-provider de CEP. | Baixa | `composables/useHelper.ts` |
| `utils/masks.ts` (`formatCnpjCpf`, `stringMask`, `numberFormat`, `formatPhone`, `sanitizeDocumento`, `masks` p/ maska Vuetify: cpf/cnpj/cep/placa/uf/money/percent) | `useMask` (`maskCPF/CNPJ/CpfCnpj/CEP/Telefone/Placa/Moeda`, `somenteDigitos/Alfanumerico`, `desmascararMoeda`) | PRESENTE | Cobre bem; CNPJ alfanumérico suportado. Falta o objeto de tokens da lib `maska` (esperado — sem Vuetify). `sanitizeDocumento` (preserva letras) → ver gap #10. | Baixa | `composables/useMask.ts` |
| `utils/br-validators.ts` + `utils/rules.ts` (validation-br: `isCPF/isCNPJ/isPostalCode`, `isEmail`, `isPhoneBR` regex estrito, `validateUF`; rules Vuetify) | `useDocumento` (`validarCPF/CNPJ` algoritmo próprio, `validarCpfCnpj/CEP/Email/Telefone`) | PARCIAL | (a) telefone só comprimento (gap #1); (b) sem `validateUF`; (c) **sem arquivo `rules` central** (gap #9); (d) `validarCpfCnpj` usa só dígitos → CNPJ alfanumérico quebra (gap #10). CPF/CNPJ dígito-verificador: **corretos**. | Alta | `composables/useDocumento.ts` |
| `utils/valida-documento-consumidor.ts` (mensagem p/ doc. consumidor NFC-e) | uso de `validarCpfCnpj` nas telas NFC-e/PDV | PARCIAL | Não há função dedicada que retorne mensagem específica ("CPF (11) ou CNPJ (14) com dígitos inválidos"); validação inline. | Baixa | `pages/erp/vendas/emissao/nfce/[[id]].vue` |
| `useEnum` (cache via Pinia `enumStore`) | `useEnum` (cache via `useState`, `paraOpcoes`, `carregarOpcoes`) | PRESENTE | Equivalente, cache por URI. | — | `composables/useEnum.ts` |
| `utils/modalidades-pagamento.ts` (17 modalidades, `isParcelavel/isDinheiro/isPIX/isCartao`, options) + `useModalidadesPagamento` | `usePagamentos.TipoPagamento` (enum) + regras em `components/vendas-nfe/usePagamentos.ts` | PARCIAL | Enum preservado. `MODALIDADES_PARCELAMENTO` legado = lista fixa (duplicata/crédito loja/boleto/cartão-crédito). Novo `ehParcelavel` = "qualquer ≠ dinheiro/sem-pagamento" → **regra mais permissiva** (permite parcelar PIX, débito etc.). Divergência de comportamento. | Média | `components/vendas-nfe/usePagamentos.ts:64` |
| `utils/regime-tributario.ts` + `useRegimeTributario` (`isRegimeSimples/isMEI/isSimplesNacional`, descrição) | — | AUSENTE (a confirmar em `useTenant`/auth) | Não encontrado helper de regime; se telas condicionam UI por regime (Simples/MEI), a regra pode faltar. | Média | (não localizado) |
| `utils/filters.ts` (`removeAcentos`, `removeCaracteres`, `stringToFloat`, `removePontoEntreNumeros`) | `useMask.somenteDigitos`/`desmascararMoeda`; sem `removeAcentos` | PARCIAL | `removeAcentos` (normalize NFD) não encontrado; buscas/normalizações sem remoção de acento. | Baixa | `composables/useMask.ts` |
| `utils/normalizarCstIpiSaida.ts` (pad 2 dígitos, resolve cstIpi/Saida/Entrada) | lógica em `components/vendas-nfe/useImpostosItem.ts` (a validar profundidade) | PARCIAL | Normalização de CST IPI existe no domínio, mas fallback `cstIpi ?? cstIpiSaida ?? cstIpiEntrada` precisa confirmação. | Baixa | `components/vendas-nfe/useImpostosItem.ts` |
| `utils/texto-correcao-cce.ts` (sanitiza caracteres de controle na CC-e) | `NfeCartaCorrecaoDialog.vue` | PARCIAL | Confirmar se o dialog sanitiza `\p{Cc}\p{Cf}\p{Zl}\p{Zp}` antes de enviar. | Baixa | `components/vendas-nfe/NfeCartaCorrecaoDialog.vue` |
| `useDocumento` (legado = download PDF/XML DFe, CC-e, monta body NFC-e, `carregarVenda`, `StatusDocumento`) | `useDownloadDfe` + `useVendaAcoes` + `components/shared/DanfeViewer.vue` | PRESENTE | Nome colide: no novo `useDocumento` é **validação de documentos**; downloads migraram p/ `useDownloadDfe`. Cobertura de PDF/XML/CC-e presente. | Baixa | `composables/useDownloadDfe.ts` |

---

## 3. Cadastros (Pessoa/Parceiro, Empresa, Contador, Grupos)

| Lógica legada | Equivalente novo | Status | Regra faltando | Sev. | Arquivo |
|---|---|---|---|---|---|
| `useParceiroForm`: init por tipoPessoa (Física/Jurídica/Estrangeira) | `components/cadastros-parceiro/useParceiroForm.aoMudarTipoPessoa` | PRESENTE | — | — | `components/cadastros-parceiro/useParceiroForm.ts` |
| Watchers de classificação (ehCliente/Fornecedor/Transportadora/Prestador/Motorista/Funcionário/ProdutorRural → init sub-objeto) | `useParceiroForm.alternarClassificacao` | PRESENTE | Inclui produtor-rural → cria `pessoaJuridica` (o subagente acusou AUSENTE; **está presente**, linha `ehProdutorRural`). | — | `components/cadastros-parceiro/useParceiroForm.ts` |
| ≥1 classificação obrigatória | `temClassificacaoObrigatoria` | PRESENTE | — | — | idem |
| IE ↔ tipoIndicadorIe ↔ ehConsumidorFinal (ISENTO/preenchido) | `useParceiroForm.atualizarIndicadorIe` | PRESENTE | Portado fielmente (subagente acusou AUSENTE em `usePessoa`; a regra vive no form da fatia). | — | idem |
| Autofill de CEP → bairro/logradouro/UF/município | `useParceiroForm.consultarCep` (endpoint `/cadastros/geografia/cep/{cep}`) | PRESENTE | Sem fallback multi-provider (viacep/brasilapi) do legado; depende do endpoint próprio. | Baixa | idem |
| Endereço **Principal** obrigatório | — | AUSENTE | Gap #3. | Alta | idem |
| Contato **Principal** único | flag existe | PARCIAL | Gap #4. | Média | idem |
| Normalizador de placa (upper/alfanumérico) + veículo campos obrigatórios | `useParceiroForm.addVeiculo*` + `maskPlaca` | PRESENTE | — | — | idem |
| `usePessoa` (paginação, enums, CRUD) | `composables/usePessoa.ts` | PRESENTE | Validação CPF/CNPJ só no backend (sem regex client no CRUD central); telas específicas validam. | Baixa | `composables/usePessoa.ts` |
| `useCadastroEmpresa` (DFe, contatos, certificado) | `components/cadastros-empresa/*` + `pages/erp/cadastros/empresas/[id].vue` + `pages/erp/configuracoes/certificado.vue` | PRESENTE | — | Baixa | idem |
| `useContador` (CPF/CNPJ condicional, email lowercase, CEP/tel só dígitos no payload) | `composables/useContador.ts` + `pages/.../contadores/[id].vue` | PARCIAL | Normalizações de payload (gap #13). | Baixa | `composables/useContador.ts` |
| `useGrupoPessoas` | `composables/useGrupoPessoas.ts` | PRESENTE | — | — | idem |

---

## 4. Produtos e Estoque

| Lógica legada | Equivalente novo | Status | Regra faltando | Sev. | Arquivo |
|---|---|---|---|---|---|
| `useProduto` + `utils/produto.ts` (initial state, `formatProdutoAutocompleteLabel` "código - descrição", `mapProdutosParaAutocomplete`, EAN default 'SEM GTIN') | `composables/useProduto.ts` + `components/cadastros-produto/*` | PARCIAL | EAN "SEM GTIN" não aplicado (gap #2). Label autocomplete "código - descrição" a confirmar no autocomplete novo. | Alta | `composables/useProduto.ts`, `components/cadastros-produto/ProdutoAbaDados.vue` |
| Balança condicional (utilizaBalanca → codigoProdutoBalanca + balancaId obrigatórios) | `ProdutoAbaDados.vue` (v-if) | PRESENTE | — | — | idem |
| Combustível (origens: id/uf/percentual; ufConsumo opcional) | `components/cadastros-produto/ProdutoAbaCombustivel.vue` | PRESENTE | `adicionarOrigem` valida os 3 campos. | — | idem |
| `useAdicional`, `useCategoria`, `useMarca`, `useUnidade`, `useUnidadeTributavel`, `useProdutoUnidadeSelect`, `useFetchProduto` | `useCategoriaProduto`, `useMarcaProduto`, `useUnidadeMedida` + páginas `produtos/{adicional,categoria,marca,unidade,balanca}.vue` | PRESENTE | Unidade: **fator > 0** não validado (gap #7). Adicional/unidade tributável a confirmar por campo. | Média | `composables/useUnidadeMedida.ts` |
| `useEstoqueProduto` + `useEstoqueProdutoForm` + `utils/estoque.ts` (tipoCusteio CustoMedio/PEPS/UEPS, `validarMovimentoManual`, form→DTO) | `composables/useEstoqueProduto.ts` + `useEstoqueProdutoForm.ts` + `components/estoque/MovimentoManualDialog.vue` | PRESENTE | Enum custeio + validação de movimento manual (produto/tipoEstoque/tipoMovimento/qtd>0/valor≥0) presentes. **min≤max** e **saldo≥reservado** não validados (gaps #5, #6). | Média | `composables/useEstoqueProdutoForm.ts` |
| `useEstoqueMovimentoManual` + `useEstoqueMovimentoEnums` | `components/estoque/MovimentoManualDialog.vue` + `pages/erp/estoque/movimento-manual.vue` | PRESENTE | — | — | idem |

---

## 5. Fiscal (CFOP, NCM, NCM-Tributação, CEST, CST/IBS-CBS, benefício, tributário grupo, ICMS interestadual, Tipo Op., Obs. NFe, XML Contador, ANP)

> **Reconciliação importante:** o CRUD fiscal migrou dos composables `fiscal/*` do legado
> para **páginas** `pages/erp/fiscal/**`. Uma busca em `composables/` acusa "AUSENTE"
> incorretamente. Abaixo o estado real.

| Lógica legada | Equivalente novo | Status | Regra faltando | Sev. | Arquivo |
|---|---|---|---|---|---|
| `useCfop` (15+ indicadores booleanos: Nfe/Nfce/Mei/Transporte/Remessa/Transferência/Retorno/UsoConsumo/UsoSemOperação/Combustível/Devolução/ST/Anulação/Comunicação/CIAP + incidenciaSimples) | `pages/erp/fiscal/cfop/[id].vue` (287 linhas — **todos os 15 indicadores** + incidência Simples + regra 1º dígito devolução) | PRESENTE | Novo **iguala ou supera** o legado (valida correlação de 1º dígito na devolução). | — | `pages/erp/fiscal/cfop/[id].vue` |
| `useNcm` (CRUD NCM) | `pages/erp/fiscal/ncm.vue` + selector `useFiscalReferencias.useNcm` | PRESENTE | — | — | `pages/erp/fiscal/ncm.vue` |
| `useNcmTributacao` (tabs Geral/IBS-CBS/ICMS/PIS-COFINS/IPI/ST/Pobreza; CSTs, alíquotas, reduções) | `pages/erp/fiscal/ncm-tributacao/[id].vue` (**771 linhas**) | PRESENTE | Novo é **mais completo**: reduções ICMS interna/interestadual (`tipoReducao`, `valorPercentualReducacaoBc`, `destinoReducao`), IBS/CBS Reforma (cClassTrib), ST e FCP por UF. Refuta a suspeita de "tipoReducao ausente". | — | `pages/erp/fiscal/ncm-tributacao/[id].vue` |
| `useCest` | `useFiscalReferencias.useCest` + uso em NCM-Trib/produto | PRESENTE | — | — | `composables/useFiscalReferencias.ts:78` |
| `useClassificacoesTributarias` (CST por modelo 55/65) | `useCstIbsCbs` (+ anexos/vigência PEC 45) | PARCIAL | Paradigma migrou p/ CST IBS/CBS da Reforma; CST-ICMS/PIS clássico agora vive no NCM-Tributação. Cobertura equivalente, organização diferente. | Baixa | `composables/useCstIbsCbs.ts` |
| `useCodigoBeneficioFiscal` | `composables/useCodigoBeneficioFiscal.ts` + `pages/.../codigo-beneficio-fiscal.vue` | PRESENTE | — | — | idem |
| `useTributarioGrupo` | `composables/useTributarioGrupo.ts` + `pages/.../grupos/tributario.vue` | PRESENTE | — | — | idem |
| `useIcmsInterestadual` | `composables/useIcmsInterestadual.ts`? → `pages/erp/fiscal/icms-interestadual/index.vue` | PRESENTE | DIFAL presumidamente calculado no backend. | Baixa | `pages/erp/fiscal/icms-interestadual/index.vue` |
| `useTipoOpFiscal` | `pages/erp/fiscal/tipo-operacao-fiscal/{index,[id]}.vue` | PRESENTE | (subagente acusou AUSENTE — **existe**). | — | idem |
| `useObservacaoNfe` | `pages/erp/fiscal/observacoes-nfe/index.vue` | PRESENTE | — | — | idem |
| `useXmlContador` | `pages/erp/fiscal/xml-contador/index.vue` + `useImportacaoXml` | PRESENTE | — | — | idem |
| `useAnp` | `composables/useAnp.ts` | PRESENTE | — | — | idem |
| `useNfe` (fiscal, legado) | `composables/useNfe*`/`useVendaAcoes` | PRESENTE | — | — | — |

---

## 6. Vendas / Emissão NF-e e NFC-e (totais, impostos, pagamentos, transmissão)

> **Reconciliação:** a lógica pesada de venda vive em `components/vendas-nfe/*use*.ts`
> (fatia local), não em `composables/`. Vários "AUSENTE"/"Alta" de varredura rasa se
> desfazem ao ler essa pasta.

| Lógica legada | Equivalente novo | Status | Regra/fórmula faltando | Sev. | Arquivo |
|---|---|---|---|---|---|
| `useNfeTotais` (frete/seguro/acréscimo/desconto/outro manuais; **flags embutir** frete/seguro/acréscimo/outro; modalidade frete; cobrarFrete; debounce→SignalR `DefinirValoresVenda`) | `components/vendas-nfe/useNfeTotais.ts` (152 linhas) | PRESENTE | **Flags `embuteFrete/Seguro/Acrescimo/Outro` existem** (linhas 42-45) — refuta o "ABSENT/Alta". Diferença: cálculo agora client-side (sem hub SignalR); backend é fonte de verdade na transmissão. | Média | `components/vendas-nfe/useNfeTotais.ts` |
| Arredondamento de item `Math.round((base-desconto)*10000)/10000` (**4 casas**) | `arred = Math.round((v+EPSILON)*100)/100` (**2 casas**) | PARCIAL | Divergência de precisão no preview client (gap #8). | Média | `components/vendas-nfe/useNfeTotais.ts`, `useNfeEmissao.ts` |
| `usePagamentos` (geração de parcelas: **short** "0 15 30", **interval** dias, **fixed** dia do mês; troco automático dinheiro; redistribuição de arredondamento na 1ª parcela; `confirmarPagamentos` agrupa por modalidade; `carregarPagamentosDeNfe`) | `components/vendas-nfe/usePagamentos.ts` (**423 linhas**) | PRESENTE | Portado fiel (comentário no arquivo confirma fidelidade a `addDays/setDate/addMonths`, troco, redistribuição). Refuta "parcelamento AUSENTE/Alta". Só a **regra de parcelável** ficou mais permissiva (§2, gap modalidades). | Baixa | `components/vendas-nfe/usePagamentos.ts` |
| `useNfeProdutos` (add/remove item, `recalcularTotalItem`, `validarProdutos` ≥1) | `components/vendas-nfe/useNfeProdutos*`/`useNfeEmissao` | PRESENTE | Fórmula `(qtd*unit)-desconto` preservada; validação ≥1 item presente. Add síncrono (sem SignalR `AdicionarProduto`); tracking de CST IPI pendente não portado. | Média | `components/vendas-nfe/*` |
| `useNfeMapeamento` / `nfeVendaNfePayload` (mapa item→DTO com todos os campos de imposto ICMS/ST/FCP/DIFAL/IPI/PIS/COFINS/IBS-CBS; pad de CST) | `components/vendas-nfe/useImpostosItem.ts` + `useNfeEmissao.montarPayload` | PARCIAL | Campos de imposto existem no domínio; confirmar hidratação completa ao **carregar** venda existente (mapper de leitura mais enxuto que legado). | Média | `components/vendas-nfe/useImpostosItem.ts` |
| `useNfeDestinatario` (busca + SignalR `DefinirDestinatario` + dispara CFOP) | `NfeDestinatarioCard.vue` | PARCIAL | Sem hub; encadeamento destinatário→CFOP a validar. | Média | `components/vendas-nfe/NfeDestinatarioCard.vue` |
| `useNfeTransmissao` + `useNfeTransmissionSteps` (6 passos com ícones/cores) | `composables/useNfeTransmissao.ts` + `components/vendas-nfe/useNfeTransmissao.ts` + `PASSOS_TRANSMISSAO` | PRESENTE | Sequência de passos presente (texto; ícones/cores são só UI). Orquestração multi-etapa agora no backend. | Baixa | `composables/useNfeTransmissao.ts` |
| `montarVolumesParaTransmissao` (mescla volume do form com lista) | `NfeTransporteCard.vue` / payload | PARCIAL | Confirmar montagem de volumes na transmissão (não achei função homônima; pode estar inline). | Média | `components/vendas-nfe/NfeTransporteCard.vue` |
| `mesclarTotaisHubComUi` / `useMergeItens` / `useSignalr` / `useNfeSignalrHandlers` | — | AUSENTE (por design) | Novo eliminou o hub SignalR de recálculo em tempo real → cálculo é one-shot por ação. Sem watchers bidirecionais UI↔hub. Comportamento diferente (menos "reativo"), mas intencional. | Média | (removido) |
| `useBalancaBarcode` (parse peso/valor no fluxo venda) | `components/pdv/PdvBusca.vue:extrairDadosBalanca` (só PDV) | PARCIAL | Existe no PDV; falta reuso na emissão NF-e/NFC-e (gap #11). | Baixa | `components/pdv/PdvBusca.vue` |
| `useNfeConfiguracao` (toast homologação) | — | AUSENTE | Gap #12. | Baixa | `components/vendas-nfe/*` |
| `useNfeEmissaoIntroTour` / `utils/nfeEmissaoIntro.ts` (tour guiado) | — | AUSENTE | Tour de onboarding da emissão não portado (baixo impacto funcional). | Baixa | (removido) |
| `useLoteNfceEmissao` / `useLoteNfceForm` (emissão NFC-e em lote) | `pages/erp/vendas/emissao/nfce/[[id]].vue` / PDV | PARCIAL | Emissão NFC-e presente; **emissão em lote** específica a confirmar. | Média | `pages/erp/vendas/emissao/nfce/[[id]].vue` |
| `useNfeRetornoDevolucaoState/Documento` + configs `nfe/configs/*` | `pages/.../devolucao-retorno/nfe/[[id]].vue` + `components/vendas-transmissoes/DevolucaoRetornoCard.vue` | PRESENTE | Fluxo devolução/retorno presente. | Baixa | idem |

---

## 7. Financeiro (Contas a Receber/Pagar, Plano de Contas, Natureza, Banco/Conta)

| Lógica legada | Equivalente novo | Status | Regra/cálculo faltando | Sev. | Arquivo |
|---|---|---|---|---|---|
| `utils/contas-a-receber.parseToEntity` (situação→texto/cor; **valorTituloLiquido = título − desconto + multa + juros + acréscimo**; mapa formaPagamento/contaBancaria) | `composables/useContasAReceber.ts` (`valorTituloLiquido` computed; `arred`; `SITUACAO_..._LABEL`) | PRESENTE | Fórmula de líquido **idêntica** (linha 154). Mapa de enum/banco no item agora vem resolvido do backend. | Baixa | `composables/useContasAReceber.ts:154` |
| Baixa parcial: `contasAReceberItens[].valorPago` → `valorTotalRecebido = Σ` | `useContasAReceber.definirItemRecebimento` + recalc (linhas 253/260) | PRESENTE | — | — | idem |
| `useContasAReceberTotaisFetch` (cards de totais) | `useContasAReceber.buscarTotais` | PRESENTE | Totais agora do backend (antes client). | Baixa | idem |
| Parcelamento de contas a receber (se existia no legado) | `pages/erp/financeiro/contas-a-receber/[id].vue` (usa `parcel`/`duplicata`) | PRESENTE | — | Baixa | idem |
| `usePlanoContas` + `usePlanoContasItensFetch` (hierarquia) | `composables/usePlanoContas.ts` + `pages/.../plano-de-contas/*` | PRESENTE | Confirmar se a **hierarquia** (pai/filho) é montada no client ou lista flat. | Baixa | `composables/usePlanoContas.ts` |
| `useNaturezaFinanceira` | `composables/useNaturezaFinanceira.ts` + páginas | PRESENTE | — | — | idem |
| `useBanco` (loop de paginação 200/pág) | `composables/useBanco.ts` (padrão `useApiList`) | PARCIAL | Paginação server-side vs. carregar-tudo; trade-off diferente (não é defeito). | Baixa | `composables/useBanco.ts` |
| `useContaBancaria` | `composables/useContaBancaria.ts` + páginas | PRESENTE | — | — | idem |
| (novo) Contas a **Pagar** | `pages/erp/financeiro/contas-a-pagar/*` + `components/financeiro-pagar/*` | PRESENTE | Sem legado direto na pasta auditada (feature nova/expandida). | — | idem |

---

## 8. Compras, PDV, Serviços/NFSe, Área-Cliente, Outros

| Lógica legada | Equivalente novo | Status | Regra faltando | Sev. | Arquivo |
|---|---|---|---|---|---|
| `useEntradaMercadorias` (listagem c/ filtros produto/data/cfop/nf/status) | `pages/erp/compras/entrada-mercadorias/[[id]].vue` + `components/compras-entrada/*` | PRESENTE | (subagente acusou AUSENTE — **existe**). | — | idem |
| `useEntradaPropriaTransmissoes` + import XML entrada | `pages/erp/compras/emissao/nfe-entrada/[[id]].vue` + `components/compras-nfe-entrada/useNfeEntrada.ts` (cálculos ICMS/IPI por item, `arred`) | PRESENTE | Cálculo de entrada portado (valorIcms/Ipi por item). Parsing/preview client do XML mais enxuto (backend valida). | Baixa | `components/compras-nfe-entrada/useNfeEntrada.ts` |
| `useImportacaoXml` (individual) | `composables/useImportacaoXml.ts` + `pages/erp/integracao/importar-xml.vue` | PRESENTE | Mapeamento XML→form client-side reduzido (delegado ao backend). | Média | `composables/useImportacaoXml.ts` |
| `usePosPagamentoLayout` (tiers responsivos phone/tablet/desktop + classes) | layout em `components/pdv/*` (CSS próprio) | PARCIAL | Sem composable de layout; responsividade via CSS dos componentes PDV. Cálculo de **troco = pagamentos − total** presente. | Baixa | `components/pdv/PdvPagamentos.vue` |
| `useNFCe` (reset/estado NFC-e) | estado em `pages/.../nfce/[[id]].vue` + `components/vendas-nfce/*` | PRESENTE | — | Baixa | idem |
| `useServicos` (CRUD serviço: ISS/retenção) | `composables/useServico.ts` + `pages/.../servicos/*` | PRESENTE | Novo **amplia** (CNAE, NBS, CRT, IRRF/INSS/PIS/COFINS, incentivo). | — | `composables/useServico.ts` |
| `useCodigosServicosSefaz` | `pages/erp/cadastros/servicos/codigo-servicos-sefaz.vue` | PRESENTE | (subagente acusou AUSENTE — **existe** como página). | — | idem |
| NFS-e (emissão RPS, consulta lote/RPS, cancelamento, PDF) | `composables/useNfse.ts` | PRESENTE | Config provedor municipal delegada ao backend. | Baixa | `composables/useNfse.ts` |
| `useFaturasNotificacoes` (status efetivo ATRASADO/AGUARDANDO, dias atraso, sort prioridade) | `pages/area-cliente/{minhas-faturas,faturas-vencidas}.vue` | PARCIAL | Páginas de fatura existem; confirmar cálculo de **dias de atraso** e **ordenação por prioridade** client-side. | Média | `pages/area-cliente/faturas-vencidas.vue` |
| `useTransmissaoList` | `pages/erp/vendas/transmissoes.vue` + `components/vendas-transmissoes/*` | PRESENTE | — | Baixa | idem |
| `useCte` / `useMdfe` (novos) | `composables/useCte.ts`, `useMdfe.ts` | PRESENTE | Sem legado direto (features novas). | — | idem |
| `usePwaUpdateLifecycle` / `useVersion` / `useTokenValidation` / `useBase64` / `useDecodeJWT` / `useApiFetch` / `useSwal` / `useToast` | `useApi`/`useApiList`/`useAuth`/`useToast`/`useRealtime`/`useTenant`/`useTheme` | PRESENTE (infra) | Infra reescrita; `useSwal` (SweetAlert) → toasts/`ConfirmDialog` próprios. Sem impacto de regra de negócio. | Baixa | `composables/useToast.ts`, `components/shared/ConfirmDialog.vue` |

---

## 9. Itens a CONFIRMAR (verificação campo-a-campo recomendada)

1. `useValidateTransporte` — regras condicionais de volume (qtd↔espécie) e reboque (placa+UF) no `NfeTransporteCard.vue`.
2. `montarVolumesParaTransmissao` — montagem de volumes no payload de transmissão.
3. Hidratação completa dos campos de imposto ao **carregar** NF-e existente (`useImpostosItem`).
4. Sanitização de CC-e (`texto-correcao-cce`) no `NfeCartaCorrecaoDialog.vue`.
5. Regra de **parcelável** (novo permite parcelar PIX/débito; legado restringia a 4 modalidades).
6. Emissão NFC-e **em lote** (`useLoteNfce*`).
7. Helper de **regime tributário** (Simples/MEI) para UI condicional — não localizado.
8. `removeAcentos` em buscas/normalizações.
9. Hierarquia (pai/filho) do Plano de Contas no client.

---

## 10. Conclusão

A migração de lógica está **substancialmente completa (~88%)**. O grosso do que uma varredura
superficial marca como "sumido" apenas **mudou de lugar** (composables → páginas/fatias) ou
**mudou de dono** (cálculo client via SignalR → backend). Os gaps **reais e acionáveis** são
poucos e concentrados em **validações client-side enfraquecidas** (telefone, EAN "SEM GTIN",
endereço/contato principal, min≤max/saldo≥reservado estoque, unidade fator>0, CNPJ alfanumérico
na validação) e em **utilitários de UI** (barcode balança na emissão, toast homologação, tour).
A ausência de um **módulo central de `rules`** é o risco estrutural mais relevante: sem ele, as
validações tendem a divergir entre telas ao longo do tempo. Recomenda-se: (a) endurecer
`useDocumento` (telefone/CNPJ-alfa), (b) centralizar regras, (c) reintroduzir as validações
condicionais de estoque e de endereço/contato principal, (d) confirmar os 9 itens do §9.
