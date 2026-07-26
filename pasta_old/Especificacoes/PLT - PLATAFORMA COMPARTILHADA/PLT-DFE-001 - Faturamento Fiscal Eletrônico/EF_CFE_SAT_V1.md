# EF_CFE_SAT_V1

## 1. Identificacao

| Campo | Conteudo |
|---|---|
| Empresa | Siser |
| Sistema | Epros |
| Modulo | PLATAFORMA_COMPARTILHADA |
| Submodulo | FATURAMENTO_FISCAL_ELETRONICO |
| Documento | Especificacao funcional - CF-e/SAT |
| Versao | V1 |
| Status | Concluido |
| Data | 2026-06-08 |

## 2. Objetivo funcional

O CF-e/SAT permite ao Epros controlar cupom fiscal eletronico presencial, com modelo fiscal 59, emissao a partir de venda/PDV, status separado, parametros por empresa, processamento dedicado, utilitario operacional, relatorios e vinculacao ao movimento fiscal.

Esta EF consolida somente o conteudo comprovado no material canonico. Equipamento SAT, assinatura, cancelamento, XML completo, extrato, ativacao, comunicacao fisica e regras fiscais detalhadas ficam registradas na MC.

## 3. Escopo

| Area | Incluso | Status |
|---|---|---|
| Modelo fiscal | CF-e = modelo 59 | Com conteudo |
| Movimento fiscal | Ambiente, ModeloDocumento, numero, protocolo, chave, serie, status, datas e XMLs | Parcial |
| Status CF-e/SAT | Status separado de CF-e/SAT | Parcial |
| Situacao/sessao/status CF-e | Controle no dominio fiscal | Parcial |
| Parametros | Parametros NFe/NFC-e/SAT por empresa | Parcial |
| Emissao | CF-e emitido a partir de venda/PDV | Parcial |
| Relatorios | Relatorio CF-e e relatorios SAT citados | Parcial |
| Utilitario | Utilitario CF-e | Parcial |
| Processamento dedicado | SAT processado em componente dedicado | Parcial |
| Calculo tributario SAT | Calculo ICMS CST 20 SAT citado no motor | Parcial |
| XML | Pastas de ciclo de vida XML e extensoes XML citadas | Parcial |
| Equipamento SAT | Ativacao, configuracao, numero de serie, comunicacao e assinatura | Incompleto |
| Cancelamento CF-e/SAT | Evento, prazo, XML e retorno | Incompleto |

## 4. Fora de escopo

| Item | Motivo |
|---|---|
| PDV completo | Esta EF descreve a emissao fiscal CF-e a partir de venda/PDV; o PDV completo pertence ao modulo dono. |
| NFC-e | Possui EF especifica. |
| NF-e saida | Possui EF especifica. |
| Sintegra | Possui EF especifica na fila macro. |
| Motor tributario completo | Possui EF especifica na fila macro. |
| Parametros fiscais gerais | Possuem EF especifica; aqui ficam somente impactos CF-e/SAT. |

## 5. Atores e responsabilidades

| Ator | Responsabilidade | Observacao |
|---|---|---|
| Operador PDV | Emitir CF-e a partir da venda/PDV. | Permissoes finais nao informadas no material. |
| Usuario fiscal | Consultar status, relatorios, XML e utilitario CF-e. | Telas/relatorios comprovados de forma parcial. |
| Administrador Siser | Configurar parametros por empresa e suporte SAT. | Parametros especificos ainda incompletos. |
| Epros | Controlar modelo 59, status, movimento fiscal, XMLs e relatorios. | Equipamento e comunicacao final ficam na MC. |

## 6. Conceitos funcionais

| Conceito | Definicao |
|---|---|
| CF-e | Cupom Fiscal Eletronico. |
| SAT | Processamento/dispositivo fiscal associado ao CF-e conforme material. |
| Modelo 59 | Codigo fiscal do CF-e. |
| Status CF-e | Status separado para controle do cupom fiscal eletronico. |
| Movimento fiscal | Registro fiscal vinculado a venda, ambiente, modelo, chave, protocolo, status e XMLs. |
| Utilitario CF-e | Rotina operacional de suporte ao CF-e. |
| Relatorio CF-e | Consulta/relatorio fiscal dedicado ao CF-e. |

