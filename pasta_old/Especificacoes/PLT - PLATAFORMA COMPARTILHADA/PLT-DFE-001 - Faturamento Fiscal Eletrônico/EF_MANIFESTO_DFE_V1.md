# EF_MANIFESTO_DFE_V1

## 1. Identificacao

| Campo | Conteudo |
|---|---|
| Empresa | Siser |
| Sistema | Epros |
| Modulo | PLATAFORMA_COMPARTILHADA |
| Submodulo | FATURAMENTO_FISCAL_ELETRONICO |
| Documento | Especificacao funcional - Manifesto DFe |
| Versao | V1 |
| Status | Concluido |
| Data | 2026-06-08 |

## 2. Objetivo funcional

O Manifesto DFe permite ao Epros consultar documentos fiscais distribuidos por NSU, controlar limite diario de consultas, registrar manifestacoes do destinatario, baixar XML quando permitido, gerar compra/fatura a partir do documento recebido e evitar duplicidade por meio da flag `fatura_salva`.

Esta EF consolida os dados comprovados no material canonico para `manifestos`, `manifesto_limites` e `item_dves`, preservando os tipos de manifestacao, o fluxo de consulta, download XML e efeitos em compra/estoque sem inventar regras fiscais ou campos nao informados.

## 3. Escopo

| Area | Incluso | Status |
|---|---|---|
| Consulta por NSU | Busca distribuicao de documentos fiscais por NSU | Com conteudo |
| Limite diario | Controle de consultas DFe diarias por manifesto | Com conteudo |
| Manifestacao | Tipos 0 a 4 | Com conteudo |
| Ciencia | Tipo 1, evento 210210 | Com conteudo |
| Confirmacao | Tipo 2, evento 210200 | Com conteudo |
| Desconhecimento | Tipo 3 | Com conteudo |
| Operacao nao realizada | Tipo 4 | Com conteudo |
| Download XML | XML DFe apos ciencia/confirmacao | Com conteudo |
| Compra/fatura | Geracao a partir do XML/documento manifestado | Parcial |
| Estoque | Atribuicao de estoque a partir do documento | Parcial |
| Itens manifestados | Produto vinculado a NF manifestada | Parcial |
| Permissao | Acesso por permissao de visualizacao ou criacao comprovada | Parcial |
| Modelo fiscal completo de retorno distribuicao | XML, schema, codigos de retorno e protocolo completo | Incompleto |

## 4. Fora de escopo

| Item | Motivo |
|---|---|
| NF-e entrada completa | Possui EF especifica; Manifesto DFe apenas origina/apoia compra e estoque. |
| Importacao XML manual | Possui EF especifica na fila macro. |
| Compras, estoque e financeiro completos | Pertencem aos respectivos modulos; aqui ficam gatilhos comprovados. |
| Regras completas de distribuicao nacional por certificado, ambiente e UF | Nao informado no material. |
| Validacao completa de XML distribuido | Nao informado no material. |

## 5. Atores e responsabilidades

| Ator | Responsabilidade | Observacao |
|---|---|---|
| Usuario fiscal | Consultar distribuicao, manifestar documentos, baixar XML e acompanhar fatura salva. | Permissao final detalhada fica na MC. |
| Usuario de compras | Gerar compra/fatura a partir de XML/documento manifestado quando permitido. | Efeito comprovado, contrato completo fica em Compras. |
| Usuario de estoque | Atribuir estoque a partir do documento recebido quando permitido. | Efeito comprovado, contrato completo fica em Estoque. |
| Administrador Siser | Controlar parametros, limites e suporte da rotina fiscal. | Limite diario comprovado; configuracao final fica na MC. |
| Epros | Consultar NSU, controlar limite, registrar manifestacao, baixar XML e bloquear duplicidade de fatura. | Deve preservar chave, tipo, NSU, documento, valor e `fatura_salva`. |

## 6. Conceitos funcionais

| Conceito | Definicao |
|---|---|
| Manifesto DFe | Consulta e manifestacao de documento fiscal recebido/distribuido. |
| NSU | Numero sequencial usado na distribuicao de documentos fiscais. |
| Tipo de manifestacao | Codigo que representa situacao/evento do destinatario. |
| Ciencia | Manifestacao tipo 1, evento 210210. |
| Confirmacao | Manifestacao tipo 2, evento 210200. |
| Desconhecimento | Manifestacao tipo 3. |
| Operacao nao realizada | Manifestacao tipo 4. |
| Sem manifestacao | Tipo 0. |
| Fatura salva | Indicador de que compra/fatura ja foi registrada a partir do manifesto. |
| Item manifestado | Produto vinculado ao documento fiscal manifestado. |
| Limite diario | Controle de quantidade de consultas DFe permitidas por dia. |

