# 01 — Auditoria de Entidades de Domínio (Legado → Novo)

**Tipo:** Auditoria cética, read-only, nível de linha/campo.
**Data:** 2026-07-05.
**Legado (fonte da verdade):** `Epros/epros_erp-main/src/Epros.ERP.Domain/Entities/` (.NET 8 / SQL Server, `Entity` base, `long` Id).
**Novo:** `EprosERP/src/Modules/Epros.Modules.*/Domain/Entities/` + `Shared/Epros.Shared/Domain/Entities/` (.NET 8 modular CQRS/EF/Postgres, `EntidadeSaaSBase`, `Guid` Id).

Método: para cada arquivo `.cs` de entidade do legado, localizei a classe equivalente no novo (pode estar em outro módulo ou renomeada), abri ambos e comparei o **conjunto de propriedades públicas** (nome, tipo, nullability, enum, navegações/coleções, value objects). Contagens de propriedades foram verificadas por `grep` linha a linha; diffs por nome (`comm`) nos casos de maior valor. Lixo `._*` do macOS foi ignorado.

---

## 0. Achado sistêmico (afeta TODAS as entidades)

A classe-base mudou e isso não é cosmético — é o eixo de toda a migração:

| Aspecto | Legado `Entity` (`Shared/DomainObjects/Entity.cs`) | Novo `EntidadeSaaSBase` (`Shared/Epros.Shared/Domain/Entities/EntidadeSaaSBase.cs`) |
|---|---|---|
| Id | `long Id` (identity/sequencial) | `Guid Id` |
| Auditoria | `DataCadastro`, `DataAlteracao?`, `Deletado?` (DateTime) | `CriadoEm`, `AlteradoEm?`, `DeletadoEm?` + `CriadoPor?`, `AlteradoPor?` |
| Multi-tenant | `TenantId` | `TenantId` + `SyncId`, `SyncVersion` (sincronização/offline — **novos**) |
| Soft delete | `Deletado != null` | `DeletadoEm != null` |

**Consequências verificáveis e recorrentes em quase todas as entidades:**
1. **Todas as PK e FK migram de `long`/`long?` para `Guid`/`Guid?`.** Isso exige mapeamento de dados no cutover (não é lossless automático) e afeta 100% dos relacionamentos.
2. **`SequenciaTenantId` (long) → `SequenciaExibicao` (long?)** — renomeado e agora anulável em praticamente toda entidade que o tinha.
3. **Value Objects achatados:** `CNPJ`, `CPF`, `Documento`, `CEP`, e enums `EEstado`(UF) do legado viram `string`/`string?` no novo (perde validação embutida no VO; dado preservado).
4. **Navegações cross-module removidas:** onde o legado tinha `ICollection<Empresa>`/`Pessoa`/`Venda`/`Compra` direto, o novo mantém só a FK `Guid` (ou uma entidade de junção nova). Não é perda de dado — é fronteira de módulo.
5. **Enums fiscais enfraquecidos para `int?` em alguns agregados-raiz** (ver Venda/Compra abaixo) — este é um gap real de tipagem, não só de estilo.

> Nota de método: onde os relatórios por módulo marcaram "PARCIAL" **apenas** por `long→Guid`, VO→string, ou nav cross-module removida, reclassifiquei como **PRESENTE** (dado preservado). "PARCIAL" abaixo é reservado a divergência que altera semântica (enum→int, campo realmente ausente, ciclo de vida novo). Isso é mais cético que os sub-relatórios, não menos.

---

## 1. Módulo VENDAS  (legado `Entities/Vendas/` → novo `Epros.Modules.Vendas`)

35 arquivos no legado, 35 no novo (0 arquivo ausente). Detalhe field-level nos de maior valor:

