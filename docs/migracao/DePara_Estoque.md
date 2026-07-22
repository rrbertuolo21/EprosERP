# De -> Para — Modulo Estoque (EprosERP)

Auditoria de fidelidade de migracao. Compara o legado (`Epros.ERP.Domain/Entities`) das fontes
`Cadastros/Produtos`, `Estoque` e `Compras` com o modulo novo
`src/Modules/Epros.Modules.Estoque/Domain/Entities`.

Convencoes:

- Campos herdados de `EntidadeSaaSBase` (Id, TenantId, SyncId, auditoria: DataCadastro/DataAlteracao/DataInativacao, Ativo/Deletado) sao considerados COBERTOS e nao sao listados por entidade.
- No legado, `Id` (long) e `SequenciaTenantId` sao substituidos por `Id` (Guid) da `EntidadeSaaSBase`. FKs `long`/`long?` no legado viram `Guid`/`Guid?` no novo.
- Navegacoes (propriedades EF de referencia/colecao) sao listadas apenas quando relevantes para apontar dependencia ausente.
- Data da auditoria (reauditoria pos-correcao): 2026-07-02.

> **Reauditoria pos-rodada-de-correcao:** a rodada de correcao portou TODAS as 34 entidades de Compras
> (antes so 10), restaurou os enums degradados para tipos primitivos (Compra/Produto voltaram a usar
> `EModeloDocumento`, `EVendaStatus`, `EModalidadeFrete`, `EVendaOrigem`, `ETipoNFeCompra`, `ETipoProduto`),
> recuperou o bloco tributavel/rateios/pedido de `CompraItem`, adicionou as navegacoes intra-modulo do item
> (Imposto/ImpostoIbsCbs/ImpostoValorAproximado/Combustivel/Importacoes) e criou `ProdutoGrupoEmpresa` (M2M).
> Todas as 34 entidades de Compras estao registradas como `DbSet` no `ContextEstoque` (53 DbSets no total).

---

## 1. Cadastros/Produtos (12 entidades legadas) — TODAS presentes