## 7. Capacidades funcionais

| Capacidade | Descricao | Entrada principal | Saida esperada |
|---|---|---|---|
| Consultar documentos por NSU | Busca documentos fiscais distribuidos. | Ultimo/proximo NSU e contexto fiscal | Documentos retornados e registrados. |
| Controlar limite diario | Impede exceder limite de consultas DFe diarias. | Data e contador de consultas | Consulta permitida ou bloqueada. |
| Registrar manifestacao | Atualiza tipo de manifestacao do documento. | Chave, tipo, NSU | Manifestacao registrada. |
| Registrar ciencia | Registra tipo 1 e evento 210210. | Documento recebido | Manifesto com ciencia. |
| Registrar confirmacao | Registra tipo 2 e evento 210200. | Documento recebido | Manifesto confirmado e download/impressao habilitados quando permitido. |
| Registrar desconhecimento | Registra tipo 3. | Documento recebido | Manifesto marcado como desconhecido. |
| Registrar operacao nao realizada | Registra tipo 4. | Documento recebido | Manifesto marcado como operacao nao realizada. |
| Baixar XML DFe | Baixa XML apos ciencia ou confirmacao. | Chave do documento | XML disponivel. |
| Gerar compra/fatura | Cria compra/fatura a partir de documento manifestado. | XML/dados do documento | Compra/fatura criada e `fatura_salva` marcada. |
| Relacionar itens | Vincula produto ao documento fiscal manifestado. | Produto e documento | Item vinculado. |

## 8. Regras funcionais

| Regra | Descricao | Contexto | Resultado esperado | Severidade | Fonte funcional |
|---|---|---|---|---|---|
| REG-MAN-001 | Manifesto DFe deve permitir consulta de distribuicao por NSU. | Consulta fiscal | Buscar documentos fiscais distribuidos. | Alta | Consulta por NSU comprovada. |
| REG-MAN-002 | Epros deve controlar limite diario de consultas DFe. | Consulta fiscal | Bloquear ou impedir excesso de consultas. | Alta | Limite diario comprovado. |
| REG-MAN-003 | Manifesto deve preservar chave do documento. | Persistencia | Documento rastreavel por chave. | Bloqueante | Campo comprovado. |
| REG-MAN-004 | Manifesto deve preservar tipo de manifestacao. | Persistencia | Tipo 0 a 4 registrado. | Bloqueante | Campo comprovado. |
| REG-MAN-005 | Manifesto deve preservar NSU. | Persistencia | Documento rastreavel por NSU. | Alta | Campo comprovado. |
| REG-MAN-006 | Manifesto deve preservar indicador `fatura_salva`. | Compra/fatura | Impedir compra duplicada. | Bloqueante | Campo comprovado. |
| REG-MAN-007 | Manifesto deve preservar localizacao quando informada. | Origem operacional | Relacionar documento a localizacao. | Media | Campo comprovado. |
| REG-MAN-008 | Manifesto deve preservar documento do participante fiscal quando informado. | Identificacao | Permitir rastreio fiscal. | Alta | Campo comprovado. |
| REG-MAN-009 | Manifesto deve preservar valor quando informado. | Compra/financeiro | Apoiar conferencia. | Media | Campo comprovado. |
| REG-MAN-010 | Tipo 0 representa documento sem manifestacao. | Dominio | Registrar sem manifestacao. | Media | Dominio comprovado. |
| REG-MAN-011 | Tipo 1 representa ciencia e deve registrar evento 210210. | Manifestacao | Atualizar tipo para ciencia. | Alta | Evento comprovado. |
| REG-MAN-012 | Tipo 2 representa confirmacao e deve registrar evento 210200. | Manifestacao | Atualizar tipo para confirmacao. | Alta | Evento comprovado. |
| REG-MAN-013 | Tipo 3 representa desconhecimento. | Manifestacao | Atualizar tipo para desconhecimento. | Alta | Tipo comprovado. |
| REG-MAN-014 | Tipo 4 representa operacao nao realizada. | Manifestacao | Atualizar tipo para operacao nao realizada. | Alta | Tipo comprovado. |
| REG-MAN-015 | Download XML DFe deve ocorrer apos ciencia ou confirmacao. | Download XML | Disponibilizar XML quando a manifestacao permitir. | Alta | Regra comprovada. |
| REG-MAN-016 | Confirmacao deve habilitar download/impressao quando permitido. | Manifestacao confirmada | Liberar acoes do documento. | Media | Efeito comprovado. |
| REG-MAN-017 | Documento com `fatura_salva` verdadeiro nao deve gerar compra/fatura duplicada. | Compra/fatura | Bloquear nova geracao de compra/fatura. | Bloqueante | Flag comprovada. |
| REG-MAN-018 | Cadastro de compra a partir de XML deve suportar salvar fornecedor, salvar fatura, cadastrar produto e atribuir estoque. | Compra/estoque | Executar etapas comprovadas quando acionadas. | Alta | Etapas comprovadas. |
| REG-MAN-019 | Item manifestado deve relacionar produto e NF manifestada. | Itens | Manter vinculo produto-documento. | Alta | `item_dves` comprovado. |
| REG-MAN-020 | Acesso ao manifesto deve exigir permissao de visualizacao ou criacao conforme material. | Seguranca | Bloquear acesso sem permissao. | Bloqueante | Permissao comprovada parcialmente. |
| REG-MAN-021 | O material nao informa schema completo do XML, codigos de retorno, protocolo e politica de retencao; estes pontos devem permanecer na MC. | Especificacao | Evitar invencao. | Bloqueante | Material parcial. |