| Entidade legada | Entidade nova | Status | Campos faltando / divergentes (reais) | Severidade | Arquivo novo |
|---|---|---|---|---|---|
| Venda | Venda | PARCIAL | `ModeloFiscal` enum `EModeloDocumento`→`int?`; `ModalidadeFrete` enum `EModalidadeFrete`→`int?`; `VendaOrigem` enum `EVendaOrigem`→`int?` (perda de tipagem). Novos campos no novo (não no legado): `CaixaId`, `Total`, `Cancelada`, `ClienteId`, `ValorDesconto`, `ValorFrete`, `FormaPagamento`, `DocumentoFiscalId`. Agregados 1:1 do legado agora todos `nullable`. | **Alta** (enum→int) | `Domain/Entities/Venda.cs` |
| VendaItem | VendaItem | PRESENTE* | FK `long→Guid`; navegações Imposto/Combustivel nullable; campos denormalizados novos (`Quantidade`,`PrecoUnitario`,`ValorTotal`). Sem perda. | Baixa | `Domain/Entities/VendaItem.cs` |
| VendaItemImposto | VendaItemImposto | PRESENTE | 70 props verificadas por grep em ambos (70=70). ICMS/ST/FCP/IPI/PIS/COFINS/DIFAL íntegros. | Baixa | `Domain/Entities/VendaItemImposto.cs` |
| VendaItemImpostoIbsCbs | VendaItemImpostoIbsCbs | PRESENTE | 21=21 props (grep). Reforma tributária íntegra. | Baixa | `Domain/Entities/VendaItemImpostoIbsCbs.cs` |
| VendaItemImpostoIbsCbsTributacaoRegular | idem | PRESENTE | Presente (arquivo existe; usado por VendaItem). | Baixa | `Domain/Entities/VendaItemImpostoIbsCbsTributacaoRegular.cs` |
| VendaItemImpostoValorAproximado | idem | PRESENTE | 6 props preservadas. | Baixa | `.../VendaItemImpostoValorAproximado.cs` |
| VendaTotal | VendaTotal | PRESENTE | 20 props preservadas. | Baixa | `.../VendaTotal.cs` |
| VendaTotalIbsCbs | VendaTotalIbsCbs | PRESENTE | 5 props preservadas. | Baixa | `.../VendaTotalIbsCbs.cs` |
| VendaConfiguracao | VendaConfiguracao | PRESENTE | 8 props preservadas. | Baixa | `.../VendaConfiguracao.cs` |
| VendaPagamento | VendaPagamento | PRESENTE* | `CartaoCnpjIntermediadorFinanceira` VO `CNPJ`→`string?`. | Baixa | `.../VendaPagamento.cs` |
| VendaEmitente | VendaEmitente | PRESENTE* | Cnpj/Cpf VO→string?; Endereco nullable. | Baixa | `.../VendaEmitente.cs` |
| VendaDestinatario | VendaDestinatario | PRESENTE* | Cnpj/Cpf/DocumentoConsumidor VO→string?. | Baixa | `.../VendaDestinatario.cs` |
| VendaNfe | VendaNfe | PRESENTE* | `DataHoraEmissao` default `Now`→`UtcNow`; coleções→IReadOnly. | Baixa | `.../VendaNfe.cs` |
| VendaNfce | VendaNfce | PRESENTE* | `DataHoraEmissao` default `Now`→`UtcNow`. | Baixa | `.../VendaNfce.cs` |
| VendaTransporte (+Reboque/Transportadora/Veiculo/Volume) | idem | PRESENTE* | Coleções→IReadOnly; back-nav droppado. | Baixa | `.../VendaTransporte*.cs` |
| VendaFatura (+Duplicata) | idem | PRESENTE* | Coleção→IReadOnly. | Baixa | `.../VendaFatura*.cs` |
| VendaEntrega, VendaCobrancaEndereco, VendaImposto, VendaAutorizacaoXml, VendaNfeReferenciada, VendaNfeExportacao, VendaNfeHistorico, VendaNfHistorico, VendaNfeIntermediador, VendaNfeCartaCorrecao, VendaEmitenteEndereco, VendaDestinatarioEndereco | idem | PRESENTE* | Apenas long→Guid / VO→string. | Baixa | `.../Venda*.cs` |

`*` = PRESENTE com padrão sistêmico (§0), sem perda de dado.

**Vendas: 34 PRESENTE, 1 PARCIAL (Venda — enum→int), 0 AUSENTE / 35.** Fiscal/IBS-CBS 100% íntegro.

---

## 2. Módulo COMPRAS  (legado `Entities/Compras/` → novo `Epros.Modules.Estoque`)

