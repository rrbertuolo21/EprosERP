# EF_MOTOR_CALCULO_TRIBUTARIO_V1

## 1. Identificacao

| Item | Conteudo |
|---|---|
| Empresa | Siser |
| Sistema | Epros |
| Modulo | Plataforma Compartilhada |
| Submodulo | Faturamento Fiscal Eletronico |
| Documento | Especificacao funcional - Motor de calculo tributario |
| Versao | V1 |
| Status | Concluido |

## 2. Objetivo funcional

O motor de calculo tributario do Epros valida dados fiscais de venda, compra, servico e documento eletronico antes da transmissao, aplica regras de CFOP, CST, CSOSN, PIS, COFINS, ICMS, IPI, IBS/CBS, ISS, IBPT e rateios, e retorna itens calculados, erros funcionais ou dados aptos para autorizacao fiscal.

O motor deve ser consumido pela emissao fiscal e pelas integracoes operacionais sem duplicar cadastro fiscal. CFOP, NCM, grupo tributario, tipo de operacao, aliquotas, beneficio, enquadramento IPI e classificacoes devem vir dos cadastros fiscais do Epros.

## 3. Escopo

| Area | Incluso |
|---|---|
| Validacao de participantes | Emitente e destinatario com CPF/CNPJ, inscricao estadual do destinatario e regras basicas de documento. |
| Validacao de item | Produto, codigo de barras, NCM, CFOP, origem, CST, CSOSN, PIS, COFINS, IPI e regime tributario do item. |
| Validacao NFC-e | CFOP permitido, CSOSN permitido, CST ICMS permitido e matriz CFOP x CSOSN / CFOP x CST. |
| ICMS | Calculo por CST e por CSOSN, incluindo cenarios de ST, reducao, desoneracao e aliquotas informadas. |
| PIS | Calculo por CST PIS informado no material. |
| COFINS | Calculo por CST COFINS informado no material. |
| IPI | Calculo por CST IPI informado no material. |
| IBS/CBS | Calculo por CST IBS/CBS 000, 200, 410 e 510 conforme material. |
| ISS | Calculo ISS para NFS-e conforme material. |
| IBPT | Aplicacao de tabela IBPT por NCM/UF no item. |
| Rateios | Rateio de acrescimo, desconto, frete, seguro e outros valores entre itens. |
| Retorno fiscal | Resultado por item, imposto, validacao, mensagens e dados de autorizacao fiscal. |

## 4. Fora de escopo deste documento

| Item | Tratamento |
|---|---|
| Cadastro fiscal mestre | Descrito na EF de cadastros fiscais. |
| Transmissao para autoridade fiscal | Descrita nas EFs especificas de NF-e, NFC-e, NFS-e e eventos. |
| Armazenamento XML/PDF | Descrito nas EFs de documentos e downloads fiscais. |
| Aliquotas oficiais nao informadas | Devem permanecer em cadastros ou MC; nao sao inventadas neste documento. |

## 5. Entradas do motor

| Entrada | Campos principais | Regra |
|---|---|---|
| Documento | Modelo, ambiente, finalidade, emitente, destinatario, itens, pagamento, transporte e cobranca quando aplicavel. | Deve ser montado antes da transmissao. |
| Emitente | Documento fiscal, CPF/CNPJ e dados fiscais vinculados a empresa. | CPF/CNPJ deve ter quantidade de caracteres valida e numero valido. |
| Destinatario | Documento fiscal, CPF/CNPJ e indicador de inscricao estadual. | CPF/CNPJ e indicador de IE devem ser validados. |
| Item | Produto, NCM, CFOP, unidade, quantidade, valor unitario, origem, CST/CSOSN, aliquotas e rateios. | Deve ser validado antes do calculo e transmissao. |
| Cadastros fiscais | CFOP, NCM, tributacao NCM, grupo tributario, aliquotas, beneficio, FCP, ICMS interestadual, IBPT e IBS/CBS. | Fonte funcional das regras do motor. |
| Totais e rateios | Frete, desconto, seguro, acrescimo e outros valores. | Devem ser rateados entre itens quando aplicavel. |

## 6. Saidas do motor

