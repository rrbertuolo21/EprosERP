# EF 3 Plataforma Compartilhada - Compliance LGPD SOX IFRS V1

## 1. Identificacao

| Item | Valor |
|---|---|
| Sistema | Epros |
| Empresa | Siser |
| Modulo | Plataforma Compartilhada |
| Submodulo | Compliance LGPD SOX IFRS |
| Versao | V1 |
| Status | Especificacao funcional para validacao humana |
| Data | 2026-06-06 |

## 2. Objetivo funcional

O submodulo Compliance LGPD SOX IFRS centraliza politicas de privacidade, base legal, consentimento, direitos do titular, mascaramento, auditoria de acesso a dados pessoais, retencao, anonimizacao, segregacao de controles financeiros e configuracoes contabeis/regulatorias por tenant.

O material analisado nao trouxe entidades operacionais detalhadas nem endpoints finais. Ele trouxe requisitos funcionais de governanca e uma estrutura minima de agregado, historico e anexo. Por isso, esta EF descreve o desenho funcional necessario para o Epros operar compliance como capacidade transversal, e a MC separa o que precisa validacao humana antes da implantacao.

## 3. Escopo

### 3.1 Dentro do escopo

| Capacidade | Descricao |
|---|---|
| Base legal | Registrar base legal por finalidade de tratamento de dados. |
| Finalidade de tratamento | Controlar por que o dado e tratado, em qual modulo, com qual periodo e responsavel. |
| Consentimento | Registrar evidencia de consentimento com timestamp, IP, termo e versao. |
| Direitos do titular | Controlar solicitacoes de exportacao, retificacao, eliminacao e oposicao. |
| Mascaramento | Definir politica de exibicao de campo sensivel por perfil, modulo e contexto. |
| Auditoria de acesso | Registrar acesso a dado pessoal e dado sensivel. |
| Retencao | Definir prazos de retencao, anonimizacao e eliminacao. |
| Controles financeiros | Registrar trilha de alteracoes financeiras e segregacao de funcoes. |
| Configuracao IFRS | Registrar parametros contabeis/regulatorios por tenant quando aplicavel. |
| Workflow | Aprovar politicas, excecoes, eliminacoes, oposicoes e controles criticos. |
| Relatorios | Disponibilizar posicao geral, auditoria de alteracoes, retencao e solicitacoes do titular. |

### 3.2 Fora do escopo

| Tema | Tratamento |
|---|---|
| Cadastro mestre de pessoa | Pertence a Cadastros Base. |
| Autenticacao e sessao | Pertencem a Identidade e Contexto Tenant. |
| Documento formal/anexo | Pertence a Gestao Eletronica de Documentos. |
| Regra operacional do modulo dono | Permanece no modulo que coleta, processa ou usa o dado. |
| Calculo contabil/fiscal | Pertence aos modulos financeiro, fiscal e contabil correspondentes. |
| Auditoria de API de baixo nivel | Pertence ao API Gateway, consumida aqui quando for evento de compliance. |

## 4. Dependencias e consumidores

### 4.1 Dependencias

| Dependencia | Uso |
|---|---|
| Identidade e Contexto Tenant | Resolver usuario, tenant, empresa, perfil e sessao. |
| Usuarios e Papeis | Aplicar permissao, segregacao de funcoes e mascaramento. |
| Cadastros Base | Identificar titular, responsavel, pessoa e empresa. |
| Gestao Eletronica de Documentos | Armazenar termos, anexos, evidencias e documentos de politica. |
| Workflow | Aprovar politicas, excecoes, eliminacoes e controles criticos. |
| API Gateway e OpenAPI | Auditar APIs, padronizar erro, correlationId e logs. |
| Analytics e Mobilidade | Indicadores, relatorios e consultas de compliance. |

### 4.2 Consumidores

| Consumidor | Uso |
|---|---|
| Todos os modulos operacionais | Consultam politica de privacidade, mascaramento, retencao e auditoria. |
| Financeiro | Usa trilha de alteracao, segregacao e evidencias de controle. |
| Cadastros Base | Usa base legal, consentimento, titular, mascaramento e direitos do titular. |
| RH | Usa privacidade de dados de colaboradores, saude, folha, recrutamento e treinamento. |
| Fiscal/Contabil | Usa retencao documental, parametros regulatorios e trilhas de controle. |
| Operacao Siser | Monitora riscos, excecoes, solicitacoes e auditorias. |

## 5. Principios funcionais