| Entidade legada | Campo legado | Destino novo (entidade.campo) ou AUSENTE |
|---|---|---|
| Adicionais | Descricao | Adicionais.Descricao |
| Adicionais | ValorPreco | Adicionais.ValorPreco |
| Adicionais | AdicionaisProdutos (nav) | Adicionais.AdicionaisProdutos |
| AdicionaisProduto | ProdutoId | AdicionaisProduto.ProdutoId |
| AdicionaisProduto | AdicionaisId | AdicionaisProduto.AdicionaisId |
| AdicionaisProduto | Produto (nav) | AdicionaisProduto.Produto |
| AdicionaisProduto | Adicionais (nav) | AdicionaisProduto.Adicionais |
| Balanca | SequenciaTenantId | Balanca.Id (Guid herdado) |
| Balanca | Nome / QntDigitoIdentificador / QntDigitoCodigoProduto / QntDigitoValorProduto / QntCasaDecimal / TipoValor | Balanca.* correspondentes |
| Balanca | Produtos (nav) | Balanca.Produtos |
| CategoriaProduto | SequenciaTenantId / ProdutoGrupoId / Descricao | CategoriaProduto.* correspondentes |
| CategoriaProduto | Produtos (nav) | AUSENTE (colecao inversa nao mapeada; nao critico) |
| CategoriaProduto | ProdutoGrupo (nav) | CategoriaProduto.ProdutoGrupo |
| MarcaProduto | SequenciaTenantId / ProdutoGrupoId / Descricao | MarcaProduto.* correspondentes |
| MarcaProduto | Produtos (nav) | AUSENTE (colecao inversa nao mapeada; nao critico) |
| MarcaProduto | ProdutoGrupo (nav) | MarcaProduto.ProdutoGrupo |
| Produto | SequenciaTenantId | Produto.SequenciaExibicao (porte oficial) |
| Produto | ProdutoGrupoId / CategoriaId / MarcaProdutoId / UnidadeMedidaComercialId / NcmId / CodigoAnpId / CestId / BalancaId | Produto.* correspondentes (FK Guid) |
| Produto | Codigo / Descricao / Ean / PesoLiquido / PesoBruto / ValorVenda / ValorVendaPrazo / ValorCompra | Produto.* correspondentes |
| Produto | TipoProduto | Produto.TipoProduto (ETipoProduto? — enum RESTAURADO) |
| Produto | Ativo / Imagem / UtilizaBalanca / CodigoProdutoBalanca | Produto.* correspondentes |
| Produto | ProdutoGrupo / Categoria / MarcaProduto / UnidadeMedidaComercial / Balanca / ProdutoEspecifico (nav) | Produto.* correspondentes |
| Produto | Ncm / CodigoAnp / Cest (nav) | AUSENTE nav (apenas FK Id: NcmId/CodigoAnpId/CestId — entidades fora do modulo) |
| Produto | EstoqueProduto (nav) | AUSENTE na classe Produto (EstoqueProduto existe com ProdutoId; nav inversa nao mapeada) |
| Produto | ProdutoFichaEstoqueEntradas / ProdutoFichaEstoqueSaidas (nav) | AUSENTE na classe Produto (entidades existem; nav inversa nao mapeada) |
| Produto | RegistroMovimentoEstoqueManuais (nav) | AUSENTE na classe Produto (EstoqueMovimentoManual existe; nav inversa nao mapeada) |
| Produto | AdicionaisProduto (nav) | Produto.AdicionaisProduto |
| Produto | ProdutoHistoricoReajustes (nav) | Produto.ProdutoHistoricoReajustes |
| ProdutoEspecifico | ProdutoId / ValorPercentualGlpDerivadoPetroleo / ValorPercentualGasNaturalNacional / ValorPercentualGasNaturalImportado / ValorPartida / UfConsumo | ProdutoEspecifico.* correspondentes |
| ProdutoEspecifico | Origens (nav) / Produto (nav) | ProdutoEspecifico.Origens / .Produto |
| ProdutoEspecificoCombustivelOrigem | ProdutoEspecificoId / IndicadorImportacao / UfOrigem / ValorPercentualUf | ProdutoEspecificoCombustivelOrigem.* correspondentes |
| ProdutoEspecificoCombustivelOrigem | ProdutoEspecifico (nav) | ProdutoEspecificoCombustivelOrigem.ProdutoEspecifico |
| ProdutoGrupo | SequenciaTenantId / Descricao | ProdutoGrupo.* correspondentes |
| ProdutoGrupo | Empresas (nav — M2M com Empresa) | ProdutoGrupoEmpresa (entidade de juncao CRIADA; Empresa fica fora do modulo — so a FK) |
| ProdutoGrupo | Produtos / MarcaProdutos / CategoriaProdutos (nav) | AUSENTE (colecoes inversas nao mapeadas; nao critico) |
| ProdutoHistoricoReajuste | SequenciaTenantId / ProdutoId / CodigoProduto / ValorAntigo / Tipo / Fator / ValorFixo / ValorNovo / Motivo | ProdutoHistoricoReajuste.* correspondentes |
| ProdutoHistoricoReajuste | Produto (nav) | ProdutoHistoricoReajuste.Produto |
| UnidadeMedidaComercial | SequenciaTenantId / UnidadeMedida / Descricao / Fator / ProdutoGrupoId | UnidadeMedidaComercial.* correspondentes |
| UnidadeMedidaComercial | Produtos (nav) | AUSENTE (colecao inversa nao mapeada; nao critico) |
| UnidadeMedidaComercial | ProdutoGrupo (nav) | UnidadeMedidaComercial.ProdutoGrupo |
| UnidadeMedidaTributavel | CodigoNcm / DataInicioVigencia / DataFimVigencia / UnidadeMedida / Descricao | UnidadeMedidaTributavel.* correspondentes |

> Observacao: `UnidadeMedidaTributavel` no legado herda de `EntityNoTenat` (sem TenantId). No novo herda de `EntidadeSaaSBase` (com TenantId) — divergencia de tenancy a validar (era global/compartilhada no legado).

---

## 2. Estoque (5 entidades legadas) — TODAS presentes