| Saida | Conteudo |
|---|---|
| Documento apto | Itens validados, tributos calculados e dados fiscais prontos para transmissao. |
| Erro funcional | Campo, regra, mensagem e item/documento afetado. |
| Resultado por item | Bases, aliquotas, valores, CST/CSOSN, rateios e tributos aproximados. |
| Resultado por imposto | ICMS, PIS, COFINS, IPI, IBS/CBS, ISS e IBPT quando aplicaveis. |
| Dados de autorizacao | Documento, itens, impostos, cobranca, transporte, pagamentos e XML quando aplicavel. |

## 7. Regras funcionais

### 7.1 Participantes

| Codigo | Regra |
|---|---|
| CALC-001 | CPF/CNPJ do destinatario deve possuir quantidade de caracteres valida. |
| CALC-002 | CPF do destinatario deve ser valido quando informado como CPF. |
| CALC-003 | CNPJ do destinatario deve ser valido quando informado como CNPJ. |
| CALC-004 | Indicador de inscricao estadual do destinatario deve aceitar apenas os valores 1, 2 ou 9. |
| CALC-005 | CPF/CNPJ do emitente deve possuir quantidade de caracteres valida. |
| CALC-006 | CPF do emitente deve ser valido quando informado como CPF. |
| CALC-007 | CNPJ do emitente deve ser valido quando informado como CNPJ. |

### 7.2 Itens e campos obrigatorios condicionais

| Codigo | Regra |
|---|---|
| CALC-008 | Item deve possuir produto, NCM, CFOP, unidade, quantidade e valor unitario antes do calculo. |
| CALC-009 | CFOP do item deve ser valido para NFC-e quando o documento for NFC-e. |
| CALC-010 | CSOSN do item deve ser valido para NFC-e quando usado em NFC-e. |
| CALC-011 | CST ICMS do item deve ser valido para NFC-e quando usado em NFC-e. |
| CALC-012 | CST PIS/COFINS deve ser valido para NF-e e NFC-e. |
| CALC-013 | Regime tributario do item deve ser valido. |
| CALC-014 | Produto com CST ICMS 10 deve informar aliquota ICMS. |
| CALC-015 | Produto com CST ICMS que exige reducao de base deve informar percentual de reducao da base de calculo. |
| CALC-016 | Produto com CST PIS 01 deve informar aliquota PIS. |
| CALC-017 | Produto com CST COFINS 01 deve informar aliquota COFINS. |

### 7.3 Validacoes NFC-e

| Codigo | Regra |
|---|---|
| CALC-018 | CFOP permitido para NFC-e: 5101, 5102, 5103, 5104, 5115, 5405, 5653, 5656, 5667 e 5933. |
| CALC-019 | CSOSN permitido para NFC-e: 102, 103, 300, 400, 500, 900, 02, 15, 53 e 61. |
| CALC-020 | CST ICMS permitido para NFC-e: 00, 20, 40, 41, 60, 90, 02, 15, 53 e 61. |
| CALC-021 | O motor deve validar matriz CFOP x CSOSN NFC-e para CSOSN 02, 102, 103, 15, 300, 400, 500, 53, 61 e 900. |
| CALC-022 | O motor deve validar matriz CFOP x CST NFC-e para CST 00, 02, 15, 20, 40, 41, 53, 60, 61 e 90. |
| CALC-023 | Codigo de barras deve ser validado quando informado no item. |

### 7.4 Impostos

| Codigo | Regra |
|---|---|
| CALC-024 | O motor deve calcular ICMS por CST. |
| CALC-025 | O motor deve calcular ICMS por CSOSN. |
| CALC-026 | O motor deve calcular PIS por CST. |
| CALC-027 | O motor deve calcular COFINS por CST. |
| CALC-028 | O motor deve calcular IPI por CST. |
| CALC-029 | O motor deve calcular IBS/CBS por CST quando a regra fiscal exigir. |
| CALC-030 | O motor deve calcular ISS para NFS-e quando a operacao for de servico. |
| CALC-031 | O motor deve aplicar tabela IBPT por NCM/UF no item quando houver dados disponiveis. |

### 7.5 ICMS

