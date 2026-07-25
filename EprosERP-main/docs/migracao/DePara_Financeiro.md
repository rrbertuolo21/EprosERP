# De → Para — Módulo Financeiro

Auditoria de fidelidade da migração do módulo **Financeiro** do legado (`Epros.ERP.Domain`) para o novo módulo modular (`Epros.Modules.Financeiro`).

- **Data:** 2026-07-01
- **Fontes legadas:**
  - `Epros.ERP.Domain/Entities/Financeiros`
  - `Epros.ERP.Domain/Entities/Cadastros/Bancos`
  - `Epros.ERP.Domain/Entities/Importacoes`
- **Módulo novo:** `src/Modules/Epros.Modules.Financeiro`

## Convenções

- Campos herdados de `EntidadeSaaSBase` (`Id`, `SyncId`, `TenantId`, `SyncVersion`, `CriadoEm`, `AlteradoEm`, `DeletadoEm`, `CriadoPor`, `AlteradoPor`) são considerados **COBERTOS** para auditoria/identidade.
- No legado, `long Id` (de `Entity`/`EntityNoTenat`) → `Guid Id` na base nova; todos os `...Id` (FKs) `long`/`long?` → `Guid`/`Guid?`.
- `SequenciaTenantId` (numeração sequencial por tenant, exibida ao usuário) **NÃO** é coberto pela auditoria da base — é campo de negócio próprio. Marcado como AUSENTE onde aplicável.
- Propriedades de navegação EF que apenas espelham FKs já mapeadas não são listadas como campos individuais (exceto quando revelam FK ausente).

---

## 1. Financeiros/ContasAPagar → `Domain/Entities/ContasAPagar`

| Entidade legada | Campo legado | Destino novo (entidade.campo) ou AUSENTE |
|---|---|---|
| ContasAPagar | SequenciaTenantId | **AUSENTE** |
| ContasAPagar | PessoaId | ContasAPagar.PessoaId (Guid) |
| ContasAPagar | PlanoDeContasFinanceiroItemId | ContasAPagar.PlanoDeContasFinanceiroItemId |
| ContasAPagar | FatoGeradorFinanceiroId | ContasAPagar.FatoGeradorFinanceiroId |
| ContasAPagar | NomePessoa | ContasAPagar.NomePessoa |
| ContasAPagar | Situacao | ContasAPagar.Situacao |
| ContasAPagar | DataVencimento | ContasAPagar.DataVencimento |
| ContasAPagar | DataEmissao | ContasAPagar.DataEmissao |
| ContasAPagar | DataBaixa | ContasAPagar.DataBaixa |
| ContasAPagar | Documento | ContasAPagar.Documento |
| ContasAPagar | ValorTitulo | ContasAPagar.ValorTitulo |
| ContasAPagar | ValorTotalDesconto | ContasAPagar.ValorTotalDesconto |
| ContasAPagar | ValorTotalMulta | ContasAPagar.ValorTotalMulta |
| ContasAPagar | ValorTotalJuros | ContasAPagar.ValorTotalJuros |
| ContasAPagar | ValorTotalTroco | ContasAPagar.ValorTotalTroco |
| ContasAPagar | ValorTotalAcrescimo | ContasAPagar.ValorTotalAcrescimo |
| ContasAPagar | ValorTotalPago | ContasAPagar.ValorTotalPago |
| ContasAPagar | ValorTotalAPagarTitulo | ContasAPagar.ValorTotalAPagarTitulo |
| ContasAPagar | ValorInicialDesconto | ContasAPagar.ValorInicialDesconto |
| ContasAPagar | ValorInicialMulta | ContasAPagar.ValorInicialMulta |
| ContasAPagar | ValorInicialJuros | ContasAPagar.ValorInicialJuros |
| ContasAPagar | ValorInicialAcrescimo | ContasAPagar.ValorInicialAcrescimo |
| ContasAPagar | ValorInicialAPagarTitulo | ContasAPagar.ValorInicialAPagarTitulo |
| ContasAPagar | NumeroParcela | ContasAPagar.NumeroParcela |
| ContasAPagar | Detalhamento | ContasAPagar.Detalhamento |
| ContasAPagar | JustificativaCancelamento | ContasAPagar.JustificativaCancelamento |
| ContasAPagar | Pessoa (nav) | Omitida (FK Guid PessoaId, sem navegação cross-módulo — intencional) |
| ContasAPagar | PlanoDeContasFinanceiroItem (nav) | ContasAPagar.PlanoDeContasFinanceiroItem |
| ContasAPagar | FatoGeradorFinanceiro (nav) | ContasAPagar.FatoGeradorFinanceiro |
| ContasAPagar | ContasAPagarItens (nav) | ContasAPagar.ContasAPagarItens |

