# EF 3 Plataforma Compartilhada — API Gateway e OpenAPI V1

## 1. Identificacao

| Item | Valor |
|---|---|
| Sistema | Epros |
| Empresa | Siser |
| Modulo | Plataforma Compartilhada |
| Submodulo | API Gateway e OpenAPI |
| Versao | V1 |
| Status | Especificacao funcional para validacao humana |
| Data | 2026-06-06 |

## 2. Objetivo funcional

O submodulo API Gateway e OpenAPI governa a entrada de APIs do Epros. Ele centraliza autenticacao, contexto de tenant/empresa, autorizacao, catalogo de rotas, contratos OpenAPI, versionamento, compatibilidade controlada, rate limit, auditoria, logs de requisicao, padronizacao de erros, seguranca de integracoes, endpoints mobile e publicacao de eventos tecnicos.

O submodulo nao e dono das regras de venda, estoque, financeiro, fiscal, compras ou cadastros. Ele orquestra a exposicao segura dos contratos e encaminha cada chamada para o modulo responsavel, preservando isolamento, rastreabilidade e padrao unico de API.

## 3. Escopo

### 3.1 Dentro do escopo

| Capacidade | Descricao |
|---|---|
| Autenticacao | Emitir, validar, renovar e revogar tokens de acesso. |
| Contexto tenant/empresa | Resolver tenant, empresa ativa, usuario, perfil e escopo de operacao. |
| Autorizacao | Aplicar permissao por rota, metodo, modulo, perfil, usuario, cliente de API e escopo. |
| Catalogo OpenAPI | Publicar contratos versionados por modulo, rota, ambiente e tenant quando aplicavel. |
| Gateway de rotas | Registrar, ativar, desativar e rotear endpoints para os modulos donos. |
| Versionamento | Controlar versao de API, compatibilidade, deprecacao e data limite de suporte. |
| Rate limit | Limitar chamadas por tenant, cliente, usuario, rota, janela e plano contratado. |
| Chaves de API | Gerenciar clientes de API, segredos, escopos, rotacao e expiracao. |
| Auditoria e logs | Registrar requisicao, resposta resumida, erro, latencia, usuario, tenant, rota e IP protegido. |
| Padrao de erros | Padronizar respostas de autenticacao, autorizacao, validacao, conflito e falha interna. |
| Batch e sincronizacao | Suportar envio em lote, sincronizacao mobile/PDV e retorno por item. |
| Compatibilidade controlada | Manter adaptadores por versao apenas quando houver contrato formal e whitelist. |
| Eventos tecnicos | Publicar eventos de gateway, falha, abuso, deprecacao e contrato publicado. |

### 3.2 Fora do escopo

| Tema | Tratamento |
|---|---|
| Regra de negocio transacional | Fica no modulo dono da operacao. |
| Criacao de cadastros mestres | Gateway apenas encaminha ou expõe contrato. |
| Calculo fiscal, financeiro ou estoque | Pertence aos modulos correspondentes. |
| Interface final de operacao de venda, compra ou PDV | Pertence aos modulos consumidores. |
| Execucao de filas de negocio | Gateway registra/encaminha; processamento pertence ao modulo tecnico ou dominio dono. |

## 4. Dependencias e consumidores

### 4.1 Dependencias

| Dependencia | Uso |
|---|---|
| Identidade e Contexto Tenant | Usuario, tenant, empresa ativa, perfil e claims. |
| Permissoes de Menu | Autorizacao por acao, rota e modulo. |
| Assinatura e Limites | Limites de API, quantidade de chamadas, modulos contratados e clientes externos. |
| Compliance e Privacidade | Log, retencao, mascaramento, consentimento e dados sensiveis. |
| Workflow | Aprovacao de publicacao/deprecacao de contratos criticos. |
| Integracoes e Conectores | Clientes externos, webhooks, conectores e adaptadores. |
| Gestao Eletronica de Documentos | Armazenamento de contratos publicados, exportacoes de especificacao e evidencias. |

### 4.2 Consumidores

| Consumidor | Uso |
|---|---|
| Aplicativo web | Autenticacao, chamadas internas e catalogo de APIs. |
| Aplicativos moveis | Login, sincronizacao, batch, consulta e upload offline. |
| PDV | Sincronizacao de produtos, clientes, vendas, caixa, documentos e PDF. |
| Parceiros externos | APIs publicadas com chave, escopo e limite. |
| Modulos internos | Contratos REST, eventos e comunicacao entre dominios. |
| Operacao Siser | Observabilidade, erros, uso por tenant e governanca de contratos. |

## 5. Principios funcionais

