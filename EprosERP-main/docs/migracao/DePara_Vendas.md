# De -> Para: Migracao Modulo Vendas (Legado -> Epros.Modules.Vendas)

Gerado em 2026-07-02. Auditoria de fidelidade campo a campo do agregado Venda.

Regra: campos herdados de EntidadeSaaSBase (Id / TenantId / SyncId / auditoria) sao considerados cobertos. Propriedades de navegacao mapeiam para a entidade filha/pai correspondente quando presente.

| Entidade legada | Campo legado | Destino novo (entidade.campo) ou AUSENTE |
|---|---|---|
| Venda | ModeloFiscal | Venda.ModeloFiscal |
| Venda | NaturezaOperacao | Venda.NaturezaOperacao |
| Venda | DataVenda | Venda.DataVenda |
| Venda | InformacoesComplementares | Venda.InformacoesComplementares |
| Venda | InformacoesAdicionaisFisco | Venda.InformacoesAdicionaisFisco |
| Venda | Status | Venda.Status |
| Venda | ModalidadeFrete | Venda.ModalidadeFrete |
| Venda | Emitente | Venda.Emitente |
| Venda | Destinatario | Venda.Destinatario |
| Venda | Transporte | Venda.Transporte |
| Venda | Total | Venda.Total |
| Venda | Configuracao | Venda.Configuracao |
| Venda | Nfce | Venda.Nfce |
| Venda | Nfe | Venda.Nfe |
| Venda | Fatura | Venda.Fatura |
| Venda | Imposto | Venda.Imposto |
| Venda | TotalIbsCbs | Venda.TotalIbsCbs |
| Venda | Pagamentos | Venda.Pagamentos (navegacao) |
| Venda | Itens | Venda.Itens (navegacao) |
| Venda | CaminhoPdfCupomNaoFiscal | Venda.CaminhoPdfCupomNaoFiscal |
| Venda | NumeroCupomNaoFiscal | Venda.NumeroCupomNaoFiscal |
| Venda | AutorizacoesXml | Venda.AutorizacoesXml (navegacao) |
| Venda | Referenciadas | Venda.Referenciadas (navegacao) |
| Venda | VendaOrigem | Venda.VendaOrigem |
| Venda | FatoGeradorFinanceiro | AUSENTE |
| Venda | VendaEntrega | Venda.VendaEntrega |
| Venda | VendaCobrancaEndereco | Venda.VendaCobrancaEndereco |
| Venda | NfHistoricos | Venda.NfHistoricos (navegacao) |
| Venda | DataUltimoProcessamento | Venda.DataUltimoProcessamento |
| Venda | IncluirFreteNoTotal | Venda.IncluirFreteNoTotal |
| VendaAutorizacaoXml | VendaId | VendaAutorizacaoXml.VendaId |
| VendaAutorizacaoXml | Documento | VendaAutorizacaoXml.Documento |
| VendaAutorizacaoXml | Venda | VendaAutorizacaoXml.Venda |
| VendaCobrancaEndereco | VendaId | VendaCobrancaEndereco.VendaId |
| VendaCobrancaEndereco | Nome | VendaCobrancaEndereco.Nome |
| VendaCobrancaEndereco | Fone | VendaCobrancaEndereco.Fone |
| VendaCobrancaEndereco | Email | VendaCobrancaEndereco.Email |
| VendaCobrancaEndereco | IE | VendaCobrancaEndereco.IE |
| VendaCobrancaEndereco | Documento | VendaCobrancaEndereco.Documento |
| VendaCobrancaEndereco | Uf | VendaCobrancaEndereco.Uf |
| VendaCobrancaEndereco | Logradouro | VendaCobrancaEndereco.Logradouro |
| VendaCobrancaEndereco | Numero | VendaCobrancaEndereco.Numero |
| VendaCobrancaEndereco | Complemento | VendaCobrancaEndereco.Complemento |
| VendaCobrancaEndereco | Bairro | VendaCobrancaEndereco.Bairro |
| VendaCobrancaEndereco | MunicipioId | VendaCobrancaEndereco.MunicipioId |
| VendaCobrancaEndereco | MunicipioNome | VendaCobrancaEndereco.MunicipioNome |
| VendaCobrancaEndereco | Cep | VendaCobrancaEndereco.Cep |
| VendaCobrancaEndereco | PaisId | VendaCobrancaEndereco.PaisId |
| VendaCobrancaEndereco | PaisNome | VendaCobrancaEndereco.PaisNome |
| VendaCobrancaEndereco | EnderecoId | VendaCobrancaEndereco.EnderecoId |
| VendaCobrancaEndereco | Venda | VendaCobrancaEndereco.Venda |
| VendaConfiguracao | VendaId | VendaConfiguracao.VendaId |
| VendaConfiguracao | TipoOperacao | VendaConfiguracao.TipoOperacao |
| VendaConfiguracao | TipoFormatoImpressaoDanfe | VendaConfiguracao.TipoFormatoImpressaoDanfe |
| VendaConfiguracao | TipoEmissao | VendaConfiguracao.TipoEmissao |
| VendaConfiguracao | TipoAmbiente | VendaConfiguracao.TipoAmbiente |
| VendaConfiguracao | FinalidadeEmissao | VendaConfiguracao.FinalidadeEmissao |
| VendaConfiguracao | IndicadorFinalidadeOperacao | VendaConfiguracao.IndicadorFinalidadeOperacao |
| VendaConfiguracao | TipoAtendimento | VendaConfiguracao.TipoAtendimento |
| VendaConfiguracao | IndicadorIntermediadorMarketplace | VendaConfiguracao.IndicadorIntermediadorMarketplace |
| VendaConfiguracao | Venda | VendaConfiguracao.Venda |
| VendaDestinatario | VendaId | VendaDestinatario.VendaId |
| VendaDestinatario | PessoaId | VendaDestinatario.PessoaId |
| VendaDestinatario | Cnpj | VendaDestinatario.Cnpj |
| VendaDestinatario | Cpf | VendaDestinatario.Cpf |
| VendaDestinatario | RazaoSocial | VendaDestinatario.RazaoSocial |
| VendaDestinatario | Telefone | VendaDestinatario.Telefone |
| VendaDestinatario | InscricaoEstadual | VendaDestinatario.InscricaoEstadual |
| VendaDestinatario | IdentificadorEstrangeiro | VendaDestinatario.IdentificadorEstrangeiro |
| VendaDestinatario | IndicadorIE | VendaDestinatario.IndicadorIE |
| VendaDestinatario | Email | VendaDestinatario.Email |
| VendaDestinatario | EhConsumidorFinal | VendaDestinatario.EhConsumidorFinal |
| VendaDestinatario | EnviarDestinatatioNaNfce | VendaDestinatario.EnviarDestinatatioNaNfce |
| VendaDestinatario | DocumentoConsumidor | VendaDestinatario.DocumentoConsumidor |
| VendaDestinatario | Enderecos | VendaDestinatario.Enderecos (navegacao) |
| VendaDestinatario | Venda | VendaDestinatario.Venda |
| VendaDestinatarioEndereco | VendaDestinatarioId | VendaDestinatarioEndereco.VendaDestinatarioId |
| VendaDestinatarioEndereco | TipoEndereco | VendaDestinatarioEndereco.TipoEndereco |
| VendaDestinatarioEndereco | Uf | VendaDestinatarioEndereco.Uf |
| VendaDestinatarioEndereco | Logradouro | VendaDestinatarioEndereco.Logradouro |
| VendaDestinatarioEndereco | Numero | VendaDestinatarioEndereco.Numero |
| VendaDestinatarioEndereco | Complemento | VendaDestinatarioEndereco.Complemento |
| VendaDestinatarioEndereco | Bairro | VendaDestinatarioEndereco.Bairro |
| VendaDestinatarioEndereco | MunicipioId | VendaDestinatarioEndereco.MunicipioId |
| VendaDestinatarioEndereco | MunicipioNome | VendaDestinatarioEndereco.MunicipioNome |
| VendaDestinatarioEndereco | Cep | VendaDestinatarioEndereco.Cep |
| VendaDestinatarioEndereco | PaisId | VendaDestinatarioEndereco.PaisId |
| VendaDestinatarioEndereco | PaisNome | VendaDestinatarioEndereco.PaisNome |
| VendaDestinatarioEndereco | VendaDestinatario | VendaDestinatarioEndereco.VendaDestinatario |
| VendaEmitente | VendaId | VendaEmitente.VendaId |
| VendaEmitente | EmpresaId | VendaEmitente.EmpresaId |
| VendaEmitente | Cnpj | VendaEmitente.Cnpj |
| VendaEmitente | Cpf | VendaEmitente.Cpf |
| VendaEmitente | RazaoSocial | VendaEmitente.RazaoSocial |
| VendaEmitente | NomeFantasia | VendaEmitente.NomeFantasia |
| VendaEmitente | Telefone | VendaEmitente.Telefone |
| VendaEmitente | InscricaoEstadual | VendaEmitente.InscricaoEstadual |
| VendaEmitente | InscricaoEstadualST | VendaEmitente.InscricaoEstadualST |
| VendaEmitente | InscricaoMunicipal | VendaEmitente.InscricaoMunicipal |
| VendaEmitente | Cnae | VendaEmitente.Cnae |
| VendaEmitente | RegimeTributario | VendaEmitente.RegimeTributario |
| VendaEmitente | Endereco | VendaEmitente.Endereco |
| VendaEmitente | Venda | VendaEmitente.Venda |
| VendaEmitenteEndereco | Uf | VendaEmitenteEndereco.Uf |
| VendaEmitenteEndereco | Logradouro | VendaEmitenteEndereco.Logradouro |
| VendaEmitenteEndereco | Numero | VendaEmitenteEndereco.Numero |
| VendaEmitenteEndereco | Complemento | VendaEmitenteEndereco.Complemento |
| VendaEmitenteEndereco | Bairro | VendaEmitenteEndereco.Bairro |
| VendaEmitenteEndereco | MunicipioId | VendaEmitenteEndereco.MunicipioId |
| VendaEmitenteEndereco | MunicipioNome | VendaEmitenteEndereco.MunicipioNome |
| VendaEmitenteEndereco | Cep | VendaEmitenteEndereco.Cep |
| VendaEmitenteEndereco | PaisId | VendaEmitenteEndereco.PaisId |
| VendaEmitenteEndereco | PaisNome | VendaEmitenteEndereco.PaisNome |
| VendaEntrega | VendaId | VendaEntrega.VendaId |
| VendaEntrega | Nome | VendaEntrega.Nome |
| VendaEntrega | Fone | VendaEntrega.Fone |
| VendaEntrega | Email | VendaEntrega.Email |
| VendaEntrega | IE | VendaEntrega.IE |
| VendaEntrega | Documento | VendaEntrega.Documento |
| VendaEntrega | Uf | VendaEntrega.Uf |
| VendaEntrega | Logradouro | VendaEntrega.Logradouro |
| VendaEntrega | Numero | VendaEntrega.Numero |
| VendaEntrega | Complemento | VendaEntrega.Complemento |
| VendaEntrega | Bairro | VendaEntrega.Bairro |
| VendaEntrega | MunicipioId | VendaEntrega.MunicipioId |
| VendaEntrega | MunicipioNome | VendaEntrega.MunicipioNome |
| VendaEntrega | Cep | VendaEntrega.Cep |
| VendaEntrega | PaisId | VendaEntrega.PaisId |
| VendaEntrega | PaisNome | VendaEntrega.PaisNome |
| VendaEntrega | EnderecoId | VendaEntrega.EnderecoId |
| VendaEntrega | Venda | VendaEntrega.Venda |
| VendaFatura | VendaId | VendaFatura.VendaId |
| VendaFatura | NumeroFatura | VendaFatura.NumeroFatura |
| VendaFatura | ValorOriginal | VendaFatura.ValorOriginal |
| VendaFatura | ValorDesconto | VendaFatura.ValorDesconto |
| VendaFatura | ValorLiquido | VendaFatura.ValorLiquido |
| VendaFatura | Duplicatas | VendaFatura.Duplicatas (navegacao) |
| VendaFatura | Venda | VendaFatura.Venda |
| VendaFaturaDuplicata | VendaFaturaId | VendaFaturaDuplicata.VendaFaturaId |
| VendaFaturaDuplicata | NumeroDuplicata | VendaFaturaDuplicata.NumeroDuplicata |
| VendaFaturaDuplicata | DataVencimento | VendaFaturaDuplicata.DataVencimento |
| VendaFaturaDuplicata | ValorDuplicata | VendaFaturaDuplicata.ValorDuplicata |
| VendaFaturaDuplicata | VendaFatura | VendaFaturaDuplicata.VendaFatura |
| VendaImposto | VendaId | VendaImposto.VendaId |
| VendaImposto | ValorAliquotaCreditoIcms | VendaImposto.ValorAliquotaCreditoIcms |
| VendaItem | VendaId | VendaItem.VendaId |
| VendaItem | ProdutoId | VendaItem.ProdutoId |
| VendaItem | CodigoProduto | VendaItem.CodigoProduto |
| VendaItem | CodigoEan | VendaItem.CodigoEan |
| VendaItem | DescricaoProduto | VendaItem.DescricaoProduto |
| VendaItem | Ncm | VendaItem.Ncm |
| VendaItem | ExcecaoNcmTipi | VendaItem.ExcecaoNcmTipi |
| VendaItem | CestId | VendaItem.CestId |
| VendaItem | Cest | VendaItem.Cest |
| VendaItem | CodigoAnpId | VendaItem.CodigoAnpId |
| VendaItem | CodigoAnp | VendaItem.CodigoAnp |
| VendaItem | Cfop | VendaItem.Cfop |
| VendaItem | UnidadeComercial | VendaItem.UnidadeComercial |
| VendaItem | QuantidadeComercial | VendaItem.QuantidadeComercial |
| VendaItem | ValorUnitarioComercial | VendaItem.ValorUnitarioComercial |
| VendaItem | ValorTotalBrutoProdutos | VendaItem.ValorTotalBrutoProdutos |
| VendaItem | CodigoEanTributavel | VendaItem.CodigoEanTributavel |
| VendaItem | UnidadeTributavel | VendaItem.UnidadeTributavel |
| VendaItem | QuantidadeTributavel | VendaItem.QuantidadeTributavel |
| VendaItem | ValorUnitarioTributavel | VendaItem.ValorUnitarioTributavel |
| VendaItem | ValorDesconto | VendaItem.ValorDesconto |
| VendaItem | ValorDescontoRateado | VendaItem.ValorDescontoRateado |
| VendaItem | ValorFreteRateado | VendaItem.ValorFreteRateado |
| VendaItem | ValorSeguroRateado | VendaItem.ValorSeguroRateado |
| VendaItem | ValorOutrasDepesasAcessoriasRateado | VendaItem.ValorOutrasDepesasAcessoriasRateado |
| VendaItem | CompoeValorTotal | VendaItem.CompoeValorTotal |
| VendaItem | InformacoesAdicionaisDoProduto | VendaItem.InformacoesAdicionaisDoProduto |
| VendaItem | CfopCorrelacao | VendaItem.CfopCorrelacao |
| VendaItem | IntegraFaturamento | VendaItem.IntegraFaturamento |
| VendaItem | NumeroItemPedidoCompra | VendaItem.NumeroItemPedidoCompra |
| VendaItem | NumeroPedidoCompra | VendaItem.NumeroPedidoCompra |
| VendaItem | FichaConteudoImportacao | VendaItem.FichaConteudoImportacao |
| VendaItem | CodigoBeneficioFiscal | VendaItem.CodigoBeneficioFiscal |
| VendaItem | ValorCusto | VendaItem.ValorCusto |
| VendaItem | Imposto | VendaItem.Imposto |
| VendaItem | ImpostoValorAproximado | VendaItem.ImpostoValorAproximado |
| VendaItem | Combustivel | VendaItem.Combustivel |
| VendaItem | ImpostoIbsCbs | VendaItem.ImpostoIbsCbs |
| VendaItem | Venda | VendaItem.Venda |
| VendaItemCombustivel | VendaItemId | VendaItemCombustivel.VendaItemId |
| VendaItemCombustivel | CodigoAnp | VendaItemCombustivel.CodigoAnp |
| VendaItemCombustivel | DescricaoAnp | VendaItemCombustivel.DescricaoAnp |
| VendaItemCombustivel | QuantidadeCombustivelFaturada | VendaItemCombustivel.QuantidadeCombustivelFaturada |
| VendaItemCombustivel | UfConsumo | VendaItemCombustivel.UfConsumo |
| VendaItemCombustivel | PercentualGlpDerivadoPetroleo | VendaItemCombustivel.PercentualGlpDerivadoPetroleo |
| VendaItemCombustivel | PercentualGasNaturalNacional | VendaItemCombustivel.PercentualGasNaturalNacional |
| VendaItemCombustivel | PercentualGasNaturalImportado | VendaItemCombustivel.PercentualGasNaturalImportado |
| VendaItemCombustivel | ValorPartida | VendaItemCombustivel.ValorPartida |
| VendaItemCombustivel | Origens | VendaItemCombustivel.Origens (navegacao) |
| VendaItemCombustivel | VendaItem | VendaItemCombustivel.VendaItem |
| VendaItemCombustivelOrigem | VendaItemCombustivelId | VendaItemCombustivelOrigem.VendaItemCombustivelId |
| VendaItemCombustivelOrigem | IndicadorImportacao | VendaItemCombustivelOrigem.IndicadorImportacao |
| VendaItemCombustivelOrigem | UfOrigem | VendaItemCombustivelOrigem.UfOrigem |
| VendaItemCombustivelOrigem | PercentualOrigem | VendaItemCombustivelOrigem.PercentualOrigem |
| VendaItemCombustivelOrigem | VendaItemCombustivel | VendaItemCombustivelOrigem.VendaItemCombustivel |
| VendaItemImposto | VendaItemId | VendaItemImposto.VendaItemId |
| VendaItemImposto | Origem | VendaItemImposto.Origem |
| VendaItemImposto | CstIcms | VendaItemImposto.CstIcms |
| VendaItemImposto | Csosn | VendaItemImposto.Csosn |
| VendaItemImposto | ModalidadeDeterminacaoBaseCalculoIcms | VendaItemImposto.ModalidadeDeterminacaoBaseCalculoIcms |
| VendaItemImposto | ValorBaseDeCalculoIcms | VendaItemImposto.ValorBaseDeCalculoIcms |
| VendaItemImposto | PercentualReducaoBaseDeCalculoIcms | VendaItemImposto.PercentualReducaoBaseDeCalculoIcms |
| VendaItemImposto | AliquotaIcms | VendaItemImposto.AliquotaIcms |
| VendaItemImposto | ValorImpostoIcms | VendaItemImposto.ValorImpostoIcms |
| VendaItemImposto | ModalidadeBaseDeCalculosST | VendaItemImposto.ModalidadeBaseDeCalculosST |
| VendaItemImposto | PercentualMvaBaseDeCalculoST | VendaItemImposto.PercentualMvaBaseDeCalculoST |
| VendaItemImposto | PercentualReducaoBaseDeCalculoST | VendaItemImposto.PercentualReducaoBaseDeCalculoST |
| VendaItemImposto | ValorBaseDeCalculoSt | VendaItemImposto.ValorBaseDeCalculoSt |
| VendaItemImposto | AliquotaSt | VendaItemImposto.AliquotaSt |
| VendaItemImposto | ValorImpostoSt | VendaItemImposto.ValorImpostoSt |
| VendaItemImposto | MotivoDesoneracaoIcms | VendaItemImposto.MotivoDesoneracaoIcms |
| VendaItemImposto | ValorBaseDeCalculoStRetido | VendaItemImposto.ValorBaseDeCalculoStRetido |
| VendaItemImposto | ValorImpostoStRetido | VendaItemImposto.ValorImpostoStRetido |
| VendaItemImposto | PercentualCreditoSimplesNacionalIcms | VendaItemImposto.PercentualCreditoSimplesNacionalIcms |
| VendaItemImposto | ValorImpostoCreditoSimplesNacionalIcms | VendaItemImposto.ValorImpostoCreditoSimplesNacionalIcms |
| VendaItemImposto | ValorBaseDeCalculoFcp | VendaItemImposto.ValorBaseDeCalculoFcp |
| VendaItemImposto | PercentualFcp | VendaItemImposto.PercentualFcp |
| VendaItemImposto | ValorImpostoFcp | VendaItemImposto.ValorImpostoFcp |
| VendaItemImposto | ValorOperacaoDiferimentoIcms | VendaItemImposto.ValorOperacaoDiferimentoIcms |
| VendaItemImposto | PercentualDiferimentoIcms | VendaItemImposto.PercentualDiferimentoIcms |
| VendaItemImposto | ValorImpostoDiferimentoIcms | VendaItemImposto.ValorImpostoDiferimentoIcms |
| VendaItemImposto | CstIpiSaida | VendaItemImposto.CstIpiSaida |
| VendaItemImposto | ValorBaseDeCalculoIpi | VendaItemImposto.ValorBaseDeCalculoIpi |
| VendaItemImposto | AliquotaIpi | VendaItemImposto.AliquotaIpi |
| VendaItemImposto | ValorImpostoDiferimentoIpi | VendaItemImposto.ValorImpostoDiferimentoIpi |
| VendaItemImposto | ValorQuantidadeTotalParaTributacaoIpi | VendaItemImposto.ValorQuantidadeTotalParaTributacaoIpi |
| VendaItemImposto | ValorPorUnidadeTributavelIpi | VendaItemImposto.ValorPorUnidadeTributavelIpi |
| VendaItemImposto | CstPis | VendaItemImposto.CstPis |
| VendaItemImposto | ValorBaseDeCalculoPis | VendaItemImposto.ValorBaseDeCalculoPis |
| VendaItemImposto | AliquotaPis | VendaItemImposto.AliquotaPis |
| VendaItemImposto | ValorQuantidadeVendidaProdutoPis | VendaItemImposto.ValorQuantidadeVendidaProdutoPis |
| VendaItemImposto | AliquotaPorUnidadeVendidaPis | VendaItemImposto.AliquotaPorUnidadeVendidaPis |
| VendaItemImposto | ValorImpostoDiferimentoPis | VendaItemImposto.ValorImpostoDiferimentoPis |
| VendaItemImposto | CstCofins | VendaItemImposto.CstCofins |
| VendaItemImposto | ValorBaseDeCalculoCofins | VendaItemImposto.ValorBaseDeCalculoCofins |
| VendaItemImposto | AliquotaCofins | VendaItemImposto.AliquotaCofins |
| VendaItemImposto | ValorQuantidadeVendidaProdutoCofins | VendaItemImposto.ValorQuantidadeVendidaProdutoCofins |
| VendaItemImposto | AliquotaPorUnidadeVendidaCofins | VendaItemImposto.AliquotaPorUnidadeVendidaCofins |
| VendaItemImposto | ValorImpostoDiferimentoCofins | VendaItemImposto.ValorImpostoDiferimentoCofins |
| VendaItemImposto | TipoReducaoIcms | VendaItemImposto.TipoReducaoIcms |
| VendaItemImposto | TipoReducaoIcmsSt | VendaItemImposto.TipoReducaoIcmsSt |
| VendaItemImposto | ValorBaseDeCalculoFcpSt | VendaItemImposto.ValorBaseDeCalculoFcpSt |
| VendaItemImposto | PercentualFcpSt | VendaItemImposto.PercentualFcpSt |
| VendaItemImposto | ValorImpostoFcpSt | VendaItemImposto.ValorImpostoFcpSt |
| VendaItemImposto | ValorIcmsProprioSubistituto | VendaItemImposto.ValorIcmsProprioSubistituto |
| VendaItemImposto | ValorAliquotaIcmsInterna | VendaItemImposto.ValorAliquotaIcmsInterna |
| VendaItemImposto | ValorAliquotaIcmsInternaEstadual | VendaItemImposto.ValorAliquotaIcmsInternaEstadual |
| VendaItemImposto | EnquadramentoIpi | VendaItemImposto.EnquadramentoIpi |
| VendaItemImposto | ValorReducaoIpiPercentual | VendaItemImposto.ValorReducaoIpiPercentual |
| VendaItemImposto | IpiEmbutido | VendaItemImposto.IpiEmbutido |
| VendaItemImposto | DifalTipoCalculoPorDentro | VendaItemImposto.DifalTipoCalculoPorDentro |
| VendaItemImposto | TipoReducaoIpi | VendaItemImposto.TipoReducaoIpi |
| VendaItemImposto | TipoCalculoBaseIcmsSt | VendaItemImposto.TipoCalculoBaseIcmsSt |
| VendaItemImposto | ValorUnitFixadoIcmsSt | VendaItemImposto.ValorUnitFixadoIcmsSt |
| VendaItemImposto | ValorBaseDeCalculoDifal | VendaItemImposto.ValorBaseDeCalculoDifal |
| VendaItemImposto | ValorImpostoDevidoDifal | VendaItemImposto.ValorImpostoDevidoDifal |
| VendaItemImposto | ValorImpostoDevidoRecolherSt | VendaItemImposto.ValorImpostoDevidoRecolherSt |
| VendaItemImposto | ValorImpostoDevidoFcp | VendaItemImposto.ValorImpostoDevidoFcp |
| VendaItemImposto | ValorIcmsIsento | VendaItemImposto.ValorIcmsIsento |
| VendaItemImposto | ValorIcmsOutros | VendaItemImposto.ValorIcmsOutros |
| VendaItemImposto | IcmsObservacao | VendaItemImposto.IcmsObservacao |
| VendaItemImposto | ValorIpiIsento | VendaItemImposto.ValorIpiIsento |
| VendaItemImposto | ValorIpiOutros | VendaItemImposto.ValorIpiOutros |
| VendaItemImposto | IpiObservacao | VendaItemImposto.IpiObservacao |
| VendaItemImposto | VendaItem | VendaItemImposto.VendaItem |
| VendaItemImpostoIbsCbs | VendaItemId | VendaItemImpostoIbsCbs.VendaItemId |
| VendaItemImpostoIbsCbs | Cst | VendaItemImpostoIbsCbs.Cst |
| VendaItemImpostoIbsCbs | CClassTrib | VendaItemImpostoIbsCbs.CClassTrib |
| VendaItemImpostoIbsCbs | AliquotaEstadual | VendaItemImpostoIbsCbs.AliquotaEstadual |
| VendaItemImpostoIbsCbs | AliquotaMunicipal | VendaItemImpostoIbsCbs.AliquotaMunicipal |
| VendaItemImpostoIbsCbs | AliquotaCbs | VendaItemImpostoIbsCbs.AliquotaCbs |
| VendaItemImpostoIbsCbs | AliquotaEstadualReducao | VendaItemImpostoIbsCbs.AliquotaEstadualReducao |
| VendaItemImpostoIbsCbs | AliquotaMunicipalReducao | VendaItemImpostoIbsCbs.AliquotaMunicipalReducao |
| VendaItemImpostoIbsCbs | AliquotaCbsReducao | VendaItemImpostoIbsCbs.AliquotaCbsReducao |
| VendaItemImpostoIbsCbs | AliquotaEstadualDiferimento | VendaItemImpostoIbsCbs.AliquotaEstadualDiferimento |
| VendaItemImpostoIbsCbs | AliquotaMunicipalDiferimento | VendaItemImpostoIbsCbs.AliquotaMunicipalDiferimento |
| VendaItemImpostoIbsCbs | AliquotaCbsDiferimento | VendaItemImpostoIbsCbs.AliquotaCbsDiferimento |
| VendaItemImpostoIbsCbs | AliquotaEfetivaEstadual | VendaItemImpostoIbsCbs.AliquotaEfetivaEstadual |
| VendaItemImpostoIbsCbs | AliquotaEfetivaMunicipal | VendaItemImpostoIbsCbs.AliquotaEfetivaMunicipal |
| VendaItemImpostoIbsCbs | AliquotaEfetivaCbs | VendaItemImpostoIbsCbs.AliquotaEfetivaCbs |
| VendaItemImpostoIbsCbs | ValorBaseDeCalculo | VendaItemImpostoIbsCbs.ValorBaseDeCalculo |
| VendaItemImpostoIbsCbs | ValorImpostoDevidoEstadual | VendaItemImpostoIbsCbs.ValorImpostoDevidoEstadual |
| VendaItemImpostoIbsCbs | ValorImpostoDevidoMunicipal | VendaItemImpostoIbsCbs.ValorImpostoDevidoMunicipal |
| VendaItemImpostoIbsCbs | ValorImpostoDevidoCbs | VendaItemImpostoIbsCbs.ValorImpostoDevidoCbs |
| VendaItemImpostoIbsCbs | VendaItemImpostoIbsCbsTributacaoRegular | VendaItemImpostoIbsCbs.VendaItemImpostoIbsCbsTributacaoRegular |
| VendaItemImpostoIbsCbs | VendaItem | VendaItemImpostoIbsCbs.VendaItem |
| VendaItemImpostoIbsCbsTributacaoRegular | VendaItemImpostoIbsCbsId | VendaItemImpostoIbsCbsTributacaoRegular.VendaItemImpostoIbsCbsId |
| VendaItemImpostoIbsCbsTributacaoRegular | Cst | VendaItemImpostoIbsCbsTributacaoRegular.Cst |
| VendaItemImpostoIbsCbsTributacaoRegular | CClassTrib | VendaItemImpostoIbsCbsTributacaoRegular.CClassTrib |
| VendaItemImpostoIbsCbsTributacaoRegular | AliquotaEfetivaIbsEstadual | VendaItemImpostoIbsCbsTributacaoRegular.AliquotaEfetivaIbsEstadual |
| VendaItemImpostoIbsCbsTributacaoRegular | ValorIbsEstadual | VendaItemImpostoIbsCbsTributacaoRegular.ValorIbsEstadual |
| VendaItemImpostoIbsCbsTributacaoRegular | AliquotaEfetivaIbsMunicipal | VendaItemImpostoIbsCbsTributacaoRegular.AliquotaEfetivaIbsMunicipal |
| VendaItemImpostoIbsCbsTributacaoRegular | ValorIbsMunicipal | VendaItemImpostoIbsCbsTributacaoRegular.ValorIbsMunicipal |
| VendaItemImpostoIbsCbsTributacaoRegular | AliquotaEfetivaCbs | VendaItemImpostoIbsCbsTributacaoRegular.AliquotaEfetivaCbs |
| VendaItemImpostoIbsCbsTributacaoRegular | ValorCbs | VendaItemImpostoIbsCbsTributacaoRegular.ValorCbs |
| VendaItemImpostoIbsCbsTributacaoRegular | VendaItemImpostoIbsCbs | VendaItemImpostoIbsCbsTributacaoRegular.VendaItemImpostoIbsCbs |
| VendaItemImpostoValorAproximado | VendaItemId | VendaItemImpostoValorAproximado.VendaItemId |
| VendaItemImpostoValorAproximado | AliquotaNacionalFederal | VendaItemImpostoValorAproximado.AliquotaNacionalFederal |
| VendaItemImpostoValorAproximado | AliquotaImportadoFederal | VendaItemImpostoValorAproximado.AliquotaImportadoFederal |
| VendaItemImpostoValorAproximado | AliquotaEstadual | VendaItemImpostoValorAproximado.AliquotaEstadual |
| VendaItemImpostoValorAproximado | AliquotaMunicipal | VendaItemImpostoValorAproximado.AliquotaMunicipal |
| VendaItemImpostoValorAproximado | Versao | VendaItemImpostoValorAproximado.Versao |
| VendaItemImpostoValorAproximado | Fonte | VendaItemImpostoValorAproximado.Fonte |
| VendaItemImpostoValorAproximado | VendaItem | VendaItemImpostoValorAproximado.VendaItem |
| VendaNfHistorico | Id | (herdado EntidadeSaaSBase) |
| VendaNfHistorico | VendaId | VendaNfHistorico.VendaId |
| VendaNfHistorico | Descricao | VendaNfHistorico.Descricao |
| VendaNfHistorico | TenantId | (herdado EntidadeSaaSBase) |
| VendaNfHistorico | DataCadastro | VendaNfHistorico.DataCadastro (navegacao) |
| VendaNfce | VendaId | VendaNfce.VendaId |
| VendaNfce | Numero | VendaNfce.Numero |
| VendaNfce | Serie | VendaNfce.Serie |
| VendaNfce | IdCsc | VendaNfce.IdCsc |
| VendaNfce | Csc | VendaNfce.Csc |
| VendaNfce | StatusInterno | VendaNfce.StatusInterno |
| VendaNfce | Chave | VendaNfce.Chave |
| VendaNfce | DataHoraEmissao | VendaNfce.DataHoraEmissao |
| VendaNfce | StatusSefaz | VendaNfce.StatusSefaz |
| VendaNfce | Protocolo | VendaNfce.Protocolo |
| VendaNfce | Xml | VendaNfce.Xml |
| VendaNfce | UltimoRetornoMensagemSefaz | VendaNfce.UltimoRetornoMensagemSefaz |
| VendaNfce | DataHoraCancelamento | VendaNfce.DataHoraCancelamento |
| VendaNfce | ProtocoloCancelamento | VendaNfce.ProtocoloCancelamento |
| VendaNfce | StatusSefazCancelamento | VendaNfce.StatusSefazCancelamento |
| VendaNfce | MotivoCancelamento | VendaNfce.MotivoCancelamento |
| VendaNfce | XmlCancelamento | VendaNfce.XmlCancelamento |
| VendaNfe | VendaId | VendaNfe.VendaId |
| VendaNfe | Numero | VendaNfe.Numero |
| VendaNfe | Serie | VendaNfe.Serie |
| VendaNfe | DataHoraEmissao | VendaNfe.DataHoraEmissao |
| VendaNfe | DataHoraSaida | VendaNfe.DataHoraSaida |
| VendaNfe | StatusInterno | VendaNfe.StatusInterno |
| VendaNfe | StatusSefaz | VendaNfe.StatusSefaz |
| VendaNfe | Chave | VendaNfe.Chave |
| VendaNfe | Protocolo | VendaNfe.Protocolo |
| VendaNfe | Xml | VendaNfe.Xml |
| VendaNfe | UltimoRetornoMensagemSefaz | VendaNfe.UltimoRetornoMensagemSefaz |
| VendaNfe | DataHoraCancelamento | VendaNfe.DataHoraCancelamento |
| VendaNfe | ProtocoloCancelamento | VendaNfe.ProtocoloCancelamento |
| VendaNfe | StatusSefazCancelamento | VendaNfe.StatusSefazCancelamento |
| VendaNfe | MotivoCancelamento | VendaNfe.MotivoCancelamento |
| VendaNfe | XmlCancelamento | VendaNfe.XmlCancelamento |
| VendaNfe | EmbuteFrete | VendaNfe.EmbuteFrete |
| VendaNfe | EmbuteSeguro | VendaNfe.EmbuteSeguro |
| VendaNfe | EmbuteAcrescimo | VendaNfe.EmbuteAcrescimo |
| VendaNfe | EmbuteOutro | VendaNfe.EmbuteOutro |
| VendaNfe | Intermediador | VendaNfe.Intermediador |
| VendaNfe | CartasCorrecoes | VendaNfe.CartasCorrecoes (navegacao) |
| VendaNfe | VendaNfeExportacao | VendaNfe.VendaNfeExportacao |
| VendaNfeCartaCorrecao | VendaNfeId | VendaNfeCartaCorrecao.VendaNfeId |
| VendaNfeCartaCorrecao | TextoCorrecao | VendaNfeCartaCorrecao.TextoCorrecao |
| VendaNfeCartaCorrecao | SequenciaEvento | VendaNfeCartaCorrecao.SequenciaEvento |
| VendaNfeCartaCorrecao | StatusSefaz | VendaNfeCartaCorrecao.StatusSefaz |
| VendaNfeCartaCorrecao | MotivoRejeicaoSefaz | VendaNfeCartaCorrecao.MotivoRejeicaoSefaz |
| VendaNfeCartaCorrecao | VendaNfe | VendaNfeCartaCorrecao.VendaNfe |
| VendaNfeExportacao | VendaNfeId | VendaNfeExportacao.VendaNfeId |
| VendaNfeExportacao | UfSaidaPais | VendaNfeExportacao.UfSaidaPais |
| VendaNfeExportacao | LocalExportacao | VendaNfeExportacao.LocalExportacao |
| VendaNfeExportacao | LocalDespacho | VendaNfeExportacao.LocalDespacho |
| VendaNfeExportacao | VendaNfe | VendaNfeExportacao.VendaNfe |
| VendaNfeHistorico | VendaId | AUSENTE |
| VendaNfeIntermediador | VendaNfeId | VendaNfeIntermediador.VendaNfeId |
| VendaNfeIntermediador | Documento | VendaNfeIntermediador.Documento |
| VendaNfeIntermediador | IdentificadorIntermediador | VendaNfeIntermediador.IdentificadorIntermediador |
| VendaNfeIntermediador | VendaNfe | VendaNfeIntermediador.VendaNfe |
| VendaNfeReferenciada | VendaId | VendaNfeReferenciada.VendaId |
| VendaNfeReferenciada | Chave | VendaNfeReferenciada.Chave |
| VendaNfeReferenciada | Venda | VendaNfeReferenciada.Venda |
| VendaPagamento | VendaId | VendaPagamento.VendaId |
| VendaPagamento | ValorTroco | VendaPagamento.ValorTroco |
| VendaPagamento | IndicadorPagamento | VendaPagamento.IndicadorPagamento |
| VendaPagamento | TipoPagamento | VendaPagamento.TipoPagamento |
| VendaPagamento | ValorPagamento | VendaPagamento.ValorPagamento |
| VendaPagamento | CartaoTipoIntegracao | VendaPagamento.CartaoTipoIntegracao |
| VendaPagamento | CartaoCnpjIntermediadorFinanceira | VendaPagamento.CartaoCnpjIntermediadorFinanceira |
| VendaPagamento | CartaoBandeira | VendaPagamento.CartaoBandeira |
| VendaPagamento | CartaoCodigoAutorizacaoOperacao | VendaPagamento.CartaoCodigoAutorizacaoOperacao |
| VendaTotal | VendaId | VendaTotal.VendaId |
| VendaTotal | ValorBaseDeCalculoIcms | VendaTotal.ValorBaseDeCalculoIcms |
| VendaTotal | ValorIcms | VendaTotal.ValorIcms |
| VendaTotal | ValorIcmsDesonerado | VendaTotal.ValorIcmsDesonerado |
| VendaTotal | ValorFcp | VendaTotal.ValorFcp |
| VendaTotal | ValorBaseDeCalculoSt | VendaTotal.ValorBaseDeCalculoSt |
| VendaTotal | ValorSt | VendaTotal.ValorSt |
| VendaTotal | ValorFcpSt | VendaTotal.ValorFcpSt |
| VendaTotal | ValorFcpRetido | VendaTotal.ValorFcpRetido |
| VendaTotal | ValorProduto | VendaTotal.ValorProduto |
| VendaTotal | ValorFrete | VendaTotal.ValorFrete |
| VendaTotal | ValorSeguro | VendaTotal.ValorSeguro |
| VendaTotal | ValorDesconto | VendaTotal.ValorDesconto |
| VendaTotal | ValorImpostoImportacao | VendaTotal.ValorImpostoImportacao |
| VendaTotal | ValorIpi | VendaTotal.ValorIpi |
| VendaTotal | ValorIpiDevolucao | VendaTotal.ValorIpiDevolucao |
| VendaTotal | ValorPis | VendaTotal.ValorPis |
| VendaTotal | ValorCofins | VendaTotal.ValorCofins |
| VendaTotal | ValorOutro | VendaTotal.ValorOutro |
| VendaTotal | ValorNotaFiscal | VendaTotal.ValorNotaFiscal |
| VendaTotal | Venda | VendaTotal.Venda |
| VendaTotalIbsCbs | VendaId | VendaTotalIbsCbs.VendaId |
| VendaTotalIbsCbs | ValorBaseDeCalculo | VendaTotalIbsCbs.ValorBaseDeCalculo |
| VendaTotalIbsCbs | ValorImpostoDevidoEstadual | VendaTotalIbsCbs.ValorImpostoDevidoEstadual |
| VendaTotalIbsCbs | ValorImpostoDevidoMunicipal | VendaTotalIbsCbs.ValorImpostoDevidoMunicipal |
| VendaTotalIbsCbs | ValorImpostoDevidoCbs | VendaTotalIbsCbs.ValorImpostoDevidoCbs |
| VendaTotalIbsCbs | Venda | VendaTotalIbsCbs.Venda |
| VendaTransporte | VendaId | VendaTransporte.VendaId |
| VendaTransporte | Transportadora | VendaTransporte.Transportadora |
| VendaTransporte | Veiculo | VendaTransporte.Veiculo |
| VendaTransporte | Volumes | VendaTransporte.Volumes (navegacao) |
| VendaTransporte | Reboques | VendaTransporte.Reboques (navegacao) |
| VendaTransporteReboque | VendaTransporteId | VendaTransporteReboque.VendaTransporteId |
| VendaTransporteReboque | VeiculoId | VendaTransporteReboque.VeiculoId |
| VendaTransporteReboque | Placa | VendaTransporteReboque.Placa |
| VendaTransporteReboque | Uf | VendaTransporteReboque.Uf |
| VendaTransporteReboque | Rntrc | VendaTransporteReboque.Rntrc |
| VendaTransporteReboque | Transporte | VendaTransporteReboque.Transporte |
| VendaTransporteReboque | Veiculo | AUSENTE |
| VendaTransporteTransportadora | VendaTransporteId | VendaTransporteTransportadora.VendaTransporteId |
| VendaTransporteTransportadora | PessoaId | VendaTransporteTransportadora.PessoaId |
| VendaTransporteTransportadora | Cnpj | VendaTransporteTransportadora.Cnpj |
| VendaTransporteTransportadora | Cpf | VendaTransporteTransportadora.Cpf |
| VendaTransporteTransportadora | RazaoSocial | VendaTransporteTransportadora.RazaoSocial |
| VendaTransporteTransportadora | InscricaoEstadual | VendaTransporteTransportadora.InscricaoEstadual |
| VendaTransporteTransportadora | Logradouro | VendaTransporteTransportadora.Logradouro |
| VendaTransporteTransportadora | Numero | VendaTransporteTransportadora.Numero |
| VendaTransporteTransportadora | Complemento | VendaTransporteTransportadora.Complemento |
| VendaTransporteTransportadora | Bairro | VendaTransporteTransportadora.Bairro |
| VendaTransporteTransportadora | Municipio | VendaTransporteTransportadora.Municipio |
| VendaTransporteTransportadora | Uf | VendaTransporteTransportadora.Uf |
| VendaTransporteTransportadora | Transportadora | VendaTransporteTransportadora.Transportadora (navegacao) |
| VendaTransporteVeiculo | VendaTransporteId | VendaTransporteVeiculo.VendaTransporteId |
| VendaTransporteVeiculo | VeiculoId | VendaTransporteVeiculo.VeiculoId |
| VendaTransporteVeiculo | Placa | VendaTransporteVeiculo.Placa |
| VendaTransporteVeiculo | Uf | VendaTransporteVeiculo.Uf |
| VendaTransporteVeiculo | Rntrc | VendaTransporteVeiculo.Rntrc |
| VendaTransporteVeiculo | Transporte | VendaTransporteVeiculo.Transporte |
| VendaTransporteVeiculo | Veiculo | VendaTransporteVeiculo.Veiculo (navegacao) |
| VendaTransporteVolume | VendaTransporteId | VendaTransporteVolume.VendaTransporteId |
| VendaTransporteVolume | QuantidadeVolumes | VendaTransporteVolume.QuantidadeVolumes |
| VendaTransporteVolume | Especie | VendaTransporteVolume.Especie |
| VendaTransporteVolume | NumeroVolumes | VendaTransporteVolume.NumeroVolumes |
| VendaTransporteVolume | PesoLiquido | VendaTransporteVolume.PesoLiquido |
| VendaTransporteVolume | PesoBruto | VendaTransporteVolume.PesoBruto |
| VendaTransporteVolume | Marca | VendaTransporteVolume.Marca |
| VendaTransporteVolume | VendaTransporte | VendaTransporteVolume.VendaTransporte |

