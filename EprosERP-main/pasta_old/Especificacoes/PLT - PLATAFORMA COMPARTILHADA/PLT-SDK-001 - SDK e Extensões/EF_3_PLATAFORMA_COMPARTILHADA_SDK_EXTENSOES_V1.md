# EF_3_PLATAFORMA_COMPARTILHADA_SDK_EXTENSOES_V1

**Projeto:** Epros  
**Empresa:** Siser  
**Tipo de documento:** Especificacao Funcional definitiva  
**Versao:** V1  
**Modulo:** PLATAFORMA_COMPARTILHADA  
**Submodulo:** SDK_EXTENSOES  
**Status:** Concluido  
**Ultima atualizacao:** 2026-06-09

## 1. Controle do documento

| Item | Conteudo |
|---|---|
| Responsavel pela elaboracao | Agente de analise funcional |
| Responsavel pela validacao funcional | Siser |
| Responsavel pela validacao tecnica | Siser |
| Area dona do processo | Plataforma Compartilhada |
| Publico-alvo | Produto, negocio, implantacao, desenvolvimento, suporte, operacao |
| Fonte de verdade | Esta EF e a fonte funcional definitiva do submodulo |

## 2. Objetivo funcional

O submodulo SDK e Extensoes do Epros deve governar extensibilidade da plataforma por meio de registry de extensoes, manifestos versionados, dependencias, instalacao por tenant, ativacao/desativacao, permissoes por modulo, menus, eventos de dominio, callbacks assinados, metadados dinamicos, pacotes assinados, hooks e utilitarios transversais homologados.

| Pergunta | Resposta |
|---|---|
| Para que o submodulo existe? | Para permitir que o Epros receba extensoes controladas, auditadas, versionadas e seguras. |
| Que problema de negocio resolve? | Evita customizacoes soltas, sem rastreabilidade, sem permissao e sem controle de tenant. |
| Qual resultado operacional deve produzir? | Extensoes registradas, instaladas, habilitadas, versionadas, auditadas e integradas a eventos, menus e permissoes. |
| Quais areas dependem dele? | API Gateway, Aplicativo, Permissoes de Menu, Integracoes e Conectores, Workflow, Compliance, Relatorios e modulos extensiveis. |

## 3. Escopo funcional

### 3.1 Dentro do escopo

| Capacidade | Descricao | Observacao |
|---|---|---|
| Registry de extensoes | Manter catalogo de extensoes, versoes, status, autor, descricao e dependencias. | Material informa registry versionado e tabela de modulos. |
| Manifesto | Exigir manifesto com metadados, provedores, arquivos, dependencias, menus e permissoes. | Material informa manifest/installdefs e stubs. |
| Instalacao por tenant | Instalar, migrar, sincronizar e habilitar extensao por tenant. | Material informa sincronizacao tenant e migracao por modulo. |
| Ativacao/desativacao | Controlar status enabled/disabled por tenant. | Material informa module_status enabled/disabled. |
| Permissoes por modulo | Associar permissao de extensao a papeis. | Material informa JSON com module_name, module_alias e module_permission. |
| Menus e UI extension points | Permitir que extensoes publiquem entradas de menu em posicoes controladas. | Material informa placements main, settings, tabs, profile e topnav. |
| Eventos de dominio | Permitir assinatura de eventos por extensao e extracao de payload padronizado. | Material informa eventos com module, type e extractor. |
| Callbacks seguros | Exigir assinatura HMAC e protecao contra replay para callbacks. | Material informa HMAC e replay protection como decisao. |
| Metadados dinamicos | Controlar labels, layouts, campos dinamicos e cache de metadados por pipeline auditavel. | Material informa customizacao visual, cache e campos dinamicos. |
| Utilitarios homologados | Centralizar validacoes, mensagens, parametros, calculos, XML, impressao e criptografia de strings. | Material informa pacote transversal utilitario. |
| Auditoria | Registrar alteracoes de pacote, metadado, permissao, instalacao e ativacao. | Material informa lacuna de trilha uniforme. |

### 3.2 Fora do escopo

| Item fora do escopo | Motivo | Destino correto |
|---|---|---|
| Regra comercial interna de uma extensao | SDK governa extensibilidade, nao substitui modulo funcional. | Modulo/Extensao dona |
| Marketplace comercial completo | Material informa registry e pacotes, mas nao modelo comercial. | Aplicativo/SaaS |
| Execucao fiscal, financeira ou estoque | Utilitarios podem apoiar, mas regra final pertence ao modulo dono. | Modulos funcionais |
| Customizacao sem governanca | Epros deve usar pipeline controlado, nao alteracao direta sem trilha. | SDK/Compliance |
| Segredos de conectores externos | SDK pode declarar necessidade; armazenamento tecnico pertence a Integracoes. | Integracoes e Conectores |

## 4. Glossario e conceitos funcionais

| Termo | Definicao funcional | Observacoes |
|---|---|---|
| Extensao | Pacote homologado capaz de adicionar metadados, menus, eventos ou capacidades ao Epros. | Deve possuir manifesto. |
| Registry | Catalogo controlado de extensoes e versoes disponiveis. | Base do ciclo de vida. |
| Manifesto | Documento estruturado com metadados, dependencias, menus, eventos, permissoes e instalacao. | Campo final depende de validacao. |
| Hook | Ponto de extensao no qual uma extensao pode reagir a evento ou renderizacao permitida. | Material informa eventos e menus. |
| Evento de dominio | Ocorrencia publicada pelo Epros para extensoes autorizadas. | Payload deve ser padronizado. |
| Extractor | Contrato que transforma evento em payload JSON padronizado. | Termo funcional preservado. |
| Permissao de modulo | Nivel de acesso de um papel a uma extensao. | Valores informados: none, view, manage, admin, yes, no. |
| Metadado dinamico | Label, layout, campo ou dropdown configuravel por extensao. | Deve ter trilha e cache controlado. |
| Callback assinado | Chamada externa com assinatura verificavel. | HMAC e replay protection informados. |

