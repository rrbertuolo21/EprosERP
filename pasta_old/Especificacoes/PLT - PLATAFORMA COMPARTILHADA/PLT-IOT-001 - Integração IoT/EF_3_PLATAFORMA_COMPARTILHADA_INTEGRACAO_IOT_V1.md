# EF_3_PLATAFORMA_COMPARTILHADA_INTEGRACAO_IOT_V1

**Projeto:** Epros  
**Empresa:** Siser  
**Tipo de documento:** Especificacao Funcional definitiva  
**Versao:** V1  
**Modulo:** PLATAFORMA_COMPARTILHADA  
**Submodulo:** INTEGRACAO_IOT  
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

O submodulo Integracao IoT do Epros deve controlar o cadastro de dispositivos, credenciais por tenant, ingestao de telemetria, leituras de condicao, eventos de dominio, trilha de auditoria e integracao com manutencao, producao e operacoes. O material define este submodulo como uma capacidade nova e parcial-controlada, sem implementacao operacional completa, mas com escopo suficiente para especificar a base funcional.

| Pergunta | Resposta |
|---|---|
| Para que o submodulo existe? | Para receber e governar telemetria de dispositivos e transforma-la em leitura operacional e evento de dominio. |
| Que problema de negocio resolve? | Permite que dados de equipamento, ativo ou sensor alimentem manutencao, producao e alertas sem cada modulo criar sua propria ingestao. |
| Qual resultado operacional deve produzir? | Dispositivos cadastrados, credenciais controladas, leituras recebidas, regras avaliadas, eventos publicados e historico auditavel. |
| Quais areas dependem dele? | Manutencao, Producao, Estoque, IA/ML, API Gateway, Offline Shell, Compliance e Analytics. |

## 3. Escopo funcional

### 3.1 Dentro do escopo

| Capacidade | Descricao | Observacao |
|---|---|---|
| Cadastro de dispositivo | Registrar dispositivo, codigo, tenant, status e responsavel. | Material informa cadastro de dispositivo por tenant. |
| Credencial por tenant | Controlar credencial de dispositivo segregada por tenant. | Material informa credencial por tenant. |
| Topico segregado | Usar padrao de topico com tenant e equipamento. | Material informa topico namespaced. |
| Ingestao de telemetria | Receber leitura enviada por dispositivo ou gateway. | Material informa ingestao de telemetria. |
| Leitura de condicao | Normalizar leitura em estrutura de condicao operacional. | Material cita ConditionReading. |
| Motor de regras | Avaliar leituras e gerar eventos de dominio. | Material cita RuleEngine. |
| Eventos de dominio | Publicar eventos para manutencao e producao. | Material informa eventos apos confirmacao. |
| Buffer offline e replay | Permitir recepcao posterior de leituras quando houver indisponibilidade. | Material cita buffer offline e replay. |
| Retencao de serie temporal | Controlar tempo de retencao de leituras. | Material informa retencao configuravel. |
| Workflow e auditoria | Controlar ciclo Rascunho, EmAnalise, Ativo, Inativo e Encerrado. | Material informa workflow e historico. |
| Anexos | Vincular documentos ao cadastro quando necessario. | Material informa anexo por GED. |

### 3.2 Fora do escopo

| Item fora do escopo | Motivo | Destino correto |
|---|---|---|
| Manutencao preditiva completa | IoT fornece leituras e eventos; planejamento preditivo pertence a manutencao. | Manutencao |
| Execucao de producao | IoT alimenta sinais; execucao produtiva pertence a producao. | Producao |
| Treinamento de modelos | IoT fornece dados; modelos pertencem a IA/ML. | IA/ML |
| Gateway/API final | Endpoints e contratos finais nao estao informados no material. | MC/API Gateway |
| Protocolos industriais completos | O material cita MQTT e nao detalha OPC-UA, Modbus ou contratos equivalentes. | MC |
| Cadastro mestre de ativo/equipamento | IoT referencia ativos, nao substitui cadastro dono. | Manutencao/Producao/Cadastros |

## 4. Glossario e conceitos funcionais

