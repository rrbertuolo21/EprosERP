# MC_MDFE_V1

## 1. Identificacao

| Campo | Conteudo |
|---|---|
| Empresa | Siser |
| Sistema | Epros |
| Modulo | PLATAFORMA_COMPARTILHADA |
| Submodulo | FATURAMENTO_FISCAL_ELETRONICO |
| Documento | Matriz de completude - MDF-e |
| Versao | V1 |
| Status | Concluido |
| Data | 2026-06-07 |

## 2. Resumo de completude

| Area | Status |
|---|---|
| Consulta de nao encerrados | Parcial |
| Encerramento | Parcial |
| Flag encerrado | Parcial |
| Identificacao MDF-e | Parcial |
| Estruturas logisticas filhas | Parcial |
| Permissoes | Parcial |
| Modelo fiscal completo | Incompleto |
| XML/PDF/protocolo completo | Incompleto |
| Eventos complementares | Incompleto |
| Integracoes logisticas/fiscais | Incompleto |

## 3. Matriz de completude

| Item | Capacidade esperada | Status | Evidencia disponivel | O que falta construir/definir | Prioridade |
|---|---|---|---|---|---|
| MC-MDFE-001 | Permissoes MDF-e | Parcial | Material informa permissoes `mdfe.*` analogas ao CT-e. | Definir permissoes finais de visualizar, criar, atualizar, excluir, consultar e encerrar. | P0 |
| MC-MDFE-002 | Consulta de nao encerrados | Parcial | Operacao de consulta de nao encerrados comprovada. | Definir filtros, retorno, armazenamento, periodicidade e conciliacao. | P0 |
| MC-MDFE-003 | Encerramento | Parcial | Encerramento por chave, protocolo e localizacao comprovado. | Definir retorno, rejeicoes, idempotencia, permissao e auditoria. | P0 |
| MC-MDFE-004 | Flag encerrado | Parcial | Campo `encerrado` comprovado. | Definir regra de atualizacao, bloqueio de reencerramento e conciliacao. | P0 |
| MC-MDFE-005 | Identificacao MDF-e | Parcial | Estado, chave, mdfe_numero e protocolo comprovados. | Definir obrigatoriedade, formato, tamanho, serie, ambiente e unicidade. | P0 |
| MC-MDFE-006 | Dominio de estados | Incompleto | Campo estado comprovado. | Definir estados finais, autorizado, rejeitado, cancelado, encerrado e contingencia. | P0 |
| MC-MDFE-007 | Municipios de carregamento | Parcial | Filhos de municipios de carregamento comprovados. | Definir campos, IBGE, UF, obrigatoriedade e cardinalidade. | P1 |
| MC-MDFE-008 | Percursos | Parcial | Filhos de percursos comprovados. | Definir UF/trecho, ordem, obrigatoriedade e validacoes. | P1 |
| MC-MDFE-009 | CIOTs | Parcial | Filhos de CIOT comprovados. | Definir formato, participantes, obrigatoriedade e validacao. | P1 |
| MC-MDFE-010 | Vale pedagio | Parcial | Filhos de vale pedagio comprovados. | Definir campos, fornecedor, comprovante, valores e regras. | P1 |
| MC-MDFE-011 | Informacoes de descarga | Parcial | `info_descargas` comprovado. | Definir municipio descarga, ordem, documentos e regras. | P1 |
| MC-MDFE-012 | NF-e de descarga | Parcial | `n_fe_descargas` comprovado. | Definir chave, validacao, relacionamento com NF-e e duplicidade. | P1 |
| MC-MDFE-013 | CT-e de descarga | Parcial | `c_te_descargas` comprovado. | Definir chave, validacao, relacionamento com CT-e e duplicidade. | P1 |
| MC-MDFE-014 | Emissao completa MDF-e | Incompleto | Material nao informa payload completo. | Definir emitente, veiculo, condutor, carga, documentos, modal, valores e certificado. | P0 |
| MC-MDFE-015 | Autorizacao fiscal | Incompleto | Protocolo existe, mas retorno/autorizacao nao detalhados. | Definir envio, retorno, protocolo, XML autorizado, rejeicoes e consulta. | P0 |
| MC-MDFE-016 | XML/PDF/DAMDFE | Incompleto | Diretorio XML MDF-e citado no material macro, sem detalhes finais. | Definir XML envio, XML retorno, PDF, caminhos, downloads e retencao. | P0 |
| MC-MDFE-017 | Eventos MDF-e | Incompleto | Apenas encerramento esta comprovado. | Definir cancelamento, inclusao condutor, encerramento, consulta e demais eventos aplicaveis. | P1 |
| MC-MDFE-018 | Integracao CT-e | Parcial | CT-e de descarga comprovado. | Definir como CT-e alimenta MDF-e e como bloquear alteracoes depois de vinculado. | P1 |
| MC-MDFE-019 | Integracao NF-e | Parcial | NF-e de descarga comprovada. | Definir validacao e origem da NF-e relacionada. | P1 |
| MC-MDFE-020 | Integracao logistica | Incompleto | Ha percursos, CIOT, vale pedagio e descargas. | Definir contrato com transporte, veiculos, motoristas, rotas e carga. | P1 |
| MC-MDFE-021 | Auditoria | Incompleto | Necessaria para consulta/encerramento, nao informada. | Definir usuario, data/hora, origem, payload e retorno. | P0 |
| MC-MDFE-022 | Segregacao por tenant/empresa | Incompleto | Nao informado no material para MDF-e. | Definir isolamento, localizacao e permissoes por empresa/filial. | P0 |

## 4. Decisoes pendentes

| Decisao | Pergunta | Impacto |
|---|---|---|
| D-MDFE-001 | MDF-e sera emissao completa nesta fase ou apenas consulta/encerramento? | Define escopo de desenvolvimento. |
| D-MDFE-002 | Qual sera o modelo fisico completo de `mdves` e filhos? | Define banco e API. |
| D-MDFE-003 | Quais eventos MDF-e serao suportados alem do encerramento? | Define fiscal e suporte. |
| D-MDFE-004 | Como MDF-e se integra com CT-e, NF-e e logistica? | Define fluxo operacional. |
| D-MDFE-005 | Qual politica de XML/PDF/DAMDFE e retencao sera adotada? | Define evidencia fiscal. |

## 5. Proximo passo

O proximo documento especifico da fila macro e `EF_MANIFESTO_DFE`, detalhando Manifesto DFe conforme material disponivel.
