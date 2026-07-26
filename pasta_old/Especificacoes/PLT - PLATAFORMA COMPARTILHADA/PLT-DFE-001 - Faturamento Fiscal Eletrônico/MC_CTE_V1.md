# MC_CTE_V1

## 1. Identificacao

| Campo | Conteudo |
|---|---|
| Empresa | Siser |
| Sistema | Epros |
| Modulo | PLATAFORMA_COMPARTILHADA |
| Submodulo | FATURAMENTO_FISCAL_ELETRONICO |
| Documento | Matriz de completude - CT-e |
| Versao | V1 |
| Status | Concluido |
| Data | 2026-06-07 |

## 2. Resumo de completude

| Area | Status |
|---|---|
| Habilitacao do modulo | Parcial |
| Permissoes basicas | Parcial |
| Estados e transmissao | Parcial |
| Referencia a NF-e | Parcial |
| Importacao XML | Parcial |
| Componentes e medidas | Parcial |
| Modelo fiscal completo CT-e | Incompleto |
| XML/PDF/protocolo | Incompleto |
| Cancelamento/eventos | Incompleto |
| Integracao MDF-e | Incompleto |

## 3. Matriz de completude

| Item | Capacidade esperada | Status | Evidencia disponivel | O que falta construir/definir | Prioridade |
|---|---|---|---|---|---|
| MC-CTE-001 | Habilitacao do modulo CT-e | Parcial | Material informa que menu/operacao exige modulo CT-e habilitado. | Definir onde fica a configuracao, escopo por empresa/plano e auditoria. | P0 |
| MC-CTE-002 | Permissao de visualizacao | Parcial | Permissao `cte.view` comprovada. | Definir telas, APIs, relatorios e escopo por filial/empresa. | P0 |
| MC-CTE-003 | Permissao de criacao | Parcial | Permissao `cte.create` comprovada. | Definir campos obrigatorios e estados permitidos. | P0 |
| MC-CTE-004 | Permissao de atualizacao | Parcial | Permissao `cte.update` comprovada. | Definir bloqueio de edicao por estado fiscal. | P0 |
| MC-CTE-005 | Permissao de exclusao | Parcial | Permissao `cte.delete` comprovada. | Definir restricoes para CT-e aprovado, transmitido ou vinculado a MDF-e. | P0 |
| MC-CTE-006 | Estados CT-e | Parcial | `DISPONIVEL`, `REJEITADO` e `APROVADO`; transmissao de disponivel/rejeitado para aprovado. | Definir demais estados, rejeicoes, cancelado, denegado, inutilizado e contingencia. | P0 |
| MC-CTE-007 | Chave CT-e | Parcial | Campo chave comprovado. | Definir obrigatoriedade, tamanho, validacao e unicidade. | P0 |
| MC-CTE-008 | Numero CT-e | Parcial | Campo cte_numero comprovado. | Definir tipo, serie, sequencia, ambiente e unicidade. | P0 |
| MC-CTE-009 | Referencia a NF-e transportada | Parcial | Campo chave_nfe comprovado. | Definir obrigatoriedade, validacao de 44 digitos, existencia da NF-e e multiplas referencias. | P0 |
| MC-CTE-010 | Tomador | Parcial | Material cita tomador. | Definir modelo completo do tomador, documento, IE, endereco, municipio, papel e obrigatoriedade. | P0 |
| MC-CTE-011 | Municipios | Parcial | Material cita municipios. | Definir municipio inicial/final, carregamento/descarregamento, UF, IBGE e rotas. | P0 |
| MC-CTE-012 | Componentes CT-e | Parcial | Filhos `componente_ctes` comprovados. | Definir campos, valores, tipos, totais, obrigatoriedade e relacao fiscal. | P1 |
| MC-CTE-013 | Medidas CT-e | Parcial | Filhos `medida_ctes` comprovados. | Definir campos, unidades, quantidade, peso/volume e validacoes. | P1 |
| MC-CTE-014 | Importacao XML CT-e | Parcial | Operacao de importacao XML comprovada. | Definir layout aceito, armazenamento, validacao, duplicidade, retorno e reprocessamento. | P0 |
| MC-CTE-015 | Emissao completa CT-e | Incompleto | Apenas transmissao e estados parciais. | Definir payload completo, participantes, modal, carga, valores, impostos, CFOP, natureza e certificado. | P0 |
| MC-CTE-016 | Autorizacao fiscal | Incompleto | Material nao informa protocolo, retorno ou codigos. | Definir comunicacao, protocolo, XML autorizado, rejeicoes, consulta e contingencia. | P0 |
| MC-CTE-017 | Cancelamento CT-e | Incompleto | Nao informado no material. | Definir regra, prazo, justificativa, protocolo, XML/PDF e efeitos. | P0 |
| MC-CTE-018 | Eventos CT-e | Incompleto | Nao informado no material. | Definir carta de correcao, desacordo, inutilizacao, encerramento quando aplicavel e demais eventos. | P1 |
| MC-CTE-019 | XML/PDF | Incompleto | Apenas importacao XML e diretorio de XML citado em material macro. | Definir XML envio, XML retorno, PDF/DACTE, caminhos, retencao e download. | P0 |
| MC-CTE-020 | Integracao MDF-e | Incompleto | CT-e e MDF-e aparecem como documentos relacionados. | Definir como CT-e alimenta MDF-e, descargas e encerramento. | P1 |
| MC-CTE-021 | Integracao financeira | Incompleto | Nao informado no material. | Definir faturamento, contas a receber/pagar, valores de frete e cancelamento. | P1 |
| MC-CTE-022 | Cadastros fiscais | Parcial | Classificacao tributaria possui indicador CT-e e CT-e OS. | Definir filtros, impostos, CFOP, CST e NCM aplicaveis ao CT-e. | P1 |
| MC-CTE-023 | Seguranca e auditoria | Incompleto | Permissoes basicas comprovadas. | Definir autenticacao, tenant, trilha de auditoria e logs de transmissao/importacao. | P0 |
| MC-CTE-024 | CT-e OS | Parcial | Indicador CT-e OS aparece em classificacao tributaria. | Definir se CT-e OS entra no escopo, campos e diferencas funcionais. | P2 |

## 4. Decisoes pendentes

| Decisao | Pergunta | Impacto |
|---|---|---|
| D-CTE-001 | CT-e entra como emissao completa nesta fase ou apenas importacao/controle? | Define escopo de construcao. |
| D-CTE-002 | Qual sera o modelo fisico completo de `ctes`, componentes e medidas? | Define banco, telas e API. |
| D-CTE-003 | Quais eventos CT-e serao suportados no Epros? | Define cancelamento, correcao, desacordo e integracoes fiscais. |
| D-CTE-004 | Como CT-e se relaciona com MDF-e no fluxo logistico? | Define integracao operacional. |
| D-CTE-005 | CT-e OS faz parte do mesmo produto nesta fase? | Define escopo tributario e tela. |

## 5. Proximo passo

O proximo documento especifico da fila macro e `EF_MDFE`, detalhando MDF-e conforme material disponivel.
