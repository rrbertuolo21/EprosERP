# EF 3 Plataforma Compartilhada - Configuracao V1

## 1. Identificacao

| Item | Valor |
|---|---|
| Sistema | Epros |
| Empresa | Siser |
| Modulo | Plataforma Compartilhada |
| Submodulo | Configuracao |
| Versao | V1 |
| Status | Especificacao funcional para validacao humana |
| Data | 2026-06-06 |

## 2. Objetivo funcional

O submodulo Configuracao centraliza parametros globais, parametros por tenant, modulos habilitados, preferencias de usuario, internacionalizacao, e-mail operacional, captcha, tema, logos, moeda, numeracao, status auxiliares, campos personalizados, politicas de arquivo, tarefas em segundo plano, instalacao tecnica, versionamento de banco, upload/download transversal, auditoria, concorrencia otimista e administracao de chave/valor do Epros.

O submodulo e transversal: ele nao e dono das regras de negocio de vendas, compras, financeiro, fiscal, projetos, RH, documentos, tickets, pagamentos ou usuarios. Ele fornece a infraestrutura de parametros e expoe configuracoes consumidas pelos modulos donos.

## 3. Escopo

### 3.1 Dentro do escopo

| Capacidade | Descricao |
|---|---|
| Hub de configuracao | Menu central para acessar secoes de configuracao do tenant. |
| Parametros gerais | Fuso, idioma, formato de data/hora, paginacao, tema, logos, moeda e branding. |
| Modulos habilitados | Controlar flags de modulos ativos/inativos e expor leitura para runtime. |
| Status auxiliares | Manter status de fatura, lead, tarefa, ticket e outras listas configuraveis. |
| Preferencias de usuario | Armazenar configuracoes por usuario, tipo e nome. |
| E-mail operacional | Configurar remetente, metodo, SMTP, templates, fila e teste de envio. |
| Captcha e seguranca basica | Configurar captcha, politicas de senha, bloqueios e limites operacionais. |
| Campos personalizados | Criar campos por entidade, com tipo, obrigatoriedade e exibicao. |
| Arquivos e armazenamento | Controlar tipos aceitos, tamanho, servidor padrao, pastas, download e politicas de arquivo. |
| Internacionalizacao | Manter idiomas, chaves de traducao, conteudos por idioma, importacao/exportacao e recarga. |
| Tarefas em segundo plano | Monitorar tarefas, execucoes, logs e crons funcionais do Epros. |
| Instalacao e versionamento | Controlar primeira configuracao, upgrades e versoes aplicadas. |
| Concorrencia otimista | Evitar sobrescrita quando registros de negocio forem alterados simultaneamente. |
| Auditoria e excecoes | Registrar logs de erro, eventos de configuracao, alteracoes e execucoes. |
| Plugins e temas | Registrar componentes instalados/habilitados, ordem e configuracao. |

### 3.2 Fora do escopo

| Tema | Tratamento |
|---|---|
| Regra operacional do modulo dono | Permanece no modulo correspondente. |
| Cobranca, gateways e pedidos SaaS | Pertencem a Pedidos e Cobranca SaaS e Servicos Financeiros. |
| Usuarios, papeis e permissoes detalhadas | Pertencem a Usuarios e Papeis e Permissoes de Menu. |
| Tickets e IMAP | Pertencem ao modulo de atendimento/colaboracao. |
| Formularios dinamicos | Pertencem a Interface Assistida e Wizards. |
| Produtos, unidades e itens | Pertencem a Estoque e Cadastros Base. |
| Documentos e armazenamento documental completo | Pertencem a Gestao Eletronica de Documentos. |
| KPIs de dashboard | Pertencem a Analytics e ao modulo dono do indicador. |

## 4. Dependencias e consumidores

| Dependencia/Consumidor | Uso |
|---|---|
| Identidade e Contexto Tenant | Resolver tenant, empresa, usuario e perfil nas configuracoes. |
| Usuarios e Papeis | Autorizar alteracao e visualizacao de parametros. |
| API Gateway e OpenAPI | Publicar endpoints, erros padronizados e correlationId. |
| Gestao Eletronica de Documentos | Consumir politicas de arquivo, upload, pastas e storage. |
| SOA e Colaboracao | Consumir e-mail, templates, notificacoes e fila. |
| Analytics e Mobilidade | Consumir preferencias, paginacao, idioma, filtros e indicadores. |
| Aplicativo | Consumir modulos habilitados, tenant, assinatura, limites e branding. |
| Cadastros Base | Consumir moeda, pais, idioma, parametros iniciais e preferencias. |
| Todos os modulos operacionais | Consultar configuracoes ativas e registrar eventos de auditoria quando alterarem parametros criticos. |

## 5. Principios funcionais

