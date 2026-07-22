# EF_3_PLATAFORMA_COMPARTILHADA_OFFLINE_SHELL_V1

**Projeto:** Epros  
**Empresa:** Siser  
**Tipo de documento:** Especificacao Funcional definitiva  
**Versao:** V1  
**Modulo:** PLATAFORMA_COMPARTILHADA  
**Submodulo:** OFFLINE_SHELL  
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

O submodulo Offline Shell do Epros deve permitir que operacoes autorizadas continuem em ambiente com conectividade instavel ou ausente, usando fila local, armazenamento local por tenant, sincronizacao posterior, retry exponencial, APIs idempotentes, indicador de conectividade, modo somente leitura quando necessario e tratamento de conflitos para entidades criticas.

| Pergunta | Resposta |
|---|---|
| Para que o submodulo existe? | Para sustentar operacoes desconectadas ou intermitentes com fila local, replay e controle de conflito. |
| Que problema de negocio resolve? | Evita perda de dados em campo e permite continuidade operacional quando o usuario nao possui conexao estavel. |
| Qual resultado operacional deve produzir? | Operacoes enfileiradas, sincronizadas, rejeitadas ou enviadas para resolucao de conflito com auditoria. |
| Quais areas dependem dele? | Vendas, Estoque, Aplicativo, API Gateway, Analytics, Workflow, Compliance e modulos com operacao de campo. |

## 3. Escopo funcional

### 3.1 Dentro do escopo

| Capacidade | Descricao | Observacao |
|---|---|---|
| Shell offline | Controlar experiencia do Epros quando a conexao estiver ausente ou instavel. | Material informa shell offline para operacao desconectada. |
| Fila local | Registrar operacoes locais pendentes de envio. | Material informa fila local e outbox. |
| Armazenamento local por tenant | Manter dados locais segregados por tenant e usuario. | Material informa tenant e armazenamento local. |
| Sincronizacao | Reenviar operacoes pendentes quando a conectividade retornar. | Material informa sincronizacao. |
| Retry exponencial | Controlar novas tentativas com intervalo progressivo. | Material informa retry exponencial. |
| Conflitos | Identificar conflitos de sincronizacao em entidades criticas. | Material informa conflito e tela de resolucao. |
| Resolucao manual | Permitir que usuario autorizado resolva conflito quando necessario. | Material informa resolucao manual. |
| Last-write-wins configuravel | Permitir politica automatica configuravel quando aplicavel. | Material informa esta decisao como opcao. |
| Indicador de conectividade | Mostrar status de conexao e sincronizacao. | Material informa indicador de conectividade. |
| Modo somente leitura | Bloquear escrita quando operacao offline nao for segura. | Material informa modo somente leitura. |
| APIs idempotentes para replay | Exigir contratos que permitam reenviar sem duplicar efeito. | Material informa dependencia com APIs idempotentes. |
| Auditoria | Registrar usuario, timestamp, IP quando disponivel e payload. | Material informa historico. |

### 3.2 Fora do escopo

| Item fora do escopo | Motivo | Destino correto |
|---|---|---|
| Regra de negocio da venda, estoque ou financeiro | Offline Shell transporta e sincroniza operacoes; a regra permanece no modulo dono. | Modulo dono da operacao |
| Definicao de API de cada dominio | Offline Shell exige idempotencia, mas cada endpoint pertence ao modulo exposto. | API Gateway e modulo dono |
| Motor completo de workflow | Pode bloquear ou solicitar aprovacao, mas nao substitui Workflow. | Workflow |
| Resolucao tributaria/fiscal | Dados fiscais sincronizados devem obedecer ao modulo fiscal. | Faturamento Fiscal Eletronico |
| Armazenamento definitivo de documentos | Anexos devem referenciar GED. | GED |
| Politica final de retencao local | Material cita LGPD, mas nao informa prazos. | Compliance |

## 4. Glossario e conceitos funcionais

| Termo | Definicao funcional | Observacoes |
|---|---|---|
| Shell offline | Camada de experiencia que permite operar o Epros sem conexao plena. | Inclui status, fila e bloqueios. |
| Outbox | Fila local de operacoes pendentes de envio. | Termo informado no material. |
| Replay | Reenvio de operacao local para o servidor. | Deve ser idempotente. |
| Retry exponencial | Politica de reenvio com espera crescente entre tentativas. | Material informa requisito. |
| Conflito de sincronizacao | Divergencia entre dado local e dado do servidor no momento do replay. | Exige politica automatica ou manual. |
| Last-write-wins | Politica na qual a ultima alteracao vence. | Deve ser configuravel e restrita a casos permitidos. |
| Modo somente leitura | Estado em que o Epros permite consulta local, mas bloqueia novas escritas. | Material informa requisito. |
| Entidade critica | Registro cujo conflito nao pode ser resolvido sem criterio explicito. | Material cita estoque e venda como exemplos. |
| Idempotencia | Capacidade de repetir a mesma operacao sem duplicar efeito. | Dependencia de API para replay. |

## 5. Atores, papeis e responsabilidades

