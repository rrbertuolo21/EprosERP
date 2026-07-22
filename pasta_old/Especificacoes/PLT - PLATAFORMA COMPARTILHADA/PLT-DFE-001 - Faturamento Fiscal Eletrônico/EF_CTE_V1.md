# EF_CTE_V1

## 1. Identificacao

| Campo | Conteudo |
|---|---|
| Empresa | Siser |
| Sistema | Epros |
| Modulo | PLATAFORMA_COMPARTILHADA |
| Submodulo | FATURAMENTO_FISCAL_ELETRONICO |
| Documento | Especificacao funcional - CT-e |
| Versao | V1 |
| Status | Concluido |
| Data | 2026-06-07 |

## 2. Objetivo funcional

O CT-e permite ao Epros controlar conhecimento de transporte eletronico, com habilitacao do modulo, permissoes operacionais, estados fiscais, chave do CT-e, numero do CT-e, referencia a NF-e transportada, tomador, municipios, componentes, medidas e importacao de XML.

Esta EF consolida apenas o conteudo comprovado no material canonico. Os campos completos de emissao, autorizacao, cancelamento, eventos, XML de retorno, protocolo e integracoes finais ficam na MC como lacunas.

## 3. Escopo

| Area | Incluso | Status |
|---|---|---|
| Habilitacao do modulo | CT-e deve estar habilitado para aparecer/operar | Com conteudo |
| Permissoes | `cte.view`, `cte.create`, `cte.update`, `cte.delete` | Com conteudo |
| Estado fiscal | `DISPONIVEL`, `REJEITADO`, `APROVADO` | Com conteudo |
| Transmissao | `DISPONIVEL` ou `REJEITADO` podem seguir para transmissao e virar `APROVADO` | Parcial |
| Identificacao | Chave, numero CT-e, chave NF-e referenciada | Parcial |
| Tomador | Tomador do transporte | Parcial |
| Municipios | Municipios do CT-e | Parcial |
| Componentes | Filhos `componente_ctes` | Parcial |
| Medidas | Filhos `medida_ctes` | Parcial |
| Importacao XML | Importacao XML de CT-e | Parcial |
| Modelo fiscal completo | Emitente, remetente, destinatario, carga, impostos, modal, protocolo, XML autorizado, PDF, cancelamento e eventos | Incompleto |

## 4. Fora de escopo

| Item | Motivo |
|---|---|
| MDF-e | Possui EF especifica na fila macro. |
| Manifesto DFe | Possui EF especifica na fila macro. |
| NF-e saida/entrada | Possuem EF especificas. |
| Modelo completo de transporte | Nao informado no material de CT-e deste submodulo. |
| Regras fiscais completas de autorizacao e cancelamento CT-e | Nao informado no material. |

## 5. Atores e responsabilidades

| Ator | Responsabilidade | Observacao |
|---|---|---|
| Usuario fiscal | Visualizar, criar, atualizar, excluir, transmitir e importar CT-e conforme permissao. | Permissoes comprovadas para visualizar/criar/atualizar/excluir. |
| Administrador Siser | Habilitar modulo CT-e e administrar parametros/seguranca. | Parametros finais nao informados no material. |
| Epros | Controlar estado, referencia fiscal, importacao XML e transicao para aprovado quando transmitido. | Transmissao esta documentada de forma parcial. |
| Tomador | Parte informada como tomadora do transporte. | Campos detalhados nao informados no material. |

## 6. Conceitos funcionais

| Conceito | Definicao |
|---|---|
| CT-e | Conhecimento de Transporte Eletronico. |
| Chave CT-e | Chave fiscal do documento de transporte. |
| Numero CT-e | Numero operacional/fiscal do CT-e. |
| Chave NF-e | Referencia a NF-e transportada. |
| Tomador | Pessoa ou organizacao relacionada como tomadora do transporte. |
| Componente CT-e | Registro filho de componentes do CT-e. |
| Medida CT-e | Registro filho de medidas do CT-e. |
| XML importado | Arquivo XML de CT-e recebido/importado no Epros. |

## 7. Capacidades funcionais