| Codigo | Regra |
|---|---|
| REG-CFG-001 | Toda configuracao tenantizada deve possuir tenant_id. |
| REG-CFG-002 | Parametro global da Siser deve ser separado de parametro do tenant. |
| REG-CFG-003 | Toda alteracao de parametro critico deve gerar historico antes/depois. |
| REG-CFG-004 | Parametro secreto deve ser armazenado protegido e nunca exibido integralmente apos gravacao. |
| REG-CFG-005 | Configuracao lida em runtime deve ser cacheavel, mas precisa invalidacao quando alterada. |
| REG-CFG-006 | Modulo pode ler flags de configuracao, mas somente Configuracao deve escrever parametros transversais. |
| REG-CFG-007 | Configuracao de modulo dono deve declarar fronteira para evitar duplicidade. |
| REG-CFG-008 | Alteracao por usuario sem permissao administrativa deve ser bloqueada. |
| REG-CFG-009 | Campos personalizados devem respeitar entidade, tipo, obrigatoriedade, ordem e exibicao. |
| REG-CFG-010 | Chave de traducao deve possuir idioma, conteudo e origem de carregamento. |
| REG-CFG-011 | Upload temporario deve ser vinculado depois ao registro dono ou descartado por retencao. |
| REG-CFG-012 | Concorrencia otimista deve bloquear update quando a versao enviada estiver vencida. |
| REG-CFG-013 | Tarefa em segundo plano deve registrar status, inicio, fim, servidor e mensagem. |
| REG-CFG-014 | Instalacao tecnica deve validar pre-requisitos antes de criar ambiente. |
| REG-CFG-015 | Configuracoes de pagamento presentes no material devem permanecer como fronteira, nao como financeiro do Epros. |

## 6. Regras funcionais detalhadas

### 6.1 Hub e navegacao de configuracao

| Codigo | Regra |
|---|---|
| REG-CFG-016 | O Epros deve oferecer hub central de configuracao com menu lateral e secoes agrupadas. |
| REG-CFG-017 | O menu de configuracao deve exibir apenas secoes autorizadas ao usuario. |
| REG-CFG-018 | Secoes de configuracao devem ser acessiveis por link direto. |
| REG-CFG-019 | Link direto deve resolver a secao correta sem expor rota nao autorizada. |
| REG-CFG-020 | Configuracoes de outros modulos devem aparecer como atalhos quando o modulo estiver habilitado. |

### 6.2 Parametros gerais do tenant

| Codigo | Regra |
|---|---|
| REG-CFG-021 | Tenant deve possuir parametros de fuso horario, formato de data, formato de data/hora, idioma padrao e paginacao. |
| REG-CFG-022 | Tenant deve possuir parametros de tema, logos, nome de exibicao, moeda e simbolo monetario quando aplicavel. |
| REG-CFG-023 | Alteracao de tema/logo deve afetar apenas o tenant ou escopo autorizado. |
| REG-CFG-024 | Parametro de moeda deve ser validado antes de ser usado por faturamento ou relatorios. |
| REG-CFG-025 | Parametro de paginacao deve definir limite padrao e limite maximo. |

### 6.3 Modulos habilitados

| Codigo | Regra |
|---|---|
| REG-CFG-026 | Cada modulo configuravel deve possuir chave de habilitacao com valor enabled/disabled. |
| REG-CFG-027 | Modulo desabilitado deve ocultar menu e bloquear rotas operacionais conforme politica do modulo. |
| REG-CFG-028 | Leitura de modulos habilitados pode ser feita por runtime, mas escrita deve passar por Configuracao ou pelo fluxo de assinatura/limites quando aplicavel. |
| REG-CFG-029 | Modulos de plano SaaS podem ser somente leitura para o tenant quando vierem do contrato comercial. |
| REG-CFG-030 | Alteracao de modulo habilitado deve invalidar cache de menu e permissoes. |

### 6.4 Status, categorias e listas auxiliares

| Codigo | Regra |
|---|---|
| REG-CFG-031 | Status auxiliares devem possuir codigo, nome, cor, ordem, status ativo/inativo e modulo dono. |
| REG-CFG-032 | Status de fatura, lead, tarefa, ticket e prioridade de tarefa devem ser configuraveis quando o modulo dono permitir. |
| REG-CFG-033 | Reordenacao de status deve preservar unicidade de ordem dentro do modulo e tenant. |
| REG-CFG-034 | Status em uso nao deve ser excluido fisicamente. |
| REG-CFG-035 | Taxas, fontes, marcos, categorias de base de conhecimento e tags devem possuir CRUD controlado por permissao. |

### 6.5 E-mail, templates e fila

| Codigo | Regra |
|---|---|
| REG-CFG-036 | Tenant deve possuir configuracao de e-mail geral e SMTP quando envio autenticado for usado. |
| REG-CFG-037 | SMTP deve armazenar host, porta, autenticacao, usuario, senha protegida, remetente e metodo de envio. |
| REG-CFG-038 | O Epros deve permitir teste de envio de e-mail operacional. |
| REG-CFG-039 | Templates de e-mail devem possuir chave, assunto, corpo, idioma, status e variaveis permitidas. |
| REG-CFG-040 | Fila de e-mail deve permitir reprocessar mensagens com falha quando autorizado. |
| REG-CFG-041 | Limite de envio por hora deve ser configuravel quando aplicavel. |