| Codigo | Regra |
|---|---|
| REG-CMP-001 | Toda politica de compliance deve estar isolada por tenant. |
| REG-CMP-002 | Toda operacao critica deve registrar usuario, data/hora, acao, antes/depois quando aplicavel e IP quando disponivel. |
| REG-CMP-003 | Dado pessoal deve ter finalidade de tratamento e base legal associadas quando o modulo exigir governanca formal. |
| REG-CMP-004 | Consentimento deve guardar evidencia auditavel, versao do termo e momento da coleta. |
| REG-CMP-005 | Solicitacao do titular deve ter protocolo, tipo, prazo, responsavel, status e evidencias. |
| REG-CMP-006 | Campo sensivel deve poder ser mascarado por perfil, permissao, modulo e contexto. |
| REG-CMP-007 | Acesso a dado pessoal sensivel deve gerar trilha de auditoria. |
| REG-CMP-008 | Politica de retencao deve indicar prazo, acao final, criterio de disparo e responsavel. |
| REG-CMP-009 | Alteracao financeira critica deve exigir trilha imutavel e segregacao de funcao quando aplicavel. |
| REG-CMP-010 | Parametros IFRS devem ser versionados por tenant e vigencia. |
| REG-CMP-011 | Eventos de dominio devem ser publicados apos commit transacional. |
| REG-CMP-012 | Regras nao detalhadas no material devem ficar registradas na MC como lacuna de completude. |

## 6. Regras funcionais detalhadas

### 6.1 Base legal e finalidade

| Codigo | Regra |
|---|---|
| REG-CMP-013 | O Epros deve manter catalogo de finalidades de tratamento por tenant. |
| REG-CMP-014 | Cada finalidade deve possuir codigo, nome, descricao, base legal, modulo aplicavel, status e responsavel. |
| REG-CMP-015 | Base legal deve aceitar contrato, obrigacao legal, consentimento, legitimo interesse e outras bases aprovadas. |
| REG-CMP-016 | Finalidade ativa deve possuir responsavel. |
| REG-CMP-017 | Alteracao de base legal ou finalidade deve gerar historico com payload antes/depois. |
| REG-CMP-018 | Finalidade inativa nao deve ser usada por novos processos. |

### 6.2 Consentimento

| Codigo | Regra |
|---|---|
| REG-CMP-019 | Consentimento deve estar vinculado a titular, finalidade, termo, versao do termo e data/hora. |
| REG-CMP-020 | Consentimento deve registrar IP quando disponivel. |
| REG-CMP-021 | Consentimento deve registrar origem de coleta, canal e usuario/sistema coletor quando disponivel. |
| REG-CMP-022 | Consentimento revogado deve impedir novos tratamentos dependentes dessa base quando nao houver outra base legal aprovada. |
| REG-CMP-023 | Revogacao deve registrar motivo, data/hora e responsavel. |
| REG-CMP-024 | Nova versao de termo pode exigir novo consentimento conforme politica da finalidade. |

### 6.3 Direitos do titular

| Codigo | Regra |
|---|---|
| REG-CMP-025 | O Epros deve registrar solicitacoes de exportacao, retificacao, eliminacao e oposicao. |
| REG-CMP-026 | Toda solicitacao deve possuir protocolo unico. |
| REG-CMP-027 | Solicitacao deve identificar titular, tipo, origem, data/hora, prazo, responsavel e status. |
| REG-CMP-028 | Exportacao deve gerar pacote ou relatorio rastreavel conforme permissao. |
| REG-CMP-029 | Retificacao deve encaminhar ajuste ao modulo dono do dado. |
| REG-CMP-030 | Eliminacao deve respeitar retencao obrigatoria antes de remover ou anonimizar. |
| REG-CMP-031 | Oposicao deve bloquear tratamento quando aplicavel e registrar justificativa quando negada. |
| REG-CMP-032 | Conclusao ou rejeicao da solicitacao deve possuir motivo e evidencia. |

### 6.4 Mascaramento e classificacao de dados

| Codigo | Regra |
|---|---|
| REG-CMP-033 | O Epros deve permitir classificar campo como pessoal, sensivel, financeiro, fiscal, saude, identificador ou publico. |
| REG-CMP-034 | Politica de mascaramento deve declarar entidade, campo, perfil, modulo, acao e regra de exibicao. |
| REG-CMP-035 | Campo mascarado deve exibir valor reduzido, oculto ou tokenizado conforme politica. |
| REG-CMP-036 | Perfil com permissao de visualizacao integral deve gerar auditoria de acesso quando o campo for sensivel. |
| REG-CMP-037 | Exportacao deve aplicar mascaramento conforme permissao e finalidade. |
| REG-CMP-038 | Relatorio deve respeitar a mesma politica de mascaramento das telas e APIs. |

### 6.5 Auditoria e trilha imutavel

| Codigo | Regra |
|---|---|
| REG-CMP-039 | Auditoria deve registrar criacao, alteracao, exclusao/inativacao, aprovacao, rejeicao, acesso sensivel, exportacao e eliminacao. |
| REG-CMP-040 | Auditoria deve possuir usuario, tenant, empresa quando aplicavel, entidade, registro, acao, data/hora, IP, correlationId e payload. |
| REG-CMP-041 | Payload de auditoria deve permitir antes/depois quando houver alteracao. |
| REG-CMP-042 | Payload sensivel deve ser protegido ou mascarado conforme politica. |
| REG-CMP-043 | Evento financeiro critico deve possuir trilha imutavel. |
| REG-CMP-044 | Auditoria nao deve ser alterada por usuario operacional. |
| REG-CMP-045 | Excecao de auditoria deve exigir autorizacao administrativa e registrar motivo. |