## 7. Capacidades funcionais

| Capacidade | Descricao | Entrada principal | Saida esperada |
|---|---|---|---|
| Emitir CF-e por venda/PDV | Gera CF-e a partir de venda presencial. | Venda/PDV | Movimento fiscal CF-e processado. |
| Controlar modelo fiscal | Classifica documento como modelo 59. | ModeloDocumento | Documento identificado como CF-e. |
| Controlar status CF-e | Mantem status proprio de CF-e/SAT separado. | Movimento/status | Status consultavel. |
| Alterar status CF-e | Atualiza status conforme rotina fiscal. | Documento e novo status | Status alterado. |
| Manter parametros SAT por empresa | Configura parametros fiscais relacionados a SAT por empresa. | Empresa | Parametros disponiveis. |
| Processar SAT em rotina dedicada | Usa processamento dedicado para SAT. | Documento CF-e | Processamento fiscal executado. |
| Consultar relatorio CF-e | Exibe documentos/posicao CF-e. | Periodo/filtros nao informados | Relatorio gerado. |
| Operar utilitario CF-e | Executa rotinas de suporte CF-e. | Acao utilitaria | Resultado funcional. |
| Preservar XML | Mantem XML de envio, retorno autorizacao e retorno cancelamento quando aplicavel. | Movimento fiscal | XML preservado. |

## 8. Regras funcionais

| Regra | Descricao | Contexto | Resultado esperado | Severidade | Fonte funcional |
|---|---|---|---|---|---|
| REG-CFE-001 | CF-e deve usar modelo fiscal 59. | Identificacao fiscal | Classificar documento como CF-e. | Bloqueante | Modelo 59 comprovado. |
| REG-CFE-002 | Movimento fiscal deve suportar ModeloDocumento com CF-e. | Movimento fiscal | Permitir registro CF-e. | Bloqueante | Movimento fiscal possui modelo NFe/NFCe/CFe. |
| REG-CFE-003 | CF-e deve ser emitido a partir de venda/PDV. | Emissao | Gerar documento fiscal a partir da venda. | Alta | Emissao por venda/PDV comprovada. |
| REG-CFE-004 | CF-e/SAT deve possuir status separado. | Ciclo de vida | Status CF-e nao deve ser confundido com demais documentos. | Alta | Status separado comprovado. |
| REG-CFE-005 | Situacao, sessao e status de CF-e devem ser controlados no dominio fiscal. | Ciclo de vida | Preservar controles especificos. | Alta | Controle comprovado. |
| REG-CFE-006 | O status do CF-e deve poder ser alterado por rotina fiscal. | Atualizacao de status | Atualizar status quando a rotina permitir. | Alta | Alteracao de status comprovada. |
| REG-CFE-007 | Parametros fiscais por empresa devem contemplar SAT quando aplicavel. | Parametrizacao | Permitir configuracao por empresa. | Alta | Parametros NFe/NFC-e/SAT comprovados. |
| REG-CFE-008 | SAT deve ser processado por rotina dedicada. | Processamento | Encaminhar CF-e/SAT ao processamento especifico. | Alta | Processamento dedicado comprovado. |
| REG-CFE-009 | CF-e deve possuir relatorios dedicados. | Relatorios | Exibir relatorio CF-e/SAT. | Media | Relatorios comprovados. |
| REG-CFE-010 | CF-e deve possuir utilitario operacional. | Suporte | Disponibilizar rotina utilitaria. | Media | Utilitario comprovado. |
| REG-CFE-011 | Movimento fiscal CF-e deve preservar PedidoID quando originado de venda. | Origem | Rastrear venda geradora. | Alta | Movimento fiscal possui PedidoID. |
| REG-CFE-012 | Movimento fiscal CF-e deve preservar Numero, Protocolo, Chave, Serie e Status quando informados. | Persistencia fiscal | Documento fiscal rastreavel. | Alta | Campos comprovados. |
| REG-CFE-013 | Movimento fiscal CF-e deve preservar Ambiente. | Persistencia fiscal | Separar homologacao/producao quando aplicavel. | Alta | Campo comprovado. |
| REG-CFE-014 | Movimento fiscal CF-e deve preservar datas de recebimento, emissao e cancelamento quando informadas. | Auditoria fiscal | Datas consultaveis. | Media | Campos comprovados. |
| REG-CFE-015 | Movimento fiscal CF-e deve preservar motivo fiscal quando informado. | Retorno fiscal | Permitir diagnostico. | Media | Campo XMotivo comprovado. |
| REG-CFE-016 | Movimento fiscal CF-e deve preservar XML de envio, retorno de autorizacao e retorno de cancelamento quando informados. | XML | XMLs consultaveis. | Alta | Campos comprovados. |
| REG-CFE-017 | Movimento fiscal CF-e deve suportar contingencia quando aplicavel. | Operacao fiscal | Preservar indicador. | Media | Campo Contigencia comprovado. |
| REG-CFE-018 | Calculo tributario deve contemplar regra SAT citada para ICMS CST 20. | Calculo | Encaminhar detalhe ao motor tributario. | Media | Calculo SAT citado no material. |
| REG-CFE-019 | A EF nao deve assumir equipamento, ativacao, assinatura, cancelamento ou XML completo quando nao informados. | Especificacao | Encaminhar para MC. | Bloqueante | Material parcial. |