| Termo | Definicao funcional | Observacoes |
|---|---|---|
| Dispositivo | Origem fisica ou logica de telemetria. | Pode representar sensor, equipamento, ativo ou gateway. |
| Gateway | Componente de entrada que recebe ou encaminha telemetria de dispositivos. | Contrato final nao informado. |
| Telemetria | Dado enviado por dispositivo contendo valor, unidade, instante e contexto. | Estrutura final criada nesta EF.[^nota1] |
| Leitura de condicao | Registro normalizado da telemetria para avaliacao operacional. | Material cita ConditionReading. |
| Topico segregado | Caminho logico de comunicacao contendo tenant e equipamento. | Material informa padrao `/{tenantId}/equipamento/{id}`. |
| Buffer offline | Armazenamento temporario para envio posterior de leituras. | Material cita buffer offline e replay. |
| Replay | Reenvio ou processamento tardio de leituras armazenadas. | Material cita replay. |
| Motor de regras | Capacidade de avaliar leitura e gerar evento quando condicao e atendida. | Material cita RuleEngine. |

## 5. Atores, papeis e responsabilidades

| Ator/Papel | Responsabilidade | Permissoes esperadas | Restricoes |
|---|---|---|---|
| Operador | Consultar dispositivos, leituras e status. | Consultar e exportar. | Nao cria credencial nem aprova dispositivo. |
| Gestor IoT | Cadastrar dispositivo, credencial, regras e parametros por tenant. | Criar, editar, inativar, configurar. | Deve respeitar auditoria. |
| Aprovador | Aprovar ou rejeitar cadastro e ativacao. | Aprovar, rejeitar, informar motivo. | Nao executa processo dono de manutencao/producao. |
| Tecnico de manutencao | Consumir eventos e leituras vinculadas a ativo. | Consultar leituras e eventos permitidos. | Nao altera credencial IoT. |
| Operador de producao | Consumir sinais vinculados a equipamento/processo. | Consultar eventos e leituras permitidos. | Nao altera regra de ingestao. |
| Dispositivo/Gateway | Enviar telemetria. | Publicar leitura autorizada. | Deve usar credencial e topico permitidos. |
| Epros | Validar, receber, normalizar, auditar e publicar eventos. | Automacao sistemica. | Nao processa leitura sem tenant/dispositivo valido. |

## 6. Visao operacional do submodulo

O gestor cadastra um dispositivo IoT no Epros com codigo, tenant, responsavel, status e vinculo operacional quando aplicavel. O cadastro segue workflow de aprovacao ate ficar ativo. O Epros emite ou registra credencial por tenant e define o topico segregado pelo tenant e equipamento. O dispositivo ou gateway envia telemetria. O Epros valida tenant, dispositivo, credencial, topico, status, formato e instante da leitura.

Leituras validas sao normalizadas como leitura de condicao, armazenadas conforme politica de retencao e encaminhadas ao motor de regras. Quando uma regra e satisfeita, o Epros publica evento de dominio para manutencao, producao ou modulo consumidor autorizado. Se o envio ocorrer fora de conectividade, o buffer offline e o replay devem preservar ordem, origem e timestamp original quando esses dados estiverem disponiveis.

Falhas de autenticacao, topico invalido, dispositivo inativo, payload incompleto, duplicidade, atraso de replay ou falta de contrato com modulo consumidor devem gerar pendencia funcional auditavel.

## 7. Capacidades funcionais

### 7.1 Cadastro e governanca de dispositivo

| Item | Especificacao |
|---|---|
| Objetivo | Controlar dispositivos IoT por tenant, responsavel e status. |
| Acionamento | Manual pelo gestor. |
| Pre-condicoes | Tenant definido e usuario autorizado. |
| Dados de entrada | Codigo, responsavel, status, tipo, vinculo de ativo/equipamento e observacoes. |
| Processamento | O Epros valida obrigatorios, cria rascunho, registra historico e permite submissao. |
| Resultado esperado | Dispositivo cadastrado e governado. |
| Pos-condicoes | Dispositivo pode ser aprovado e ativado. |
| Excecoes | Tenant ausente, codigo ausente, responsavel ausente ou vinculo invalido. |
| Auditoria | Usuario, timestamp, IP, acao e payload. |

### 7.2 Credencial e topico por tenant

| Item | Especificacao |
|---|---|
| Objetivo | Garantir que cada dispositivo publique somente no contexto autorizado. |
| Acionamento | Criacao/ativacao de dispositivo ou rotacao de credencial. |
| Pre-condicoes | Dispositivo cadastrado e tenant valido. |
| Dados de entrada | Dispositivo, tenant, credencial, topico e status. |
| Processamento | O Epros associa credencial ao dispositivo e valida topico segregado. |
| Resultado esperado | Dispositivo apto a publicar telemetria. |
| Pos-condicoes | Leituras podem ser aceitas quando o dispositivo estiver ativo. |
| Excecoes | Credencial ausente, topico fora do tenant, dispositivo inativo. |
| Auditoria | Criacao, rotacao, bloqueio e uso de credencial. |