### 6.6 Retencao, anonimizacao e eliminacao

| Codigo | Regra |
|---|---|
| REG-CMP-046 | Politica de retencao deve definir tipo de dado, modulo, prazo, acao final e base legal. |
| REG-CMP-047 | Acao final pode ser manter, anonimizar, eliminar, bloquear ou revisar. |
| REG-CMP-048 | Execucao automatica de retencao deve gerar lote auditavel. |
| REG-CMP-049 | Anonimizacao deve preservar integridade estatistica quando o modulo depender de historico agregado. |
| REG-CMP-050 | Eliminacao deve verificar bloqueios legais, financeiros, fiscais e contratuais. |
| REG-CMP-051 | Falha de retencao deve gerar pendencia operacional. |

### 6.7 Controles financeiros e segregacao

| Codigo | Regra |
|---|---|
| REG-CMP-052 | Alteracao financeira critica deve possuir trilha de usuario, valor anterior, valor novo e motivo. |
| REG-CMP-053 | Segregacao de funcao deve impedir que o mesmo usuario solicite e aprove controle critico quando a politica exigir. |
| REG-CMP-054 | Aprovacao de controle financeiro deve registrar aprovador, data/hora e justificativa. |
| REG-CMP-055 | Politica de controle deve indicar modulo, acao, risco, nivel de aprovacao e perfis incompativeis. |
| REG-CMP-056 | Violacao de segregacao deve bloquear operacao ou gerar excecao aprovada, conforme politica. |

### 6.8 Configuracao IFRS

| Codigo | Regra |
|---|---|
| REG-CMP-057 | Parametros IFRS devem ser registrados por tenant, vigencia, escopo e responsavel. |
| REG-CMP-058 | Alteracao de parametro IFRS deve gerar historico antes/depois. |
| REG-CMP-059 | Parametro IFRS ativo deve possuir data de inicio de vigencia. |
| REG-CMP-060 | Parametro substituido deve permanecer historico e consultavel. |
| REG-CMP-061 | Modulo consumidor deve consultar a configuracao vigente na data da operacao. |

## 7. Estados

| Estado | Descricao | Entrada | Saida |
|---|---|---|---|
| Rascunho | Registro criado e editavel. | Criacao. | Submeter, cancelar. |
| Em analise | Registro aguarda aprovacao. | Submissao. | Aprovar, rejeitar. |
| Ativo | Registro aprovado e vigente. | Aprovacao. | Inativar, encerrar, revisar. |
| Rejeitado | Registro recusado com motivo. | Rejeicao. | Retornar a rascunho quando permitido. |
| Inativo | Registro desativado. | Inativacao. | Reativar. |
| Encerrado | Registro finalizado para historico. | Encerramento. | Nao informado no material. |
| Em revisao | Registro ativo sob revisao de politica. | Revisao manual ou vencimento. | Aprovar nova versao, manter ativo, inativar. |

## 8. Fluxos operacionais

### 8.1 Criacao e aprovacao de politica

| Passo | Ator | Acao | Resultado |
|---|---|---|---|
| 1 | Operador | Cria registro em rascunho. | Registro editavel. |
| 2 | Operador | Submete para analise. | Status em analise. |
| 3 | Aprovador | Aprova ou rejeita. | Registro ativo ou retornado. |
| 4 | Epros | Registra historico. | Auditoria com usuario, data/hora e IP quando disponivel. |
| 5 | Epros | Publica evento. | Modulos consumidores atualizam cache ou comportamento. |

### 8.2 Solicitacao do titular

| Passo | Ator | Acao | Resultado |
|---|---|---|---|
| 1 | Titular/Operador | Registra solicitacao. | Protocolo criado. |
| 2 | Epros | Classifica tipo e prazo. | Responsavel definido. |
| 3 | Responsavel | Analisa dados e restricoes. | Decisao preparada. |
| 4 | Workflow | Aprova quando necessario. | Execucao autorizada ou rejeitada. |
| 5 | Modulo dono | Executa exportacao, retificacao, eliminacao ou oposicao. | Evidencia registrada. |
| 6 | Epros | Encerra protocolo. | Historico completo. |

### 8.3 Retencao automatica

| Passo | Ator | Acao | Resultado |
|---|---|---|---|
| 1 | Epros | Identifica politica vencida. | Lote de retencao criado. |
| 2 | Epros | Verifica bloqueios legais/financeiros/fiscais. | Itens elegiveis e bloqueados. |
| 3 | Epros | Executa acao final. | Manter, anonimizar, eliminar, bloquear ou revisar. |
| 4 | Epros | Registra auditoria. | Lote rastreavel. |

## 9. Permissoes funcionais