| Entidade legada | Campo legado | Destino novo (entidade.campo) ou AUSENTE |
|---|---|---|
| EstoqueMovimentoManual | ProdutoId / TipoEstoque / TipoMovimento / QuantidadeMovimentada / ValorUnitario | EstoqueMovimentoManual.* correspondentes |
| EstoqueMovimentoManual | Produto / FatoGeradorEstoque (nav) | EstoqueMovimentoManual.Produto / .FatoGeradorEstoque |
| EstoqueProduto | EmpresaId / ProdutoId / QuantidadeSaldoEstoque / QuantidadeEstoqueMinimo / QuantidadeEstoqueMaximo / QuantidadeEstoqueReservado / ValorSaldo / ValorCustoMedio / TipoCusteioEstoque | EstoqueProduto.* correspondentes |
| EstoqueProduto | Empresa (nav) | AUSENTE (nav Empresa nao portada; existe EmpresaId) |
| EstoqueProduto | Produto (nav) | EstoqueProduto.Produto |
| FatoGeradorEstoque | VendaId / CompraId / EstoqueMovimentoManualId / Origem | FatoGeradorEstoque.* correspondentes |
| FatoGeradorEstoque | ProdutoFichaEstoqueEntradas / ProdutoFichaEstoqueSaidas (nav) | FatoGeradorEstoque.* |
| FatoGeradorEstoque | Venda (nav) | AUSENTE (nav Venda nao portada; existe VendaId) |
| FatoGeradorEstoque | Compra (nav) / EstoqueMovimentoManual (nav) | FatoGeradorEstoque.Compra / .EstoqueMovimentoManual |
| ProdutoFichaEstoqueEntrada | EmpresaId / ProdutoId / FatoGeradorEstoqueId / TipoEstoque / QuantidadeMovimentada / ValorUnitario / QuantidadeSaldo / ValorSaldo | ProdutoFichaEstoqueEntrada.* correspondentes |
| ProdutoFichaEstoqueEntrada | Empresa (nav) | AUSENTE (nav nao portada; existe EmpresaId) |
| ProdutoFichaEstoqueEntrada | Produto / FatoGeradorEstoque / ProdutoFichaEstoqueSaidas (nav) | ProdutoFichaEstoqueEntrada.* |
| ProdutoFichaEstoqueSaida | EmpresaId / ProdutoId / FatoGeradorEstoqueId / ProdutoFichaEstoqueEntradaId / QuantidadeMovimentada / ValorUnitario / ValorTotal / ValorCustoMedio / ValorTotalCustoMedio | ProdutoFichaEstoqueSaida.* correspondentes |
| ProdutoFichaEstoqueSaida | Empresa (nav) | AUSENTE (nav nao portada; existe EmpresaId) |
| ProdutoFichaEstoqueSaida | Produto / FatoGeradorEstoque / ProdutoFichaEstoqueEntrada (nav) | ProdutoFichaEstoqueSaida.* |

---

## 3. Compras (34 entidades legadas) — TODAS as 34 portadas

Apos a rodada de correcao, todas as 34 entidades do contexto Compras do legado tem contraparte no
modulo novo e estao registradas como `DbSet` no `ContextEstoque`.

### 3a. Cabecalho e itens