34 arquivos no legado; todos com equivalente no novo módulo Estoque (0 arquivo ausente — contagens de props verificadas por grep para os 20 que o sub-relatório só amostrou; ver abaixo).

| Entidade legada | Entidade nova | Status | Campos faltando / divergentes (reais) | Severidade | Arquivo novo |
|---|---|---|---|---|---|
| Compra | Compra | PARCIAL | Agregados 1:1 do legado (Emitente, Destinatario, Transporte, Nfe, Fatura, Total, Imposto, Configuracao, Entrega, TotalIbsCbs) agora **todos nullable** — Configuracao/Entrega deixaram de ser obrigatórios. Novos campos denormalizados: `FornecedorCnpj`, `FornecedorNome`, `NumeroNota`, `ChaveAcesso`, `ValorTotal`, `DataEmissao`, `Cancelada`, `FormaPagamento`. | Média | `Domain/Entities/Compra.cs` |
| CompraItem | CompraItem | PRESENTE* | long→Guid; navegações nullable; campos denormalizados novos (`Quantidade`,`PrecoUnitario`,`ValorIms`,`ValorIpi`,`ValorTotal`,`ValorCusto`). Sem perda. | Baixa | `.../CompraItem.cs` |
| CompraItemImposto | CompraItemImposto | PRESENTE | ~70 props (ICMS/ST/FCP/IPI/PIS/COFINS/DIFAL) preservadas. | Baixa | `.../CompraItemImposto.cs` |
| CompraItemImpostoIbsCbs | idem | PRESENTE | Props IBS/CBS preservadas. | Baixa | `.../CompraItemImpostoIbsCbs.cs` |
| CompraTotal | CompraTotal | PRESENTE | 20 props preservadas. | Baixa | `.../CompraTotal.cs` |
| CompraTotalIbsCbs | idem | PRESENTE | 5 props preservadas. | Baixa | `.../CompraTotalIbsCbs.cs` |
| CompraNfe (+CartaCorrecao/Historico/Intermediador/Referenciada) | idem | PRESENTE | 13 props na raiz; filhos verificados por contagem. | Baixa | `.../CompraNfe*.cs` |
| CompraItemImportacao (+Adicao) | idem | PRESENTE* | long→Guid; VO→string; coleção Adicoes preservada (6 props). | Baixa | `.../CompraItemImportacao*.cs` |
| CompraEmitente (+Endereco) | idem | PRESENTE* | long→Guid; VO→string; Endereco nullable. | Baixa | `.../CompraEmitente*.cs` |
| CompraDestinatario (+Endereco) | idem | PRESENTE | 13=13 props (grep). | Baixa | `.../CompraDestinatario*.cs` |
| CompraFatura (+Duplicata), CompraConfiguracao, CompraTransporte(+Reboque/Transportadora/Veiculo/Volume), CompraPagamento, CompraAutorizacaoXml, CompraCobrancaEndereco, CompraEntrega, CompraImposto, CompraItemCombustivel(+Origem), CompraItemImpostoValorAproximado | idem | PRESENTE | Contagens verificadas 1:1 (diffs de ±1 = FK/back-nav droppada). | Baixa | `.../Compra*.cs` |

**Compras: 33 PRESENTE, 1 PARCIAL (Compra — agregados obrigatórios viraram opcionais), 0 AUSENTE / 34.** Fiscal 100% íntegro.

---

## 3. Módulo CADASTROS  (legado `Entities/Cadastros/` → novo, espalhado)

41 arquivos legado, distribuídos em Estoque (Produtos), GestaoClientes (Pessoas/Empresas/Enderecos), Financeiro (Bancos), Fiscal (Servicos/Contador via Contabeis).