## 9. Estados e situacoes

| Situacao | Descricao | Observacao |
|---|---|---|
| Status CF-e | Status especifico do CF-e/SAT. | Dominio final nao informado no material. |
| Situacao CF-e | Situacao fiscal do CF-e. | Dominio final nao informado no material. |
| Sessao CF-e | Sessao operacional do CF-e. | Dominio final nao informado no material. |
| Processado | Processamento dedicado SAT executado. | Dominio final nao informado no material. |
| Cancelado | Data/XML de cancelamento existem no movimento fiscal quando informados. | Regra de cancelamento nao informada. |

## 10. Modelo de dados funcional e implantavel

O material comprova uso de movimento fiscal com modelo CF-e, status separado de CF-e/SAT, parametros SAT/CFe e rotinas de relatorio/utilitario. Como nao ha tabela completa propria de CF-e/SAT neste recorte, a EF organiza um modelo funcional com `movimento_fiscal_cfe`, `status_cfe`, `parametros_cfe_sat`, `cfe_relatorio` e `cfe_utilitario`, preservando campos comprovados e marcando como lacuna o detalhamento fisico final.[^1]

| Entidade funcional | Finalidade | Cardinalidade | Persistencia indicada |
|---|---|---|---|
| movimento_fiscal_cfe | Registrar documento CF-e no movimento fiscal. | 1 por CF-e | Parcialmente comprovada em MovimentoFiscal. |
| status_cfe | Controlar status/situacao/sessao CF-e. | 1..N por CF-e/status | Comprovada como Status_CFe. |
| parametros_cfe_sat | Parametros SAT por empresa. | 0..1 por empresa/contexto | Parcialmente comprovada. |
| cfe_relatorio | Consultas e relatorios CF-e/SAT. | Nao informado no material | Consolidacao funcional.[^1] |
| cfe_utilitario | Rotinas utilitarias CF-e. | Nao informado no material | Consolidacao funcional.[^1] |

### 10.1 Relacionamentos funcionais

| Origem | Relacao | Destino | Regra |
|---|---|---|---|
| Venda/PDV | gera | movimento_fiscal_cfe | CF-e e emitido a partir de venda/PDV. |
| movimento_fiscal_cfe | possui | status_cfe | Status separado controla ciclo CF-e/SAT. |
| parametros_cfe_sat | habilita | movimento_fiscal_cfe | Parametros por empresa sustentam processamento SAT. |
| movimento_fiscal_cfe | alimenta | cfe_relatorio | Relatorios exibem documentos/status CF-e. |
| movimento_fiscal_cfe | pode usar | cfe_utilitario | Utilitario executa suporte operacional. |