## 2. Financeiros/ContasAPagarItem → `Domain/Entities/ContasAPagarItem`

| Entidade legada | Campo legado | Destino novo (entidade.campo) ou AUSENTE |
|---|---|---|
| ContasAPagarItem | ContasAPagarId | ContasAPagarItem.ContasAPagarId |
| ContasAPagarItem | PlanoDeContasFinanceiroItemId | ContasAPagarItem.PlanoDeContasFinanceiroItemId |
| ContasAPagarItem | ContaBancariaId | ContasAPagarItem.ContaBancariaId |
| ContasAPagarItem | TipoPagamento | ContasAPagarItem.TipoPagamento |
| ContasAPagarItem | ValorParcela | ContasAPagarItem.ValorParcela |
| ContasAPagarItem | ValorPago | ContasAPagarItem.ValorPago |
| ContasAPagarItem | ValorDesconto | ContasAPagarItem.ValorDesconto |
| ContasAPagarItem | ValorMulta | ContasAPagarItem.ValorMulta |
| ContasAPagarItem | ValorJuros | ContasAPagarItem.ValorJuros |
| ContasAPagarItem | ValorTroco | ContasAPagarItem.ValorTroco |
| ContasAPagarItem | ValorAcrescimo | ContasAPagarItem.ValorAcrescimo |
| ContasAPagarItem | ValorAPagar | ContasAPagarItem.ValorAPagar |
| ContasAPagarItem | DataPagamento | ContasAPagarItem.DataPagamento |
| ContasAPagarItem | ContaBancaria (nav) | ContasAPagarItem.ContaBancaria |

## 3. Financeiros/ContasAReceber → `Domain/Entities/ContasAReceber`

| Entidade legada | Campo legado | Destino novo (entidade.campo) ou AUSENTE |
|---|---|---|
| ContasAReceber | SequenciaTenantId | **AUSENTE** |
| ContasAReceber | PessoaId | ContasAReceber.PessoaId |
| ContasAReceber | PlanoDeContasFinanceiroItemId | ContasAReceber.PlanoDeContasFinanceiroItemId |
| ContasAReceber | FatoGeradorFinanceiroId | ContasAReceber.FatoGeradorFinanceiroId |
| ContasAReceber | NomePessoa | ContasAReceber.NomePessoa |
| ContasAReceber | Situacao | ContasAReceber.Situacao |
| ContasAReceber | DataVencimento | ContasAReceber.DataVencimento |
| ContasAReceber | DataEmissao | ContasAReceber.DataEmissao |
| ContasAReceber | DataBaixa | ContasAReceber.DataBaixa |
| ContasAReceber | Documento | ContasAReceber.Documento |
| ContasAReceber | ValorTitulo | ContasAReceber.ValorTitulo |
| ContasAReceber | ValorTotalDesconto | ContasAReceber.ValorTotalDesconto |
| ContasAReceber | ValorTotalMulta | ContasAReceber.ValorTotalMulta |
| ContasAReceber | ValorTotalJuros | ContasAReceber.ValorTotalJuros |
| ContasAReceber | ValorTotalTroco | ContasAReceber.ValorTotalTroco |
| ContasAReceber | ValorTotalAcrescimo | ContasAReceber.ValorTotalAcrescimo |
| ContasAReceber | ValorTotalRecebido | ContasAReceber.ValorTotalRecebido |
| ContasAReceber | ValorTotalAReceberTitulo | ContasAReceber.ValorTotalAReceberTitulo |
| ContasAReceber | ValorInicialDesconto | ContasAReceber.ValorInicialDesconto |
| ContasAReceber | ValorInicialMulta | ContasAReceber.ValorInicialMulta |
| ContasAReceber | ValorInicialJuros | ContasAReceber.ValorInicialJuros |
| ContasAReceber | ValorInicialAcrescimo | ContasAReceber.ValorInicialAcrescimo |
| ContasAReceber | ValorInicialAReceberTitulo | ContasAReceber.ValorInicialAReceberTitulo |
| ContasAReceber | NumeroParcela | ContasAReceber.NumeroParcela |
| ContasAReceber | Detalhamento | ContasAReceber.Detalhamento |
| ContasAReceber | JustificativaCancelamento | ContasAReceber.JustificativaCancelamento |
| ContasAReceber | Pessoa (nav) | Omitida (FK Guid PessoaId, sem navegação cross-módulo — intencional) |
| ContasAReceber | PlanoDeContasFinanceiroItem (nav) | ContasAReceber.PlanoDeContasFinanceiroItem |
| ContasAReceber | FatoGeradorFinanceiro (nav) | ContasAReceber.FatoGeradorFinanceiro |
| ContasAReceber | ContasAReceberItens (nav) | ContasAReceber.ContasAReceberItens |