| Entidade legada | Entidade nova | Status | Campos faltando / divergentes (reais) | Severidade | Arquivo novo |
|---|---|---|---|---|---|
| Banco, CartaoDeCredito, CartaoDeCreditoFatura, ContaBancaria | idem (Financeiro) | PRESENTE* | Só long→Guid. | Baixa | `Epros.Modules.Financeiro/.../*.cs` |
| CertificadoDigital | **EmpresaCertificado** | PRESENTE (renomeado) | Renomeado; movido p/ GestaoClientes. | Média | `GestaoClientes/.../EmpresaCertificado.cs` |
| Empresa | Empresa | PARCIAL | Novos: `EhMei`, `DateFormat`, `TimeZoneId`, `CurrencyId`, `Ativo`. `SequenciaTenantId`→`SequenciaExibicao`. FKs mantidas (NcmId, PlanoContasFinanceiroId, TributarioGrupoId). | Média | `GestaoClientes/.../Empresa.cs` |
| EmpresaContato | EmpresaContato | PRESENTE* | long→Guid. | Baixa | `.../EmpresaContato.cs` |
| **EmpresaEndereco** | (dissolvida) | **AUSENTE** | Tabela de junção Pessoa/Empresa/Endereco removida; `Endereco` no novo referencia `PessoaId`/`EmpresaId?` direto. Verificar cobertura do vínculo empresa↔endereço no cutover. | **Média** | — |
| EmpresaParametrosDfe | EmpresaParametrosDfe | PRESENTE | 4 arquivos legado (Nfe/NfceHom/NfceProd) consolidados como **owned types** (ParametrosDfeNfe, ParametrosDfeNfceHomologacao, ParametrosDfeNfceProducao) dentro de EmpresaParametrosDfe. Props portadas 1:1. | Baixa | `GestaoClientes/.../EmpresaParametrosDfe.cs` |
| EmpresaParametrosDfeNfe / NfceHomologacao / NfceProducao | owned types acima | PRESENTE | Ver linha anterior. | Baixa | (embutido) |
| Endereco | Endereco | PARCIAL | CEP VO→string; UF enum→string. Novos: `SubdivisaoId`, `CodigoPostalInternacional`, `LinhaEndereco1/2`, `Latitude`, `Longitude`, `ContadorId`, `EmpresaId`. Absorve o papel de PessoaEndereco/EmpresaEndereco. | Média | `GestaoClientes/.../Endereco.cs` |
| Municipio, Pais | idem | PRESENTE* | long→Guid. | Baixa | `GestaoClientes/.../*.cs` |
| Pessoa | Pessoa | PARCIAL | Novo enum `Status` (`EEstadoPessoa`) + ciclo de vida (Submeter/Aprovar/Rejeitar/Inativar/Bloquear/Reativar); `SequenciaTenantId`→`SequenciaExibicao`; coleção `Veiculos` (1:N, absorvida de PessoaMotorista). Campos legado preservados. | Média | `GestaoClientes/.../Pessoa.cs` |
| PessoaCliente, PessoaContato, PessoaEstrangeiro, PessoaFisica, PessoaFuncionario, PessoaGrupo, PessoaJuridica, PessoaPrestadorServico, PessoaTransportadora | idem | PRESENTE* | long→Guid. | Baixa | `GestaoClientes/.../Pessoa*.cs` |
| **PessoaEndereco** | (dissolvida em Endereco) | **AUSENTE** | Junção removida; `Endereco.PessoaId` direto. Dado preservável, mas o mapeamento N:N legado (uma pessoa com vários endereços por empresa) precisa ser validado. | **Média** | — |
| PessoaMotorista | PessoaMotorista | PARCIAL | `TipoCategoriaCnh?`→não-nullable (default). Coleção/métodos de gestão de veículos removidos daqui (movidos para `Pessoa.Veiculos`). | Média | `GestaoClientes/.../PessoaMotorista.cs` |
| PessoaVeiculo | PessoaVeiculo | PRESENTE* | Autônoma agora (1:N em Pessoa); `PaisId` permaneceu `long` (inconsistência — ver §9); novos `TipoVeiculo`. | Baixa | `GestaoClientes/.../PessoaVeiculo.cs` |
| Adicionais, AdicionaisProduto, Balanca, CategoriaProduto, MarcaProduto, ProdutoEspecifico(+CombustivelOrigem), ProdutoGrupo, ProdutoHistoricoReajuste, UnidadeMedidaComercial, UnidadeMedidaTributavel | idem (Estoque) | PRESENTE* | long→Guid. | Baixa | `Epros.Modules.Estoque/.../*.cs` |
| Produto | Produto | PRESENTE* | Diff por nome (`comm`): as únicas ausências são **navegações** (`Ncm`, `Cest`, `CodigoAnp` nav — as FKs `NcmId`/`CestId`/`CodigoAnpId` **existem**) e **reverse navs** (`EstoqueProduto`, `ProdutoFichaEstoqueEntradas/Saidas`, `RegistroMovimentoEstoqueManuais` — dados vivem em entidades próprias). `SequenciaTenantId`→`SequenciaExibicao`. Novos denormalizados: `Sku`,`Nome`,`PrecoVenda`,`SaldoEstoque`,`CustoMedio`. **Nenhum campo escalar perdido.** (Sub-relatório marcou Alta; rebaixado para Baixa após diff por nome.) | Baixa | `Epros.Modules.Estoque/.../Produto.cs` |
| CodigoServicoSefaz, Servico | idem (Fiscal) | PRESENTE | Servico: 23 campos fiscais (ISS/PIS/COFINS/IBS-CBS/CClassTrib) portados 1:1. | Baixa | `Epros.Modules.Fiscal/.../*.cs` |

