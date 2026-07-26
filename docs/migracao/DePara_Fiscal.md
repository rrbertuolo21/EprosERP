# De -> Para: Módulo Fiscal (Legado Epros.ERP -> Epros.Modules.Fiscal)

Auditoria de fidelidade da migração. Data: 2026-07-01.

## Fontes legadas auditadas
- `Epros.ERP.Domain/Entities/Fiscais`
- `Epros.ERP.Domain/Entities/Tributarios`
- `Epros.ERP.Domain/Entities/Configuracoes`
- `Epros.ERP.Domain/Entities/Cadastros/Servicos`

## Destino
- `EprosERP/src/Modules/Epros.Modules.Fiscal` (schema: plataforma, ContextFiscal)

## Convenções
- Campos herdados no legado de `Entity` / `EntityNoTenat` (`Id`, `DataCadastro`, `DataAlteracao`, `Deletado`, `TenantId`, etc.) e `SequenciaTenantId` (`ISequenciaTenant`) são considerados **COBERTOS** pela herança de `EntidadeSaaSBase` (Id/TenantId/SyncId/auditoria).
- FKs `long` do legado passam a `Guid` no novo modelo (mudança de tipo esperada, não é ausência).
- Propriedades de navegação EF que apontam para outros módulos (`Produto`, `Empresa`, `UnidadeMedidaComercial`) não são portadas como navegação intra-módulo; a referência passa a ser feita por `Guid` (FK) — considerado COBERTO quando a FK correspondente existe.

---

## Tabela De -> Para