## 5. Atores, papeis e responsabilidades

| Ator/Papel | Responsabilidade | Permissoes esperadas | Restricoes |
|---|---|---|---|
| Administrador Siser | Homologar extensoes, versoes, dependencias e politicas. | Criar registry, aprovar, revogar, publicar. | Deve auditar impacto. |
| Administrador do tenant | Instalar, habilitar e desabilitar extensoes permitidas. | Instalar, ativar, configurar, sincronizar permissoes. | Nao instala extensao nao homologada. |
| Gestor de permissoes | Ajustar permissao por papel e extensao. | Gerir none/view/manage/admin/yes/no conforme matriz. | Deve respeitar papeis do Aplicativo. |
| Desenvolvedor de extensao | Entregar pacote e manifesto conforme contrato. | Submeter extensao para homologacao. | Nao altera metadado produtivo sem pipeline. |
| Epros | Validar, instalar, migrar, sincronizar, auditar e publicar eventos. | Automacao sistemica. | Nao executa extensao inativa ou sem permissao. |
| Suporte | Diagnosticar instalacao, eventos, menus e falhas. | Consultar logs e status. | Nao acessa segredos ou altera codigo. |

## 6. Visao operacional do submodulo

A Siser registra uma extensao no registry com nome, alias, identificador unico, descricao, autor, versao, status, dependencias, manifesto e escopo. Quando uma extensao e liberada para um tenant, o Epros sincroniza o registro do tenant, executa etapas de instalacao/migracao declaradas, registra status e atualiza permissoes por papel.

Extensoes habilitadas podem declarar menus, hooks, eventos e metadados dinamicos. O Epros renderiza menus apenas quando a extensao esta habilitada, o usuario possui permissao e o ponto de extensao e permitido. Eventos de dominio usam payload padronizado com modulo, tipo e extractor. Callbacks externos devem usar assinatura HMAC e protecao contra replay.

Customizacoes de labels, layouts, campos e cache devem ocorrer por pipeline controlado, com auditoria e separacao entre contrato de metadados e persistencia tecnica. O material nao informa modelo final completo para manifesto, instalador, assinaturas, sandbox e versionamento; esta EF consolida o modelo funcional implantavel com nota de autoria.[^nota1]

## 7. Capacidades funcionais

### 7.1 Registry e manifesto

| Item | Especificacao |
|---|---|
| Objetivo | Controlar extensoes homologadas e suas versoes. |
| Acionamento | Cadastro, homologacao, publicacao, revogacao ou atualizacao. |
| Pre-condicoes | Usuario Siser autorizado e pacote submetido. |
| Dados de entrada | Nome, alias, uniqueid, descricao, autor, versao, status, manifesto e dependencias. |
| Processamento | O Epros valida manifesto, dependencias, assinatura, versao e compatibilidade. |
| Resultado esperado | Extensao registrada e pronta para liberacao. |
| Pos-condicoes | Tenant autorizado pode instalar ou habilitar. |
| Excecoes | Manifesto invalido, dependencia ausente, assinatura invalida, versao incompativel. |
| Auditoria | Usuario, versao, acao, manifesto, resultado e erro. |

### 7.2 Instalacao por tenant

| Item | Especificacao |
|---|---|
| Objetivo | Instalar extensao de forma isolada por tenant. |
| Acionamento | Administrador do tenant solicita instalacao. |
| Pre-condicoes | Extensao homologada, tenant autorizado, dependencias atendidas. |
| Dados de entrada | Tenant, extensao, versao, parametros e permissao inicial. |
| Processamento | O Epros registra a extensao no tenant, executa etapas declaradas, sincroniza permissoes e marca status. |
| Resultado esperado | Extensao instalada como enabled ou disabled conforme politica. |
| Pos-condicoes | Menus, eventos e permissoes podem ficar disponiveis. |
| Excecoes | Falha de instalacao, dependencia ausente, migracao rejeitada, permissao insuficiente. |
| Auditoria | Tenant, extensao, versao, etapas executadas, status e erro. |

### 7.3 Permissoes por extensao

| Item | Especificacao |
|---|---|
| Objetivo | Controlar acesso de papeis a extensoes. |
| Acionamento | Instalacao, edicao de papel, sincronizacao periodica ou habilitacao. |
| Pre-condicoes | Extensao instalada e papel existente. |
| Dados de entrada | Papel, module_name, module_alias, module_permission. |
| Processamento | O Epros preserva permissao existente, aplica padrao por papel e sincroniza extensoes habilitadas. |
| Resultado esperado | Papel possui permissao consistente para a extensao. |
| Pos-condicoes | Menus e acoes respeitam permissao. |
| Excecoes | JSON invalido, extensao ausente, papel inexistente, permissao fora do dominio. |
| Auditoria | Alteracao por papel, usuario, antes/depois e origem. |

### 7.4 Menus e pontos de extensao visual