**Cadastros: 37 PRESENTE, 2 PARCIAL(-mudança) real de peso (Pessoa ciclo de vida, PessoaMotorista veículos) + Empresa/Endereco (novos campos), 2 AUSENTE (PessoaEndereco, EmpresaEndereco — dissolvidas) / 41.**

---

## 4. Módulo FINANCEIROS  (legado `Entities/Financeiros/` → novo `Epros.Modules.Financeiro`)

| Entidade legada | Entidade nova | Status | Campos faltando / divergentes (reais) | Severidade | Arquivo novo |
|---|---|---|---|---|---|
| ContasAPagar | ContasAPagar | PRESENTE* | long→Guid; nav Pessoa→FK; `SequenciaTenantId`→`SequenciaExibicao`. Todos os valores/datas/parcelas/situação preservados. | Baixa | `.../ContasAPagar.cs` |
| ContasAPagarItem | idem | PRESENTE* | long→Guid nas FKs. | Baixa | `.../ContasAPagarItem.cs` |
| ContasAReceber | idem | PRESENTE* | Idem ContasAPagar. | Baixa | `.../ContasAReceber.cs` |
| ContasAReceberItem | idem | PRESENTE* | Idem. | Baixa | `.../ContasAReceberItem.cs` |
| FatoGeradorFinanceiro | idem | PRESENTE* | VendaId/CompraId long?→Guid?; navs Venda/Compra removidas (cross-module). | Baixa | `.../FatoGeradorFinanceiro.cs` |
| ConfiguracaoCodigoNaturezaFinanceira | idem | PRESENTE* | long→Guid; nav Empresa→FK. | Baixa | `.../ConfiguracaoCodigoNaturezaFinanceira.cs` |
| PlanoDeContasFinanceiro | idem | PRESENTE* | `ICollection<Empresa>` → junção nova `PlanoDeContasFinanceiroEmpresa`; `SequenciaTenantId`→`SequenciaExibicao`. | Baixa | `.../PlanoDeContasFinanceiro.cs` |
| PlanoDeContasFinanceiroItem | idem | PRESENTE* | long→Guid; navs de item removidas. | Baixa | `.../PlanoDeContasFinanceiroItem.cs` |
| (nova) | PlanoDeContasFinanceiroEmpresa | N/A (nova) | Junção N:N restaurando `PlanoDeContasFinanceiro↔Empresa`. | — | `.../PlanoDeContasFinanceiroEmpresa.cs` |

**Financeiros: 8 PRESENTE, 0 PARCIAL(-real), 0 AUSENTE / 8.** Valores financeiros (ValorTitulo, Situacao, vencimento/baixa, parcelas, juros/multa) íntegros. (O sub-relatório marcou tudo PARCIAL, mas exclusivamente por long→Guid/nav/assinatura — reclassificado.)

---

## 5. Módulos FISCAIS + TRIBUTARIOS  (→ `Epros.Modules.Fiscal`; `IeSt`→GestaoClientes)