| Permissao | Descricao |
|---|---|
| compliance.visualizar | Consultar politicas e registros permitidos. |
| compliance.criar | Criar politica, finalidade, controle ou solicitacao. |
| compliance.editar | Editar registro em rascunho ou revisao. |
| compliance.submeter | Submeter registro para analise. |
| compliance.aprovar | Aprovar politica, excecao ou solicitacao. |
| compliance.rejeitar | Rejeitar registro com motivo. |
| compliance.inativar | Inativar politica ou controle. |
| compliance.reativar | Reativar politica inativa. |
| compliance.auditoria | Consultar trilhas completas. |
| compliance.exportar | Exportar dados e relatorios conforme mascaramento. |
| compliance.eliminar | Autorizar eliminacao quando permitido. |
| compliance.configurar-ifrs | Manter parametros IFRS. |

## 10. Modelo de dados funcional e implantavel

### 10.1 Visao geral

| Entidade | Tipo | Finalidade | Observacao |
|---|---|---|---|
| cmp_registro | Agregado | Registro generico de politica/controle de compliance. | Preserva campos minimos do material. |
| cmp_historico | Auditoria | Historico com acao, usuario e payload. | Preserva campos minimos do material. |
| cmp_anexo | Anexo | Vincula arquivos formais ao registro. | Preserva campo ArquivoId. |
| cmp_base_legal | Configuracao | Catalogo de bases legais. | Necessario para RF-CMP-001. |
| cmp_finalidade_tratamento | Configuracao | Finalidade de tratamento por modulo. | Necessario para RF-CMP-001. |
| cmp_consentimento | Movimento | Evidencia de consentimento. | Necessario para RF-CMP-002. |
| cmp_dsr_solicitacao | Movimento | Direitos do titular. | Necessario para RF-CMP-003. |
| cmp_mascaramento_politica | Configuracao | Regras de mascaramento por campo/perfil. | Necessario para RF-CMP-004. |
| cmp_auditoria_acesso_dado | Auditoria | Acesso a dado pessoal/sensivel. | Necessario para RF-CMP-005. |
| cmp_retencao_politica | Configuracao | Prazos e acoes de retencao. | Necessario para RF-CMP-006. |
| cmp_retencao_execucao | Movimento | Lotes de retencao/anonimizacao/eliminacao. | Necessario para operacao automatica. |
| cmp_controle_sox | Configuracao | Segregacao e controles financeiros. | Necessario para RF-CMP-007. |
| cmp_evento_financeiro_auditado | Auditoria | Trilhas financeiras criticas. | Necessario para controle financeiro. |
| cmp_configuracao_ifrs | Configuracao | Parametros IFRS por tenant e vigencia. | Necessario para configuracao regulatoria. |

### 10.2 Entidade cmp_registro

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| id | uuid | 36 | Sim | PK | Campo Id preservado. |
| tenant_id | uuid | 36 | Sim | FK tenant | Campo TenantId preservado. |
| codigo | string | Nao informado no material | Sim | Chave funcional | Campo Codigo preservado. |
| tipo_registro | enum | Base legal; Finalidade; Consentimento; Controle; Retencao; IFRS; Outro | Sim | Campo simples | Classifica o registro. |
| status | enum | Rascunho; Em analise; Ativo; Rejeitado; Inativo; Encerrado; Em revisao | Sim | Estado | Campo Status preservado. |
| responsavel_id | uuid | 36 | Sim | FK usuario/pessoa | Campo ResponsavelId preservado. |
| descricao | texto | Nao informado no material | Nao | Campo simples | Descricao funcional. |
| criado_em | datetime | ISO 8601 | Sim | Auditoria | Data/hora de criacao. |
| atualizado_em | datetime | ISO 8601 | Sim | Auditoria | Data/hora de atualizacao. |

### 10.3 Entidade cmp_historico

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| id | uuid | 36 | Sim | PK | Identificador do historico. |
| registro_id | uuid | 36 | Sim | FK cmp_registro | Registro associado. |
| acao | enum/texto | Nao informado no material | Sim | Auditoria | Campo Acao preservado. |
| usuario_id | uuid | 36 | Sim | FK usuario | Campo UsuarioId preservado. |
| payload_json | json | Nao informado no material | Sim | Auditoria | Campo PayloadJson preservado. |
| ip | string | Nao informado no material | Nao | Auditoria | Informado no fluxo de auditoria. |
| correlation_id | string | Nao informado no material | Nao | Observabilidade | Necessario para APIs. |
| criado_em | datetime | ISO 8601 | Sim | Auditoria | Timestamp da acao. |

### 10.4 Entidade cmp_anexo

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| id | uuid | 36 | Sim | PK | Identificador do anexo. |
| registro_id | uuid | 36 | Sim | FK cmp_registro | Registro associado. |
| arquivo_id | uuid | 36 | Sim | FK documento/arquivo | Campo ArquivoId preservado. |
| tipo_anexo | string | Nao informado no material | Nao | Campo simples | Termo, evidencia, parecer ou documento. |
| criado_em | datetime | ISO 8601 | Sim | Auditoria | Data/hora do vinculo. |