| Codigo | Regra |
|---|---|
| REG-API-001 | Toda chamada deve ter contexto de tenant resolvido antes de acessar dados tenantizados. |
| REG-API-002 | TenantId informado pelo cliente nunca pode ampliar escopo alem do token/chave autorizada. |
| REG-API-003 | Toda rota deve declarar dono funcional, modulo, metodo, versao, permissao e politica de acesso. |
| REG-API-004 | Nenhuma rota de dados de negocio deve ficar publica por omissao. |
| REG-API-005 | Rotas publicas devem ser explicitamente marcadas, justificadas e auditadas. |
| REG-API-006 | Contrato OpenAPI publicado deve refletir a versao ativa da rota. |
| REG-API-007 | Rota depreciada deve informar substituta e data final de suporte. |
| REG-API-008 | API externa deve usar chave, token ou autenticacao equivalente com escopo limitado. |
| REG-API-009 | Respostas de erro devem seguir formato padrao do Epros. |
| REG-API-010 | Segredos, tokens e chaves nunca devem ser retornados em claro depois da criacao. |
| REG-API-011 | Compatibilidade com contratos antigos so pode existir por adaptador explicitamente permitido. |
| REG-API-012 | Chamadas em lote devem retornar sucesso/falha por item e resumo do lote. |

## 6. Regras funcionais detalhadas

### 6.1 Autenticacao

| Codigo | Regra |
|---|---|
| REG-API-013 | O Epros deve oferecer endpoint de emissao de token para usuario autorizado. |
| REG-API-014 | Login deve validar credencial, status do usuario, tenant, empresa ativa e permissoes minimas. |
| REG-API-015 | Token invalido deve retornar erro padronizado de autenticacao. |
| REG-API-016 | Usuario sem acesso a empresa deve receber erro padronizado de autorizacao. |
| REG-API-017 | E-mail invalido deve ser tratado como erro de validacao. |
| REG-API-018 | Credencial invalida deve retornar mensagem padronizada sem revelar qual campo falhou. |
| REG-API-019 | Recuperacao de senha deve retornar resposta segura, sem expor existencia de usuario alem da politica definida. |
| REG-API-020 | Acao nao autorizada deve retornar HTTP 403. |
| REG-API-021 | Token deve conter identificador de usuario, tenant, empresa ativa, perfil, escopos e expiracao. |
| REG-API-022 | Refresh token, quando usado, deve possuir rotacao e revogacao. |
| REG-API-023 | Logout deve revogar ou invalidar a sessao conforme politica de token. |

### 6.2 Tenant, empresa e escopo

| Codigo | Regra |
|---|---|
| REG-API-024 | Toda requisicao tenantizada deve resolver TenantId a partir do token, chave ou sessao segura. |
| REG-API-025 | EmpresaId deve ser obrigatorio quando a rota operar dentro de empresa/filial especifica. |
| REG-API-026 | Login por empresa deve exigir EmpresaId valido e permitido ao usuario. |
| REG-API-027 | Criacao de tenant deve exigir nome, documento e status ativo/inativo. |
| REG-API-028 | Atualizacao de tenant deve exigir TenantId, nome, documento e status. |
| REG-API-029 | Contato e telefone do tenant sao opcionais. |
| REG-API-030 | Tenant inativo nao pode emitir token nem consumir rotas operacionais, salvo rotas administrativas autorizadas. |
| REG-API-031 | Troca de empresa ativa deve gerar novo contexto ou novo token. |
| REG-API-032 | Logs devem registrar tenant e empresa resolvidos, nunca apenas o valor recebido no payload. |

### 6.3 Catalogo de rotas

| Codigo | Regra |
|---|---|
| REG-API-033 | Toda rota deve ser registrada no catalogo antes de ficar ativa. |
| REG-API-034 | Registro de rota deve conter metodo HTTP, caminho, modulo dono, recurso, versao, status, permissao, autenticacao e politica de rate limit. |
| REG-API-035 | Rota ativa sem contrato OpenAPI deve ser bloqueada para publicacao externa. |
| REG-API-036 | Rotas internas podem ser marcadas como privadas e omitidas do catalogo publico. |
| REG-API-037 | Rota de helper tecnico deve ser isolada do contrato funcional de negocio. |
| REG-API-038 | Rotas duplicadas para o mesmo recurso devem ser consolidadas ou justificadas como compatibilidade. |
| REG-API-039 | Rota que apenas sincroniza dados deve declarar direcao: leitura, envio, lote ou bidirecional. |
| REG-API-040 | Rota que gera PDF, documento ou arquivo deve declarar tipo de conteudo e permissao de download. |

### 6.4 OpenAPI e versionamento

| Codigo | Regra |
|---|---|
| REG-API-041 | O Epros deve publicar catalogo OpenAPI unico por ambiente. |
| REG-API-042 | Cada modulo pode ter grupo proprio dentro do catalogo. |
| REG-API-043 | Cada versao de API deve possuir identificador, data de publicacao e status. |
| REG-API-044 | Header de versao pode ser usado para selecionar versao futura quando a rota suportar. |
| REG-API-045 | Mudanca incompatível exige nova versao. |
| REG-API-046 | Mudanca compativel pode atualizar versao menor do contrato. |
| REG-API-047 | Contrato depreciado deve informar data final de suporte. |
| REG-API-048 | Contrato removido deve permanecer documentado no historico. |
| REG-API-049 | Adaptadores de compatibilidade devem usar whitelist de operacoes permitidas. |
| REG-API-050 | Despacho dinamico por nome de metodo nao pode expor operacao sem registro no catalogo. |