## 9. Estados e tipos

| Tipo | Nome funcional | Evento | Efeito comprovado |
|---|---|---|---|
| 0 | Sem manifestacao | Nao informado no material | Documento ainda sem manifestacao. |
| 1 | Ciencia | 210210 | Registro de ciencia. |
| 2 | Confirmacao | 210200 | Habilita download/impressao quando permitido. |
| 3 | Desconhecimento | Nao informado no material | Documento marcado como desconhecido. |
| 4 | Operacao nao realizada | Nao informado no material | Documento marcado como operacao nao realizada. |

## 10. Modelo de dados funcional e implantavel

O material comprova `manifestos`, `manifesto_limites` e `item_dves`. Tambem comprova download XML DFe, controle de fatura salva e etapas de compra/estoque. Para implantacao, esta EF conserva os campos comprovados e cria estruturas funcionais auxiliares para consulta, manifestacao, download XML e geracao de compra apenas como consolidacao operacional dos fluxos existentes.[^1]

| Entidade funcional | Finalidade | Cardinalidade | Persistencia indicada |
|---|---|---|---|
| manifestos | Registrar documento distribuido/manifestado. | 1 por documento/manifestacao | Comprovada. |
| manifesto_limites | Controlar limite diario de consultas DFe. | 1 por controle de limite | Comprovada. |
| item_dves | Relacionar produto a NF manifestada. | 0..N por manifesto | Comprovada. |
| manifesto_consulta_nsu | Registrar consulta de distribuicao por NSU. | 0..N por consulta | Consolidacao funcional.[^1] |
| manifesto_evento | Registrar evento de manifestacao enviado/aplicado. | 0..N por manifesto | Consolidacao funcional.[^1] |
| manifesto_xml | Registrar XML DFe baixado. | 0..1 por manifesto | Consolidacao funcional.[^1] |
| manifesto_compra_fatura | Registrar geracao de compra/fatura a partir do manifesto. | 0..1 por manifesto | Consolidacao funcional.[^1] |

### 10.1 Relacionamentos funcionais

| Origem | Relacao | Destino | Regra |
|---|---|---|---|
| manifesto_consulta_nsu | gera/atualiza | manifestos | Consulta por NSU cria ou atualiza documentos recebidos. |
| manifestos | possui | item_dves | Documento pode possuir itens vinculados a produtos. |
| manifestos | possui | manifesto_evento | Documento pode receber manifestacoes. |
| manifestos | pode possuir | manifesto_xml | XML pode ser baixado apos ciencia/confirmacao. |
| manifestos | pode gerar | manifesto_compra_fatura | Compra/fatura pode ser criada quando `fatura_salva` ainda nao estiver marcada. |
| manifesto_limites | controla | manifesto_consulta_nsu | Limite diario controla consultas. |