## 4. Financeiros/ContasAReceberItem → `Domain/Entities/ContasAReceberItem`

| Entidade legada | Campo legado | Destino novo (entidade.campo) ou AUSENTE |
|---|---|---|
| ContasAReceberItem | ContasAReceberId | ContasAReceberItem.ContasAReceberId |
| ContasAReceberItem | PlanoDeContasFinanceiroItemId | ContasAReceberItem.PlanoDeContasFinanceiroItemId |
| ContasAReceberItem | ContaBancariaId | ContasAReceberItem.ContaBancariaId |
| ContasAReceberItem | TipoPagamento | ContasAReceberItem.TipoPagamento |
| ContasAReceberItem | ValorParcela | ContasAReceberItem.ValorParcela |
| ContasAReceberItem | ValorPago | ContasAReceberItem.ValorPago |
| ContasAReceberItem | ValorDesconto | ContasAReceberItem.ValorDesconto |
| ContasAReceberItem | ValorMulta | ContasAReceberItem.ValorMulta |
| ContasAReceberItem | ValorJuros | ContasAReceberItem.ValorJuros |
| ContasAReceberItem | ValorTroco | ContasAReceberItem.ValorTroco |
| ContasAReceberItem | ValorAcrescimo | ContasAReceberItem.ValorAcrescimo |
| ContasAReceberItem | ValorAReceber | ContasAReceberItem.ValorAReceber |
| ContasAReceberItem | DataRecebimento | ContasAReceberItem.DataRecebimento |
| ContasAReceberItem | ContaBancaria (nav) | ContasAReceberItem.ContaBancaria |

## 5. Financeiros/FatoGeradorFinanceiro → `Domain/Entities/FatoGeradorFinanceiro`

| Entidade legada | Campo legado | Destino novo (entidade.campo) ou AUSENTE |
|---|---|---|
| FatoGeradorFinanceiro | Origem | FatoGeradorFinanceiro.Origem |
| FatoGeradorFinanceiro | VendaId | FatoGeradorFinanceiro.VendaId (Guid?) |
| FatoGeradorFinanceiro | CompraId | FatoGeradorFinanceiro.CompraId (Guid?) |
| FatoGeradorFinanceiro | Descricao | FatoGeradorFinanceiro.Descricao |
| FatoGeradorFinanceiro | ContasARecebers (nav) | FatoGeradorFinanceiro.ContasARecebers |
| FatoGeradorFinanceiro | ContasAPagars (nav) | FatoGeradorFinanceiro.ContasAPagars |
| FatoGeradorFinanceiro | Venda (nav) | Omitida (cross-módulo por Guid — intencional) |
| FatoGeradorFinanceiro | Compra (nav) | Omitida (cross-módulo por Guid — intencional) |
| FatoGeradorFinanceiro | Duplicar() / VincularVendaPaiParaClone() | **AUSENTE** (comportamento de clone não portado) |

## 6. Financeiros/PlanoDeContasFinanceiro → `Domain/Entities/PlanoDeContasFinanceiro`