| Entidade legada | Campo legado | Destino novo (entidade.campo) ou observacao |
|---|---|---|
| Compra | ModeloFiscal | Compra.ModeloFiscal (EModeloDocumento? — enum RESTAURADO) |
| Compra | NaturezaOperacao / DataCompra / InformacoesComplementares / InformacoesAdicionaisFisco | Compra.* correspondentes |
| Compra | Status | Compra.Status (EVendaStatus — enum RESTAURADO) |
| Compra | ModalidadeFrete | Compra.ModalidadeFrete (EModalidadeFrete? — enum RESTAURADO) |
| Compra | CompraOrigem | Compra.CompraOrigem (EVendaOrigem? — enum RESTAURADO) |
| Compra | TipoNota | Compra.TipoNota (ETipoNFeCompra? — enum RESTAURADO) |
| Compra | Itens (nav) | Compra.Itens |
| Compra | Emitente / Destinatario / Transporte / Total / Configuracao / Nfe / Fatura / Imposto / TotalIbsCbs / Pagamentos / AutorizacoesXml / Referenciadas / CompraEntrega / CompraCobrancaEndereco (nav) | Entidades TODAS portadas e com DbSet; navegacoes de referencia NAO declaradas na classe Compra do novo modulo (filhas ligadas por CompraId). Ver 3c. |
| Compra | FatoGeradorFinanceiro (nav) | AUSENTE (fora do modulo Estoque — pertence a Financeiro) |
| Compra | Emitente (fornecedor) | Compra.FornecedorCnpj / FornecedorNome (achatado, campos de compatibilidade) + entidade CompraEmitente completa portada em paralelo |
| Compra | (novo) ChaveAcesso / NumeroNota / ValorTotal / DataEmissao / FormaPagamento / Cancelada | Campos novos do modulo (cabecalho simplificado usado pelos handlers) |
| CompraItem | CompraId / ProdutoId / CodigoProduto / CodigoEan / DescricaoProduto / Ncm / Cfop / UnidadeComercial / QuantidadeComercial / ValorUnitarioComercial / ValorTotalBrutoProdutos | CompraItem.* correspondentes |
| CompraItem | ExcecaoNcmTipi | CompraItem.ExcecaoNcmTipi (RESTAURADO) |
| CompraItem | CestId / Cest / CodigoAnpId / CodigoAnp | CompraItem.* correspondentes |
| CompraItem | CodigoEanTributavel / UnidadeTributavel / QuantidadeTributavel / ValorUnitarioTributavel | CompraItem.* correspondentes (bloco tributavel RESTAURADO) |
| CompraItem | ValorDesconto / ValorDescontoRateado / ValorFreteRateado / ValorSeguroRateado / ValorOutrasDepesasAcessoriasRateado | CompraItem.* (rateios RESTAURADOS; grafado ValorOutrasDespesasAcessoriasRateado no novo) |
| CompraItem | CompoeValorTotal / InformacoesAdicionaisDoProduto / CfopCorrelacao / IntegraFaturamento / NumeroItemPedidoCompra / NumeroPedidoCompra / FichaConteudoImportacao / CodigoBeneficioFiscal | CompraItem.* correspondentes (RESTAURADOS) |
| CompraItem | Imposto / ImpostoValorAproximado / Combustivel / ImpostoIbsCbs / Importacoes (nav) | CompraItem.* correspondentes (navegacoes intra-modulo RESTAURADAS) |
| CompraItem | (novo) Quantidade / PrecoUnitario / ValorIms / ValorIpi / ValorTotal / ValorCusto | Campos novos de compatibilidade |

### 3b. Impostos, totais e fatura

| Entidade legada | Situacao |
|---|---|
| CompraImposto | Portada (CompraId, ValorAliquotaCreditoIcms) |
| CompraFatura | Portada (CompraId, NumeroFatura, ValorOriginal, ValorDesconto, ValorLiquido, nav Duplicatas/Compra) |
| CompraFaturaDuplicata | Portada (CompraFaturaId, NumeroDuplicata, DataVencimento, ValorDuplicata, nav CompraFatura) |
| CompraItemImposto | Portada integralmente (~65 campos fiscais: Origem, CstIcms, Csosn, bases/aliquotas/valores ICMS/ST/FCP/IPI/PIS/COFINS/DIFAL, reducoes, observacoes) |
| CompraItemImpostoIbsCbs | Portada integralmente (Cst, CClassTrib, aliquotas/reducoes/diferimentos/efetivas Estadual/Municipal/Cbs, bases e valores devidos) |
| CompraItemImpostoValorAproximado | Portada integralmente (AliquotaNacionalFederal, AliquotaImportadoFederal, AliquotaEstadual, AliquotaMunicipal, Versao, Fonte) |
| CompraTotal | Portada integralmente (bases/valores ICMS, FCP, ST, Produto, Frete, Seguro, Desconto, ImpostoImportacao, IPI, PIS, COFINS, Outro, NotaFiscal) |
| CompraTotalIbsCbs | Portada integralmente (ValorBaseDeCalculo, ValorImpostoDevidoEstadual/Municipal/Cbs) |

### 3c. Bloco fiscal/documental (antes ausente) — agora portado