| Capacidade | Descricao | Entrada principal | Saida esperada |
|---|---|---|---|
| Habilitar CT-e | Disponibiliza operacao CT-e apenas quando modulo estiver habilitado. | Modulo CT-e habilitado | Menus/rotinas CT-e disponiveis. |
| Controlar permissoes CT-e | Aplica permissoes de visualizar, criar, atualizar e excluir. | Permissao do usuario | Operacao permitida ou bloqueada. |
| Criar CT-e | Registra CT-e com dados comprovados disponiveis. | Chave, numero, chave NF-e, tomador, municipios, componentes e medidas | CT-e em estado operacional. |
| Atualizar CT-e | Permite ajustar CT-e conforme permissao e estado. | CT-e existente | CT-e atualizado ou bloqueado. |
| Excluir CT-e | Permite exclusao conforme permissao e restricoes finais. | CT-e existente | CT-e excluido ou bloqueado. |
| Transmitir CT-e | Permite transmissao quando estado for `DISPONIVEL` ou `REJEITADO`. | CT-e valido | Estado `APROVADO` quando aceito. |
| Importar XML CT-e | Recebe XML de CT-e. | XML | CT-e importado ou erro registrado. |
| Referenciar NF-e transportada | Vincula CT-e a chave da NF-e. | Chave NF-e | Relacao fiscal registrada. |

## 8. Regras funcionais

| Regra | Descricao | Contexto | Resultado esperado | Severidade | Fonte funcional |
|---|---|---|---|---|---|
| REG-CTE-001 | O modulo CT-e deve estar habilitado para operacao CT-e. | Acesso ao modulo | Bloquear acesso quando CT-e nao estiver habilitado. | Bloqueante | Material informa habilitacao por modulo. |
| REG-CTE-002 | Visualizacao de CT-e exige permissao `cte.view`. | Consulta/listagem | Permitir ou bloquear visualizacao. | Bloqueante | Permissao comprovada. |
| REG-CTE-003 | Criacao de CT-e exige permissao `cte.create`. | Criacao | Permitir ou bloquear criacao. | Bloqueante | Permissao comprovada. |
| REG-CTE-004 | Atualizacao de CT-e exige permissao `cte.update`. | Atualizacao | Permitir ou bloquear edicao. | Bloqueante | Permissao comprovada. |
| REG-CTE-005 | Exclusao de CT-e exige permissao `cte.delete`. | Exclusao | Permitir ou bloquear exclusao. | Bloqueante | Permissao comprovada. |
| REG-CTE-006 | CT-e pode estar nos estados `DISPONIVEL`, `REJEITADO` ou `APROVADO`. | Ciclo de vida | Registrar estado conforme dominio comprovado. | Bloqueante | Estados comprovados. |
| REG-CTE-007 | CT-e em estado `DISPONIVEL` pode seguir para transmissao. | Transmissao | Permitir tentativa de transmissao. | Alta | Transicao comprovada. |
| REG-CTE-008 | CT-e em estado `REJEITADO` pode seguir para nova transmissao. | Retransmissao | Permitir nova tentativa. | Alta | Transicao comprovada. |
| REG-CTE-009 | Transmissao aceita deve levar CT-e para `APROVADO`. | Transmissao | Atualizar estado para aprovado. | Bloqueante | Transicao comprovada. |
| REG-CTE-010 | CT-e deve suportar chave fiscal do proprio CT-e. | Identificacao | Preservar campo Chave. | Alta | Campo comprovado. |
| REG-CTE-011 | CT-e deve suportar numero do CT-e. | Identificacao | Preservar campo CteNumero. | Alta | Campo comprovado. |
| REG-CTE-012 | CT-e deve referenciar a chave da NF-e transportada. | Relacao fiscal | Preservar campo ChaveNfe. | Alta | Campo comprovado. |
| REG-CTE-013 | CT-e deve suportar tomador. | Partes | Preservar tomador. | Alta | Campo comprovado. |
| REG-CTE-014 | CT-e deve suportar municipios relacionados. | Localizacao/transporte | Preservar municipios. | Media | Campo comprovado de forma parcial. |
| REG-CTE-015 | CT-e deve suportar componentes filhos. | Detalhamento | Manter relacao com componente_ctes. | Media | Filhos comprovados. |
| REG-CTE-016 | CT-e deve suportar medidas filhas. | Detalhamento | Manter relacao com medida_ctes. | Media | Filhos comprovados. |
| REG-CTE-017 | CT-e deve suportar importacao XML. | Importacao | Receber XML e registrar resultado. | Alta | Operacao comprovada. |
| REG-CTE-018 | A EF nao deve assumir campos completos de emissao, autorizacao, cancelamento ou encerramento CT-e quando nao informados. | Especificacao | Encaminhar para MC. | Bloqueante | Material parcial. |
| REG-CTE-019 | Classificacoes tributarias devem indicar quando sao aplicaveis a CT-e. | Cadastros fiscais | Permitir filtro por aplicabilidade CT-e. | Media | Material informa indicador CT-e em classificacao tributaria. |
| REG-CTE-020 | Classificacoes tributarias devem indicar quando sao aplicaveis a CT-e OS. | Cadastros fiscais | Permitir filtro por aplicabilidade CT-e OS. | Media | Material informa indicador CT-e OS em classificacao tributaria. |
| REG-CTE-021 | Situacao de documento fiscal deve suportar situacoes que citam CT-e denegado ou numeracao inutilizada. | Escrituracao/status fiscal | Preservar dominio quando aplicavel. | Media | Dominio fiscal compartilhado informado. |