## 11. Dicionario de dados implantavel

### 11.1 manifestos

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| Id | Identificador | Nao informado no material | Sim | Chave primaria | Identificador interno.[^1] |
| Chave | Texto | Nao informado no material | Nao informado no material | Chave fiscal | Campo comprovado. |
| Tipo | Numero/enum | 0=sem, 1=ciencia, 2=confirmacao, 3=desconhecimento, 4=operacao nao realizada | Sim | Tipo de manifestacao | Dominio comprovado. |
| Nsu | Texto/numero | Nao informado no material | Condicional | Sequencial fiscal | Usado em consulta de documentos. |
| FaturaSalva | Booleano | Sim/Nao | Nao informado no material | Controle de compra/fatura | Impede duplicidade. |
| LocalizacaoId | Identificador | Nao informado no material | Nao informado no material | Localizacao | Campo comprovado como `location_id`; nome funcional consolidado.[^1] |
| Documento | Texto | Nao informado no material | Nao informado no material | Documento fiscal/participante | Campo comprovado. |
| Valor | Decimal | Nao informado no material | Nao informado no material | Valor fiscal | Campo comprovado. |

### 11.2 manifesto_limites

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| Id | Identificador | Nao informado no material | Sim | Chave primaria | Identificador interno.[^1] |
| DataControle | Data | Nao informado no material | Nao informado no material | Controle diario | Campo funcional necessario; estrutura final nao informada.[^1] |
| QuantidadeConsultas | Numero inteiro | Nao informado no material | Nao informado no material | Contador | Controla consultas diarias.[^1] |
| LimiteDiario | Numero inteiro | Nao informado no material | Nao informado no material | Limite | Material comprova limite, nao informa valor final. |

### 11.3 item_dves

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| Id | Identificador | Nao informado no material | Sim | Chave primaria | Identificador interno.[^1] |
| ManifestoId | Identificador | Nao informado no material | Sim | Relacao com manifestos | Vinculo com documento manifestado.[^1] |
| ProdutoId | Identificador | Nao informado no material | Nao informado no material | Produto | Relaciona produto a NF manifestada. |
| DocumentoFiscalId | Identificador/chave | Nao informado no material | Nao informado no material | NF manifestada | Estrutura final nao informada.[^1] |

### 11.4 manifesto_consulta_nsu

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| Id | Identificador | Nao informado no material | Sim | Chave primaria | Identificador interno.[^1] |
| NsuConsulta | Texto/numero | Nao informado no material | Sim | NSU | Consulta por NSU comprovada. |
| DataConsulta | Data/hora | Nao informado no material | Nao informado no material | Auditoria | Estrutura final nao informada.[^1] |
| ResultadoConsulta | Texto/estrutura | Nao informado no material | Nao informado no material | Resultado | Formato final nao informado.[^1] |

### 11.5 manifesto_evento

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| Id | Identificador | Nao informado no material | Sim | Chave primaria | Identificador interno.[^1] |
| ManifestoId | Identificador | Nao informado no material | Sim | Relacao com manifestos | Vinculo com documento manifestado.[^1] |
| Tipo | Numero/enum | 1, 2, 3, 4 | Sim | Tipo de manifestacao | Tipo 0 representa sem evento. |
| CodigoEvento | Numero/texto | 210210, 210200 ou Nao informado no material | Condicional | Evento fiscal | Ciencia e confirmacao possuem codigo comprovado. |
| DataEvento | Data/hora | Nao informado no material | Nao informado no material | Auditoria | Estrutura final nao informada.[^1] |
| ResultadoEvento | Texto/estrutura | Nao informado no material | Nao informado no material | Retorno | Retorno final nao informado.[^1] |

### 11.6 manifesto_xml

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| Id | Identificador | Nao informado no material | Sim | Chave primaria | Identificador interno.[^1] |
| ManifestoId | Identificador | Nao informado no material | Sim | Relacao com manifestos | Vinculo com documento manifestado.[^1] |
| Chave | Texto | Nao informado no material | Sim | Chave fiscal | XML localizado pela chave. |
| Xml | Texto/arquivo | XML | Nao informado no material | XML DFe | Download comprovado apos ciencia/confirmacao. |
| CaminhoXml | Texto | Nao informado no material | Nao informado no material | Armazenamento | Caminho final nao informado no material. |

