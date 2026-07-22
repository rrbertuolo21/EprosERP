# Matriz de Completude - Epros

**Modulo:** PLATAFORMA_COMPARTILHADA  
**Submodulo:** FATURAMENTO_FISCAL_ELETRONICO  
**Capacidade:** DEVOLUCAO_FISCAL  
**Versao:** V1  
**Empresa:** Siser  

## 1. Objetivo

Registrar lacunas da devolucao fiscal para que a especificacao avance para implantacao sem inventar campos, eventos ou efeitos integrados nao comprovados no material.

## 2. Resumo de completude

| Item | Status |
|---|---|
| Upload de XML de entrada | Parcial |
| Documento `devolucaos` | Parcial |
| Itens `item_devolucaos` | Parcial |
| Estados 0/1/2/3 | Completo no material |
| Chave da NF de entrada | Parcial |
| Chave gerada | Parcial |
| Numero gerado | Parcial |
| Transmissao | Parcial |
| Cancelamento | Parcial |
| Correcao | Parcial |
| Numeracao compartilhada | Parcial |
| Efeitos em estoque/financeiro | Incompleto |
| Permissoes finais | Incompleto |

## 3. Matriz

| ID | Capacidade esperada | Status | Conteudo comprovado | Falta para implantacao | Prioridade |
|---|---|---|---|---|---|
| MC-DEV-001 | Upload de XML de entrada | Parcial | Upload de XML como entrada da devolucao. | Definir tipos aceitos, validacao, schema, assinatura, duplicidade e mensagens. | P0 |
| MC-DEV-002 | Leitura do XML de entrada | Incompleto | XML de entrada e chave da NF de entrada sao citados. | Definir campos extraidos, validacoes, itens, totais e tratamento de erro. | P0 |
| MC-DEV-003 | Documento `devolucaos` | Parcial | Estado, chave da NF de entrada, chave gerada e numero gerado. | Definir PK, FKs, empresa, datas, valores, serie, ambiente, protocolo, XML e auditoria. | P0 |
| MC-DEV-004 | Itens `item_devolucaos` | Parcial | NCM, CFOP e CST por linha. | Definir produto, quantidade, unidade, valores, impostos, lote/serie e vinculo com item original. | P0 |
| MC-DEV-005 | Estados da devolucao | Completo no material | 0=NOVO, 1=APROVADO, 2=REJEITADO, 3=CANCELADO. | Definir transicoes permitidas e permissao por transicao. | P0 |
| MC-DEV-006 | Chave da NF de entrada | Parcial | Campo `chave_nf_entrada` comprovado. | Definir obrigatoriedade final, formato, validacao e unicidade. | P0 |
| MC-DEV-007 | Chave gerada | Parcial | Campo `chave_gerada` comprovado. | Definir momento de gravacao, unicidade, consulta e retencao. | P0 |
| MC-DEV-008 | Numero gerado | Parcial | Campo `numero_gerado` comprovado. | Definir serie, concorrencia, rollback, ambiente e vinculo com sequencia NF-e. | P0 |
| MC-DEV-009 | Sequencia fiscal compartilhada | Parcial | Numero gerado da devolucao entra na sequencia NF-e. | Definir algoritmo transacional, reserva, bloqueio de duplicidade e reconciliacao. | P0 |
| MC-DEV-010 | Transmissao | Parcial | A transmissao da devolucao e comprovada. | Definir contrato de envio, retorno, XML, protocolo, rejeicoes, timeout e reprocesso. | P0 |
| MC-DEV-011 | Cancelamento | Parcial | Cancelamento da devolucao e comprovado. | Definir prazo, justificativa, protocolo, XML de evento, permissao e efeitos. | P0 |
| MC-DEV-012 | Correcao | Parcial | Correcao da devolucao e comprovada. | Definir texto/campos, sequencia, protocolo, XML de evento e limites legais. | P0 |
| MC-DEV-013 | XML da devolucao gerada | Incompleto | XML de entrada comprovado; caminho final de XML gerado nao informado. | Definir armazenamento, retencao, download, imutabilidade e auditoria. | P0 |
| MC-DEV-014 | Duplicidade fiscal | Incompleto | Ha chave de entrada, chave gerada e numero gerado. | Definir bloqueio por chave de entrada, chave gerada, numero/serie e item devolvido. | P0 |
| MC-DEV-015 | Efeitos em estoque | Incompleto | Itens existem e devolucao e operacionalmente integrada. | Definir movimento de estoque, deposito, custo, lote/serie e reversao no cancelamento. | P0 |
| MC-DEV-016 | Efeitos financeiros | Incompleto | Nao ha contrato detalhado no material. | Definir contas a pagar/receber, abatimentos, creditos, impostos e cancelamento financeiro. | P1 |
| MC-DEV-017 | Permissoes | Incompleto | Acoes de upload, listagem, transmissao, cancelamento e correcao aparecem. | Definir RBAC por ator e segregacao fiscal. | P0 |
| MC-DEV-018 | Auditoria | Parcial | Estados, chaves e numero permitem rastreio minimo. | Definir trilha completa por usuario/processo, tentativas, eventos e arquivos. | P1 |
| MC-DEV-019 | Mensagens funcionais | Incompleto | Material nao traz catalogo completo de mensagens. | Definir mensagens padronizadas para XML, referencia, itens, transmissao, cancelamento e correcao. | P1 |
| MC-DEV-020 | Testes de aceitacao fiscal | Parcial | Estados e acoes permitem montar cenarios basicos. | Definir massa fiscal, rejeicoes, cancelamento, correcao, duplicidade e concorrencia. | P1 |

## 4. Decisoes necessarias

| ID | Decisao | Impacto |
|---|---|---|
| D-DEV-001 | Definir modelo completo de cabecalho e itens da devolucao. | Necessario para banco, tela, API, teste e implantacao. |
| D-DEV-002 | Definir se devolucao usa sempre XML de entrada ou tambem documento interno referenciado. | Afeta fluxo operacional e validacoes. |
| D-DEV-003 | Definir regra transacional de numeracao compartilhada. | Evita duplicidade fiscal com NF-e. |
| D-DEV-004 | Definir contrato de cancelamento e correcao da devolucao. | Necessario para eventos fiscais e auditoria. |
| D-DEV-005 | Definir efeitos em estoque e financeiro. | Necessario para integracao operacional. |
| D-DEV-006 | Definir retencao e download de XML/PDF da devolucao. | Necessario para compliance fiscal. |

## 5. Proximo passo

O proximo documento especifico da fila macro e `EF_CANCELAMENTO_DFE`, separando o evento fiscal generico de cancelamento dos cancelamentos especificos citados em NF-e, NFC-e e devolucao.