| Item | Especificacao |
|---|---|
| Objetivo | Permitir que extensoes publiquem menus em posicoes permitidas. |
| Acionamento | Renderizacao da interface ou sincronizacao de menus. |
| Pre-condicoes | Extensao habilitada, menu declarado, usuario autorizado. |
| Dados de entrada | Placement, parent, title, tipo, user_type, user_module_role e chave sanitizada. |
| Processamento | O Epros valida declaracao, permissao e visibilidade antes de exibir. |
| Resultado esperado | Menu visivel apenas para usuarios autorizados. |
| Pos-condicoes | Usuario acessa funcionalidade da extensao. |
| Excecoes | Placement invalido, parent ausente, permissao inexistente, extensao inativa. |
| Auditoria | Erros de menu e alteracoes de declaracao. |

### 7.5 Eventos e callbacks

| Item | Especificacao |
|---|---|
| Objetivo | Permitir que extensoes assinem eventos e recebam payloads padronizados. |
| Acionamento | Evento de dominio publicado pelo Epros. |
| Pre-condicoes | Extensao ativa, evento declarado, extractor homologado e permissao. |
| Dados de entrada | Evento, modulo, tipo, extractor, payload e destino. |
| Processamento | O Epros monta payload JSON padronizado, assina callback quando externo e registra entrega. |
| Resultado esperado | Evento entregue ou falha registrada. |
| Pos-condicoes | Extensao processa evento conforme contrato. |
| Excecoes | Evento nao declarado, extractor ausente, callback sem assinatura, replay detectado. |
| Auditoria | Evento, payload mascarado, entrega, status e erro. |

### 7.6 Metadados dinamicos e customizacoes

| Item | Especificacao |
|---|---|
| Objetivo | Permitir evolucao controlada de labels, layouts, campos e dropdowns. |
| Acionamento | Usuario autorizado altera metadado ou pacote aplica atualizacao. |
| Pre-condicoes | Extensao habilitada, pipeline aprovado e permissao. |
| Dados de entrada | Tipo de metadado, entidade, campo, label, layout, dropdown, versao e justificativa. |
| Processamento | O Epros valida contrato, grava metadado, constroi cache e registra historico. |
| Resultado esperado | Metadado atualizado com trilha. |
| Pos-condicoes | Interface ou API passa a refletir alteracao conforme cache. |
| Excecoes | Campo invalido, quebra de contrato, permissao ausente, falha de cache. |
| Auditoria | Antes/depois, usuario, versao e motivo. |

### 7.7 Utilitarios homologados

| Item | Especificacao |
|---|---|
| Objetivo | Disponibilizar utilitarios transversais padronizados. |
| Acionamento | Modulo do Epros usa servico utilitario. |
| Pre-condicoes | Utilitario homologado e permitido para o modulo. |
| Dados de entrada | Parametros globais, validacoes, calculos, mensagens, XML, impressao ou criptografia de strings. |
| Processamento | O Epros executa funcao homologada sem expor dependencia tecnica antiga. |
| Resultado esperado | Resultado padronizado e auditavel quando sensivel. |
| Pos-condicoes | Modulo consumidor aplica sua propria regra final. |
| Excecoes | Parametro ausente, formato invalido, utilitario nao homologado. |
| Auditoria | Uso sensivel, erro e contexto. |

## 8. Regras de negocio