| Entidade legada | Entidade nova | Status | Campos faltando / divergentes (reais) | Severidade | Arquivo novo |
|---|---|---|---|---|---|
| Cfop, CfopPadrao | idem | PRESENTE | Só base-class/SaaS. | Baixa | `Fiscal/.../Cfop*.cs` |
| CodigoBeneficioFiscal (+Csosn/+Cst) | idem | PRESENTE | `SequenciaTenantId` migrado p/ base. | Baixa | `Fiscal/.../CodigoBeneficioFiscal*.cs` |
| ObservacaoNfe, TipoOperacaoFiscal | idem | PRESENTE* | TipoOperacaoFiscal: FKs long→Guid. | Baixa | `Fiscal/.../*.cs` |
| Cest | Cest | PRESENTE* | Nav `Produtos` removida (cross-module); `IGlobalEntity`. | Baixa | `Fiscal/.../Cest.cs` |
| ClassificacaoTributaria (+Anexo) | idem | PRESENTE* | long→Guid. Simples Nacional íntegro. | Baixa | `Fiscal/.../ClassificacaoTributaria*.cs` |
| CodigoAnp, EnquadramentoIpi, FcpAliquotaUf, IcmsAliquotaInterestadual | idem | PRESENTE | Alíquotas/CST íntegros. | Baixa | `Fiscal/.../*.cs` |
| CstIbsCbs | idem | PRESENTE* | `DataCadastro` removido (migrado p/ base); reforma tributária íntegra. | Baixa | `Fiscal/.../CstIbsCbs.cs` |
| IeSt | IeSt (GestaoClientes) | PRESENTE* | EmpresaId long→Guid; relocado de módulo. | Baixa | `GestaoClientes/.../IeSt.cs` |
| Ncm | Ncm | PRESENTE* | Nav `Produtos` removida (cross-module). | Baixa | `Fiscal/.../Ncm.cs` |
| NcmConfiguracao | idem | PRESENTE* | long→Guid. | Baixa | `Fiscal/.../NcmConfiguracao.cs` |
| NcmTributacao | NcmTributacao | PRESENTE* | 53=53 props (grep). `ICollection<Empresa>` → junção nova `NcmTributacaoEmpresa`; `SequenciaTenantId`→`SequenciaExibicao`. **Todas as alíquotas/CST íntegras.** (Sub-relatório marcou Alta pela mudança N:N; dado não perdido → Baixa.) | Baixa | `Fiscal/.../NcmTributacao.cs` |
| NcmTributacaoSt | idem | PRESENTE | Alíquotas/CST ST íntegras. | Baixa | `Fiscal/.../NcmTributacaoSt.cs` |
| NcmTributacaoFundoCombatePobreza | idem | PRESENTE* | long→Guid. | Baixa | `Fiscal/.../NcmTributacaoFundoCombatePobreza.cs` |
| TributarioGrupo | idem | PRESENTE* | `ICollection<Empresa>` → junção nova `TributarioGrupoEmpresa`. | Baixa | `Fiscal/.../TributarioGrupo.cs` |
| (novas) | NcmTributacaoEmpresa, TributarioGrupoEmpresa | N/A (novas) | Junções N:N restaurando os vínculos com Empresa. | — | `Fiscal/.../*Empresa.cs` |

**Fiscais+Tributarios: 22 PRESENTE, 0 PARCIAL(-real), 0 AUSENTE / 22.** Espinha dorsal tributária (NCM/CST/alíquotas/IBS-CBS) **100% íntegra em estrutura** — lembrando que o gap conhecido é de **dados** (NCM/CEST vêm da fonte externa), não de esquema.

---

## 6. Módulos ESTOQUE (nativo), CONFIGURACOES, CONTABEIS, IMPORTACOES, PERMISSOES, USUARIOS