| Entidade legada | Campo legado | Destino novo (entidade.campo) ou AUSENTE |
|---|---|---|
| PlanoDeContasFinanceiro | SequenciaTenantId | **AUSENTE** |
| PlanoDeContasFinanceiro | ConfiguracaoCodigoNaturezaFinanceiraRecebimentoId | PlanoDeContasFinanceiro.ConfiguracaoCodigoNaturezaFinanceiraRecebimentoId |
| PlanoDeContasFinanceiro | ConfiguracaoCodigoNaturezaFinanceiraPagamentoId | PlanoDeContasFinanceiro.ConfiguracaoCodigoNaturezaFinanceiraPagamentoId |
| PlanoDeContasFinanceiro | Descricao | PlanoDeContasFinanceiro.Descricao |
| PlanoDeContasFinanceiro | Mascara | PlanoDeContasFinanceiro.Mascara |
| PlanoDeContasFinanceiro | EhPadrao | PlanoDeContasFinanceiro.EhPadrao |
| PlanoDeContasFinanceiro | Itens (nav) | PlanoDeContasFinanceiro.Itens |
| PlanoDeContasFinanceiro | Empresas (nav) | **AUSENTE** (relação N:N Plano↔Empresa não portada) |
| PlanoDeContasFinanceiro | ConfiguracaoCodigoNaturezaFinanceiraRecebimento (nav) | PlanoDeContasFinanceiro.ConfiguracaoCodigoNaturezaFinanceiraRecebimento |
| PlanoDeContasFinanceiro | ConfiguracaoCodigoNaturezaFinanceiraPagamento (nav) | PlanoDeContasFinanceiro.ConfiguracaoCodigoNaturezaFinanceiraPagamento |
| PlanoDeContasFinanceiro | IncluirPlanoDeContasFinanceiroItem / AlterarPlanoDeContasFinanceiroItem / DeletarPlanoDeContasFinanceiroItem | **AUSENTE** (métodos de agregação de Itens não portados na entidade) |

## 7. Financeiros/PlanoDeContasFinanceiroItem → `Domain/Entities/PlanoDeContasFinanceiroItem`

| Entidade legada | Campo legado | Destino novo (entidade.campo) ou AUSENTE |
|---|---|---|
| PlanoDeContasFinanceiroItem | SequenciaTenantId | **AUSENTE** |
| PlanoDeContasFinanceiroItem | PlanoDeContasFinanceiroId | PlanoDeContasFinanceiroItem.PlanoDeContasFinanceiroId |
| PlanoDeContasFinanceiroItem | Codigo | PlanoDeContasFinanceiroItem.Codigo |
| PlanoDeContasFinanceiroItem | Descricao | PlanoDeContasFinanceiroItem.Descricao |
| PlanoDeContasFinanceiroItem | TipoDetalhamento | PlanoDeContasFinanceiroItem.TipoDetalhamento |
| PlanoDeContasFinanceiroItem | MovimentaCaixa | PlanoDeContasFinanceiroItem.MovimentaCaixa |
| PlanoDeContasFinanceiroItem | PlanoDeContasFinanceiro (nav) | PlanoDeContasFinanceiroItem.PlanoDeContasFinanceiro |
| PlanoDeContasFinanceiroItem | ContasAReceberItens (nav) | Omitida (coleção inversa não recriada — não crítica) |
| PlanoDeContasFinanceiroItem | ConfiguracaoCodigoNaturezaFinanceiras (nav) | Omitida (coleção inversa não recriada — não crítica) |

## 8. Financeiros/ConfiguracaoCodigoNaturezaFinanceira → `Domain/Entities/ConfiguracaoCodigoNaturezaFinanceira`