| Regra | Descricao | Condicao | Resultado | Severidade | Observacoes |
|---|---|---|---|---|---|
| SDK-001 | Todo registro de extensao deve possuir tenant quando instalado em tenant. | Instalacao/uso por tenant. | Operacao sem tenant e bloqueada. | Bloqueante | Material informa TenantId obrigatorio. |
| SDK-002 | Registro principal deve possuir codigo, status e responsavel. | Cadastro e manutencao. | Ausencia bloqueia persistencia. | Bloqueante | Campos informados. |
| SDK-003 | Novo registro nasce em Rascunho. | Criacao valida. | Status Rascunho. | Bloqueante | Fluxo informado. |
| SDK-004 | Rascunho pode ser submetido para EmAnalise por operador. | Submissao. | Status EmAnalise. | Normal | Fluxo informado. |
| SDK-005 | EmAnalise pode ser aprovado e tornar-se Ativo. | Aprovacao. | Status Ativo. | Bloqueante | Fluxo informado. |
| SDK-006 | EmAnalise pode ser rejeitado e voltar a Rascunho com motivo. | Rejeicao. | Status Rascunho. | Normal | Fluxo informado. |
| SDK-007 | Ativo pode ser inativado ou encerrado por gestor. | Gestao. | Status Inativo ou Encerrado. | Normal | Fluxo informado. |
| SDK-008 | Inativo pode ser reativado por gestor. | Reativacao. | Status Ativo. | Normal | Fluxo informado. |
| SDK-009 | Transicoes devem registrar usuario, timestamp e IP quando disponivel. | Alteracao de estado. | Historico gravado. | Bloqueante | Auditoria informada. |
| SDK-010 | Eventos de dominio devem ser publicados apos confirmacao transacional. | Persistencia concluida. | Evento publicado. | Normal | Material informa eventos apos commit. |
| SDK-011 | Extensao deve possuir manifesto valido antes de homologacao. | Cadastro/publicacao. | Homologacao bloqueada. | Bloqueante | Material informa manifestos e instalacao. |
| SDK-012 | Extensao deve possuir versao e dependencias declaradas. | Cadastro/publicacao. | Pacote sem versao/dependencia e bloqueado. | Bloqueante | Material informa semver e dependencias. |
| SDK-013 | Extensao instalada no tenant deve ter status enabled ou disabled. | Instalacao/sincronizacao. | Status fora do dominio e rejeitado. | Bloqueante | Dominio informado. |
| SDK-014 | Extensao habilitada deve atualizar lista de extensoes ativas do tenant. | Enable/disable. | Lista ativa sincronizada. | Normal | Material informa enabled modules. |
| SDK-015 | Instalacao deve sincronizar registro da extensao no tenant. | Instalacao por tenant. | Registro tenant criado/atualizado. | Bloqueante | Material informa updateOrInsert. |
| SDK-016 | Instalacao pode executar migracao declarada por extensao. | Instalacao/upgrade. | Etapa registrada e auditada. | Bloqueante | Material informa migrate por modulo. |
| SDK-017 | Permissao por extensao deve preservar valor existente quando sincronizada. | Sync de permissoes. | Permissao anterior mantida. | Normal | Regra informada. |
| SDK-018 | Papel administrador recebe permissao padrao elevada quando aplicavel. | Criacao/sync de papel. | Permissao admin aplicada. | Normal | Material informa default admin. |
| SDK-019 | Demais papeis recebem permissao padrao restrita quando aplicavel. | Criacao/sync de papel. | Permissao none aplicada. | Normal | Material informa default demais none. |
| SDK-020 | Permissao de extensao deve usar estrutura module_name, module_alias e module_permission. | Persistencia de permissao. | JSON/estrutura valida. | Bloqueante | Campos informados. |
| SDK-021 | Valores de permissao devem respeitar dominio none, view, manage, admin, yes ou no. | Edicao de permissao. | Valor invalido e rejeitado. | Bloqueante | Dominio informado. |
| SDK-022 | Leitura de permissao deve tolerar JSON invalido retornando vazio seguro. | Consulta de permissao. | Nenhum acesso indevido. | Bloqueante | Parse tolerante informado. |
| SDK-023 | Chave de acesso de extensao deve ser normalizada/sanitizada. | Consulta por papel. | Chave consistente. | Normal | Material informa sanitize. |
| SDK-024 | Menus de extensao so aparecem quando a extensao esta habilitada. | Renderizacao. | Menu oculto se inativa. | Bloqueante | Regra informada. |
| SDK-025 | Menu deve declarar placement, parent e title. | Cadastro/renderizacao. | Menu incompleto e rejeitado. | Bloqueante | Campos obrigatorios informados. |
| SDK-026 | Tipos de menu devem respeitar single, dropdown ou dropdown-child. | Declaracao de menu. | Tipo invalido e rejeitado. | Normal | Dominio informado. |
| SDK-027 | Visibilidade de menu deve respeitar user_type. | Renderizacao. | Menu aparece apenas para perfil permitido. | Bloqueante | Regra informada. |
| SDK-028 | Visibilidade de menu deve respeitar permissao de modulo do papel. | Renderizacao. | Menu oculto sem permissao. | Bloqueante | Regra informada. |
| SDK-029 | Eventos de extensao devem declarar modulo, tipo e extractor. | Cadastro de evento. | Evento incompleto e rejeitado. | Bloqueante | Regra informada. |
| SDK-030 | Extractor deve produzir payload JSON padronizado. | Evento publicado. | Payload consistente. | Bloqueante | Regra informada. |
| SDK-031 | Callback externo deve exigir HMAC e protecao contra replay. | Entrega externa. | Callback inseguro e bloqueado. | Bloqueante | Decisao informada. |
| SDK-032 | Customizacao de metadado deve manter rastreabilidade. | Alterar label, layout, campo ou dropdown. | Alteracao sem trilha e bloqueada. | Bloqueante | Material aponta lacuna de trilha. |
| SDK-033 | Metadados dinamicos devem ser aplicados via pipeline controlado. | Customizacao. | Alteracao direta sem pipeline e bloqueada. | Bloqueante | Decisao informada. |
| SDK-034 | Cache de metadados deve ser reconstruido apos alteracao aprovada. | Alteracao de metadado. | Cache atualizado. | Normal | Material informa cache. |
| SDK-035 | Parametros globais devem ser governados por tenant/empresa quando aplicavel. | Uso de utilitario. | Parametro fora do contexto e bloqueado. | Bloqueante | Material informa parametros globais. |
| SDK-036 | Validacao de CPF/CNPJ deve estar disponivel como utilitario homologado. | Modulo consumidor solicita validacao. | Retorna valido/invalido. | Normal | Regra informada. |
| SDK-037 | Mascaras de entrada numerica devem ser padronizadas. | Entrada numerica. | Formato consistente. | Normal | Regra informada. |
| SDK-038 | Calculos fiscais, comerciais e boleto devem ser utilitarios, mas a regra final pertence ao modulo dono. | Uso por modulo. | Resultado auxiliar sem substituir regra funcional. | Bloqueante | Fronteira funcional. |
| SDK-039 | Serializacao XML generica deve ser padronizada. | Gerar/ler XML. | XML consistente. | Normal | Regra informada. |
| SDK-040 | Impressao direta deve ser roteada por capacidade homologada. | Solicitar impressao. | Impressao auditavel quando sensivel. | Normal | Regra informada. |
| SDK-041 | Strings sensiveis devem usar criptografia homologada. | Armazenar segredo ou parametro sensivel. | Texto protegido. | Bloqueante | Regra informada sem algoritmo final. |

## 9. Parametros de configuracao