| Entidade legada | Campo legado | Destino novo (entidade.campo) ou AUSENTE |
|---|---|---|
| **Ncm** | CodigoNcm | Ncm.CodigoNcm |
| Ncm | Descricao | Ncm.Descricao |
| Ncm | DataInicio | Ncm.DataInicio |
| Ncm | DataFim | Ncm.DataFim |
| Ncm | TipoAtoIni | Ncm.TipoAtoIni |
| Ncm | NumeroAtoIni | Ncm.NumeroAtoIni |
| Ncm | AnoAtoIni | Ncm.AnoAtoIni |
| Ncm | Produtos (nav) | N/A (nav cross-módulo, referência por FK no Produto) |
| Ncm | NcmConfiguracao (nav) | Coberto via NcmConfiguracao.NcmId |
| **NcmTributacao** | SequenciaTenantId | Coberto (EntidadeSaaSBase) |
| NcmTributacao | TributarioGrupoId | NcmTributacao.TributarioGrupoId (long->Guid) |
| NcmTributacao | CodigoBeneficioFiscalId | NcmTributacao.CodigoBeneficioFiscalId |
| NcmTributacao | CodRegra | NcmTributacao.CodRegra |
| NcmTributacao | Descricao | NcmTributacao.Descricao |
| NcmTributacao | CfopNotaConsumidor | NcmTributacao.CfopNotaConsumidor |
| NcmTributacao | CfopNotaFiscal | NcmTributacao.CfopNotaFiscal |
| NcmTributacao | CfopNotaFiscalInterestadual | NcmTributacao.CfopNotaFiscalInterestadual |
| NcmTributacao | Origem | NcmTributacao.Origem |
| NcmTributacao | CsosnNotaConsumidor | NcmTributacao.CsosnNotaConsumidor |
| NcmTributacao | CstIcmsNotaConsumidor | NcmTributacao.CstIcmsNotaConsumidor |
| NcmTributacao | CsosnNotaFiscal | NcmTributacao.CsosnNotaFiscal |
| NcmTributacao | CstIcmsNotaFiscalInterna | NcmTributacao.CstIcmsNotaFiscalInterna |
| NcmTributacao | CstIcmsNotaFiscalInterstadual | NcmTributacao.CstIcmsNotaFiscalInterstadual |
| NcmTributacao | CstPis | NcmTributacao.CstPis |
| NcmTributacao | CstCofins | NcmTributacao.CstCofins |
| NcmTributacao | ValorUnitFixoPis | NcmTributacao.ValorUnitFixoPis |
| NcmTributacao | ValorUnitFixoCofins | NcmTributacao.ValorUnitFixoCofins |
| NcmTributacao | ValorAliquotaPis | NcmTributacao.ValorAliquotaPis |
| NcmTributacao | ValorAliquotaCofins | NcmTributacao.ValorAliquotaCofins |
| NcmTributacao | CstPisCofinsEntrada | NcmTributacao.CstPisCofinsEntrada |
| NcmTributacao | CstIpiSaida | NcmTributacao.CstIpiSaida |
| NcmTributacao | CstIpiEntrada | NcmTributacao.CstIpiEntrada |
| NcmTributacao | ValorAliquotaIpi | NcmTributacao.ValorAliquotaIpi |
| NcmTributacao | ValorPercentualReducacaoBcIpi | NcmTributacao.ValorPercentualReducacaoBcIpi |
| NcmTributacao | TipoReducaoIpi | NcmTributacao.TipoReducaoIpi |
| NcmTributacao | DestinoReducaoIpi | NcmTributacao.DestinoReducaoIpi |
| NcmTributacao | IpiEmbutido | NcmTributacao.IpiEmbutido |
| NcmTributacao | EnquadramentoIpi | NcmTributacao.EnquadramentoIpi |
| NcmTributacao | CodigoValorFiscalIcmsInterna | NcmTributacao.CodigoValorFiscalIcmsInterna |
| NcmTributacao | CodigoValorFiscalcmsInterstadual | NcmTributacao.CodigoValorFiscalcmsInterstadual |
| NcmTributacao | ValorAliquotaIcmsInterna | NcmTributacao.ValorAliquotaIcmsInterna |
| NcmTributacao | ValorPercentualReducacaoBcIcmsInterna | NcmTributacao.ValorPercentualReducacaoBcIcmsInterna |
| NcmTributacao | TipoReducaoIcmsInterna | NcmTributacao.TipoReducaoIcmsInterna |
| NcmTributacao | DestinoReducaoIcmsInterna | NcmTributacao.DestinoReducaoIcmsInterna |
| NcmTributacao | ValorAliquotaIcmsInterstadual | NcmTributacao.ValorAliquotaIcmsInterstadual |
| NcmTributacao | ValorPercentualReducacaoBcIcmsInterstadual | NcmTributacao.ValorPercentualReducacaoBcIcmsInterstadual |
| NcmTributacao | TipoReducaoIcmsInterstadual | NcmTributacao.TipoReducaoIcmsInterstadual |
| NcmTributacao | DestinoReducaoIcmsInterstadual | NcmTributacao.DestinoReducaoIcmsInterstadual |
| NcmTributacao | CodigoBeneficioFiscalIcms | NcmTributacao.CodigoBeneficioFiscalIcms |
| NcmTributacao | MotivoDesoneracaoIcms | NcmTributacao.MotivoDesoneracaoIcms |
| NcmTributacao | InformacoesComplementares | NcmTributacao.InformacoesComplementares |
| NcmTributacao | InformacoesAdicionaisAoFisco | NcmTributacao.InformacoesAdicionaisAoFisco |
| NcmTributacao | CstIbsCbsNfe | NcmTributacao.CstIbsCbsNfe |
| NcmTributacao | CClassTribNfe | NcmTributacao.CClassTribNfe |
| NcmTributacao | CstIbsCbsNfce | NcmTributacao.CstIbsCbsNfce |
| NcmTributacao | CClassTribNfce | NcmTributacao.CClassTribNfce |
| NcmTributacao | Empresas (nav) | AUSENTE (nav many-to-many NcmTributacao<->Empresa não portada; sem tabela de junção correspondente) |
| **NcmTributacaoSt** | NcmTributacaoId | NcmTributacaoSt.NcmTributacaoId |
| NcmTributacaoSt | Uf | NcmTributacaoSt.Uf |
| NcmTributacaoSt | TipoCalculo | NcmTributacaoSt.TipoCalculo |
| NcmTributacaoSt | ValorAliquotaIcmsSt | NcmTributacaoSt.ValorAliquotaIcmsSt |
| NcmTributacaoSt | ValorMva | NcmTributacaoSt.ValorMva |
| NcmTributacaoSt | ValorPercentualReducaoBcIcmsSt | NcmTributacaoSt.ValorPercentualReducaoBcIcmsSt |
| NcmTributacaoSt | TipoReducaoIcmsSt | NcmTributacaoSt.TipoReducaoIcmsSt |
| NcmTributacaoSt | ValorUnitarioSt | NcmTributacaoSt.ValorUnitarioSt |
| NcmTributacaoSt | ValorPercentualFcpSt | NcmTributacaoSt.ValorPercentualFcpSt |
| **NcmTributacaoFundoCombatePobreza** | NcmTributacaoId | NcmTributacaoFundoCombatePobreza.NcmTributacaoId |
| NcmTributacaoFundoCombatePobreza | Uf | NcmTributacaoFundoCombatePobreza.Uf |
| NcmTributacaoFundoCombatePobreza | ValorPercentual | NcmTributacaoFundoCombatePobreza.ValorPercentual |
| **NcmConfiguracao** | SequenciaTenantId | Coberto (EntidadeSaaSBase) |
| NcmConfiguracao | NcmId | NcmConfiguracao.NcmId |
| NcmConfiguracao | NcmTributacaoId | NcmConfiguracao.NcmTributacaoId |
| **Cest** | Id | Coberto (EntidadeSaaSBase) |
| Cest | Codigo | Cest.Codigo |
| Cest | Descricao | Cest.Descricao |
| Cest | Produtos (nav) | N/A (nav cross-módulo) |
| **CstIbsCbs** | Id | Coberto (EntidadeSaaSBase) |
| CstIbsCbs | Cst | CstIbsCbs.Cst |
| CstIbsCbs | Descricao | CstIbsCbs.Descricao |
| CstIbsCbs | DataInicioVigencia | CstIbsCbs.DataInicioVigencia |
| CstIbsCbs | DataFimVigencia | CstIbsCbs.DataFimVigencia |
| CstIbsCbs | DataCadastro | Coberto (auditoria EntidadeSaaSBase) |
| CstIbsCbs | ClassesTributarias (nav) | CstIbsCbs.ClassesTributarias |
| **ClassificacaoTributaria** | Id | Coberto (EntidadeSaaSBase) |
| ClassificacaoTributaria | CstIbsCbsId | ClassificacaoTributaria.CstIbsCbsId |
| ClassificacaoTributaria | Codigo | ClassificacaoTributaria.Codigo |
| ClassificacaoTributaria | Descricao | ClassificacaoTributaria.Descricao |
| ClassificacaoTributaria | DataInicioVigencia | ClassificacaoTributaria.DataInicioVigencia |
| ClassificacaoTributaria | DataFimVigencia | ClassificacaoTributaria.DataFimVigencia |
| ClassificacaoTributaria | IndNfe | ClassificacaoTributaria.IndNfe |
| ClassificacaoTributaria | IndNfce | ClassificacaoTributaria.IndNfce |
| ClassificacaoTributaria | IndCte | ClassificacaoTributaria.IndCte |
| ClassificacaoTributaria | IndCteos | ClassificacaoTributaria.IndCteos |
| ClassificacaoTributaria | IndNfse | ClassificacaoTributaria.IndNfse |
| ClassificacaoTributaria | IndTribRegular | ClassificacaoTributaria.IndTribRegular |
| ClassificacaoTributaria | Anexos (nav) | ClassificacaoTributaria.Anexos |
| **ClassificacaoTributariaAnexo** | Id | Coberto (EntidadeSaaSBase) |
| ClassificacaoTributariaAnexo | ClassificacaoTributariaId | ClassificacaoTributariaAnexo.ClassificacaoTributariaId |
| ClassificacaoTributariaAnexo | NroAnexo | ClassificacaoTributariaAnexo.NroAnexo |
| ClassificacaoTributariaAnexo | Codigo | ClassificacaoTributariaAnexo.Codigo |
| ClassificacaoTributariaAnexo | DataInicioVigencia | ClassificacaoTributariaAnexo.DataInicioVigencia |
| ClassificacaoTributariaAnexo | DataFimVigencia | ClassificacaoTributariaAnexo.DataFimVigencia |
| **CodigoAnp** | Id | Coberto (EntidadeSaaSBase) |
| CodigoAnp | Codigo | CodigoAnp.Codigo |
| CodigoAnp | Descricao | CodigoAnp.Descricao |
| CodigoAnp | DataInicioVigencia | CodigoAnp.DataInicioVigencia |
| CodigoAnp | DataFinalVigencia | CodigoAnp.DataFinalVigencia |
| CodigoAnp | Produtos (nav) | N/A (nav cross-módulo) |
| **EnquadramentoIpi** | Id | Coberto (EntidadeSaaSBase; int->Guid) |
| EnquadramentoIpi | Codigo | EnquadramentoIpi.Codigo |
| EnquadramentoIpi | Descricao | EnquadramentoIpi.Descricao |
| EnquadramentoIpi | TipoOperacao | EnquadramentoIpi.TipoOperacao |
| **FcpAliquotaUf** | Uf | FcpAliquotaUf.Uf |
| FcpAliquotaUf | ValorAliquota | FcpAliquotaUf.ValorAliquota |
| FcpAliquotaUf | Observacao | FcpAliquotaUf.Observacao |
| **IcmsAliquotaInterestadual** | Id | Coberto (EntidadeSaaSBase) |
| IcmsAliquotaInterestadual | UfOrigem | IcmsAliquotaInterestadual.UfOrigem |
| IcmsAliquotaInterestadual | UfDestino | IcmsAliquotaInterestadual.UfDestino |
| IcmsAliquotaInterestadual | ValorAliquota | IcmsAliquotaInterestadual.ValorAliquota |
| **IeSt** | EmpresaId | IeSt.EmpresaId (long->Guid) |
| IeSt | Uf | IeSt.Uf |
| IeSt | Ie | IeSt.Ie |
| **TributarioGrupo** | SequenciaTenantId | Coberto (EntidadeSaaSBase) |
| TributarioGrupo | Descricao | TributarioGrupo.Descricao |
| TributarioGrupo | NcmTributacoes (nav) | Coberto via NcmTributacao.TributarioGrupoId |
| TributarioGrupo | Empresas (nav) | AUSENTE (nav many-to-many TributarioGrupo<->Empresa não portada) |
| **Cfop** | CfopCodigo | Cfop.CfopCodigo |
| Cfop | Descricao | Cfop.Descricao |
| Cfop | NaturezaOperacao | Cfop.NaturezaOperacao |
| Cfop | CfopCorrelacao | Cfop.CfopCorrelacao |
| Cfop | IntegraFaturamento | Cfop.IntegraFaturamento |
| Cfop | IndicadorNfe | Cfop.IndicadorNfe |
| Cfop | IndicadorComunicacao | Cfop.IndicadorComunicacao |
| Cfop | IndicadorTransporte | Cfop.IndicadorTransporte |
| Cfop | IndicadorDevolucao | Cfop.IndicadorDevolucao |
| Cfop | IndicadorRetorno | Cfop.IndicadorRetorno |
| Cfop | IndicadorAnulacao | Cfop.IndicadorAnulacao |
| Cfop | IndicadorRemessa | Cfop.IndicadorRemessa |
| Cfop | IndicadorCombustivel | Cfop.IndicadorCombustivel |
| Cfop | IndicadorTransferencia | Cfop.IndicadorTransferencia |
| Cfop | IndicadorNfce | Cfop.IndicadorNfce |
| Cfop | IndicadorCiap | Cfop.IndicadorCiap |
| Cfop | IndicadorUsoConsumo | Cfop.IndicadorUsoConsumo |
| Cfop | IndicadorUsoSemOperacao | Cfop.IndicadorUsoSemOperacao |
| Cfop | IndicadorSt | Cfop.IndicadorSt |
| Cfop | IndicadorMei | Cfop.IndicadorMei |
| Cfop | IncidenciaSimples | Cfop.IncidenciaSimples |
| Cfop | CfopDevolucao | Cfop.CfopDevolucao |
| **CfopPadrao** | CfopCodigo | CfopPadrao.CfopCodigo |
| CfopPadrao | DataInicioVigencia | CfopPadrao.DataInicioVigencia |
| CfopPadrao | DataFimVigencia | CfopPadrao.DataFimVigencia |
| CfopPadrao | Descricao | CfopPadrao.Descricao |
| CfopPadrao | NaturezaOperacao | CfopPadrao.NaturezaOperacao |
| CfopPadrao | CfopCorrelacao | CfopPadrao.CfopCorrelacao |
| CfopPadrao | IntegraFaturamento..IndicadorMei (18 flags) | CfopPadrao.* (todos presentes) |
| CfopPadrao | IncidenciaSimples | CfopPadrao.IncidenciaSimples |
| CfopPadrao | CfopDevolucao | CfopPadrao.CfopDevolucao |
| **CodigoBeneficioFiscal** | SequenciaTenantId | Coberto (EntidadeSaaSBase) |
| CodigoBeneficioFiscal | Codigo | CodigoBeneficioFiscal.Codigo |
| CodigoBeneficioFiscal | Descricao | CodigoBeneficioFiscal.Descricao |
| CodigoBeneficioFiscal | Uf | CodigoBeneficioFiscal.Uf |
| CodigoBeneficioFiscal | Csosns (nav) | CodigoBeneficioFiscal.Csosns |
| CodigoBeneficioFiscal | Csts (nav) | CodigoBeneficioFiscal.Csts |
| CodigoBeneficioFiscal | NcmTributacao (nav) | Coberto via NcmTributacao.CodigoBeneficioFiscalId |
| **CodigoBeneficioFiscalCsosn** | SequenciaTenantId | Coberto (EntidadeSaaSBase) |
| CodigoBeneficioFiscalCsosn | CodigoBeneficioFiscalId | CodigoBeneficioFiscalCsosn.CodigoBeneficioFiscalId |
| CodigoBeneficioFiscalCsosn | Csosn | CodigoBeneficioFiscalCsosn.Csosn |
| **CodigoBeneficioFiscalCst** | SequenciaTenantId | Coberto (EntidadeSaaSBase) |
| CodigoBeneficioFiscalCst | CodigoBeneficioFiscalId | CodigoBeneficioFiscalCst.CodigoBeneficioFiscalId |
| CodigoBeneficioFiscalCst | Cst | CodigoBeneficioFiscalCst.Cst |
| **ObservacaoNfe** | SequenciaTenantId | Coberto (EntidadeSaaSBase) |
| ObservacaoNfe | Descricao | ObservacaoNfe.Descricao |
| **TipoOperacaoFiscal** | SequenciaTenantId | Coberto (EntidadeSaaSBase) |
| TipoOperacaoFiscal | TributarioGrupoId | TipoOperacaoFiscal.TributarioGrupoId |
| TipoOperacaoFiscal | CfopNfeId | TipoOperacaoFiscal.CfopNfeId |
| TipoOperacaoFiscal | CfopNfceId | TipoOperacaoFiscal.CfopNfceId |
| TipoOperacaoFiscal | Descricao | TipoOperacaoFiscal.Descricao |
| TipoOperacaoFiscal | SobescreveTributacaoNcm | TipoOperacaoFiscal.SobescreveTributacaoNcm |
| TipoOperacaoFiscal | Finalidade | TipoOperacaoFiscal.Finalidade |
| TipoOperacaoFiscal | Atendimento | TipoOperacaoFiscal.Atendimento |
| TipoOperacaoFiscal | TipoFrete | TipoOperacaoFiscal.TipoFrete |
| TipoOperacaoFiscal | TipoMovimento | TipoOperacaoFiscal.TipoMovimento |
| **ConfiguracaoDFe** | (classe inteira comentada no legado) | N/A (não é entidade ativa no legado) |
| **ConfiguracaoImpressaoNfce** | SequenciaTenantId | Coberto (EntidadeSaaSBase) |
| ConfiguracaoImpressaoNfce | EmpresaId | ConfiguracaoImpressaoNfce.EmpresaId (long->Guid) |
| ConfiguracaoImpressaoNfce | DetalheVendaNormal | ConfiguracaoImpressaoNfce.DetalheVendaNormal |
| ConfiguracaoImpressaoNfce | DetalheVendaContingencia | ConfiguracaoImpressaoNfce.DetalheVendaContingencia |
| ConfiguracaoImpressaoNfce | ImprimeDescontoItem | ConfiguracaoImpressaoNfce.ImprimeDescontoItem |
| ConfiguracaoImpressaoNfce | ImprimeFoneEmitente | ConfiguracaoImpressaoNfce.ImprimeFoneEmitente |
| ConfiguracaoImpressaoNfce | MargemEsquerda | ConfiguracaoImpressaoNfce.MargemEsquerda |
| ConfiguracaoImpressaoNfce | MargemDireita | ConfiguracaoImpressaoNfce.MargemDireita |
| ConfiguracaoImpressaoNfce | ModoImpressao | ConfiguracaoImpressaoNfce.ModoImpressao |
| ConfiguracaoImpressaoNfce | NfceLayoutQrCode | ConfiguracaoImpressaoNfce.NfceLayoutQrCode |
| ConfiguracaoImpressaoNfce | VersaoQrCode | ConfiguracaoImpressaoNfce.VersaoQrCode |
| ConfiguracaoImpressaoNfce | SegundaViaContingencia | ConfiguracaoImpressaoNfce.SegundaViaContingencia |
| **CodigoServicoSefaz** | Codigo | CodigoServicoSefaz.Codigo |
| CodigoServicoSefaz | Descricao | CodigoServicoSefaz.Descricao |
| CodigoServicoSefaz | Servico (nav) | Coberto via Servico.CodigoServicoSefazId |
| **Servico** | EmpresaId | **AUSENTE** (campo crítico: escopo de empresa perdido) |
| Servico | UnidadeMedidaId | Servico.UnidadeMedidaId (long->Guid) |
| Servico | CodigoServicoSefazId | Servico.CodigoServicoSefazId |
| Servico | SequenciaTenantId | Coberto (EntidadeSaaSBase) |
| Servico | Codigo | Servico.Codigo |
| Servico | Descricao | Servico.Descricao |
| Servico | Valor | Servico.Valor |
| Servico | InformacaoAdicional | Servico.InformacaoAdicional |
| Servico | ServicoAtivo | Servico.ServicoAtivo |
| Servico | Cnae | Servico.Cnae |
| Servico | CodigoNbs | Servico.CodigoNbs |
| Servico | IndicadorIss | Servico.IndicadorIss |
| Servico | IndicadorIncentivo | Servico.IndicadorIncentivo |
| Servico | CstIbsCbs | Servico.CstIbsCbs |
| Servico | CClassTrib | Servico.CClassTrib |
| Servico | AliquotaIss | Servico.AliquotaIss |
| Servico | AliquotaIssRetido | Servico.AliquotaIssRetido |
| Servico | AliquotaIrrfRetido | Servico.AliquotaIrrfRetido |
| Servico | AliquotaInss | Servico.AliquotaInss |
| Servico | CstPisCofins | Servico.CstPisCofins |
| Servico | AliquotaPis | Servico.AliquotaPis |
| Servico | AliquotaCofins | Servico.AliquotaCofins |
| Servico | CalcularRetencao | Servico.CalcularRetencao |
| Servico | AnexoSimplesNacional | Servico.AnexoSimplesNacional |

