# EF 3 - PLATAFORMA COMPARTILHADA / WORKFLOW V1

## 1. Controle do documento

| Item | Valor |
|---|---|
| Sistema | Epros |
| Empresa | Siser |
| Modulo | PLATAFORMA_COMPARTILHADA |
| Submodulo | WORKFLOW |
| Versao | V1 |
| Data | 2026-06-11 |
| Status | Concluido |
| Conteudo analisado | 11 documentos canonicos do submodulo |
| Classificacao | Com conteudo parcial-controlado |

## 2. Objetivo funcional

O submodulo Workflow define o motor transversal do Epros para aprovacoes, tarefas humanas, filas de aprovacao, historico de transicoes, anexos, comentarios de aprovacao, eventos de dominio e execucao agendada de jobs operacionais.

O objetivo e permitir que modulos de negocio usem um padrao comum para submeter registros, aprovar, rejeitar, inativar, encerrar, reativar, auditar transicoes e disparar eventos apos confirmacao transacional.

## 3. Escopo funcional

| Area | Descricao |
|---|---|
| Workflow de aprovacao | Estados, eventos e permissoes para rascunho, analise, aprovacao, rejeicao, ativacao, inativacao e encerramento. |
| Tarefas humanas | Itens atribuiveis a operador, aprovador ou gestor, com responsavel e periodo. |
| Solicitacao com aprovacao | Caso funcional com datas, motivo, anexo, status, aprovador, comentario e data de aprovacao. |
| Historico | Registro de transicoes com usuario, data/hora, IP, antes/depois e payload quando aplicavel. |
| Eventos de dominio | Publicacao de evento apos commit transacional. |
| Parametrizacao por empresa | Regras configuraveis por tenant sem deploy de codigo. |
| Agendamentos | Cadastro de agenda intervalar e execucao de jobs recorrentes. |
| Fila de jobs | Controle de sucesso, falha, retry, adiamento, falha final e resolucao. |
| Telas e relatorios | Lista, detalhe, painel gestor, posicao geral e auditoria. |

## 4. Fora de escopo

| Item | Tratamento |
|---|---|
| BPMN completo | Nao informado no material. |
| Desenho visual de processos | Nao informado no material. |
| Alcadas financeiras completas | Devem ser definidas nos modulos financeiros e na MC. |
| Segregacao detalhada de funcoes | Deve ser integrada ao modulo de governanca e permissoes. |
| Regras finais de cada modulo | O workflow fornece motor comum; cada modulo define gatilhos e campos proprios. |

## 5. Atores e responsabilidades

| Ator | Responsabilidades |
|---|---|
| Operador | Criar registro, manter rascunho e submeter para analise. |
| Aprovador | Aprovar ou rejeitar registros em analise. |
| Gestor | Inativar, encerrar ou reativar registros conforme permissao. |
| Responsavel | Receber tarefa humana e executar acao pendente. |
| Administrador | Parametrizar tipos de workflow, agendas, jobs, permissoes e paineis. |
| Processo automatico | Executar jobs agendados, registrar resultado, retry e falha final. |
| Auditor | Consultar historico, antes/depois, IP, usuario e eventos. |

## 6. Conceitos funcionais

| Conceito | Definicao |
|---|---|
| Definicao de workflow | Configuracao reutilizavel de estados, transicoes, permissoes e eventos. |
| Instancia de workflow | Execucao aplicada a um registro de negocio especifico. |
| Transicao | Mudanca de estado causada por evento permitido. |
| Tarefa humana | Pendencia atribuida a usuario ou papel para completar etapa. |
| Aprovacao | Transicao positiva executada por aprovador. |
| Rejeicao | Transicao que retorna registro ao rascunho com motivo ou comentario. |
| Job agendado | Execucao automatica recorrente baseada em agenda intervalar. |
| Retry | Nova tentativa apos falha de job. |
| Falha final | Encerramento de job apos esgotar politica de tentativas. |

## 7. Regras funcionais

### 7.1 Ciclo de vida principal

