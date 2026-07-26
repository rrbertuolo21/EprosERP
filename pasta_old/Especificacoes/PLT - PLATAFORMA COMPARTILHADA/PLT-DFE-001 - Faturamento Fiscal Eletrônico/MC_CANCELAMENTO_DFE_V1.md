# Matriz de Completude - Epros

**Modulo:** PLATAFORMA_COMPARTILHADA  
**Submodulo:** FATURAMENTO_FISCAL_ELETRONICO  
**Capacidade:** CANCELAMENTO_DFE  
**Versao:** V1  
**Empresa:** Siser  

## 1. Objetivo

Registrar lacunas do cancelamento de documentos fiscais eletronicos para evoluir a especificacao sem inventar regras fiscais, prazos, protocolos ou efeitos integrados nao comprovados no material.

## 2. Resumo de completude

| Item | Status |
|---|---|
| Cancelamento NF-e | Parcial |
| Cancelamento NFC-e | Parcial |
| Pre-condicao documento autorizado | Completo no material |
| Retorno autorizado cStat 135 | Completo no material |
| Duplicidade cStat 573 | Parcial |
| Consulta/reconciliacao | Parcial |
| XML de cancelamento | Completo no material para campos extraidos |
| PDF de cancelamento | Parcial |
| Downloads por chave | Parcial |
| Justificativa | Incompleto |
| Protocolo | Incompleto |
| Efeitos integrados | Incompleto |

## 3. Matriz

| ID | Capacidade esperada | Status | Conteudo comprovado | Falta para implantacao | Prioridade |
|---|---|---|---|---|---|
| MC-CANC-001 | Cancelamento de NF-e | Parcial | Documento autorizado, cancelamento, XML/PDF e status cancelado. | Definir contrato final de entrada, protocolo, justificativa, prazo e efeitos. | P0 |
| MC-CANC-002 | Cancelamento de NFC-e | Parcial | Documento autorizado, cStat 135, XML/PDF e status cancelado. | Definir contrato final de entrada, protocolo, justificativa, prazo e efeitos PDV. | P0 |
| MC-CANC-003 | Pre-condicao de autorizacao | Completo no material | Cancelamento exige status fiscal autorizado. | Definir validacao por modelo e origem do status final. | P0 |
| MC-CANC-004 | Chave fiscal invalida | Parcial | Chave invalida gera erro funcional. | Definir formato, validacao por modelo e mensagens padronizadas. | P0 |
| MC-CANC-005 | Certificado ausente | Parcial | Certificado ausente bloqueia cancelamento. | Definir regra por empresa/filial, validade e renovacao. | P0 |
| MC-CANC-006 | Retorno autorizado | Completo no material | cStat 135 grava XML/PDF e status cancelado. | Confirmar tratamento de cStat 101 na mesma matriz de autorizacao. | P0 |
| MC-CANC-007 | Duplicidade de evento | Parcial | cStat 573 aciona consulta de situacao. | Definir reconciliacao completa, retry, pendencia e auditoria. | P0 |
| MC-CANC-008 | Registro `nfe_simplificado_cancelamento` | Parcial | TenantId, StatusSefaz, PdfCaminho, XmlCaminho e Xml. | Definir PK, FK, chave fiscal, protocolo, justificativa e datas. | P0 |
| MC-CANC-009 | Registro `nfce_simplificado_cancelamento` | Parcial | TenantId, StatusSefaz, PdfCaminho, XmlCaminho e Xml. | Definir PK, FK, chave fiscal, protocolo, justificativa e datas. | P0 |
| MC-CANC-010 | XML de cancelamento | Completo no material para campos extraidos | XML em nvarchar(max), caminhos logicos e download por chave. | Definir retencao, imutabilidade, backup, compactacao e assinatura. | P0 |
| MC-CANC-011 | PDF de cancelamento | Parcial | PDF de cancelamento e caminho ate 500 caracteres. | Definir template, geracao, regeneracao, logo, armazenamento e falhas. | P1 |
| MC-CANC-012 | Downloads por chave | Parcial | Download XML/PDF de cancelamento por chave. | Definir permissao, auditoria, nome de arquivo, mime type e tratamento de arquivo ausente. | P1 |
| MC-CANC-013 | Justificativa | Incompleto | Justificativa e citada como dado de evento, mas sem campo/regra detalhada. | Definir obrigatoriedade, tamanho, validacao e exibicao. | P0 |
| MC-CANC-014 | Protocolo de cancelamento | Incompleto | Protocolo aparece em contexto fiscal geral, mas nao nos campos especificos de cancelamento. | Definir campo, obrigatoriedade e relacao com XML. | P0 |
| MC-CANC-015 | Prazo legal | Incompleto | Nao informado no material. | Definir janela por documento/UF/modelo quando aplicavel. | P0 |
| MC-CANC-016 | Cancelamento importado por XML | Parcial | Cancelamento sem autorizacao relacionada deve ser rejeitado. | Definir contrato de importacao, matching por chave e reconciliacao. | P1 |
| MC-CANC-017 | Efeitos em vendas | Incompleto | Cancelamento pode vir de fluxo de venda. | Definir rollback, status comercial, bloqueios e auditoria. | P0 |
| MC-CANC-018 | Efeitos financeiros | Incompleto | Cancelamento fiscal deve afetar financeiro quando houver titulo. | Definir estorno, baixa, reabertura, permissoes e conciliacao. | P0 |
| MC-CANC-019 | Efeitos de estoque | Incompleto | Cancelamento fiscal pode afetar estoque. | Definir movimento reverso, lote/serie, custo e restricoes. | P1 |
| MC-CANC-020 | Permissoes | Incompleto | Material indica download sem matriz final e cancelamento por fluxos diversos. | Definir RBAC de cancelar, reconciliar, baixar, imprimir e reprocessar. | P0 |
| MC-CANC-021 | Testes | Parcial | Cenarios cStat 135 e duplicidade 573 comprovados. | Adicionar testes de chave invalida, certificado ausente, documento nao autorizado, arquivo ausente e consulta sem confirmacao. | P1 |

## 4. Decisoes necessarias

| ID | Decisao | Impacto |
|---|---|---|
| D-CANC-001 | Definir prazo legal e regra de justificativa do cancelamento. | Necessario para validacao fiscal e tela. |
| D-CANC-002 | Definir campos finais de protocolo, chave, data e justificativa nas tabelas de cancelamento. | Necessario para persistencia implantavel. |
| D-CANC-003 | Definir reconciliacao completa para duplicidade cStat 573. | Evita divergencia entre autoridade fiscal e Epros. |
| D-CANC-004 | Definir efeitos integrados em vendas, financeiro e estoque. | Necessario para consistencia operacional. |
| D-CANC-005 | Definir politica de retencao e imutabilidade de XML/PDF de cancelamento. | Necessario para compliance fiscal. |
| D-CANC-006 | Definir matriz de permissoes. | Necessario para seguranca operacional. |

## 5. Proximo passo

O proximo documento especifico da fila macro e `EF_CARTA_CORRECAO`, mantendo separado o evento de correcao do evento de cancelamento.
