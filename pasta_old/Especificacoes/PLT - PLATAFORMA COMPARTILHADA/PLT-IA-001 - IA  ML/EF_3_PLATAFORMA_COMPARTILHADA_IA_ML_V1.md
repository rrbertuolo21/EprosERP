# EF_3_PLATAFORMA_COMPARTILHADA_IA_ML_V1

**Projeto:** Epros  
**Empresa:** Siser  
**Tipo de documento:** Especificacao Funcional definitiva  
**Versao:** V1  
**Modulo:** PLATAFORMA_COMPARTILHADA  
**Submodulo:** IA_ML  
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

O submodulo IA_ML existe para governar, parametrizar, executar e auditar capacidades de inteligencia artificial e aprendizado de maquina dentro do Epros. Ele centraliza modelos preditivos, inferencias, assistentes, prompts, bases de conhecimento, quotas, eventos, anexos, historico e controles de conformidade.

| Pergunta | Resposta |
|---|---|
| Para que o submodulo existe? | Para oferecer uma camada governada de IA/ML reutilizavel por outros submodulos do Epros. |
| Que problema de negocio resolve? | Evita que cada modulo implemente inferencia, assistente, score, sugestao ou automacao inteligente de forma isolada e sem auditoria. |
| Qual resultado operacional deve produzir? | Inferencias registradas, modelos versionados, prompts controlados, eventos publicados, limites por tenant e trilha de auditoria. |
| Quais areas dependem dele? | Cadastros Base, Estoque, Plataforma Fiscal, Interface Assistida, Analytics, GED, Compliance e modulos consumidores futuros. |

## 3. Escopo funcional

### 3.1 Dentro do escopo

| Capacidade | Descricao | Observacao |
|---|---|---|
| Cadastro de recursos de IA/ML | Manter registros funcionais do dominio IA/ML com codigo, status, responsavel e versionamento. | Material informa entidade principal e versionamento. |
| Workflow de aprovacao | Controlar ciclo Rascunho, EmAnalise, Ativo, Inativo e Encerrado. | Fluxo informado no material. |
| Consulta e exportacao | Permitir listagem com filtros de status, periodo e responsavel, alem de exportacao. | Telas e requisitos informados. |
| Historico e anexos | Registrar alteracoes, payload de auditoria e arquivos vinculados via GED. | Entidades IamlHistorico e IamlAnexo informadas. |
| Eventos de dominio | Publicar eventos apos confirmacao transacional. | Material informa eventos de dominio. |
| Parametrizacao por tenant | Permitir regras e limites por tenant sem mudanca de codigo. | Material informa parametrizacao por tenant. |
| Modelos preditivos | Controlar modelos e suas versoes para inferencia. | Material cita modelos preditivos e governanca. |
| Inferencia | Receber entrada, executar ou encaminhar inferencia e registrar resultado. | Material cita inferencia e API isolada. |
| Assistentes e prompts | Controlar prompts e assistentes com auditoria. | Material cita assistentes LLM. |
| RAG | Integrar bases de conhecimento com GED quando formalizadas. | Material cita RAG e GED. |
| Quotas | Controlar limites de uso por tenant. | Material cita quotas. |
| LGPD e opt-out | Respeitar base legal, mascaramento, retencao, anonimizacao e opt-out quando aplicavel. | Material cita conformidade LGPD e opt-out. |

### 3.2 Fora do escopo

| Item fora do escopo | Motivo | Destino correto |
|---|---|---|
| Substituir cadastros mestres | IA/ML deve referenciar IDs, nao duplicar cadastros. | Cadastros Base |
| Executar processo de negocio dono | IA/ML sugere, classifica, alerta ou automatiza, mas o efeito final pertence ao modulo consumidor. | Modulo consumidor |
| Completar modelos oficiais sem validacao | O material nao informa treinamento, algoritmos, metricas finais ou provedores. | MC |
| Definir fornecedor externo de IA | Nao informado no material. | MC |
| Processar dados sem base legal | Conformidade depende de finalidade e base legal. | Compliance |

## 4. Glossario e conceitos funcionais

| Termo | Definicao funcional | Observacoes |
|---|---|---|
| Modelo | Recurso funcional que realiza predicao, classificacao, sugestao, alerta ou apoio automatizado. | Pode ter versoes. |
| Versao do modelo | Estado publicado ou controlado de um modelo, com status e metadados. | Necessaria para auditoria. |
| Inferencia | Execucao de um modelo, prompt ou assistente sobre uma entrada. | Deve gerar registro auditavel. |
| Prompt | Instrucao controlada usada por assistente LLM. | Deve possuir versionamento. |
| RAG | Uso de base de conhecimento para apoiar respostas ou inferencias. | Pode usar GED. |
| Feature Store | Conjunto governado de atributos usados por modelos. | Estrutura funcional criada nesta EF.[^nota1] |
| Model Registry | Registro funcional de modelos, versoes e status. | Estrutura funcional criada nesta EF.[^nota1] |
| Drift | Alteracao de comportamento ou desempenho do modelo ao longo do tempo. | Material cita governanca de drift. |
| Opt-out | Direito/configuracao para nao participar de inferencia automatizada quando aplicavel. | Relacionado a LGPD. |

## 5. Atores, papeis e responsabilidades