### 7.3 Ingestao e normalizacao de telemetria

| Item | Especificacao |
|---|---|
| Objetivo | Receber telemetria e transforma-la em leitura de condicao. |
| Acionamento | Publicacao do dispositivo/gateway. |
| Pre-condicoes | Credencial valida, topico valido e dispositivo ativo. |
| Dados de entrada | Tenant, equipamento, timestamp, variavel, valor, unidade e payload. |
| Processamento | O Epros valida, normaliza, registra e disponibiliza a leitura. |
| Resultado esperado | Leitura de condicao armazenada. |
| Pos-condicoes | Motor de regras pode avaliar a leitura. |
| Excecoes | Payload incompleto, unidade desconhecida, topico invalido ou duplicidade. |
| Auditoria | Origem, horario de coleta, horario de recebimento, status e erro. |

### 7.4 Buffer offline e replay

| Item | Especificacao |
|---|---|
| Objetivo | Processar leituras que chegam apos periodo offline. |
| Acionamento | Recepcao de lote ou leitura com timestamp anterior. |
| Pre-condicoes | Dispositivo e credencial validos. |
| Dados de entrada | Lote de leituras, sequencia, timestamp original e payload. |
| Processamento | O Epros valida ordem, duplicidade e janela de aceitacao. |
| Resultado esperado | Leituras aceitas, rejeitadas ou pendentes. |
| Pos-condicoes | Leituras aceitas podem gerar eventos. |
| Excecoes | Janela expirada, duplicidade, sequencia inconsistente ou lote incompleto. |
| Auditoria | Lote, quantidade, aceitas, rejeitadas e motivo. |

### 7.5 Motor de regras e eventos

| Item | Especificacao |
|---|---|
| Objetivo | Avaliar leituras e gerar eventos de dominio para modulos consumidores. |
| Acionamento | Nova leitura ou replay aceito. |
| Pre-condicoes | Regra ativa e contrato com modulo consumidor. |
| Dados de entrada | Leitura, regra, limiar, ativo/equipamento e modulo destino. |
| Processamento | O Epros avalia condicao e publica evento apos confirmacao transacional. |
| Resultado esperado | Evento publicado ou leitura registrada sem evento. |
| Pos-condicoes | Modulo consumidor pode agir conforme contrato. |
| Excecoes | Regra inativa, destino nao definido ou evento rejeitado. |
| Auditoria | Regra, leitura, resultado, evento e consumidor. |

## 8. Regras de negocio