| Codigo | Regra |
|---|---|
| CALC-032 | ICMS por CST deve contemplar CST 00, 10, 20, 30, 40, 41, 50, 51, 53, 60, 61, 70 e 90 conforme material. |
| CALC-033 | ICMS CST 20 SAT deve ser tratado quando a operacao CF-e/SAT exigir. |
| CALC-034 | ICMS por CSOSN deve contemplar CSOSN 101, 102, 103, 201, 202, 203, 300, 400, 500 e 900 conforme material. |
| CALC-035 | ICMS deve retornar estrutura de imposto do item quando calculado. |
| CALC-036 | ST retida em operacao anterior deve considerar base, aliquota, valor de ICMS ST retido e valor de ICMS proprio substituto quando esses campos existirem no item. |

### 7.6 PIS, COFINS, IPI, IBS/CBS e ISS

| Codigo | Regra |
|---|---|
| CALC-037 | COFINS deve contemplar CST 01, 02, 03, 04, 05, 06, 07, 08, 09, 49 e 99 conforme material. |
| CALC-038 | PIS deve contemplar CST 01, 02, 03, 04, 05, 06, 07, 08, 09, 49 e 99 conforme material. |
| CALC-039 | IPI deve contemplar CST 00, 01, 02, 03, 04, 05, 49, 50, 51, 52, 53, 54, 55 e 99 conforme material. |
| CALC-040 | IBS/CBS deve contemplar CST 000, 200, 410 e 510 conforme material. |
| CALC-041 | ISS deve ser calculado para NFS-e conforme a estrutura de valores de servico. |
| CALC-042 | Reducao de IPI deve considerar percentual e tipo de reducao quando informados no item. |

### 7.7 Rateios

| Codigo | Regra |
|---|---|
| CALC-043 | Frete deve poder ser rateado entre itens. |
| CALC-044 | Desconto deve poder ser rateado entre itens. |
| CALC-045 | Seguro deve poder ser rateado entre itens. |
| CALC-046 | Acrescimo deve poder ser rateado entre itens. |
| CALC-047 | Outros valores devem poder ser rateados entre itens. |
| CALC-048 | O material comprova rateio proporcional de frete; demais criterios finais de rateio devem ser validados na MC. |

### 7.8 Integridade operacional

| Codigo | Regra |
|---|---|
| CALC-049 | O motor deve consumir CFOP e NCM dos cadastros fiscais do Epros. |
| CALC-050 | O motor deve consumir tributacao cadastrada para definir regras aplicaveis ao item. |
| CALC-051 | O motor deve validar itens antes da transmissao fiscal. |
| CALC-052 | Documento com erro de validacao deve retornar erro funcional e nao deve seguir para transmissao. |
| CALC-053 | Resultado autorizado deve carregar impostos calculados para compor dados de autorizacao. |

## 8. Impostos suportados

| Grupo | Variantes informadas |
|---|---|
| ICMS CST | 00, 10, 20, 20 SAT, 30, 40, 41, 50, 51, 53, 60, 61, 70, 90 |
| ICMS CSOSN | 101, 102, 103, 201, 202, 203, 300, 400, 500, 900 |
| PIS | CST 01, 02, 03, 04, 05, 06, 07, 08, 09, 49, 99 |
| COFINS | CST 01, 02, 03, 04, 05, 06, 07, 08, 09, 49, 99 |
| IPI | CST 00, 01, 02, 03, 04, 05, 49, 50, 51, 52, 53, 54, 55, 99 |
| IBS/CBS | CST 000, 200, 410, 510 |
| ISS | ISS para NFS-e |
| IBPT | Calculo por NCM/UF aplicado ao item |

## 9. Fluxo funcional

| Passo | Ator | Acao | Entrada | Validacao | Saida |
|---|---|---|---|---|---|
| 1 | Modulo solicitante | Envia documento para validacao/calculo. | Documento, participantes, itens e totais. | Empresa, modelo e dados minimos. | Documento recebido. |
| 2 | Epros | Valida participantes. | Emitente e destinatario. | CPF/CNPJ e indicador IE. | Participantes validos ou erro. |
| 3 | Epros | Valida itens. | Produto, NCM, CFOP, CST/CSOSN, aliquotas e regime. | Matrizes e obrigatoriedade condicional. | Itens validos ou erro. |
| 4 | Epros | Aplica rateios. | Frete, desconto, seguro, acrescimo e outros valores. | Criterio de rateio disponivel. | Valores rateados por item. |
| 5 | Epros | Calcula impostos. | Itens validados e cadastros fiscais. | Imposto aplicavel e regra por CST/CSOSN. | Resultado por imposto. |
| 6 | Epros | Aplica IBPT. | NCM e UF. | Tabela IBPT disponivel. | Tributos aproximados no item. |
| 7 | Epros | Monta retorno fiscal. | Itens e impostos calculados. | Consistencia final. | Documento apto ou erro funcional. |