| Ator/Papel | Responsabilidade | Permissoes esperadas | Restricoes |
|---|---|---|---|
| Operador | Cadastrar registros em rascunho, consultar listagens e anexar documentos permitidos. | Criar, consultar, editar rascunho, exportar conforme permissao. | Nao aprova nem ativa modelos. |
| Aprovador | Avaliar registros em analise e aprovar ou rejeitar. | Aprovar, rejeitar, informar motivo. | Deve respeitar segregacao definida pela Siser. |
| Gestor | Ativar, inativar, encerrar, configurar limites e acompanhar painel. | Gerir ciclo de vida e parametrizacao. | Nao pode burlar auditoria. |
| Responsavel tecnico | Validar requisitos tecnicos, versoes e integracoes de inferencia. | Manter metadados tecnicos aprovados. | Nao substitui validacao funcional. |
| Usuario consumidor | Acionar inferencias e consultar resultados permitidos. | Executar inferencias autorizadas. | Acesso limitado ao contexto do tenant e da permissao. |
| Epros | Validar, persistir, auditar, publicar eventos e controlar quotas. | Automacao sistemica. | Nao cria inferencia sem contexto permitido. |

## 6. Visao operacional do submodulo

O usuario cria um registro de IA/ML em rascunho, informa codigo, responsavel, finalidade e configuracoes disponiveis. O Epros valida obrigatorios, tenant, status e referencias externas. O registro pode receber historico e anexos. Quando completo, o usuario submete para analise. O aprovador aprova ou rejeita com motivo. Registros aprovados podem ser ativados, usados por modulos consumidores e acompanhados por painel operacional.

Quando uma inferencia e solicitada, o Epros valida tenant, permissao, quota, status do recurso, base legal quando aplicavel e dados de entrada. A execucao registra entrada, resultado, status, mensagens, explicabilidade quando disponivel, usuario, timestamp e contexto. Eventos de dominio sao publicados somente apos confirmacao transacional.

Situacoes de excecao incluem falta de obrigatorios, referencia inexistente, quota excedida, recurso inativo, dado bloqueado por LGPD, ausencia de base de conhecimento, falha de integracao e tentativa de publicar evento a partir de rascunho.

## 7. Capacidades funcionais

### 7.1 Cadastro e manutencao de recurso IA/ML

| Item | Especificacao |
|---|---|
| Objetivo | Registrar recursos de IA/ML com codigo, tenant, status e responsavel. |
| Acionamento | Manual. |
| Pre-condicoes | Usuario autenticado e tenant definido. |
| Dados de entrada | Codigo, responsavel, finalidade, status inicial, configuracoes e anexos quando houver. |
| Processamento | O Epros valida obrigatorios, cria registro em Rascunho e registra historico. |
| Resultado esperado | Recurso IA/ML cadastrado. |
| Pos-condicoes | Registro disponivel para consulta, edicao e submissao. |
| Excecoes | Codigo ausente, responsavel ausente, tenant ausente ou referencia invalida. |
| Auditoria | Usuario, data, IP, antes/depois e payload funcional. |

### 7.2 Workflow de aprovacao

| Item | Especificacao |
|---|---|
| Objetivo | Controlar avaliacao e publicacao funcional de recursos IA/ML. |
| Acionamento | Manual. |
| Pre-condicoes | Registro em estado valido para transicao. |
| Dados de entrada | Registro, evento de transicao, aprovador, motivo quando houver. |
| Processamento | O Epros aplica a maquina de estados e registra historico. |
| Resultado esperado | Estado atualizado. |
| Pos-condicoes | Eventos de dominio podem ser publicados apos commit quando aplicavel. |
| Excecoes | Transicao invalida, usuario sem permissao ou motivo ausente em rejeicao. |
| Auditoria | Estado anterior, novo estado, usuario, timestamp, IP e motivo. |

### 7.3 Inferencia governada

| Item | Especificacao |
|---|---|
| Objetivo | Executar ou encaminhar inferencias de forma controlada e auditavel. |
| Acionamento | Manual, por integracao, por evento ou por modulo consumidor. |
| Pre-condicoes | Recurso ativo, tenant autorizado, quota disponivel e dados permitidos. |
| Dados de entrada | Recurso, versao, payload de entrada, contexto, modulo consumidor e usuario. |
| Processamento | O Epros valida contexto, registra solicitacao, executa inferencia, registra resultado e publica evento quando aplicavel. |
| Resultado esperado | Resultado de inferencia com status e mensagens. |
| Pos-condicoes | Historico e indicadores atualizados. |
| Excecoes | Recurso inativo, quota excedida, dado bloqueado, falha de integracao ou resultado indisponivel. |
| Auditoria | Entrada, resultado, status, usuario, tenant, modulo consumidor e horario. |

### 7.4 Assistentes LLM e prompts

| Item | Especificacao |
|---|---|
| Objetivo | Controlar assistentes e prompts usados para orientar interacoes, wizards e apoio operacional. |
| Acionamento | Manual ou por modulo consumidor. |
| Pre-condicoes | Prompt aprovado, contexto permitido e politica LGPD aplicada. |
| Dados de entrada | Prompt, versao, contexto, parametros e usuario. |
| Processamento | O Epros aplica prompt versionado, valida contexto e registra resposta. |
| Resultado esperado | Resposta, recomendacao ou passo assistido. |
| Pos-condicoes | Historico e consumo de quota registrados. |
| Excecoes | Prompt inativo, contexto insuficiente, quota excedida ou base legal ausente. |
| Auditoria | Prompt, versao, entrada, saida, usuario, horario e tenant. |