1. O registro controlado inicia em Rascunho.
2. Usuario com permissao de operador pode submeter Rascunho para EmAnalise.
3. Usuario com permissao de aprovador pode aprovar registro EmAnalise.
4. A aprovacao move o registro para Ativo.
5. Usuario com permissao de aprovador pode rejeitar registro EmAnalise.
6. A rejeicao retorna o registro para Rascunho.
7. Rejeicao deve registrar motivo ou comentario quando informado.
8. Usuario com permissao de gestor pode inativar registro Ativo.
9. Usuario com permissao de gestor pode encerrar registro Ativo.
10. Usuario com permissao de gestor pode reativar registro Inativo.
11. Toda transicao deve registrar historico com usuario, data/hora e IP.
12. Eventos de dominio devem ser publicados apos commit transacional.

### 7.2 Solicitacao aprovada

1. A solicitacao pode conter data inicial, data final, total de dias, motivo, anexo, status, comentario do aprovador, data de aprovacao, colaborador, tipo, aprovador, criador e usuario de criacao.
2. Data inicial e data final devem ser tratadas como datas.
3. Data de aprovacao deve ser tratada como data-hora.
4. Status previsto no material para solicitacao: pending, approved e rejected.
5. A aprovacao deve registrar aprovador e data de aprovacao.
6. A rejeicao pode registrar comentario do aprovador.
7. Anexo e motivo podem acompanhar a solicitacao.
8. Colaborador e tipo de solicitacao devem referenciar cadastros do modulo dono.

### 7.3 Parametrizacao por empresa

1. O workflow deve permitir parametrizacao por tenant sem deploy de codigo.
2. Regras de workflow devem referenciar cadastros mestres por identificador, sem duplicar dados.
3. Operacoes devem exigir tenant em escopo multiempresa.
4. Alteracoes devem respeitar auditoria transversal do Epros.
5. Quando o registro envolver parceiro, a integracao deve referenciar cadastro de pessoa/organizacao.
6. Quando houver impacto financeiro, o workflow de aprovacao deve ser aplicado conforme regra do modulo dono.

### 7.4 Agendamentos

1. O Epros deve permitir agenda baseada em expressao intervalar no formato funcional `mins::hours::day_of_month::months::day_of_week`.
2. A agenda deve ser validada antes de salvar.
3. O calculo de proximas execucoes deve ser feito por servico dedicado.
4. O agendamento deve possuir status ativo/inativo.
5. O enfileiramento deve ocorrer apenas para agendamento ativo.
6. O enfileiramento deve evitar pendencia duplicada quando ja existir job pendente para a mesma agenda.
7. Jobs padrao podem ser reconstruidos por rotina administrativa quando parametrizado.

### 7.5 Fila de jobs

1. Job deve possuir agenda, nome, status, tentativa, usuario/contexto, data prevista, inicio, fim e log.
2. Job pode ser resolvido como sucesso.
3. Job pode ser resolvido como falha.
4. Falha pode gerar retry conforme politica.
5. Retry deve preservar historico de tentativas.
6. Falha final deve encerrar o job quando nao houver nova tentativa.
7. Job pode ser adiado quando a regra permitir.
8. A execucao deve trocar contexto de usuario quando a agenda exigir.
9. A execucao deve encerrar de forma segura, registrando log e status.
10. O mecanismo de retry deve ficar separado da regra do dominio executada.

### 7.6 Consulta, filtros e exportacao

1. A lista de workflows deve permitir filtros por status, periodo e responsavel.
2. A lista deve ter paginacao.
3. A lista deve permitir exportacao quando o usuario tiver permissao.
4. A tela de detalhe deve apresentar dados, historico, anexos e aprovacao.
5. O painel gestor deve apresentar KPIs e fila de aprovacao.
6. Relatorio de posicao geral deve consolidar registros por status.
7. Relatorio de auditoria deve permitir trilha por periodo.

### 7.7 Auditoria e LGPD