| Regra | Descricao | Condicao | Resultado | Severidade | Observacoes |
|---|---|---|---|---|---|
| IOT-001 | Todo registro IoT deve possuir tenant. | Qualquer operacao. | Operacao sem tenant e bloqueada. | Bloqueante | Material informa TenantId obrigatorio. |
| IOT-002 | O dispositivo deve possuir codigo, status e responsavel. | Cadastro e manutencao. | Ausencia gera bloqueio. | Bloqueante | Campos informados. |
| IOT-003 | Novo dispositivo nasce em Rascunho. | Criacao valida. | Status Rascunho. | Bloqueante | Fluxo informado. |
| IOT-004 | Rascunho pode ser submetido para EmAnalise por operador. | Submissao. | Status EmAnalise. | Bloqueante | Fluxo informado. |
| IOT-005 | EmAnalise pode ser aprovado e tornar-se Ativo. | Aprovacao. | Status Ativo. | Bloqueante | Fluxo informado. |
| IOT-006 | EmAnalise pode ser rejeitado e voltar a Rascunho com motivo. | Rejeicao. | Status Rascunho. | Bloqueante | Fluxo informado. |
| IOT-007 | Ativo pode ser inativado ou encerrado por gestor. | Gestao. | Status Inativo ou Encerrado. | Bloqueante | Fluxo informado. |
| IOT-008 | Inativo pode ser reativado por gestor. | Reativacao. | Status Ativo. | Bloqueante | Fluxo informado. |
| IOT-009 | Dispositivo deve possuir credencial por tenant para publicar telemetria. | Ingestao. | Leitura sem credencial valida e rejeitada. | Bloqueante | Material informa credencial por tenant. |
| IOT-010 | Topico de publicacao deve conter tenant e equipamento. | Publicacao de telemetria. | Topico fora do padrao e rejeitado. | Bloqueante | Material informa padrao de topico. |
| IOT-011 | Dispositivo inativo ou encerrado nao deve publicar leitura aceita. | Ingestao. | Leitura bloqueada. | Bloqueante | Derivado do ciclo de vida.[^nota1] |
| IOT-012 | Telemetria aceita deve gerar leitura de condicao. | Payload valido. | Leitura normalizada. | Bloqueante | Material cita leitura de condicao. |
| IOT-013 | Eventos de dominio devem ser publicados somente apos confirmacao transacional. | Regra gera evento. | Evento publicado apos confirmacao. | Bloqueante | Material informa eventos apos commit. |
| IOT-014 | Buffer offline deve permitir replay quando habilitado. | Leitura em lote/offline. | Leitura processada conforme janela. | Bloqueante | Material cita buffer e replay. |
| IOT-015 | Serie temporal deve respeitar retencao configuravel. | Armazenamento de leitura. | Leitura retida ou expurgada conforme politica. | Bloqueante | Material informa retencao configuravel. |
| IOT-016 | Dados pessoais devem seguir base legal, retencao e anonimizacao. | Leitura ou vinculo com pessoa. | Dado bloqueado, mascarado ou retido conforme Compliance. | Bloqueante | Material informa LGPD. |
| IOT-017 | IoT nao deve duplicar cadastros mestres de ativo, pessoa ou equipamento. | Vinculo operacional. | Usa referencia por ID. | Bloqueante | Escopo informa nao duplicar cadastros. |
| IOT-018 | Integracao com modulo consumidor exige contrato documentado. | Evento para manutencao/producao/estoque. | Sem contrato, evento fica pendente. | Bloqueante | Evita inventar integracao. |
| IOT-019 | Alteracoes e transicoes devem registrar usuario, timestamp, IP e payload. | Workflow e edicao. | Historico auditavel. | Bloqueante | Material informa historico. |
| IOT-020 | Anexos devem referenciar arquivo do GED. | Inclusao de anexo. | Anexo sem arquivo e bloqueado. | Bloqueante | Material informa anexo. |

## 9. Parametros de configuracao

| Parametro | Finalidade | Tipo/formato | Valor padrao | Obrigatorio | Nivel | Quem pode alterar | Impacto |
|---|---|---|---|---|---|---|---|
| IOT_Habilitado | Ativar o submodulo para o tenant. | Booleano | Nao informado no material | Sim | Tenant | Gestor | Bloqueia ingestao quando inativo. |
| TopicoPadrao | Definir padrao de topico por tenant/equipamento. | Texto | `/{tenantId}/equipamento/{id}` | Sim | Tenant | Gestor tecnico | Valida publicacao. |
| RetencaoSerieTemporal | Definir prazo de retencao de leituras. | Periodo | Nao informado no material | Sim | Tenant | Gestor/Compliance | Controla armazenamento. |
| BufferOfflineHabilitado | Permitir envio posterior de leituras. | Booleano | Nao informado no material | Condicional | Tenant/Dispositivo | Gestor | Controla replay. |
| JanelaReplay | Limitar idade de leituras offline. | Periodo | Nao informado no material | Condicional | Tenant | Gestor | Bloqueia leitura tardia. |
| PoliticaEvento | Definir publicacao de evento por regra. | Texto/JSON | Nao informado no material | Condicional | Tenant/Regra | Gestor | Controla integracao. |

## 10. Modelo de dados funcional e implantavel

### 10.1 Visao geral do modelo

O material informa um agregado raiz com Id, TenantId, Codigo, Status e ResponsavelId, historico com Acao, UsuarioId e PayloadJson, anexo com ArquivoId, e arquitetura funcional com dispositivo, credencial por tenant, topico, ingestao, leitura de condicao, motor de regras, evento de dominio, buffer offline e retencao de serie temporal. As entidades abaixo consolidam essas capacidades em modelo funcional implantavel do Epros.[^nota1]

| Grupo de dados | Entidades/tabelas | Papel funcional | Observacoes |
|---|---|---|---|
| Cadastro mestre IoT | `iot_dispositivo`, `iot_credencial` | Controla dispositivo e credencial por tenant. | Campos base informados no material. |
| Ingestao | `iot_topico`, `iot_telemetria_lote`, `iot_leitura_condicao` | Recebe, valida e normaliza telemetria. | Estrutura criada para implantar a arquitetura citada.[^nota1] |
| Regras e eventos | `iot_regra`, `iot_evento_dominio` | Avalia leituras e publica eventos. | Material cita motor de regras e evento. |
| Offline | `iot_buffer_offline` | Controla replay e leituras tardias. | Material cita buffer offline. |
| Auditoria | `iot_historico`, `iot_anexo` | Registra alteracoes e arquivos. | Campos informados. |