### 6.5 Chaves de API, clientes e rate limit

| Codigo | Regra |
|---|---|
| REG-API-051 | Cliente de API deve possuir tenant, nome, ambiente, escopos, status, responsavel e data de expiracao opcional. |
| REG-API-052 | Segredo de cliente deve ser mostrado apenas na criacao e armazenado como hash/segredo protegido. |
| REG-API-053 | Chave de API deve poder ser revogada manualmente. |
| REG-API-054 | Chave de API deve poder ser rotacionada. |
| REG-API-055 | Rate limit deve poder ser definido por tenant, cliente, usuario, rota e janela de tempo. |
| REG-API-056 | Excesso de limite deve retornar HTTP 429 com informacao padronizada de retentativa quando aplicavel. |
| REG-API-057 | Rotas criticas podem ter limite mais restritivo. |
| REG-API-058 | Plano contratado pode reduzir ou ampliar limite de API. |

### 6.6 Batch, mobile e sincronizacao

| Codigo | Regra |
|---|---|
| REG-API-059 | Endpoints em lote devem aceitar lista de itens e processar em transacao conforme politica do endpoint. |
| REG-API-060 | Retorno de lote deve conter identificador local, identificador gerado, sucesso, mensagem e erro por item. |
| REG-API-061 | Lote deve suportar rollback total ou parcial conforme contrato da rota. |
| REG-API-062 | Sincronizacao mobile deve diferenciar leitura de catalogo, envio de dados e confirmacao de recebimento. |
| REG-API-063 | Venda ou operacao enviada por mobile deve usar empresa do contexto autorizado, nao apenas empresa informada no payload. |
| REG-API-064 | Inventario ou movimento enviado por integracao deve ser roteado ao modulo dono para validar estoque. |
| REG-API-065 | Cliente/fornecedor enviado por integracao deve ser roteado ao cadastro mestre para validar pessoa. |
| REG-API-066 | Chamada de faturamento deve ser roteada ao modulo fiscal/comercial dono. |
| REG-API-067 | PDF ou documento baixado pela API deve exigir permissao e log de acesso. |

### 6.7 Padrao de erro e resposta

| Codigo | Regra |
|---|---|
| REG-API-068 | Resposta de erro deve conter codigo, mensagem, detalhe opcional, correlationId e timestamp. |
| REG-API-069 | Erro de validacao deve retornar HTTP 400 ou 422 conforme padrao definido. |
| REG-API-070 | Erro de autenticacao deve retornar HTTP 401. |
| REG-API-071 | Erro de autorizacao deve retornar HTTP 403. |
| REG-API-072 | Recurso inexistente deve retornar HTTP 404. |
| REG-API-073 | Conflito funcional deve retornar HTTP 409. |
| REG-API-074 | Falha interna deve retornar mensagem segura sem detalhe tecnico sensivel. |
| REG-API-075 | Erros de lote devem informar item, codigo e mensagem. |
| REG-API-076 | Todas as respostas devem carregar correlationId. |

### 6.8 Auditoria, observabilidade e seguranca

| Codigo | Regra |
|---|---|
| REG-API-077 | Toda requisicao deve gerar log tecnico com correlationId. |
| REG-API-078 | Log deve registrar tenant, empresa, usuario, cliente de API, rota, metodo, status HTTP, latencia e IP protegido. |
| REG-API-079 | Payload sensivel deve ser mascarado ou omitido no log. |
| REG-API-080 | Download de documento, emissao de token, falha de autenticacao, excesso de limite e erro 5xx devem gerar evento de auditoria. |
| REG-API-081 | Endpoint publico deve ter monitoramento especial. |
| REG-API-082 | Rota sem autenticacao deve ser excecao documentada, aprovada e monitorada. |
| REG-API-083 | Gateway deve bloquear rotas de scaffold, debug, introspeccao ou helper nao aprovadas. |
| REG-API-084 | Gateway deve manter lista de operacoes indisponiveis por inconsistência de contrato. |
| REG-API-085 | Logs devem respeitar politica de retencao e privacidade. |

### 6.9 Eventos e integracoes

| Codigo | Regra |
|---|---|
| REG-API-086 | Eventos assincronos devem ser publicados apos commit do modulo produtor. |
| REG-API-087 | Evento deve declarar produtor, consumidor esperado, versao e schema. |
| REG-API-088 | Gateway deve expor catalogo de eventos quando o contrato for publico ou integravel. |
| REG-API-089 | Evento de documento fiscal autorizado deve poder alimentar financeiro e estoque conforme contrato. |
| REG-API-090 | Evento de compra confirmada deve poder alimentar contas a pagar e estoque conforme contrato. |
| REG-API-091 | Evento de venda faturada deve poder alimentar contas a receber e fiscal conforme contrato. |
| REG-API-092 | Falha de entrega de evento deve gerar log e retentativa conforme politica do barramento. |