## Cobertura por entidade

| Entidade legada | Existe no novo | Campos cobertos | Total | % |
|---|---|---:|---:|---:|
| Venda | Sim | 29 | 30 | 97% |
| VendaAutorizacaoXml | Sim | 3 | 3 | 100% |
| VendaCobrancaEndereco | Sim | 18 | 18 | 100% |
| VendaConfiguracao | Sim | 10 | 10 | 100% |
| VendaDestinatario | Sim | 15 | 15 | 100% |
| VendaDestinatarioEndereco | Sim | 13 | 13 | 100% |
| VendaEmitente | Sim | 14 | 14 | 100% |
| VendaEmitenteEndereco | Sim | 10 | 10 | 100% |
| VendaEntrega | Sim | 18 | 18 | 100% |
| VendaFatura | Sim | 7 | 7 | 100% |
| VendaFaturaDuplicata | Sim | 5 | 5 | 100% |
| VendaImposto | Sim | 2 | 2 | 100% |
| VendaItem | Sim | 39 | 39 | 100% |
| VendaItemCombustivel | Sim | 11 | 11 | 100% |
| VendaItemCombustivelOrigem | Sim | 5 | 5 | 100% |
| VendaItemImposto | Sim | 70 | 70 | 100% |
| VendaItemImpostoIbsCbs | Sim | 21 | 21 | 100% |
| VendaItemImpostoIbsCbsTributacaoRegular | Sim | 10 | 10 | 100% |
| VendaItemImpostoValorAproximado | Sim | 8 | 8 | 100% |
| VendaNfHistorico | Sim | 5 | 5 | 100% |
| VendaNfce | Sim | 17 | 17 | 100% |
| VendaNfe | Sim | 23 | 23 | 100% |
| VendaNfeCartaCorrecao | Sim | 6 | 6 | 100% |
| VendaNfeExportacao | Sim | 5 | 5 | 100% |
| VendaNfeHistorico | NAO | 0 | 1 | 0% |
| VendaNfeIntermediador | Sim | 4 | 4 | 100% |
| VendaNfeReferenciada | Sim | 3 | 3 | 100% |
| VendaPagamento | Sim | 9 | 9 | 100% |
| VendaTotal | Sim | 21 | 21 | 100% |
| VendaTotalIbsCbs | Sim | 6 | 6 | 100% |
| VendaTransporte | Sim | 5 | 5 | 100% |
| VendaTransporteReboque | Sim | 6 | 7 | 86% |
| VendaTransporteTransportadora | Sim | 13 | 13 | 100% |
| VendaTransporteVeiculo | Sim | 7 | 7 | 100% |
| VendaTransporteVolume | Sim | 8 | 8 | 100% |
| **TOTAL** |  | **446** | **449** | **99.3%** |