| Entidade legada | Entidade nova | Status | Campos faltando / divergentes (reais) | Severidade | Arquivo novo |
|---|---|---|---|---|---|
| EstoqueMovimentoManual, EstoqueProduto, FatoGeradorEstoque, ProdutoFichaEstoqueEntrada, ProdutoFichaEstoqueSaida | idem (Estoque) | PRESENTE* | long→Guid. Saldos/custos/fichas íntegros. | Baixa | `Estoque/.../*.cs` |
| ImportacaoXml, ImportacaoArquivoXmlSaida | idem (Estoque) | PRESENTE* | long→Guid. | Baixa | `Estoque/.../Importacao*.cs` |
| ImportacacaoArquivoOfx (+Transacao) | idem (Financeiro) | PRESENTE* | long→Guid; métodos ConciliarCP/CR adicionados. | Baixa | `Financeiro/.../ImportacacaoArquivoOfx*.cs` |
| ConfiguracaoDFe | idem (Fiscal) | PRESENTE* | 12 campos (série/CSC/NFe/NFCe) íntegros; nav Empresa removida. | Baixa | `Fiscal/.../ConfiguracaoDFe.cs` |
| ConfiguracaoImpressaoNfce | idem (Fiscal) | PRESENTE* | 11 props íntegras. | Baixa | `Fiscal/.../ConfiguracaoImpressaoNfce.cs` |
| Contador | Contador (Fiscal) | PARCIAL | **`EnderecoId` + nav `Endereco` removidos** — endereço achatado em 7 campos (`Logradouro`,`Numero`,`Complemento`,`Bairro`,`Cep`,`MunicipioId`,`Uf`). Nav reversa `ICollection<Empresa>` removida. CPF/CNPJ/Email VO→string. Dado do endereço preservável, mas o **vínculo por FK a `Endereco` mudou de referência para cópia** — validar no cutover. | Média | `Fiscal/.../Contador.cs` |
| Menu, MenuItemNivel1, MenuItemNivel2 | idem (GestaoClientes) | PRESENTE | +campo `Modulo`; base `EntityNoTenat`→`Notifiable`; long→Guid. | Baixa | `GestaoClientes/.../Menu*.cs` |
| PerfilUsuario | **PerfilAcesso** | PRESENTE (renomeado) | +`Ativo`, +coleção `Acessos`; nav `Usuarios` removida; métodos → Sincronizar/Ativar/Inativar. | Média | `GestaoClientes/.../PerfilAcesso.cs` |
| PerfilUsuarioAcesso | **PerfilAcessoMenu** | PRESENTE (renomeado) | PK/FK long→Guid; navs cruzadas removidas; Ver/Editar/Excluir preservados. | Baixa | `GestaoClientes/.../PerfilAcessoMenu.cs` |
| Usuario | Usuario (Aplicativo) | PARCIAL | `Senha` (hash) → `PasswordHash` (renomeado). **Muitos campos novos**: `Nome`, `Status` enum, `Tipo` enum, `Telefone`, `MfaHabilitado`, `ForcarTrocaSenha`, `AccessFailedCount`, `LockoutEnd`, `ForgotPasswordToken*`, `ApiKey*`, `ApiKeyRateLimit`. Superset do legado (nenhum campo legado perdido além do rename). | Média | `Aplicativo/.../Usuario.cs` |
| UsuarioEmpresa | idem (Aplicativo) | PRESENTE* | long→Guid; `IsAdmin`→`EhAdmin`; `PerfilUsuarioId`→`PerfilAcessoId`. | Baixa | `Aplicativo/.../UsuarioEmpresa.cs` |

**Este bloco: 19 PRESENTE, 2 PARCIAL(-real): Contador (FK Endereco→cópia) e Usuario (superset), 0 AUSENTE / 21.**

---

## 7. Totais

| Módulo | Entidades legado | PRESENTE | PARCIAL (real) | AUSENTE |
|---|---:|---:|---:|---:|
| Vendas | 35 | 34 | 1 | 0 |
| Compras | 34 | 33 | 1 | 0 |
| Cadastros | 41 | 37 | 2 | 2 |
| Financeiros | 8 | 8 | 0 | 0 |
| Fiscais + Tributarios | 22 | 22 | 0 | 0 |
| Estoque/Config/Contabeis/Import/Permissoes/Usuarios | 25 | 22 | 3 | 0 |
| **TOTAL** | **165** | **156** | **7** | **2** |

**% PRESENTE (dado preservado, incl. renomeações/owned-types): ~94,5% (156/165).**
**PARCIAL real (divergência semântica): 7. AUSENTE (entidade dissolvida): 2.**

Observação cética sobre a alegação anterior de "99,3% de campos": a nível de **arquivo/entidade** a cobertura é alta e real (só 2 dissolvidas, ambas junções de endereço). Mas há **gaps de tipagem/semântica reais** que 99,3% escondia — principalmente enums fiscais degradados para `int?` no agregado-raiz Venda (e agregados obrigatórios virando opcionais em Venda/Compra). Nenhum **campo escalar fiscal/de valor** foi perdido em Vendas, Compras, Financeiro ou Fiscal.

