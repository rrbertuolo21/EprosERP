# MC_MOTOR_CALCULO_TRIBUTARIO_V1

## 1. Identificacao

| Item | Conteudo |
|---|---|
| Empresa | Siser |
| Sistema | Epros |
| Modulo | Plataforma Compartilhada |
| Submodulo | Faturamento Fiscal Eletronico |
| Documento | Matriz de completude - Motor de calculo tributario |
| Versao | V1 |
| Status | Concluido |

## 2. Cobertura do material

| Capacidade | Status | Evidencia funcional consolidada |
|---|---|---|
| Validacao emitente/destinatario | Parcial | CPF/CNPJ e indicador IE do destinatario aparecem com validacoes. |
| Validacao item fiscal | Parcial | CFOP, CSOSN, CST ICMS, CST PIS/COFINS, aliquotas obrigatorias e regime aparecem. |
| CFOP NFC-e | Completo no material para lista extraida | Lista permitida: 5101, 5102, 5103, 5104, 5115, 5405, 5653, 5656, 5667 e 5933. |
| CSOSN NFC-e | Completo no material para lista extraida | Lista permitida: 102, 103, 300, 400, 500, 900, 02, 15, 53 e 61. |
| CST ICMS NFC-e | Completo no material para lista extraida | Lista permitida: 00, 20, 40, 41, 60, 90, 02, 15, 53 e 61. |
| Matrizes NFC-e | Parcial | Existem matrizes CFOP x CSOSN e CFOP x CST para codigos citados. |
| ICMS | Parcial | Cobertura por CST e CSOSN comprovada; formulas completas nao informadas. |
| PIS | Parcial | Cobertura por CST comprovada; formulas completas nao informadas. |
| COFINS | Parcial | Cobertura por CST comprovada; formulas completas nao informadas. |
| IPI | Parcial | Cobertura por CST comprovada; formulas completas nao informadas. |
| IBS/CBS | Parcial | CST 000, 200, 410 e 510 comprovados; regras completas pendentes. |
| ISS | Parcial | Calculo ISS NFS-e comprovado; regras municipais completas pendentes. |
| IBPT | Parcial | Aplicacao por NCM/UF no item comprovada; governanca da tabela fica em documento proprio. |
| Rateios | Parcial | Frete, desconto, seguro, acrescimo e outros valores comprovados; criterio completo pendente. |
| Modelo de dados do motor | Parcial | Campos de item e retorno comprovados; tabelas finais nao informadas. |

## 3. Itens de completude