### 11.7 manifesto_compra_fatura

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| Id | Identificador | Nao informado no material | Sim | Chave primaria | Identificador interno.[^1] |
| ManifestoId | Identificador | Nao informado no material | Sim | Relacao com manifestos | Vinculo com documento manifestado.[^1] |
| FornecedorSalvo | Booleano | Sim/Nao | Nao informado no material | Etapa de compra | Material comprova salvar fornecedor. |
| FaturaSalva | Booleano | Sim/Nao | Nao informado no material | Etapa de compra | Material comprova salvar fatura e flag. |
| ProdutoCadastrado | Booleano | Sim/Nao | Nao informado no material | Etapa de estoque/cadastro | Material comprova cadastrar produto. |
| EstoqueAtribuido | Booleano | Sim/Nao | Nao informado no material | Etapa de estoque | Material comprova atribuir estoque. |
| DataProcessamento | Data/hora | Nao informado no material | Nao informado no material | Auditoria | Estrutura final nao informada.[^1] |

## 12. Fluxos funcionais

### 12.1 Consultar distribuicao por NSU

| Passo | Responsavel | Acao | Entrada | Saida |
|---|---|---|---|---|
| 1 | Usuario fiscal | Solicita busca de documentos. | NSU e contexto fiscal | Consulta iniciada. |
| 2 | Epros | Verifica limite diario. | `manifesto_limites` | Consulta permitida ou bloqueada. |
| 3 | Epros | Consulta distribuicao por NSU. | NSU | Documentos retornados. |
| 4 | Epros | Cria ou atualiza manifestos. | Chave, tipo, NSU, documento, valor, localizacao | Manifestos registrados. |

### 12.2 Manifestar documento

| Passo | Responsavel | Acao | Entrada | Saida |
|---|---|---|---|---|
| 1 | Usuario fiscal | Seleciona documento. | Manifesto | Documento em manifestacao. |
| 2 | Usuario fiscal | Escolhe tipo 1, 2, 3 ou 4. | Tipo de manifestacao | Evento preparado. |
| 3 | Epros | Registra tipo/evento. | Tipo, chave, NSU | Manifesto atualizado. |
| 4 | Epros | Habilita download XML quando tipo permitir. | Ciencia ou confirmacao | XML disponivel quando baixado. |

### 12.3 Baixar XML DFe

| Passo | Responsavel | Acao | Entrada | Saida |
|---|---|---|---|---|
| 1 | Usuario fiscal | Solicita download XML. | Chave | Pedido recebido. |
| 2 | Epros | Verifica se houve ciencia ou confirmacao. | Tipo do manifesto | Download permitido ou bloqueado. |
| 3 | Epros | Recupera XML pela chave. | Chave | XML entregue/registrado. |

### 12.4 Gerar compra/fatura e estoque

| Passo | Responsavel | Acao | Entrada | Saida |
|---|---|---|---|---|
| 1 | Usuario de compras | Solicita cadastro de compra/fatura a partir do manifesto. | XML/documento manifestado | Processo iniciado. |
| 2 | Epros | Verifica `fatura_salva`. | Manifesto | Bloqueio se ja salvo. |
| 3 | Epros | Salva fornecedor e fatura quando aplicavel. | Dados do XML/documento | Compra/fatura gerada. |
| 4 | Epros | Cadastra produto e atribui estoque quando aplicavel. | Itens do documento | Estoque/cadastro atualizado. |
| 5 | Epros | Atualiza `fatura_salva`. | Processamento concluido | Duplicidade bloqueada em novas tentativas. |

## 13. Validacoes e mensagens

| Codigo | Mensagem | Condicao |
|---|---|---|
| MSG-MAN-001 | Limite diario de consultas DFe atingido. | Consulta excede limite. |
| MSG-MAN-002 | NSU nao informado. | Consulta por NSU sem NSU. |
| MSG-MAN-003 | Tipo de manifestacao invalido. | Tipo fora do dominio 0 a 4. |
| MSG-MAN-004 | XML disponivel somente apos ciencia ou confirmacao. | Download sem tipo permitido. |
| MSG-MAN-005 | Documento ja possui fatura salva. | Tentativa de gerar compra/fatura duplicada. |
| MSG-MAN-006 | Usuario sem permissao para acessar Manifesto DFe. | Acesso sem permissao comprovada. |

## 14. Integracoes

