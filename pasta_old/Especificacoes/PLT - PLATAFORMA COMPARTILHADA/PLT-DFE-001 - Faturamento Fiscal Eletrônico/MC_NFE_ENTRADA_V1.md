# Matriz de Completude - Epros

**Modulo:** PLATAFORMA_COMPARTILHADA  
**Submodulo:** FATURAMENTO_FISCAL_ELETRONICO  
**Capacidade:** NFE_ENTRADA  
**Versao:** V1  
**Empresa:** Siser  

## 1. Objetivo

Registrar lacunas da NF-e de entrada e importacao fiscal de XML de compra para que a especificacao avance para implantacao sem inventar regras, tabelas ou dominios nao comprovados no material.

## 2. Resumo de completude

| Item | Status |
|---|---|
| Emissao NF-e entrada sobre compra | Parcial |
| Numeracao e chave de entrada | Parcial |
| XML de entrada emitida | Parcial |
| Importacao manual de XML de compra | Parcial |
| Registro `importacao_xml` | Completo no material para campos extraidos |
| Status de importacao/cadastro/PDF | Parcial |
| Cadastro a partir do XML | Parcial |
| Compra/fatura a partir do XML/manifesto | Parcial |
| Contas a pagar a partir da compra | Parcial |
| Atribuicao de estoque | Parcial |
| Retencao XML/PDF | Incompleto |
| Permissoes finais | Incompleto |

## 3. Matriz