## 7. Estados

| Entidade | Estados |
|---|---|
| Rota de API | Rascunho; EmAnalise; Ativa; Depreciada; Inativa; Encerrada |
| Versao de contrato | Rascunho; Publicada; Depreciada; Encerrada |
| Cliente de API | Ativo; Suspenso; Revogado; Expirado |
| Chave de API | Ativa; Revogada; Expirada; Rotacionada |
| Politica de rate limit | Rascunho; Ativa; Inativa |
| Execucao de lote | Recebida; EmProcessamento; Concluida; ConcluidaComErros; Falha; Cancelada |
| Publicacao OpenAPI | Gerada; Publicada; Falha; Substituida |

## 8. Fluxos funcionais

### 8.1 Emissao de token

1. Cliente envia credenciais.
2. O Epros valida usuario, senha, tenant e empresa.
3. O Epros valida status e permissoes minimas.
4. O Epros emite token com tenant, empresa, usuario, perfil, escopos e expiracao.
5. O Epros registra auditoria de login.

### 8.2 Chamada autenticada

1. Cliente chama rota com token ou chave valida.
2. Gateway resolve tenant, empresa, usuario/cliente e correlationId.
3. Gateway valida rota, versao, permissao, escopo e rate limit.
4. Gateway encaminha para o modulo dono.
5. Gateway registra status, latencia e resumo seguro.
6. Gateway retorna resposta padronizada.

### 8.3 Publicacao de contrato OpenAPI

1. Responsavel registra rota ou altera contrato.
2. O Epros valida dono funcional, permissao, schema e versao.
3. Contrato fica em Rascunho ou EmAnalise.
4. Aprovador publica a versao.
5. Catalogo OpenAPI e atualizado por ambiente.
6. Evento de contrato publicado e emitido.

### 8.4 Chamada em lote

1. Cliente envia lote com itens e identificadores locais.
2. Gateway valida autenticacao, rota, limite e tamanho do lote.
3. Modulo dono processa conforme contrato de transacao.
4. Gateway retorna resumo do lote e resultado por item.
5. Falhas ficam auditadas com correlationId.

### 8.5 Deprecacao de API

1. Responsavel marca rota/versao como depreciada.
2. Informa rota substituta, motivo e data final.
3. O Epros registra comunicacao e historico.
4. Chamadas passam a receber cabecalho de aviso quando aplicavel.
5. Na data final, rota e encerrada ou bloqueada.

## 9. Telas e experiencia operacional

| ID | Tela | Funcao |
|---|---|---|
| TEL-API-001 | Catalogo de APIs | Lista rotas, metodos, versoes, status, dono, permissao e ambiente. |
| TEL-API-002 | Detalhe de rota | Mostra contrato, parametros, schemas, rate limit, logs e historico. |
| TEL-API-003 | Clientes de API | Cria cliente, escopos, chaves, rotacao e revogacao. |
| TEL-API-004 | Publicacao OpenAPI | Gera, valida, aprova e publica especificacoes. |
| TEL-API-005 | Politicas de acesso | Configura permissao, escopo, tenant, empresa e rota publica. |
| TEL-API-006 | Rate limits | Configura limites por plano, tenant, cliente, usuario e rota. |
| TEL-API-007 | Logs e auditoria | Consulta requisicoes, falhas, excesso de limite e downloads. |
| TEL-API-008 | Compatibilidade | Gerencia adaptadores, versoes antigas, whitelist e deprecacao. |
| TEL-API-009 | Lotes e sincronizacao | Acompanha lotes mobile/PDV, erros por item e retentativas. |

## 10. APIs funcionais do gateway

**Base administrativa:** `/api/v1/plataforma/api-gateway`

| Metodo | Rota | Funcao |
|---|---|---|
| POST | `/auth/token` | Emite token. |
| POST | `/auth/refresh` | Renova token quando habilitado. |
| POST | `/auth/logout` | Revoga sessao/token. |
| POST | `/auth/recuperar-senha` | Inicia recuperacao de senha. |
| GET | `/rotas` | Lista rotas catalogadas. |
| POST | `/rotas` | Cria rota no catalogo. |
| PUT | `/rotas/{id}` | Atualiza rota. |
| POST | `/rotas/{id}/publicar` | Publica rota. |
| POST | `/rotas/{id}/depreciar` | Deprecia rota. |
| GET | `/openapi` | Lista contratos publicados. |
| POST | `/openapi/gerar` | Gera especificacao OpenAPI. |
| POST | `/openapi/{id}/publicar` | Publica especificacao. |
| GET | `/clientes-api` | Lista clientes de API. |
| POST | `/clientes-api` | Cria cliente de API. |
| POST | `/clientes-api/{id}/rotacionar-chave` | Rotaciona chave. |
| POST | `/clientes-api/{id}/revogar` | Revoga cliente ou chave. |
| GET | `/rate-limits` | Lista politicas de limite. |
| POST | `/rate-limits` | Cria politica de limite. |
| GET | `/logs` | Consulta logs tecnicos. |
| GET | `/auditoria` | Consulta eventos auditaveis. |
| GET | `/lotes/{id}` | Consulta execucao de lote. |