### 6.6 Captcha, seguranca e restricoes

| Codigo | Regra |
|---|---|
| REG-CFG-042 | Captcha deve poder ser habilitado para login, cadastro ou download conforme escopo. |
| REG-CFG-043 | Politica de senha deve suportar tamanho minimo, tamanho maximo, maiusculas, numeros e caracteres especiais. |
| REG-CFG-044 | Bloqueio por IP deve registrar IP, data, tipo, notas e expiracao. |
| REG-CFG-045 | Bloqueio por tentativas de login deve ser configuravel. |
| REG-CFG-046 | Modo manutencao deve bloquear acesso operacional conforme politica aprovada. |

### 6.7 Campos personalizados

| Codigo | Regra |
|---|---|
| REG-CFG-047 | Campo personalizado deve estar vinculado a entidade funcional e tenant. |
| REG-CFG-048 | Campo personalizado deve definir tipo, nome, label, obrigatoriedade, ordem e exibicao. |
| REG-CFG-049 | Campo personalizado deve poder ser ativado/inativado sem apagar valores historicos. |
| REG-CFG-050 | Exibicao em tabela, formulario e relatorio deve respeitar a configuracao do campo. |
| REG-CFG-051 | Campo personalizado de entidade externa ao submodulo deve ser governado pelo modulo dono. |

### 6.8 Arquivos, upload e armazenamento

| Codigo | Regra |
|---|---|
| REG-CFG-052 | Upload temporario deve aceitar um arquivo por requisicao quando esse modo estiver configurado. |
| REG-CFG-053 | Upload de imagem deve poder gerar miniatura quando aplicavel. |
| REG-CFG-054 | Download deve validar permissao e politica de acesso antes de entregar arquivo. |
| REG-CFG-055 | Configuracao de storage deve suportar servidor padrao, tipos aceitos, tipos bloqueados, tamanho maximo, pastas e regras de visibilidade. |
| REG-CFG-056 | Servidor de arquivo deve registrar rotulo, tipo, endereco, porta, credenciais protegidas, status, caminho, dominio, uso e prioridade. |
| REG-CFG-057 | Fila de acao de arquivo deve registrar delete, move ou restore, status, mensagem, criacao, atualizacao e data de acao. |
| REG-CFG-058 | Token de download deve possuir token, usuario quando houver, IP, arquivo, criacao, expiracao, velocidade e threads maximas. |
| REG-CFG-059 | Rastreamento de download deve registrar arquivo, IP, usuario, inicio, atualizacao, fim, status e offsets. |

### 6.9 Internacionalizacao

| Codigo | Regra |
|---|---|
| REG-CFG-060 | Idioma deve possuir identificador, nome, status ativo/inativo, flag e direcao de leitura quando aplicavel. |
| REG-CFG-061 | Chave de traducao deve possuir chave, conteudo padrao, area administrativa e indicador de varredura. |
| REG-CFG-062 | Conteudo traduzido deve vincular chave e idioma. |
| REG-CFG-063 | Administrador autorizado deve poder importar, exportar e reconstruir traducoes. |
| REG-CFG-064 | Alteracao de traducao deve recarregar o registro de textos do Epros. |
| REG-CFG-065 | Strings com marcacao visual devem ser preservadas como conteudo controlado e revisadas antes de exibicao. |

### 6.10 Preferencias, permissoes e usuario

| Codigo | Regra |
|---|---|
| REG-CFG-066 | Preferencia de usuario deve possuir usuario, tipo, nome e valor. |
| REG-CFG-067 | Preferencia deve ser unica por usuario, tipo e nome. |
| REG-CFG-068 | Permissoes de configuracao devem ser aplicadas por perfil, usuario, rota e acao. |
| REG-CFG-069 | Registro de excecao deve capturar aplicacao, maquina, data, tipo, origem, mensagem, detalhe, URL, metodo, IP, status e hash de erro. |
| REG-CFG-070 | Registro de versao aplicada deve guardar versao, data de aplicacao e descricao. |

### 6.11 Tarefas em segundo plano

| Codigo | Regra |
|---|---|
| REG-CFG-071 | Tarefa em segundo plano deve possuir nome, ultimo update e status running/finished/not_run. |
| REG-CFG-072 | Log da tarefa deve possuir task_id, inicio, fim, status, servidor e mensagem. |
| REG-CFG-073 | Tarefas de poda, fila de arquivos, downloads remotos, arquivos redundantes, notificacoes e plugins devem ser rastreadas. |
| REG-CFG-074 | Falha de tarefa deve ficar visivel para operacao autorizada. |
| REG-CFG-075 | Agendamento externo nao informado no material deve permanecer como lacuna na MC. |