| ID | Capacidade esperada | Status | Conteudo comprovado | Falta para implantacao | Prioridade |
|---|---|---|---|---|---|
| MC-NFEENT-001 | Emissao NF-e de entrada sobre compra | Parcial | Rotinas de nova entrada, transmissao e detalhe sobre compra. | Fechar contrato de campos da compra, validacoes fiscais, status e retorno completo. | P0 |
| MC-NFEENT-002 | Modelo fisico da NF-e entrada | Incompleto | Campos `numero_nfe_entrada` e `chave_entrada`; XML em `xml_nfe_entrada/{cnpj}/`. | Definir tabela final, PK, FKs, status, unicidade e historico. | P0 |
| MC-NFEENT-003 | Numeracao de entrada | Parcial | Campo proprio de numero de entrada comprovado. | Definir serie, reserva transacional, concorrencia, rollback e faixa por empresa/filial/ambiente. | P0 |
| MC-NFEENT-004 | Chave de entrada | Parcial | Campo de chave de entrada comprovado. | Definir unicidade por empresa, validacao de formato e comportamento de duplicidade. | P0 |
| MC-NFEENT-005 | XML de entrada emitida | Parcial | Repositorio logico por CNPJ comprovado. | Definir retencao, imutabilidade, backup, permissao, download e auditoria. | P0 |
| MC-NFEENT-006 | Importacao manual de XML de compra | Parcial | Importacao manual de compra e repositorio `xml_entrada/{cnpj}/`. | Fechar contrato de upload, tipos aceitos, validacao de assinatura/schema e duplicidade. | P0 |
| MC-NFEENT-007 | Registro `importacao_xml` | Completo no material para campos extraidos | `TenantId`, `Xml`, `TipoDeXml`, `NfeId`, status/mensagens, `CodigoSefaz`, `TipoEvento`. | Confirmar obrigatoriedade final, dominios de status e indices. | P0 |
| MC-NFEENT-008 | Dominios de status | Incompleto | Existem `StatusImportacaoXml`, `StatusCadastro`, `StatusSalvarPdf`. | Definir valores, transicoes, estados finais, reprocessamento e exibicao. | P0 |
| MC-NFEENT-009 | Mensagens de erro | Parcial | Mensagens por importacao, cadastro e PDF; campos varchar(500). | Definir padrao de codigo, traducao, severidade e exibicao ao usuario. | P1 |
| MC-NFEENT-010 | Cadastro de pessoas/fornecedor | Parcial | Erros de pessoa/grupo ausente e cadastro de pessoas/veiculos. | Definir regras de criacao, merge, validacao documental e permissao. | P0 |
| MC-NFEENT-011 | Cadastro de produtos | Parcial | Erro de cadastro de produtos e contadores de produtos localizados/importados. | Definir matching por codigo/NCM/EAN/descricao, criacao automatica e aprovacao. | P0 |
| MC-NFEENT-012 | Unidades de medida | Parcial | Erro de cadastro de unidades de medida comprovado. | Definir conversao, unidade comercial/tributavel e tolerancias. | P0 |
| MC-NFEENT-013 | Tributacao NCM | Parcial | Erro de cadastro de tributacao de NCM comprovado. | Definir origem, vigencia, grupo tributario, ICMS/PIS/COFINS/IPI e reforma tributaria quando aplicavel. | P0 |
| MC-NFEENT-014 | Geracao de compra/fatura | Parcial | XML/manifesto pode gerar compra; erro quando nenhuma compra processada. | Definir contrato de compra, itens, impostos, rateios, frete, desconto, totais e rollback. | P0 |
| MC-NFEENT-015 | Controle de fatura salva | Parcial | Campo `fatura_salva` impede duplicidade a partir de manifesto. | Definir regra final de idempotencia entre manifesto, XML e compra. | P0 |
| MC-NFEENT-016 | Contas a pagar | Parcial | Erros de plano de contas ausente, tipo de pagamento sem plano e nenhuma conta a pagar cadastrada. | Definir parcelas, vencimentos, centro de custo, impostos retidos, aprovacao e rollback. | P0 |
| MC-NFEENT-017 | Atribuicao de estoque | Parcial | Material cita atribuicao de estoque a partir do XML. | Definir vinculo item-produto, lote/serie, deposito, custo, unidade, conferencias e divergencias. | P0 |
| MC-NFEENT-018 | Salvamento de PDF | Parcial | Status e mensagem de salvamento de PDF comprovados. | Definir geracao, origem, template, armazenamento, download, retencao e reprocessamento. | P1 |
| MC-NFEENT-019 | Fila/lote de arquivo XML | Parcial | Estrutura com nome de arquivo, quantidades, status e mensagem. | Confirmar se a estrutura e usada para entrada, saida ou ambas; definir processamento ZIP/lote. | P1 |
| MC-NFEENT-020 | Consulta de importacoes | Parcial | Contrato retorna `data` e `totalRegistros`. | Definir filtros, ordenacao, paginacao, permissoes e colunas exibidas. | P1 |
| MC-NFEENT-021 | Reprocessamento | Incompleto | Erros por etapa permitem inferir necessidade de reprocesso. | Definir quem pode reprocessar, quais etapas, idempotencia e trilha de auditoria. | P0 |
| MC-NFEENT-022 | Permissoes | Incompleto | Operacoes de nova entrada, transmissao, detalhe e importacao aparecem. | Definir RBAC final por ator: importar, transmitir, consultar, gerar compra, gerar financeiro, reprocessar e baixar arquivos. | P0 |
| MC-NFEENT-023 | Integracao com manifesto | Parcial | Chave, tipo, NSU, `fatura_salva`, itens e download existem no material. | Detalhar na EF de manifesto e fechar contrato com NF-e entrada/compra. | P1 |
| MC-NFEENT-024 | Duplicidade fiscal | Incompleto | Chave de entrada e identificador fiscal existem. | Definir regra unica por empresa/documento/chave/fornecedor e comportamento de XML duplicado. | P0 |
| MC-NFEENT-025 | Auditoria | Parcial | Status, mensagens e data de importacao comprovados. | Definir trilha completa por usuario/processo, antes/depois, download e reprocessamento. | P1 |

## 4. Decisoes necessarias

| ID | Decisao | Impacto |
|---|---|---|
| D-NFEENT-001 | Definir tabela final de NF-e de entrada emitida. | Necessario para persistencia implantavel de numero, chave, status, XML e vinculo com compra. |
| D-NFEENT-002 | Definir dominios de `StatusImportacaoXml`, `StatusCadastro` e `StatusSalvarPdf`. | Necessario para tela, regras, testes e reprocessamento. |
| D-NFEENT-003 | Definir politica de duplicidade por chave fiscal, XML e compra/fatura. | Evita compra, estoque e financeiro duplicados. |
| D-NFEENT-004 | Definir contrato completo entre importacao XML, compra, estoque e contas a pagar. | Necessario para implantacao integrada. |
| D-NFEENT-005 | Definir politica de retencao e imutabilidade de XML/PDF. | Necessario para compliance fiscal. |
| D-NFEENT-006 | Definir matriz de permissoes da entrada fiscal. | Necessario para seguranca operacional. |

## 5. Proximo passo

O proximo documento especifico da fila macro e `EF_DEVOLUCAO_FISCAL`, mantendo separacao entre entrada fiscal, devolucao, cancelamento e demais eventos.
