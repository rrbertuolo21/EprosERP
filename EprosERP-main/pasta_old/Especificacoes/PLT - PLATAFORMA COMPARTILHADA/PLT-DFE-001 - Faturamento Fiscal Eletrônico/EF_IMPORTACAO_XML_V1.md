# EF_IMPORTACAO_XML_V1

## 1. Identificacao

| Item | Conteudo |
|---|---|
| Empresa | Siser |
| Sistema | Epros |
| Modulo | Plataforma Compartilhada |
| Submodulo | Faturamento Fiscal Eletronico |
| Documento | Especificacao funcional - Importacao XML |
| Versao | V1 |
| Status | Concluido |

## 2. Objetivo funcional

A importacao XML permite receber arquivos XML ou ZIP, validar o documento fiscal, controlar duplicidade, processar o conteudo, registrar status por etapa, cadastrar ou relacionar entidades quando aplicavel, controlar o salvamento de PDF e disponibilizar diagnostico operacional para correcao.

O processo deve permitir que o Epros aproveite o XML fiscal como origem de informacao operacional, mantendo trilha de processamento suficiente para saber se a leitura do XML, os cadastros relacionados e o PDF foram concluidos ou ficaram em erro.

## 3. Escopo

| Area | Incluso |
|---|---|
| Upload | Recebimento de arquivo XML ou ZIP. |
| Validacao inicial | Empresa identificada, arquivo valido, XML valido, emitente/empresa coerentes e usuario autorizado. |
| Duplicidade | Rejeicao de XML/documento ja importado por chave, NfeId ou identificador fiscal equivalente disponivel. |
| Processamento XML | Leitura do conteudo, identificacao do tipo de XML, codigo fiscal, tipo de evento e identificador fiscal. |
| Cadastro relacionado | Controle de status e mensagem para cadastro ou relacionamento de pessoas, produtos, unidades, tributacao ou entidades relacionadas. |
| PDF | Controle de status e mensagem para geracao ou salvamento do PDF associado ao XML. |
| Lote | Controle de nome do arquivo, quantidade de XMLs, XMLs invalidos, produtos/clientes localizados e importados. |
| Consulta | Retorno de registros de importacao com dados e total de registros. |
| Efeitos operacionais | Possibilidade de gerar compra/fatura, contas a pagar, estoque e cadastros quando houver dados suficientes e regras atendidas. |

## 4. Principios funcionais

| Principio | Regra |
|---|---|
| Fonte unica do processamento | Cada XML importado deve ter registro operacional com status e mensagens por etapa. |
| Diagnostico por etapa | Erro de leitura, erro de cadastro e erro de PDF devem ser controlados separadamente. |
| Nao duplicidade | XML/documento ja importado deve ser bloqueado antes de gerar efeitos operacionais repetidos. |
| Integridade por empresa | A importacao deve estar associada a empresa correta do Epros. |
| Preservacao fiscal | O conteudo XML deve ser preservado no registro de importacao. |
| Lote rastreavel | Upload ZIP/XML deve manter contadores e status do lote. |

## 5. Perfis e responsabilidades

| Perfil | Responsabilidades |
|---|---|
| Usuario fiscal | Enviar XML/ZIP, acompanhar status e tratar erros retornados. |
| Usuario de compras | Validar efeitos em compra/fatura quando o XML for usado como origem de entrada. |
| Usuario financeiro | Validar contas a pagar quando geradas a partir do XML importado. |
| Usuario de cadastro | Corrigir ou completar fornecedores, clientes, produtos, unidades e tributacao quando necessario. |
| Epros | Validar arquivo, processar XML, bloquear duplicidade, registrar status, mensagens e efeitos permitidos. |

## 6. Entradas e saidas

### 6.1 Entradas