### 6.12 Instalacao e upgrades

| Codigo | Regra |
|---|---|
| REG-CFG-076 | Instalador deve validar pre-requisitos tecnicos antes de permitir configuracao inicial. |
| REG-CFG-077 | Instalador deve coletar dados de banco, criar estrutura e gravar configuracao inicial. |
| REG-CFG-078 | Upgrade deve registrar scripts aplicados e versao resultante. |
| REG-CFG-079 | Ambiente inicial nao deve manter senha padrao operacional. |
| REG-CFG-080 | Configuracao de banco, URL, paths e storage deve ficar protegida. |

### 6.13 Concorrencia e auditoria

| Codigo | Regra |
|---|---|
| REG-CFG-081 | Entidade com concorrencia otimista deve possuir versao de linha. |
| REG-CFG-082 | Atualizacao deve comparar versao enviada com versao atual. |
| REG-CFG-083 | Conflito de versao deve impedir gravacao e retornar erro funcional. |
| REG-CFG-084 | Auditoria padrao deve registrar insert, update, delete logico, usuario e data/hora. |
| REG-CFG-085 | Exclusao logica deve usar status ativo/inativo em vez de apagar fisicamente quando houver historico. |

## 7. Estados

| Entidade | Estados | Observacao |
|---|---|---|
| Parametro | Rascunho; Ativo; Inativo; Substituido | Configuracao versionavel. |
| Modulo habilitado | Enabled; Disabled | Valor preservado do material. |
| Tarefa em segundo plano | running; finished; not_run | Estados preservados. |
| Log de tarefa | started; finished | Estados preservados. |
| Download | downloading; finished; error; cancelled | Estados preservados. |
| Acao de arquivo | pending; processing; complete; failed; cancelled | Estados preservados. |
| Usuario | active; pending; disabled; suspended | Estados preservados. |
| Pedido premium | pending; cancelled; completed | Fronteira de monetizacao, nao financeiro operacional. |
| Idioma | Ativo; Inativo; Bloqueado | Conforme campos isActive/isLocked. |
| Tema/plugin | Instalado; Nao instalado; Habilitado; Desabilitado | Conforme campos do material. |

## 8. Modelo de dados funcional e implantavel

### 8.1 Entidades canonicas do Epros

| Entidade | Tipo | Finalidade |
|---|---|---|
| cfg_parametro | Configuracao | Chaves e valores de configuracao por tenant/escopo. |
| cfg_parametro_grupo | Configuracao | Agrupar parametros por secao. |
| cfg_modulo_flag | Configuracao | Controlar enabled/disabled por modulo. |
| cfg_status_auxiliar | Configuracao | Status e prioridades configuraveis. |
| cfg_preferencia_usuario | Configuracao | Preferencias por usuario. |
| cfg_campo_personalizado | Configuracao | Campos customizados por entidade. |
| cfg_email_configuracao | Configuracao | Metodo de envio e SMTP. |
| cfg_email_template | Configuracao | Templates de e-mail. |
| cfg_email_fila | Movimento | Mensagens pendentes/falhas. |
| cfg_idioma | Configuracao | Idiomas do Epros. |
| cfg_traducao_chave | Configuracao | Chaves de traducao. |
| cfg_traducao_conteudo | Configuracao | Conteudo por idioma. |
| cfg_upload_politica | Configuracao | Tipos, limites e storage. |
| cfg_arquivo_servidor | Configuracao | Servidores de arquivo. |
| cfg_download_token | Movimento | Tokens de download. |
| cfg_download_tracker | Auditoria | Rastreamento de download. |
| cfg_tarefa_background | Operacao | Tarefas em segundo plano. |
| cfg_tarefa_log | Auditoria | Logs de tarefas. |
| cfg_excecao | Auditoria | Logs de erro. |
| cfg_versao_aplicada | Auditoria | Versionamento de banco/configuracao. |
| cfg_plugin | Configuracao | Plugins/componentes opcionais. |
| cfg_tema | Configuracao | Temas instalados. |
| cfg_instalacao | Operacao | Instalacao inicial e upgrades. |
| cfg_historico | Auditoria | Historico antes/depois de parametros. |

### 8.2 Inventario de tabelas e estruturas preservadas do material