1. Toda alteracao deve registrar usuario, data, antes e depois quando aplicavel.
2. Toda transicao deve registrar IP quando disponivel.
3. Payload de auditoria deve ser mascarado quando contiver dado sensivel.
4. Anexos devem ser referenciados pelo repositorio documental do Epros quando formalizados.
5. Historico de workflow deve ser imutavel para usuarios operacionais.

## 8. Fluxos funcionais

### 8.1 Fluxo de aprovacao

| Passo | Acao | Resultado |
|---|---|---|
| 1 | Operador cria registro | Registro fica em Rascunho. |
| 2 | Operador submete | Registro passa para EmAnalise. |
| 3 | Aprovador avalia | Aprovador pode aprovar ou rejeitar. |
| 4 | Aprovador aprova | Registro passa para Ativo e evento e publicado apos commit. |
| 5 | Aprovador rejeita | Registro retorna para Rascunho com comentario/motivo quando informado. |
| 6 | Gestor encerra ou inativa | Registro passa para Encerrado ou Inativo. |
| 7 | Gestor reativa | Registro Inativo volta para Ativo. |

### 8.2 Fluxo de solicitacao aprovada

| Passo | Acao | Resultado |
|---|---|---|
| 1 | Usuario cria solicitacao | Datas, motivo, tipo, colaborador e anexo sao registrados quando informados. |
| 2 | Usuario envia para aprovacao | Solicitacao fica pendente. |
| 3 | Aprovador aprova | Epros registra aprovador, data de aprovacao e status aprovado. |
| 4 | Aprovador rejeita | Epros registra status rejeitado e comentario quando informado. |

### 8.3 Fluxo de job agendado

| Passo | Acao | Resultado |
|---|---|---|
| 1 | Administrador configura agenda | Epros valida expressao intervalar. |
| 2 | Processo verifica agenda | Epros calcula proximas execucoes. |
| 3 | Agenda ativa sem pendencia | Epros cria job. |
| 4 | Job executa | Epros registra inicio, contexto e log. |
| 5 | Job conclui | Epros marca sucesso. |
| 6 | Job falha | Epros aplica retry, adiamento ou falha final. |

## 9. Estados funcionais

| Entidade | Estados |
|---|---|
| Registro controlado | Rascunho, EmAnalise, Ativo, Inativo, Encerrado |
| Solicitacao aprovada | pending, approved, rejected |
| Definicao de workflow | Rascunho, Ativo, Inativo |
| Instancia de workflow | Rascunho, EmAnalise, Ativo, Rejeitado, Inativo, Encerrado |
| Tarefa humana | Aberta, EmExecucao, Concluida, Cancelada |
| Agenda | Ativa, Inativa |
| Job | Pendente, EmExecucao, Sucesso, Falha, Adiado, FalhaFinal |

## 10. Modelo de dados funcional e implantavel

### 10.1 Visao geral das entidades

| Entidade | Finalidade |
|---|---|
| wf_definicao | Configuracao de workflow por tenant e modulo. |
| wf_estado | Estados permitidos em uma definicao. |
| wf_transicao | Eventos, origem, destino e permissao. |
| wf_instancia | Execucao do workflow sobre registro de negocio. |
| wf_tarefa | Tarefa humana atribuida a usuario ou papel. |
| wf_solicitacao | Caso de solicitacao com aprovacao e campos preservados do material. |
| wf_historico | Historico imutavel de transicoes e alteracoes. |
| wf_anexo | Anexos relacionados a instancia, tarefa ou solicitacao. |
| wf_evento_dominio | Eventos publicados apos commit. |
| wf_agendamento | Agenda intervalar de jobs. |
| wf_job | Execucao de job agendado. |
| wf_job_tentativa | Tentativas, retry e falhas de job. |
| wf_parametro | Parametros por tenant. |

### 10.2 Relacionamentos principais