| Entidade legada | Campo legado | Destino novo (entidade.campo) ou AUSENTE |
|---|---|---|
| ConfiguracaoCodigoNaturezaFinanceira | SequenciaTenantId | **AUSENTE** |
| ConfiguracaoCodigoNaturezaFinanceira | EmpresaId | ConfiguracaoCodigoNaturezaFinanceira.EmpresaId (Guid) |
| ConfiguracaoCodigoNaturezaFinanceira | Descricao | ConfiguracaoCodigoNaturezaFinanceira.Descricao |
| ConfiguracaoCodigoNaturezaFinanceira | ItemPlanoDeContasFinanceiroDinheiroId ... TrocoId (23 FKs de forma de pagamento) | Todos os 23 mapeados 1:1 (Dinheiro, CartaoCheque, CartaoCredito, CartaoDebito, CartaoDaLoja, ValeAlimentacao, ValeRefeicao, ValePresente, ValeCombustivel, DuplicataMercantil, BoletoBancario, DepositoBancario, PixDinamico, TransferenciaBancaria, ProgramaDeFidelidade, PixEstatico, CreditoEmLoja, PagamentoEletronicoNaoInformado, Outros, Desconto, Acrescimo, Juros, Multa, Troco) |
| ConfiguracaoCodigoNaturezaFinanceira | TipoConfiguracaoNatureza | ConfiguracaoCodigoNaturezaFinanceira.TipoConfiguracaoNatureza |
| ConfiguracaoCodigoNaturezaFinanceira | Navs Item* (23) | Todas presentes |
| ConfiguracaoCodigoNaturezaFinanceira | Empresa (nav) | Omitida (cross-módulo por Guid EmpresaId — intencional) |
| ConfiguracaoCodigoNaturezaFinanceira | PlanoDeContasFinanceirosRecebimento (nav) | Omitida (coleção inversa não recriada — não crítica) |
| ConfiguracaoCodigoNaturezaFinanceira | PlanoDeContasFinanceirosPagamento (nav) | Omitida (coleção inversa não recriada — não crítica) |

## 9. Cadastros/Bancos/Banco → `Domain/Entities/Banco`

| Entidade legada | Campo legado | Destino novo (entidade.campo) ou AUSENTE |
|---|---|---|
| Banco (EntityNoTenat) | Codigo | Banco.Codigo |
| Banco | Descricao | Banco.Descricao |
| Banco | ContaBancarias (nav) | Omitida (coleção inversa não recriada — não crítica) |

> Legado herda de `EntityNoTenat` (global, sem tenant). Novo usa `EntidadeSaaSBase` + `IGlobalEntity` com `TenantId = "system"`. Equivalência semântica de entidade global preservada.

## 10. Cadastros/Bancos/ContaBancaria → `Domain/Entities/ContaBancaria`

| Entidade legada | Campo legado | Destino novo (entidade.campo) ou AUSENTE |
|---|---|---|
| ContaBancaria | SequenciaTenantId | **AUSENTE** |
| ContaBancaria | EmpresaID | **AUSENTE** — campo crítico: não há `EmpresaId` na entidade nova |
| ContaBancaria | BancoID | ContaBancaria.BancoId |
| ContaBancaria | TipoContaBancaria | ContaBancaria.TipoContaBancaria |
| ContaBancaria | Apelido | ContaBancaria.Apelido |
| ContaBancaria | Titular | ContaBancaria.Titular |
| ContaBancaria | Agencia | ContaBancaria.Agencia |
| ContaBancaria | Conta | ContaBancaria.Conta |
| ContaBancaria | Gerente | ContaBancaria.Gerente |
| ContaBancaria | FoneGerente | ContaBancaria.FoneGerente |
| ContaBancaria | Detalhe | ContaBancaria.Detalhe |
| ContaBancaria | DigitoAgencia | ContaBancaria.DigitoAgencia |
| ContaBancaria | DataEncerramento | ContaBancaria.DataEncerramento |
| ContaBancaria | Banco (nav) | ContaBancaria.Banco |
| ContaBancaria | Empresa (nav) | **AUSENTE** (decorre da ausência de EmpresaId) |
| ContaBancaria | ContasAReceberItem (nav) | Omitida (não crítica) |

## 11. Cadastros/Bancos/CartaoDeCredito → `Domain/Entities/CartaoDeCredito`