| Entrada | Obrigatorio | Regra |
|---|---|---|
| Arquivo | Sim | Deve ser XML ou ZIP. |
| Empresa | Sim | Empresa precisa estar identificada para validacao operacional. |
| Tipo de XML | Condicional | Quando informado, orienta o processamento. |
| NFeId | Condicional | Usado como identificador fiscal quando extraido ou informado. |
| Codigo fiscal | Condicional | Registrado quando disponivel. |
| Tipo de evento | Condicional | Registrado quando aplicavel. |
| XML | Sim para registro individual | Conteudo fiscal preservado no registro de importacao. |

### 6.2 Saidas

| Saida | Conteudo |
|---|---|
| Registro de importacao | XML, tipo, identificador fiscal, status, mensagens, codigo fiscal, tipo de evento e data de importacao. |
| Registro de lote | Nome do arquivo, contadores, status e mensagem de erro. |
| Status de importacao XML | NaoProcessado, Processando, Finalizado ou Erro. |
| Status de cadastro | NaoProcessado, Processando, Finalizado ou Erro. |
| Status de PDF | NaoProcessado, Processando, Finalizado ou Erro. |
| Consulta paginada/listada | Dados da importacao e total de registros. |
| Efeitos operacionais | Compra/fatura, contas a pagar, estoque e cadastros quando aplicavel. |

## 7. Jornada operacional

| Etapa | Acao | Validacoes | Resultado |
|---|---|---|---|
| 1 | Usuario envia XML/ZIP. | Arquivo informado, extensao permitida, empresa identificada e permissao valida. | Lote criado ou erro registrado. |
| 2 | Epros abre o lote. | XMLs validos e invalidos identificados. | Contadores do lote atualizados. |
| 3 | Epros processa cada XML. | XML valido, tipo reconhecido, emitente/empresa coerentes e duplicidade inexistente. | Registro `importacao_xml` atualizado. |
| 4 | Epros registra status da leitura. | Resultado da etapa de importacao. | `StatusImportacaoXml` finalizado ou erro com mensagem. |
| 5 | Epros cadastra ou relaciona entidades. | Dados suficientes para pessoa, produto, unidade, tributacao ou entidades relacionadas. | `StatusCadastro` finalizado ou erro com mensagem. |
| 6 | Epros salva ou gera PDF quando aplicavel. | Dados suficientes para representacao em PDF. | `StatusSalvarPdf` finalizado ou erro com mensagem. |
| 7 | Epros aplica efeitos operacionais quando aplicavel. | XML processado, cadastros relacionados, plano de contas configurado e tipo de pagamento mapeado. | Compra/fatura, contas a pagar e estoque gerados ou erro funcional. |
| 8 | Usuario consulta resultado. | Empresa e permissao validas. | Lista com dados e total de registros. |

## 8. Regras funcionais

| Codigo | Regra |
|---|---|
| IMP-001 | A importacao deve aceitar arquivo XML ou ZIP. |
| IMP-002 | A empresa deve estar identificada antes do processamento operacional. |
| IMP-003 | O Epros deve validar se o emitente do XML e a empresa operacional sao coerentes quando essa informacao estiver disponivel. |
| IMP-004 | XML invalido deve ser rejeitado e ter erro registrado. |
| IMP-005 | XML/documento duplicado deve ser bloqueado por chave, NfeId ou identificador fiscal equivalente disponivel. |
| IMP-006 | O conteudo XML deve ser preservado no registro individual de importacao. |
| IMP-007 | A etapa de leitura/importacao do XML deve possuir status proprio. |
| IMP-008 | A etapa de cadastro/relacionamento deve possuir status proprio. |
| IMP-009 | A etapa de salvamento de PDF deve possuir status proprio. |
| IMP-010 | Cada mensagem de erro de etapa deve aceitar ate 500 caracteres. |
| IMP-011 | O identificador `NfeId` deve aceitar ate 100 caracteres. |
| IMP-012 | O tipo de evento deve aceitar ate 100 caracteres. |
| IMP-013 | O nome do arquivo de lote deve aceitar ate 150 caracteres. |
| IMP-014 | O lote deve registrar quantidade de XMLs recebidos e quantidade de XMLs invalidos. |
| IMP-015 | O lote deve registrar produtos e clientes/pessoas localizados quando essa verificacao ocorrer. |
| IMP-016 | O lote deve registrar produtos e clientes/pessoas importados quando essa importacao ocorrer. |
| IMP-017 | Erro de cadastro deve indicar falha em pessoa, produto, unidade, tributacao ou entidade relacionada quando o detalhe estiver disponivel. |
| IMP-018 | Erro de PDF deve ser registrado sem impedir a preservacao do XML importado. |
| IMP-019 | A consulta de importacoes deve retornar os dados e o total de registros. |
| IMP-020 | Compra/fatura somente deve ser gerada quando o XML estiver processado e os dados necessarios estiverem completos. |
| IMP-021 | Contas a pagar somente devem ser geradas quando houver plano de contas configurado e tipo de pagamento mapeado. |
| IMP-022 | Quando compra/fatura ou contas a pagar nao puderem ser geradas, o Epros deve registrar erro funcional. |
| IMP-023 | Cancelamento importado sem documento autorizado relacionado deve ser tratado como excecao. |
| IMP-024 | Falha inesperada de salvamento de PDF deve atualizar status e mensagem da etapa de PDF. |
| IMP-025 | Registros em processamento devem evoluir para Finalizado ou Erro por etapa. |