| Codigo | Area | Status | O que existe | O que falta | Prioridade |
|---|---|---|---|---|---|
| MC-CALC-001 | Participantes | Parcial | CPF/CNPJ emitente/destinatario e IE destinatario. | Definir todos os campos obrigatorios por modelo, UF e operacao. | P0 |
| MC-CALC-002 | Item fiscal | Parcial | Produto, NCM, CFOP, CST/CSOSN, aliquotas, rateios e IPI. | Definir obrigatoriedade final por modelo e regime. | P0 |
| MC-CALC-003 | CFOP NFC-e | Completo no material para lista extraida | Lista de CFOPs permitidos. | Homologar se lista e fixa, parametrizavel ou vigente por UF/regime. | P0 |
| MC-CALC-004 | CSOSN NFC-e | Completo no material para lista extraida | Lista de CSOSNs permitidos. | Homologar matriz completa por CFOP, regime e excecoes. | P0 |
| MC-CALC-005 | CST ICMS NFC-e | Completo no material para lista extraida | Lista de CSTs permitidos. | Homologar matriz completa por CFOP, regime e excecoes. | P0 |
| MC-CALC-006 | Matriz CFOP x CSOSN | Parcial | Matrizes comprovadas para codigos citados. | Extrair ou homologar combinacoes permitidas completas. | P0 |
| MC-CALC-007 | Matriz CFOP x CST | Parcial | Matrizes comprovadas para codigos citados. | Extrair ou homologar combinacoes permitidas completas. | P0 |
| MC-CALC-008 | ICMS CST | Parcial | Cobertura por CST 00, 10, 20, 20 SAT, 30, 40, 41, 50, 51, 53, 60, 61, 70 e 90. | Definir formulas, arredondamento, reducao, desoneracao e ST por CST. | P0 |
| MC-CALC-009 | ICMS CSOSN | Parcial | Cobertura por CSOSN 101, 102, 103, 201, 202, 203, 300, 400, 500 e 900. | Definir formulas, creditos, ST e mensagens por CSOSN. | P0 |
| MC-CALC-010 | PIS | Parcial | Cobertura por CST 01, 02, 03, 04, 05, 06, 07, 08, 09, 49 e 99. | Definir formula completa por CST e criterios de base. | P1 |
| MC-CALC-011 | COFINS | Parcial | Cobertura por CST 01, 02, 03, 04, 05, 06, 07, 08, 09, 49 e 99. | Definir formula completa por CST e criterios de base. | P1 |
| MC-CALC-012 | IPI | Parcial | Cobertura por CST 00, 01, 02, 03, 04, 05, 49, 50, 51, 52, 53, 54, 55 e 99. | Definir formula completa por CST, enquadramento e reducao. | P1 |
| MC-CALC-013 | IBS/CBS | Parcial | CST 000, 200, 410 e 510. | Definir formulas, vigencia, bases, reducoes e coexistencia com tributos atuais. | P0 |
| MC-CALC-014 | ISS | Parcial | Calculo ISS NFS-e comprovado. | Definir regras municipais, retencao, aliquotas, reducoes e arredondamento. | P0 |
| MC-CALC-015 | IBPT | Parcial | Aplicacao por NCM/UF no item. | Definir versao, vigencia, fallback e divergencia com tabela ausente. | P1 |
| MC-CALC-016 | Rateio frete | Parcial | Teste de rateio proporcional comprovado. | Definir base proporcional, arredondamento e residuo. | P0 |
| MC-CALC-017 | Rateio desconto/seguro/acrescimo/outros | Parcial | Operacoes de rateio comprovadas. | Definir criterio, prioridade e tratamento de residuo. | P0 |
| MC-CALC-018 | Retorno de erros | Parcial | Erro funcional bloqueia transmissao. | Definir catalogo de mensagens, codigos e severidade. | P1 |
| MC-CALC-019 | Persistencia do resultado | Pendente | Nao informado no material. | Definir se resultados sao persistidos, recalculados ou gravados apenas no documento. | P1 |
| MC-CALC-020 | Testes fiscais | Parcial | Testes indicados para CST 10, ICMS CST 00, rateio frete e IBPT. | Criar massa fiscal completa por imposto, CST, CSOSN, modelo e UF. | P0 |

## 4. Decisoes pendentes

| Codigo | Decisao | Motivo |
|---|---|---|
| D-CALC-001 | Definir matriz homologada completa CFOP x CST x CSOSN por modelo/regime/UF. | Evita rejeicoes fiscais e divergencia de regra. |
| D-CALC-002 | Definir formulas completas por CST/CSOSN de ICMS, PIS, COFINS, IPI, IBS/CBS e ISS. | Necessario para testes e homologacao. |
| D-CALC-003 | Definir arredondamento, casas decimais e tratamento de residuo de rateio. | Evita divergencias de centavos. |
| D-CALC-004 | Definir persistencia do resultado do motor. | Define auditoria e rastreabilidade do calculo. |
| D-CALC-005 | Definir comportamento com tabela IBPT ausente ou vencida. | Necessario para emissao com tributos aproximados. |
| D-CALC-006 | Definir prioridade entre tipo de operacao, tributacao NCM e regra manual do item. | Evita conflito de origem de regra fiscal. |

## 5. Proximo passo operacional

O proximo documento especifico da fila macro e `EF_SPED_EFD`, avaliando se ha conteudo suficiente para EF implantavel; se nao houver, registrar completude parcial sem inventar regras.