| Estrutura | Uso funcional no Epros | Tratamento |
|---|---|---|
| settings | Parametros amplos do tenant. | Consolidar em cfg_parametro e entidades especificas. |
| settings2 | Complementos de faturas/numeracao. | Consolidar em cfg_parametro. |
| invoice_statuses | Status de faturas. | cfg_status_auxiliar. |
| lead_statuses | Status de leads. | cfg_status_auxiliar. |
| task_statuses | Status de tarefas. | cfg_status_auxiliar. |
| task_priorities | Prioridades de tarefas. | cfg_status_auxiliar. |
| ticket_statuses | Status de tickets. | Fronteira com atendimento. |
| taxrates | Taxas configuraveis. | Fronteira fiscal/financeira. |
| milestones | Marcos/categorias. | Fronteira projetos. |
| kbcategories | Categorias de base de conhecimento. | Fronteira conhecimento. |
| leads_sources | Fontes de leads. | Fronteira comercial. |
| customfields | Campos personalizados por entidade. | cfg_campo_personalizado. |
| file_folders | Pastas default. | Fronteira GED/storage. |
| email_templates | Templates de e-mail. | cfg_email_template. |
| email_log | Log de envio. | cfg_email_fila/auditoria. |
| email_queue | Fila de e-mail. | cfg_email_fila. |
| webmail_templates | Templates webmail. | Fronteira colaboracao. |
| subscription_plans | Planos de assinatura. | Fronteira Aplicativo/cobranca. |
| Users | Usuarios de plataforma. | Consumido por identidade. |
| Roles | Papeis. | Consumido por usuarios/papeis. |
| UserRoles | Vinculo usuario/papel. | Consumido por usuarios/papeis. |
| RolePermissions | Permissoes por papel. | Consumido por permissoes. |
| UserPermissions | Permissoes por usuario. | Consumido por permissoes. |
| UserPreferences | Preferencias. | cfg_preferencia_usuario. |
| Languages/language | Idiomas. | cfg_idioma. |
| language_key | Chaves de traducao. | cfg_traducao_chave. |
| language_content | Conteudo traduzido. | cfg_traducao_conteudo. |
| Exceptions | Logs de excecao. | cfg_excecao. |
| VersionInfo | Versoes aplicadas. | cfg_versao_aplicada. |
| background_task | Tarefas. | cfg_tarefa_background. |
| background_task_log | Log de tarefas. | cfg_tarefa_log. |
| banned_ips | Bloqueio de IP. | Politica de seguranca. |
| country_info | Pais/moeda. | Fronteira geografia/cadastros. |
| cross_site_action | Acao cruzada autorizada. | Integracao controlada. |
| download_page | Paginas de download. | Fronteira GED/download. |
| download_token | Token de download. | cfg_download_token. |
| download_tracker | Rastreamento de download. | cfg_download_tracker. |
| file | Metadados de arquivo. | Fronteira GED. |
| file_action | Fila de acao de arquivo. | Fronteira GED/storage. |
| file_folder | Pasta de arquivo. | Fronteira GED. |
| file_folder_share | Compartilhamento de pasta. | Fronteira GED. |
| file_report | Denuncia/reporte de arquivo. | Fronteira GED/compliance. |
| file_server | Servidor de arquivo. | cfg_arquivo_servidor. |
| file_server_status | Status de servidor. | cfg_status_auxiliar. |
| file_status | Status de arquivo. | cfg_status_auxiliar. |
| internal_notification | Notificacao interna. | Fronteira SOA/colaboracao. |
| login_failure | Falha de login. | Fronteira identidade. |
| login_success | Sucesso de login. | Fronteira identidade. |
| payment_log | Log de pagamento de monetizacao. | Fronteira cobranca SaaS. |
| plugin | Plugin/componente. | cfg_plugin. |
| plugin_filepreviewer_meta | Metadados de preview. | Fronteira GED. |
| plugin_filepreviewer_watermark | Marca d'agua de preview. | Fronteira GED. |
| premium_order | Pedido premium/monetizacao. | Fronteira cobranca SaaS. |
| remote_url_download_queue | Fila de download remoto. | Fronteira GED/storage. |
| sessions | Sessoes. | Fronteira identidade. |
| site_config | Chave/valor runtime. | cfg_parametro. |
| stats | Estatisticas. | Fronteira analytics. |
| theme | Tema. | cfg_tema. |
| user_level | Nivel de usuario/plano. | Fronteira usuarios/limites. |
| user_level_pricing | Preco por nivel. | Fronteira cobranca SaaS. |
| file_block_hash | Hash de bloco de arquivo. | Fronteira GED/integridade. |

## 9. Dicionario de dados implantavel

### 9.1 cfg_parametro

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| id | uuid | 36 | Sim | PK | Identificador. |
| tenant_id | uuid | 36 | Nao | FK tenant | Nulo apenas para parametro global Siser. |
| grupo_id | uuid | 36 | Nao | FK cfg_parametro_grupo | Agrupamento. |
| chave | string | 100 | Sim | Unica por escopo | Equivalente funcional de config_key/settings_*. |
| valor | texto/json | Nao informado no material | Nao | Campo protegido quando secreto | Valor configurado. |
| descricao | string | 255 | Nao | Campo simples | Descricao funcional. |
| tipo | string | 30 | Sim | Campo simples | Tipo de parametro. |
| valores_disponiveis | string/json | 255 | Nao | Campo simples | Lista de opcoes. |
| escopo | enum | Global; Tenant; Empresa; Usuario; Modulo | Sim | Campo simples | Define alcance. |
| secreto | boolean | true/false | Sim | Campo simples | Se deve proteger exibicao. |
| status | enum | Ativo; Inativo; Substituido | Sim | Estado | Controla uso. |
| criado_em | datetime | ISO 8601 | Sim | Auditoria | Criacao. |
| atualizado_em | datetime | ISO 8601 | Sim | Auditoria | Atualizacao. |