| Parametro | Finalidade | Tipo/formato | Valor padrao | Obrigatorio | Nivel | Quem pode alterar | Impacto |
|---|---|---|---|---|---|---|---|
| ExtensoesHabilitadas | Permitir uso de extensoes no tenant. | Booleano | Nao informado no material | Sim | Tenant | Administrador Siser | Bloqueia instalacao. |
| RegistryUrl | Origem de consulta do registry. | URL/texto | Nao informado no material | Condicional | Global | Siser | Controla catalogo. |
| PoliticaAssinaturaPacote | Exigir pacote assinado. | Booleano | Nao informado no material | Sim | Global | Siser | Bloqueia pacotes nao assinados. |
| PoliticaCallbackHmac | Exigir HMAC em callbacks. | Booleano | Obrigatorio por regra | Sim | Global/tenant | Siser | Seguranca externa. |
| ReplayWindow | Janela de protecao contra replay. | Duracao | Nao informado no material | Sim | Global/tenant | Siser | Evita reenvio malicioso. |
| SyncPermissoesFrequencia | Frequencia de sincronizacao de permissoes. | Periodo | Nao informado no material | Sim | Tenant | Administrador | Controla atualizacao. |
| PermissaoPadraoAdmin | Permissao inicial de admin. | Enum | admin | Sim | Tenant | Administrador | Padrao informado. |
| PermissaoPadraoDemais | Permissao inicial dos demais papeis. | Enum | none | Sim | Tenant | Administrador | Padrao informado. |
| MetadadosPipelineObrigatorio | Exigir pipeline para alterar metadados. | Booleano | Sim | Sim | Global | Siser | Evita alteracao direta. |
| RetencaoLogsExtensao | Prazo de logs e auditoria. | Periodo | Nao informado no material | Sim | Global/tenant | Compliance | Afeta auditoria. |

## 10. Modelo de dados funcional e implantavel

### 10.1 Visao geral do modelo

O material informa entidade principal com Id, TenantId, Codigo, Status e ResponsavelId, historico com Acao, UsuarioId e PayloadJson, anexo com ArquivoId, tabela de extensoes por tenant com module_id, module_name, module_alias, module_uniqueid, module_description, module_author, module_version e module_status, permissao por papel em JSON, registry central, ativador de status, menus, eventos, manifestos e metadados dinamicos. As entidades abaixo consolidam esse material em modelo funcional implantavel do Epros.[^nota1]

| Grupo de dados | Entidades/tabelas | Papel funcional | Observacoes |
|---|---|---|---|
| Registry | `sdk_extensao`, `sdk_extensao_versao`, `sdk_manifesto` | Catalogo global e versoes. | Preserva campos module*. |
| Tenant | `sdk_tenant_extensao`, `sdk_instalacao_etapa` | Instalacao e status por tenant. | Preserva enabled/disabled. |
| Permissoes | `sdk_permissao_papel` | Permissoes por papel e extensao. | Preserva JSON informado. |
| Menus | `sdk_menu_extensao` | Entradas de menu e visibilidade. | Preserva placements e tipos. |
| Eventos | `sdk_evento_extensao`, `sdk_evento_entrega` | Assinatura e entrega de eventos. | Preserva module/type/extractor. |
| Metadados | `sdk_metadado_customizado`, `sdk_cache_metadado` | Labels, layouts, campos e cache. | Criado para pipeline controlado. |
| Utilitarios | `sdk_utilitario`, `sdk_utilitario_execucao` | Funcoes transversais homologadas. | Criado a partir do pacote utilitario. |
| Auditoria | `sdk_historico`, `sdk_anexo` | Trilha e anexos. | Campos informados. |

## 11. Dicionario de dados implantavel

### 11.1 `sdk_extensao`

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| id | UUID/inteiro | Nao informado no material | Sim | PK | Identificador. |
| codigo | Texto | Nao informado no material | Sim | Unico | Codigo funcional. |
| module_id | Texto/inteiro | Nao informado no material | Condicional |  | Campo informado. |
| module_name | Texto | Nao informado no material | Sim |  | Campo informado. |
| module_alias | Texto | Nao informado no material | Sim |  | Campo informado. |
| module_uniqueid | Texto | Nao informado no material | Sim | Unico | Campo informado. |
| module_description | Texto | Nao informado no material | Nao |  | Campo informado. |
| module_author_name | Texto | Nao informado no material | Nao |  | Derivado de module_author_*. |
| status | Enum | Rascunho/EmAnalise/Ativo/Inativo/Encerrado | Sim |  | Workflow informado. |
| responsavel_id | UUID/inteiro | Nao informado no material | Sim | FK usuario/pessoa | Campo informado. |
| criado_em | Data/hora | ISO 8601 | Sim |  | Criacao. |
| atualizado_em | Data/hora | ISO 8601 | Sim |  | Atualizacao. |

### 11.2 `sdk_extensao_versao`

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| id | UUID/inteiro | Nao informado no material | Sim | PK | Identificador. |
| extensao_id | UUID/inteiro | Nao informado no material | Sim | FK sdk_extensao | Extensao. |
| module_version | Texto/semver | Nao informado no material | Sim |  | Campo informado. |
| checksum | Texto | Nao informado no material | Condicional |  | Criado para integridade.[^nota1] |
| assinatura_valida | Booleano | true/false | Sim |  | Pacote assinado. |
| manifesto_id | UUID/inteiro | Nao informado no material | Sim | FK sdk_manifesto | Manifesto da versao. |
| status | Enum | Publicada/Revogada/Obsoleta | Sim |  | Ciclo de versao. |