### 10.5 Entidades especificas

| Entidade | Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|---|
| cmp_base_legal | id | uuid | 36 | Sim | PK | Identificador. |
| cmp_base_legal | tenant_id | uuid | 36 | Sim | FK tenant | Isolamento. |
| cmp_base_legal | codigo | string | Nao informado no material | Sim | Chave funcional | Codigo da base. |
| cmp_base_legal | nome | string | Nao informado no material | Sim | Campo simples | Nome da base legal. |
| cmp_base_legal | descricao | texto | Nao informado no material | Nao | Campo simples | Detalhe. |
| cmp_base_legal | status | enum | Ativa; Inativa | Sim | Estado | Uso em finalidades. |
| cmp_finalidade_tratamento | id | uuid | 36 | Sim | PK | Identificador. |
| cmp_finalidade_tratamento | tenant_id | uuid | 36 | Sim | FK tenant | Isolamento. |
| cmp_finalidade_tratamento | codigo | string | Nao informado no material | Sim | Chave funcional | Codigo da finalidade. |
| cmp_finalidade_tratamento | base_legal_id | uuid | 36 | Sim | FK cmp_base_legal | Base legal. |
| cmp_finalidade_tratamento | modulo | string | Nao informado no material | Sim | Referencia funcional | Modulo consumidor. |
| cmp_finalidade_tratamento | responsavel_id | uuid | 36 | Sim | FK usuario/pessoa | Responsavel. |
| cmp_finalidade_tratamento | status | enum | Rascunho; Em analise; Ativa; Inativa; Encerrada | Sim | Estado | Workflow. |
| cmp_consentimento | id | uuid | 36 | Sim | PK | Identificador. |
| cmp_consentimento | tenant_id | uuid | 36 | Sim | FK tenant | Isolamento. |
| cmp_consentimento | titular_pessoa_id | uuid | 36 | Sim | FK pessoa | Titular. |
| cmp_consentimento | finalidade_id | uuid | 36 | Sim | FK cmp_finalidade_tratamento | Finalidade. |
| cmp_consentimento | termo_documento_id | uuid | 36 | Nao | FK documento | Termo formal. |
| cmp_consentimento | versao_termo | string | Nao informado no material | Sim | Campo simples | Versao do termo. |
| cmp_consentimento | ip | string | Nao informado no material | Nao | Auditoria | Evidencia. |
| cmp_consentimento | consentido_em | datetime | ISO 8601 | Sim | Auditoria | Timestamp. |
| cmp_consentimento | revogado_em | datetime | ISO 8601 | Nao | Auditoria | Quando revogado. |
| cmp_dsr_solicitacao | id | uuid | 36 | Sim | PK | Identificador. |
| cmp_dsr_solicitacao | tenant_id | uuid | 36 | Sim | FK tenant | Isolamento. |
| cmp_dsr_solicitacao | protocolo | string | Nao informado no material | Sim | Chave funcional | Protocolo unico. |
| cmp_dsr_solicitacao | titular_pessoa_id | uuid | 36 | Sim | FK pessoa | Titular. |
| cmp_dsr_solicitacao | tipo | enum | Exportacao; Retificacao; Eliminacao; Oposicao | Sim | Campo simples | RF-CMP-003. |
| cmp_dsr_solicitacao | status | enum | Aberta; Em analise; Aprovada; Rejeitada; Executada; Encerrada | Sim | Estado | Ciclo da solicitacao. |
| cmp_dsr_solicitacao | prazo_resposta | datetime | ISO 8601 | Nao | SLA | Nao informado no material. |
| cmp_dsr_solicitacao | responsavel_id | uuid | 36 | Sim | FK usuario/pessoa | Responsavel. |
| cmp_mascaramento_politica | id | uuid | 36 | Sim | PK | Identificador. |
| cmp_mascaramento_politica | tenant_id | uuid | 36 | Sim | FK tenant | Isolamento. |
| cmp_mascaramento_politica | entidade | string | Nao informado no material | Sim | Referencia funcional | Entidade/campo protegido. |
| cmp_mascaramento_politica | campo | string | Nao informado no material | Sim | Referencia funcional | Campo protegido. |
| cmp_mascaramento_politica | perfil_id | uuid | 36 | Nao | FK perfil | Perfil aplicavel. |
| cmp_mascaramento_politica | regra_mascaramento | string | Nao informado no material | Sim | Campo simples | Ocultar, parcial, tokenizar. |
| cmp_auditoria_acesso_dado | id | uuid | 36 | Sim | PK | Identificador. |
| cmp_auditoria_acesso_dado | tenant_id | uuid | 36 | Sim | FK tenant | Isolamento. |
| cmp_auditoria_acesso_dado | usuario_id | uuid | 36 | Sim | FK usuario | Usuario que acessou. |
| cmp_auditoria_acesso_dado | entidade | string | Nao informado no material | Sim | Referencia funcional | Entidade acessada. |
| cmp_auditoria_acesso_dado | registro_id | uuid/string | Nao informado no material | Sim | Referencia funcional | Registro acessado. |
| cmp_auditoria_acesso_dado | campo | string | Nao informado no material | Nao | Referencia funcional | Campo acessado. |
| cmp_auditoria_acesso_dado | acao | enum | Visualizar; Exportar; Revelar; Alterar; Eliminar | Sim | Auditoria | Tipo de acesso. |
| cmp_auditoria_acesso_dado | acessado_em | datetime | ISO 8601 | Sim | Auditoria | Timestamp. |
| cmp_retencao_politica | id | uuid | 36 | Sim | PK | Identificador. |
| cmp_retencao_politica | tenant_id | uuid | 36 | Sim | FK tenant | Isolamento. |
| cmp_retencao_politica | modulo | string | Nao informado no material | Sim | Referencia funcional | Modulo dono. |
| cmp_retencao_politica | tipo_dado | string | Nao informado no material | Sim | Campo simples | Classe do dado. |
| cmp_retencao_politica | prazo_dias | inteiro | >= 0 | Sim | Campo simples | Prazo. |
| cmp_retencao_politica | acao_final | enum | Manter; Anonimizar; Eliminar; Bloquear; Revisar | Sim | Campo simples | Acao pos-prazo. |
| cmp_retencao_execucao | id | uuid | 36 | Sim | PK | Identificador. |
| cmp_retencao_execucao | politica_id | uuid | 36 | Sim | FK cmp_retencao_politica | Politica aplicada. |
| cmp_retencao_execucao | status | enum | Pendente; Em execucao; Concluida; Falha; Parcial | Sim | Estado | Status do lote. |
| cmp_retencao_execucao | total_itens | inteiro | >= 0 | Sim | Campo simples | Quantidade avaliada. |
| cmp_retencao_execucao | iniciado_em | datetime | ISO 8601 | Sim | Auditoria | Inicio. |
| cmp_retencao_execucao | concluido_em | datetime | ISO 8601 | Nao | Auditoria | Fim. |
| cmp_controle_sox | id | uuid | 36 | Sim | PK | Identificador. |
| cmp_controle_sox | tenant_id | uuid | 36 | Sim | FK tenant | Isolamento. |
| cmp_controle_sox | modulo | string | Nao informado no material | Sim | Referencia funcional | Modulo financeiro/operacional. |
| cmp_controle_sox | acao_critica | string | Nao informado no material | Sim | Campo simples | Acao controlada. |
| cmp_controle_sox | exige_aprovacao | boolean | true/false | Sim | Campo simples | Segregacao. |
| cmp_controle_sox | perfil_solicitante_id | uuid | 36 | Nao | FK perfil | Perfil solicitante. |
| cmp_controle_sox | perfil_aprovador_id | uuid | 36 | Nao | FK perfil | Perfil aprovador. |
| cmp_evento_financeiro_auditado | id | uuid | 36 | Sim | PK | Identificador. |
| cmp_evento_financeiro_auditado | tenant_id | uuid | 36 | Sim | FK tenant | Isolamento. |
| cmp_evento_financeiro_auditado | modulo | string | Nao informado no material | Sim | Referencia funcional | Modulo produtor. |
| cmp_evento_financeiro_auditado | entidade | string | Nao informado no material | Sim | Referencia funcional | Entidade alterada. |
| cmp_evento_financeiro_auditado | registro_id | uuid/string | Nao informado no material | Sim | Referencia funcional | Registro alterado. |
| cmp_evento_financeiro_auditado | usuario_id | uuid | 36 | Sim | FK usuario | Executor. |
| cmp_evento_financeiro_auditado | valor_anterior_json | json | Nao informado no material | Nao | Auditoria | Antes. |
| cmp_evento_financeiro_auditado | valor_novo_json | json | Nao informado no material | Nao | Auditoria | Depois. |
| cmp_configuracao_ifrs | id | uuid | 36 | Sim | PK | Identificador. |
| cmp_configuracao_ifrs | tenant_id | uuid | 36 | Sim | FK tenant | Isolamento. |
| cmp_configuracao_ifrs | codigo | string | Nao informado no material | Sim | Chave funcional | Codigo do parametro. |
| cmp_configuracao_ifrs | descricao | texto | Nao informado no material | Nao | Campo simples | Descricao. |
| cmp_configuracao_ifrs | valor_json | json | Nao informado no material | Sim | Configuracao | Valor parametrizado. |
| cmp_configuracao_ifrs | inicio_vigencia | date | ISO 8601 | Sim | Vigencia | Inicio. |
| cmp_configuracao_ifrs | fim_vigencia | date | ISO 8601 | Nao | Vigencia | Fim. |
| cmp_configuracao_ifrs | status | enum | Rascunho; Ativa; Inativa; Substituida | Sim | Estado | Status. |

