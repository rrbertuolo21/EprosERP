# MC_CFE_SAT_V1

## 1. Identificacao

| Campo | Conteudo |
|---|---|
| Empresa | Siser |
| Sistema | Epros |
| Modulo | PLATAFORMA_COMPARTILHADA |
| Submodulo | FATURAMENTO_FISCAL_ELETRONICO |
| Documento | Matriz de completude - CF-e/SAT |
| Versao | V1 |
| Status | Concluido |
| Data | 2026-06-08 |

## 2. Resumo de completude

| Area | Status |
|---|---|
| Modelo fiscal 59 | Parcial |
| Emissao por venda/PDV | Parcial |
| Movimento fiscal | Parcial |
| Status CF-e/SAT | Parcial |
| Parametros SAT por empresa | Parcial |
| Relatorios/utilitario | Parcial |
| Processamento dedicado SAT | Parcial |
| Equipamento SAT | Incompleto |
| Cancelamento CF-e/SAT | Incompleto |
| XML/extrato/armazenamento | Incompleto |

## 3. Matriz de completude

| Item | Capacidade esperada | Status | Evidencia disponivel | O que falta construir/definir | Prioridade |
|---|---|---|---|---|---|
| MC-CFE-001 | Modelo fiscal CF-e | Parcial | Modelo CFe=59 comprovado. | Definir dominio final em todos os cadastros e validacoes. | P0 |
| MC-CFE-002 | Emissao por venda/PDV | Parcial | Material informa CF-e a partir de venda/PDV. | Definir contrato completo com PDV, bloqueios, retorno e reemissao. | P0 |
| MC-CFE-003 | Movimento fiscal CF-e | Parcial | Movimento fiscal possui PedidoID, numero, protocolo, chave, serie, status, ambiente, modelo, datas e XMLs. | Definir tabela final, obrigatoriedade, indices e relacao com venda. | P0 |
| MC-CFE-004 | Status CF-e/SAT separado | Parcial | Status separado comprovado. | Definir dominio completo, transicoes, mensagens e auditoria. | P0 |
| MC-CFE-005 | Situacao/sessao/status CF-e | Parcial | Controle no dominio fiscal comprovado. | Definir nomes finais, regras de alteracao e persistencia. | P0 |
| MC-CFE-006 | Parametros SAT por empresa | Parcial | Parametros NFe/NFC-e/SAT por empresa comprovados. | Levantar campos SAT, obrigatoriedade, seguranca e validacoes. | P0 |
| MC-CFE-007 | Processamento dedicado SAT | Parcial | SAT processado em rotina dedicada. | Definir contrato tecnico-funcional, entradas, saidas, erros e timeout. | P0 |
| MC-CFE-008 | Relatorios CF-e/SAT | Parcial | Relatorio CF-e e relatorios SAT citados. | Definir filtros, colunas, exportacao, permissoes e auditoria. | P1 |
| MC-CFE-009 | Utilitario CF-e | Parcial | Utilitario CF-e comprovado. | Definir acoes utilitarias, permissoes, logs e riscos. | P1 |
| MC-CFE-010 | XML CF-e | Parcial | Movimento fiscal possui XML de envio, retorno autorizacao e retorno cancelamento. | Definir XML completo, armazenamento, assinatura, download e retencao. | P0 |
| MC-CFE-011 | Cancelamento CF-e/SAT | Incompleto | Data/XML de cancelamento aparecem no movimento fiscal, mas regra nao esta detalhada. | Definir prazo, evento, retorno, status e efeitos no PDV/financeiro. | P0 |
| MC-CFE-012 | Equipamento SAT | Incompleto | Material cita SAT, mas nao equipamento. | Definir ativacao, codigo, numero de serie, comunicacao, certificado e contingencia. | P0 |
| MC-CFE-013 | Extrato CF-e | Incompleto | Material cita relatorios, nao extrato final. | Definir impressao/extrato, layout, segunda via e armazenamento. | P1 |
| MC-CFE-014 | Permissoes | Incompleto | Permissoes finais nao informadas. | Definir matriz para emitir, consultar, cancelar, utilitario, relatorio e XML. | P0 |
| MC-CFE-015 | Calculo SAT | Parcial | Calculo ICMS CST 20 SAT citado. | Detalhar regras tributarias SAT no motor fiscal. | P1 |
| MC-CFE-016 | Reconciliacao XML/banco | Parcial | Material cita reconciliacao XML autorizado com banco no dominio fiscal. | Definir aplicabilidade CF-e, rotina, erros e reprocessamento. | P1 |
| MC-CFE-017 | Ambiente/contingencia | Parcial | Movimento fiscal possui Ambiente e Contigencia. | Definir regras especificas CF-e/SAT. | P1 |
| MC-CFE-018 | Relacao com parametros fiscais | Parcial | Parametros CF-e/SAT citados como lacuna em parametros fiscais. | Fechar campos e governanca entre EFs. | P0 |

## 4. Decisoes pendentes

| Decisao | Pergunta | Impacto |
|---|---|---|
| D-CFE-001 | CF-e/SAT sera emissao completa nesta fase ou apenas estrutura preparada? | Define escopo de desenvolvimento. |
| D-CFE-002 | Qual equipamento/contrato SAT sera suportado? | Define parametros, comunicacao e suporte. |
| D-CFE-003 | Qual sera o dominio final de Status_CFe, situacao e sessao? | Define ciclo de vida. |
| D-CFE-004 | Como CF-e/SAT se integra com PDV, vendas e financeiro? | Define fluxo ponta a ponta. |
| D-CFE-005 | Qual politica de XML/extrato/cancelamento sera adotada? | Define evidencia fiscal e suporte. |

## 5. Proximo passo

O proximo documento especifico da fila macro e `EF_XML_CONTADOR_DOWNLOADS`, detalhando XML contador e downloads fiscais conforme material disponivel.