## 11. Modelo de dados funcional e implantavel

### 11.1 Visao geral

| Entidade | Papel | Cardinalidade principal |
|---|---|---|
| api_tenant_contexto | Contexto de tenant/empresa | Tenant 1:N |
| api_rota | Catalogo de rotas | Modulo 1:N |
| api_rota_versao | Versoes da rota | Rota 1:N |
| api_openapi_documento | Documento OpenAPI publicado | Ambiente 1:N |
| api_cliente | Cliente de API externo/interno | Tenant 1:N |
| api_cliente_chave | Chaves e segredos | Cliente 1:N |
| api_escopo | Escopos funcionais | Global 1:N |
| api_cliente_escopo | Escopos por cliente | Cliente N:N |
| api_politica_rate_limit | Politicas de limite | Rota/Cliente/Tenant 1:N |
| api_log_requisicao | Log tecnico de chamada | Tenant 1:N |
| api_evento_auditoria | Auditoria de evento critico | Tenant 1:N |
| api_erro_catalogo | Catalogo padronizado de erros | Global 1:N |
| api_lote | Execucao de lote | Tenant 1:N |
| api_lote_item | Resultado por item | Lote 1:N |
| api_adaptador_compatibilidade | Adaptador por contrato/versao | Rota/versao 1:N |
| api_evento_contrato | Catalogo de eventos assincronos | Produtor 1:N |
| api_depreciacao | Controle de deprecacao | Rota/versao 1:N |

### 11.2 Constraints e indices minimos

| Entidade | Constraint/indice |
|---|---|
| api_rota | Unico por Metodo + Caminho + Versao + Ambiente. |
| api_rota_versao | Unico por RotaId + Versao. |
| api_openapi_documento | Unico por Ambiente + VersaoCatalogo + StatusPublicado. |
| api_cliente | Unico por TenantId + Nome + Ambiente. |
| api_cliente_chave | Indice por ClienteId, Status e ExpiraEm. |
| api_log_requisicao | Indice por TenantId, CorrelationId, RotaId, DataHora e StatusHttp. |
| api_politica_rate_limit | Indice por TenantId, ClienteId, UsuarioId, RotaId e Janela. |
| api_lote | Indice por TenantId, RotaId, Status e DataRecebimento. |
| api_lote_item | Indice por LoteId, IdentificadorLocal e Status. |
| api_evento_auditoria | Indice por TenantId, TipoEvento, UsuarioId, DataEvento. |

## 12. Dicionario de dados implantavel

### 12.1 api_tenant_contexto

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| Id | uuid | uuid | Sim | PK | Gerado pelo Epros. |
| TenantId | texto | 200 | Sim | Indice | Tenant resolvido. |
| EmpresaId | uuid/long | Nao informado no material | Condicional | FK empresa | Obrigatorio quando a rota exigir empresa ativa. |
| UsuarioId | uuid | uuid | Nao | FK usuario | Nulo para cliente tecnico. |
| ClienteApiId | uuid | uuid | Nao | FK api_cliente | Nulo para usuario humano. |
| PerfilId | uuid | uuid | Nao | FK perfil |  |
| EscoposJson | json | Nao informado no material | Sim |  | Escopos efetivos. |
| Origem | enum | Web/Mobile/PDV/Externo/Interno | Sim |  |  |
| CriadoEm | data/hora | ISO 8601 | Sim |  |  |
| ExpiraEm | data/hora | ISO 8601 | Nao |  |  |

### 12.2 api_rota e api_rota_versao

| Entidade | Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|---|
| api_rota | Id | uuid | uuid | Sim | PK |  |
| api_rota | Codigo | texto | Nao informado no material | Sim | Unico funcional |  |
| api_rota | ModuloDono | texto | Nao informado no material | Sim |  | Modulo responsavel pela regra. |
| api_rota | Recurso | texto | Nao informado no material | Sim |  |  |
| api_rota | MetodoHttp | enum | GET/POST/PUT/PATCH/DELETE | Sim |  |  |
| api_rota | Caminho | texto | Nao informado no material | Sim |  | Caminho publicado. |
| api_rota | TipoAcesso | enum | Publica/Autenticada/Interna/Parceiro | Sim |  | Publica exige justificativa. |
| api_rota | ExigeTenant | booleano | true/false | Sim |  |  |
| api_rota | ExigeEmpresa | booleano | true/false | Sim |  |  |
| api_rota | ExigePermissao | booleano | true/false | Sim |  |  |
| api_rota | PermissaoCodigo | texto | Nao informado no material | Nao |  |  |
| api_rota | Status | enum | Rascunho/EmAnalise/Ativa/Depreciada/Inativa/Encerrada | Sim | Indice |  |
| api_rota | Sensivel | booleano | true/false | Sim |  | Log mascarado. |
| api_rota_versao | Id | uuid | uuid | Sim | PK |  |
| api_rota_versao | RotaId | uuid | uuid | Sim | FK api_rota |  |
| api_rota_versao | Versao | texto | Nao informado no material | Sim | Unico por rota | Ex.: v1. |
| api_rota_versao | Compatibilidade | enum | Compativel/Incompativel/Adaptador | Sim |  |  |
| api_rota_versao | SchemaRequestJson | json | Nao informado no material | Nao |  |  |
| api_rota_versao | SchemaResponseJson | json | Nao informado no material | Nao |  |  |
| api_rota_versao | PublicadaEm | data/hora | ISO 8601 | Nao |  |  |
| api_rota_versao | DepreciadaEm | data/hora | ISO 8601 | Nao |  |  |
| api_rota_versao | SuporteAte | data | ISO 8601 | Nao |  |  |
| api_rota_versao | RotaSubstitutaId | uuid | uuid | Nao | FK api_rota |  |