## 10. Modelo de dados funcional e implantavel

As estruturas abaixo sao estruturas funcionais de entrada, processamento e retorno do motor. O material comprova campos e operacoes do motor, mas nao informa tabelas finais especificas para persistencia do motor de calculo.[^nota1]

### 10.1 Entidades funcionais

| Entidade funcional | Finalidade | Cardinalidade | Observacao |
|---|---|---|---|
| `calculo_tributario_requisicao` | Representar a solicitacao de validacao/calculo. | 1 por operacao | Entrada do motor. |
| `calculo_tributario_participante` | Representar emitente e destinatario. | 1..N por requisicao | Usado para validar CPF/CNPJ e IE. |
| `calculo_tributario_item` | Representar item fiscal calculavel. | 1..N por requisicao | Base para validacao, rateio e impostos. |
| `calculo_tributario_rateio_item` | Representar valores rateados no item. | 0..N por item | Frete, desconto, seguro, acrescimo e outros. |
| `calculo_tributario_imposto_item` | Representar imposto calculado por item. | 0..N por item | ICMS, PIS, COFINS, IPI, IBS/CBS, ISS e IBPT. |
| `calculo_tributario_validacao` | Representar resultado de validacao. | 0..N por requisicao/item | Campo, regra e mensagem. |
| `calculo_tributario_retorno` | Representar resultado final do motor. | 1 por requisicao | Documento apto ou erro funcional. |
| `matriz_validacao_fiscal` | Representar combinacoes fiscais permitidas. | 0..N | CFOP x CST e CFOP x CSOSN. |

### 10.2 Relacionamentos

| Origem | Relacao | Destino | Regra |
|---|---|---|---|
| `calculo_tributario_requisicao` | possui | `calculo_tributario_participante` | Emitente e destinatario pertencem a uma requisicao. |
| `calculo_tributario_requisicao` | possui | `calculo_tributario_item` | Cada requisicao possui itens. |
| `calculo_tributario_item` | possui | `calculo_tributario_rateio_item` | Rateios sao aplicados por item. |
| `calculo_tributario_item` | possui | `calculo_tributario_imposto_item` | Impostos sao calculados por item. |
| `calculo_tributario_requisicao` | possui | `calculo_tributario_validacao` | Erros e avisos pertencem a requisicao. |
| `calculo_tributario_item` | pode possuir | `calculo_tributario_validacao` | Erros de item apontam o item afetado. |
| `calculo_tributario_requisicao` | gera | `calculo_tributario_retorno` | Retorno final da operacao. |
| `matriz_validacao_fiscal` | valida | `calculo_tributario_item` | Matriz compara modelo, CFOP, CST e CSOSN. |

## 11. Dicionario de dados implantavel

### 11.1 `calculo_tributario_requisicao`

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| Id | Identificador | Nao informado no material | Sim | Chave primaria funcional | Identificador da requisicao. |
| TenantId | Texto | varchar(200) quando aplicavel | Condicional | Tenant | Isolamento quando informado. |
| EmpresaId | Identificador | Nao informado no material | Sim | Empresa | Empresa emissora/operacional. |
| OrigemOperacional | Enum/texto | Venda, Compra, Servico, ImportacaoXml, PDV | Condicional | Origem | Consolidado a partir das operacoes comprovadas.[^nota1] |
| ModeloDocumento | Numero/texto | 55, 65, 59, NFS-e ou Nao informado no material | Condicional | Documento | Modelo orienta validacao e imposto. |
| Ambiente | Enum/texto | Nao informado no material | Condicional | Parametro fiscal | Ambiente fiscal. |
| Finalidade | Enum | Nao informado no material | Condicional | Regra fiscal | Finalidade da operacao. |
| TipoFrete | Enum | Nao informado no material | Condicional | Transporte/rateio | Tipo de frete. |
| ValorFrete | Decimal | Nao informado no material | Nao | Rateio | Valor total de frete a ratear. |
| ValorDesconto | Decimal | Nao informado no material | Nao | Rateio | Valor total de desconto a ratear. |
| ValorSeguro | Decimal | Nao informado no material | Nao | Rateio | Valor total de seguro a ratear. |
| ValorAcrescimo | Decimal | Nao informado no material | Nao | Rateio | Valor total de acrescimo a ratear. |
| ValorOutro | Decimal | Nao informado no material | Nao | Rateio | Outros valores a ratear. |
| StatusCalculo | Enum/texto | Apto, Erro, Nao informado no material | Sim | Resultado | Status funcional do calculo.[^nota1] |