### 10.2 Entidades, finalidade e cardinalidade

| Entidade | Finalidade | Cardinalidade | Observacao |
|---|---|---|---|
| `iot_dispositivo` | Representar dispositivo, sensor, equipamento ou gateway. | 1 por dispositivo | Agregado principal. |
| `iot_credencial` | Controlar credencial por tenant/dispositivo. | 0..N por dispositivo | Permite rotacao. |
| `iot_topico` | Controlar topico autorizado. | 1..N por dispositivo | Padrao informado com tenant/equipamento. |
| `iot_telemetria_lote` | Agrupar telemetria recebida. | 0..N por dispositivo | Necessario para offline/replay.[^nota1] |
| `iot_leitura_condicao` | Registrar leitura normalizada. | 0..N por dispositivo | Material cita ConditionReading. |
| `iot_regra` | Definir condicao de avaliacao. | 0..N por tenant/dispositivo | Motor de regras citado. |
| `iot_evento_dominio` | Registrar evento publicado ou pendente. | 0..N por leitura/regra | Eventos para manutencao/producao. |
| `iot_buffer_offline` | Controlar leituras offline/replay. | 0..N por lote | Material cita buffer e replay. |
| `iot_historico` | Auditar alteracoes e transicoes. | 0..N por dispositivo | Campos informados. |
| `iot_anexo` | Vincular documento GED. | 0..N por dispositivo | ArquivoId informado. |

### 10.3 Relacionamentos

| Origem | Relacao | Destino | Regra |
|---|---|---|---|
| Tenant | possui | `iot_dispositivo` | Todo dispositivo pertence a um tenant. |
| `iot_dispositivo` | possui | `iot_credencial` | Credencial autoriza publicacao. |
| `iot_dispositivo` | possui | `iot_topico` | Topico deve ser segregado por tenant/equipamento. |
| `iot_dispositivo` | gera | `iot_telemetria_lote` | Lotes agrupam leituras. |
| `iot_telemetria_lote` | possui | `iot_leitura_condicao` | Leituras sao normalizadas. |
| `iot_leitura_condicao` | aciona | `iot_regra` | Regras avaliam leitura. |
| `iot_regra` | gera | `iot_evento_dominio` | Eventos sao publicados para consumidores. |
| `iot_dispositivo` | possui | `iot_historico` | Workflow e alteracoes sao auditadas. |
| `iot_dispositivo` | possui | `iot_anexo` | Anexos usam GED. |

### 10.4 Estados funcionais

| Estado | Significado | Entrada permitida | Saida permitida |
|---|---|---|---|
| Rascunho | Dispositivo cadastrado e ainda nao aprovado. | Criacao ou rejeicao. | EmAnalise. |
| EmAnalise | Dispositivo aguardando aprovacao. | Submissao. | Ativo ou Rascunho. |
| Ativo | Dispositivo autorizado para operacao. | Aprovacao ou reativacao. | Inativo ou Encerrado. |
| Inativo | Dispositivo temporariamente indisponivel. | Inativacao. | Ativo. |
| Encerrado | Dispositivo encerrado. | Encerramento. | Nao informado no material. |

## 11. Dicionario de dados implantavel

### 11.1 `iot_dispositivo`

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| Id | Identificador | Nao informado no material | Sim | Chave primaria funcional | Campo informado. |
| TenantId | Identificador | Nao informado no material | Sim | Tenant | Obrigatorio em todas as operacoes. |
| Codigo | Texto | Nao informado no material | Sim | Codigo funcional | Campo informado. |
| Status | Enum/texto | Rascunho, EmAnalise, Ativo, Inativo, Encerrado | Sim | Estado | Campo informado. |
| ResponsavelId | Identificador | Nao informado no material | Sim | Pessoa/RH quando aplicavel | Campo informado. |
| TipoDispositivo | Texto | Nao informado no material | Nao informado no material | Classificacao | Sensor, gateway ou equipamento nao detalhados. |
| AtivoOperacionalId | Identificador | Nao informado no material | Nao informado no material | Manutencao/Producao | Vinculo a ativo/equipamento. |
| CriadoEm | Data/hora | Nao informado no material | Nao informado no material | Auditoria | Data de criacao. |