## 11. Dicionario de dados implantavel

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| Id/id | uuid | 36 | Sim | PK | Identificador tecnico preservado e normalizado como id. |
| TenantId/tenant_id | uuid | 36 | Sim | FK tenant | Obrigatorio em todas as operacoes tenantizadas. |
| Codigo/codigo | string | Nao informado no material | Sim | Chave funcional | Codigo funcional do registro, base, finalidade ou parametro. |
| Status/status | enum | Conforme entidade | Sim | Estado | Controla ciclo de vida. |
| ResponsavelId/responsavel_id | uuid | 36 | Sim | FK usuario/pessoa | Responsavel pelo registro. |
| Acao/acao | enum/texto | Nao informado no material | Sim | Auditoria | Acao executada. |
| UsuarioId/usuario_id | uuid | 36 | Sim | FK usuario | Usuario executor. |
| PayloadJson/payload_json | json | Nao informado no material | Sim | Auditoria | Dados de auditoria, antes/depois quando aplicavel. |
| ArquivoId/arquivo_id | uuid | 36 | Sim | FK documento/arquivo | Anexo formal. |
| tipo_registro | enum | Base legal; Finalidade; Consentimento; Controle; Retencao; IFRS; Outro | Sim | Campo simples | Classificacao do registro. |
| descricao | texto | Nao informado no material | Nao | Campo simples | Descricao funcional. |
| ip | string | Nao informado no material | Nao | Auditoria | IP quando disponivel. |
| correlation_id | string | Nao informado no material | Nao | Observabilidade | Rastreio de API. |
| criado_em | datetime | ISO 8601 | Sim | Auditoria | Data/hora de criacao. |
| atualizado_em | datetime | ISO 8601 | Sim | Auditoria | Data/hora de atualizacao. |
| nome | string | Nao informado no material | Sim | Campo simples | Nome da base legal ou finalidade. |
| base_legal_id | uuid | 36 | Sim | FK cmp_base_legal | Base legal aplicada. |
| modulo | string | Nao informado no material | Sim | Referencia funcional | Modulo consumidor/produtor. |
| titular_pessoa_id | uuid | 36 | Sim | FK pessoa | Titular de dados. |
| termo_documento_id | uuid | 36 | Nao | FK documento | Termo de consentimento. |
| versao_termo | string | Nao informado no material | Sim | Campo simples | Versao do termo. |
| consentido_em | datetime | ISO 8601 | Sim | Auditoria | Momento do consentimento. |
| revogado_em | datetime | ISO 8601 | Nao | Auditoria | Momento da revogacao. |
| protocolo | string | Nao informado no material | Sim | Chave funcional | Protocolo da solicitacao do titular. |
| tipo | enum | Conforme entidade | Sim | Campo simples | Tipo de solicitacao/notificacao/anexo. |
| prazo_resposta | datetime | ISO 8601 | Nao | SLA | Prazo da solicitacao. |
| entidade | string | Nao informado no material | Sim | Referencia funcional | Entidade auditada/protegida. |
| campo | string | Nao informado no material | Nao | Referencia funcional | Campo protegido/acessado. |
| perfil_id | uuid | 36 | Nao | FK perfil | Perfil aplicavel. |
| regra_mascaramento | string | Nao informado no material | Sim | Campo simples | Como mascarar. |
| registro_id | uuid/string | Nao informado no material | Sim | Referencia funcional | Registro auditado. |
| acessado_em | datetime | ISO 8601 | Sim | Auditoria | Momento do acesso. |
| tipo_dado | string | Nao informado no material | Sim | Campo simples | Classe de dado. |
| prazo_dias | inteiro | >= 0 | Sim | Campo simples | Prazo de retencao. |
| acao_final | enum | Manter; Anonimizar; Eliminar; Bloquear; Revisar | Sim | Campo simples | Acao ao fim do prazo. |
| politica_id | uuid | 36 | Sim | FK politica | Politica aplicada. |
| total_itens | inteiro | >= 0 | Sim | Campo simples | Quantidade de itens no lote. |
| iniciado_em | datetime | ISO 8601 | Sim | Auditoria | Inicio do lote. |
| concluido_em | datetime | ISO 8601 | Nao | Auditoria | Fim do lote. |
| acao_critica | string | Nao informado no material | Sim | Campo simples | Acao de controle financeiro. |
| exige_aprovacao | boolean | true/false | Sim | Campo simples | Se exige aprovacao. |
| perfil_solicitante_id | uuid | 36 | Nao | FK perfil | Perfil solicitante. |
| perfil_aprovador_id | uuid | 36 | Nao | FK perfil | Perfil aprovador. |
| valor_anterior_json | json | Nao informado no material | Nao | Auditoria | Valor antes. |
| valor_novo_json | json | Nao informado no material | Nao | Auditoria | Valor depois. |
| valor_json | json | Nao informado no material | Sim | Configuracao | Valor de parametro IFRS. |
| inicio_vigencia | date | ISO 8601 | Sim | Vigencia | Inicio. |
| fim_vigencia | date | ISO 8601 | Nao | Vigencia | Fim. |

