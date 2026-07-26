# MC_3_PLATAFORMA_COMPARTILHADA_IA_ML_V1

**Projeto:** Epros  
**Empresa:** Siser  
**Modulo:** PLATAFORMA_COMPARTILHADA  
**Submodulo:** IA_ML  
**Documento:** Matriz de completude  
**Versao:** V1  
**Status:** Concluido  
**Ultima atualizacao:** 2026-06-09

## 1. Objetivo

Registrar o nivel de completude do submodulo IA_ML, separando o que esta comprovado no material, o que foi estruturado funcionalmente para implantacao e o que ainda precisa de decisao, levantamento ou validacao da Siser.

## 2. Resumo de cobertura

| Area | Status | Evidencia funcional consolidada |
|---|---|---|
| Escopo IA/ML | Parcial | Material cita modelos preditivos, assistentes LLM, governanca, inferencia, quotas e RAG. |
| Entidades extraidas | Incompleto | Material informa apenas agregado principal, historico e anexo com poucos campos. |
| Workflow | Parcial | Ciclo Rascunho, EmAnalise, Ativo, Inativo e Encerrado informado. |
| Auditoria | Parcial | Historico com acao, usuario, payload, timestamp e IP. |
| Anexos | Parcial | ArquivoId vinculado ao GED informado. |
| Eventos | Parcial | Publicacao de eventos de dominio apos confirmacao transacional. |
| Parametrizacao por tenant | Parcial | Regras por tenant sem deploy de codigo informadas. |
| Casos de uso | Parcial | Deduplicacao, NCM, demanda, anomalia fiscal e assistente de wizard citados. |
| API final | Pendente | Nao informado no material. |
| Provedor/modelos/algoritmos | Pendente | Nao informado no material. |
| Treinamento e metricas | Pendente | Nao informado no material. |

## 3. Itens de completude