### 7.5 RAG e base de conhecimento

| Item | Especificacao |
|---|---|
| Objetivo | Usar base de conhecimento vinculada ao GED para apoiar respostas e inferencias. |
| Acionamento | Por inferencia ou assistente. |
| Pre-condicoes | Base aprovada, documentos permitidos e acesso autorizado. |
| Dados de entrada | Base, documentos, consulta, usuario e contexto. |
| Processamento | O Epros localiza conteudo permitido, compoe contexto e registra uso. |
| Resultado esperado | Resposta ou inferencia apoiada por base de conhecimento. |
| Pos-condicoes | Evidencias e referencias funcionais registradas quando disponiveis. |
| Excecoes | Documento indisponivel, acesso negado ou base inativa. |
| Auditoria | Base usada, documentos referenciados, usuario, tenant e horario. |

### 7.6 Quotas e parametrizacao por tenant

| Item | Especificacao |
|---|---|
| Objetivo | Controlar limites e regras de uso por tenant sem deploy de codigo. |
| Acionamento | Manual por gestor ou automatico no uso. |
| Pre-condicoes | Tenant existente e usuario autorizado. |
| Dados de entrada | Limites, periodo, recurso, politica de bloqueio e alertas. |
| Processamento | O Epros compara uso acumulado com limite e aplica bloqueio ou alerta. |
| Resultado esperado | Uso autorizado, bloqueado ou alertado. |
| Pos-condicoes | Consumo e historico atualizados. |
| Excecoes | Limite ausente, configuracao inconsistente ou tenant bloqueado. |
| Auditoria | Alteracoes de limite e consumo por recurso. |

## 8. Regras de negocio

| Regra | Descricao | Condicao | Resultado | Severidade | Observacoes |
|---|---|---|---|---|---|
| IA-001 | Todo registro de IA/ML deve possuir tenant. | Criacao, edicao, inferencia ou parametrizacao. | Operacao sem tenant e bloqueada. | Bloqueante | Material informa tenant obrigatorio. |
| IA-002 | Todo registro principal deve possuir codigo, status e responsavel. | Criacao e manutencao. | Ausencia gera bloqueio. | Bloqueante | Campos informados no material. |
| IA-003 | Registro novo nasce em Rascunho. | Criacao valida. | Status inicial Rascunho. | Bloqueante | Fluxo informado. |
| IA-004 | Rascunho pode ser submetido para EmAnalise por operador. | Submissao. | Estado passa para EmAnalise. | Bloqueante | Fluxo informado. |
| IA-005 | EmAnalise pode ser aprovado e tornar-se Ativo. | Aprovacao por aprovador. | Estado passa para Ativo. | Bloqueante | Fluxo informado. |
| IA-006 | EmAnalise pode ser rejeitado com retorno a Rascunho. | Rejeicao por aprovador. | Estado passa para Rascunho com motivo. | Bloqueante | Motivo deve ser registrado. |
| IA-007 | Ativo pode ser inativado ou encerrado por gestor. | Gestao do ciclo de vida. | Estado passa para Inativo ou Encerrado. | Bloqueante | Fluxo informado. |
| IA-008 | Inativo pode ser reativado por gestor. | Reativacao. | Estado passa para Ativo. | Bloqueante | Fluxo informado. |
| IA-009 | Eventos de dominio sao publicados somente apos confirmacao transacional. | Operacao que gere evento. | Evento publicado apos confirmacao. | Bloqueante | Material informa evento apos commit. |
| IA-010 | Historico deve registrar usuario, timestamp e IP nas transicoes. | Qualquer transicao. | Historico gravado. | Bloqueante | Material informa historico. |
| IA-011 | Alteracoes devem registrar payload funcional antes/depois quando aplicavel. | Edicao de registro. | Auditoria armazenada. | Bloqueante | Material informa auditoria. |
| IA-012 | Anexos devem referenciar arquivo do GED. | Inclusao de anexo. | Anexo sem arquivo e bloqueado. | Bloqueante | Entidade IamlAnexo informada. |
| IA-013 | Regras podem ser parametrizadas por tenant sem deploy de codigo. | Parametrizacao. | Configuracao fica segregada por tenant. | Bloqueante | Material informa parametrizacao por tenant. |
| IA-014 | Inferencia nao deve duplicar cadastro mestre. | Inferencia com referencia externa. | Usa referencia por ID. | Bloqueante | Escopo informa nao duplicar cadastros. |
| IA-015 | Dados pessoais devem respeitar base legal, retencao e anonimizacao. | Uso de dado pessoal. | Uso bloqueado ou mascarado quando nao permitido. | Bloqueante | Material informa LGPD. |
| IA-016 | Usuario com opt-out aplicavel nao deve participar de inferencia automatizada. | Politica de opt-out ativa. | Inferencia bloqueada ou ajustada. | Bloqueante | Material cita opt-out. |
| IA-017 | Quota por tenant deve ser validada antes de inferencia. | Execucao de inferencia. | Uso permitido, alertado ou bloqueado. | Bloqueante | README cita quotas. |
| IA-018 | Recurso inativo ou encerrado nao pode executar inferencia. | Execucao solicitada. | Execucao bloqueada. | Bloqueante | Derivado do ciclo de vida.[^nota1] |
| IA-019 | Resultado de inferencia deve ser auditavel. | Inferencia concluida ou falha. | Resultado, status e mensagens registrados. | Bloqueante | Necessario para governanca.[^nota1] |
| IA-020 | Caso de uso sem contrato de integracao documentado deve ficar pendente. | Novo modulo consumidor. | Item vai para MC. | Alerta | Evita inventar integracoes. |