## 9. Estados

### 9.1 Status das etapas de importacao, cadastro e PDF

| Estado | Valor | Significado |
|---|---:|---|
| NaoProcessado | 1 | Etapa ainda nao executada. |
| Processando | 2 | Etapa em execucao. |
| Finalizado | 3 | Etapa concluida com sucesso. |
| Erro | 4 | Etapa encerrada com erro registrado. |

### 9.2 Estados do lote

| Estado | Valor |
|---|---|
| Status do lote | Nao informado no material |

## 10. Excecoes

| Excecao | Tratamento |
|---|---|
| Arquivo nao informado | Rejeitar upload e informar erro. |
| Formato nao permitido | Rejeitar upload e informar que somente XML/ZIP e aceito. |
| XML invalido | Registrar erro de importacao XML. |
| Emitente divergente | Registrar erro e impedir efeitos operacionais. |
| Duplicidade | Bloquear importacao operacional duplicada. |
| Cancelamento sem autorizacao relacionada | Registrar erro funcional. |
| Erro de cadastro | Registrar mensagem em `MensagemErroCadastro`. |
| Erro ao salvar PDF | Registrar mensagem em `MensagemErroSalvarPdf`. |
| Erro ao gerar compra/fatura | Registrar erro funcional e nao gerar efeito incompleto. |
| Falta de plano de contas | Impedir geracao financeira e registrar erro. |
| Tipo de pagamento sem plano | Impedir geracao financeira e registrar erro. |
| Nenhum contas a pagar registrado | Registrar erro funcional. |

## 11. Modelo de dados funcional e implantavel

### 11.1 Entidades

| Entidade | Finalidade | Cardinalidade | Observacao |
|---|---|---|---|
| `importacao_xml` | Registrar cada XML importado, seu conteudo, tipo, identificador fiscal, status por etapa, mensagens e data de importacao. | 0..N por empresa | Empresa e tenant controlam isolamento operacional. |
| `importacao_arquivo_xml_saida` | Registrar lote/arquivo XML ou ZIP importado, com contadores, status e mensagem de erro. | 0..N por tenant | Usado para controle de processamento em lote. |

### 11.2 Relacionamentos