## 12. APIs funcionais

| API | Metodo | Finalidade | Observacao |
|---|---|---|---|
| /compliance/registros | POST | Criar registro de politica/controle. | Endpoint final nao informado no material. |
| /compliance/registros | GET | Listar registros por status, periodo e responsavel. | Filtros informados no material. |
| /compliance/registros/{id} | GET | Consultar detalhe com historico, anexos e aprovacao. | Tela informada. |
| /compliance/registros/{id}/submeter | POST | Submeter para analise. | Fluxo informado. |
| /compliance/registros/{id}/aprovar | POST | Aprovar registro. | Fluxo informado. |
| /compliance/registros/{id}/rejeitar | POST | Rejeitar com motivo. | Fluxo informado. |
| /compliance/consentimentos | POST | Registrar consentimento. | Necessario para RF-CMP-002. |
| /compliance/direitos-titular | POST | Abrir solicitacao do titular. | Necessario para RF-CMP-003. |
| /compliance/mascaramento/politicas | POST | Criar politica de mascaramento. | Necessario para RF-CMP-004. |
| /compliance/auditoria/acessos | GET | Consultar auditoria de acesso a dados. | Necessario para RF-CMP-005. |
| /compliance/retencao/politicas | POST | Criar politica de retencao. | Necessario para RF-CMP-006. |
| /compliance/controles-financeiros | POST | Criar controle financeiro/segregacao. | Necessario para RF-CMP-007. |
| /compliance/ifrs/configuracoes | POST | Criar configuracao IFRS. | Necessario para configuracao regulatoria. |