## 9. Parametros de configuracao

| Parametro | Finalidade | Tipo/formato | Valor padrao | Obrigatorio | Nivel | Quem pode alterar | Impacto |
|---|---|---|---|---|---|---|---|
| IA_ML_Habilitado | Ativar o submodulo para o tenant. | Booleano | Nao informado no material | Sim | Tenant | Gestor | Habilita ou bloqueia uso. |
| QuotaInferenciaPeriodo | Limitar inferencias por periodo. | Numero | Nao informado no material | Condicional | Tenant | Gestor | Bloqueia ou alerta consumo. |
| PoliticaOptOut | Controlar uso de dados com opt-out. | Enum/texto | Nao informado no material | Condicional | Tenant | Gestor/Compliance | Impacta uso de dados pessoais. |
| WorkflowObrigatorio | Exigir aprovacao antes de ativar. | Booleano | Nao informado no material | Sim | Tenant | Gestor | Controla governanca. |
| RetencaoHistorico | Definir prazo de historico. | Periodo | Nao informado no material | Condicional | Tenant | Gestor/Compliance | Impacta auditoria e LGPD. |
| ProvedorInferencia | Definir provedor/servico de inferencia. | Texto | Nao informado no material | Condicional | Tenant/Global | Responsavel tecnico | Nao informado no material. |

## 10. Modelo de dados funcional e implantavel

### 10.1 Visao geral do modelo

O modelo do submodulo combina dados mestres de governanca, movimentos de inferencia, controle de workflow, anexos, historico, quotas e integracoes. O material informa as estruturas Iaml, IamlHistorico e IamlAnexo com campos minimos; as demais entidades funcionais foram criadas para tornar implantaveis as capacidades de modelos, inferencia, prompts, RAG, quotas e auditoria citadas no material.[^nota1]

| Grupo de dados | Entidades/tabelas | Papel funcional | Observacoes |
|---|---|---|---|
| Cadastros mestres | `ia_ml_recurso`, `ia_ml_modelo`, `ia_ml_prompt`, `ia_ml_base_conhecimento` | Controlam recursos governados de IA/ML. | Parte criada para completar capacidade comprovada.[^nota1] |
| Versionamento | `ia_ml_modelo_versao`, `ia_ml_prompt_versao` | Preserva versoes aprovadas e historico de uso. | Material cita versionamento. |
| Movimentos/transacoes | `ia_ml_inferencia`, `ia_ml_inferencia_resultado` | Registra solicitacoes e resultados. | Material cita inferencia e auditoria. |
| Tabelas auxiliares | `ia_ml_config_tenant`, `ia_ml_quota`, `ia_ml_feature` | Parametriza tenant, limites e atributos. | Material cita parametrizacao e feature store. |
| Tabelas de relacionamento | `ia_ml_recurso_anexo`, `ia_ml_base_documento` | Vincula GED e base de conhecimento. | GED citado no material. |
| Auditoria | `ia_ml_historico`, `ia_ml_evento` | Registra transicoes, payload e eventos. | IamlHistorico informado no material. |

### 10.2 Entidades, finalidade e cardinalidade

| Entidade | Finalidade | Cardinalidade | Observacao |
|---|---|---|---|
| `ia_ml_recurso` | Representar o registro principal do dominio IA/ML. | 1 por recurso | Equivale ao agregado principal informado. |
| `ia_ml_modelo` | Representar modelo preditivo ou classificador. | 0..N por tenant | Criado para governar modelos citados.[^nota1] |
| `ia_ml_modelo_versao` | Controlar versoes do modelo. | 1..N por modelo | Versionamento citado no material. |
| `ia_ml_prompt` | Controlar prompt/assistente. | 0..N por tenant | Criado para assistentes LLM citados.[^nota1] |
| `ia_ml_prompt_versao` | Controlar versoes de prompt. | 1..N por prompt | Necessario para auditoria.[^nota1] |
| `ia_ml_base_conhecimento` | Controlar base RAG vinculada ao GED. | 0..N por tenant | RAG e GED citados. |
| `ia_ml_base_documento` | Vincular documentos a base de conhecimento. | 0..N por base | Referencia GED. |
| `ia_ml_inferencia` | Registrar solicitacao de inferencia. | 0..N por recurso/modelo/prompt | Criado para inferencia governada.[^nota1] |
| `ia_ml_inferencia_resultado` | Registrar saida, score, alerta ou resposta. | 0..N por inferencia | Resultado auditavel. |
| `ia_ml_config_tenant` | Guardar parametrizacao por tenant. | 0..N por tenant | Material cita parametrizacao sem deploy. |
| `ia_ml_quota` | Controlar limite e consumo. | 0..N por tenant/recurso | README cita quotas. |
| `ia_ml_feature` | Registrar atributos disponiveis para modelos. | 0..N por tenant/modelo | Feature store citado na arquitetura. |
| `ia_ml_historico` | Auditar alteracoes e transicoes. | 0..N por recurso | Campos Acao, UsuarioId e PayloadJson informados. |
| `ia_ml_recurso_anexo` | Vincular anexos GED ao recurso. | 0..N por recurso | ArquivoId informado. |
| `ia_ml_evento` | Registrar evento de dominio publicado. | 0..N por operacao | Material cita eventos. |