### 12.3 api_openapi_documento

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| Id | uuid | uuid | Sim | PK |  |
| Ambiente | enum | Desenvolvimento/Homologacao/Producao | Sim | Indice |  |
| VersaoCatalogo | texto | Nao informado no material | Sim |  |  |
| Titulo | texto | Nao informado no material | Sim |  |  |
| ConteudoJson | json | OpenAPI 3.1 | Sim |  | Contrato publicado. |
| HashConteudo | texto/hash | Nao informado no material | Sim |  | Integridade. |
| Status | enum | Gerada/Publicada/Falha/Substituida | Sim |  |  |
| GeradoPorUsuarioId | uuid | uuid | Sim | FK usuario |  |
| GeradoEm | data/hora | ISO 8601 | Sim |  |  |
| PublicadoPorUsuarioId | uuid | uuid | Nao | FK usuario |  |
| PublicadoEm | data/hora | ISO 8601 | Nao |  |  |
| ArquivoId | uuid | uuid | Nao | FK GED | Quando armazenado como artefato. |

### 12.4 api_cliente, chaves e escopos

| Entidade | Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|---|
| api_cliente | Id | uuid | uuid | Sim | PK |  |
| api_cliente | TenantId | texto | 200 | Sim | Indice |  |
| api_cliente | Nome | texto | Nao informado no material | Sim |  |  |
| api_cliente | Ambiente | enum | Homologacao/Producao | Sim |  |  |
| api_cliente | ResponsavelPessoaId | uuid | uuid | Nao | FK pessoa |  |
| api_cliente | Status | enum | Ativo/Suspenso/Revogado/Expirado | Sim |  |  |
| api_cliente | ExpiraEm | data/hora | ISO 8601 | Nao |  |  |
| api_cliente_chave | Id | uuid | uuid | Sim | PK |  |
| api_cliente_chave | ClienteApiId | uuid | uuid | Sim | FK api_cliente |  |
| api_cliente_chave | ChavePublica | texto | Nao informado no material | Sim | Unico | Identificador nao secreto. |
| api_cliente_chave | SegredoHash | texto/hash | Nao informado no material | Sim |  | Segredo protegido. |
| api_cliente_chave | Status | enum | Ativa/Revogada/Expirada/Rotacionada | Sim |  |  |
| api_cliente_chave | CriadaEm | data/hora | ISO 8601 | Sim |  |  |
| api_cliente_chave | ExpiraEm | data/hora | ISO 8601 | Nao |  |  |
| api_cliente_chave | RevogadaEm | data/hora | ISO 8601 | Nao |  |  |
| api_escopo | Id | uuid | uuid | Sim | PK |  |
| api_escopo | Codigo | texto | Nao informado no material | Sim | Unico |  |
| api_escopo | Descricao | texto | Nao informado no material | Sim |  |  |
| api_cliente_escopo | ClienteApiId | uuid | uuid | Sim | PK/FK cliente |  |
| api_cliente_escopo | EscopoId | uuid | uuid | Sim | PK/FK escopo |  |

### 12.5 rate limit, logs e auditoria