## 11. Dicionario de dados implantavel

### 11.1 movimento_fiscal_cfe

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| Id | Identificador | Nao informado no material | Sim | Chave primaria | Identificador interno.[^1] |
| PedidoID | Numero/identificador | Nao informado no material | Nao informado no material | Venda/PDV | Origem de venda. |
| Numero | Texto/numero | Nao informado no material | Nao informado no material | Numero fiscal | Campo comprovado. |
| Protocolo | Texto | Nao informado no material | Nao informado no material | Protocolo fiscal | Campo comprovado. |
| Chave | Texto | Nao informado no material | Nao informado no material | Chave fiscal | Campo comprovado. |
| Serie | Texto/numero | Nao informado no material | Nao informado no material | Serie fiscal | Campo comprovado. |
| Status | Texto/enum | Nao informado no material | Nao informado no material | Status fiscal | Campo comprovado. |
| Ambiente | Enum/texto | Homologacao/producao quando aplicavel | Nao informado no material | Ambiente fiscal | Campo comprovado. |
| ModeloDocumento | Enum/codigo | CFe=59 | Sim | Modelo fiscal | Modelo 59. |
| DataRecebimento | Data/hora | Nao informado no material | Nao | Auditoria fiscal | Campo comprovado. |
| DataEmissao | Data/hora | Nao informado no material | Nao | Emissao | Campo comprovado. |
| DataCancelamento | Data/hora | Nao informado no material | Nao | Cancelamento | Campo comprovado; regra final nao informada. |
| XMotivo | Texto | Nao informado no material | Nao | Motivo fiscal | Campo comprovado. |
| XmlEnvio | Texto/binario | XML | Nao informado no material | XML | Campo comprovado. |
| XmlRetornoAutorizacao | Texto/binario | XML | Nao informado no material | XML | Campo comprovado. |
| XmlRetornoCancelamento | Texto/binario | XML | Nao informado no material | Nao | XML | Campo comprovado; cancelamento final na MC. |
| Contigencia | Booleano | Sim/Nao | Nao informado no material | Operacao fiscal | Campo comprovado. |

### 11.2 status_cfe

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| Id | Identificador | Nao informado no material | Sim | Chave primaria | Identificador interno.[^1] |
| MovimentoFiscalCfeId | Identificador | Nao informado no material | Sim | Relacao com movimento_fiscal_cfe | Vinculo com CF-e.[^1] |
| SituacaoCfe | Texto/enum | Nao informado no material | Nao informado no material | Situacao | Dominio final nao informado. |
| SessaoCfe | Texto/enum | Nao informado no material | Nao informado no material | Sessao | Dominio final nao informado. |
| StatusCfe | Texto/enum | Nao informado no material | Nao informado no material | Status separado | Status CF-e/SAT separado. |
| DataAlteracao | Data/hora | Nao informado no material | Nao informado no material | Auditoria | Campo funcional necessario; estrutura final nao informada.[^1] |

### 11.3 parametros_cfe_sat

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| Id | Identificador | Nao informado no material | Sim | Chave primaria | Identificador interno.[^1] |
| EmpresaId | Identificador | Nao informado no material | Sim | Empresa | Parametros por empresa. |
| ModeloDocumento | Enum/codigo | CFe=59 | Nao informado no material | Modelo fiscal | Parametro deve reconhecer CF-e. |
| ParametrosSat | Estrutura/texto | Nao informado no material | Nao informado no material | Parametrizacao | Material comprova parametros SAT, sem detalhar campos. |
| Ativo | Booleano | Sim/Nao | Nao informado no material | Controle | Campo funcional; estrutura final nao informada.[^1] |