| Entidade legada | Campo legado | Destino novo (entidade.campo) ou AUSENTE |
|---|---|---|
| CartaoDeCredito | SequenciaTenantId | **AUSENTE** |
| CartaoDeCredito | ContaBancariaId | CartaoDeCredito.ContaBancariaId |
| CartaoDeCredito | Apelido | CartaoDeCredito.Apelido |
| CartaoDeCredito | Titular | CartaoDeCredito.Titular |
| CartaoDeCredito | BandeiraCartao | CartaoDeCredito.BandeiraCartao |
| CartaoDeCredito | Observacao | CartaoDeCredito.Observacao |
| CartaoDeCredito | ContaBancaria (nav) | CartaoDeCredito.ContaBancaria |
| CartaoDeCredito | CartaoDeCreditoFaturas (nav) | CartaoDeCredito.CartaoDeCreditoFaturas |

## 12. Cadastros/Bancos/CartaoDeCreditoFatura → `Domain/Entities/CartaoDeCreditoFatura`

| Entidade legada | Campo legado | Destino novo (entidade.campo) ou AUSENTE |
|---|---|---|
| CartaoDeCreditoFatura | CartaoDeCreditoId | CartaoDeCreditoFatura.CartaoDeCreditoId |
| CartaoDeCreditoFatura | DataLancamento | CartaoDeCreditoFatura.DataLancamento |
| CartaoDeCreditoFatura | DataVencimento | CartaoDeCreditoFatura.DataVencimento |
| CartaoDeCreditoFatura | Valor | CartaoDeCreditoFatura.Valor |
| CartaoDeCreditoFatura | Pago | CartaoDeCreditoFatura.Pago |
| CartaoDeCreditoFatura | CartaoDeCredito (nav) | CartaoDeCreditoFatura.CartaoDeCredito |

## 13. Importacoes/ImportacacaoArquivoOfx → `Domain/Entities/ImportacacaoArquivoOfx`

| Entidade legada | Campo legado | Destino novo (entidade.campo) ou AUSENTE |
|---|---|---|
| ImportacacaoArquivoOfx | CodigoBanco | ImportacacaoArquivoOfx.CodigoBanco |
| ImportacacaoArquivoOfx | NumeroConta | ImportacacaoArquivoOfx.NumeroConta |
| ImportacacaoArquivoOfx | TipoConta | ImportacacaoArquivoOfx.TipoConta |
| ImportacacaoArquivoOfx | DataInicioExtrato | ImportacacaoArquivoOfx.DataInicioExtrato |
| ImportacacaoArquivoOfx | DataFimExtrato | ImportacacaoArquivoOfx.DataFimExtrato |
| ImportacacaoArquivoOfx | Transacoes (nav) | ImportacacaoArquivoOfx.Transacoes |

## 14. Importacoes/ImportacacaoArquivoOfxTransacao → `Domain/Entities/ImportacacaoArquivoOfxTransacao`

| Entidade legada | Campo legado | Destino novo (entidade.campo) ou AUSENTE |
|---|---|---|
| ImportacacaoArquivoOfxTransacao | ImportacacaoArquivoOfxId | ImportacacaoArquivoOfxTransacao.ImportacacaoArquivoOfxId |
| ImportacacaoArquivoOfxTransacao | ContasAReceberId | ImportacacaoArquivoOfxTransacao.ContasAReceberId |
| ImportacacaoArquivoOfxTransacao | ContasAPagarId | ImportacacaoArquivoOfxTransacao.ContasAPagarId |
| ImportacacaoArquivoOfxTransacao | IdentificadorTransacao | ImportacacaoArquivoOfxTransacao.IdentificadorTransacao |
| ImportacacaoArquivoOfxTransacao | Data | ImportacacaoArquivoOfxTransacao.Data |
| ImportacacaoArquivoOfxTransacao | Valor | ImportacacaoArquivoOfxTransacao.Valor |
| ImportacacaoArquivoOfxTransacao | Tipo | ImportacacaoArquivoOfxTransacao.Tipo |
| ImportacacaoArquivoOfxTransacao | Descricao | ImportacacaoArquivoOfxTransacao.Descricao |
| ImportacacaoArquivoOfxTransacao | Conciliado | ImportacacaoArquivoOfxTransacao.Conciliado |
| ImportacacaoArquivoOfxTransacao | ImportacacaoArquivoOfx (nav) | ImportacacaoArquivoOfxTransacao.ImportacacaoArquivoOfx |
| ImportacacaoArquivoOfxTransacao | ContasAReceber (nav) | ImportacacaoArquivoOfxTransacao.ContasAReceber |
| ImportacacaoArquivoOfxTransacao | ContasAPagar (nav) | ImportacacaoArquivoOfxTransacao.ContasAPagar |