| Ator/Papel | Responsabilidade | Permissoes esperadas | Restricoes |
|---|---|---|---|
| Usuario de campo | Operar funcionalidades autorizadas em conectividade instavel. | Criar rascunhos e operacoes offline permitidas. | Nao resolve conflito critico sem permissao. |
| Gestor operacional | Acompanhar fila, falhas e conflitos. | Consultar, priorizar e resolver conflitos. | Nao altera regras de negocio do modulo dono. |
| Administrador do tenant | Configurar politica offline por tenant. | Parametrizar offline, modo leitura, conflito e retry. | Deve respeitar Compliance. |
| Epros | Detectar conectividade, armazenar localmente, sincronizar, auditar e publicar eventos. | Automacao sistemica. | Nao deve duplicar efeito em replay. |
| Modulo dono | Validar e aplicar a operacao sincronizada. | Receber replay idempotente. | Deve retornar conflito, sucesso ou rejeicao. |
| Suporte | Diagnosticar fila, falhas e historico. | Consultar auditoria e payload mascarado. | Nao acessa dados sensiveis sem autorizacao. |

## 6. Visao operacional do submodulo

O Epros monitora a conectividade do dispositivo e informa ao usuario se esta online, offline, sincronizando, em erro ou em modo somente leitura. Quando a operacao estiver autorizada para uso offline, o Epros salva localmente o payload, associa tenant, usuario, modulo, entidade, codigo de correlacao, status e data/hora. A operacao entra na fila local e aguarda sincronizacao.

Quando a conectividade retorna, o Epros processa a fila conforme prioridade, tentativa, retry exponencial e dependencias entre operacoes. Cada replay deve usar chave idempotente para evitar duplicidade no servidor. O modulo dono valida a regra de negocio, aplica a operacao e retorna sucesso, rejeicao ou conflito.

Quando houver conflito, o Epros aplica politica configurada. Quando a politica automatica for permitida, o Epros pode usar last-write-wins configuravel. Quando o conflito envolver entidade critica, o item deve ser encaminhado para resolucao manual, com comparacao entre versao local e versao do servidor. O material nao informa contratos finais, tabelas fisicas nem payloads definitivos; esta EF cria modelo funcional implantavel com nota de autoria.[^nota1]

## 7. Capacidades funcionais

### 7.1 Cadastro e governanca da configuracao offline

| Item | Especificacao |
|---|---|
| Objetivo | Controlar quando e como cada tenant/modulo pode operar offline. |
| Acionamento | Configuracao inicial, edicao ou ativacao do modo offline. |
| Pre-condicoes | Tenant valido e usuario administrador autorizado. |
| Dados de entrada | Codigo, status, responsavel, modulos habilitados, politica de conflito, retry, modo leitura e retencao local. |
| Processamento | O Epros valida obrigatorios, registra historico e publica evento apos persistencia. |
| Resultado esperado | Politica offline ativa, inativa ou em aprovacao. |
| Pos-condicoes | Operacoes offline usam a politica definida. |
| Excecoes | Tenant ausente, responsavel ausente, politica incompleta ou modulo sem contrato. |
| Auditoria | Usuario, acao, payload, timestamp e IP quando disponivel. |

### 7.2 Fila local de operacoes

| Item | Especificacao |
|---|---|
| Objetivo | Guardar operacoes geradas sem conexao ou em conexao instavel. |
| Acionamento | Usuario executa operacao autorizada enquanto o Epros esta offline ou sem confirmacao do servidor. |
| Pre-condicoes | Modulo habilitado para offline e usuario autenticado previamente. |
| Dados de entrada | Tenant, usuario, modulo, submodulo, entidade, operacao, payload, chave idempotente e timestamp local. |
| Processamento | O Epros valida permissao local, salva na outbox e atualiza indicador de pendencias. |
| Resultado esperado | Operacao enfileirada como pendente. |
| Pos-condicoes | Sincronizacao podera reenviar o item. |
| Excecoes | Operacao nao permitida offline, payload invalido, armazenamento indisponivel, tenant ausente. |
| Auditoria | Criacao local, payload mascarado e origem. |

### 7.3 Sincronizacao e retry

| Item | Especificacao |
|---|---|
| Objetivo | Enviar operacoes pendentes para o servidor com controle de tentativa. |
| Acionamento | Conectividade restaurada, sincronizacao manual ou tarefa em segundo plano. |
| Pre-condicoes | Fila pendente, API disponivel e chave idempotente presente. |
| Dados de entrada | Item da fila, tentativas, proxima tentativa, payload e correlacao. |
| Processamento | O Epros envia item, recebe resultado, atualiza status e agenda retry exponencial em falha transitoria. |
| Resultado esperado | Item sincronizado, rejeitado, em conflito ou reagendado. |
| Pos-condicoes | Eventos de dominio sao publicados conforme resultado. |
| Excecoes | API indisponivel, autenticacao expirada, conflito, rejeicao de regra, falha definitiva. |
| Auditoria | Tentativa, resposta, erro, duracao e status. |

### 7.4 Conflitos de sincronizacao