| Integracao | Direcao | Dados | Regra | Lacuna |
|---|---|---|---|---|
| Parametros fiscais | Entrada | Certificado, documento fiscal, ambiente e contexto de consulta | Necessarios para consulta distribuicao. | Detalhe completo nao informado. |
| Compras | Saida | Fornecedor, fatura, XML, valor, documento | Manifesto pode gerar compra/fatura. | Contrato completo fica em Compras. |
| Estoque | Saida | Produto e atribuicao de estoque | Manifesto pode cadastrar produto e atribuir estoque. | Contrato completo fica em Estoque. |
| Cadastros Base | Entrada/Saida | Fornecedor, documento, produto | Dados mestres devem ser reaproveitados. | Regras de deduplicacao ficam nos modulos donos. |
| NF-e entrada | Entrada/Saida | XML/documento recebido, `fatura_salva`, itens | Manifesto alimenta entrada fiscal. | Detalhamento complementar na EF de NF-e entrada. |

## 15. Permissoes e seguranca

| Controle | Regra |
|---|---|
| Acesso | Material comprova acesso por permissao de visualizacao ou criacao; matriz final fica na MC. |
| Download XML | Deve respeitar tipo de manifestacao e permissao. |
| Compra/fatura | Deve impedir duplicidade por `fatura_salva`. |
| Auditoria | Necessaria para consulta, manifestacao, download e geracao de compra; estrutura final nao informada. |
| Tenant/empresa | Nao informado no material especifico; deve seguir regra macro de contexto fiscal. |

## 16. Relatorios e consultas

| Consulta | Filtros comprovados | Resultado |
|---|---|---|
| Distribuicao por NSU | NSU | Documentos fiscais recebidos/distribuidos. |
| Manifestos | Chave, tipo, NSU, documento, valor, localizacao | Lista de documentos manifestados. |
| Limite diario | Data/controle diario | Quantidade de consultas e limite. |
| Itens manifestados | Manifesto/produto | Produtos vinculados a NF manifestada. |
| Faturas salvas | `fatura_salva` | Manifestos que ja geraram compra/fatura. |

## 17. Criterios de aceite

| Codigo | Criterio |
|---|---|
| CA-MAN-001 | Consulta por NSU deve respeitar limite diario. |
| CA-MAN-002 | Manifesto deve registrar chave, tipo, NSU, documento, valor e `fatura_salva` quando informados. |
| CA-MAN-003 | Tipo de manifestacao deve aceitar apenas 0, 1, 2, 3 ou 4. |
| CA-MAN-004 | Tipo 1 deve registrar ciencia com evento 210210. |
| CA-MAN-005 | Tipo 2 deve registrar confirmacao com evento 210200. |
| CA-MAN-006 | Download XML deve ser permitido apos ciencia ou confirmacao. |
| CA-MAN-007 | Manifesto com `fatura_salva` verdadeiro nao deve gerar compra/fatura duplicada. |
| CA-MAN-008 | Geracao de compra/fatura deve contemplar salvar fornecedor, salvar fatura, cadastrar produto e atribuir estoque quando aplicavel. |
| CA-MAN-009 | Item manifestado deve vincular produto e NF manifestada. |
| CA-MAN-010 | Campos nao informados no material nao devem ser preenchidos por suposicao na EF. |

## 18. Lacunas encaminhadas para MC

| Lacuna | Impacto |
|---|---|
| Schema completo do XML distribuido | Necessario para validacao e importacao robusta. |
| Codigos de retorno da consulta e manifestacao | Necessario para tratamento de erro. |
| Protocolo/evento completo para tipos 3 e 4 | Necessario para fiscal completo. |
| Politica de armazenamento e retencao XML | Necessario para evidencia fiscal. |
| Matriz final de permissao | Necessario para seguranca. |
| Contrato completo com compras, estoque e financeiro | Necessario para implantacao integrada. |
| Regras de deduplicacao por chave, NSU e documento | Necessario para evitar duplicidade fiscal. |

## 19. Proximo passo

O proximo documento especifico da fila macro e `EF_CFE_SAT`, detalhando CF-e/SAT conforme material disponivel.

[^1]: Consolidacao funcional criada para tornar implantavel a especificacao, pois o material comprova `manifestos`, `manifesto_limites`, `item_dves`, consulta por NSU, manifestacoes, download XML e geracao de compra/estoque, mas nao informa tabela final completa de eventos, consulta, XML, auditoria, protocolo ou contratos completos com Compras e Estoque.