| Origem | Relacao | Destino |
|---|---|---|
| wf_definicao | possui | wf_estado |
| wf_definicao | possui | wf_transicao |
| wf_instancia | usa | wf_definicao |
| wf_instancia | possui | wf_tarefa |
| wf_instancia | possui | wf_historico |
| wf_instancia | pode possuir | wf_anexo |
| wf_solicitacao | pode possuir | wf_anexo |
| wf_instancia | pode gerar | wf_evento_dominio |
| wf_agendamento | gera | wf_job |
| wf_job | possui | wf_job_tentativa |

### 10.3 Entidade wf_definicao

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| id | inteiro | Nao informado no material | Sim | PK | Identificador. |
| tenant_id | inteiro | Nao informado no material | Sim | FK empresa | Empresa da definicao. |
| modulo | texto | Nao informado no material | Sim | indice | Modulo dono. |
| entidade | texto | Nao informado no material | Sim | indice | Entidade controlada. |
| nome | texto | Nao informado no material | Sim |  | Nome funcional. |
| versao | inteiro | Nao informado no material | Sim |  | Versionamento da definicao. |
| status | enum | Rascunho, Ativo, Inativo | Sim | indice | Situacao. |
| criado_por_usuario_id | inteiro | Nao informado no material | Sim | FK usuario | Criador. |
| criado_em | data-hora | Nao informado no material | Sim |  | Criacao. |

### 10.4 Entidade wf_estado

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| id | inteiro | Nao informado no material | Sim | PK | Identificador. |
| definicao_id | inteiro | Nao informado no material | Sim | FK wf_definicao | Definicao. |
| codigo | texto | Rascunho, EmAnalise, Ativo, Inativo, Encerrado ou Nao informado no material | Sim | UK por definicao | Codigo do estado. |
| nome | texto | Nao informado no material | Sim |  | Nome exibido. |
| inicial | booleano | true/false | Sim |  | Rascunho e inicial no ciclo principal. |
| final | booleano | true/false | Sim |  | Indica encerramento. |
| ativo | booleano | true/false | Sim |  | Estado habilitado. |

### 10.5 Entidade wf_transicao

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| id | inteiro | Nao informado no material | Sim | PK | Identificador. |
| definicao_id | inteiro | Nao informado no material | Sim | FK wf_definicao | Definicao. |
| estado_origem_id | inteiro | Nao informado no material | Sim | FK wf_estado | Estado atual. |
| estado_destino_id | inteiro | Nao informado no material | Sim | FK wf_estado | Proximo estado. |
| evento | texto | Submeter, Aprovar, Rejeitar, Inativar, Encerrar, Reativar | Sim | indice | Evento permitido. |
| permissao_requerida | texto | Operador, Aprovador, Gestor ou Nao informado no material | Sim |  | Papel/acao exigida. |
| exige_comentario | booleano | true/false | Nao |  | Nao informado como obrigatorio; rejeicao aceita comentario. |
| publica_evento | booleano | true/false | Sim |  | Evento apos commit quando habilitado. |

### 10.6 Entidade wf_instancia

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| id | inteiro | Nao informado no material | Sim | PK | Identificador. |
| tenant_id | inteiro | Nao informado no material | Sim | FK empresa | Empresa. |
| definicao_id | inteiro | Nao informado no material | Sim | FK wf_definicao | Definicao usada. |
| entidade_tipo | texto | Nao informado no material | Sim | indice | Entidade controlada. |
| entidade_id | inteiro/texto | Nao informado no material | Sim | indice | Registro controlado. |
| estado_atual_id | inteiro | Nao informado no material | Sim | FK wf_estado | Estado atual. |
| responsavel_usuario_id | inteiro | Nao informado no material | Nao | FK usuario | Responsavel atual. |
| status | texto | Conforme estado | Sim | indice | Estado funcional. |
| criado_em | data-hora | Nao informado no material | Sim |  | Criacao. |
| atualizado_em | data-hora | Nao informado no material | Nao |  | Alteracao. |