## Entidades ausentes e campos criticos faltando

### Entidades ausentes
- **VendaNfeHistorico** (legado `: Entity`, unico campo `VendaId`): NAO existe como tipo no modulo novo. Funcionalmente redundante com `VendaNfHistorico` (que FOI portado, incluindo `Descricao` e `CriadoEm` no lugar de `DataCadastro`). Impacto BAIXO: provavel duplicata legada; confirmar se algum consumidor usava especificamente `VendaNfeHistorico`.

### Campos ausentes (nao navegacao)
- **Venda.FatoGeradorFinanceiro**: objeto de gatilho financeiro (cross-module). Nao portado no agregado Venda. O novo `Venda` referencia o modulo Fiscal por `DocumentoFiscalId` (Guid) e expoe `CaixaId`/`FormaPagamento`. Se a integracao com o Financeiro depender do fato gerador, validar onde essa responsabilidade passou a residir (modulo Financeiro/Caixa). Criticidade: MEDIA (depende do desenho alvo).

### Falsos-positivos resolvidos (navegacao)
Os seguintes "AUSENTE" iniciais eram apenas propriedades de navegacao (colecoes `ICollection<>`/referencias), com FK escalar coberta no destino, portanto considerados COBERTOS:
Venda.Itens, Venda.Pagamentos, Venda.AutorizacoesXml, Venda.Referenciadas, Venda.NfHistoricos, VendaDestinatario.Enderecos, VendaFatura.Duplicatas, VendaItemCombustivel.Origens, VendaNfe.CartasCorrecoes, VendaTransporte.Volumes, VendaTransporte.Reboques, VendaTransporteReboque.Veiculo (FK VeiculoId ok), VendaTransporteVeiculo.Veiculo (FK VeiculoId ok), VendaTransporteTransportadora.Transportadora (FK PessoaId ok).

### Cobertura estimada
- Entidades: 34 de 35 portadas (97%). A 35a (`VendaNfeHistorico`) e redundante.
- Campos: 446 de 449 cobertos (99,3%). Descontando navegacao pura, o unico dado escalar genuinamente ausente e `Venda.FatoGeradorFinanceiro` (cross-module).