### 11.2 `calculo_tributario_participante`

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| Id | Identificador | Nao informado no material | Sim | Chave primaria funcional | Identificador do participante. |
| RequisicaoId | Identificador | Nao informado no material | Sim | Requisicao | Vinculo com a requisicao. |
| Papel | Enum/texto | Emitente, Destinatario | Sim | Papel fiscal | Define validacoes aplicaveis. |
| Documento | Texto | CPF/CNPJ | Sim | Documento fiscal | Deve ter quantidade de caracteres e numero validos. |
| IndicadorIeDestinatario | Numero | 1, 2, 9 | Condicional | Destinatario | Obrigatorio quando participante for destinatario e regra exigir. |

### 11.3 `calculo_tributario_item`

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| Id | Identificador | Nao informado no material | Sim | Chave primaria funcional | Identificador do item. |
| RequisicaoId | Identificador | Nao informado no material | Sim | Requisicao | Vinculo com a requisicao. |
| CodigoProduto | Texto | varchar(60) | Sim | Produto | Codigo do produto. |
| NomeProduto | Texto | varchar(120) | Sim | Produto | Nome do produto. |
| CodigoBarras | Texto | varchar(20) | Sim | Produto | Validado quando informado. |
| Ncm | Texto | varchar(50); 8 caracteres quando usado como codigo NCM | Sim | NCM | Codigo NCM do item. |
| Cfop | Numero/codigo | 4 caracteres quando informado | Sim | CFOP | CFOP do item. |
| Unidade | Texto | varchar(50) | Sim | Unidade | Unidade do item. |
| ValorUnitario | Decimal | decimal(21,10) | Sim | Valor | Valor unitario. |
| Quantidade | Decimal | decimal(15,4) | Sim | Quantidade | Quantidade fiscal. |
| Origem | Texto | varchar(5) | Sim | Origem mercadoria | Origem fiscal. |
| Csosn | Texto | varchar(5) | Nao | CSOSN | Obrigatorio conforme regime/regra. |
| CstIcms | Texto | varchar(5) | Nao | CST ICMS | Obrigatorio conforme regime/regra. |
| CstPisCofins | Texto | varchar(3) | Sim | CST PIS/COFINS | Validado para NF-e/NFC-e. |
| CstIpi | Texto | varchar(5) | Nao | CST IPI | Usado quando IPI aplicavel. |
| EnquadramentoIpi | Texto | varchar(5) | Nao | IPI | Enquadramento IPI. |
| CompoeValorTotal | Enum | Nao informado no material | Sim | Total | Indica se compoe total. |
| RegimeTributario | Enum/texto | Nao informado no material | Condicional | Regime | Deve ser valido. |

### 11.4 `calculo_tributario_rateio_item`

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| Id | Identificador | Nao informado no material | Sim | Chave primaria funcional | Identificador do rateio. |
| ItemId | Identificador | Nao informado no material | Sim | Item | Item rateado. |
| ValorDesconto | Decimal | decimal(18,2) | Sim | Valor | Desconto do item. |
| ValorDescontoRateado | Decimal | decimal(18,2) | Sim | Rateio | Desconto rateado. |
| ValorFreteRateado | Decimal | decimal(18,2) | Sim | Rateio | Frete rateado. |
| ValorSeguroRateado | Decimal | decimal(18,2) | Sim | Rateio | Seguro rateado. |
| ValorAcrescimoRateado | Decimal | decimal(18,2) | Sim | Rateio | Acrescimo rateado. |
| ValorOutroRateado | Decimal | decimal(18,2) | Sim | Rateio | Outros valores rateados. |