| Item | Especificacao |
|---|---|
| Objetivo | Identificar e resolver divergencias entre dado local e dado do servidor. |
| Acionamento | Servidor retorna conflito ou comparacao de versao detecta divergencia. |
| Pre-condicoes | Entidade sincronizada com versao ou base de comparacao. |
| Dados de entrada | Versao local, versao servidor, payload local, payload servidor, politica configurada. |
| Processamento | O Epros classifica conflito, aplica politica automatica quando permitida ou envia para resolucao manual. |
| Resultado esperado | Conflito resolvido, pendente ou rejeitado. |
| Pos-condicoes | Item da fila fica sincronizado ou exige acao humana. |
| Excecoes | Politica ausente, entidade critica, dados insuficientes ou usuario sem permissao. |
| Auditoria | Decisao, responsavel, motivo e payload comparativo mascarado. |

### 7.5 Indicador de conectividade e modo somente leitura

| Item | Especificacao |
|---|---|
| Objetivo | Informar estado operacional e bloquear escritas inseguras. |
| Acionamento | Mudanca de conectividade, expiracao de sessao ou politica de modulo. |
| Pre-condicoes | Usuario em sessao local valida ou operando em modo restrito. |
| Dados de entrada | Status da conexao, sessao, fila, erros e politica do modulo. |
| Processamento | O Epros atualiza indicador e permite ou bloqueia comandos conforme politica. |
| Resultado esperado | Usuario sabe se pode consultar, registrar, sincronizar ou apenas ler. |
| Pos-condicoes | Escritas bloqueadas nao entram na fila indevidamente. |
| Excecoes | Status desconhecido, sessao expirada ou armazenamento indisponivel. |
| Auditoria | Mudanca de estado e comandos bloqueados. |

### 7.6 Auditoria e privacidade local

| Item | Especificacao |
|---|---|
| Objetivo | Registrar trilhas de operacao offline sem expor dados sensiveis. |
| Acionamento | Criacao, sincronizacao, falha, conflito, resolucao ou expurgo. |
| Pre-condicoes | Tenant e usuario/processo identificados. |
| Dados de entrada | Acao, usuario, payload, status, erro, IP quando disponivel e timestamps. |
| Processamento | O Epros grava historico, mascara payload em consulta e aplica retencao quando definida. |
| Resultado esperado | Trilha auditavel para suporte, compliance e operacao. |
| Pos-condicoes | Logs podem alimentar relatorios e indicadores. |
| Excecoes | Politica de retencao nao definida, payload sensivel sem classificacao. |
| Auditoria | A propria trilha e o resultado esperado. |

## 8. Regras de negocio

| Regra | Descricao | Condicao | Resultado | Severidade | Observacoes |
|---|---|---|---|---|---|
| OFF-001 | Toda configuracao offline deve possuir tenant. | Qualquer operacao. | Operacao sem tenant e bloqueada. | Bloqueante | Material informa TenantId obrigatorio. |
| OFF-002 | Registro principal deve possuir codigo, status e responsavel. | Cadastro e manutencao. | Ausencia bloqueia persistencia. | Bloqueante | Campos informados. |
| OFF-003 | Novo registro nasce em Rascunho. | Criacao valida. | Status Rascunho. | Bloqueante | Fluxo informado. |
| OFF-004 | Rascunho pode ser submetido para EmAnalise por operador. | Submissao. | Status EmAnalise. | Normal | Fluxo informado. |
| OFF-005 | EmAnalise pode ser aprovado e tornar-se Ativo. | Aprovacao. | Status Ativo. | Bloqueante | Fluxo informado. |
| OFF-006 | EmAnalise pode ser rejeitado e voltar a Rascunho com motivo. | Rejeicao. | Status Rascunho. | Normal | Fluxo informado. |
| OFF-007 | Ativo pode ser inativado ou encerrado por gestor. | Gestao. | Status Inativo ou Encerrado. | Normal | Fluxo informado. |
| OFF-008 | Inativo pode ser reativado por gestor. | Reativacao. | Status Ativo. | Normal | Fluxo informado. |
| OFF-009 | Transicoes devem registrar usuario, timestamp e IP quando disponivel. | Alteracao de estado. | Historico gravado. | Bloqueante | Auditoria informada. |
| OFF-010 | Eventos de dominio devem ser publicados apos confirmacao transacional. | Persistencia concluida. | Evento publicado. | Normal | Material informa eventos apos commit. |
| OFF-011 | Operacao offline so pode ser enfileirada quando o modulo estiver habilitado para offline. | Usuario cria operacao offline. | Caso contrario, comando e bloqueado ou modo leitura e aplicado. | Bloqueante | Material informa configuracao por escopo. |
| OFF-012 | Item de fila deve possuir tenant, usuario, modulo, entidade, operacao, payload e chave idempotente. | Criacao da fila. | Item incompleto e bloqueado. | Bloqueante | Chave idempotente deriva da dependencia de replay seguro.[^nota1] |
| OFF-013 | Armazenamento local deve ser segregado por tenant. | Gravar ou ler fila local. | Dados de outro tenant nao sao acessados. | Bloqueante | Material informa armazenamento local e multi-tenant. |
| OFF-014 | Dados pessoais locais devem seguir privacidade, retencao e mascaramento. | Fila, historico ou conflito com dados pessoais. | Dado protegido ou expurgado conforme politica. | Bloqueante | Material cita conformidade LGPD. |
| OFF-015 | Sincronizacao deve usar retry exponencial em falha transitoria. | Falha temporaria de envio. | Proxima tentativa e reagendada. | Normal | Requisito informado. |
| OFF-016 | Falha definitiva deve parar retry automatico. | Servidor rejeita por regra ou erro nao recuperavel. | Item fica rejeitado. | Bloqueante | Criado para evitar loop infinito.[^nota1] |
| OFF-017 | Replay deve ser idempotente. | Reenvio de item. | Repeticao nao duplica efeito. | Bloqueante | Material informa APIs idempotentes. |
| OFF-018 | Conflito em entidade critica deve ir para resolucao manual quando politica automatica nao for permitida. | Conflito detectado. | Item fica pendente de resolucao. | Bloqueante | Material cita entidades criticas. |
| OFF-019 | Last-write-wins so pode ser aplicado quando configurado para o modulo/entidade. | Conflito detectado. | Politica automatica aplicada ou bloqueada. | Bloqueante | Material informa configuravel. |
| OFF-020 | Modo somente leitura deve bloquear nova escrita offline quando operacao nao for segura. | Conexao indisponivel ou politica bloqueia escrita. | Usuario pode consultar, mas nao registrar. | Bloqueante | Requisito informado. |
| OFF-021 | Indicador de conectividade deve refletir conexao e status de sincronizacao. | Mudanca de estado. | Usuario visualiza estado atual. | Normal | Requisito informado. |
| OFF-022 | Conflito resolvido manualmente exige permissao e motivo. | Resolucao humana. | Decisao auditada. | Bloqueante | Criado para governanca.[^nota1] |
| OFF-023 | Operacao sincronizada com sucesso deve sair da fila pendente. | Resultado sucesso. | Item fica sincronizado. | Normal | Derivado do fluxo de sincronizacao. |
| OFF-024 | Item rejeitado deve manter motivo consultavel. | Resultado rejeitado. | Usuario/suporte visualiza causa. | Normal | Necessario para operacao. |
| OFF-025 | Operacoes offline devem referenciar cadastros mestres por identificador, sem duplicar mestre. | Payload local. | Duplicacao de cadastro mestre e bloqueada. | Bloqueante | Escopo informa nao duplicar cadastros. |