### 11.2 `iot_credencial`

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| Id | Identificador | Nao informado no material | Sim | Chave primaria funcional | Identificador da credencial. |
| DispositivoId | Identificador | Nao informado no material | Sim | `iot_dispositivo` | Dispositivo autorizado. |
| TenantId | Identificador | Nao informado no material | Sim | Tenant | Credencial segregada por tenant. |
| IdentificadorCredencial | Texto | Nao informado no material | Sim | Credencial | Conteudo sensivel nao detalhado. |
| StatusCredencial | Enum/texto | Ativa, Revogada, Expirada, Nao informado no material | Sim | Status | Dominio criado para controle.[^nota1] |
| CriadoEm | Data/hora | Nao informado no material | Nao informado no material | Auditoria | Criacao da credencial. |
| RevogadoEm | Data/hora | Nao informado no material | Nao | Auditoria | Rotacao/revogacao. |

### 11.3 `iot_topico`

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| Id | Identificador | Nao informado no material | Sim | Chave primaria funcional | Identificador do topico. |
| DispositivoId | Identificador | Nao informado no material | Sim | `iot_dispositivo` | Dispositivo dono. |
| TenantId | Identificador | Nao informado no material | Sim | Tenant | Usado no topico. |
| Topico | Texto | `/{tenantId}/equipamento/{id}` | Sim | Topico | Padrao informado no material. |
| Ativo | Booleano | Sim/Nao | Sim | Status | Controla uso. |

### 11.4 `iot_telemetria_lote`

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| Id | Identificador | Nao informado no material | Sim | Chave primaria funcional | Identificador do lote. |
| DispositivoId | Identificador | Nao informado no material | Sim | `iot_dispositivo` | Origem das leituras. |
| TenantId | Identificador | Nao informado no material | Sim | Tenant | Segregacao. |
| Origem | Texto | Gateway, Dispositivo, Nao informado no material | Sim | Origem | Origem funcional criada.[^nota1] |
| RecebidoEm | Data/hora | Nao informado no material | Sim | Auditoria | Data de recebimento. |
| QuantidadeLeituras | Numero | Nao informado no material | Nao informado no material | Controle | Contagem do lote. |
| Replay | Booleano | Sim/Nao | Sim | Offline | Indica replay. |
| StatusLote | Enum/texto | Aceito, Parcial, Rejeitado, Nao informado no material | Sim | Status | Dominio criado para controle.[^nota1] |

### 11.5 `iot_leitura_condicao`

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| Id | Identificador | Nao informado no material | Sim | Chave primaria funcional | Identificador da leitura. |
| LoteId | Identificador | Nao informado no material | Sim | `iot_telemetria_lote` | Lote de origem. |
| DispositivoId | Identificador | Nao informado no material | Sim | `iot_dispositivo` | Dispositivo origem. |
| Variavel | Texto | Nao informado no material | Sim | Medicao | Temperatura, pressao etc. nao detalhados. |
| Valor | Decimal/texto | Nao informado no material | Sim | Medicao | Valor recebido. |
| Unidade | Texto | Nao informado no material | Nao informado no material | Unidade | Unidade nao detalhada. |
| TimestampColeta | Data/hora | Nao informado no material | Sim | Tempo | Instante original. |
| TimestampRecebimento | Data/hora | Nao informado no material | Sim | Tempo | Instante no Epros. |
| PayloadJson | JSON | Nao informado no material | Nao informado no material | Payload | Dado bruto quando retido. |
| StatusLeitura | Enum/texto | Valida, Rejeitada, Pendente, Nao informado no material | Sim | Status | Dominio criado para controle.[^nota1] |

### 11.6 `iot_regra`

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| Id | Identificador | Nao informado no material | Sim | Chave primaria funcional | Identificador da regra. |
| TenantId | Identificador | Nao informado no material | Sim | Tenant | Segregacao. |
| DispositivoId | Identificador | Nao informado no material | Condicional | `iot_dispositivo` | Pode ser por dispositivo. |
| CodigoRegra | Texto | Nao informado no material | Sim | Codigo | Criado para motor de regras.[^nota1] |
| Variavel | Texto | Nao informado no material | Sim | Medicao | Variavel avaliada. |
| Condicao | Texto | Nao informado no material | Sim | Regra | Operador/limiar nao detalhados. |
| ModuloDestino | Texto | Manutencao, Producao, Estoque, Nao informado no material | Sim | Integracao | Consumidor do evento. |
| Ativa | Booleano | Sim/Nao | Sim | Status | Controla avaliacao. |