## 9. Estados e transicoes

| Estado origem | Acao | Estado destino | Regra |
|---|---|---|---|
| DISPONIVEL | Transmitir | APROVADO | Transicao comprovada. |
| REJEITADO | Transmitir | APROVADO | Transicao comprovada. |
| APROVADO | Nao informado no material | Nao informado no material | Cancelamento/eventos ficam na MC. |

## 10. Modelo de dados funcional e implantavel

O material comprova a entidade funcional `ctes` com campos principais e filhos `componente_ctes` e `medida_ctes`, mas nao informa a lista completa de colunas, tipos fisicos, obrigatoriedade detalhada, XML de retorno, protocolo, emitente, remetente, destinatario, expedidor, recebedor, carga, modal, impostos ou eventos. Para tornar a EF implantavel sem inventar, o modelo abaixo conserva os campos comprovados e marca os demais como lacuna na MC.[^1]

| Entidade funcional | Finalidade | Cardinalidade | Persistencia indicada |
|---|---|---|---|
| ctes | Controlar CT-e com estado, chave, numero, referencia NF-e, tomador e municipios. | 1 por CT-e | Comprovada parcialmente. |
| componente_ctes | Registrar componentes filhos do CT-e. | 0..N por CT-e | Comprovada parcialmente. |
| medida_ctes | Registrar medidas filhas do CT-e. | 0..N por CT-e | Comprovada parcialmente. |
| cte_importacao_xml | Controlar importacao de XML CT-e. | 0..N por CT-e/importacao | Consolidacao funcional a partir da operacao comprovada.[^1] |

### 10.1 Relacionamentos funcionais

| Origem | Relacao | Destino | Regra |
|---|---|---|---|
| ctes | referencia | NF-e transportada | Relacao pela chave da NF-e. |
| ctes | possui | componente_ctes | CT-e pode possuir componentes. |
| ctes | possui | medida_ctes | CT-e pode possuir medidas. |
| ctes | pode originar | cte_importacao_xml | Importacao XML pode criar ou atualizar dados do CT-e. |

## 11. Dicionario de dados implantavel

### 11.1 ctes

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| Id | Identificador | Nao informado no material | Sim | Chave primaria | Identificador interno.[^1] |
| Estado | Enum/texto | DISPONIVEL, REJEITADO, APROVADO | Sim | Estado fiscal | Transicoes comprovadas. |
| Chave | Texto | Nao informado no material | Nao informado no material | Chave CT-e | Chave fiscal do CT-e. |
| CteNumero | Texto/numero | Nao informado no material | Nao informado no material | Numero CT-e | Numero do CT-e. |
| ChaveNfe | Texto | Nao informado no material | Nao informado no material | Referencia NF-e | Referencia a NF-e transportada. |
| Tomador | Texto/referencia | Nao informado no material | Nao informado no material | Tomador | Estrutura final nao informada. |
| Municipios | Texto/estrutura | Nao informado no material | Nao informado no material | Municipios | Material cita municipios sem detalhar campos. |
| XmlImportado | Texto/arquivo | Nao informado no material | Nao | XML | Campo funcional de importacao; estrutura final nao informada.[^1] |
| Protocolo | Texto | Nao informado no material | Nao informado no material | Protocolo fiscal | Nao informado no material. |
| PdfCaminho | Texto | Nao informado no material | Nao informado no material | Documento auxiliar | Nao informado no material. |
| XmlCaminho | Texto | Nao informado no material | Nao informado no material | XML | Nao informado no material. |

### 11.2 componente_ctes

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| Id | Identificador | Nao informado no material | Sim | Chave primaria | Identificador interno.[^1] |
| CteId | Identificador | Nao informado no material | Sim | Relacao com ctes | Vinculo com CT-e.[^1] |
| DescricaoComponente | Texto | Nao informado no material | Nao informado no material | Componente | Nome final nao informado; material comprova componente filho.[^1] |
| ValorComponente | Decimal | Nao informado no material | Nao informado no material | Valor | Valor final nao informado; material comprova componente filho.[^1] |