## 15. Importacoes/ImportacaoXml → **AUSENTE (entidade não portada)**

Entidade legada `ImportacaoXml` (campos: EmpresaId, Xml, TipoDeXml, NfeId, StatusImportacaoXml, MensagemErroImportacaoXml, StatusCadastro, MensagemErroCadastro, StatusSalvarPdf, MensagemErroSalvarPdf, CodigoSefaz, TipoEvento) **não possui destino** no módulo Financeiro novo.

> Observação: pertence a importação fiscal de XML (NF-e), fora do escopo financeiro/OFX. Provável destino é módulo Fiscal/DFE, não Financeiro. Registrar para migração no módulo correto.

## 16. Importacoes/ImportacaoArquivoXmlSaida → **AUSENTE (entidade não portada)**

Entidade legada `ImportacaoArquivoXmlSaida` (campos: NomeArquivo, QtdXmls, QtdXmlsInvalidos, QtdProdutosLocalizados, QtdClientesLocalizados, QtdProdutosImportados, QtdClientesImportados, MensagemErro, Status) **não possui destino** no módulo Financeiro novo.

> Observação: importação de XML de saída (produtos/clientes), fora do escopo financeiro. Provável destino é módulo Fiscal ou Cadastros, não Financeiro.

---

## Resumo de auditoria

### Entidades cobertas (14 de 16 do escopo das fontes)

Financeiros (8/8): ContasAPagar, ContasAPagarItem, ContasAReceber, ContasAReceberItem, FatoGeradorFinanceiro, PlanoDeContasFinanceiro, PlanoDeContasFinanceiroItem, ConfiguracaoCodigoNaturezaFinanceira.
Bancos (4/4): Banco, ContaBancaria, CartaoDeCredito, CartaoDeCreditoFatura.
Importacoes OFX (2/2): ImportacacaoArquivoOfx, ImportacacaoArquivoOfxTransacao.

### Entidades ausentes (2)

- `ImportacaoXml` — não portada (escopo fiscal/DFE, não financeiro).
- `ImportacaoArquivoXmlSaida` — não portada (escopo fiscal/cadastros, não financeiro).

### Campos críticos faltando

1. **ContaBancaria.EmpresaID** — AUSENTE no novo. Campo obrigatório no legado (validado `> 0`) que vincula a conta a uma empresa. Sem ele, contas bancárias não têm dono empresarial; impacta filtro/segregação por empresa. **Alta severidade.**
2. **SequenciaTenantId** — AUSENTE em 6 entidades (ContasAPagar, ContasAReceber, PlanoDeContasFinanceiro, PlanoDeContasFinanceiroItem, ConfiguracaoCodigoNaturezaFinanceira, ContaBancaria, CartaoDeCredito). Numeração sequencial por tenant exibida ao usuário; não é coberta pela auditoria da base. **Média severidade** (impacto em numeração amigável / relatórios).
3. **PlanoDeContasFinanceiro.Empresas (N:N)** — relação Plano↔Empresa não portada. Se o legado permite vincular planos a empresas específicas, a definição do plano padrão por empresa fica comprometida. **Média severidade.**
4. **FatoGeradorFinanceiro.Duplicar()** — comportamento de clonagem não portado (usado em duplicação de venda/compra). **Baixa/Média severidade** (comportamento, não campo).

### Observações não críticas

- Todas as FKs `long` → `Guid`; navegações cross-módulo (Pessoa, Venda, Compra, Empresa) intencionalmente substituídas por FK `Guid` sem propriedade de navegação (padrão de modularização). Considerado coberto.
- Coleções inversas de navegação (ContaBancaria.ContasAReceberItem, PlanoDeContasFinanceiroItem.ContasAReceberItens, etc.) não recriadas — não crítico.
- Entidades novas sem correspondência legada nas fontes analisadas: `ContaPagar`, `ContaPagarBaixa`, `ContaReceber`, `ContaReceberBaixa` (modelo alternativo de baixa; fora do escopo desta auditoria de → para).