## 9. Parametros de configuracao

| Parametro | Finalidade | Tipo/formato | Valor padrao | Obrigatorio | Nivel | Quem pode alterar | Impacto |
|---|---|---|---|---|---|---|---|
| OfflineHabilitado | Habilitar o submodulo para o tenant. | Booleano | Nao informado no material | Sim | Tenant | Administrador | Permite ou bloqueia operacao offline. |
| ModulosOffline | Definir modulos habilitados para fila local. | Lista | Nao informado no material | Sim | Tenant/modulo | Administrador | Controla escopo offline. |
| PoliticaConflito | Definir resolucao automatica ou manual. | Enum | Nao informado no material | Sim | Modulo/entidade | Administrador | Controla conflitos. |
| LastWriteWinsHabilitado | Permitir ultima gravacao vencer em casos configurados. | Booleano | Nao informado no material | Condicional | Modulo/entidade | Administrador | Resolve conflitos automaticamente. |
| RetryBase | Intervalo inicial de retry. | Duracao | Nao informado no material | Sim | Tenant | Administrador | Controla sincronizacao. |
| RetryMaximo | Limite de tentativas ou tempo. | Inteiro/duracao | Nao informado no material | Sim | Tenant | Administrador | Evita loop indefinido. |
| ModoSomenteLeitura | Bloquear escritas offline por politica. | Booleano | Nao informado no material | Condicional | Modulo | Administrador | Reduz risco em modulos criticos. |
| RetencaoLocal | Prazo de retencao dos dados locais. | Periodo | Nao informado no material | Sim | Tenant | Compliance/Siser | Define expurgo local. |
| LimiteFilaLocal | Quantidade/tamanho maximo da fila. | Inteiro/tamanho | Nao informado no material | Sim | Tenant/dispositivo | Administrador | Evita consumo excessivo. |
| EntidadesCriticas | Entidades que exigem resolucao manual. | Lista | Estoque e venda citados como exemplos | Sim | Tenant/modulo | Administrador | Controla conflitos sensiveis. |

## 10. Modelo de dados funcional e implantavel

### 10.1 Visao geral do modelo

O material informa somente entidade raiz com Id, TenantId, Codigo, Status e ResponsavelId, historico com Acao, UsuarioId e PayloadJson, anexo com ArquivoId, alem de requisitos de outbox local, conflitos, sincronizacao, retry exponencial, modo leitura, indicador de conectividade e APIs idempotentes. As entidades abaixo consolidam esse conteudo em um modelo funcional implantavel do Epros.[^nota1]