### 11.3 medida_ctes

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| Id | Identificador | Nao informado no material | Sim | Chave primaria | Identificador interno.[^1] |
| CteId | Identificador | Nao informado no material | Sim | Relacao com ctes | Vinculo com CT-e.[^1] |
| TipoMedida | Texto | Nao informado no material | Nao informado no material | Medida | Nome final nao informado; material comprova medida filha.[^1] |
| QuantidadeMedida | Decimal | Nao informado no material | Nao informado no material | Quantidade | Quantidade final nao informada; material comprova medida filha.[^1] |

### 11.4 cte_importacao_xml

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| Id | Identificador | Nao informado no material | Sim | Chave primaria | Identificador interno.[^1] |
| CteId | Identificador | Nao informado no material | Nao | Relacao com ctes | Pode existir antes de identificar CT-e.[^1] |
| Xml | Texto/arquivo | Nao informado no material | Sim | XML importado | Importacao XML comprovada. |
| StatusImportacao | Enum/texto | Nao informado no material | Nao informado no material | Status | Dominio nao informado. |
| MensagemImportacao | Texto | Nao informado no material | Nao | Mensagem | Necessaria para retorno de importacao; estrutura final nao informada.[^1] |
| DataImportacao | Data/hora | Nao informado no material | Nao informado no material | Auditoria | Necessaria para rastreio; estrutura final nao informada.[^1] |

## 12. Fluxos funcionais

### 12.1 Acessar modulo CT-e

| Passo | Responsavel | Acao | Entrada | Saida |
|---|---|---|---|---|
| 1 | Usuario fiscal | Acessa area de CT-e. | Usuario e empresa | Pedido de acesso. |
| 2 | Epros | Verifica se CT-e esta habilitado. | Configuracao do modulo | Acesso permitido ou bloqueado. |
| 3 | Epros | Verifica permissao `cte.view`. | Perfil do usuario | Lista/consulta disponivel ou bloqueada. |

### 12.2 Criar ou atualizar CT-e

| Passo | Responsavel | Acao | Entrada | Saida |
|---|---|---|---|---|
| 1 | Usuario fiscal | Solicita criacao ou atualizacao. | Dados CT-e disponiveis | Operacao iniciada. |
| 2 | Epros | Verifica permissao adequada. | `cte.create` ou `cte.update` | Operacao permitida ou bloqueada. |
| 3 | Epros | Registra campos comprovados. | Estado, chave, numero, chave NF-e, tomador, municipios, componentes e medidas | CT-e salvo. |
| 4 | Epros | Mantem lacunas sem preenchimento automatico. | Campos nao informados | Itens pendentes ficam na MC. |

### 12.3 Transmitir CT-e

| Passo | Responsavel | Acao | Entrada | Saida |
|---|---|---|---|---|
| 1 | Usuario fiscal | Solicita transmissao. | CT-e em `DISPONIVEL` ou `REJEITADO` | Transmissao iniciada. |
| 2 | Epros | Valida estado permitido. | Estado atual | Bloqueio ou envio. |
| 3 | Epros | Transmite CT-e. | Dados CT-e | Retorno fiscal. |
| 4 | Epros | Atualiza estado quando aceito. | Retorno aceito | Estado `APROVADO`. |

### 12.4 Importar XML CT-e

| Passo | Responsavel | Acao | Entrada | Saida |
|---|---|---|---|---|
| 1 | Usuario fiscal | Envia XML CT-e. | Arquivo XML | Importacao iniciada. |
| 2 | Epros | Processa XML. | XML | CT-e identificado ou erro. |
| 3 | Epros | Registra resultado da importacao. | Dados extraidos/erro | CT-e importado ou mensagem registrada.[^1] |

## 13. Validacoes e mensagens

| Codigo | Mensagem | Condicao |
|---|---|---|
| MSG-CTE-001 | Modulo CT-e nao habilitado. | Usuario tenta acessar CT-e sem modulo habilitado. |
| MSG-CTE-002 | Usuario sem permissao para visualizar CT-e. | Falta `cte.view`. |
| MSG-CTE-003 | Usuario sem permissao para criar CT-e. | Falta `cte.create`. |
| MSG-CTE-004 | Usuario sem permissao para atualizar CT-e. | Falta `cte.update`. |
| MSG-CTE-005 | Usuario sem permissao para excluir CT-e. | Falta `cte.delete`. |
| MSG-CTE-006 | CT-e so pode ser transmitido quando estiver disponivel ou rejeitado. | Estado diferente dos estados comprovados para transmissao. |
| MSG-CTE-007 | XML de CT-e nao informado. | Importacao sem XML. |
| MSG-CTE-008 | Chave da NF-e transportada nao informada. | Regra final de obrigatoriedade pendente; alerta funcional. |