### 9.2 cfg_modulo_flag e cfg_status_auxiliar

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| id | uuid | 36 | Sim | PK | Identificador. |
| tenant_id | uuid | 36 | Sim | FK tenant | Isolamento. |
| modulo | string | Nao informado no material | Sim | Chave funcional | Modulo ao qual a flag/status pertence. |
| chave | string | Nao informado no material | Sim | Chave funcional | Ex.: settings_modules_*. |
| valor | enum | enabled; disabled | Sim | Campo simples | Para flags de modulo. |
| codigo | string | Nao informado no material | Sim | Chave funcional | Para status auxiliar. |
| nome | string | Nao informado no material | Sim | Campo simples | Nome exibido. |
| cor | string | Nao informado no material | Nao | Campo simples | Cor visual quando aplicavel. |
| ordem | inteiro | Nao informado no material | Nao | Campo simples | Reordenacao. |
| status | enum | Ativo; Inativo | Sim | Estado | Status do registro. |

### 9.3 cfg_email_configuracao, cfg_email_template e cfg_email_fila

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| id | uuid | 36 | Sim | PK | Identificador. |
| tenant_id | uuid | 36 | Sim | FK tenant | Isolamento. |
| metodo_envio | enum/texto | Nao informado no material | Sim | Campo simples | Metodo de envio. |
| smtp_host | string | Nao informado no material | Nao | Campo simples | Host SMTP. |
| smtp_porta | inteiro | 1..65535 | Nao | Campo simples | Porta SMTP. |
| smtp_requer_autenticacao | boolean | true/false | Nao | Campo simples | Conforme site_config. |
| smtp_usuario | string | Nao informado no material | Nao | Campo protegido | Usuario. |
| smtp_senha | segredo | Nao informado no material | Nao | Campo protegido | Nunca exibir integralmente. |
| remetente_email | string | Nao informado no material | Nao | Campo simples | Email padrao de envio. |
| template_chave | string | Nao informado no material | Sim | Chave funcional | Chave do template. |
| assunto | string | Nao informado no material | Nao | Campo simples | Assunto. |
| corpo | texto/html | Nao informado no material | Nao | Campo simples | Corpo do template. |
| idioma_id | uuid | 36 | Nao | FK cfg_idioma | Idioma. |
| fila_status | enum | Pendente; Enviado; Falha; Reprocessando | Nao | Estado | Status da fila. |
| erro | texto | Nao informado no material | Nao | Campo simples | Erro de envio. |

### 9.4 cfg_idioma, cfg_traducao_chave e cfg_traducao_conteudo

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| id | uuid | 36 | Sim | PK | Identificador. |
| language_id | string | 10 | Sim | Chave funcional | Ex.: en, pt-BR. |
| language_name | string | 50/255 | Sim | Campo simples | Nome do idioma. |
| is_locked | boolean | true/false | Nao | Campo simples | Bloqueio de edicao. |
| is_active | boolean | true/false | Sim | Estado | Idioma ativo. |
| flag | string | 20 | Nao | Campo simples | Bandeira/icone. |
| direction | enum | LTR; RTL | Nao | Campo simples | Direcao de leitura. |
| language_key | string | 255 | Sim | Chave funcional | Chave de traducao. |
| default_content | texto | Nao informado no material | Sim | Campo simples | Conteudo padrao. |
| is_admin_area | boolean | true/false | Nao | Campo simples | Area administrativa. |
| found_on_scan | boolean | true/false | Nao | Campo simples | Encontrada em varredura. |
| content | texto | Nao informado no material | Sim | Campo simples | Conteudo traduzido. |

### 9.5 cfg_upload_politica, cfg_arquivo_servidor e download

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| id | uuid | 36 | Sim | PK | Identificador. |
| tenant_id | uuid | 36 | Sim | FK tenant | Isolamento. |
| tipos_aceitos | texto/lista | Nao informado no material | Nao | Campo simples | Tipos aceitos. |
| tipos_bloqueados | texto/lista | Nao informado no material | Nao | Campo simples | Tipos bloqueados. |
| tamanho_maximo | bigint | bytes | Nao | Campo simples | Tamanho maximo. |
| servidor_padrao_id | uuid | 36 | Nao | FK cfg_arquivo_servidor | Servidor padrao. |
| server_label | string | 100 | Sim | Campo simples | Rotulo do servidor. |
| server_type | enum | remote; local; ftp; sftp; direct; amazon_s3 | Sim | Campo simples | Tipos preservados. |
| ip_address | string | 255 | Nao | Campo simples | Endereco. |
| ftp_port | inteiro | 1..65535 | Nao | Campo simples | Porta. |
| ftp_username | string | 50 | Nao | Campo protegido | Usuario. |
| ftp_password | segredo | 50 | Nao | Campo protegido | Senha protegida. |
| status_id | inteiro/uuid | Nao informado no material | Sim | FK status | Status. |
| storage_path | string | 255 | Nao | Campo simples | Caminho. |
| maximum_storage_bytes | bigint | bytes | Nao | Campo simples | Limite. |
| priority | inteiro | Nao informado no material | Nao | Campo simples | Prioridade. |
| token | string | 64 | Sim | Chave funcional | Token de download. |
| expiry | datetime | ISO 8601 | Sim | Campo simples | Expiracao. |
| download_speed | inteiro | Nao informado no material | Nao | Campo simples | Velocidade. |
| max_threads | inteiro | Nao informado no material | Nao | Campo simples | Threads. |