| Grupo de dados | Entidades/tabelas | Papel funcional | Observacoes |
|---|---|---|---|
| Configuracao | `offline_configuracao`, `offline_modulo` | Define politica por tenant e modulo. | Baseado em TenantId/Codigo/Status/ResponsavelId. |
| Fila local | `offline_fila_item`, `offline_fila_tentativa` | Registra operacoes pendentes e tentativas. | Criado a partir de outbox/retry. |
| Conflitos | `offline_conflito`, `offline_conflito_decisao` | Controla divergencias e resolucao. | Criado a partir de SyncConflict. |
| Estado do dispositivo | `offline_dispositivo_estado` | Guarda status de conectividade e sincronizacao. | Criado a partir do indicador. |
| Auditoria | `offline_historico`, `offline_anexo` | Registra alteracoes e arquivos. | Campos informados. |

### 10.2 Entidades, finalidade e cardinalidade

| Entidade | Finalidade | Cardinalidade | Observacao |
|---|---|---|---|
| `offline_configuracao` | Politica offline por tenant. | 1 tenant possui N configuracoes. | Preserva campos raiz. |
| `offline_modulo` | Modulo/entidade habilitado para offline. | 1 configuracao possui N modulos. | Define escopo. |
| `offline_fila_item` | Item de outbox local ou espelhado para diagnostico. | 1 usuario/dispositivo possui N itens. | Necessario para sincronizacao. |
| `offline_fila_tentativa` | Tentativas de envio de um item. | 1 item possui N tentativas. | Necessario para retry. |
| `offline_conflito` | Conflito detectado no replay. | 1 item pode gerar 0 ou N conflitos. | Necessario para resolucao. |
| `offline_conflito_decisao` | Decisao de conflito. | 1 conflito possui N decisoes/historico. | Necessario para auditoria. |
| `offline_dispositivo_estado` | Estado de conectividade e fila no dispositivo. | 1 usuario/dispositivo possui N estados. | Necessario para painel/indicador. |
| `offline_historico` | Historico funcional. | N historicos por entidade. | Campos informados. |
| `offline_anexo` | Anexo vinculado a item/configuracao. | N anexos por entidade. | Usa GED. |

## 11. Dicionario de dados implantavel

### 11.1 `offline_configuracao`

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| id | UUID/inteiro | Nao informado no material | Sim | PK | Identificador da configuracao. |
| tenant_id | UUID/inteiro | Nao informado no material | Sim | FK tenant | Campo TenantId informado. |
| codigo | Texto | Nao informado no material | Sim | Unico por tenant | Campo Codigo informado. |
| status | Enum | Rascunho/EmAnalise/Ativo/Inativo/Encerrado | Sim |  | Campo Status e fluxo informados. |
| responsavel_id | UUID/inteiro | Nao informado no material | Sim | FK pessoa/usuario | Campo ResponsavelId informado. |
| offline_habilitado | Booleano | true/false | Sim |  | Criado para governanca offline.[^nota1] |
| politica_conflito | Enum | Manual/LastWriteWins/PorModulo | Sim |  | Derivado do requisito de conflito. |
| modo_somente_leitura | Booleano | true/false | Sim |  | Requisito informado. |
| retry_base | Duracao | Nao informado no material | Sim |  | Intervalo inicial; valor nao informado. |
| retry_maximo | Inteiro/duracao | Nao informado no material | Sim |  | Limite; valor nao informado. |
| retencao_local | Periodo | Nao informado no material | Sim |  | Necessario por privacidade; prazo nao informado. |
| criado_em | Data/hora | ISO 8601 | Sim |  | Criacao. |
| atualizado_em | Data/hora | ISO 8601 | Sim |  | Atualizacao. |

### 11.2 `offline_modulo`

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| id | UUID/inteiro | Nao informado no material | Sim | PK | Identificador. |
| configuracao_id | UUID/inteiro | Nao informado no material | Sim | FK offline_configuracao | Politica dona. |
| modulo | Texto | Nao informado no material | Sim |  | Modulo habilitado. |
| submodulo | Texto | Nao informado no material | Nao |  | Submodulo habilitado quando aplicavel. |
| entidade | Texto | Nao informado no material | Condicional |  | Entidade sincronizavel. |
| permite_escrita_offline | Booleano | true/false | Sim |  | Define se entra na fila. |
| exige_resolucao_manual | Booleano | true/false | Sim |  | Entidade critica. |
| politica_conflito | Enum | Manual/LastWriteWins/Nao informado no material | Sim |  | Politica por modulo. |
| status | Enum | Ativo/Inativo | Sim |  | Controla uso. |

### 11.3 `offline_fila_item`

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| id | UUID/inteiro | Nao informado no material | Sim | PK | Identificador do item. |
| tenant_id | UUID/inteiro | Nao informado no material | Sim | FK tenant | Segregacao. |
| usuario_id | UUID/inteiro | Nao informado no material | Sim | FK usuario | Usuario que originou. |
| dispositivo_id | Texto/UUID | Nao informado no material | Condicional |  | Dispositivo local; criado para rastreio.[^nota1] |
| modulo | Texto | Nao informado no material | Sim |  | Modulo dono. |
| submodulo | Texto | Nao informado no material | Nao |  | Submodulo dono. |
| entidade | Texto | Nao informado no material | Sim |  | Entidade afetada. |
| entidade_id_local | Texto/UUID | Nao informado no material | Condicional |  | Identificador local temporario. |
| operacao | Enum | Criar/Atualizar/Excluir/Acao | Sim |  | Tipo de operacao. |
| payload_json | JSON | Nao informado no material | Sim |  | Dados da operacao. |
| chave_idempotente | Texto | Nao informado no material | Sim | Unica por tenant/modulo | Necessaria para replay seguro. |
| status | Enum | Pendente/Sincronizando/Sincronizado/Rejeitado/Conflito | Sim |  | Estados criados para fila.[^nota1] |
| tentativa_atual | Inteiro | >= 0 | Sim |  | Controle de retry. |
| proxima_tentativa_em | Data/hora | ISO 8601 | Nao |  | Retry exponencial. |
| criado_local_em | Data/hora | ISO 8601 | Sim |  | Data local. |
| sincronizado_em | Data/hora | ISO 8601 | Nao |  | Data de sucesso. |
| erro | Texto | Nao informado no material | Nao |  | Motivo de rejeicao/falha. |