### 10.3 Relacionamentos e integridade

| Origem | Relacao | Destino | Regra |
|---|---|---|---|
| Tenant | possui | `ia_ml_recurso` | Todo recurso pertence a um tenant. |
| `ia_ml_recurso` | possui | `ia_ml_historico` | Toda transicao deve gerar historico. |
| `ia_ml_recurso` | possui | `ia_ml_recurso_anexo` | Anexos referenciam GED. |
| `ia_ml_modelo` | possui | `ia_ml_modelo_versao` | Modelo deve ter versoes controladas para uso. |
| `ia_ml_prompt` | possui | `ia_ml_prompt_versao` | Prompt deve ter versoes controladas para uso. |
| `ia_ml_base_conhecimento` | possui | `ia_ml_base_documento` | Base usa documentos autorizados do GED. |
| `ia_ml_recurso` | gera | `ia_ml_inferencia` | Inferencias devem referenciar recurso ativo. |
| `ia_ml_inferencia` | possui | `ia_ml_inferencia_resultado` | Resultado registra saida e status. |
| Tenant | possui | `ia_ml_config_tenant` | Parametros devem ser segregados por tenant. |
| Tenant/Recurso | possui | `ia_ml_quota` | Quotas controlam uso por periodo. |

### 10.4 Estados funcionais

| Estado | Significado | Entrada permitida | Saida permitida |
|---|---|---|---|
| Rascunho | Registro criado e ainda nao aprovado. | Criacao ou rejeicao. | EmAnalise. |
| EmAnalise | Registro submetido para aprovacao. | Submissao. | Ativo ou Rascunho. |
| Ativo | Registro aprovado e operacional. | Aprovacao ou reativacao. | Inativo ou Encerrado. |
| Inativo | Registro temporariamente indisponivel. | Inativacao. | Ativo. |
| Encerrado | Registro encerrado. | Encerramento. | Nao informado no material. |

## 11. Dicionario de dados implantavel

### 11.1 `ia_ml_recurso`

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| Id | Identificador | Nao informado no material | Sim | Chave primaria funcional | Campo informado como obrigatorio. |
| TenantId | Identificador | Nao informado no material | Sim | Tenant | Obrigatorio em todas as operacoes. |
| Codigo | Texto | Nao informado no material | Sim | Identificador funcional | Campo informado como obrigatorio. |
| Status | Enum/texto | Rascunho, EmAnalise, Ativo, Inativo, Encerrado | Sim | Estado | Campo informado como obrigatorio. |
| ResponsavelId | Identificador | Nao informado no material | Sim | Pessoa/RH quando aplicavel | Campo informado como obrigatorio. |
| Finalidade | Texto | Nao informado no material | Nao informado no material | Governanca | Necessario para LGPD e auditoria.[^nota1] |
| Descricao | Texto | Nao informado no material | Nao informado no material | Descritivo | Nao informado no material. |
| CriadoEm | Data/hora | Nao informado no material | Nao informado no material | Auditoria | Data de criacao. |
| AtualizadoEm | Data/hora | Nao informado no material | Nao informado no material | Auditoria | Data de alteracao. |

### 11.2 `ia_ml_modelo`

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| Id | Identificador | Nao informado no material | Sim | Chave primaria funcional | Identificador do modelo. |
| TenantId | Identificador | Nao informado no material | Sim | Tenant | Segregacao obrigatoria. |
| RecursoId | Identificador | Nao informado no material | Sim | `ia_ml_recurso` | Modelo vinculado ao recurso. |
| CodigoModelo | Texto | Nao informado no material | Sim | Codigo | Criado para governanca.[^nota1] |
| TipoModelo | Enum/texto | Preditivo, Classificacao, Anomalia, Assistente, Nao informado no material | Sim | Tipo | Dominios consolidados dos casos citados.[^nota1] |
| Status | Enum/texto | Rascunho, EmAnalise, Ativo, Inativo, Encerrado | Sim | Estado | Segue ciclo de vida. |
| ResponsavelId | Identificador | Nao informado no material | Sim | Responsavel | Responsavel funcional. |

### 11.3 `ia_ml_modelo_versao`

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| Id | Identificador | Nao informado no material | Sim | Chave primaria funcional | Identificador da versao. |
| ModeloId | Identificador | Nao informado no material | Sim | `ia_ml_modelo` | Modelo versionado. |
| Versao | Texto | Nao informado no material | Sim | Versionamento | Material cita versionamento. |
| StatusVersao | Enum/texto | Rascunho, EmAnalise, Ativo, Inativo, Encerrado | Sim | Estado | Controla uso da versao. |
| DataPublicacao | Data/hora | Nao informado no material | Nao informado no material | Auditoria | Publicacao nao detalhada. |
| MetricasJson | JSON | Nao informado no material | Nao informado no material | Governanca | Metricas finais nao informadas. |
| DriftMonitorado | Booleano | Sim/Nao | Nao informado no material | Governanca | Material cita drift. |

