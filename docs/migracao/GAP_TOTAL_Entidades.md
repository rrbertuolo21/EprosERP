# GAP TOTAL — Entidades × Campos (Legado Epros → EprosERP)

> Auditoria de migração dimensão **Entidades-Campos**. Objetivo: NADA ficar para trás antes de aposentar o legado.
> Comprovado via extração automatizada (brace-matching de classes C#) + verificação manual por grep. Data: 2026-07-04.

## Metodologia

- **Legado:** `Epros/epros_erp-main/src/Epros.ERP.Domain/Entities` — 318 arquivos `.cs`, dos quais **159 são classes reais** (os outros 159 são arquivos-lixo AppleDouble `._*` do macOS, ignorados).
- **Novo:** `EprosERP/src/Modules/*/Domain/Entities` — 254 classes de entidade.
- Match por **nome de entidade** (case-insensitive) + fallback por **família de prefixo** e **pool global de campos** (para detectar campos realocados em entidades owned/renomeadas).
- **Ignorados** (herdados de `EntidadeSaaSBase`/infra): `Id, TenantId, SyncId`, auditoria (DataCriacao/Atualizacao/Exclusao, CriadoPor…), `Ativo`, `RowVersion` etc.
- **`SequenciaTenantId`** (legacy long sequencial) é campo de infraestrutura do legado — não é dado de negócio; contabilizado à parte.

## Cobertura

| Métrica | Valor |
|---|---|
| Entidades legadas reais | 159 |
| Campos comparáveis (excl. herdados) | 1809 |
| Campos com destino EXATO (mesmo nome) | 1597 (88.3%) |
| + Realocados/renomeados (família/global) | +97 → 1694 (93.6%) |
| **Cobertura de CAMPOS DE DADOS (excl. nav-props + SequenciaTenantId)** | **99.3% (1418/1428)** |
| Nav-properties sem contrapartida (relacionamento, não dado) | 74 |
| `SequenciaTenantId` sem contrapartida (infra legado) | 31 |
| **Campos de DADO realmente candidatos a ausência** | **10 → verificados: 1 real** |

## Conclusão executiva

O núcleo fiscal (Venda*/Compra* — impostos, IBS/CBS, combustível, importação, transporte) está **100% portado no nível de campo**:
`VendaItemImposto`/`CompraItemImposto` = **70/70 campos**; blocos IBS/CBS 20-21/21; Importação 15/15 + adição 7/7; Transporte (reboque/veículo/volume/transportadora) integral; `NcmTributacao` 52/53 (só falta `SequenciaTenantId`). **Zero coluna de imposto perdida.**

Os "AUSENTE" restantes são, na esmagadora maioria, **propriedades de navegação** (coleções e refs a outras entidades — o relacionamento existe, apenas foi remodelado) ou **renomeações**:

| Campo legado "ausente" | Situação real no novo | Veredito |
|---|---|---|
| `CertificadoDigital.DataValidadeInicial/Final` | `EmpresaCertificado.ValidadeInicial/Final` | Renomeado — OK |
| `CertificadoDigital.Senha/Serial` | `EmpresaCertificado.SenhaSegredoId`+`Serial` (cofre de segredos) | Remodelado — OK |
| `CertificadoDigital.CaminhoCompleto` | Não há coluna equivalente (novo guarda cert via secret-vault, sem caminho físico) | **ÚNICO GAP REAL** (arquitetural; sem perda se cert re-upado no cutover) |
| `Municipio.Estado (EEstado enum)` | `Municipio.Uf (string)` | Renomeado + tipo — OK |
| `Pessoa.EhEstrangeiro (bool)` | Modelado via relação `PessoaEstrangeiro` | Remodelado — OK |
| `UsuarioEmpresa.IsAdmin` / `PerfilUsuarioId` | `EhAdmin` / `PerfilAcessoId` | Renomeado — OK |
| `PessoaVeiculo.PessoaMotoristaId/TransportadoraId` | **Comentados no próprio legado** (linhas 27-28) | Não é campo — OK |
| `PerfilUsuarioAcesso.*` (Ver/Editar/Excluir/Menu…) | `PerfilAcessoMenu.*` (todos presentes, Guid) | Renomeado/portado — OK |

**Único item de atenção para o cutover:** `CertificadoDigital.CaminhoCompleto` (path físico do .pfx) não tem coluna destino — o novo modelo usa cofre de segredos (`CertificadoSegredoId`/`SenhaSegredoId`). Migração: re-associar/re-upload dos certificados no cutover; nenhum dado tributário afetado.

---

## Tabela completa — Entidade × Campo × Status

Status: `ok` = campo com destino exato · `ok (realocado)` = renomeado/movido para entidade da mesma família · `**AUSENTE**` = sem destino (nav-prop, SequenciaTenantId ou dado real — ver seção executiva).

### Adicionais

| Campo | Tipo | Status |
|---|---|---|
| Descricao | string | ok |
| ValorPreco | decimal | ok |
| AdicionaisProdutos | ICollection<AdicionaisProduto> | ok |

### AdicionaisProduto

| Campo | Tipo | Status |
|---|---|---|
| ProdutoId | long | ok |
| AdicionaisId | long | ok |
| Produto | Produto | ok |
| Adicionais | Adicionais | ok |

### Balanca

| Campo | Tipo | Status |
|---|---|---|
| SequenciaTenantId | long | **AUSENTE** |
| Nome | string | ok |
| QntDigitoIdentificador | int | ok |
| QntDigitoCodigoProduto | int | ok |
| QntDigitoValorProduto | int | ok |
| QntCasaDecimal | int | ok |
| TipoValor | ETipoValorBalanca | ok |
| Produtos | ICollection<Produto> | ok |

### Banco

| Campo | Tipo | Status |
|---|---|---|
| Codigo | string | ok |
| Descricao | string | ok |
| ContaBancarias | ICollection<ContaBancaria> | **AUSENTE** |

### CartaoDeCredito

| Campo | Tipo | Status |
|---|---|---|
| SequenciaTenantId | long | **AUSENTE** |
| ContaBancariaId | long | ok |
| Apelido | string | ok |
| Titular | string | ok |
| BandeiraCartao | EBandeiraCartao | ok |
| Observacao | string? | ok |
| ContaBancaria | ContaBancaria | ok |
| CartaoDeCreditoFaturas | ICollection<CartaoDeCreditoFatura>? | ok |

### CartaoDeCreditoFatura

| Campo | Tipo | Status |
|---|---|---|
| CartaoDeCreditoId | long | ok |
| DataLancamento | DateTime | ok |
| DataVencimento | DateTime | ok |
| Valor | decimal | ok |
| Pago | bool | ok |
| CartaoDeCredito | CartaoDeCredito | ok |

### CategoriaProduto

| Campo | Tipo | Status |
|---|---|---|
| SequenciaTenantId | long | **AUSENTE** |
| ProdutoGrupoId | long | ok |
| Descricao | string | ok |
| Produtos | ICollection<Produto> | ok (realocado) |
| ProdutoGrupo | ProdutoGrupo | ok |

### CertificadoDigital

| Campo | Tipo | Status |
|---|---|---|
| Nome | string? | ok (realocado) |
| CNPJ | CNPJ | ok (realocado) |
| Senha | string | ok (realocado) |
| Serial | string | ok (realocado) |
| CaminhoCompleto | string | **AUSENTE** |
| DataValidadeInicial | DateTime? | **AUSENTE** |
| DataValidadeFinal | DateTime? | **AUSENTE** |
| Tipo | ECertificadoDigitalTipo | ok (realocado) |
| Origem | ECertificadoDigitalOrigem | ok (realocado) |
| Empresa | Empresa | **AUSENTE** |

### Cest

| Campo | Tipo | Status |
|---|---|---|
| Codigo | string | ok |
| Descricao | string | ok |
| Produtos | ICollection<Produto>? | ok (realocado) |

### Cfop

| Campo | Tipo | Status |
|---|---|---|
| CfopCodigo | int | ok |
| Descricao | string | ok |
| NaturezaOperacao | string | ok |
| CfopCorrelacao | string | ok |
| IntegraFaturamento | bool | ok |
| IndicadorNfe | bool | ok |
| IndicadorComunicacao | bool | ok |
| IndicadorTransporte | bool | ok |
| IndicadorDevolucao | bool | ok |
| IndicadorRetorno | bool | ok |
| IndicadorAnulacao | bool | ok |
| IndicadorRemessa | bool | ok |
| IndicadorCombustivel | bool | ok |
| IndicadorTransferencia | bool | ok |
| IndicadorNfce | bool | ok |
| IndicadorCiap | bool | ok |
| IndicadorUsoConsumo | bool | ok |
| IndicadorUsoSemOperacao | bool | ok |
| IndicadorSt | bool | ok |
| IndicadorMei | bool | ok |
| IncidenciaSimples | EIncidenciaSimples | ok |
| CfopDevolucao | string? | ok |

### CfopPadrao

| Campo | Tipo | Status |
|---|---|---|
| CfopCodigo | int | ok |
| DataInicioVigencia | DateTime | ok |
| DataFimVigencia | DateTime? | ok |
| Descricao | string | ok |
| NaturezaOperacao | string | ok |
| CfopCorrelacao | string? | ok |
| IntegraFaturamento | bool | ok |
| IndicadorNfe | bool | ok |
| IndicadorComunicacao | bool | ok |
| IndicadorTransporte | bool | ok |
| IndicadorDevolucao | bool | ok |
| IndicadorRetorno | bool | ok |
| IndicadorAnulacao | bool | ok |
| IndicadorRemessa | bool | ok |
| IndicadorCombustivel | bool | ok |
| IndicadorTransferencia | bool | ok |
| IndicadorNfce | bool | ok |
| IndicadorCiap | bool | ok |
| IndicadorUsoConsumo | bool | ok |
| IndicadorUsoSemOperacao | bool | ok |
| IndicadorSt | bool | ok |
| IndicadorMei | bool | ok |
| IncidenciaSimples | EIncidenciaSimples | ok |
| CfopDevolucao | string? | ok |

### ClassificacaoTributaria

| Campo | Tipo | Status |
|---|---|---|
| CstIbsCbsId | long | ok |
| Codigo | string | ok |
| Descricao | string | ok |
| DataInicioVigencia | DateTime | ok |
| DataFimVigencia | DateTime? | ok |
| IndNfe | bool | ok |
| IndNfce | bool | ok |
| IndCte | bool | ok |
| IndCteos | bool | ok |
| IndNfse | bool | ok |
| IndTribRegular | bool | ok |
| Anexos | List<ClassificacaoTributariaAnexo>? | ok |
| CstIbsCbs | CstIbsCbs | ok (realocado) |

### ClassificacaoTributariaAnexo

| Campo | Tipo | Status |
|---|---|---|
| ClassificacaoTributariaId | long | ok |
| NroAnexo | int | ok |
| Codigo | string | ok |
| DataInicioVigencia | DateTime | ok |
| DataFimVigencia | DateTime? | ok |
| ClassificacaoTributaria | ClassificacaoTributaria | **AUSENTE** |

### CodigoAnp

| Campo | Tipo | Status |
|---|---|---|
| Codigo | string | ok |
| Descricao | string | ok |
| DataInicioVigencia | DateTime | ok |
| DataFinalVigencia | DateTime? | ok |
| Produtos | ICollection<Produto>? | ok (realocado) |

### CodigoBeneficioFiscal

| Campo | Tipo | Status |
|---|---|---|
| SequenciaTenantId | long | **AUSENTE** |
| Codigo | string | ok |
| Descricao | string? | ok |
| Uf | EEstado | ok |
| Csosns | ICollection<CodigoBeneficioFiscalCsosn> | ok |
| Csts | ICollection<CodigoBeneficioFiscalCst> | ok |
| NcmTributacao | NcmTributacao | **AUSENTE** |

### CodigoBeneficioFiscalCsosn

| Campo | Tipo | Status |
|---|---|---|
| SequenciaTenantId | long | **AUSENTE** |
| CodigoBeneficioFiscalId | long | ok |
| Csosn | ECodigoSituacaoOperacaoSimplesNacional | ok |
| CodigoBeneficioFiscal | CodigoBeneficioFiscal | ok (realocado) |

### CodigoBeneficioFiscalCst

| Campo | Tipo | Status |
|---|---|---|
| SequenciaTenantId | long | **AUSENTE** |
| CodigoBeneficioFiscalId | long | ok |
| Cst | ECodigoSituacaoTributariaIcms | ok |
| CodigoBeneficioFiscal | CodigoBeneficioFiscal | ok (realocado) |

### CodigoServicoSefaz

| Campo | Tipo | Status |
|---|---|---|
| Codigo | string | ok |
| Descricao | string | ok |
| Servico | Servico | **AUSENTE** |

### Compra

| Campo | Tipo | Status |
|---|---|---|
| ModeloFiscal | EModeloDocumento | ok |
| NaturezaOperacao | string | ok |
| DataCompra | DateTime | ok |
| InformacoesComplementares | string? | ok |
| InformacoesAdicionaisFisco | string? | ok |
| Status | EVendaStatus | ok |
| ModalidadeFrete | EModalidadeFrete | ok |
| Emitente | CompraEmitente | ok |
| Destinatario | CompraDestinatario | ok |
| Transporte | CompraTransporte? | ok |
| Total | CompraTotal | ok |
| Configuracao | CompraConfiguracao | ok (realocado) |
| Nfe | CompraNfe? | ok |
| Fatura | CompraFatura? | ok |
| Imposto | CompraImposto? | ok |
| TotalIbsCbs | CompraTotalIbsCbs? | ok (realocado) |
| Pagamentos | ICollection<CompraPagamento> | ok |
| Itens | ICollection<CompraItem> | ok |
| AutorizacoesXml | ICollection<CompraAutorizacaoXml> | ok (realocado) |
| Referenciadas | ICollection<CompraNfeReferenciada>? | ok (realocado) |
| CompraOrigem | EVendaOrigem | ok |
| TipoNota | ETipoNFeCompra | ok |
| FatoGeradorFinanceiro | FatoGeradorFinanceiro | ok (realocado) |
| CompraEntrega | CompraEntrega? | **AUSENTE** |
| CompraCobrancaEndereco | CompraCobrancaEndereco? | **AUSENTE** |

### CompraAutorizacaoXml

| Campo | Tipo | Status |
|---|---|---|
| CompraId | long | ok |
| Documento | Documento | ok |
| Compra | Compra | ok |

### CompraCobrancaEndereco

| Campo | Tipo | Status |
|---|---|---|
| CompraId | long | ok |
| Nome | string | ok |
| Fone | string | ok |
| Email | string | ok |
| IE | string | ok |
| Documento | Documento | ok |
| Uf | EEstado | ok |
| Logradouro | string? | ok |
| Numero | string? | ok |
| Complemento | string? | ok |
| Bairro | string? | ok |
| MunicipioId | int | ok |
| MunicipioNome | string? | ok |
| Cep | CEP? | ok |
| PaisId | int | ok |
| PaisNome | string? | ok |
| Compra | Compra | ok |

### CompraConfiguracao

| Campo | Tipo | Status |
|---|---|---|
| CompraId | long | ok |
| TipoOperacao | ETipoOperacaoNfe | ok |
| TipoFormatoImpressaoDanfe | ETipoFormatoImpressaoDanfe | ok |
| TipoEmissao | ETipoEmissao | ok |
| TipoAmbiente | ETipoAmbiente | ok |
| FinalidadeEmissao | EFinalidadeEmissao | ok |
| IndicadorFinalidadeOperacao | EIndicadorFinalidadeOperacao | ok |
| TipoAtendimento | ETipoAtendimento | ok |
| IndicadorIntermediadorMarketplace | EIndicadorIntermediadorMarketplace | ok |
| Compra | Compra | ok |

### CompraDestinatario

| Campo | Tipo | Status |
|---|---|---|
| CompraId | long | ok |
| PessoaId | long? | ok |
| Cnpj | CNPJ? | ok |
| Cpf | CPF? | ok |
| RazaoSocial | string? | ok |
| Telefone | string? | ok |
| InscricaoEstadual | string? | ok |
| IdentificadorEstrangeiro | string? | ok |
| IndicadorIE | ETipoIndicadorIe | ok |
| Email | string? | ok |
| EhConsumidorFinal | bool | ok |
| Enderecos | ICollection<CompraDestinatarioEndereco?> | ok |
| Compra | Compra | ok |

### CompraDestinatarioEndereco

| Campo | Tipo | Status |
|---|---|---|
| CompraDestinatarioId | long | ok |
| TipoEndereco | ETipoEndereco | ok |
| Uf | EEstado | ok |
| Logradouro | string? | ok |
| Numero | string? | ok |
| Complemento | string? | ok |
| Bairro | string? | ok |
| MunicipioId | int | ok |
| MunicipioNome | string? | ok |
| Cep | string? | ok |
| PaisId | int | ok |
| PaisNome | string? | ok |
| CompraDestinatario | CompraDestinatario | ok |

### CompraEmitente

| Campo | Tipo | Status |
|---|---|---|
| CompraId | long | ok |
| EmpresaId | long? | ok |
| PessoaId | long? | ok |
| Cnpj | CNPJ? | ok |
| Cpf | CPF? | ok |
| RazaoSocial | string | ok |
| NomeFantasia | string? | ok |
| Telefone | string? | ok |
| InscricaoEstadual | string | ok |
| InscricaoEstadualST | string? | ok |
| InscricaoMunicipal | string? | ok |
| Cnae | int | ok |
| RegimeTributario | ERegimeTributario | ok |
| Endereco | CompraEmitenteEndereco | ok |
| Compra | Compra | ok |
| Empresa | Empresa? | **AUSENTE** |
| Pessoa | Pessoa? | **AUSENTE** |

### CompraEmitenteEndereco

| Campo | Tipo | Status |
|---|---|---|
| Uf | EEstado | ok |
| Logradouro | string? | ok |
| Numero | string? | ok |
| Complemento | string? | ok |
| Bairro | string? | ok |
| MunicipioId | int | ok |
| MunicipioNome | string? | ok |
| Cep | string? | ok |
| PaisId | int | ok |
| PaisNome | string? | ok |

### CompraEntrega

| Campo | Tipo | Status |
|---|---|---|
| CompraId | long | ok |
| Nome | string | ok |
| Fone | string | ok |
| Email | string | ok |
| IE | string | ok |
| Documento | Documento | ok |
| Uf | EEstado | ok |
| Logradouro | string? | ok |
| Numero | string? | ok |
| Complemento | string? | ok |
| Bairro | string? | ok |
| MunicipioId | int | ok |
| MunicipioNome | string? | ok |
| Cep | CEP? | ok |
| PaisId | int | ok |
| PaisNome | string? | ok |
| Compra | Compra | ok |

### CompraFatura

| Campo | Tipo | Status |
|---|---|---|
| CompraId | long | ok |
| NumeroFatura | string? | ok |
| ValorOriginal | decimal | ok |
| ValorDesconto | decimal | ok |
| ValorLiquido | decimal | ok |
| Duplicatas | ICollection<CompraFaturaDuplicata> | ok |
| Compra | Compra | ok |

### CompraFaturaDuplicata

| Campo | Tipo | Status |
|---|---|---|
| CompraFaturaId | long | ok |
| NumeroDuplicata | string | ok |
| DataVencimento | DateTime | ok |
| ValorDuplicata | decimal | ok |
| CompraFatura | CompraFatura | ok |

### CompraImposto

| Campo | Tipo | Status |
|---|---|---|
| CompraId | long | ok |
| ValorAliquotaCreditoIcms | decimal | ok |

### CompraItem

| Campo | Tipo | Status |
|---|---|---|
| CompraId | long | ok |
| ProdutoId | long | ok |
| CodigoProduto | string | ok |
| CodigoEan | string? | ok |
| DescricaoProduto | string | ok |
| Ncm | string | ok |
| ExcecaoNcmTipi | string? | ok |
| CestId | long? | ok |
| Cest | string? | ok |
| CodigoAnpId | long? | ok |
| CodigoAnp | string? | ok |
| Cfop | int | ok |
| UnidadeComercial | string | ok |
| QuantidadeComercial | decimal | ok |
| ValorUnitarioComercial | decimal | ok |
| ValorTotalBrutoProdutos | decimal | ok |
| CodigoEanTributavel | string? | ok |
| UnidadeTributavel | string | ok |
| QuantidadeTributavel | decimal | ok |
| ValorUnitarioTributavel | decimal | ok |
| ValorDesconto | decimal | ok |
| ValorDescontoRateado | decimal | ok |
| ValorFreteRateado | decimal | ok |
| ValorSeguroRateado | decimal | ok |
| ValorOutrasDepesasAcessoriasRateado | decimal | ok (realocado) |
| CompoeValorTotal | EIndicadorTotalizador | ok |
| InformacoesAdicionaisDoProduto | string? | ok |
| CfopCorrelacao | int | ok |
| IntegraFaturamento | bool | ok |
| NumeroItemPedidoCompra | int | ok |
| NumeroPedidoCompra | string? | ok |
| FichaConteudoImportacao | string? | ok |
| CodigoBeneficioFiscal | string? | ok |
| Imposto | CompraItemImposto | ok |
| ImpostoValorAproximado | CompraItemImpostoValorAproximado | ok |
| Combustivel | CompraItemCombustivel | ok |
| ImpostoIbsCbs | CompraItemImpostoIbsCbs? | ok |
| Importacoes | ICollection<CompraItemImportacao> | ok |
| Compra | Compra | ok |

### CompraItemCombustivel

| Campo | Tipo | Status |
|---|---|---|
| CompraItemId | long | ok |
| CodigoAnp | string? | ok |
| DescricaoAnp | string? | ok |
| QuantidadeCombustivelFaturada | decimal | ok |
| UfConsumo | EEstado | ok |
| PercentualGlpDerivadoPetroleo | decimal | ok |
| PercentualGasNaturalNacional | decimal | ok |
| PercentualGasNaturalImportado | decimal | ok |
| ValorPartida | decimal | ok |
| Origens | ICollection<CompraItemCombustivelOrigem>? | ok |
| CompraItem | CompraItem | ok |

### CompraItemCombustivelOrigem

| Campo | Tipo | Status |
|---|---|---|
| CompraItemCombustivelId | long | ok |
| IndicadorImportacao | int | ok |
| UfOrigem | EEstado | ok |
| PercentualOrigem | decimal | ok |
| CompraItemCombustivel | CompraItemCombustivel | ok |

### CompraItemImportacao

| Campo | Tipo | Status |
|---|---|---|
| CompraItemId | long | ok |
| NumeroDeclaracaoImportacao | string | ok |
| DataDeclaracaoImportacao | DateTime | ok |
| LocalDesembaraco | string | ok |
| UfDesembaraco | string | ok |
| DataDesembaraco | DateTime | ok |
| TipoViaTransporte | ETipoViaTransporte | ok |
| ValorAFRMM | decimal | ok |
| TipoIntermedio | ETipoIntermedioImportacao | ok |
| Cnpj | CNPJ? | ok |
| Cpf | CPF? | ok |
| UfTerceiro | string? | ok |
| CodigoExportador | string | ok |
| Adicoes | ICollection<CompraItemImportacaoAdicao> | ok |
| CompraItem | CompraItem | ok |

### CompraItemImportacaoAdicao

| Campo | Tipo | Status |
|---|---|---|
| CompraItemImportacaoId | long | ok |
| NumeroAdicao | int | ok |
| NumeroSequencialAdicao | int | ok |
| CodigoFabricante | string | ok |
| ValorDesconto | decimal | ok |
| NumeroAtoConcessorio | string? | ok |
| CompraItemImportacao | CompraItemImportacao | ok |

### CompraItemImposto

| Campo | Tipo | Status |
|---|---|---|
| CompraItemId | long | ok |
| Origem | EOrigemMercadoria | ok |
| CstIcms | ECodigoSituacaoTributariaIcms | ok |
| Csosn | ECodigoSituacaoOperacaoSimplesNacional | ok |
| ModalidadeDeterminacaoBaseCalculoIcms | EModalidadeBaseDeCalculosIcms | ok |
| ValorBaseDeCalculoIcms | decimal | ok |
| PercentualReducaoBaseDeCalculoIcms | decimal | ok |
| AliquotaIcms | decimal | ok |
| ValorImpostoIcms | decimal | ok |
| ModalidadeBaseDeCalculosST | EModalidadeBaseDeCalculosST | ok |
| PercentualMvaBaseDeCalculoST | decimal | ok |
| PercentualReducaoBaseDeCalculoST | decimal | ok |
| ValorBaseDeCalculoSt | decimal | ok |
| AliquotaSt | decimal | ok |
| ValorImpostoSt | decimal | ok |
| MotivoDesoneracaoIcms | EMotivoDesoneracaoIcms | ok |
| ValorBaseDeCalculoStRetido | decimal | ok |
| ValorImpostoStRetido | decimal | ok |
| PercentualCreditoSimplesNacionalIcms | decimal | ok |
| ValorImpostoCreditoSimplesNacionalIcms | decimal | ok |
| ValorBaseDeCalculoFcp | decimal | ok |
| PercentualFcp | decimal | ok |
| ValorImpostoFcp | decimal | ok |
| ValorOperacaoDiferimentoIcms | decimal | ok |
| PercentualDiferimentoIcms | decimal | ok |
| ValorImpostoDiferimentoIcms | decimal | ok |
| CstIpiSaida | ECodigoSituacaoTributariaIpi | ok |
| ValorBaseDeCalculoIpi | decimal | ok |
| AliquotaIpi | decimal | ok |
| ValorImpostoDiferimentoIpi | decimal | ok |
| ValorQuantidadeTotalParaTributacaoIpi | decimal | ok |
| ValorPorUnidadeTributavelIpi | decimal | ok |
| CstPis | ECodigoSituacaoTributariaPisCofins | ok |
| ValorBaseDeCalculoPis | decimal | ok |
| AliquotaPis | decimal | ok |
| ValorQuantidadeVendidaProdutoPis | decimal | ok |
| AliquotaPorUnidadeVendidaPis | decimal | ok |
| ValorImpostoDiferimentoPis | decimal | ok |
| CstCofins | ECodigoSituacaoTributariaPisCofins | ok |
| ValorBaseDeCalculoCofins | decimal | ok |
| AliquotaCofins | decimal | ok |
| ValorQuantidadeVendidaProdutoCofins | decimal | ok |
| AliquotaPorUnidadeVendidaCofins | decimal | ok |
| ValorImpostoDiferimentoCofins | decimal | ok |
| TipoReducaoIcms | ETipoReducaoBaseDeCalculo | ok |
| TipoReducaoIcmsSt | ETipoReducaoBaseDeCalculo | ok |
| ValorBaseDeCalculoFcpSt | decimal | ok |
| PercentualFcpSt | decimal | ok |
| ValorImpostoFcpSt | decimal | ok |
| ValorIcmsProprioSubistituto | decimal | ok |
| ValorAliquotaIcmsInterna | decimal | ok |
| ValorAliquotaIcmsInternaEstadual | decimal | ok |
| EnquadramentoIpi | int | ok |
| ValorReducaoIpiPercentual | decimal | ok |
| IpiEmbutido | bool | ok |
| DifalTipoCalculoPorDentro | bool | ok |
| TipoReducaoIpi | ETipoReducaoBaseDeCalculo | ok |
| TipoCalculoBaseIcmsSt | EDeterminacaoBaseIcmsSt | ok |
| ValorUnitFixadoIcmsSt | decimal | ok |
| ValorBaseDeCalculoDifal | decimal | ok |
| ValorImpostoDevidoDifal | decimal | ok |
| ValorImpostoDevidoRecolherSt | decimal | ok |
| ValorImpostoDevidoFcp | decimal | ok |
| ValorIcmsIsento | decimal | ok |
| ValorIcmsOutros | decimal | ok |
| IcmsObservacao | string? | ok |
| ValorIpiIsento | decimal | ok |
| ValorIpiOutros | decimal | ok |
| IpiObservacao | string? | ok |
| CompraItem | CompraItem | ok |

### CompraItemImpostoIbsCbs

| Campo | Tipo | Status |
|---|---|---|
| CompraItemId | long | ok |
| Cst | string | ok |
| CClassTrib | string | ok |
| AliquotaEstadual | decimal | ok |
| AliquotaMunicipal | decimal | ok |
| AliquotaCbs | decimal | ok |
| AliquotaEstadualReducao | decimal | ok |
| AliquotaMunicipalReducao | decimal | ok |
| AliquotaCbsReducao | decimal | ok |
| AliquotaEstadualDiferimento | decimal | ok |
| AliquotaMunicipalDiferimento | decimal | ok |
| AliquotaCbsDiferimento | decimal | ok |
| AliquotaEfetivaEstadual | decimal | ok |
| AliquotaEfetivaMunicipal | decimal | ok |
| AliquotaEfetivaCbs | decimal | ok |
| ValorBaseDeCalculo | decimal | ok |
| ValorImpostoDevidoEstadual | decimal | ok |
| ValorImpostoDevidoMunicipal | decimal | ok |
| ValorImpostoDevidoCbs | decimal | ok |
| CompraItem | CompraItem | ok |

### CompraItemImpostoValorAproximado

| Campo | Tipo | Status |
|---|---|---|
| CompraItemId | long | ok |
| AliquotaNacionalFederal | decimal | ok |
| AliquotaImportadoFederal | decimal | ok |
| AliquotaEstadual | decimal | ok |
| AliquotaMunicipal | decimal | ok |
| Versao | string? | ok |
| Fonte | string? | ok |
| CompraItem | CompraItem | ok |

### CompraNfe

| Campo | Tipo | Status |
|---|---|---|
| CompraId | long | ok |
| Numero | long | ok |
| Serie | int | ok |
| DataHoraEmissao | DateTime | ok |
| DataHoraSaida | DateTime? | ok |
| StatusInterno | EDocumentoFiscalStatus | ok |
| StatusSefaz | int | ok |
| Chave | string? | ok |
| Protocolo | string? | ok |
| Xml | string? | ok |
| UltimoRetornoMensagemSefaz | string? | ok |
| DataHoraCancelamento | DateTime | ok |
| ProtocoloCancelamento | string? | ok |
| StatusSefazCancelamento | int | ok |
| MotivoCancelamento | string? | ok |
| XmlCancelamento | string? | ok |
| EmbuteFrete | bool | ok |
| EmbuteSeguro | bool | ok |
| EmbuteAcrescimo | bool | ok |
| EmbuteOutro | bool | ok |
| Intermediador | CompraNfeIntermediador? | ok |
| CartasCorrecoes | ICollection<CompraNfeCartaCorrecao> | ok |

### CompraNfeCartaCorrecao

| Campo | Tipo | Status |
|---|---|---|
| CompraNfeId | long | ok |
| TextoCorrecao | string | ok |
| SequenciaEvento | int | ok |
| StatusSefaz | int | ok |
| MotivoRejeicaoSefaz | string? | ok |
| CompraNfe | CompraNfe | ok |

### CompraNfeHistorico

| Campo | Tipo | Status |
|---|---|---|
| CompraId | long | ok |

### CompraNfeIntermediador

| Campo | Tipo | Status |
|---|---|---|
| CompraNfeId | long | ok |
| Documento | Documento | ok |
| IdentificadorIntermediador | string? | ok |
| CompraNfe | CompraNfe | ok |

### CompraNfeReferenciada

| Campo | Tipo | Status |
|---|---|---|
| CompraId | long | ok |
| Chave | string | ok |
| Compra | Compra | ok |

### CompraPagamento

| Campo | Tipo | Status |
|---|---|---|
| CompraId | long | ok |
| ValorTroco | decimal | ok |
| IndicadorPagamento | EIndicadorPagamento | ok |
| TipoPagamento | ETipoPagamento | ok |
| ValorPagamento | decimal | ok |
| CartaoTipoIntegracao | ETipoIntegracaoPagamentoCArtao | ok |
| CartaoCnpjIntermediadorFinanceira | CNPJ? | ok |
| CartaoBandeira | EBandeiraCartao | ok |
| CartaoCodigoAutorizacaoOperacao | string? | ok |

### CompraTotal

| Campo | Tipo | Status |
|---|---|---|
| CompraId | long | ok |
| ValorBaseDeCalculoIcms | decimal | ok |
| ValorIcms | decimal | ok |
| ValorIcmsDesonerado | decimal | ok |
| ValorFcp | decimal | ok |
| ValorBaseDeCalculoSt | decimal | ok |
| ValorSt | decimal | ok |
| ValorFcpSt | decimal | ok |
| ValorFcpRetido | decimal | ok |
| ValorProduto | decimal | ok |
| ValorFrete | decimal | ok |
| ValorSeguro | decimal | ok |
| ValorDesconto | decimal | ok |
| ValorImpostoImportacao | decimal | ok |
| ValorIpi | decimal | ok |
| ValorIpiDevolucao | decimal | ok |
| ValorPis | decimal | ok |
| ValorCofins | decimal | ok |
| ValorOutro | decimal | ok |
| ValorNotaFiscal | decimal | ok |
| Compra | Compra | ok |

### CompraTotalIbsCbs

| Campo | Tipo | Status |
|---|---|---|
| CompraId | long | ok |
| ValorBaseDeCalculo | decimal | ok |
| ValorImpostoDevidoEstadual | decimal | ok |
| ValorImpostoDevidoMunicipal | decimal | ok |
| ValorImpostoDevidoCbs | decimal | ok |
| Compra | Compra | ok |

### CompraTransporte

| Campo | Tipo | Status |
|---|---|---|
| CompraId | long | ok |
| Transportadora | CompraTransporteTransportadora? | ok |
| Veiculo | CompraTransporteVeiculo? | ok |
| Volumes | ICollection<CompraTransporteVolume>? | ok |
| Reboques | ICollection<CompraTransporteReboque>? | ok |

### CompraTransporteReboque

| Campo | Tipo | Status |
|---|---|---|
| CompraTransporteId | long | ok |
| VeiculoId | long? | ok |
| Placa | string | ok |
| Uf | EEstado | ok |
| Rntrc | string? | ok |
| Transporte | CompraTransporte | ok |
| Veiculo | PessoaVeiculo | ok (realocado) |

### CompraTransporteTransportadora

| Campo | Tipo | Status |
|---|---|---|
| CompraTransporteId | long | ok |
| PessoaId | long? | ok |
| Cnpj | CNPJ? | ok |
| Cpf | CPF? | ok |
| RazaoSocial | string? | ok |
| InscricaoEstadual | string? | ok |
| EnderecoCompleto | string? | ok |
| NomeMunicipio | string? | ok |
| Uf | EEstado | ok |
| Transportadora | PessoaTransportadora? | ok (realocado) |

### CompraTransporteVeiculo

| Campo | Tipo | Status |
|---|---|---|
| CompraTransporteId | long | ok |
| VeiculoId | long? | ok |
| Placa | string | ok |
| Uf | EEstado | ok |
| Rntrc | string? | ok |
| Transporte | CompraTransporte | ok |
| Veiculo | PessoaVeiculo | ok (realocado) |

### CompraTransporteVolume

| Campo | Tipo | Status |
|---|---|---|
| CompraTransporteId | long | ok |
| QuantidadeVolumes | int | ok |
| Especie | string? | ok |
| NumeroVolumes | string? | ok |
| PesoLiquido | decimal | ok |
| PesoBruto | decimal | ok |
| Marca | string? | ok |
| CompraTransporte | CompraTransporte | ok |

### ConfiguracaoCodigoNaturezaFinanceira

| Campo | Tipo | Status |
|---|---|---|
| SequenciaTenantId | long | **AUSENTE** |
| EmpresaId | long | ok |
| Descricao | string | ok |
| ItemPlanoDeContasFinanceiroDinheiroId | long? | ok |
| ItemPlanoDeContasFinanceiroCartaoChequeId | long? | ok |
| ItemPlanoDeContasFinanceiroCartaoCreditoId | long? | ok |
| ItemPlanoDeContasFinanceiroCartaoDebitoId | long? | ok |
| ItemPlanoDeContasFinanceiroCartaoDaLojaId | long? | ok |
| ItemPlanoDeContasFinanceiroValeAlimentacaoId | long? | ok |
| ItemPlanoDeContasFinanceiroValeRefeicaoId | long? | ok |
| ItemPlanoDeContasFinanceiroValePresenteId | long? | ok |
| ItemPlanoDeContasFinanceiroValeCombustivelId | long? | ok |
| ItemPlanoDeContasFinanceiroDuplicataMercantilId | long? | ok |
| ItemPlanoDeContasFinanceiroBoletoBancarioId | long? | ok |
| ItemPlanoDeContasFinanceiroDepositoBancarioId | long? | ok |
| ItemPlanoDeContasFinanceiroPagamentoInstantaneoPixDinamicoId | long? | ok |
| ItemPlanoDeContasFinanceiroTransferenciaBancariaId | long? | ok |
| ItemPlanoDeContasFinanceiroProgramaDeFidelidadeId | long? | ok |
| ItemPlanoDeContasFinanceiroPagamentoInstantaneoPixEstaticoId | long? | ok |
| ItemPlanoDeContasFinanceiroCreditoEmLojaId | long? | ok |
| ItemPlanoDeContasFinanceiroPagamentoEletronicoNaoInformadoId | long? | ok |
| ItemPlanoDeContasFinanceiroOutrosId | long? | ok |
| ItemPlanoDeContasFinanceiroDescontoId | long? | ok |
| ItemPlanoDeContasFinanceiroAcrescimoId | long? | ok |
| ItemPlanoDeContasFinanceiroJurosId | long? | ok |
| ItemPlanoDeContasFinanceiroMultaId | long? | ok |
| ItemPlanoDeContasFinanceiroTrocoId | long? | ok |
| TipoConfiguracaoNatureza | ETipoConfiguracaoNatureza | ok |
| Empresa | Empresa | **AUSENTE** |
| ItemPlanoDeContasFinanceiroDinheiro | PlanoDeContasFinanceiroItem? | ok |
| ItemPlanoDeContasFinanceiroCartaoCheque | PlanoDeContasFinanceiroItem? | ok |
| ItemPlanoDeContasFinanceiroCartaoCredito | PlanoDeContasFinanceiroItem? | ok |
| ItemPlanoDeContasFinanceiroCartaoDebito | PlanoDeContasFinanceiroItem? | ok |
| ItemPlanoDeContasFinanceiroCartaoDaLoja | PlanoDeContasFinanceiroItem? | ok |
| ItemPlanoDeContasFinanceiroValeAlimentacao | PlanoDeContasFinanceiroItem? | ok |
| ItemPlanoDeContasFinanceiroValeRefeicao | PlanoDeContasFinanceiroItem? | ok |
| ItemPlanoDeContasFinanceiroValePresente | PlanoDeContasFinanceiroItem? | ok |
| ItemPlanoDeContasFinanceiroValeCombustivel | PlanoDeContasFinanceiroItem? | ok |
| ItemPlanoDeContasFinanceiroDuplicataMercantil | PlanoDeContasFinanceiroItem? | ok |
| ItemPlanoDeContasFinanceiroBoletoBancario | PlanoDeContasFinanceiroItem? | ok |
| ItemPlanoDeContasFinanceiroDepositoBancario | PlanoDeContasFinanceiroItem? | ok |
| ItemPlanoDeContasFinanceiroPagamentoInstantaneoPixDinamico | PlanoDeContasFinanceiroItem? | ok |
| ItemPlanoDeContasFinanceiroTransferenciaBancaria | PlanoDeContasFinanceiroItem? | ok |
| ItemPlanoDeContasFinanceiroProgramaDeFidelidade | PlanoDeContasFinanceiroItem? | ok |
| ItemPlanoDeContasFinanceiroPagamentoInstantaneoPixEstatico | PlanoDeContasFinanceiroItem? | ok |
| ItemPlanoDeContasFinanceiroCreditoEmLoja | PlanoDeContasFinanceiroItem? | ok |
| ItemPlanoDeContasFinanceiroPagamentoEletronicoNaoInformado | PlanoDeContasFinanceiroItem? | ok |
| ItemPlanoDeContasFinanceiroOutros | PlanoDeContasFinanceiroItem? | ok |
| ItemPlanoDeContasFinanceiroDesconto | PlanoDeContasFinanceiroItem? | ok |
| ItemPlanoDeContasFinanceiroAcrescimo | PlanoDeContasFinanceiroItem? | ok |
| ItemPlanoDeContasFinanceiroJuros | PlanoDeContasFinanceiroItem? | ok |
| ItemPlanoDeContasFinanceiroMulta | PlanoDeContasFinanceiroItem? | ok |
| ItemPlanoDeContasFinanceiroTroco | PlanoDeContasFinanceiroItem? | ok |
| PlanoDeContasFinanceirosRecebimento | ICollection<PlanoDeContasFinanceiro> | **AUSENTE** |
| PlanoDeContasFinanceirosPagamento | ICollection<PlanoDeContasFinanceiro> | **AUSENTE** |

### ConfiguracaoDFe

| Campo | Tipo | Status |
|---|---|---|
| NFeSerieProducao | string | ok |
| NFeUltimoNrProducao | string | ok |
| NFeSerieHomologacao | string | ok |
| NFeUltimoNrHomologacao | string | ok |
| NfceCscProducao | string | ok |
| NfceIdCscProducao | string | ok |
| NfceSerieProducao | string | ok |
| NfceUltimoNrProducao | string | ok |
| NfceCscHomologacao | string | ok |
| NfceIdCscHomologacao | string | ok |
| NfceSerieHomologacao | string | ok |
| NfceUltimoNrHomologacao | string | ok |
| Empresa | Empresa | **AUSENTE** |

### ConfiguracaoImpressaoNfce

| Campo | Tipo | Status |
|---|---|---|
| SequenciaTenantId | long | **AUSENTE** |
| EmpresaId | long | ok |
| DetalheVendaNormal | ENfceDetalheVendaNormal? | ok |
| DetalheVendaContingencia | ENfceDetalheVendaContingencia? | ok |
| ImprimeDescontoItem | bool? | ok |
| ImprimeFoneEmitente | bool? | ok |
| MargemEsquerda | float? | ok |
| MargemDireita | float? | ok |
| ModoImpressao | ENfceModoImpressao? | ok |
| NfceLayoutQrCode | ENfceLayoutQrCode? | ok |
| VersaoQrCode | EVersaoQrCode? | ok |
| SegundaViaContingencia | bool? | ok |
| Empresa | Empresa | **AUSENTE** |

### ContaBancaria

| Campo | Tipo | Status |
|---|---|---|
| SequenciaTenantId | long | **AUSENTE** |
| EmpresaID | long | ok (realocado) |
| BancoID | long | ok (realocado) |
| TipoContaBancaria | ETipoContaBancaria | ok |
| Apelido | string | ok |
| Titular | string | ok |
| Agencia | string | ok |
| Conta | string | ok |
| Gerente | string? | ok |
| FoneGerente | string? | ok |
| Detalhe | string? | ok |
| DigitoAgencia | string? | ok |
| DataEncerramento | DateTime? | ok |
| Banco | Banco | ok |
| Empresa | Empresa | **AUSENTE** |
| ContasAReceberItem | ContasAReceberItem | **AUSENTE** |

### Contador

| Campo | Tipo | Status |
|---|---|---|
| SequenciaTenantId | long | **AUSENTE** |
| EnderecoId | long | ok (realocado) |
| RazaoSocial | string? | ok |
| NomeContador | string? | ok |
| Cpf | CPF? | ok |
| Cnpj | CNPJ? | ok |
| NumeroCrc | string | ok |
| UfCrc | EEstado | ok |
| DataVencimentoCrc | DateTime | ok |
| Qualificacao | string? | ok |
| Funcao | string? | ok |
| Telefone | string? | ok |
| Email | Email? | ok |
| PermissaoTransmissao | EPermissaoTransmissao | ok |
| Endereco | Endereco | ok (realocado) |
| Empresas | ICollection<Empresa> | ok (realocado) |

### ContasAPagar

| Campo | Tipo | Status |
|---|---|---|
| SequenciaTenantId | long | **AUSENTE** |
| PessoaId | long | ok |
| PlanoDeContasFinanceiroItemId | long | ok |
| FatoGeradorFinanceiroId | long | ok |
| NomePessoa | string? | ok |
| Situacao | ESituacao | ok |
| DataVencimento | DateTime | ok |
| DataEmissao | DateTime | ok |
| DataBaixa | DateTime? | ok |
| Documento | string | ok |
| ValorTitulo | decimal | ok |
| ValorTotalDesconto | decimal | ok |
| ValorTotalMulta | decimal | ok |
| ValorTotalJuros | decimal | ok |
| ValorTotalTroco | decimal | ok |
| ValorTotalAcrescimo | decimal | ok |
| ValorTotalPago | decimal | ok |
| ValorTotalAPagarTitulo | decimal | ok |
| ValorInicialDesconto | decimal | ok |
| ValorInicialMulta | decimal | ok |
| ValorInicialJuros | decimal | ok |
| ValorInicialAcrescimo | decimal | ok |
| ValorInicialAPagarTitulo | decimal | ok |
| NumeroParcela | int | ok |
| Detalhamento | string? | ok |
| JustificativaCancelamento | string? | ok |
| Pessoa | Pessoa | **AUSENTE** |
| PlanoDeContasFinanceiroItem | PlanoDeContasFinanceiroItem | ok |
| FatoGeradorFinanceiro | FatoGeradorFinanceiro | ok |
| ContasAPagarItens | ICollection<ContasAPagarItem>? | ok |

### ContasAPagarItem

| Campo | Tipo | Status |
|---|---|---|
| ContasAPagarId | long | ok |
| PlanoDeContasFinanceiroItemId | long | ok |
| ContaBancariaId | long? | ok |
| TipoPagamento | ETipoPagamento | ok |
| ValorParcela | decimal | ok |
| ValorPago | decimal | ok |
| ValorDesconto | decimal | ok |
| ValorMulta | decimal | ok |
| ValorJuros | decimal | ok |
| ValorTroco | decimal | ok |
| ValorAcrescimo | decimal | ok |
| ValorAPagar | decimal | ok |
| DataPagamento | DateTime | ok |
| ContasAPagar | ContasAPagar | ok |
| PlanoDeContasFinanceiroItem | PlanoDeContasFinanceiroItem | ok |
| ConfiguracaoCodigoNaturezaFinanceira | ConfiguracaoCodigoNaturezaFinanceira | **AUSENTE** |
| ContaBancaria | ContaBancaria? | ok |

### ContasAReceber

| Campo | Tipo | Status |
|---|---|---|
| SequenciaTenantId | long | **AUSENTE** |
| PessoaId | long | ok |
| PlanoDeContasFinanceiroItemId | long | ok |
| FatoGeradorFinanceiroId | long | ok |
| NomePessoa | string? | ok |
| Situacao | ESituacao | ok |
| DataVencimento | DateTime | ok |
| DataEmissao | DateTime | ok |
| DataBaixa | DateTime? | ok |
| Documento | string | ok |
| ValorTitulo | decimal | ok |
| ValorTotalDesconto | decimal | ok |
| ValorTotalMulta | decimal | ok |
| ValorTotalJuros | decimal | ok |
| ValorTotalTroco | decimal | ok |
| ValorTotalAcrescimo | decimal | ok |
| ValorTotalRecebido | decimal | ok |
| ValorTotalAReceberTitulo | decimal | ok |
| ValorInicialDesconto | decimal | ok |
| ValorInicialMulta | decimal | ok |
| ValorInicialJuros | decimal | ok |
| ValorInicialAcrescimo | decimal | ok |
| ValorInicialAReceberTitulo | decimal | ok |
| NumeroParcela | int | ok |
| Detalhamento | string? | ok |
| JustificativaCancelamento | string? | ok |
| Pessoa | Pessoa | **AUSENTE** |
| PlanoDeContasFinanceiroItem | PlanoDeContasFinanceiroItem | ok |
| FatoGeradorFinanceiro | FatoGeradorFinanceiro | ok |
| ContasAReceberItens | ICollection<ContasAReceberItem>? | ok |

### ContasAReceberItem

| Campo | Tipo | Status |
|---|---|---|
| ContasAReceberId | long | ok |
| PlanoDeContasFinanceiroItemId | long | ok |
| ContaBancariaId | long? | ok |
| TipoPagamento | ETipoPagamento | ok |
| ValorParcela | decimal | ok |
| ValorPago | decimal | ok |
| ValorDesconto | decimal | ok |
| ValorMulta | decimal | ok |
| ValorJuros | decimal | ok |
| ValorTroco | decimal | ok |
| ValorAcrescimo | decimal | ok |
| ValorAReceber | decimal | ok |
| DataRecebimento | DateTime | ok |
| ContasAReceber | ContasAReceber | ok |
| PlanoDeContasFinanceiroItem | PlanoDeContasFinanceiroItem | ok |
| ConfiguracaoCodigoNaturezaFinanceira | ConfiguracaoCodigoNaturezaFinanceira | **AUSENTE** |
| ContaBancaria | ContaBancaria? | ok |

### CstIbsCbs

| Campo | Tipo | Status |
|---|---|---|
| Cst | string | ok |
| Descricao | string | ok |
| DataInicioVigencia | DateTime | ok |
| DataFimVigencia | DateTime? | ok |
| ClassesTributarias | List<ClassificacaoTributaria> | ok |

### Empresa

| Campo | Tipo | Status |
|---|---|---|
| PessoaGrupoId | long | ok |
| ProdutoGrupoId | long | ok |
| PlanoContasFinanceiroId | long? | ok |
| TributarioGrupoId | long | ok |
| NcmTributacaoId | long? | ok |
| CertificadoDigitalId | long? | ok |
| EmpresaParametrosDfeId | long | ok |
| ContadorId | long? | ok |
| RazaoSocial | string | ok |
| NomeFantasia | string? | ok |
| RegimeApuracao | ERegimeApuracao | ok |
| RegimeTributario | ERegimeTributario | ok |
| Cnpj | CNPJ? | ok |
| Cpf | CPF? | ok |
| InscricaoMunicipal | string? | ok |
| InscricaoEstadual | string? | ok |
| Cnae | int? | ok |
| InscricaoSuframa | string? | ok |
| LinkWebApiAppVendas | string? | ok |
| TokenMercadoPagoPix | string? | ok |
| Logo | string? | ok |
| EhIndustria | bool | ok |
| Endereco | EmpresaEndereco | ok |
| EmpresaParametrosDfe | EmpresaParametrosDfe | ok |
| SequenciaTenantId | long | **AUSENTE** |
| TipoConfiguracaoEstoque | ETipoConfiguracaoEstoque | ok |
| IeSts | ICollection<IeSt> | ok |
| Contatos | ICollection<EmpresaContato> | ok |
| ContasBancarias | ICollection<ContaBancaria> | **AUSENTE** |
| PessoaGrupo | PessoaGrupo | **AUSENTE** |
| ProdutoGrupo | ProdutoGrupo | ok (realocado) |
| CertificadoDigital | CertificadoDigital | **AUSENTE** |
| ConfiguracaoCodigoNaturezaFinanceiras | ICollection<ConfiguracaoCodigoNaturezaFinanceira> | **AUSENTE** |
| TributarioGrupo | TributarioGrupo | ok (realocado) |
| NcmTributacao | NcmTributacao | **AUSENTE** |
| PlanoContasFinanceiro | PlanoDeContasFinanceiro | **AUSENTE** |
| ConfiguracaoImpressaoNfce | ConfiguracaoImpressaoNfce | **AUSENTE** |
| UsuariosEmpresas | ICollection<UsuarioEmpresa> | **AUSENTE** |
| Contador | Contador? | **AUSENTE** |
| Servico | Servico? | **AUSENTE** |
| ImportacaoXmls | ICollection<ImportacaoXml> | **AUSENTE** |

### EmpresaContato

| Campo | Tipo | Status |
|---|---|---|
| EmpresaId | long | ok |
| Nome | string | ok |
| Email | Email? | ok |
| TipoTelefone | ETipoContatoTelefonico? | ok |
| Telefone | string? | ok |

### EmpresaEndereco

| Campo | Tipo | Status |
|---|---|---|
| TipoEndereco | ETipoEndereco | ok (realocado) |
| Cep | CEP | ok (realocado) |
| Uf | EEstado | ok (realocado) |
| MunicipioId | int | ok (realocado) |
| Logradouro | string | ok (realocado) |
| Complemento | string? | ok (realocado) |
| Numero | string | ok (realocado) |
| Bairro | string | ok (realocado) |
| Municipio | Municipio | ok (realocado) |

### EmpresaParametrosDfe

| Campo | Tipo | Status |
|---|---|---|
| DestacarIcmsSt | bool | ok |
| Nfe | EmpresaParametrosDfeNfe | ok |
| NfceHomologacao | EmpresaParametrosDfeNfceHomologacao | ok |
| NfceProdutocao | EmpresaParametrosDfeNfceProducao | **AUSENTE** |
| TipoAmbienteNfce | ETipoAmbiente | ok |
| TipoAmbienteNfe | ETipoAmbiente | ok |
| Empresa | Empresa | **AUSENTE** |

### EmpresaParametrosDfeNfceHomologacao

| Campo | Tipo | Status |
|---|---|---|
| NfceCscHomologacao | string? | ok (realocado) |
| NfceIdCscHomologacao | string? | ok (realocado) |
| NfceSerieHomologacao | int | ok (realocado) |
| NfceProximoNrHomologacao | long | ok (realocado) |
| NfceGerarContingenciaEmHomologacao | bool | ok (realocado) |

### EmpresaParametrosDfeNfceProducao

| Campo | Tipo | Status |
|---|---|---|
| NfceCscProducao | string? | ok (realocado) |
| NfceIdCscProducao | string? | ok (realocado) |
| NfceSerieProducao | int | ok (realocado) |
| NfceProximoNrProducao | long | ok (realocado) |

### EmpresaParametrosDfeNfe

| Campo | Tipo | Status |
|---|---|---|
| NfeSerieProducao | int | ok (realocado) |
| NfeProximoNrProducao | long | ok (realocado) |
| NfeSerieHomologacao | int | ok (realocado) |
| NfeProximoNrHomologacao | long | ok (realocado) |
| ValorAliquotaCreditoIcms | decimal | ok (realocado) |
| NfeGerarContingenciaEmHomologacao | bool | ok (realocado) |
| IndicadorSt | bool | ok (realocado) |
| EmitirNfeConjugada | bool | ok (realocado) |

### Endereco

| Campo | Tipo | Status |
|---|---|---|
| PaisId | int | ok |
| MunicipioId | int | ok |
| TipoEndereco | ETipoEndereco | ok |
| Cep | CEP | ok |
| Uf | EEstado | ok |
| Logradouro | string | ok |
| Complemento | string? | ok |
| Numero | string | ok |
| Bairro | string | ok |
| Referencia | string? | ok |
| NomeDoRecebedor | string? | ok |
| DocumentoDoRecebedor | string? | ok |
| Pessoas | ICollection<Pessoa> | **AUSENTE** |
| Pais | Pais | ok |
| Municipio | Municipio | ok |
| Contador | Contador | **AUSENTE** |

### EnquadramentoIpi

| Campo | Tipo | Status |
|---|---|---|
| Codigo | string | ok |
| Descricao | string | ok |
| TipoOperacao | ETipoOperacaoEnquadramentoIpi | ok |

### EstoqueMovimentoManual

| Campo | Tipo | Status |
|---|---|---|
| ProdutoId | long | ok |
| TipoEstoque | ETipoEstoque | ok |
| TipoMovimento | ETipoMovimento | ok |
| QuantidadeMovimentada | decimal | ok |
| ValorUnitario | decimal | ok |
| Produto | Produto | ok |
| FatoGeradorEstoque | FatoGeradorEstoque | ok |

### EstoqueProduto

| Campo | Tipo | Status |
|---|---|---|
| EmpresaId | long | ok |
| ProdutoId | long | ok |
| QuantidadeSaldoEstoque | decimal | ok |
| QuantidadeEstoqueMinimo | decimal | ok |
| QuantidadeEstoqueMaximo | decimal | ok |
| QuantidadeEstoqueReservado | decimal | ok |
| ValorSaldo | decimal | ok |
| ValorCustoMedio | decimal | ok |
| TipoCusteioEstoque | ETipoCusteioEstoque | ok |
| Empresa | Empresa | **AUSENTE** |
| Produto | Produto | ok |

### FatoGeradorEstoque

| Campo | Tipo | Status |
|---|---|---|
| VendaId | long? | ok |
| CompraId | long? | ok |
| EstoqueMovimentoManualId | long? | ok |
| Origem | EOrigem | ok |
| ProdutoFichaEstoqueEntradas | ICollection<ProdutoFichaEstoqueEntrada> | ok |
| ProdutoFichaEstoqueSaidas | ICollection<ProdutoFichaEstoqueSaida> | ok |
| Venda | Venda? | ok (realocado) |
| Compra | Compra? | ok |
| EstoqueMovimentoManual | EstoqueMovimentoManual? | ok |

### FatoGeradorFinanceiro

| Campo | Tipo | Status |
|---|---|---|
| Origem | EOrigem | ok |
| VendaId | long? | ok |
| CompraId | long? | ok |
| Descricao | string? | ok |
| ContasARecebers | ICollection<ContasAReceber> | ok |
| ContasAPagars | ICollection<ContasAPagar> | ok |
| Venda | Venda | ok (realocado) |
| Compra | Compra | ok (realocado) |

### FcpAliquotaUf

| Campo | Tipo | Status |
|---|---|---|
| Uf | EEstado | ok |
| ValorAliquota | decimal | ok |
| Observacao | string? | ok |

### IcmsAliquotaInterestadual

| Campo | Tipo | Status |
|---|---|---|
| UfOrigem | EEstado | ok |
| UfDestino | EEstado | ok |
| ValorAliquota | decimal | ok |

### IeSt

| Campo | Tipo | Status |
|---|---|---|
| EmpresaId | long | ok |
| Uf | EEstado | ok |
| Ie | string | ok |

### ImportacacaoArquivoOfx

| Campo | Tipo | Status |
|---|---|---|
| CodigoBanco | string | ok |
| NumeroConta | string | ok |
| TipoConta | string | ok |
| DataInicioExtrato | DateTime | ok |
| DataFimExtrato | DateTime | ok |
| Transacoes | ICollection<ImportacacaoArquivoOfxTransacao> | ok |

### ImportacacaoArquivoOfxTransacao

| Campo | Tipo | Status |
|---|---|---|
| ImportacacaoArquivoOfxId | long | ok |
| ContasAReceberId | long? | ok |
| ContasAPagarId | long? | ok |
| IdentificadorTransacao | string | ok |
| Data | DateTime | ok |
| Valor | decimal | ok |
| Tipo | string | ok |
| Descricao | string? | ok |
| Conciliado | bool | ok |
| ImportacacaoArquivoOfx | ImportacacaoArquivoOfx | ok |
| ContasAReceber | ContasAReceber? | ok |
| ContasAPagar | ContasAPagar? | ok |

### ImportacaoArquivoXmlSaida

| Campo | Tipo | Status |
|---|---|---|
| NomeArquivo | string | ok |
| QtdXmls | int | ok |
| QtdXmlsInvalidos | int | ok |
| QtdProdutosLocalizados | int | ok |
| QtdClientesLocalizados | int | ok |
| QtdProdutosImportados | int | ok |
| QtdClientesImportados | int | ok |
| MensagemErro | string | ok |
| Status | EImportacaoArquivoXmlSaidaStatus | ok |

### ImportacaoXml

| Campo | Tipo | Status |
|---|---|---|
| EmpresaId | long? | ok |
| Xml | string | ok |
| TipoDeXml | ETipoXml | ok |
| NfeId | string | ok |
| StatusImportacaoXml | EStatusProcessamento | ok |
| MensagemErroImportacaoXml | string? | ok |
| StatusCadastro | EStatusProcessamento | ok |
| MensagemErroCadastro | string? | ok |
| StatusSalvarPdf | EStatusProcessamento | ok |
| MensagemErroSalvarPdf | string? | ok |
| CodigoSefaz | int | ok |
| TipoEvento | string | ok |
| Empresa | Empresa | **AUSENTE** |

### MarcaProduto

| Campo | Tipo | Status |
|---|---|---|
| SequenciaTenantId | long | **AUSENTE** |
| ProdutoGrupoId | long | ok |
| Descricao | string | ok |
| Produtos | ICollection<Produto> | ok (realocado) |
| ProdutoGrupo | ProdutoGrupo | ok |

### Menu

| Campo | Tipo | Status |
|---|---|---|
| Descricao | string | ok |
| Icon | string? | ok |
| To | string? | ok |
| Ordem | int | ok |
| Itens | ICollection<MenuItemNivel1> | ok (realocado) |

### MenuItemNivel1

| Campo | Tipo | Status |
|---|---|---|
| MenuId | long | ok |
| Descricao | string | ok |
| Icon | string? | ok |
| To | string? | ok |
| Ordem | int | ok |
| Itens | ICollection<MenuItemNivel2> | ok (realocado) |
| Menu | Menu | **AUSENTE** |

### MenuItemNivel2

| Campo | Tipo | Status |
|---|---|---|
| MenuItemNivel1Id | long | ok |
| Descricao | string | ok |
| Icon | string? | ok |
| To | string? | ok |
| Ordem | int | ok |
| MenuItemNivel1 | MenuItemNivel1 | **AUSENTE** |

### Municipio

| Campo | Tipo | Status |
|---|---|---|
| Estado | EEstado | **AUSENTE** |
| Nome | string | ok |
| Empresa | Empresa | **AUSENTE** |
| Endereco | ICollection<Endereco> | ok (realocado) |

### Ncm

| Campo | Tipo | Status |
|---|---|---|
| CodigoNcm | string | ok |
| Descricao | string | ok |
| DataInicio | DateTime | ok |
| DataFim | DateTime? | ok |
| TipoAtoIni | string? | ok |
| NumeroAtoIni | string? | ok |
| AnoAtoIni | string? | ok |
| Produtos | ICollection<Produto> | ok (realocado) |
| NcmConfiguracao | NcmConfiguracao | **AUSENTE** |

### NcmConfiguracao

| Campo | Tipo | Status |
|---|---|---|
| SequenciaTenantId | long | **AUSENTE** |
| NcmId | long | ok |
| NcmTributacaoId | long | ok |
| Ncm | Ncm | ok (realocado) |
| NcmTributacao | NcmTributacao | **AUSENTE** |

### NcmTributacao

| Campo | Tipo | Status |
|---|---|---|
| SequenciaTenantId | long | **AUSENTE** |
| TributarioGrupoId | long | ok |
| CodigoBeneficioFiscalId | long? | ok |
| CodRegra | int | ok |
| Descricao | string | ok |
| CfopNotaConsumidor | int | ok |
| CfopNotaFiscal | int | ok |
| CfopNotaFiscalInterestadual | int | ok |
| Origem | EOrigemMercadoria | ok |
| CsosnNotaConsumidor | ECodigoSituacaoOperacaoSimplesNacional | ok |
| CstIcmsNotaConsumidor | ECodigoSituacaoTributariaIcms | ok |
| CsosnNotaFiscal | ECodigoSituacaoOperacaoSimplesNacional | ok |
| CstIcmsNotaFiscalInterna | ECodigoSituacaoTributariaIcms | ok |
| CstIcmsNotaFiscalInterstadual | ECodigoSituacaoTributariaIcms | ok |
| CstPis | ECodigoSituacaoTributariaPisCofins | ok |
| CstCofins | ECodigoSituacaoTributariaPisCofins | ok |
| ValorUnitFixoPis | decimal | ok |
| ValorUnitFixoCofins | decimal | ok |
| ValorAliquotaPis | decimal | ok |
| ValorAliquotaCofins | decimal | ok |
| CstPisCofinsEntrada | ECodigoSituacaoTributariaPisCofins | ok |
| CstIpiSaida | ECodigoSituacaoTributariaIpi | ok |
| CstIpiEntrada | ECodigoSituacaoTributariaIpi | ok |
| ValorAliquotaIpi | decimal | ok |
| ValorPercentualReducacaoBcIpi | decimal | ok |
| TipoReducaoIpi | ETipoReducaoBaseDeCalculo | ok |
| DestinoReducaoIpi | EDestinoReducao | ok |
| IpiEmbutido | bool | ok |
| EnquadramentoIpi | string? | ok |
| CodigoValorFiscalIcmsInterna | ECodigoValorFiscalIcms | ok |
| CodigoValorFiscalcmsInterstadual | ECodigoValorFiscalIcms | ok |
| ValorAliquotaIcmsInterna | decimal | ok |
| ValorPercentualReducacaoBcIcmsInterna | decimal | ok |
| TipoReducaoIcmsInterna | ETipoReducaoBaseDeCalculo | ok |
| DestinoReducaoIcmsInterna | EDestinoReducao | ok |
| ValorAliquotaIcmsInterstadual | decimal | ok |
| ValorPercentualReducacaoBcIcmsInterstadual | decimal | ok |
| TipoReducaoIcmsInterstadual | ETipoReducaoBaseDeCalculo | ok |
| DestinoReducaoIcmsInterstadual | EDestinoReducao | ok |
| CodigoBeneficioFiscalIcms | string? | ok |
| MotivoDesoneracaoIcms | int | ok |
| InformacoesComplementares | string? | ok |
| InformacoesAdicionaisAoFisco | string? | ok |
| CstIbsCbsNfe | string? | ok |
| CClassTribNfe | string? | ok |
| CstIbsCbsNfce | string? | ok |
| CClassTribNfce | string? | ok |
| TributarioGrupo | TributarioGrupo | ok |
| NcmConfiguracoes | ICollection<NcmConfiguracao> | ok |
| NcmTributacaoSts | ICollection<NcmTributacaoSt> | ok |
| NcmTributacaoFundoCombatePobrezas | ICollection<NcmTributacaoFundoCombatePobreza> | ok |
| Empresas | ICollection<Empresa> | ok |
| CodigoBeneficioFiscal | CodigoBeneficioFiscal? | ok |

### NcmTributacaoFundoCombatePobreza

| Campo | Tipo | Status |
|---|---|---|
| NcmTributacaoId | long | ok |
| Uf | string | ok |
| ValorPercentual | decimal | ok |
| NcmTributacao | NcmTributacao | **AUSENTE** |

### NcmTributacaoSt

| Campo | Tipo | Status |
|---|---|---|
| NcmTributacaoId | long | ok |
| Uf | string | ok |
| TipoCalculo | ETipoCalculo | ok |
| ValorAliquotaIcmsSt | decimal | ok |
| ValorMva | decimal | ok |
| ValorPercentualReducaoBcIcmsSt | decimal | ok |
| TipoReducaoIcmsSt | int | ok |
| ValorUnitarioSt | decimal | ok |
| ValorPercentualFcpSt | decimal | ok |
| NcmTributacao | NcmTributacao | **AUSENTE** |

### ObservacaoNfe

| Campo | Tipo | Status |
|---|---|---|
| SequenciaTenantId | long | **AUSENTE** |
| Descricao | string | ok |

### Pais

| Campo | Tipo | Status |
|---|---|---|
| Nome | string | ok |
| Capital | string? | ok |

### PerfilUsuario

| Campo | Tipo | Status |
|---|---|---|
| Descricao | string | ok (realocado) |
| Acessos | ICollection<PerfilUsuarioAcesso> | ok (realocado) |
| Usuarios | List<Usuario> | **AUSENTE** |

### PerfilUsuarioAcesso

| Campo | Tipo | Status |
|---|---|---|
| PerfilUsuarioId | long | **AUSENTE** |
| MenuId | long | ok (realocado) |
| MenuItemNivel1Id | long | ok (realocado) |
| MenuItemNivel2Id | long? | ok (realocado) |
| Ver | bool | ok (realocado) |
| Editar | bool | ok (realocado) |
| Excluir | bool | ok (realocado) |
| Menu | Menu | **AUSENTE** |
| MenuItemNivel1 | MenuItemNivel1 | **AUSENTE** |
| MenuItemNivel2 | MenuItemNivel2 | **AUSENTE** |
| PerfilUsuario | PerfilUsuario | **AUSENTE** |

### Pessoa

| Campo | Tipo | Status |
|---|---|---|
| SequenciaTenantId | long | **AUSENTE** |
| PessoaGrupoId | long | ok |
| TipoPessoa | ETipoPessoa | ok |
| TipoIndicadorIe | ETipoIndicadorIe | ok |
| InscricaoSuframa | int? | ok |
| TitularContaBancaria | string? | ok |
| AgenciaContaBancaria | string? | ok |
| NumeroContaBancaria | string? | ok |
| TipoPix | ETipoPix? | ok |
| ChavePix | string? | ok |
| Observacoes | string? | ok |
| EhCliente | bool | ok |
| EhFuncionario | bool | ok |
| EhMotorista | bool | ok |
| EhPrestadorServico | bool | ok |
| EhProdutorRural | bool | ok |
| EhTransportadora | bool | ok |
| EhFornecedor | bool | ok |
| EhInativo | bool | ok |
| PessoaFisica | PessoaFisica? | ok |
| PessoaJuridica | PessoaJuridica? | ok |
| PessoaEstrangeiro | PessoaEstrangeiro? | ok |
| PessoaGrupo | PessoaGrupo | **AUSENTE** |
| Enderecos | ICollection<Endereco> | ok |
| Contatos | ICollection<PessoaContato> | ok |
| PessoaCliente | PessoaCliente? | ok |
| PessoaPrestadorServico | PessoaPrestadorServico? | ok |
| PessoaMotorista | PessoaMotorista? | ok |
| PessoaTransportadora | PessoaTransportadora? | ok |
| PessoaFuncionario | PessoaFuncionario? | ok |
| ContasARecebers | ICollection<ContasAReceber> | ok (realocado) |
| EhEstrangeiro | bool | **AUSENTE** |

### PessoaCliente

| Campo | Tipo | Status |
|---|---|---|
| PessoaId | long | ok |
| EhConsumidorFinal | bool | ok |
| TipoContribuinte | ETipoContribuinte | ok |
| Pessoa | Pessoa | **AUSENTE** |

### PessoaContato

| Campo | Tipo | Status |
|---|---|---|
| SequenciaTenantId | long | **AUSENTE** |
| PessoaId | long | ok |
| Nome | string | ok |
| TipoContatoEmail | ETipoContatoEmail? | ok |
| Email | Email? | ok |
| TipoContatoTelefonico | ETipoContatoTelefonico? | ok |
| NumeroTelefone | string? | ok |
| EhPrincipal | bool | ok |
| Pessoa | Pessoa | **AUSENTE** |

### PessoaEndereco

| Campo | Tipo | Status |
|---|---|---|
| PessoaId | long | ok (realocado) |
| EmpresaId | long | ok (realocado) |
| Pessoa | Pessoa | **AUSENTE** |
| Endereco | Endereco | ok (realocado) |

### PessoaEstrangeiro

| Campo | Tipo | Status |
|---|---|---|
| PessoaId | long | ok |
| Nome | string | ok |
| IdentificacaoEstrangeiro | string? | ok |
| Pessoa | Pessoa | **AUSENTE** |

### PessoaFisica

| Campo | Tipo | Status |
|---|---|---|
| PessoaId | long | ok |
| Cpf | CPF | ok |
| RgNumero | string? | ok |
| RgOrgaoEmissor | string? | ok |
| Nome | string | ok |
| Sobrenome | string | ok |
| TipoGenero | ETipoGenero? | ok |
| Pessoa | Pessoa | **AUSENTE** |

### PessoaFuncionario

| Campo | Tipo | Status |
|---|---|---|
| PessoaId | long | ok |
| TipoCargo | ETipoCargo | ok |
| ValorPercentualComissao | decimal | ok |
| Pessoa | Pessoa | **AUSENTE** |

### PessoaGrupo

| Campo | Tipo | Status |
|---|---|---|
| SequenciaTenantId | long | **AUSENTE** |
| Descricao | string | ok |
| Empresas | ICollection<Empresa> | ok (realocado) |
| Pessoas | ICollection<Pessoa> | **AUSENTE** |

### PessoaJuridica

| Campo | Tipo | Status |
|---|---|---|
| PessoaId | long | ok |
| Cnpj | CNPJ | ok |
| NomeFantasia | string? | ok |
| RazaoSocial | string | ok |
| InscricaoEstadual | string? | ok |
| InscricaoMunicipal | string? | ok |
| Cnae | string? | ok |
| Pessoa | Pessoa | **AUSENTE** |

### PessoaMotorista

| Campo | Tipo | Status |
|---|---|---|
| PessoaId | long | ok |
| TipoVinculoMotorista | ETipoVinculoMotorista | ok |
| TipoCategoriaCnh | ETipoCategoriaCnh? | ok |
| DataEmissaoCnh | DateTime? | ok |
| DataVencimentoCnh | DateTime? | ok |
| Rntrc | string? | ok |
| Pessoa | Pessoa | **AUSENTE** |
| Veiculos | ICollection<PessoaVeiculo>? | ok (realocado) |

### PessoaPrestadorServico

| Campo | Tipo | Status |
|---|---|---|
| PessoaId | long | ok |
| Cei | string? | ok |
| Pessoa | Pessoa | **AUSENTE** |

### PessoaTransportadora

| Campo | Tipo | Status |
|---|---|---|
| PessoaId | long | ok |
| Ciot | string? | ok |
| Rntrc | string? | ok |
| Pessoa | Pessoa | **AUSENTE** |
| Veiculos | ICollection<PessoaVeiculo>? | ok (realocado) |

### PessoaVeiculo

| Campo | Tipo | Status |
|---|---|---|
| SequenciaTenantId | long | **AUSENTE** |
| PessoaMotoristaId | long? | **AUSENTE** |
| PessoaTransportadoraId | long? | **AUSENTE** |
| PaisId | int | ok |
| TipoVeiculo | ETipoVeiculo | ok |
| Uf | EEstado | ok |
| Placa | string | ok |
| Rntrc | string? | ok |
| Pais | Pais | ok (realocado) |
| PessoaMotorista | ICollection<PessoaMotorista>? | ok (realocado) |
| PessoaTransportadora | ICollection<PessoaTransportadora>? | ok (realocado) |

### PlanoDeContasFinanceiro

| Campo | Tipo | Status |
|---|---|---|
| SequenciaTenantId | long | **AUSENTE** |
| ConfiguracaoCodigoNaturezaFinanceiraRecebimentoId | long? | ok |
| ConfiguracaoCodigoNaturezaFinanceiraPagamentoId | long? | ok |
| Descricao | string | ok |
| Mascara | string | ok |
| EhPadrao | bool | ok |
| Itens | ICollection<PlanoDeContasFinanceiroItem> | ok |
| Empresas | ICollection<Empresa> | ok |
| ConfiguracaoCodigoNaturezaFinanceiraRecebimento | ConfiguracaoCodigoNaturezaFinanceira? | ok |
| ConfiguracaoCodigoNaturezaFinanceiraPagamento | ConfiguracaoCodigoNaturezaFinanceira? | ok |

### PlanoDeContasFinanceiroItem

| Campo | Tipo | Status |
|---|---|---|
| SequenciaTenantId | long | **AUSENTE** |
| PlanoDeContasFinanceiroId | long | ok |
| Codigo | string | ok |
| Descricao | string | ok |
| TipoDetalhamento | ETipoDetalhamento | ok |
| MovimentaCaixa | bool | ok |
| PlanoDeContasFinanceiro | PlanoDeContasFinanceiro | ok |
| ContasARecebers | ICollection<ContasAReceber> | ok (realocado) |
| ContasAReceberItens | ICollection<ContasAReceberItem> | ok (realocado) |
| ConfiguracaoCodigoNaturezaFinanceiras | ICollection<ConfiguracaoCodigoNaturezaFinanceira> | **AUSENTE** |

### Produto

| Campo | Tipo | Status |
|---|---|---|
| SequenciaTenantId | long | **AUSENTE** |
| ProdutoGrupoId | long | ok |
| CategoriaId | long? | ok |
| MarcaProdutoId | long? | ok |
| UnidadeMedidaComercialId | long | ok |
| NcmId | long | ok |
| CodigoAnpId | long? | ok |
| CestId | long? | ok |
| BalancaId | long? | ok |
| Codigo | string? | ok |
| Descricao | string | ok |
| Ean | string? | ok |
| PesoLiquido | decimal | ok |
| PesoBruto | decimal | ok |
| ValorVenda | decimal | ok |
| ValorVendaPrazo | decimal | ok |
| ValorCompra | decimal | ok |
| TipoProduto | ETipoProduto? | ok |
| Imagem | string | ok |
| UtilizaBalanca | bool | ok |
| CodigoProdutoBalanca | string? | ok |
| ProdutoGrupo | ProdutoGrupo | ok |
| Categoria | CategoriaProduto | ok |
| MarcaProduto | MarcaProduto | ok |
| Ncm | Ncm | ok (realocado) |
| CodigoAnp | CodigoAnp? | ok (realocado) |
| Cest | Cest? | ok (realocado) |
| AdicionaisProduto | ICollection<AdicionaisProduto>? | ok |
| UnidadeMedidaComercial | UnidadeMedidaComercial | ok |
| ProdutoEspecifico | ProdutoEspecifico? | ok |
| EstoqueProduto | EstoqueProduto | **AUSENTE** |
| ProdutoFichaEstoqueEntradas | ICollection<ProdutoFichaEstoqueEntrada> | ok (realocado) |
| ProdutoFichaEstoqueSaidas | ICollection<ProdutoFichaEstoqueSaida> | ok (realocado) |
| RegistroMovimentoEstoqueManuais | ICollection<EstoqueMovimentoManual> | **AUSENTE** |
| Balanca | Balanca? | ok |
| ProdutoHistoricoReajustes | ICollection<ProdutoHistoricoReajuste> | ok |

### ProdutoEspecifico

| Campo | Tipo | Status |
|---|---|---|
| ProdutoId | long | ok |
| ValorPercentualGlpDerivadoPetroleo | decimal | ok |
| ValorPercentualGasNaturalNacional | decimal | ok |
| ValorPercentualGasNaturalImportado | decimal | ok |
| ValorPartida | decimal | ok |
| UfConsumo | EEstado | ok |
| Origens | ICollection<ProdutoEspecificoCombustivelOrigem>? | ok |
| Produto | Produto | ok |

### ProdutoEspecificoCombustivelOrigem

| Campo | Tipo | Status |
|---|---|---|
| ProdutoEspecificoId | long | ok |
| IndicadorImportacao | EOrigemTributacaoCombustivel | ok |
| UfOrigem | EEstado | ok |
| ValorPercentualUf | decimal | ok |
| ProdutoEspecifico | ProdutoEspecifico | ok |

### ProdutoFichaEstoqueEntrada

| Campo | Tipo | Status |
|---|---|---|
| EmpresaId | long | ok |
| ProdutoId | long | ok |
| FatoGeradorEstoqueId | long | ok |
| TipoEstoque | ETipoEstoque | ok |
| QuantidadeMovimentada | decimal | ok |
| ValorUnitario | decimal | ok |
| QuantidadeSaldo | decimal | ok |
| ValorSaldo | decimal | ok |
| Empresa | Empresa | **AUSENTE** |
| Produto | Produto | ok |
| FatoGeradorEstoque | FatoGeradorEstoque | ok |
| ProdutoFichaEstoqueSaidas | ICollection<ProdutoFichaEstoqueSaida> | ok |

### ProdutoFichaEstoqueSaida

| Campo | Tipo | Status |
|---|---|---|
| EmpresaId | long | ok |
| ProdutoId | long | ok |
| FatoGeradorEstoqueId | long | ok |
| ProdutoFichaEstoqueEntradaId | long | ok |
| QuantidadeMovimentada | decimal | ok |
| ValorUnitario | decimal | ok |
| ValorTotal | decimal | ok |
| ValorCustoMedio | decimal | ok |
| ValorTotalCustoMedio | decimal | ok |
| Empresa | Empresa | **AUSENTE** |
| Produto | Produto | ok |
| FatoGeradorEstoque | FatoGeradorEstoque | ok |
| ProdutoFichaEstoqueEntrada | ProdutoFichaEstoqueEntrada | ok |

### ProdutoGrupo

| Campo | Tipo | Status |
|---|---|---|
| SequenciaTenantId | long | **AUSENTE** |
| Descricao | string | ok |
| Empresas | ICollection<Empresa> | ok |
| Produtos | ICollection<Produto> | ok (realocado) |
| MarcaProdutos | ICollection<MarcaProduto> | **AUSENTE** |
| CategoriaProdutos | ICollection<CategoriaProduto> | **AUSENTE** |

### ProdutoHistoricoReajuste

| Campo | Tipo | Status |
|---|---|---|
| SequenciaTenantId | long | **AUSENTE** |
| ProdutoId | long | ok |
| CodigoProduto | string? | ok |
| ValorAntigo | decimal | ok |
| Tipo | ETipoReajuste | ok |
| Fator | decimal | ok |
| ValorFixo | decimal | ok |
| ValorNovo | decimal | ok |
| Motivo | string? | ok |
| Produto | Produto | ok |

### Servico

| Campo | Tipo | Status |
|---|---|---|
| EmpresaId | long | ok |
| UnidadeMedidaId | long | ok |
| CodigoServicoSefazId | long | ok |
| SequenciaTenantId | long | **AUSENTE** |
| Codigo | string | ok |
| Descricao | string | ok |
| Valor | decimal | ok |
| InformacaoAdicional | string? | ok |
| ServicoAtivo | bool | ok |
| Cnae | int | ok |
| CodigoNbs | string | ok |
| IndicadorIss | bool | ok |
| IndicadorIncentivo | bool | ok |
| CstIbsCbs | string? | ok |
| CClassTrib | string? | ok |
| AliquotaIss | decimal | ok |
| AliquotaIssRetido | decimal | ok |
| AliquotaIrrfRetido | decimal | ok |
| AliquotaInss | decimal | ok |
| CstPisCofins | ECodigoSituacaoTributariaPisCofins | ok |
| AliquotaPis | decimal | ok |
| AliquotaCofins | decimal | ok |
| CalcularRetencao | bool | ok |
| AnexoSimplesNacional | EAnexoSimlesNacional | ok |
| Empresa | Empresa | **AUSENTE** |
| UnidadeMedida | UnidadeMedidaComercial | ok (realocado) |
| CodigoServicoSefaz | CodigoServicoSefaz | ok |

### TipoOperacaoFiscal

| Campo | Tipo | Status |
|---|---|---|
| SequenciaTenantId | long | **AUSENTE** |
| TributarioGrupoId | long | ok |
| CfopNfeId | long? | ok |
| CfopNfceId | long? | ok |
| Descricao | string | ok |
| SobescreveTributacaoNcm | bool | ok |
| Finalidade | EFinalidadeEmissao | ok |
| Atendimento | ETipoAtendimento | ok |
| TipoFrete | EModalidadeFrete | ok |
| TipoMovimento | ETipoMovimento | ok |
| TributarioGrupo | TributarioGrupo | ok (realocado) |
| CfopNfe | Cfop | ok |
| CfopNfce | Cfop | ok |

### TributarioGrupo

| Campo | Tipo | Status |
|---|---|---|
| SequenciaTenantId | long | **AUSENTE** |
| Descricao | string | ok |
| NcmTributacoes | ICollection<NcmTributacao> | **AUSENTE** |
| Empresas | ICollection<Empresa> | ok |

### UnidadeMedidaComercial

| Campo | Tipo | Status |
|---|---|---|
| SequenciaTenantId | long | **AUSENTE** |
| UnidadeMedida | string? | ok |
| Descricao | string? | ok |
| Fator | decimal | ok |
| ProdutoGrupoId | long | ok |
| Produtos | ICollection<Produto> | ok (realocado) |
| ProdutoGrupo | ProdutoGrupo | ok |

### UnidadeMedidaTributavel

| Campo | Tipo | Status |
|---|---|---|
| CodigoNcm | string | ok |
| DataInicioVigencia | DateTime | ok |
| DataFimVigencia | DateTime? | ok |
| UnidadeMedida | string | ok |
| Descricao | string | ok |

### Usuario

| Campo | Tipo | Status |
|---|---|---|
| SequenciaTenantId | long | **AUSENTE** |
| Login | string | ok |
| Senha | string | ok (realocado) |
| Email | string | ok |
| UsuarioEmpresa | ICollection<UsuarioEmpresa> | **AUSENTE** |

### UsuarioEmpresa

| Campo | Tipo | Status |
|---|---|---|
| EmpresaId | long | ok |
| UsuarioId | long | ok |
| PerfilUsuarioId | long? | **AUSENTE** |
| IsAdmin | bool | **AUSENTE** |
| PerfilUsuario | PerfilUsuario? | **AUSENTE** |
| Empresa | Empresa | **AUSENTE** |
| Usuario | Usuario | **AUSENTE** |

### Venda

| Campo | Tipo | Status |
|---|---|---|
| ModeloFiscal | EModeloDocumento | ok |
| NaturezaOperacao | string | ok |
| DataVenda | DateTime | ok |
| InformacoesComplementares | string? | ok |
| InformacoesAdicionaisFisco | string? | ok |
| Status | EVendaStatus | ok |
| ModalidadeFrete | EModalidadeFrete | ok |
| Emitente | VendaEmitente | ok |
| Destinatario | VendaDestinatario | ok |
| Transporte | VendaTransporte? | ok |
| Total | VendaTotal | ok |
| Configuracao | VendaConfiguracao | ok |
| Nfce | VendaNfce? | ok |
| Nfe | VendaNfe? | ok |
| Fatura | VendaFatura? | ok |
| Imposto | VendaImposto? | ok |
| TotalIbsCbs | VendaTotalIbsCbs? | ok |
| Pagamentos | ICollection<VendaPagamento> | ok |
| Itens | ICollection<VendaItem> | ok |
| CaminhoPdfCupomNaoFiscal | string? | ok |
| NumeroCupomNaoFiscal | long? | ok |
| AutorizacoesXml | ICollection<VendaAutorizacaoXml> | ok |
| Referenciadas | ICollection<VendaNfeReferenciada>? | ok |
| VendaOrigem | EVendaOrigem | ok |
| FatoGeradorFinanceiro | FatoGeradorFinanceiro | ok (realocado) |
| VendaEntrega | VendaEntrega? | ok |
| VendaCobrancaEndereco | VendaCobrancaEndereco? | ok |
| NfHistoricos | ICollection<VendaNfHistorico>? | ok |
| DataUltimoProcessamento | DateTime | ok |
| IncluirFreteNoTotal | bool | ok |

### VendaAutorizacaoXml

| Campo | Tipo | Status |
|---|---|---|
| VendaId | long | ok |
| Documento | Documento | ok |
| Venda | Venda | ok |

### VendaCobrancaEndereco

| Campo | Tipo | Status |
|---|---|---|
| VendaId | long | ok |
| Nome | string | ok |
| Fone | string | ok |
| Email | string | ok |
| IE | string? | ok |
| Documento | Documento | ok |
| Uf | EEstado | ok |
| Logradouro | string? | ok |
| Numero | string? | ok |
| Complemento | string? | ok |
| Bairro | string? | ok |
| MunicipioId | int | ok |
| MunicipioNome | string? | ok |
| Cep | CEP? | ok |
| PaisId | int | ok |
| PaisNome | string? | ok |
| EnderecoId | long | ok |
| Venda | Venda | ok |

### VendaConfiguracao

| Campo | Tipo | Status |
|---|---|---|
| VendaId | long | ok |
| TipoOperacao | ETipoOperacaoNfe | ok |
| TipoFormatoImpressaoDanfe | ETipoFormatoImpressaoDanfe | ok |
| TipoEmissao | ETipoEmissao | ok |
| TipoAmbiente | ETipoAmbiente | ok |
| FinalidadeEmissao | EFinalidadeEmissao | ok |
| IndicadorFinalidadeOperacao | EIndicadorFinalidadeOperacao | ok |
| TipoAtendimento | ETipoAtendimento | ok |
| IndicadorIntermediadorMarketplace | EIndicadorIntermediadorMarketplace | ok |
| Venda | Venda | ok |

### VendaDestinatario

| Campo | Tipo | Status |
|---|---|---|
| VendaId | long | ok |
| PessoaId | long? | ok |
| Cnpj | CNPJ? | ok |
| Cpf | CPF? | ok |
| RazaoSocial | string? | ok |
| Telefone | string? | ok |
| InscricaoEstadual | string? | ok |
| IdentificadorEstrangeiro | string? | ok |
| IndicadorIE | ETipoIndicadorIe | ok |
| Email | string? | ok |
| EhConsumidorFinal | bool | ok |
| EnviarDestinatatioNaNfce | bool | ok |
| DocumentoConsumidor | Documento? | ok |
| Enderecos | ICollection<VendaDestinatarioEndereco?> | ok |
| Venda | Venda | ok |

### VendaDestinatarioEndereco

| Campo | Tipo | Status |
|---|---|---|
| VendaDestinatarioId | long | ok |
| TipoEndereco | ETipoEndereco | ok |
| Uf | EEstado | ok |
| Logradouro | string? | ok |
| Numero | string? | ok |
| Complemento | string? | ok |
| Bairro | string? | ok |
| MunicipioId | int | ok |
| MunicipioNome | string? | ok |
| Cep | string? | ok |
| PaisId | int | ok |
| PaisNome | string? | ok |
| VendaDestinatario | VendaDestinatario | ok |

### VendaEmitente

| Campo | Tipo | Status |
|---|---|---|
| VendaId | long | ok |
| EmpresaId | long | ok |
| Cnpj | CNPJ? | ok |
| Cpf | CPF? | ok |
| RazaoSocial | string | ok |
| NomeFantasia | string? | ok |
| Telefone | string? | ok |
| InscricaoEstadual | string | ok |
| InscricaoEstadualST | string? | ok |
| InscricaoMunicipal | string? | ok |
| Cnae | int | ok |
| RegimeTributario | ERegimeTributario | ok |
| Endereco | VendaEmitenteEndereco | ok |
| Venda | Venda | ok |

### VendaEmitenteEndereco

| Campo | Tipo | Status |
|---|---|---|
| Uf | EEstado | ok |
| Logradouro | string? | ok |
| Numero | string? | ok |
| Complemento | string? | ok |
| Bairro | string? | ok |
| MunicipioId | int | ok |
| MunicipioNome | string? | ok |
| Cep | string? | ok |
| PaisId | int | ok |
| PaisNome | string? | ok |

### VendaEntrega

| Campo | Tipo | Status |
|---|---|---|
| VendaId | long | ok |
| Nome | string | ok |
| Fone | string | ok |
| Email | string | ok |
| IE | string? | ok |
| Documento | Documento | ok |
| Uf | EEstado | ok |
| Logradouro | string? | ok |
| Numero | string? | ok |
| Complemento | string? | ok |
| Bairro | string? | ok |
| MunicipioId | int | ok |
| MunicipioNome | string? | ok |
| Cep | CEP? | ok |
| PaisId | int | ok |
| PaisNome | string? | ok |
| EnderecoId | long | ok |
| Venda | Venda | ok |

### VendaFatura

| Campo | Tipo | Status |
|---|---|---|
| VendaId | long | ok |
| NumeroFatura | string? | ok |
| ValorOriginal | decimal | ok |
| ValorDesconto | decimal | ok |
| ValorLiquido | decimal | ok |
| Duplicatas | ICollection<VendaFaturaDuplicata> | ok |
| Venda | Venda | ok |

### VendaFaturaDuplicata

| Campo | Tipo | Status |
|---|---|---|
| VendaFaturaId | long | ok |
| NumeroDuplicata | string | ok |
| DataVencimento | DateTime | ok |
| ValorDuplicata | decimal | ok |
| VendaFatura | VendaFatura | ok |

### VendaImposto

| Campo | Tipo | Status |
|---|---|---|
| VendaId | long | ok |
| ValorAliquotaCreditoIcms | decimal | ok |

### VendaItem

| Campo | Tipo | Status |
|---|---|---|
| VendaId | long | ok |
| ProdutoId | long | ok |
| CodigoProduto | string | ok |
| CodigoEan | string? | ok |
| DescricaoProduto | string | ok |
| Ncm | string | ok |
| ExcecaoNcmTipi | string? | ok |
| CestId | long? | ok |
| Cest | string? | ok |
| CodigoAnpId | long? | ok |
| CodigoAnp | string? | ok |
| Cfop | int | ok |
| UnidadeComercial | string | ok |
| QuantidadeComercial | decimal | ok |
| ValorUnitarioComercial | decimal | ok |
| ValorTotalBrutoProdutos | decimal | ok |
| CodigoEanTributavel | string? | ok |
| UnidadeTributavel | string | ok |
| QuantidadeTributavel | decimal | ok |
| ValorUnitarioTributavel | decimal | ok |
| ValorDesconto | decimal | ok |
| ValorDescontoRateado | decimal | ok |
| ValorFreteRateado | decimal | ok |
| ValorSeguroRateado | decimal | ok |
| ValorOutrasDepesasAcessoriasRateado | decimal | ok |
| CompoeValorTotal | EIndicadorTotalizador | ok |
| InformacoesAdicionaisDoProduto | string? | ok |
| CfopCorrelacao | int | ok |
| IntegraFaturamento | bool | ok |
| NumeroItemPedidoCompra | int | ok |
| NumeroPedidoCompra | string? | ok |
| FichaConteudoImportacao | string? | ok |
| CodigoBeneficioFiscal | string? | ok |
| ValorCusto | decimal | ok |
| Imposto | VendaItemImposto | ok |
| ImpostoValorAproximado | VendaItemImpostoValorAproximado | ok |
| Combustivel | VendaItemCombustivel | ok |
| ImpostoIbsCbs | VendaItemImpostoIbsCbs? | ok |
| Venda | Venda | ok |

### VendaItemCombustivel

| Campo | Tipo | Status |
|---|---|---|
| VendaItemId | long | ok |
| CodigoAnp | string? | ok |
| DescricaoAnp | string? | ok |
| QuantidadeCombustivelFaturada | decimal | ok |
| UfConsumo | EEstado | ok |
| PercentualGlpDerivadoPetroleo | decimal | ok |
| PercentualGasNaturalNacional | decimal | ok |
| PercentualGasNaturalImportado | decimal | ok |
| ValorPartida | decimal | ok |
| Origens | ICollection<VendaItemCombustivelOrigem>? | ok |
| VendaItem | VendaItem | ok |

### VendaItemCombustivelOrigem

| Campo | Tipo | Status |
|---|---|---|
| VendaItemCombustivelId | long | ok |
| IndicadorImportacao | int | ok |
| UfOrigem | EEstado | ok |
| PercentualOrigem | decimal | ok |
| VendaItemCombustivel | VendaItemCombustivel | ok |

### VendaItemImposto

| Campo | Tipo | Status |
|---|---|---|
| VendaItemId | long | ok |
| Origem | EOrigemMercadoria | ok |
| CstIcms | ECodigoSituacaoTributariaIcms | ok |
| Csosn | ECodigoSituacaoOperacaoSimplesNacional | ok |
| ModalidadeDeterminacaoBaseCalculoIcms | EModalidadeBaseDeCalculosIcms | ok |
| ValorBaseDeCalculoIcms | decimal | ok |
| PercentualReducaoBaseDeCalculoIcms | decimal | ok |
| AliquotaIcms | decimal | ok |
| ValorImpostoIcms | decimal | ok |
| ModalidadeBaseDeCalculosST | EModalidadeBaseDeCalculosST | ok |
| PercentualMvaBaseDeCalculoST | decimal | ok |
| PercentualReducaoBaseDeCalculoST | decimal | ok |
| ValorBaseDeCalculoSt | decimal | ok |
| AliquotaSt | decimal | ok |
| ValorImpostoSt | decimal | ok |
| MotivoDesoneracaoIcms | EMotivoDesoneracaoIcms | ok |
| ValorBaseDeCalculoStRetido | decimal | ok |
| ValorImpostoStRetido | decimal | ok |
| PercentualCreditoSimplesNacionalIcms | decimal | ok |
| ValorImpostoCreditoSimplesNacionalIcms | decimal | ok |
| ValorBaseDeCalculoFcp | decimal | ok |
| PercentualFcp | decimal | ok |
| ValorImpostoFcp | decimal | ok |
| ValorOperacaoDiferimentoIcms | decimal | ok |
| PercentualDiferimentoIcms | decimal | ok |
| ValorImpostoDiferimentoIcms | decimal | ok |
| CstIpiSaida | ECodigoSituacaoTributariaIpi | ok |
| ValorBaseDeCalculoIpi | decimal | ok |
| AliquotaIpi | decimal | ok |
| ValorImpostoDiferimentoIpi | decimal | ok |
| ValorQuantidadeTotalParaTributacaoIpi | decimal | ok |
| ValorPorUnidadeTributavelIpi | decimal | ok |
| CstPis | ECodigoSituacaoTributariaPisCofins | ok |
| ValorBaseDeCalculoPis | decimal | ok |
| AliquotaPis | decimal | ok |
| ValorQuantidadeVendidaProdutoPis | decimal | ok |
| AliquotaPorUnidadeVendidaPis | decimal | ok |
| ValorImpostoDiferimentoPis | decimal | ok |
| CstCofins | ECodigoSituacaoTributariaPisCofins | ok |
| ValorBaseDeCalculoCofins | decimal | ok |
| AliquotaCofins | decimal | ok |
| ValorQuantidadeVendidaProdutoCofins | decimal | ok |
| AliquotaPorUnidadeVendidaCofins | decimal | ok |
| ValorImpostoDiferimentoCofins | decimal | ok |
| TipoReducaoIcms | ETipoReducaoBaseDeCalculo | ok |
| TipoReducaoIcmsSt | ETipoReducaoBaseDeCalculo | ok |
| ValorBaseDeCalculoFcpSt | decimal | ok |
| PercentualFcpSt | decimal | ok |
| ValorImpostoFcpSt | decimal | ok |
| ValorIcmsProprioSubistituto | decimal | ok |
| ValorAliquotaIcmsInterna | decimal | ok |
| ValorAliquotaIcmsInternaEstadual | decimal | ok |
| EnquadramentoIpi | int | ok |
| ValorReducaoIpiPercentual | decimal | ok |
| IpiEmbutido | bool | ok |
| DifalTipoCalculoPorDentro | bool | ok |
| TipoReducaoIpi | ETipoReducaoBaseDeCalculo | ok |
| TipoCalculoBaseIcmsSt | EDeterminacaoBaseIcmsSt | ok |
| ValorUnitFixadoIcmsSt | decimal | ok |
| ValorBaseDeCalculoDifal | decimal | ok |
| ValorImpostoDevidoDifal | decimal | ok |
| ValorImpostoDevidoRecolherSt | decimal | ok |
| ValorImpostoDevidoFcp | decimal | ok |
| ValorIcmsIsento | decimal | ok |
| ValorIcmsOutros | decimal | ok |
| IcmsObservacao | string? | ok |
| ValorIpiIsento | decimal | ok |
| ValorIpiOutros | decimal | ok |
| IpiObservacao | string? | ok |
| VendaItem | VendaItem | ok |

### VendaItemImpostoIbsCbs

| Campo | Tipo | Status |
|---|---|---|
| VendaItemId | long | ok |
| Cst | string | ok |
| CClassTrib | string | ok |
| AliquotaEstadual | decimal | ok |
| AliquotaMunicipal | decimal | ok |
| AliquotaCbs | decimal | ok |
| AliquotaEstadualReducao | decimal | ok |
| AliquotaMunicipalReducao | decimal | ok |
| AliquotaCbsReducao | decimal | ok |
| AliquotaEstadualDiferimento | decimal | ok |
| AliquotaMunicipalDiferimento | decimal | ok |
| AliquotaCbsDiferimento | decimal | ok |
| AliquotaEfetivaEstadual | decimal | ok |
| AliquotaEfetivaMunicipal | decimal | ok |
| AliquotaEfetivaCbs | decimal | ok |
| ValorBaseDeCalculo | decimal | ok |
| ValorImpostoDevidoEstadual | decimal | ok |
| ValorImpostoDevidoMunicipal | decimal | ok |
| ValorImpostoDevidoCbs | decimal | ok |
| VendaItemImpostoIbsCbsTributacaoRegular | VendaItemImpostoIbsCbsTributacaoRegular? | ok |
| VendaItem | VendaItem | ok |

### VendaItemImpostoIbsCbsTributacaoRegular

| Campo | Tipo | Status |
|---|---|---|
| VendaItemImpostoIbsCbsId | long | ok |
| Cst | string | ok |
| CClassTrib | string | ok |
| AliquotaEfetivaIbsEstadual | decimal | ok |
| ValorIbsEstadual | decimal | ok |
| AliquotaEfetivaIbsMunicipal | decimal | ok |
| ValorIbsMunicipal | decimal | ok |
| AliquotaEfetivaCbs | decimal | ok |
| ValorCbs | decimal | ok |
| VendaItemImpostoIbsCbs | VendaItemImpostoIbsCbs | ok |

### VendaItemImpostoValorAproximado

| Campo | Tipo | Status |
|---|---|---|
| VendaItemId | long | ok |
| AliquotaNacionalFederal | decimal | ok |
| AliquotaImportadoFederal | decimal | ok |
| AliquotaEstadual | decimal | ok |
| AliquotaMunicipal | decimal | ok |
| Versao | string? | ok |
| Fonte | string? | ok |
| VendaItem | VendaItem | ok |

### VendaNfHistorico

| Campo | Tipo | Status |
|---|---|---|
| VendaId | long | ok |
| Descricao | string | ok |

### VendaNfce

| Campo | Tipo | Status |
|---|---|---|
| VendaId | long | ok |
| Numero | long | ok |
| Serie | int | ok |
| IdCsc | string? | ok |
| Csc | string? | ok |
| StatusInterno | EDocumentoFiscalStatus | ok |
| Chave | string? | ok |
| DataHoraEmissao | DateTime | ok |
| StatusSefaz | int | ok |
| Protocolo | string? | ok |
| Xml | string? | ok |
| UltimoRetornoMensagemSefaz | string? | ok |
| DataHoraCancelamento | DateTime? | ok |
| ProtocoloCancelamento | string? | ok |
| StatusSefazCancelamento | int | ok |
| MotivoCancelamento | string? | ok |
| XmlCancelamento | string? | ok |

### VendaNfe

| Campo | Tipo | Status |
|---|---|---|
| VendaId | long | ok |
| Numero | long | ok |
| Serie | int | ok |
| DataHoraEmissao | DateTime | ok |
| DataHoraSaida | DateTime? | ok |
| StatusInterno | EDocumentoFiscalStatus | ok |
| StatusSefaz | int | ok |
| Chave | string? | ok |
| Protocolo | string? | ok |
| Xml | string? | ok |
| UltimoRetornoMensagemSefaz | string? | ok |
| DataHoraCancelamento | DateTime | ok |
| ProtocoloCancelamento | string? | ok |
| StatusSefazCancelamento | int | ok |
| MotivoCancelamento | string? | ok |
| XmlCancelamento | string? | ok |
| EmbuteFrete | bool | ok |
| EmbuteSeguro | bool | ok |
| EmbuteAcrescimo | bool | ok |
| EmbuteOutro | bool | ok |
| Intermediador | VendaNfeIntermediador? | ok |
| CartasCorrecoes | ICollection<VendaNfeCartaCorrecao> | ok |
| VendaNfeExportacao | VendaNfeExportacao | ok |

### VendaNfeCartaCorrecao

| Campo | Tipo | Status |
|---|---|---|
| VendaNfeId | long | ok |
| TextoCorrecao | string | ok |
| SequenciaEvento | int | ok |
| StatusSefaz | int | ok |
| MotivoRejeicaoSefaz | string? | ok |
| VendaNfe | VendaNfe | ok |

### VendaNfeExportacao

| Campo | Tipo | Status |
|---|---|---|
| VendaNfeId | long | ok |
| UfSaidaPais | EEstado | ok |
| LocalExportacao | string | ok |
| LocalDespacho | string? | ok |
| VendaNfe | VendaNfe | ok |

### VendaNfeHistorico

| Campo | Tipo | Status |
|---|---|---|
| VendaId | long | ok |

### VendaNfeIntermediador

| Campo | Tipo | Status |
|---|---|---|
| VendaNfeId | long | ok |
| Documento | Documento | ok |
| IdentificadorIntermediador | string? | ok |
| VendaNfe | VendaNfe | ok |

### VendaNfeReferenciada

| Campo | Tipo | Status |
|---|---|---|
| VendaId | long | ok |
| Chave | string | ok |
| Venda | Venda | ok |

### VendaPagamento

| Campo | Tipo | Status |
|---|---|---|
| VendaId | long | ok |
| ValorTroco | decimal | ok |
| IndicadorPagamento | EIndicadorPagamento | ok |
| TipoPagamento | ETipoPagamento | ok |
| ValorPagamento | decimal | ok |
| CartaoTipoIntegracao | ETipoIntegracaoPagamentoCArtao | ok |
| CartaoCnpjIntermediadorFinanceira | CNPJ? | ok |
| CartaoBandeira | EBandeiraCartao | ok |
| CartaoCodigoAutorizacaoOperacao | string? | ok |

### VendaTotal

| Campo | Tipo | Status |
|---|---|---|
| VendaId | long | ok |
| ValorBaseDeCalculoIcms | decimal | ok |
| ValorIcms | decimal | ok |
| ValorIcmsDesonerado | decimal | ok |
| ValorFcp | decimal | ok |
| ValorBaseDeCalculoSt | decimal | ok |
| ValorSt | decimal | ok |
| ValorFcpSt | decimal | ok |
| ValorFcpRetido | decimal | ok |
| ValorProduto | decimal | ok |
| ValorFrete | decimal | ok |
| ValorSeguro | decimal | ok |
| ValorDesconto | decimal | ok |
| ValorImpostoImportacao | decimal | ok |
| ValorIpi | decimal | ok |
| ValorIpiDevolucao | decimal | ok |
| ValorPis | decimal | ok |
| ValorCofins | decimal | ok |
| ValorOutro | decimal | ok |
| ValorNotaFiscal | decimal | ok |
| Venda | Venda | ok |

### VendaTotalIbsCbs

| Campo | Tipo | Status |
|---|---|---|
| VendaId | long | ok |
| ValorBaseDeCalculo | decimal | ok |
| ValorImpostoDevidoEstadual | decimal | ok |
| ValorImpostoDevidoMunicipal | decimal | ok |
| ValorImpostoDevidoCbs | decimal | ok |
| Venda | Venda | ok |

### VendaTransporte

| Campo | Tipo | Status |
|---|---|---|
| VendaId | long | ok |
| Transportadora | VendaTransporteTransportadora? | ok |
| Veiculo | VendaTransporteVeiculo? | ok |
| Volumes | ICollection<VendaTransporteVolume>? | ok |
| Reboques | ICollection<VendaTransporteReboque>? | ok |

### VendaTransporteReboque

| Campo | Tipo | Status |
|---|---|---|
| VendaTransporteId | long | ok |
| VeiculoId | long? | ok |
| Placa | string | ok |
| Uf | EEstado | ok |
| Rntrc | string? | ok |
| Transporte | VendaTransporte | ok |
| Veiculo | PessoaVeiculo | ok (realocado) |

### VendaTransporteTransportadora

| Campo | Tipo | Status |
|---|---|---|
| VendaTransporteId | long | ok |
| PessoaId | long? | ok |
| Cnpj | CNPJ? | ok |
| Cpf | CPF? | ok |
| RazaoSocial | string? | ok |
| InscricaoEstadual | string? | ok |
| Logradouro | string? | ok |
| Numero | string? | ok |
| Complemento | string? | ok |
| Bairro | string? | ok |
| Municipio | string? | ok |
| Uf | EEstado | ok |
| Transportadora | PessoaTransportadora? | ok (realocado) |

### VendaTransporteVeiculo

| Campo | Tipo | Status |
|---|---|---|
| VendaTransporteId | long | ok |
| VeiculoId | long? | ok |
| Placa | string | ok |
| Uf | EEstado | ok |
| Rntrc | string? | ok |
| Transporte | VendaTransporte | ok |
| Veiculo | PessoaVeiculo | ok (realocado) |

### VendaTransporteVolume

| Campo | Tipo | Status |
|---|---|---|
| VendaTransporteId | long | ok |
| QuantidadeVolumes | int | ok |
| Especie | string? | ok |
| NumeroVolumes | string? | ok |
| PesoLiquido | decimal | ok |
| PesoBruto | decimal | ok |
| Marca | string? | ok |
| VendaTransporte | VendaTransporte | ok |