### 11.5 `calculo_tributario_imposto_item`

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| Id | Identificador | Nao informado no material | Sim | Chave primaria funcional | Identificador do imposto. |
| ItemId | Identificador | Nao informado no material | Sim | Item | Item calculado. |
| TipoImposto | Enum/texto | ICMS, PIS, COFINS, IPI, IBS, CBS, ISS, IBPT | Sim | Imposto | Tipo de imposto calculado. |
| Cst | Texto | Nao informado no material | Condicional | CST | CST aplicavel. |
| Csosn | Texto | Nao informado no material | Condicional | CSOSN | CSOSN aplicavel. |
| ValorBaseCalculo | Decimal | Nao informado no material | Condicional | Base | Base de calculo. |
| ValorAliquota | Decimal | Nao informado no material | Condicional | Aliquota | Aliquota usada no calculo. |
| ValorTributo | Decimal | Nao informado no material | Condicional | Resultado | Valor calculado. |
| ValorReducaoPercentual | Decimal | decimal(18,2) quando ICMS/IPI do item | Condicional | Reducao | Percentual de reducao. |
| TipoReducao | Enum | Nao informado no material | Condicional | Reducao | Tipo de reducao. |
| ValorBaseCalculoStRetidoOperacaoAnterior | Decimal | decimal(18,3) | Condicional | ST | Base ST retida anterior. |
| ValorAliquotaSt | Decimal | decimal(18,2) | Condicional | ST | Aliquota ST. |
| ValorIcmsStRetidoOperacaoAnterior | Decimal | decimal(18,2) | Condicional | ST | ICMS ST retido anterior. |
| ValorIcmsProprioSubstituto | Decimal | decimal(18,2) | Condicional | ST | ICMS proprio substituto. |
| ValorAliquotaIcms | Decimal | decimal(18,3) | Condicional | ICMS | Aliquota ICMS do item. |
| ValorAliquotaPis | Decimal | decimal(18,2) | Condicional | PIS | Aliquota PIS. |
| ValorAliquotaPisReal | Decimal | decimal(18,4) | Condicional | PIS | Aliquota real PIS. |
| ValorAliquotaCofins | Decimal | decimal(18,2) | Condicional | COFINS | Aliquota COFINS. |
| ValorAliquotaCofinsReal | Decimal | decimal(18,4) | Condicional | COFINS | Aliquota real COFINS. |
| ValorAliquotaIpi | Decimal | decimal(18,2) | Condicional | IPI | Aliquota IPI. |
| FonteCalculo | Texto | CadastroFiscal, IBPT, RegraDocumento ou Nao informado no material | Condicional | Origem regra | Consolidado para rastrear origem funcional sem citar fontes anteriores.[^nota1] |

### 11.6 `calculo_tributario_validacao`

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| Id | Identificador | Nao informado no material | Sim | Chave primaria funcional | Identificador da validacao. |
| RequisicaoId | Identificador | Nao informado no material | Sim | Requisicao | Requisicao validada. |
| ItemId | Identificador | Nao informado no material | Condicional | Item | Item afetado quando aplicavel. |
| Campo | Texto | Nao informado no material | Sim | Campo | Campo validado. |
| Regra | Texto | Nao informado no material | Sim | Regra | Regra funcional aplicada. |
| Mensagem | Texto | Nao informado no material | Sim | Mensagem | Mensagem de erro ou alerta. |
| Severidade | Enum/texto | Erro, Alerta ou Nao informado no material | Sim | Resultado | Erro bloqueia transmissao. |

### 11.7 `calculo_tributario_retorno`

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| Id | Identificador | Nao informado no material | Sim | Chave primaria funcional | Identificador do retorno. |
| RequisicaoId | Identificador | Nao informado no material | Sim | Requisicao | Requisicao respondida. |
| AptoTransmissao | Booleano | Nao informado no material | Sim | Resultado | Indica se documento pode seguir para transmissao. |
| PossuiErro | Booleano | Nao informado no material | Sim | Resultado | Indica erro funcional. |
| Mensagem | Texto | Nao informado no material | Nao | Resultado | Mensagem consolidada. |
| DadosAutorizacaoGerados | Booleano | Nao informado no material | Condicional | Autorizacao | Indica se dados de autorizacao foram montados. |