### 11.3 `sdk_manifesto`

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| id | UUID/inteiro | Nao informado no material | Sim | PK | Identificador. |
| extensao_id | UUID/inteiro | Nao informado no material | Sim | FK sdk_extensao | Extensao. |
| manifest_json | JSON | Nao informado no material | Sim |  | Manifesto completo. |
| dependencias_json | JSON | Nao informado no material | Nao |  | Dependencias. |
| providers_json | JSON | Nao informado no material | Nao |  | Provedores declarados. |
| files_json | JSON | Nao informado no material | Nao |  | Arquivos declarados. |
| menus_json | JSON | Nao informado no material | Nao |  | Menus declarados. |
| permissoes_json | JSON | Nao informado no material | Nao |  | Permissoes declaradas. |
| valido | Booleano | true/false | Sim |  | Resultado de validacao. |

### 11.4 `sdk_tenant_extensao`

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| id | UUID/inteiro | Nao informado no material | Sim | PK | Identificador. |
| tenant_id | UUID/inteiro | Nao informado no material | Sim | FK tenant | Campo TenantId. |
| extensao_id | UUID/inteiro | Nao informado no material | Sim | FK sdk_extensao | Extensao instalada. |
| versao_id | UUID/inteiro | Nao informado no material | Sim | FK sdk_extensao_versao | Versao instalada. |
| module_status | Enum | enabled/disabled | Sim |  | Dominio informado. |
| instalado_em | Data/hora | ISO 8601 | Nao |  | Data de instalacao. |
| habilitado_em | Data/hora | ISO 8601 | Nao |  | Data de habilitacao. |
| erro | Texto | Nao informado no material | Nao |  | Falha de instalacao. |

### 11.5 `sdk_permissao_papel`

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| id | UUID/inteiro | Nao informado no material | Sim | PK | Identificador. |
| tenant_id | UUID/inteiro | Nao informado no material | Sim | FK tenant | Segregacao. |
| papel_id | UUID/inteiro | Nao informado no material | Sim | FK papel | Papel. |
| extensao_id | UUID/inteiro | Nao informado no material | Sim | FK sdk_extensao | Extensao. |
| module_name | Texto | Nao informado no material | Sim |  | Campo informado. |
| module_alias | Texto | Nao informado no material | Sim |  | Campo informado. |
| module_permission | Enum | none/view/manage/admin/yes/no | Sim |  | Dominio informado. |
| chave_normalizada | Texto | Nao informado no material | Sim |  | Chave sanitizada. |
| atualizado_em | Data/hora | ISO 8601 | Sim |  | Atualizacao. |

### 11.6 `sdk_menu_extensao`

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| id | UUID/inteiro | Nao informado no material | Sim | PK | Identificador. |
| extensao_id | UUID/inteiro | Nao informado no material | Sim | FK sdk_extensao | Extensao. |
| placement | Enum | main/settings/tabs/profile/topnav | Sim |  | Placements informados. |
| parent | Texto | Nao informado no material | Sim |  | Campo obrigatorio informado. |
| title | Texto | Nao informado no material | Sim |  | Campo obrigatorio informado. |
| tipo | Enum | single/dropdown/dropdown-child | Sim |  | Tipos informados. |
| user_type | Enum/texto | admin/team/client/all | Nao |  | Visibilidade informada. |
| user_module_role | Texto | Nao informado no material | Nao |  | Visibilidade por permissao. |
| status | Enum | Ativo/Inativo | Sim |  | Controla renderizacao. |

### 11.7 `sdk_evento_extensao`

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| id | UUID/inteiro | Nao informado no material | Sim | PK | Identificador. |
| extensao_id | UUID/inteiro | Nao informado no material | Sim | FK sdk_extensao | Extensao. |
| evento | Texto | Nao informado no material | Sim |  | Nome do evento. |
| module | Texto | Nao informado no material | Sim |  | Campo informado. |
| type | Enum/texto | company/super_admin/outros homologados | Sim |  | Tipo informado. |
| extractor | Texto | Nao informado no material | Sim |  | Extractor do payload. |
| callback_url | URL | Nao informado no material | Nao |  | Callback externo. |
| hmac_obrigatorio | Booleano | true/false | Sim |  | Assinatura obrigatoria. |
| status | Enum | Ativo/Inativo | Sim |  | Controla entrega. |

### 11.8 `sdk_evento_entrega`

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| id | UUID/inteiro | Nao informado no material | Sim | PK | Identificador. |
| evento_extensao_id | UUID/inteiro | Nao informado no material | Sim | FK sdk_evento_extensao | Evento. |
| tenant_id | UUID/inteiro | Nao informado no material | Sim | FK tenant | Segregacao. |
| payload_json | JSON | Nao informado no material | Sim |  | Payload padronizado. |
| assinatura | Texto | Nao informado no material | Condicional |  | HMAC. |
| nonce | Texto | Nao informado no material | Condicional |  | Protecao contra replay.[^nota1] |
| status | Enum | Pendente/Enviado/Falha/Bloqueado | Sim |  | Resultado. |
| erro | Texto | Nao informado no material | Nao |  | Motivo. |
| criado_em | Data/hora | ISO 8601 | Sim |  | Criacao. |

### 11.9 `sdk_metadado_customizado`

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| id | UUID/inteiro | Nao informado no material | Sim | PK | Identificador. |
| tenant_id | UUID/inteiro | Nao informado no material | Sim | FK tenant | Segregacao. |
| extensao_id | UUID/inteiro | Nao informado no material | Condicional | FK sdk_extensao | Extensao responsavel. |
| tipo | Enum | Label/Layout/Campo/Dropdown/Tab | Sim |  | Tipos preservados. |
| entidade | Texto | Nao informado no material | Sim |  | Entidade afetada. |
| chave | Texto | Nao informado no material | Sim |  | Campo/label/dropdown. |
| valor_json | JSON | Nao informado no material | Sim |  | Valor do metadado. |
| versao | Texto | Nao informado no material | Nao |  | Versao do metadado. |
| status | Enum | Rascunho/Aprovado/Publicado/Revogado | Sim |  | Pipeline controlado. |