### 11.4 cfe_relatorio

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| Id | Identificador | Nao informado no material | Sim | Chave primaria | Identificador interno.[^1] |
| PeriodoInicial | Data | Nao informado no material | Nao informado no material | Filtro | Filtro funcional de relatorio.[^1] |
| PeriodoFinal | Data | Nao informado no material | Nao informado no material | Filtro | Filtro funcional de relatorio.[^1] |
| StatusCfe | Texto/enum | Nao informado no material | Nao | Filtro | Relatorio pode filtrar status quando definido.[^1] |
| Resultado | Estrutura | Nao informado no material | Nao informado no material | Saida | Layout final nao informado.[^1] |

### 11.5 cfe_utilitario

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| Id | Identificador | Nao informado no material | Sim | Chave primaria | Identificador interno.[^1] |
| AcaoUtilitaria | Texto/enum | Nao informado no material | Sim | Acao | Material comprova utilitario, nao detalha acoes. |
| Resultado | Texto/estrutura | Nao informado no material | Nao informado no material | Retorno | Retorno final nao informado.[^1] |
| DataExecucao | Data/hora | Nao informado no material | Nao informado no material | Auditoria | Estrutura final nao informada.[^1] |

## 12. Fluxos funcionais

### 12.1 Emitir CF-e a partir de venda/PDV

| Passo | Responsavel | Acao | Entrada | Saida |
|---|---|---|---|---|
| 1 | Operador PDV | Finaliza venda com emissao CF-e. | Venda/PDV | Pedido de emissao. |
| 2 | Epros | Identifica modelo fiscal 59. | Parametros e venda | Documento classificado como CF-e. |
| 3 | Epros | Processa SAT em rotina dedicada. | Dados fiscais | Retorno de processamento. |
| 4 | Epros | Grava movimento fiscal. | Numero, chave, serie, status, XMLs quando disponiveis | CF-e registrado. |
| 5 | Epros | Atualiza status CF-e. | Retorno/status | Status separado atualizado. |

### 12.2 Alterar status CF-e

| Passo | Responsavel | Acao | Entrada | Saida |
|---|---|---|---|---|
| 1 | Usuario fiscal/Epros | Solicita alteracao de status. | CF-e e status | Alteracao iniciada. |
| 2 | Epros | Valida rotina permitida. | Situacao/sessao/status | Alteracao permitida ou bloqueada. |
| 3 | Epros | Registra novo status. | Status | Historico/status atualizado.[^1] |

### 12.3 Consultar relatorio CF-e/SAT

| Passo | Responsavel | Acao | Entrada | Saida |
|---|---|---|---|---|
| 1 | Usuario fiscal | Solicita relatorio CF-e/SAT. | Filtros nao informados | Consulta iniciada. |
| 2 | Epros | Consulta movimentos CF-e. | Modelo 59/status/periodo quando definido | Resultado do relatorio. |
| 3 | Epros | Exibe resultado. | Dados fiscais | Relatorio disponivel. |

### 12.4 Executar utilitario CF-e

| Passo | Responsavel | Acao | Entrada | Saida |
|---|---|---|---|---|
| 1 | Usuario fiscal/suporte | Seleciona acao utilitaria. | Acao CF-e | Acao iniciada. |
| 2 | Epros | Executa rotina utilitaria. | Parametros nao informados | Resultado retornado. |
| 3 | Epros | Registra resultado funcional. | Resultado | Registro operacional.[^1] |

## 13. Validacoes e mensagens

| Codigo | Mensagem | Condicao |
|---|---|---|
| MSG-CFE-001 | Modelo fiscal CF-e deve ser 59. | Documento CF-e com modelo diferente. |
| MSG-CFE-002 | Parametros SAT da empresa nao informados. | Emissao sem parametrizacao suficiente. |
| MSG-CFE-003 | Status CF-e nao informado. | Alteracao/consulta sem status definido. |
| MSG-CFE-004 | Venda/PDV de origem nao informada. | Emissao CF-e sem origem. |
| MSG-CFE-005 | Processamento SAT nao retornou resultado. | Falha de processamento dedicado. |
| MSG-CFE-006 | XML CF-e nao disponivel. | Consulta/download de XML sem arquivo/registro. |

## 14. Integracoes