| Origem | Relacao | Destino | Regra |
|---|---|---|---|
| Empresa | possui | `importacao_xml` | Cada importacao operacional deve estar associada a empresa identificada. |
| Tenant | isola | `importacao_xml` | `TenantId` separa dados por ambiente/organizacao. |
| Tenant | isola | `importacao_arquivo_xml_saida` | `TenantId` separa lotes por ambiente/organizacao. |
| `importacao_arquivo_xml_saida` | agrupa | `importacao_xml` | Relacao funcional de lote com XMLs importados; chave fisica nao informada no material. |
| `importacao_xml` | pode gerar | Compra/fatura | Apenas quando dados e regras estiverem completos. |
| `importacao_xml` | pode gerar | Contas a pagar | Apenas com plano de contas e tipo de pagamento mapeados. |
| `importacao_xml` | pode alimentar | Estoque | Apenas quando a entrada fiscal/compra exigir efeito de estoque. |
| `importacao_xml` | pode cadastrar ou relacionar | Pessoas, produtos, unidades e tributacao | Controlado pela etapa de cadastro. |

### 11.3 Indices e unicidade funcional

| Item | Regra |
|---|---|
| Duplicidade fiscal | Chave, NfeId ou identificador fiscal equivalente deve impedir importacao operacional repetida. |
| Consulta por empresa | Importacoes devem ser consultaveis por empresa. |
| Consulta por tenant | Importacoes e lotes devem respeitar isolamento por `TenantId`. |
| Auditoria por data | `dataImportacao` deve permitir acompanhamento temporal quando informado. |

## 12. Dicionario de dados implantavel

### 12.1 `importacao_xml`

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| Id | Identificador | Nao informado no material | Sim | Chave primaria | Identificador interno da importacao. |
| TenantId | Texto | varchar(200) | Sim | Tenant | Isolamento da importacao. |
| EmpresaId | Numero | Nao informado no material | Condicional | Empresa | Empresa do upload; validacao operacional exige empresa identificada. |
| Xml | Texto/XML | text | Sim | Conteudo fiscal | XML importado deve ser preservado. |
| TipoDeXml | Enum/numero | NaoAplicavel=-1, NotaFiscalEntrada=1, NotaFiscalSaida=2, NotaFiscalEntradaPropria=3, NotaFiscalCancelamento=4 | Nao informado no material | Tipo de documento | Tipo funcional do XML. |
| NfeId | Texto | varchar(100) | Nao | Identificador fiscal | Identificador fiscal quando extraido. |
| StatusImportacaoXml | Enum/numero | NaoProcessado=1, Processando=2, Finalizado=3, Erro=4 | Nao | Status da etapa | Status da leitura/importacao do XML. |
| MensagemErroImportacaoXml | Texto | varchar(500) | Nao | Mensagem da etapa | Erro da etapa de importacao. |
| StatusCadastro | Enum/numero | NaoProcessado=1, Processando=2, Finalizado=3, Erro=4 | Nao | Status da etapa | Status do cadastro/relacionamento de entidades. |
| MensagemErroCadastro | Texto | varchar(500) | Nao | Mensagem da etapa | Erro de cadastro de pessoas, produtos, unidades, tributacao ou entidades relacionadas. |
| StatusSalvarPdf | Enum/numero | NaoProcessado=1, Processando=2, Finalizado=3, Erro=4 | Nao | Status da etapa | Status da geracao/salvamento de PDF. |
| MensagemErroSalvarPdf | Texto | varchar(500) | Nao | Mensagem da etapa | Erro da etapa de PDF. |
| CodigoSefaz | Codigo/numero | Nao informado no material | Nao | Retorno fiscal | Codigo retornado quando disponivel. |
| TipoEvento | Texto | varchar(100) | Nao | Evento fiscal | Evento associado quando aplicavel. |
| dataImportacao | Data/hora | Nao informado no material | Nao | Auditoria | Data da importacao. |