| Entidade | Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|---|
| api_politica_rate_limit | Id | uuid | uuid | Sim | PK |  |
| api_politica_rate_limit | TenantId | texto | 200 | Nao | Indice | Nulo para politica global. |
| api_politica_rate_limit | RotaId | uuid | uuid | Nao | FK rota |  |
| api_politica_rate_limit | ClienteApiId | uuid | uuid | Nao | FK cliente |  |
| api_politica_rate_limit | UsuarioId | uuid | uuid | Nao | FK usuario |  |
| api_politica_rate_limit | Limite | inteiro | Nao informado no material | Sim |  | Quantidade permitida. |
| api_politica_rate_limit | JanelaSegundos | inteiro | Nao informado no material | Sim |  |  |
| api_politica_rate_limit | Status | enum | Rascunho/Ativa/Inativa | Sim |  |  |
| api_log_requisicao | Id | uuid | uuid | Sim | PK |  |
| api_log_requisicao | CorrelationId | texto/uuid | Nao informado no material | Sim | Indice |  |
| api_log_requisicao | TenantId | texto | 200 | Nao | Indice |  |
| api_log_requisicao | EmpresaId | uuid/long | Nao informado no material | Nao | Indice |  |
| api_log_requisicao | UsuarioId | uuid | uuid | Nao | FK usuario |  |
| api_log_requisicao | ClienteApiId | uuid | uuid | Nao | FK cliente |  |
| api_log_requisicao | RotaId | uuid | uuid | Nao | FK rota |  |
| api_log_requisicao | MetodoHttp | enum | GET/POST/PUT/PATCH/DELETE | Sim |  |  |
| api_log_requisicao | Caminho | texto | Nao informado no material | Sim |  |  |
| api_log_requisicao | StatusHttp | inteiro | 100-599 | Sim | Indice |  |
| api_log_requisicao | LatenciaMs | inteiro | Nao informado no material | Sim |  |  |
| api_log_requisicao | IpHash | texto/hash | Nao informado no material | Nao |  | IP protegido. |
| api_log_requisicao | RequestResumoJson | json | Nao informado no material | Nao |  | Sem dados sensiveis. |
| api_log_requisicao | ResponseResumoJson | json | Nao informado no material | Nao |  | Sem dados sensiveis. |
| api_log_requisicao | DataHora | data/hora | ISO 8601 | Sim | Indice |  |
| api_evento_auditoria | Id | uuid | uuid | Sim | PK |  |
| api_evento_auditoria | TenantId | texto | 200 | Nao | Indice |  |
| api_evento_auditoria | TipoEvento | enum/texto | Nao informado no material | Sim | Indice | Login, download, falha, limite, publicacao. |
| api_evento_auditoria | UsuarioId | uuid | uuid | Nao | FK usuario |  |
| api_evento_auditoria | ClienteApiId | uuid | uuid | Nao | FK cliente |  |
| api_evento_auditoria | DetalheJson | json | Nao informado no material | Nao |  | Mascarado. |
| api_evento_auditoria | DataEvento | data/hora | ISO 8601 | Sim | Indice |  |

### 12.6 erro, lote, compatibilidade e eventos

| Entidade | Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|---|
| api_erro_catalogo | Codigo | texto | Nao informado no material | Sim | PK | Codigo padrao Epros. |
| api_erro_catalogo | HttpStatus | inteiro | 100-599 | Sim |  |  |
| api_erro_catalogo | MensagemPadrao | texto | Nao informado no material | Sim |  |  |
| api_erro_catalogo | Categoria | enum | Autenticacao/Autorizacao/Validacao/Conflito/Limite/FalhaInterna | Sim |  |  |
| api_lote | Id | uuid | uuid | Sim | PK |  |
| api_lote | TenantId | texto | 200 | Sim | Indice |  |
| api_lote | RotaId | uuid | uuid | Sim | FK rota |  |
| api_lote | UsuarioId | uuid | uuid | Nao | FK usuario |  |
| api_lote | ClienteApiId | uuid | uuid | Nao | FK cliente |  |
| api_lote | Status | enum | Recebida/EmProcessamento/Concluida/ConcluidaComErros/Falha/Cancelada | Sim |  |  |
| api_lote | TotalItens | inteiro | Nao informado no material | Sim |  |  |
| api_lote | ItensSucesso | inteiro | Nao informado no material | Sim |  |  |
| api_lote | ItensFalha | inteiro | Nao informado no material | Sim |  |  |
| api_lote | DataRecebimento | data/hora | ISO 8601 | Sim |  |  |
| api_lote_item | Id | uuid | uuid | Sim | PK |  |
| api_lote_item | LoteId | uuid | uuid | Sim | FK lote |  |
| api_lote_item | IdentificadorLocal | texto | Nao informado no material | Nao |  |  |
| api_lote_item | IdentificadorGerado | texto/uuid | Nao informado no material | Nao |  |  |
| api_lote_item | Status | enum | Sucesso/Falha/Ignorado | Sim |  |  |
| api_lote_item | CodigoErro | texto | Nao informado no material | Nao | FK erro_catalogo |  |
| api_lote_item | Mensagem | texto | Nao informado no material | Nao |  |  |
| api_adaptador_compatibilidade | Id | uuid | uuid | Sim | PK |  |
| api_adaptador_compatibilidade | RotaId | uuid | uuid | Sim | FK rota |  |
| api_adaptador_compatibilidade | VersaoOrigem | texto | Nao informado no material | Sim |  |  |
| api_adaptador_compatibilidade | OperacoesPermitidasJson | json | Nao informado no material | Sim |  | Whitelist. |
| api_adaptador_compatibilidade | Status | enum | Ativo/Inativo/Depreciado | Sim |  |  |
| api_evento_contrato | Id | uuid | uuid | Sim | PK |  |
| api_evento_contrato | CodigoEvento | texto | Nao informado no material | Sim | Unico |  |
| api_evento_contrato | ProdutorModulo | texto | Nao informado no material | Sim |  |  |
| api_evento_contrato | Versao | texto | Nao informado no material | Sim |  |  |
| api_evento_contrato | SchemaJson | json | Nao informado no material | Sim |  |  |
| api_evento_contrato | Status | enum | Rascunho/Publicado/Depreciado | Sim |  |  |
| api_depreciacao | Id | uuid | uuid | Sim | PK |  |
| api_depreciacao | RotaVersaoId | uuid | uuid | Sim | FK rota_versao |  |
| api_depreciacao | Motivo | texto | Nao informado no material | Sim |  |  |
| api_depreciacao | RotaSubstitutaId | uuid | uuid | Nao | FK rota |  |
| api_depreciacao | DataAviso | data | ISO 8601 | Sim |  |  |
| api_depreciacao | SuporteAte | data | ISO 8601 | Sim |  |  |