### 11.10 `sdk_utilitario`

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| id | UUID/inteiro | Nao informado no material | Sim | PK | Identificador. |
| codigo | Texto | Nao informado no material | Sim | Unico | Codigo do utilitario. |
| categoria | Enum | Parametro/Validacao/Calculo/Mensagem/XML/Impressao/Criptografia | Sim |  | Capacidades preservadas. |
| descricao | Texto | Nao informado no material | Sim |  | Descricao. |
| modulo_dono | Texto | Nao informado no material | Nao |  | Modulo responsavel quando aplicavel. |
| status | Enum | Ativo/Inativo | Sim |  | Controla uso. |

### 11.11 `sdk_historico`

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| id | UUID/inteiro | Nao informado no material | Sim | PK | Identificador. |
| tenant_id | UUID/inteiro | Nao informado no material | Condicional | FK tenant | Obrigatorio em contexto tenant. |
| entidade | Texto | Nao informado no material | Sim |  | Entidade afetada. |
| entidade_id | UUID/inteiro/texto | Nao informado no material | Sim |  | Registro afetado. |
| acao | Texto | Nao informado no material | Sim |  | Campo Acao informado. |
| usuario_id | UUID/inteiro | Nao informado no material | Sim | FK usuario | Campo UsuarioId informado. |
| payload_json | JSON | Nao informado no material | Sim |  | Campo PayloadJson informado; mascarar sensiveis. |
| ip | IP | IPv4/IPv6 | Nao |  | Auditoria de transicao. |
| criado_em | Data/hora | ISO 8601 | Sim |  | Data do historico. |

### 11.12 `sdk_anexo`

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| id | UUID/inteiro | Nao informado no material | Sim | PK | Identificador. |
| entidade | Texto | Nao informado no material | Sim |  | Extensao, versao, manifesto ou metadado. |
| entidade_id | UUID/inteiro/texto | Nao informado no material | Sim |  | Registro relacionado. |
| arquivo_id | UUID/inteiro | Nao informado no material | Sim | FK GED | Campo ArquivoId informado. |
| criado_por | UUID/inteiro | Nao informado no material | Sim | FK usuario | Usuario. |
| criado_em | Data/hora | ISO 8601 | Sim |  | Data de inclusao. |

## 12. Fluxos e estados

### 12.1 Ciclo de vida principal

| Estado atual | Evento | Proximo estado | Permissao | Regra |
|---|---|---|---|---|
| Rascunho | Submeter | EmAnalise | Operador | Validar obrigatorios e manifesto. |
| EmAnalise | Aprovar | Ativo | Aprovador | Extensao fica apta a publicacao. |
| EmAnalise | Rejeitar | Rascunho | Aprovador | Exigir motivo. |
| Ativo | Inativar | Inativo | Gestor | Bloqueia uso. |
| Ativo | Encerrar | Encerrado | Gestor | Finaliza ciclo. |
| Inativo | Reativar | Ativo | Gestor | Reabilita uso. |

### 12.2 Fluxos operacionais

| Fluxo | Passos principais | Resultado esperado |
|---|---|---|
| Registro de extensao | Cadastrar, validar manifesto, homologar, publicar. | Extensao no registry. |
| Instalacao tenant | Selecionar versao, validar dependencias, executar etapas, sincronizar permissoes. | Extensao instalada. |
| Habilitar/desabilitar | Alterar module_status, atualizar lista ativa, registrar historico. | Extensao enabled/disabled. |
| Renderizar menu | Validar extensao ativa, placement, user_type e permissao. | Menu visivel apenas quando permitido. |
| Publicar evento | Capturar evento, executar extractor, montar payload, assinar e entregar. | Evento entregue ou falha registrada. |
| Alterar metadado | Submeter alteracao, aprovar, aplicar, reconstruir cache. | Customizacao rastreavel. |

## 13. APIs e contratos funcionais

| Contrato | Direcao | Entrada | Saida | Observacoes |
|---|---|---|---|---|
| Consultar registry | Cliente para Epros | Filtros de extensao, versao, status | Lista de extensoes | Endpoint final nao informado. |
| Publicar extensao | Siser para Epros | Pacote, manifesto, assinatura | Extensao registrada | Exige validacao. |
| Instalar extensao | Tenant para Epros | Extensao, versao, parametros | Instalacao criada | Exige autorizacao. |
| Habilitar/desabilitar | Tenant para Epros | Extensao e status | Status alterado | Enabled/disabled. |
| Sincronizar permissoes | Epros interno | Papel, extensoes ativas | Permissoes atualizadas | Preserva permissao existente. |
| Renderizar menus | Epros interno | Usuario, papel, extensoes ativas | Menus autorizados | Respeita placement e permissao. |
| Publicar evento | Epros interno/externo | Evento e payload base | Entregas | Usa extractor. |
| Validar callback | Externo para Epros | Payload, assinatura, timestamp/nonce | Aceito/rejeitado | HMAC e replay protection. |
| Aplicar metadado | Siser/tenant para Epros | Tipo, entidade, valor, versao | Metadado aplicado | Pipeline controlado. |

## 14. Telas, consultas e relatorios