### 10.7 Entidade wf_tarefa

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| id | inteiro | Nao informado no material | Sim | PK | Identificador. |
| instancia_id | inteiro | Nao informado no material | Sim | FK wf_instancia | Instancia. |
| titulo | texto | Nao informado no material | Sim |  | Tarefa humana. |
| responsavel_usuario_id | inteiro | Nao informado no material | Nao | FK usuario | Responsavel individual. |
| responsavel_papel | texto | Operador, Aprovador, Gestor ou Nao informado no material | Nao |  | Responsavel por papel. |
| status | enum | Aberta, EmExecucao, Concluida, Cancelada | Sim | indice | Situacao. |
| prazo_em | data-hora | Nao informado no material | Nao |  | SLA nao informado. |
| concluida_em | data-hora | Nao informado no material | Nao |  | Conclusao. |

### 10.8 Entidade wf_solicitacao

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| id | inteiro | Nao informado no material | Sim | PK | Identificador. |
| tenant_id | inteiro | Nao informado no material | Sim | FK empresa | Empresa. |
| start_date | data | Nao informado no material | Nao |  | Data inicial preservada. |
| end_date | data | Nao informado no material | Nao |  | Data final preservada. |
| total_days | decimal | Nao informado no material | Nao |  | Total de dias. |
| reason | texto | Nao informado no material | Nao |  | Motivo. |
| attachment | texto | Nao informado no material | Nao |  | Referencia de anexo. |
| status | enum | pending, approved, rejected | Sim | indice | Status previsto no material. |
| approver_comment | texto | Nao informado no material | Nao |  | Comentario de aprovador. |
| approved_at | data-hora | Nao informado no material | Nao |  | Data de aprovacao. |
| employee_id | inteiro/texto | Nao informado no material | Nao | FK colaborador | Colaborador. |
| leave_type_id | inteiro/texto | Nao informado no material | Nao | FK tipo | Tipo de solicitacao. |
| approved_by | inteiro/texto | Nao informado no material | Nao | FK usuario | Aprovador. |
| creator_id | inteiro/texto | Nao informado no material | Nao | FK usuario | Criador. |
| created_by | inteiro/texto | Nao informado no material | Nao | FK usuario | Usuario de criacao. |

### 10.9 Entidade wf_historico

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| id | inteiro | Nao informado no material | Sim | PK | Identificador. |
| tenant_id | inteiro | Nao informado no material | Sim | FK empresa | Empresa. |
| instancia_id | inteiro | Nao informado no material | Nao | FK wf_instancia | Instancia quando aplicavel. |
| entidade_tipo | texto | Nao informado no material | Sim | indice | Entidade auditada. |
| entidade_id | inteiro/texto | Nao informado no material | Sim | indice | Registro auditado. |
| acao | texto | Nao informado no material | Sim |  | Acao/transicao. |
| estado_anterior | texto | Nao informado no material | Nao |  | Antes. |
| estado_novo | texto | Nao informado no material | Nao |  | Depois. |
| usuario_id | inteiro | Nao informado no material | Sim | FK usuario | Executor. |
| ip_origem | texto | Nao informado no material | Nao |  | IP. |
| payload_json | texto/json | Nao informado no material | Nao |  | Antes/depois e dados adicionais. |
| criado_em | data-hora | Nao informado no material | Sim |  | Momento. |

### 10.10 Entidade wf_agendamento

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| id | inteiro | Nao informado no material | Sim | PK | Identificador. |
| tenant_id | inteiro | Nao informado no material | Nao | FK empresa | Pode ser central ou por tenant. |
| nome | texto | Nao informado no material | Sim |  | Nome da agenda. |
| expressao_intervalar | texto | mins::hours::day_of_month::months::day_of_week | Sim |  | Formato informado no material. |
| ativo | booleano | true/false | Sim | indice | Apenas ativo enfileira. |
| proxima_execucao_em | data-hora | Nao informado no material | Nao | indice | Calculada por servico. |
| criado_em | data-hora | Nao informado no material | Sim |  | Criacao. |