### 11.4 `offline_fila_tentativa`

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| id | UUID/inteiro | Nao informado no material | Sim | PK | Identificador da tentativa. |
| fila_item_id | UUID/inteiro | Nao informado no material | Sim | FK offline_fila_item | Item enviado. |
| numero_tentativa | Inteiro | >= 1 | Sim |  | Sequencia. |
| enviada_em | Data/hora | ISO 8601 | Sim |  | Data de envio. |
| resultado | Enum | Sucesso/FalhaTransitoria/FalhaDefinitiva/Conflito | Sim |  | Resultado da tentativa. |
| http_status | Inteiro | 100-599 | Nao |  | Quando aplicavel. |
| resposta_json | JSON | Nao informado no material | Nao |  | Resposta mascarada. |
| erro | Texto | Nao informado no material | Nao |  | Motivo. |
| duracao_ms | Inteiro | >= 0 | Nao |  | Indicador operacional. |

### 11.5 `offline_conflito`

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| id | UUID/inteiro | Nao informado no material | Sim | PK | Identificador do conflito. |
| fila_item_id | UUID/inteiro | Nao informado no material | Sim | FK offline_fila_item | Item que gerou conflito. |
| tenant_id | UUID/inteiro | Nao informado no material | Sim | FK tenant | Segregacao. |
| modulo | Texto | Nao informado no material | Sim |  | Modulo dono. |
| entidade | Texto | Nao informado no material | Sim |  | Entidade em conflito. |
| payload_local | JSON | Nao informado no material | Sim |  | Versao local mascarada. |
| payload_servidor | JSON | Nao informado no material | Condicional |  | Versao do servidor mascarada. |
| politica_aplicavel | Enum | Manual/LastWriteWins/Nao informado no material | Sim |  | Politica no momento. |
| status | Enum | Pendente/Resolvido/Rejeitado | Sim |  | Estado do conflito. |
| detectado_em | Data/hora | ISO 8601 | Sim |  | Data de deteccao. |

### 11.6 `offline_conflito_decisao`

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| id | UUID/inteiro | Nao informado no material | Sim | PK | Identificador da decisao. |
| conflito_id | UUID/inteiro | Nao informado no material | Sim | FK offline_conflito | Conflito decidido. |
| decisao | Enum | UsarLocal/UsarServidor/Mesclar/Rejeitar | Sim |  | Opcoes criadas para resolucao.[^nota1] |
| motivo | Texto | Nao informado no material | Sim |  | Obrigatorio para auditoria. |
| usuario_id | UUID/inteiro | Nao informado no material | Sim | FK usuario | Responsavel. |
| payload_resultante | JSON | Nao informado no material | Nao |  | Resultado da decisao. |
| decidido_em | Data/hora | ISO 8601 | Sim |  | Data da decisao. |

### 11.7 `offline_dispositivo_estado`

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| id | UUID/inteiro | Nao informado no material | Sim | PK | Identificador. |
| tenant_id | UUID/inteiro | Nao informado no material | Sim | FK tenant | Segregacao. |
| usuario_id | UUID/inteiro | Nao informado no material | Sim | FK usuario | Usuario local. |
| dispositivo_id | Texto/UUID | Nao informado no material | Sim |  | Dispositivo. |
| conectividade | Enum | Online/Offline/Instavel/Desconhecida | Sim |  | Indicador de conectividade. |
| sync_status | Enum | Ocioso/Sincronizando/Erro/SomenteLeitura | Sim |  | Estado operacional. |
| pendentes | Inteiro | >= 0 | Sim |  | Quantidade na fila. |
| conflitos | Inteiro | >= 0 | Sim |  | Quantidade de conflitos. |
| ultima_sincronizacao_em | Data/hora | ISO 8601 | Nao |  | Ultimo sucesso. |
| atualizado_em | Data/hora | ISO 8601 | Sim |  | Ultima atualizacao. |