### 11.4 `ia_ml_prompt`

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| Id | Identificador | Nao informado no material | Sim | Chave primaria funcional | Identificador do prompt. |
| TenantId | Identificador | Nao informado no material | Sim | Tenant | Segregacao obrigatoria. |
| RecursoId | Identificador | Nao informado no material | Sim | `ia_ml_recurso` | Prompt vinculado ao recurso. |
| CodigoPrompt | Texto | Nao informado no material | Sim | Codigo | Criado para controle.[^nota1] |
| Finalidade | Texto | Nao informado no material | Sim | Governanca | Necessaria para uso controlado.[^nota1] |
| Status | Enum/texto | Rascunho, EmAnalise, Ativo, Inativo, Encerrado | Sim | Estado | Segue ciclo de vida. |

### 11.5 `ia_ml_prompt_versao`

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| Id | Identificador | Nao informado no material | Sim | Chave primaria funcional | Identificador da versao. |
| PromptId | Identificador | Nao informado no material | Sim | `ia_ml_prompt` | Prompt versionado. |
| Versao | Texto | Nao informado no material | Sim | Versionamento | Necessario para auditoria.[^nota1] |
| ConteudoPrompt | Texto | Nao informado no material | Sim | Prompt | Conteudo final nao informado no material. |
| StatusVersao | Enum/texto | Rascunho, EmAnalise, Ativo, Inativo, Encerrado | Sim | Estado | Controla uso. |

### 11.6 `ia_ml_base_conhecimento`

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| Id | Identificador | Nao informado no material | Sim | Chave primaria funcional | Identificador da base. |
| TenantId | Identificador | Nao informado no material | Sim | Tenant | Segregacao obrigatoria. |
| RecursoId | Identificador | Nao informado no material | Sim | `ia_ml_recurso` | Base vinculada ao recurso. |
| CodigoBase | Texto | Nao informado no material | Sim | Codigo | Criado para RAG.[^nota1] |
| Status | Enum/texto | Rascunho, EmAnalise, Ativo, Inativo, Encerrado | Sim | Estado | Apenas bases ativas podem ser usadas. |

### 11.7 `ia_ml_base_documento`

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| Id | Identificador | Nao informado no material | Sim | Chave primaria funcional | Identificador do vinculo. |
| BaseId | Identificador | Nao informado no material | Sim | `ia_ml_base_conhecimento` | Base de conhecimento. |
| ArquivoId | Identificador | Nao informado no material | Sim | GED | Referencia documento GED. |
| StatusDocumento | Enum/texto | Nao informado no material | Sim | Estado | Dominio final pendente. |

### 11.8 `ia_ml_inferencia`

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| Id | Identificador | Nao informado no material | Sim | Chave primaria funcional | Identificador da inferencia. |
| TenantId | Identificador | Nao informado no material | Sim | Tenant | Segregacao obrigatoria. |
| RecursoId | Identificador | Nao informado no material | Sim | `ia_ml_recurso` | Recurso usado. |
| ModeloVersaoId | Identificador | Nao informado no material | Condicional | `ia_ml_modelo_versao` | Obrigatorio quando usar modelo. |
| PromptVersaoId | Identificador | Nao informado no material | Condicional | `ia_ml_prompt_versao` | Obrigatorio quando usar prompt. |
| ModuloConsumidor | Texto | Nao informado no material | Sim | Integracao | Modulo que solicitou. |
| EntradaJson | JSON | Nao informado no material | Sim | Payload | Entrada auditavel. |
| StatusInferencia | Enum/texto | Solicitada, Processada, Falha, Bloqueada, Nao informado no material | Sim | Status | Dominio consolidado para controle.[^nota1] |
| UsuarioId | Identificador | Nao informado no material | Nao informado no material | Usuario | Usuario solicitante quando houver. |
| CriadoEm | Data/hora | Nao informado no material | Nao informado no material | Auditoria | Data da solicitacao. |

### 11.9 `ia_ml_inferencia_resultado`

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| Id | Identificador | Nao informado no material | Sim | Chave primaria funcional | Identificador do resultado. |
| InferenciaId | Identificador | Nao informado no material | Sim | `ia_ml_inferencia` | Inferencia relacionada. |
| SaidaJson | JSON | Nao informado no material | Nao informado no material | Resultado | Saida funcional. |
| Score | Decimal | Nao informado no material | Nao informado no material | Resultado | Usado quando inferencia retornar score. |
| ExplicabilidadeJson | JSON | Nao informado no material | Nao informado no material | Governanca | Material cita explainability. |
| Mensagem | Texto | Nao informado no material | Nao | Mensagem | Erro, alerta ou observacao. |
| CriadoEm | Data/hora | Nao informado no material | Nao informado no material | Auditoria | Data do resultado. |

### 11.10 `ia_ml_config_tenant`

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| Id | Identificador | Nao informado no material | Sim | Chave primaria funcional | Identificador da configuracao. |
| TenantId | Identificador | Nao informado no material | Sim | Tenant | Parametrizacao por tenant. |
| Parametro | Texto | Nao informado no material | Sim | Chave de parametro | Parametro funcional. |
| Valor | Texto/JSON | Nao informado no material | Sim | Valor | Valor configurado. |
| Ativo | Booleano | Sim/Nao | Sim | Status | Permite desligar parametro. |