### 11.7 `iot_evento_dominio`

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| Id | Identificador | Nao informado no material | Sim | Chave primaria funcional | Identificador do evento. |
| LeituraId | Identificador | Nao informado no material | Sim | `iot_leitura_condicao` | Leitura que originou evento. |
| RegraId | Identificador | Nao informado no material | Sim | `iot_regra` | Regra atendida. |
| TipoEvento | Texto | Nao informado no material | Sim | Evento | Tipo final nao informado. |
| ModuloDestino | Texto | Manutencao, Producao, Estoque, Nao informado no material | Sim | Integracao | Consumidor. |
| PayloadJson | JSON | Nao informado no material | Sim | Payload | Dados enviados. |
| StatusPublicacao | Enum/texto | Pendente, Publicado, Rejeitado, Nao informado no material | Sim | Status | Dominio criado para controle.[^nota1] |
| PublicadoEm | Data/hora | Nao informado no material | Nao | Auditoria | Publicacao apos confirmacao. |

### 11.8 `iot_buffer_offline`

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| Id | Identificador | Nao informado no material | Sim | Chave primaria funcional | Identificador do buffer. |
| LoteId | Identificador | Nao informado no material | Sim | `iot_telemetria_lote` | Lote em replay/offline. |
| Sequencia | Numero | Nao informado no material | Nao informado no material | Ordem | Ordem final nao informada. |
| TimestampOriginal | Data/hora | Nao informado no material | Sim | Tempo | Horario da coleta original. |
| RecebidoEm | Data/hora | Nao informado no material | Sim | Tempo | Horario de recebimento. |
| StatusReplay | Enum/texto | Aceito, Rejeitado, Pendente, Nao informado no material | Sim | Status | Controle de replay. |
| MotivoRejeicao | Texto | Nao informado no material | Nao | Erro | Motivo quando rejeitado. |

### 11.9 `iot_historico`

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| Id | Identificador | Nao informado no material | Sim | Chave primaria funcional | Identificador do historico. |
| DispositivoId | Identificador | Nao informado no material | Sim | `iot_dispositivo` | Dispositivo auditado. |
| Acao | Texto | Nao informado no material | Sim | Acao | Campo informado. |
| UsuarioId | Identificador | Nao informado no material | Sim | Usuario | Campo informado. |
| PayloadJson | JSON | Nao informado no material | Sim | Auditoria | Campo informado. |
| Timestamp | Data/hora | Nao informado no material | Sim | Auditoria | Material informa timestamp. |
| Ip | Texto | Nao informado no material | Sim | Auditoria | Material informa IP. |

### 11.10 `iot_anexo`

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| Id | Identificador | Nao informado no material | Sim | Chave primaria funcional | Identificador do anexo. |
| DispositivoId | Identificador | Nao informado no material | Sim | `iot_dispositivo` | Dispositivo vinculado. |
| ArquivoId | Identificador | Nao informado no material | Sim | GED | Campo informado. |
| CriadoEm | Data/hora | Nao informado no material | Nao informado no material | Auditoria | Data de vinculo. |

## 12. Integracoes e fronteiras

| Origem/Destino | Tipo | Dados trocados | Regra |
|---|---|---|---|
| API Gateway | Entrada/Saida | Autenticacao, contratos e exposicao de ingestao. | Contrato final nao informado. |
| Offline Shell | Entrada | Buffer offline e replay. | Usado quando leituras forem enviadas apos indisponibilidade. |
| Manutencao | Saida | Eventos de condicao, alerta e ativo/equipamento. | Manutencao e dona da ordem/trabalho. |
| Producao | Saida | Eventos e leituras associadas a equipamento/processo. | Producao e dona da execucao produtiva. |
| Estoque | Saida condicional | Sinais operacionais quando houver contrato. | Nao informado no material. |
| IA/ML | Saida | Series historicas e leituras para modelos. | Treinamento/modelos ficam em IA/ML. |
| Compliance | Entrada/Saida | Retencao, anonimizacao e base legal. | Dados pessoais seguem Compliance. |
| GED | Entrada/Saida | Anexos de dispositivo. | ArquivoId informado. |

## 13. Telas, relatorios e experiencia operacional

| ID | Tela/Relatorio | Especificacao |
|---|---|---|
| TEL-IOT-001 | Lista | Listar registros de IoT com filtros por status, periodo e responsavel; acoes novo e exportar. |
| TEL-IOT-002 | Detalhe/Formulario | Exibir dados, historico, anexos e aprovacao. |
| TEL-IOT-003 | Painel gestor | Exibir KPIs e fila de aprovacao. |
| REL-IOT-001 | Posicao geral | Snapshot por status. |
| REL-IOT-002 | Auditoria de alteracoes | Trilha por periodo. |