## 13. Telas e relatorios

| Tela/Relatorio | Conteudo | Filtros/Acoes |
|---|---|---|
| Lista de compliance | Registros, status, responsavel, periodo. | Novo, exportar, filtrar. |
| Detalhe/formulario | Dados, historico, anexos e aprovacao. | Salvar, submeter, aprovar, rejeitar, inativar. |
| Painel gestor | KPIs, fila de aprovacao, pendencias e riscos. | Periodo, responsavel, tipo. |
| Posicao geral | Snapshot por status. | Status, periodo, responsavel. |
| Auditoria de alteracoes | Trilha por periodo. | Usuario, entidade, acao, periodo. |
| Solicitacoes do titular | Protocolos e prazos. | Tipo, status, titular, responsavel. |
| Retencao | Politicas e execucoes. | Modulo, tipo de dado, status. |

## 14. Criterios de aceite

| ID | Criterio |
|---|---|
| CA-CMP-001 | Registro valido nasce em rascunho com tenant, codigo, status e responsavel. |
| CA-CMP-002 | Submissao sem campos obrigatorios retorna erro de validacao. |
| CA-CMP-003 | Aprovacao altera status para ativo e gera historico. |
| CA-CMP-004 | Rejeicao exige motivo e retorna registro ao estado permitido. |
| CA-CMP-005 | Mascaramento oculta campo sensivel para perfil sem permissao. |
| CA-CMP-006 | Acesso a dado sensivel gera auditoria. |
| CA-CMP-007 | Consentimento registra titular, finalidade, termo, versao, timestamp e IP quando disponivel. |
| CA-CMP-008 | Solicitacao do titular recebe protocolo unico e status inicial. |
| CA-CMP-009 | Politica de retencao executada gera lote auditavel. |
| CA-CMP-010 | Alteracao financeira critica gera trilha antes/depois. |

## 15. Testes funcionais

| ID | Cenario | Resultado esperado |
|---|---|---|
| CT-CMP-001 | Criar registro valido. | Status rascunho. |
| CT-CMP-002 | Submeter sem obrigatorios. | Erro de validacao. |
| CT-CMP-003 | Aprovar registro. | Status ativo e historico. |
| CT-CMP-004 | Tentar publicar evento a partir de rascunho. | Evento nao publicado. |
| CT-CMP-005 | Inativar registro referenciado. | Bloqueio ou inativacao conforme politica. |
| CT-CMP-006 | Consultar campo sensivel sem permissao. | Campo mascarado. |
| CT-CMP-007 | Registrar consentimento. | Evidencia registrada. |
| CT-CMP-008 | Abrir solicitacao de eliminacao. | Protocolo criado e workflow iniciado. |
| CT-CMP-009 | Executar retencao com bloqueio legal. | Item mantido/bloqueado e auditado. |
| CT-CMP-010 | Alterar parametro IFRS vigente. | Historico antes/depois e nova vigencia. |

## 16. Notas de rodape

1. Entidades especificas de base legal, consentimento, direitos do titular, mascaramento, auditoria de acesso, retencao, controles financeiros e IFRS foram estruturadas para tornar a especificacao implantavel porque o material trouxe esses requisitos, mas nao trouxe tabelas completas nem endpoints finais.