### 9.6 cfg_tarefa_background, cfg_tarefa_log, cfg_excecao e cfg_versao_aplicada

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| id | uuid | 36 | Sim | PK | Identificador. |
| task | string | 255 | Sim | Chave funcional | Nome da tarefa. |
| last_update | datetime | ISO 8601 | Nao | Auditoria | Ultima atualizacao. |
| status | enum | running; finished; not_run; started; failed; partial | Sim | Estado | Estado da tarefa/log. |
| task_id | uuid | 36 | Sim | FK cfg_tarefa_background | Tarefa associada. |
| start_time | datetime | ISO 8601 | Sim | Auditoria | Inicio. |
| end_time | datetime | ISO 8601 | Nao | Auditoria | Fim. |
| server_name | string | 255 | Nao | Campo simples | Servidor executor. |
| log_message | texto | Nao informado no material | Nao | Campo simples | Mensagem. |
| guid | uuid | 36 | Nao | Chave funcional | Identificador de excecao. |
| application_name | string | 50 | Nao | Campo simples | Aplicacao. |
| machine_name | string | 50 | Nao | Campo simples | Maquina. |
| creation_date | datetime | ISO 8601 | Sim | Auditoria | Data da excecao. |
| type | string | 100 | Nao | Campo simples | Tipo do erro. |
| host | string | Nao informado no material | Nao | Campo simples | Host. |
| url | string | Nao informado no material | Nao | Campo simples | URL. |
| http_method | string | Nao informado no material | Nao | Campo simples | Metodo. |
| ip_address | string | 45 | Nao | Auditoria | IP. |
| message | texto | Nao informado no material | Nao | Campo simples | Mensagem. |
| detail | texto | Nao informado no material | Nao | Campo protegido | Detalhe tecnico. |
| status_code | inteiro | Nao informado no material | Nao | Campo simples | Codigo HTTP. |
| error_hash | inteiro/string | Nao informado no material | Nao | Chave funcional | Agrupamento. |
| version | bigint | Nao informado no material | Sim | PK/Chave | Versao aplicada. |
| applied_on | datetime | ISO 8601 | Nao | Auditoria | Data de aplicacao. |
| description | string | 1024 | Nao | Campo simples | Descricao. |

### 9.7 cfg_preferencia_usuario, cfg_plugin e cfg_tema

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| id | uuid | 36 | Sim | PK | Identificador. |
| user_id | uuid | 36 | Sim | FK usuario | Usuario. |
| preference_type | string | 100 | Sim | Chave composta | Tipo. |
| name | string | 200 | Sim | Chave composta | Nome. |
| value | texto | Nao informado no material | Nao | Campo simples | Valor. |
| plugin_name | string | 150 | Nao | Campo simples | Nome do componente. |
| folder_name | string | 100 | Nao | Campo simples | Pasta tecnica interna. |
| plugin_description | string | 255 | Nao | Campo simples | Descricao. |
| is_installed | boolean | true/false | Sim | Estado | Instalado. |
| plugin_enabled | boolean | true/false | Sim | Estado | Habilitado. |
| load_order | inteiro | Nao informado no material | Nao | Campo simples | Ordem de carga. |
| plugin_settings | texto/json | Nao informado no material | Nao | Campo protegido | Configuracao. |
| theme_name | string | 150 | Nao | Campo simples | Nome do tema. |
| theme_description | string | 255 | Nao | Campo simples | Descricao. |
| author_name | string | 255 | Nao | Campo simples | Autor. |
| theme_settings | texto/json | Nao informado no material | Nao | Campo protegido | Configuracao do tema. |

## 10. APIs funcionais