### 12.2 `importacao_arquivo_xml_saida`

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| Id | Identificador | Nao informado no material | Sim | Chave primaria | Identificador interno do arquivo/lote. |
| TenantId | Texto | varchar(200) | Sim | Tenant | Isolamento do lote. |
| NomeArquivo | Texto | varchar(150) | Sim | Arquivo | Nome do arquivo importado. |
| QtdXmls | Numero | int | Nao | Contador | Quantidade de XMLs no lote. |
| QtdXmlsInvalidos | Numero | int | Nao | Contador | Quantidade de XMLs invalidos. |
| QtdProdutosLocalizados | Numero | int | Nao | Contador | Produtos encontrados. |
| QtdClientesLocalizados | Numero | int | Nao | Contador | Clientes/pessoas encontrados. |
| QtdProdutosImportados | Numero | int | Nao | Contador | Produtos importados. |
| QtdClientesImportados | Numero | int | Nao | Contador | Clientes/pessoas importados. |
| Status | Enum/numero | Nao informado no material | Nao | Status do lote | Dominio final nao informado no material. |
| MensagemErro | Texto | varchar(500) | Nao | Mensagem do lote | Mensagem de erro do processamento. |

## 13. Integracoes funcionais

| Integracao | Quando ocorre | Efeito |
|---|---|---|
| Cadastros base | Durante etapa de cadastro | Pessoas, produtos, unidades e tributacao podem ser relacionados ou cadastrados quando aplicavel. |
| Compras | Quando XML de entrada possui dados suficientes | Compra/fatura pode ser criada. |
| Financeiro | Quando compra/fatura exige contas a pagar | Contas a pagar pode ser criado se plano e tipo de pagamento estiverem configurados. |
| Estoque | Quando a entrada fiscal exige movimentacao | Estoque pode ser alimentado conforme a compra/entrada processada. |
| Fiscal eletronico | Durante leitura do XML | Codigo fiscal, tipo de XML, tipo de evento e retorno fiscal ficam associados a importacao. |
| Documentos/PDF | Apos leitura do XML | PDF pode ser salvo ou ter erro registrado. |

## 14. Consultas e operacao

| Consulta/acao | Campos principais | Resultado |
|---|---|---|
| Enviar XML/ZIP | Arquivo e empresa | Lote/processamento iniciado ou erro. |
| Consultar importacoes | Empresa, filtros operacionais nao detalhados no material | Lista de importacoes com `data` e `totalRegistros`. |
| Ver diagnostico | Status e mensagens das etapas | Identificacao da etapa em erro. |
| Reprocessar/corrigir | Nao informado no material | Necessidade registrada na MC. |

## 15. Criterios de aceite

| Codigo | Criterio |
|---|---|
| CA-001 | Upload XML deve criar registro de importacao ou retornar erro funcional. |
| CA-002 | Upload ZIP deve registrar lote e contadores de XMLs. |
| CA-003 | XML invalido deve atualizar status de importacao como Erro e preencher mensagem. |
| CA-004 | XML duplicado deve ser bloqueado antes de gerar compra, financeiro ou estoque duplicado. |
| CA-005 | Erro de cadastro deve ficar separado de erro de XML e erro de PDF. |
| CA-006 | Erro de PDF deve ficar separado de erro de XML e erro de cadastro. |
| CA-007 | Mensagens de erro devem respeitar limite de 500 caracteres. |
| CA-008 | Nome do arquivo deve respeitar limite de 150 caracteres. |
| CA-009 | Consulta de importacoes deve retornar dados e total de registros. |
| CA-010 | Efeitos em compras, financeiro e estoque somente devem ocorrer quando as regras de pre-condicao forem atendidas. |

## 16. Pontos pendentes para validacao

| Ponto | Impacto |
|---|---|
| Dominio final do status do lote | Necessario para tela, filtros e testes de lote. |
| Regra fisica de ligacao entre lote e XML individual | Necessaria para auditoria completa de ZIP com multiplos XMLs. |
| Criterio final de reprocessamento | Necessario para corrigir erros sem duplicar efeitos. |
| Politica de retencao de XML/PDF | Necessaria para armazenamento, auditoria e conformidade. |
| Filtros finais da consulta | Necessarios para operacao diaria. |
| Permissoes por acao | Necessarias para implantacao segura. |

## 17. Notas de autoria

Nao foram adicionadas regras fora do material como regra definitiva. Relacoes de efeito operacional com compras, financeiro, estoque e cadastros foram descritas como condicionais porque aparecem como efeitos esperados, mas dependem de detalhamento final registrado na MC.