| Integracao | Direcao | Dados | Regra | Lacuna |
|---|---|---|---|---|
| PDV/Vendas | Entrada | Venda/PDV, PedidoID | CF-e nasce da venda/PDV. | Contrato completo com PDV. |
| Parametros fiscais | Entrada | Parametros por empresa, modelo 59, SAT | Necessario para emissao. | Campos SAT finais. |
| Movimento fiscal | Saida | Numero, chave, protocolo, serie, status, datas, XMLs | Registro fiscal do CF-e. | Tabela final propria ou compartilhada. |
| Motor tributario | Entrada/Saida | ICMS CST 20 SAT e demais impostos | Calculo SAT citado. | EF de motor tributario detalha regras. |
| Relatorios | Saida | Dados CF-e/SAT | Relatorios dedicados. | Layout e filtros finais. |

## 15. Permissoes e seguranca

| Controle | Regra |
|---|---|
| Emissao CF-e | Permissao final nao informada no material. |
| Relatorio CF-e | Permissao final nao informada no material. |
| Utilitario CF-e | Deve ser restrito a usuario fiscal/suporte autorizado; matriz final nao informada. |
| XML | Acesso deve respeitar permissao fiscal e contexto de empresa. |
| Equipamento SAT | Seguranca de equipamento/comunicacao nao informada no material. |

## 16. Relatorios e consultas

| Consulta | Filtros comprovados | Resultado |
|---|---|---|
| Relatorio CF-e | Nao informado no material | Documentos/status CF-e. |
| Relatorio SAT | Nao informado no material | Dados SAT/CF-e. |
| Consulta movimento CF-e | Modelo 59, status quando definido | Movimento fiscal CF-e. |
| Utilitario CF-e | Acao utilitaria nao informada | Resultado operacional. |

## 17. Criterios de aceite

| Codigo | Criterio |
|---|---|
| CA-CFE-001 | Documento CF-e deve usar modelo fiscal 59. |
| CA-CFE-002 | Emissao CF-e deve partir de venda/PDV. |
| CA-CFE-003 | CF-e deve possuir status separado. |
| CA-CFE-004 | Alteracao de status CF-e deve atualizar registro de status. |
| CA-CFE-005 | Parametros por empresa devem reconhecer SAT/CF-e. |
| CA-CFE-006 | Movimento fiscal CF-e deve preservar numero, protocolo, chave, serie, status, ambiente, modelo e XMLs quando informados. |
| CA-CFE-007 | Relatorio CF-e/SAT deve consultar dados fiscais do CF-e. |
| CA-CFE-008 | Utilitario CF-e deve registrar resultado operacional. |
| CA-CFE-009 | Campos nao informados no material nao devem ser preenchidos por suposicao na EF. |

## 18. Lacunas encaminhadas para MC

| Lacuna | Impacto |
|---|---|
| Equipamento SAT, ativacao e comunicacao | Necessario para emissao real. |
| Parametros SAT completos | Necessario para configuracao por empresa. |
| Status/situacao/sessao finais | Necessario para ciclo de vida implantavel. |
| Cancelamento CF-e/SAT | Necessario para operacao fiscal completa. |
| XML completo, assinatura, armazenamento e extrato | Necessario para evidencia fiscal. |
| Permissoes finais de emissao, utilitario e relatorio | Necessario para seguranca. |
| Contrato com PDV/Vendas | Necessario para implantacao ponta a ponta. |
| Regras tributarias SAT completas | Necessario para calculo correto. |

## 19. Proximo passo

O proximo documento especifico da fila macro e `EF_XML_CONTADOR_DOWNLOADS`, detalhando XML contador e downloads fiscais conforme material disponivel.

[^1]: Consolidacao funcional criada para tornar implantavel a especificacao, pois o material comprova modelo 59, MovimentoFiscal, Status_CFe, parametros SAT/CFe, emissao por venda/PDV, relatorios/utilitario e processamento dedicado, mas nao informa tabela final completa, equipamento, campos SAT, status finais, XML completo ou contrato de cancelamento.