### 11.8 `offline_historico`

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| id | UUID/inteiro | Nao informado no material | Sim | PK | Identificador. |
| tenant_id | UUID/inteiro | Nao informado no material | Sim | FK tenant | Segregacao. |
| entidade | Texto | Nao informado no material | Sim |  | Entidade afetada. |
| entidade_id | UUID/inteiro/texto | Nao informado no material | Sim |  | Registro afetado. |
| acao | Texto | Nao informado no material | Sim |  | Campo Acao informado. |
| usuario_id | UUID/inteiro | Nao informado no material | Sim | FK usuario | Campo UsuarioId informado. |
| payload_json | JSON | Nao informado no material | Sim |  | Campo PayloadJson informado; mascarar sensiveis. |
| ip | IP | IPv4/IPv6 | Nao |  | Auditoria de transicao. |
| criado_em | Data/hora | ISO 8601 | Sim |  | Data do historico. |

### 11.9 `offline_anexo`

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| id | UUID/inteiro | Nao informado no material | Sim | PK | Identificador. |
| entidade | Texto | Nao informado no material | Sim |  | Configuracao, fila, conflito ou historico. |
| entidade_id | UUID/inteiro/texto | Nao informado no material | Sim |  | Registro relacionado. |
| arquivo_id | UUID/inteiro | Nao informado no material | Sim | FK GED | Campo ArquivoId informado. |
| criado_por | UUID/inteiro | Nao informado no material | Sim | FK usuario | Usuario. |
| criado_em | Data/hora | ISO 8601 | Sim |  | Data de inclusao. |

## 12. Fluxos e estados

### 12.1 Ciclo de vida da configuracao

| Estado atual | Evento | Proximo estado | Permissao | Regra |
|---|---|---|---|---|
| Rascunho | Submeter | EmAnalise | Operador | Validar obrigatorios. |
| EmAnalise | Aprovar | Ativo | Aprovador | Politica passa a valer. |
| EmAnalise | Rejeitar | Rascunho | Aprovador | Exigir motivo. |
| Ativo | Inativar | Inativo | Gestor | Bloqueia novas filas. |
| Ativo | Encerrar | Encerrado | Gestor | Finaliza uso. |
| Inativo | Reativar | Ativo | Gestor | Reabilita uso. |

### 12.2 Estados do item de fila

| Estado | Significado | Proxima acao |
|---|---|---|
| Pendente | Item salvo localmente e aguardando envio. | Sincronizar quando possivel. |
| Sincronizando | Item em tentativa de replay. | Aguardar resposta. |
| Sincronizado | Servidor aceitou operacao. | Remover da fila pendente/logar sucesso. |
| Rejeitado | Servidor negou por regra ou falha definitiva. | Exibir motivo e bloquear retry automatico. |
| Conflito | Divergencia detectada. | Resolver automaticamente ou manualmente. |

### 12.3 Estados do indicador

| Estado | Significado | Comportamento |
|---|---|---|
| Online | Servidor alcancavel. | Operacoes podem ir direto ou sincronizar fila. |
| Offline | Servidor indisponivel. | Escritas permitidas somente se modulo aceitar offline. |
| Instavel | Conexao intermitente. | Preferir fila e confirmar envio posteriormente. |
| SomenteLeitura | Escrita bloqueada por politica. | Usuario apenas consulta dados permitidos. |
| Erro | Sincronizacao falhou. | Exibir motivo e permitir diagnostico. |

## 13. APIs e contratos funcionais

| Contrato | Direcao | Entrada | Saida | Observacoes |
|---|---|---|---|---|
| Registrar configuracao offline | Cliente para Epros | Tenant, codigo, status, responsavel, politicas | Configuracao salva | Endpoint final nao informado. |
| Consultar politica offline | Cliente para Epros | Tenant, modulo, usuario | Politica aplicavel | Necessario para shell local. |
| Enfileirar operacao local | Cliente local | Modulo, entidade, operacao, payload, chave idempotente | Item pendente | Persistencia local. |
| Reenviar item | Cliente para Epros | Chave idempotente, payload, contexto | Sucesso, rejeicao ou conflito | APIs devem ser idempotentes. |
| Consultar fila | Cliente/local/suporte | Usuario, tenant, status | Itens e contadores | Para painel e suporte. |
| Resolver conflito | Usuario autorizado para Epros | Conflito, decisao, motivo | Conflito resolvido | Exige permissao. |
| Atualizar conectividade | Cliente local | Estado de conexao e fila | Indicador atualizado | Pode ser local e/ou enviado ao servidor. |

## 14. Telas, consultas e relatorios

| Interface | Objetivo | Campos/acoes minimas | Observacoes |
|---|---|---|---|
| Indicador de conectividade | Mostrar estado online/offline/sincronizando/erro/somente leitura. | Estado, pendentes, conflitos, ultima sincronizacao. | Requisito informado. |
| Painel da fila | Consultar itens pendentes, sincronizados, rejeitados e em conflito. | Status, modulo, entidade, usuario, tentativa, erro, data. | Criado para operacao do requisito. |
| Resolucao de conflitos | Comparar local/servidor e decidir. | Payload local, payload servidor, politica, decisao, motivo. | Material informa tela para entidades criticas. |
| Configuracao offline | Parametrizar tenant/modulo. | Modulos, politica, retry, modo leitura, retencao. | Campos finais incompletos no material. |
| Historico | Consultar alteracoes e sincronizacoes. | Acao, usuario, payload, IP, periodo. | Material informa auditoria. |
| Lista administrativa | Consultar configuracoes. | Status, periodo, responsavel, novo, exportar. | Material informa tela lista. |
| Detalhe | Ver dados, historico, anexos e aprovacao. | Dados, historico, anexos, aprovacao. | Material informa abas. |
| Painel gestor | KPIs e fila de aprovacao. | Pendentes, conflitos, falhas, aprovacao. | Material informa painel gestor. |