### 11.11 `ia_ml_quota`

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| Id | Identificador | Nao informado no material | Sim | Chave primaria funcional | Identificador da quota. |
| TenantId | Identificador | Nao informado no material | Sim | Tenant | Tenant controlado. |
| RecursoId | Identificador | Nao informado no material | Condicional | `ia_ml_recurso` | Quota por recurso quando aplicavel. |
| Periodo | Texto/periodo | Nao informado no material | Sim | Periodo | Periodo de controle. |
| Limite | Numero | Nao informado no material | Sim | Limite | Limite de uso. |
| ConsumoAtual | Numero | Nao informado no material | Sim | Consumo | Consumo acumulado. |
| PoliticaExcedente | Enum/texto | Bloquear, Alertar, Nao informado no material | Sim | Politica | Dominio final pendente. |

### 11.12 `ia_ml_feature`

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| Id | Identificador | Nao informado no material | Sim | Chave primaria funcional | Identificador do atributo. |
| TenantId | Identificador | Nao informado no material | Sim | Tenant | Segregacao obrigatoria. |
| CodigoFeature | Texto | Nao informado no material | Sim | Codigo | Criado para feature store.[^nota1] |
| OrigemDado | Texto | Nao informado no material | Sim | Fonte | Modulo/dado de origem. |
| TipoDado | Texto | Nao informado no material | Nao informado no material | Tipo | Tipo final pendente. |
| Ativo | Booleano | Sim/Nao | Sim | Status | Controla uso do atributo. |

### 11.13 `ia_ml_historico`

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| Id | Identificador | Nao informado no material | Sim | Chave primaria funcional | Identificador do historico. |
| RecursoId | Identificador | Nao informado no material | Sim | `ia_ml_recurso` | Recurso auditado. |
| Acao | Texto | Nao informado no material | Sim | Acao | Campo informado. |
| UsuarioId | Identificador | Nao informado no material | Sim | Usuario | Campo informado. |
| PayloadJson | JSON | Nao informado no material | Sim | Auditoria | Campo informado. |
| Timestamp | Data/hora | Nao informado no material | Sim | Auditoria | Material informa timestamp. |
| Ip | Texto | Nao informado no material | Sim | Auditoria | Material informa IP. |

### 11.14 `ia_ml_recurso_anexo`

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| Id | Identificador | Nao informado no material | Sim | Chave primaria funcional | Identificador do anexo. |
| RecursoId | Identificador | Nao informado no material | Sim | `ia_ml_recurso` | Recurso vinculado. |
| ArquivoId | Identificador | Nao informado no material | Sim | GED | Campo informado. |
| CriadoEm | Data/hora | Nao informado no material | Nao informado no material | Auditoria | Data de vinculo. |

### 11.15 `ia_ml_evento`

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| Id | Identificador | Nao informado no material | Sim | Chave primaria funcional | Identificador do evento. |
| RecursoId | Identificador | Nao informado no material | Sim | `ia_ml_recurso` | Recurso relacionado. |
| TipoEvento | Texto | Nao informado no material | Sim | Evento | Tipo final nao informado. |
| PayloadJson | JSON | Nao informado no material | Sim | Payload | Conteudo do evento. |
| PublicadoEm | Data/hora | Nao informado no material | Nao informado no material | Auditoria | Publicado apos confirmacao transacional. |
| StatusPublicacao | Enum/texto | Nao informado no material | Sim | Status | Dominio final pendente. |

## 12. Integracoes e contratos funcionais

| Integracao | Direcao | Dados | Regra |
|---|---|---|---|
| Compliance | Entrada/Saida | Base legal, mascaramento, retencao, anonimizacao e opt-out. | Dados pessoais dependem de regra de conformidade. |
| GED | Entrada/Saida | Anexos, arquivos e bases de conhecimento. | Anexos e RAG referenciam documentos permitidos. |
| API Gateway | Entrada/Saida | Contratos de inferencia e eventos. | Exposicao final nao informada no material. |
| Cadastros Base | Entrada | Pessoa, parceiro, responsavel e dados cadastrais. | IA/ML referencia IDs, nao duplica cadastros. |
| Estoque | Entrada/Saida | Produto, NCM sugerido, serie historica e forecast. | Casos citados dependem de contrato. |
| Faturamento Fiscal Eletronico | Entrada/Saida | XML emitido, alerta de inconsistencia. | Caso de anomalia fiscal citado. |
| Interface Assistida | Saida | Passos de wizard e contexto do tenant. | Caso de assistente citado. |
| Analytics | Saida | Indicadores, consumo e automacoes. | Fase futura citada no material. |
| IoT | Entrada/Saida | Sinais e eventos quando aplicavel. | Dependencia citada, contrato nao informado. |

## 13. Telas, relatorios e experiencia operacional

| ID | Tela/Relatorio | Especificacao |
|---|---|---|
| TEL-IA-001 | Lista | Listar registros de IA/ML com filtros por status, periodo e responsavel; acoes novo e exportar. |
| TEL-IA-002 | Detalhe/Formulario | Exibir abas de dados, historico, anexos e aprovacao. |
| TEL-IA-003 | Painel gestor | Exibir KPIs e fila de aprovacao. |
| REL-IA-001 | Posicao geral | Snapshot por status. |
| REL-IA-002 | Auditoria de alteracoes | Trilha por periodo. |

## 14. Fluxos e estados

| Estado atual | Evento | Proximo estado | Papel |
|---|---|---|---|
| Rascunho | Submeter | EmAnalise | Operador |
| EmAnalise | Aprovar | Ativo | Aprovador |
| EmAnalise | Rejeitar | Rascunho | Aprovador |
| Ativo | Inativar | Inativo | Gestor |
| Ativo | Encerrar | Encerrado | Gestor |
| Inativo | Reativar | Ativo | Gestor |