## 14. Cenarios de validacao

| ID | Cenario | Resultado esperado |
|---|---|---|
| CT-IOT-001 | Criar registro valido. | Status Rascunho. |
| CT-IOT-002 | Submeter sem obrigatorios. | Operacao bloqueada. |
| CT-IOT-003 | Aprovar registro. | Status Ativo. |
| CT-IOT-004 | Tentar integrar rascunho. | Sem evento publicado. |
| CT-IOT-005 | Inativar com referencia. | Bloqueio ou inativacao conforme regra definida. |
| CT-IOT-006 | Aplicar mascaramento LGPD. | Campo oculto quando aplicavel. |
| CT-IOT-007 | Receber leitura com topico fora do tenant. | Leitura rejeitada. |
| CT-IOT-008 | Receber replay fora da janela permitida. | Lote rejeitado ou pendente. |

## 15. Indicadores e controles

| Indicador | Descricao |
|---|---|
| Dispositivos por status | Quantidade em Rascunho, EmAnalise, Ativo, Inativo e Encerrado. |
| Leituras por periodo | Quantidade de leituras recebidas, aceitas, rejeitadas e pendentes. |
| Eventos gerados | Eventos por regra, modulo destino e dispositivo. |
| Falhas de autenticacao | Tentativas rejeitadas por credencial/topico. |
| Replays processados | Lotes offline aceitos, parciais e rejeitados. |
| Retencao aplicada | Leituras expurgadas ou retidas por politica. |

## 16. Seguranca, privacidade e auditoria

| Area | Regra funcional |
|---|---|
| Tenant | Dispositivo, credencial, topico, leitura e evento devem ser segregados por tenant. |
| Credencial | Leitura sem credencial valida deve ser rejeitada. |
| Topico | Topico deve conter tenant e equipamento autorizados. |
| Auditoria | Cadastro, credencial, leitura, replay e evento devem registrar historico quando aplicavel. |
| LGPD | Retencao, anonimizacao e mascaramento seguem Compliance. |
| Eventos | Publicacao deve ocorrer apos confirmacao transacional. |

## 17. Matriz de rastreabilidade funcional

| Capacidade | Regras | Dados | Testes |
|---|---|---|---|
| Cadastro dispositivo | IOT-001 a IOT-008 | `iot_dispositivo`, `iot_historico` | CT-IOT-001 a CT-IOT-005 |
| Credencial/topico | IOT-009, IOT-010 | `iot_credencial`, `iot_topico` | CT-IOT-007 |
| Ingestao | IOT-011, IOT-012 | `iot_telemetria_lote`, `iot_leitura_condicao` | CT-IOT-007 |
| Eventos | IOT-013, IOT-018 | `iot_regra`, `iot_evento_dominio` | CT-IOT-004 |
| Offline/replay | IOT-014 | `iot_buffer_offline` | CT-IOT-008 |
| Retencao/LGPD | IOT-015, IOT-016 | `iot_leitura_condicao`, Compliance | CT-IOT-006 |
| Anexos | IOT-020 | `iot_anexo` | CT-IOT-001 |

## 18. Criterios de aceite

| Criterio | Resultado esperado |
|---|---|
| Modelo de dados | Dispositivo, credencial, topico, leitura, evento, buffer, historico e anexo estao definidos. |
| Tenant | Nenhuma leitura e aceita fora do tenant autorizado. |
| Workflow | Cadastro segue Rascunho, EmAnalise, Ativo, Inativo e Encerrado. |
| Ingestao | Telemetria valida gera leitura de condicao. |
| Eventos | Eventos sao publicados apenas por regras ativas e contratos definidos. |
| Offline | Replay respeita janela, sequencia e duplicidade quando definidos. |
| Retencao | Serie temporal segue politica configuravel. |
| Ausencia de invencao | O que nao esta informado no material aparece como `Nao informado no material` ou item da MC. |

## 19. Notas de rodape

[^nota1]: As entidades e dominios alem de `iot_dispositivo`, `iot_historico` e `iot_anexo` foram criados nesta especificacao para tornar implantaveis as capacidades comprovadas no material: dispositivo, credencial por tenant, topico segregado, ingestao, leitura de condicao, motor de regras, evento de dominio, buffer offline, replay e retencao de serie temporal. O material nao informa contratos finais, protocolos completos, campos fisicos, algoritmos de regra ou modelo definitivo de armazenamento.