---

## 8. Top 10 gaps reais priorizados

1. **Venda: `ModeloFiscal`/`ModalidadeFrete`/`VendaOrigem` enum → `int?`** (`Vendas/Domain/Entities/Venda.cs:29,34,35`). Perda de tipagem forte em campo que dirige emissão fiscal. **Alta.**
2. **EmpresaEndereco — AUSENTE (dissolvida).** Junção Pessoa/Empresa/Endereco sumiu; validar cobertura de "endereço da empresa" via `Endereco.EmpresaId`. **Média-Alta.**
3. **PessoaEndereco — AUSENTE (dissolvida).** N:N pessoa↔endereço colapsado em `Endereco.PessoaId` (1:N). Validar migração de pessoas com múltiplos endereços por empresa. **Média-Alta.**
4. **Compra: agregados obrigatórios (`Configuracao`, `Entrega`) viraram opcionais/nullable** (`Estoque/Domain/Entities/Compra.cs`). Risco de gravar compra sem configuração fiscal. **Média.**
5. **Contador: `EnderecoId`+nav `Endereco` → 7 campos achatados** (`Fiscal/Domain/Entities/Contador.cs`). Vínculo por FK virou cópia; validar no cutover; `MunicipioId` agora Guid. **Média.**
6. **PessoaMotorista: gestão de veículos migrada para `Pessoa.Veiculos`; `TipoCategoriaCnh?`→não-nullable** (`GestaoClientes/.../PessoaMotorista.cs`). Comportamento de default pode reescrever nulos legados. **Média.**
7. **Pessoa: novo ciclo de vida com enum `Status`** (`GestaoClientes/.../Pessoa.cs`). Registros legados precisam de valor `Status` inicial coerente no cutover (default pode marcar tudo "Rascunho"). **Média.**
8. **Usuario: `Senha`→`PasswordHash` (rename) + superset de auth** (`Aplicativo/.../Usuario.cs`). Rename exige mapeamento na migração de credenciais; hashing precisa ser compatível. **Média.**
9. **PerfilUsuario→PerfilAcesso / PerfilUsuarioAcesso→PerfilAcessoMenu (renomes).** Scripts de migração e qualquer referência por nome precisam apontar para as novas classes/tabelas. **Média-Baixa.**
10. **`PessoaVeiculo.PaisId` permaneceu `long`** enquanto o resto migrou para `Guid` (`GestaoClientes/.../PessoaVeiculo.cs`) — inconsistência de tipo de FK; provável bug latente de relacionamento. **Baixa-Média.**

---

## 9. Notas de verificação (ceticismo aplicado)

- **Rebaixamentos vs sub-relatórios:** `Produto`, `NcmTributacao`, `TributarioGrupo`, e o bloco Financeiro foram marcados "PARCIAL/Alta" pelos agentes por causa de long→Guid, VO→string ou nav cross-module. Diff por nome (`comm`) e contagem (`grep`) confirmaram **zero perda de campo escalar** — reclassificados para PRESENTE/Baixa aqui.
- **Confirmações diretas por mim (não por agente):** base-class (`Entity` vs `EntidadeSaaSBase`), `Venda.cs` (enum→int), `VendaItemImposto` 70=70, `VendaItemImpostoIbsCbs` 21=21, `NcmTributacao` 53=53, `Produto` diff por nome, `Contador` diff por nome, e contagem 1:1 dos 20 filhos de Compras que o sub-relatório só amostrou.
- **Não auditado a fundo (baixo risco, arquivos existem):** propriedades internas de alguns filhos de transporte/endereço além da contagem; corpo de métodos (fora de escopo — auditamos dados/campos, não comportamento).
- **Entidades novas sem contrapartida legado** (não são gap; são expansão do produto SaaS): módulos DMS, ESG, GRC, Manutencao, Producao, Projetos, Qualidade, RH inteiros, e em GestaoClientes/Aplicativo dezenas de entidades SaaS (Plano, Assinatura, Cupom, Revenda, Fatura SaaS, SessaoUsuario, etc.). Fora do escopo "completude da migração do legado".