### 12.7 contratos tenant identificados

| Contrato | Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|---|
| LoginTenant | EmpresaId | uuid/long | Nao informado no material | Sim | FK empresa | Usado para login/contexto de empresa. |
| TenantCreate | Nome | texto | Nao informado no material | Sim |  | Nome do tenant. |
| TenantCreate | Documento | texto | Nao informado no material | Sim |  | Documento do tenant. |
| TenantCreate | Contato | texto | Nao informado no material | Nao |  |  |
| TenantCreate | Telefone | texto | Nao informado no material | Nao |  |  |
| TenantCreate | Ativo | booleano | true/false | Sim |  |  |
| TenantUpdate | TenantId | texto | 200 | Sim | PK logica |  |
| TenantUpdate | Nome | texto | Nao informado no material | Sim |  |  |
| TenantUpdate | Documento | texto | Nao informado no material | Sim |  |  |
| TenantUpdate | Contato | texto | Nao informado no material | Nao |  |  |
| TenantUpdate | Telefone | texto | Nao informado no material | Nao |  |  |
| TenantUpdate | Ativo | booleano | true/false | Sim |  |  |

## 13. Eventos

| Evento | Produtor | Consumidor esperado |
|---|---|---|
| api.contrato.publicado | API Gateway | Operacao Siser, consumidores externos e modulos internos. |
| api.rota.depreciada | API Gateway | Clientes de API, suporte e operacao. |
| api.limite.excedido | API Gateway | Operacao Siser, seguranca e tenant. |
| api.autenticacao.falhou | API Gateway | Seguranca e auditoria. |
| api.exportacao.openapi.publicada | API Gateway | GED e portal tecnico. |
| documento.fiscal.autorizado | Fiscal | Financeiro e estoque, conforme contrato. |
| compra.confirmada | Compras | Financeiro e estoque, conforme contrato. |
| venda.faturada | Vendas | Financeiro e fiscal, conforme contrato. |

## 14. Criterios de aceite

| ID | Criterio |
|---|---|
| CA-API-001 | Token invalido retorna HTTP 401 com erro padronizado. |
| CA-API-002 | Usuario sem acesso a empresa retorna HTTP 403. |
| CA-API-003 | Credencial invalida nao revela qual campo falhou. |
| CA-API-004 | Toda rota ativa possui dono, metodo, caminho, versao, permissao e status. |
| CA-API-005 | Rota publica exige justificativa e auditoria. |
| CA-API-006 | Rota tenantizada nao aceita TenantId do payload para ampliar escopo. |
| CA-API-007 | OpenAPI publicado contem somente rotas aprovadas para o ambiente. |
| CA-API-008 | Alteracao incompativel exige nova versao. |
| CA-API-009 | Rota depreciada informa substituta e data final de suporte. |
| CA-API-010 | Cliente de API nao exibe segredo depois da criacao. |
| CA-API-011 | Rate limit excedido retorna HTTP 429. |
| CA-API-012 | Batch retorna resultado por item. |
| CA-API-013 | Download de documento exige permissao e gera log. |
| CA-API-014 | Logs mascaram tokens, senhas, chaves e dados sensiveis. |
| CA-API-015 | Operacao nao registrada no catalogo nao pode ser executada por adaptador. |
| CA-API-016 | Todas as respostas carregam correlationId. |
| CA-API-017 | Contrato de evento publicado possui produtor, versao e schema. |
| CA-API-018 | Testes cobrem autenticacao, tenant, autorizacao, rate limit, OpenAPI e lote. |

## 15. Notas de rodape

[^1]: As entidades de rota, cliente de API, chaves, rate limit, log, lote, adaptador, deprecacao e eventos foram estruturadas a partir das regras e lacunas do material, que identifica contratos, autenticação, tenant, endpoints, versionamento, OpenAPI, batch, sincronizacao e riscos de seguranca, mas nao apresenta um modelo final unificado para o gateway.
[^2]: Rotas tecnicas e contratos inconsistentes foram transformados em regras de whitelist, indisponibilidade ou lacuna da MC, sem reproduzir exposicao insegura.