### 11.8 `matriz_validacao_fiscal`

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| Id | Identificador | Nao informado no material | Sim | Chave primaria funcional | Identificador da matriz. |
| ModeloDocumento | Texto/numero | NFC-e quando informado | Sim | Documento | Modelo validado. |
| Cfop | Texto | 4 caracteres quando informado | Sim | CFOP | CFOP validado. |
| TipoCodigo | Enum/texto | CST, CSOSN | Sim | Tipo | Define a coluna validada. |
| CodigoTributario | Texto | Nao informado no material | Sim | CST/CSOSN | Codigo validado. |
| Permitido | Booleano | Nao informado no material | Sim | Resultado | Indica combinacao permitida. |
| Observacao | Texto | Nao informado no material | Nao | Observacao | Uso para lacunas homologadas. |

## 12. Integracoes funcionais

| Integracao | Dados consumidos | Efeito |
|---|---|---|
| Cadastros fiscais | CFOP, NCM, tributacao, grupo, FCP, ICMS, beneficio, IPI e IBS/CBS | Define regras e aliquotas de calculo. |
| Vendas | Documento, itens, cliente, pagamento e transporte | Solicita calculo antes da emissao. |
| Compras | Dados de compra e XML quando aplicavel | Permite aplicar dados fiscais de entrada. |
| NFS-e | Servico e valores | Usa calculo ISS. |
| CF-e/SAT | Itens e CST ICMS SAT quando aplicavel | Usa calculo ICMS SAT citado no material. |
| IBPT | NCM e UF | Retorna tributos aproximados por item. |
| Autorizacao fiscal | Documento calculado | Recebe dados aptos para montar autorizacao. |

## 13. Criterios de aceite

| Codigo | Criterio |
|---|---|
| CA-CALC-001 | CPF/CNPJ de emitente e destinatario deve ser validado antes do calculo final. |
| CA-CALC-002 | Indicador IE do destinatario deve aceitar somente 1, 2 ou 9. |
| CA-CALC-003 | NFC-e deve rejeitar CFOP fora da lista permitida. |
| CA-CALC-004 | NFC-e deve rejeitar CSOSN fora da lista permitida. |
| CA-CALC-005 | NFC-e deve rejeitar CST ICMS fora da lista permitida. |
| CA-CALC-006 | CST ICMS 10 deve exigir aliquota ICMS. |
| CA-CALC-007 | CST PIS 01 deve exigir aliquota PIS. |
| CA-CALC-008 | CST COFINS 01 deve exigir aliquota COFINS. |
| CA-CALC-009 | Frete deve ser rateado proporcionalmente entre itens conforme teste indicado no material. |
| CA-CALC-010 | IBPT deve ser aplicado ao item por NCM/UF quando tabela estiver disponivel. |
| CA-CALC-011 | ICMS CST 00 deve calcular base conforme teste indicado no material. |
| CA-CALC-012 | Documento com erro funcional nao deve seguir para transmissao. |
| CA-CALC-013 | O motor deve consumir CFOP/NCM/tributacao dos cadastros fiscais. |

## 14. Pontos pendentes para validacao

| Ponto | Impacto |
|---|---|
| Matriz completa CFOP x CST x CSOSN por modelo, regime e UF | Necessaria para homologacao fiscal ampla. |
| Formula detalhada de cada CST/CSOSN | Material comprova cobertura, mas nao traz todas as formulas. |
| Criterio final de rateio para desconto, seguro, acrescimo e outros valores | Material comprova os rateios, mas so explicita teste proporcional para frete. |
| Arredondamento e casas decimais finais por tributo | Necessario para evitar divergencia de centavos. |
| Regras completas de IBS/CBS | Material comprova CSTs 000, 200, 410 e 510, mas detalhamento final depende de homologacao. |
| Persistencia fisica do resultado do motor | Material comprova estruturas funcionais, nao tabela final do motor. |

## 15. Notas de autoria

[^nota1]: Estruturas funcionais do motor foram organizadas para implantacao e validacao humana porque o material comprova campos, validacoes, impostos e retornos, mas nao informa tabelas finais especificas de persistencia para o motor de calculo.