### 10.11 Entidade wf_job

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| id | inteiro | Nao informado no material | Sim | PK | Identificador. |
| agendamento_id | inteiro | Nao informado no material | Sim | FK wf_agendamento | Agenda. |
| nome | texto | Nao informado no material | Sim |  | Nome do job. |
| status | enum | Pendente, EmExecucao, Sucesso, Falha, Adiado, FalhaFinal | Sim | indice | Situacao. |
| tentativa_atual | inteiro | Nao informado no material | Sim |  | Tentativa corrente. |
| contexto_usuario_id | inteiro | Nao informado no material | Nao | FK usuario | Usuario de execucao. |
| previsto_para | data-hora | Nao informado no material | Sim | indice | Execucao prevista. |
| iniciado_em | data-hora | Nao informado no material | Nao |  | Inicio. |
| finalizado_em | data-hora | Nao informado no material | Nao |  | Fim. |
| log | texto | Nao informado no material | Nao |  | Resultado. |

### 10.12 Entidade wf_job_tentativa

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| id | inteiro | Nao informado no material | Sim | PK | Identificador. |
| job_id | inteiro | Nao informado no material | Sim | FK wf_job | Job. |
| numero_tentativa | inteiro | Nao informado no material | Sim |  | Sequencia. |
| status | enum | Sucesso, Falha, Retry, FalhaFinal | Sim | indice | Resultado da tentativa. |
| mensagem | texto | Nao informado no material | Nao |  | Mensagem de erro ou sucesso. |
| iniciado_em | data-hora | Nao informado no material | Nao |  | Inicio. |
| finalizado_em | data-hora | Nao informado no material | Nao |  | Fim. |

### 10.13 Entidades wf_anexo, wf_evento_dominio e wf_parametro

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| id | inteiro | Nao informado no material | Sim | PK | Identificador. |
| tenant_id | inteiro | Nao informado no material | Sim | FK empresa | Empresa. |
| entidade_tipo | texto | instancia, tarefa, solicitacao, evento, parametro | Sim | indice | Tipo de entidade vinculada. |
| entidade_id | inteiro/texto | Nao informado no material | Sim | indice | Registro vinculado. |
| chave | texto | Nao informado no material | Condicional |  | Usado para parametro/evento. |
| valor | texto/json | Nao informado no material | Condicional |  | Valor parametrizado ou payload. |
| arquivo_id | inteiro/texto | Nao informado no material | Condicional | FK GED | Usado para anexo. |
| criado_em | data-hora | Nao informado no material | Sim |  | Criacao. |

## 11. Dicionario de dados implantavel

### 11.1 Campos transversais

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| id | inteiro | Nao informado no material | Sim | PK | Identificador unico. |
| tenant_id | inteiro | Nao informado no material | Sim quando escopo empresa | FK empresa | Isolamento por tenant. |
| status | texto/enum | Conforme entidade | Sim | indice | Estado funcional. |
| criado_em | data-hora | Nao informado no material | Sim |  | Data de criacao. |
| atualizado_em | data-hora | Nao informado no material | Nao |  | Data de alteracao. |
| usuario_id | inteiro | Nao informado no material | Sim quando houver acao | FK usuario | Usuario executor. |
| payload_json | texto/json | Nao informado no material | Nao |  | Antes/depois e dados complementares. |

### 11.2 Campos de solicitacao

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| start_date | data | Nao informado no material | Nao |  | Data inicial. |
| end_date | data | Nao informado no material | Nao |  | Data final. |
| total_days | decimal | Nao informado no material | Nao |  | Total de dias. |
| reason | texto | Nao informado no material | Nao |  | Motivo. |
| attachment | texto | Nao informado no material | Nao |  | Referencia de anexo. |
| approver_comment | texto | Nao informado no material | Nao |  | Comentario. |
| approved_at | data-hora | Nao informado no material | Nao |  | Data de aprovacao. |
| approved_by | inteiro/texto | Nao informado no material | Nao | FK usuario | Aprovador. |

### 11.3 Campos de job

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| expressao_intervalar | texto | mins::hours::day_of_month::months::day_of_week | Sim |  | Agenda recorrente. |
| proxima_execucao_em | data-hora | Nao informado no material | Nao | indice | Calculada. |
| tentativa_atual | inteiro | Nao informado no material | Sim |  | Tentativa corrente. |
| previsto_para | data-hora | Nao informado no material | Sim | indice | Data planejada. |
| log | texto | Nao informado no material | Nao |  | Resultado/falha. |