| Interface | Objetivo | Campos/acoes minimas | Observacoes |
|---|---|---|---|
| Lista de extensoes | Consultar registry e instalacoes. | Nome, alias, versao, status, autor, tenant, filtros, exportar. | Material informa lista. |
| Detalhe da extensao | Ver manifesto, versoes, dependencias, historico e anexos. | Dados, historico, anexos, aprovacao. | Material informa abas. |
| Instalacao por tenant | Instalar, habilitar e desabilitar. | Tenant, extensao, versao, status, etapas, erros. | Baseado nos fluxos de instalacao. |
| Permissoes por extensao | Configurar permissao por papel. | Papel, permissao, extensao, sincronizar. | Valores informados. |
| Menus de extensao | Validar declaracoes de menu. | Placement, parent, title, tipo, visibilidade. | Placements informados. |
| Eventos e callbacks | Monitorar eventos e entregas. | Evento, modulo, extractor, status, assinatura, erro. | HMAC/replay. |
| Metadados | Gerir labels, layouts, campos e dropdowns. | Tipo, entidade, valor, versao, aprovar, publicar. | Pipeline controlado. |
| Painel gestor | KPIs e fila de aprovacao. | Pendencias, falhas, extensoes ativas, eventos. | Material informa painel gestor. |

| Relatorio | Descricao | Filtros | Observacoes |
|---|---|---|---|
| Posicao geral | Snapshot por status. | Tenant, status, versao, periodo. | Material informa REL-SDK-001. |
| Auditoria de alteracoes | Trilha por periodo. | Usuario, acao, extensao, periodo. | Material informa REL-SDK-002. |
| Instalacoes | Extensoes instaladas por tenant. | Tenant, extensao, status, versao. | Criado para operacao.[^nota1] |
| Eventos | Entregas de eventos por extensao. | Evento, status, periodo. | Necessario para suporte. |
| Metadados | Alteracoes de labels/layout/campos. | Tipo, entidade, usuario, periodo. | Necessario para auditoria. |

## 15. Seguranca, privacidade e auditoria

| Tema | Regra funcional |
|---|---|
| Tenant | Instalacao e uso de extensao devem respeitar tenant. |
| Assinatura | Pacotes e callbacks devem ter assinatura validavel quando aplicavel. |
| Replay | Callbacks externos devem possuir protecao contra replay. |
| Permissao | Menus, eventos e acoes respeitam permissao por papel. |
| Metadados | Alteracoes de metadados exigem pipeline e auditoria. |
| Segredos | Segredos de extensao nao devem ser expostos em logs. |
| Payload | Payload de evento deve ser mascarado quando sensivel. |
| Anexos | Arquivos devem referenciar GED. |

## 16. Testes funcionais minimos

| Cenario | Dado/condicao | Resultado esperado |
|---|---|---|
| Criar extensao valida | Codigo, status e responsavel informados. | Status Rascunho. |
| Criar sem obrigatorios | Falta codigo/status/responsavel. | Erro de validacao. |
| Aprovar extensao | Registro EmAnalise e aprovador autorizado. | Status Ativo. |
| Instalar sem dependencia | Dependencia ausente. | Instalacao bloqueada. |
| Instalar com manifesto invalido | Manifesto incompleto. | Instalacao bloqueada. |
| Habilitar extensao | Extensao instalada. | module_status enabled. |
| Desabilitar extensao | Extensao ativa. | module_status disabled e menus ocultos. |
| Sincronizar permissao | Papel com permissao existente. | Valor preservado. |
| Menu sem permissao | Usuario sem module_permission. | Menu nao aparece. |
| Evento sem extractor | Evento declarado incompleto. | Publicacao bloqueada. |
| Callback sem HMAC | Callback externo recebido. | Requisicao rejeitada. |
| Replay de callback | Mesmo nonce/timestamp repetido. | Requisicao rejeitada. |
| Alterar metadado sem pipeline | Usuario tenta alterar direto. | Operacao bloqueada. |
| Validar CPF/CNPJ | Valor enviado ao utilitario. | Retorna valido/invalido. |
| Mascaramento LGPD | Payload sensivel em historico. | Campo oculto em consulta. |

## 17. Criterios de aceite

| Codigo | Criterio |
|---|---|
| CA-SDK-001 | Extensao deve possuir codigo, status, responsavel, manifesto e versao. |
| CA-SDK-002 | Workflow Rascunho, EmAnalise, Ativo, Inativo e Encerrado deve funcionar com auditoria. |
| CA-SDK-003 | Instalacao por tenant deve registrar extensao, versao, status e etapas executadas. |
| CA-SDK-004 | Status de tenant deve aceitar enabled ou disabled. |
| CA-SDK-005 | Permissao por papel deve usar module_name, module_alias e module_permission. |
| CA-SDK-006 | Menus devem respeitar placement, parent, title, tipo, user_type e permissao. |
| CA-SDK-007 | Eventos devem declarar module, type e extractor. |
| CA-SDK-008 | Callbacks externos devem exigir HMAC e protecao contra replay. |
| CA-SDK-009 | Metadados dinamicos devem ter pipeline e trilha de auditoria. |
| CA-SDK-010 | Utilitarios homologados devem estar catalogados e nao substituir regra final do modulo dono. |

## 18. Notas de autoria e saneamento funcional

[^nota1]: O modelo funcional de registry, versao, manifesto, instalacao, permissoes, menus, eventos, metadados, utilitarios e entregas foi criado nesta EF para tornar o Epros implantavel. O material comprova campos e regras parciais, mas nao informa o contrato final completo de manifesto, assinatura de pacote, sandbox, APIs, cache e pipeline.