## 14. Integracoes

| Integracao | Direcao | Dados | Regra | Lacuna |
|---|---|---|---|---|
| NF-e | Entrada/referencia | Chave da NF-e transportada | CT-e referencia NF-e. | Validacao completa da chave e existencia da NF-e. |
| Cadastros Base | Entrada | Tomador e municipios | Dados mestres nao devem ser duplicados. | Campos finais do tomador e municipios. |
| Parametros fiscais | Entrada | Habilitacao CT-e, ambiente, certificado e transmissao | Necessario para operacao fiscal. | Parametros CT-e nao informados. |
| MDF-e | Saida/relacao futura | CT-e pode alimentar MDF-e. | Nao detalhado nesta EF. | EF MDF-e na fila. |
| Importacao XML | Entrada | XML CT-e | Importacao comprovada. | Estrutura final de armazenamento e retorno. |

## 15. Permissoes e seguranca

| Permissao | Operacao |
|---|---|
| `cte.view` | Visualizar/listar/consultar CT-e. |
| `cte.create` | Criar CT-e. |
| `cte.update` | Atualizar CT-e. |
| `cte.delete` | Excluir CT-e. |

| Controle | Regra |
|---|---|
| Habilitacao | CT-e depende de modulo habilitado. |
| Autenticacao | Nao informado no material. |
| Auditoria | Nao informado no material; deve ser definida na MC. |
| Segregacao por empresa/tenant | Nao informado no material para CT-e. |

## 16. Relatorios e consultas

| Consulta | Filtros comprovados | Resultado |
|---|---|---|
| Listagem CT-e | Nao informado no material | CT-e com estado, chave, numero, chave NF-e, tomador e municipios quando disponiveis. |
| Consulta por estado | Estado `DISPONIVEL`, `REJEITADO`, `APROVADO` | Documentos conforme estado. |
| Consulta por NF-e transportada | Chave NF-e | CT-e relacionado a NF-e. |
| Importacao XML | XML enviado | Resultado da importacao. |

## 17. Criterios de aceite

| Codigo | Criterio |
|---|---|
| CA-CTE-001 | Usuario sem modulo CT-e habilitado nao deve acessar rotinas CT-e. |
| CA-CTE-002 | Usuario sem `cte.view` nao deve visualizar CT-e. |
| CA-CTE-003 | Usuario sem `cte.create` nao deve criar CT-e. |
| CA-CTE-004 | Usuario sem `cte.update` nao deve atualizar CT-e. |
| CA-CTE-005 | Usuario sem `cte.delete` nao deve excluir CT-e. |
| CA-CTE-006 | CT-e em `DISPONIVEL` deve poder seguir para transmissao. |
| CA-CTE-007 | CT-e em `REJEITADO` deve poder seguir para transmissao. |
| CA-CTE-008 | Transmissao aceita deve atualizar estado para `APROVADO`. |
| CA-CTE-009 | CT-e deve preservar chave do CT-e, numero do CT-e e chave da NF-e quando informados. |
| CA-CTE-010 | Importacao XML deve registrar resultado de sucesso ou falha. |
| CA-CTE-011 | Campos nao informados no material nao devem ser preenchidos por suposicao na EF. |

## 18. Lacunas encaminhadas para MC

| Lacuna | Impacto |
|---|---|
| Modelo completo do CT-e | Necessario para desenvolvimento completo de emissao/autorizacao. |
| Campos de emitente, remetente, destinatario, expedidor, recebedor, tomador detalhado, carga, modal, valores e impostos | Necessario para documento fiscal completo. |
| XML autorizado, XML de envio, protocolo, PDF e armazenamento | Necessario para evidencia fiscal. |
| Cancelamento, carta de correcao, inutilizacao, eventos e encerramentos CT-e | Nao informado no material. |
| Regras fiscais por UF, ambiente e certificado | Necessario para transmissao real. |
| Integracao com MDF-e | Necessario para cadeia logistica. |
| Persistencia completa de componentes e medidas | Campos finais nao informados. |
| Auditoria, seguranca, tenant e permissoes avancadas | Necessario para operacao segura. |

## 19. Proximo passo

O proximo documento especifico da fila macro e `EF_MDFE`, detalhando MDF-e conforme material disponivel.

[^1]: Consolidacao funcional criada para tornar implantavel a especificacao, pois o material comprova entidade CT-e, componentes, medidas e importacao XML, mas nao informa tabela final completa, chaves fisicas, colunas de componentes/medidas, auditoria, retorno de importacao, XML/PDF ou protocolo para CT-e.