## 12. Integracoes funcionais

| Modulo | Integracao |
|---|---|
| GED | Anexos de solicitacoes, tarefas e aprovacoes. |
| Compliance | Auditoria, mascaramento e trilha antes/depois. |
| Cadastros Base | Parceiros, colaboradores e pessoas envolvidas. |
| Financeiro | Aprovacoes com impacto financeiro, quando o modulo dono exigir. |
| RH | Caso de uso de solicitacao aprovada com colaborador e tipo. |
| Assinatura Eletronica | Contratos podem ser ativados apos etapa de assinatura quando o modulo dono exigir. |
| Governanca | Segregacao de aprovador e controle de papeis. |
| SOA Colaboracao | Comentarios, notificacoes e fila de aprovacao quando integrados. |

## 13. Telas e relatorios

| Tela/visao | Conteudo esperado |
|---|---|
| Lista de workflows | Status, periodo, responsavel, novo e exportar. |
| Detalhe/formulario | Dados, historico, anexos e aprovacao. |
| Painel gestor | KPIs, fila de aprovacao e pendencias. |
| Cadastro de agendamentos | Edicao, gravacao, listagem e busca de agendas. |
| Subpainel de jobs | Execucoes, status, tentativas, logs e retry. |

| Relatorio | Descricao |
|---|---|
| Posicao geral | Snapshot por status. |
| Auditoria de alteracoes | Trilha por periodo. |
| Jobs e agendamentos | Sucessos, falhas, retries e falhas finais. |

## 14. Criterios de aceite

| Codigo | Criterio |
|---|---|
| WF-CA-001 | Registro criado deve iniciar em Rascunho. |
| WF-CA-002 | Submissao deve mover Rascunho para EmAnalise apenas por operador. |
| WF-CA-003 | Aprovacao deve mover EmAnalise para Ativo apenas por aprovador. |
| WF-CA-004 | Rejeicao deve mover EmAnalise para Rascunho e registrar comentario/motivo quando informado. |
| WF-CA-005 | Inativacao e encerramento devem exigir gestor. |
| WF-CA-006 | Reativacao deve mover Inativo para Ativo apenas por gestor. |
| WF-CA-007 | Toda transicao deve criar historico com usuario, data/hora e IP quando disponivel. |
| WF-CA-008 | Evento de dominio deve ser publicado somente apos commit transacional. |
| WF-CA-009 | Solicitacao aprovada deve registrar approved_by e approved_at. |
| WF-CA-010 | Status de solicitacao deve aceitar pending, approved e rejected. |
| WF-CA-011 | Agenda ativa nao deve enfileirar job duplicado pendente. |
| WF-CA-012 | Job com falha deve aplicar retry, adiamento ou falha final conforme politica. |
| WF-CA-013 | Lista deve filtrar por status, periodo e responsavel. |
| WF-CA-014 | Auditoria deve mascarar dado sensivel quando aplicavel. |

## 15. Pontos de decisao encaminhados para MC

1. Definir catalogo final de modulos que usam workflow na V1.
2. Definir modelo de alcadas e segregacao de funcoes.
3. Definir se o Epros tera BPMN/desenho visual no futuro.
4. Definir SLA, escalonamento e notificacoes de tarefa.
5. Definir politica de retry, maximo de tentativas e adiamento de jobs.
6. Definir APIs finais do motor de workflow.

## 16. Notas de elaboracao

[^1]: Foram criados nomes funcionais padronizados para tabelas do Epros com prefixo `wf_`, pois o material informa regras, estados e alguns campos, mas nao apresenta nomenclatura fisica definitiva para a nova base.
[^2]: O modelo generico de definicao, instancia, estado e transicao foi criado para estruturar o motor transversal citado no material; as regras obrigatorias foram mantidas apenas quando informadas.