## 15. Casos de uso priorizados

| Caso | Entrada | Saida | Modulo consumidor | Status de completude |
|---|---|---|---|---|
| Deduplicacao cadastral | Nome e documento parcial | Score de correspondencia | Cadastros Base | Parcial |
| Classificacao NCM | Descricao de produto | NCM sugerido | Estoque | Parcial |
| Previsao de demanda | Serie historica de venda | Forecast por SKU | Estoque | Parcial |
| Anomalia fiscal | XML emitido | Alerta de inconsistencia | Faturamento Fiscal Eletronico | Parcial |
| Assistente de wizard | Contexto do tenant | Passos de interface | Interface Assistida | Parcial |

## 16. Cenarios de validacao

| ID | Cenario | Resultado esperado |
|---|---|---|
| CT-IA-001 | Criar registro valido. | Status Rascunho. |
| CT-IA-002 | Submeter sem obrigatorios. | Operacao bloqueada. |
| CT-IA-003 | Aprovar registro. | Status Ativo. |
| CT-IA-004 | Tentar publicar evento a partir de rascunho. | Sem evento publicado. |
| CT-IA-005 | Inativar com referencia. | Bloqueio ou inativacao conforme regra definida. |
| CT-IA-006 | Aplicar mascaramento LGPD. | Campo oculto quando aplicavel. |
| CT-IA-007 | Executar inferencia com quota excedida. | Inferencia bloqueada ou alertada conforme politica. |
| CT-IA-008 | Executar prompt inativo. | Execucao bloqueada. |

## 17. Indicadores e controles

| Indicador | Descricao |
|---|---|
| Recursos por status | Quantidade de recursos em Rascunho, EmAnalise, Ativo, Inativo e Encerrado. |
| Inferencias por periodo | Quantidade de inferencias solicitadas, processadas, falhas e bloqueadas. |
| Consumo de quota | Uso por tenant, recurso e periodo. |
| Pendencias de aprovacao | Registros aguardando aprovacao. |
| Falhas de integracao | Inferencias ou eventos com falha. |
| Uso por modulo consumidor | Distribuicao de consumo por modulo. |

## 18. Seguranca, privacidade e auditoria

| Area | Regra funcional |
|---|---|
| Tenant | Todos os registros, inferencias, quotas e configuracoes devem ser segregados por tenant. |
| LGPD | Dados pessoais seguem base legal, retencao, anonimizacao e mascaramento definidos em Compliance. |
| Opt-out | Quando aplicavel, o Epros deve bloquear ou ajustar inferencia automatizada. |
| Auditoria | Alteracoes, transicoes, inferencias e eventos devem registrar usuario, horario, IP e payload funcional. |
| Anexos | Arquivos devem ser vinculados por GED. |
| Eventos | Publicacao ocorre somente apos confirmacao transacional. |

## 19. Matriz de rastreabilidade funcional

| Capacidade | Regras | Dados | Testes |
|---|---|---|---|
| Cadastro IA/ML | IA-001 a IA-003 | `ia_ml_recurso` | CT-IA-001, CT-IA-002 |
| Workflow | IA-004 a IA-008 | `ia_ml_recurso`, `ia_ml_historico` | CT-IA-003, CT-IA-005 |
| Eventos | IA-009 | `ia_ml_evento` | CT-IA-004 |
| Historico e anexos | IA-010 a IA-012 | `ia_ml_historico`, `ia_ml_recurso_anexo` | CT-IA-001 |
| Parametrizacao | IA-013 | `ia_ml_config_tenant` | CT-IA-002 |
| LGPD | IA-015, IA-016 | `ia_ml_inferencia`, Compliance | CT-IA-006 |
| Quotas | IA-017 | `ia_ml_quota` | CT-IA-007 |
| Assistentes | IA-018, IA-019 | `ia_ml_prompt`, `ia_ml_prompt_versao` | CT-IA-008 |

## 20. Criterios de aceite

| Criterio | Resultado esperado |
|---|---|
| Modelo de dados | Entidades funcionais, campos obrigatorios, relacionamentos e lacunas estao documentados. |
| Workflow | Transicoes do ciclo de vida estao implementaveis e auditaveis. |
| Auditoria | Historico registra usuario, timestamp, IP e payload. |
| Anexos | Vinculo com GED usa ArquivoId. |
| Tenant | Operacoes sem tenant sao bloqueadas. |
| LGPD | Mascaramento, retencao, anonimizacao e opt-out ficam integrados a Compliance. |
| Quotas | Consumo por tenant pode ser medido e bloqueado/alertado. |
| Ausencia de invencao | O que nao esta informado no material aparece como `Nao informado no material` ou item da MC. |

## 21. Notas de rodape

[^nota1]: As entidades e dominios alem de `ia_ml_recurso`, `ia_ml_historico` e `ia_ml_recurso_anexo` foram criados nesta especificacao para tornar implantaveis as capacidades comprovadas no material: modelos, inferencia, LLM, RAG, quotas, governanca, drift, opt-out, eventos e auditoria. O material nao informa tabelas finais, algoritmos, fornecedores, metricas completas ou contratos definitivos; por isso esses pontos permanecem detalhados na MC.