| API | Metodo | Finalidade | Observacao |
|---|---|---|---|
| /configuracoes | GET | Listar parametros por grupo, modulo e escopo. | Contrato final nao informado no material. |
| /configuracoes/{id} | GET | Consultar parametro. | Deve mascarar segredos. |
| /configuracoes | POST | Criar parametro autorizado. | Requer permissao administrativa. |
| /configuracoes/{id} | PUT | Atualizar parametro. | Deve gerar historico. |
| /configuracoes/modulos | GET | Listar flags de modulos. | Leitura runtime. |
| /configuracoes/modulos/{modulo} | PUT | Atualizar flag de modulo. | Escrita controlada. |
| /configuracoes/status | GET/POST | CRUD de status auxiliares. | Por modulo dono. |
| /configuracoes/email/teste | POST | Testar envio de e-mail. | Usa configuracao ativa. |
| /configuracoes/campos-personalizados | GET/POST | CRUD de campos customizados. | Por entidade. |
| /configuracoes/upload/temporario | POST | Upload temporario. | Um arquivo por requisicao quando aplicavel. |
| /configuracoes/idiomas | GET/POST | CRUD de idiomas. | Requer permissao. |
| /configuracoes/traducoes | GET/PUT | Listar e atualizar traducoes. | Deve recarregar registro de textos. |
| /configuracoes/tarefas | GET | Monitorar tarefas. | Somente operacao autorizada. |
| /configuracoes/excecoes | GET | Consultar erros. | Deve mascarar detalhe sensivel. |

## 11. Telas

| Tela | Conteudo |
|---|---|
| Hub de configuracao | Cards/secoes de parametros com menu lateral. |
| Geral | Fuso, data/hora, idioma, paginacao, nome e modo operacional. |
| Modulos | Flags enabled/disabled e origem da permissao. |
| Empresa/branding | Dados visuais, tema, logos e identidade visual. |
| Moeda | Codigo, simbolo e formato monetario. |
| Clientes/leads/tarefas | Status, prioridades, fontes e listas auxiliares. |
| Projetos | Parametros gerais, permissoes de cliente/equipe e automacoes. |
| Faturas/estimativas/propostas/contratos | Numeracao, status, automacoes e campos de exibicao. |
| E-mail | Geral, SMTP, teste, templates, fila. |
| Captcha/seguranca | Captcha, politicas de senha, bloqueios e manutencao. |
| Campos personalizados | Campos por entidade, tipo, ordem e exibicao. |
| Arquivos | Tipos, tamanho, pastas, storage, download e servidores. |
| Idiomas | Idiomas, chaves, traducoes, importacao/exportacao. |
| Tarefas | Tarefas, logs, ultimas execucoes e falhas. |
| Plugins e temas | Componentes instalados, habilitados e ordem. |
| Sistema | Informacoes tecnicas, versoes, logs e suporte operacional. |

## 12. Criterios de aceite

| ID | Criterio |
|---|---|
| CA-CFG-001 | Usuario sem permissao administrativa nao altera parametro. |
| CA-CFG-002 | Alteracao de parametro critico gera historico antes/depois. |
| CA-CFG-003 | Segredo gravado nao e exibido integralmente em consulta posterior. |
| CA-CFG-004 | Flag de modulo desabilitado oculta menu e bloqueia uso conforme politica. |
| CA-CFG-005 | Teste de SMTP retorna sucesso/falha operacional. |
| CA-CFG-006 | Campo personalizado aparece conforme entidade, ordem e exibicao configuradas. |
| CA-CFG-007 | Upload temporario retorna referencia e gera miniatura de imagem quando aplicavel. |
| CA-CFG-008 | Conflito de versao impede sobrescrita de registro. |
| CA-CFG-009 | Idioma e traducao atualizados recarregam textos. |
| CA-CFG-010 | Tarefa em segundo plano registra inicio, fim, status e mensagem. |

## 13. Testes funcionais

| ID | Cenario | Resultado esperado |
|---|---|---|
| CT-CFG-001 | Abrir hub de configuracao. | Menu e secoes carregam conforme permissao. |
| CT-CFG-002 | Alterar flag de modulo. | Valor persistido e cache invalidado. |
| CT-CFG-003 | Testar SMTP. | Resultado registrado. |
| CT-CFG-004 | Criar campo personalizado. | Campo vinculado a entidade. |
| CT-CFG-005 | Acessar secao por link direto. | Secao correta ou acesso negado. |
| CT-CFG-006 | Executar boot/versionamento. | Versoes aplicadas registradas. |
| CT-CFG-007 | Fazer upload temporario. | Arquivo temporario criado. |
| CT-CFG-008 | Baixar arquivo sem permissao. | Acesso negado. |
| CT-CFG-009 | Atualizar registro com versao vencida. | Conflito funcional. |
| CT-CFG-010 | Editar traducao sem permissao. | Acesso negado. |
| CT-CFG-011 | Alterar chave de configuracao por editor chave/valor. | Valor atualizado e historico gerado. |
| CT-CFG-012 | Executar tarefa em segundo plano. | Log de inicio/fim registrado. |

## 14. Notas de rodape

1. As entidades canonicas `cfg_*` foram estruturadas para organizar as tabelas e chaves dispersas do material em um desenho implantavel para o Epros; as tabelas e campos preservados aparecem no inventario e no dicionario, e decisoes de fronteira ficam na MC.