| Relatorio | Descricao | Filtros | Observacoes |
|---|---|---|---|
| Posicao geral | Snapshot por status da configuracao e fila. | Tenant, modulo, status, periodo. | Material informa posicao geral. |
| Auditoria de alteracoes | Trilha por periodo. | Usuario, acao, entidade, periodo. | Material informa auditoria. |
| Sincronizacao | Pendentes, sucesso, rejeicao e conflitos. | Modulo, usuario, status, periodo. | Criado para operar fila.[^nota1] |
| Conflitos | Conflitos pendentes e resolvidos. | Entidade, decisao, responsavel, periodo. | Necessario para entidades criticas. |

## 15. Seguranca, privacidade e auditoria

| Tema | Regra funcional |
|---|---|
| Tenant | Dados locais e sincronizados devem ser segregados por tenant. |
| Usuario | Operacoes offline devem estar vinculadas a usuario previamente autenticado. |
| Dados locais | Armazenamento local deve respeitar privacidade, retencao e expurgo. |
| Payload | Payloads exibidos devem ser mascarados quando contiverem dados sensiveis. |
| Idempotencia | Toda operacao replay deve possuir chave idempotente. |
| Conflito | Resolucao manual exige permissao, motivo e historico. |
| Modo leitura | Modulos sem seguranca offline devem bloquear escrita local. |
| Anexos | Arquivos devem ser referenciados pelo GED. |
| Auditoria | Criacao, envio, falha, conflito e decisao devem gerar historico. |

## 16. Testes funcionais minimos

| Cenario | Dado/condicao | Resultado esperado |
|---|---|---|
| Criar configuracao valida | Tenant, codigo, status e responsavel informados. | Status Rascunho. |
| Criar sem obrigatorios | Falta tenant/codigo/status/responsavel. | Erro de validacao. |
| Aprovar configuracao | Registro EmAnalise e aprovador autorizado. | Status Ativo. |
| Integracao rejeita rascunho | Configuracao ainda Rascunho. | Nenhum evento de aplicacao. |
| Inativar com referencia | Configuracao ativa em uso. | Bloqueio ou inativacao conforme politica. |
| Mascaramento LGPD | Payload com dado pessoal. | Campo oculto em consulta. |
| Enfileirar offline permitido | Modulo habilitado, usuario valido. | Item Pendente. |
| Enfileirar offline bloqueado | Modulo nao habilitado. | Comando bloqueado ou modo leitura. |
| Replay com sucesso | API disponivel e chave idempotente. | Item Sincronizado. |
| Replay duplicado | Mesma chave idempotente reenviada. | Sem duplicidade de efeito. |
| Falha transitoria | API indisponivel. | Retry exponencial agendado. |
| Falha definitiva | Servidor rejeita regra. | Item Rejeitado sem retry automatico. |
| Conflito critico | Servidor retorna conflito em entidade critica. | Item em Conflito e resolucao manual exigida. |
| Last-write-wins permitido | Conflito em entidade configurada. | Politica automatica aplicada. |
| Modo somente leitura | Offline e modulo bloqueia escrita. | Usuario nao consegue gravar. |

## 17. Criterios de aceite

| Codigo | Criterio |
|---|---|
| CA-OFF-001 | Configuracao offline deve possuir tenant, codigo, status e responsavel. |
| CA-OFF-002 | Ciclo Rascunho, EmAnalise, Ativo, Inativo e Encerrado deve funcionar com auditoria. |
| CA-OFF-003 | Operacao offline so deve ser aceita para modulo habilitado. |
| CA-OFF-004 | Item de fila deve registrar tenant, usuario, modulo, entidade, operacao, payload e chave idempotente. |
| CA-OFF-005 | Sincronizacao deve usar retry exponencial em falha transitoria. |
| CA-OFF-006 | Replay deve ser idempotente e nao duplicar efeito. |
| CA-OFF-007 | Conflito deve ser classificado e resolvido por politica automatica ou manual. |
| CA-OFF-008 | Entidade critica deve exigir resolucao manual quando configurada. |
| CA-OFF-009 | Indicador de conectividade deve exibir estado e pendencias. |
| CA-OFF-010 | Modo somente leitura deve bloquear escritas inseguras. |
| CA-OFF-011 | Historico deve registrar usuario, acao, payload, timestamp e IP quando disponivel. |
| CA-OFF-012 | Dados locais devem respeitar privacidade, mascaramento e retencao. |

## 18. Notas de autoria e saneamento funcional

[^nota1]: O modelo de fila, tentativa, conflito, decisao, dispositivo e estados operacionais foi criado nesta EF para tornar o Epros implantavel. O material comprova requisitos de outbox local, sincronizacao, conflitos, retry exponencial, modo somente leitura, indicador de conectividade e APIs idempotentes, mas nao informa tabelas fisicas, contratos finais, payloads nem dominios completos.