---

## Entidades ausentes
- **Nenhuma entidade de negócio ativa está totalmente ausente.** Todas as 23 entidades legadas ativas foram portadas.
- `ConfiguracaoDFe`: classe totalmente comentada no legado (não é entidade ativa) — não requer porte.

## Campos críticos faltando
1. **Servico.EmpresaId — AUSENTE.** O legado escopava o serviço por empresa (além do tenant) e validava `EmpresaId > 0`. No novo `Servico` esse campo foi removido; o serviço passa a ser escopado apenas por `TenantId`. Impacto: perda de segregação por empresa dentro do mesmo tenant e potencial quebra de migração de dados existentes.
2. **NcmTributacao.Empresas (relação N:N) — AUSENTE.** A associação many-to-many `NcmTributacao <-> Empresa` do legado não foi portada. Impacto: se a vinculação de tributação a empresas específicas era usada, ela é perdida.
3. **TributarioGrupo.Empresas (relação N:N) — AUSENTE.** A associação many-to-many `TributarioGrupo <-> Empresa` não foi portada. Impacto: perda do vínculo grupo tributário <-> empresas.

## Observações
- Mudanças de tipo de chave `long -> Guid` e a substituição de navegações cross-módulo por FKs `Guid` são esperadas na arquitetura modular SaaS e não constituem ausência de campo.
- Campos de identidade/auditoria/`SequenciaTenantId` estão cobertos por `EntidadeSaaSBase`.