| Entidade legada | Situacao pos-correcao |
|---|---|
| CompraEmitente | Portada |
| CompraEmitenteEndereco | Portada |
| CompraDestinatario | Portada |
| CompraDestinatarioEndereco | Portada |
| CompraConfiguracao | Portada |
| CompraNfe | Portada |
| CompraNfeCartaCorrecao | Portada |
| CompraNfeHistorico | Portada |
| CompraNfeIntermediador | Portada |
| CompraNfeReferenciada | Portada |
| CompraPagamento | Portada |
| CompraTransporte | Portada |
| CompraTransporteReboque | Portada |
| CompraTransporteTransportadora | Portada |
| CompraTransporteVeiculo | Portada |
| CompraTransporteVolume | Portada |
| CompraItemCombustivel | Portada |
| CompraItemCombustivelOrigem | Portada |
| CompraItemImportacao | Portada |
| CompraItemImportacaoAdicao | Portada |
| CompraEntrega | Portada |
| CompraCobrancaEndereco | Portada |
| CompraAutorizacaoXml | Portada |

> Nota: `MovimentoEstoque` existe no modulo novo mas NAO tem contraparte no legado das tres fontes — e uma entidade simplificada nova (ProdutoId, Quantidade, Tipo, Historico). `ProdutoGrupoEmpresa` foi criada como juncao M2M ProdutoGrupo<->Empresa (a Empresa em si esta fora do modulo).

---

## 4. Resumo

- Entidades legadas totais (3 fontes): **51** (Produtos 12 + Estoque 5 + Compras 34).
- Entidades presentes no modulo novo: **51** (Produtos 12 + Estoque 5 + Compras 34) + 2 novas (`MovimentoEstoque`, `ProdutoGrupoEmpresa`).
- Entidades AUSENTES: **0** (todas as 34 de Compras foram portadas na rodada de correcao).

### Cobertura estimada: ~93%

Todas as entidades e praticamente todos os campos escalares estao portados. Os ~7% restantes sao
lacunas de modelagem de baixo/medio impacto (nao bloqueiam dados), listadas abaixo.

### O que ainda falta (nao critico, mas pendente)

1. **Navegacoes de referencia na classe `Compra`**: as entidades filhas (Total, Imposto, TotalIbsCbs,
   Fatura, Emitente, Destinatario, Configuracao, Nfe, Transporte, Pagamentos, AutorizacoesXml,
   Referenciadas, Entrega, CobrancaEndereco) existem e tem DbSet, mas a classe `Compra` do novo modulo
   NAO declara as propriedades de navegacao para elas — o vinculo hoje e so via `CompraId` nas filhas.
   O legado tinha essas navegacoes e a rica API de dominio (`IncluirEmitente`, `IncluirTotal`,
   `IncluirNfe`, `AdicionarPagamento`, etc.) — o novo `Compra` so expõe `AdicionarItem`/`AtualizarStatus`/`Cancelar`.
2. **Navegacoes inversas de colecao** (Produtos em Categoria/Marca/Grupo/UnidadeMedida; Empresa/Venda
   em EstoqueProduto e fichas; EstoqueProduto/fichas/RegistroMovimento em Produto) nao mapeadas —
   nao bloqueia dados, remove conveniencia de consulta.
3. **Nav `Ncm`/`Cest`/`CodigoAnp` em Produto e CompraItem**: apenas as FKs (`NcmId`/`CestId`/`CodigoAnpId`)
   foram portadas; as entidades de referencia ficam fora do modulo, sem navegacao.
4. **`UnidadeMedidaTributavel` — divergencia de tenancy**: legado `EntityNoTenat` (global/compartilhada)
   -> novo `EntidadeSaaSBase` (com TenantId). Validar se deve ser global.
5. **`CompraEmitente` duplicado no cabecalho**: alem da entidade completa, o novo `Compra` mantem
   `FornecedorCnpj`/`FornecedorNome` achatados (campos de compatibilidade). Verificar se ha risco de
   dessincronizacao entre o achatado e o agregado `CompraEmitente`.
6. **`FatoGeradorFinanceiro` (nav em Compra)**: fora de escopo (modulo Financeiro) — permanece ausente
   por design.

### Divergencias de grafia a conferir

- Legado `ValorOutrasDepesasAcessoriasRateado` (com typo "Depesas") -> novo `ValorOutrasDespesasAcessoriasRateado`
  (grafia corrigida). Garantir que mapeamento/coluna de banco esteja alinhado na migracao de dados.