| Codigo | Area | Status | O que existe | O que falta | Prioridade |
|---|---|---|---|---|---|
| MC-IA-001 | Escopo funcional | Parcial | Modelos, inferencia, LLM, RAG, quotas, governanca e auditoria citados. | Validar escopo definitivo da primeira entrega. | P0 |
| MC-IA-002 | Entidade principal | Parcial | Id, TenantId, Codigo, Status e ResponsavelId informados. | Definir tipos, tamanhos, unicidade, descricao, finalidade e auditoria tecnica. | P0 |
| MC-IA-003 | Historico | Parcial | Acao, UsuarioId e PayloadJson informados; fluxo cita timestamp e IP. | Definir payload padrao, retencao, consulta, mascaramento e assinatura de auditoria. | P0 |
| MC-IA-004 | Anexos | Parcial | ArquivoId via GED informado. | Definir tipos permitidos, versao, permissao, retencao e relacao com base de conhecimento. | P1 |
| MC-IA-005 | Workflow | Parcial | Estados e transicoes principais informados. | Definir motivos obrigatorios, prazos, SLA, segregacao e aprovacao por impacto. | P0 |
| MC-IA-006 | Eventos | Parcial | Eventos apos confirmacao transacional. | Definir nomes, payloads, consumidores, idempotencia, retry e auditoria. | P0 |
| MC-IA-007 | Parametrizacao por tenant | Parcial | Regras por tenant sem deploy. | Definir parametros oficiais, valores padrao, heranca, permissao e historico. | P0 |
| MC-IA-008 | Quotas | Parcial | Quotas citadas. | Definir limite, periodo, medicao, excedente, alertas, reset e relatorios. | P0 |
| MC-IA-009 | Modelos | Incompleto | Modelos preditivos citados. | Definir cadastro final, tipos, versoes, status, publicacao, metricas e responsaveis. | P0 |
| MC-IA-010 | Inferencia | Incompleto | Inferencia e API isolada citadas. | Definir contrato de entrada/saida, status, timeout, erros, reprocessamento e armazenamento. | P0 |
| MC-IA-011 | LLM e prompts | Incompleto | Assistentes LLM citados. | Definir prompts, versoes, variaveis, seguranca, revisao, limites e testes. | P0 |
| MC-IA-012 | RAG | Incompleto | RAG e GED citados. | Definir indexacao, atualizacao, permissoes, evidencias, relevancia e expiracao. | P1 |
| MC-IA-013 | Feature Store | Incompleto | Arquitetura cita Feature Store. | Definir atributos, origem, qualidade, atualizacao, linhagem e permissao. | P1 |
| MC-IA-014 | Model Registry | Incompleto | Arquitetura cita Model Registry. | Definir metadados, versoes, aprovacao, rollback, responsavel e metricas. | P0 |
| MC-IA-015 | Drift | Incompleto | Governanca cita drift. | Definir metrica, frequencia, alerta, acao corretiva e responsavel. | P1 |
| MC-IA-016 | LGPD e opt-out | Parcial | Base legal, retencao, anonimizacao e opt-out citados. | Definir regras por caso, campos sensiveis, mascaramento e trilha de decisao automatizada. | P0 |
| MC-IA-017 | Casos de uso | Parcial | Deduplicacao, NCM, demanda, anomalia fiscal e wizard citados. | Criar EF ou contrato especifico por caso com dados, regras, aceite e dono. | P0 |
| MC-IA-018 | Telas | Parcial | Lista, formulario e painel gestor citados. | Definir campos, acoes, mensagens, permissoes e estados vazios. | P1 |
| MC-IA-019 | Relatorios | Parcial | Posicao geral e auditoria citadas. | Definir colunas, filtros, exportacao, periodicidade e seguranca. | P1 |
| MC-IA-020 | Integracao API | Pendente | Material informa lacuna. | Definir endpoints, autenticacao, rate limit, erros, versao e observabilidade. | P0 |
| MC-IA-021 | Provedor externo | Pendente | Nao informado no material. | Decidir se havera provedor externo, interno ou ambos. | P1 |
| MC-IA-022 | Treinamento | Pendente | Nao informado no material. | Definir se Epros treina modelos ou apenas consome modelos. | P1 |
| MC-IA-023 | Testes | Parcial | CT-IA-001 a CT-IA-006 informados; EF acrescenta cenarios de quota e prompt. | Definir massa de dados, criterios de acuracia, seguranca e testes por caso de uso. | P0 |
| MC-IA-024 | Operacao | Incompleto | Painel gestor citado. | Definir monitoramento, filas, SLA, incidentes, fallback e suporte. | P1 |

## 4. Decisoes pendentes

| Codigo | Decisao | Motivo |
|---|---|---|
| D-IA-001 | Confirmar a primeira lista de casos de uso a construir. | Evita construir plataforma ampla sem entrega priorizada. |
| D-IA-002 | Definir se o Epros treina modelos, consome modelos externos ou opera ambos. | Define arquitetura, responsabilidades e custos. |
| D-IA-003 | Definir contratos de inferencia por modulo consumidor. | Necessario para deduplicacao, NCM, demanda, anomalia fiscal e wizard. |
| D-IA-004 | Definir politica de quotas por plano, tenant e recurso. | Necessario para operacao e custo. |
| D-IA-005 | Definir regras LGPD para inferencia automatizada e opt-out. | Necessario para conformidade. |
| D-IA-006 | Definir fornecedor, provedor ou runtime de inferencia. | Nao informado no material. |
| D-IA-007 | Definir modelo de armazenamento de prompts, resultados e payloads. | Impacta seguranca, auditoria e retencao. |
| D-IA-008 | Definir se cada caso de uso tera EF especifica propria. | Recomendado para implantacao segura. |

## 5. Proximo passo operacional

O submodulo `PLATAFORMA_COMPARTILHADA/IA_ML` foi processado e esta concluido com conteudo parcial-controlado. O proximo item da matriz principal e `PLATAFORMA_COMPARTILHADA/IMPRESSAO_TERMICA`.
