# MC_3_PLATAFORMA_COMPARTILHADA_OFFLINE_SHELL_V1

**Projeto:** Epros  
**Empresa:** Siser  
**Modulo:** PLATAFORMA_COMPARTILHADA  
**Submodulo:** OFFLINE_SHELL  
**Documento:** Matriz de completude  
**Versao:** V1  
**Status:** Concluido  
**Ultima atualizacao:** 2026-06-09

## 1. Objetivo

Registrar o nivel de completude do submodulo Offline Shell, separando capacidades comprovadas no material, estruturas funcionais criadas para implantacao e lacunas que precisam de decisao da Siser.

## 2. Resumo de cobertura

| Area | Status | Evidencia funcional consolidada |
|---|---|---|
| Escopo offline | Parcial | Material informa fila offline, sincronizacao, conflitos e seguranca no dispositivo. |
| Entidade principal | Parcial | Id, TenantId, Codigo, Status e ResponsavelId informados. |
| Workflow | Parcial | Rascunho, EmAnalise, Ativo, Inativo e Encerrado informados. |
| Historico | Parcial | Acao, UsuarioId, PayloadJson, timestamp e IP informados. |
| Anexos | Parcial | ArquivoId via GED informado. |
| Fila local | Incompleto | Outbox por tenant informado, sem tabelas ou payload final. |
| Sincronizacao | Incompleto | Sync background e retry exponencial informados, sem algoritmo final. |
| Conflitos | Incompleto | Conflito, last-write-wins configuravel e resolucao manual informados. |
| Modo somente leitura | Parcial | Indicador e modo leitura informados. |
| APIs | Pendente | Endpoints finais nao informados. |
| Privacidade local | Incompleto | LGPD citada, sem retencao, criptografia ou expurgo final. |

## 3. Itens de completude

| Codigo | Area | Status | O que existe | O que falta | Prioridade |
|---|---|---|---|---|---|
| MC-OFF-001 | Escopo | Parcial | Operacao offline, fila local, sincronizacao e conflitos. | Validar quais modulos entram no MVP. | P0 |
| MC-OFF-002 | Configuracao | Parcial | Id, TenantId, Codigo, Status e ResponsavelId. | Definir parametros finais por tenant/modulo. | P0 |
| MC-OFF-003 | Workflow | Parcial | Estados e transicoes principais. | Definir se configuracao offline exige aprovacao sempre ou apenas em modulos criticos. | P1 |
| MC-OFF-004 | Fila local | Incompleto | Outbox por tenant citada. | Definir payload, chave idempotente, prioridade, limites e armazenamento local. | P0 |
| MC-OFF-005 | Armazenamento local | Incompleto | Armazenamento local e LGPD citados. | Definir tecnologia, criptografia, expurgo, limites e dados permitidos. | P0 |
| MC-OFF-006 | Sincronizacao | Incompleto | Sync background e replay citados. | Definir gatilhos, ordem, dependencia entre itens, lote e confirmacao. | P0 |
| MC-OFF-007 | Retry | Parcial | Retry exponencial informado. | Definir intervalo base, limite, jitter, erro transitorio e erro definitivo. | P0 |
| MC-OFF-008 | Idempotencia | Parcial | APIs idempotentes para replay informadas. | Definir padrao de chave idempotente e resposta para duplicidade. | P0 |
| MC-OFF-009 | Conflito | Parcial | SyncConflict, last-write-wins configuravel e resolucao manual informados. | Definir comparacao de versao, entidades criticas, tela e regras de decisao. | P0 |
| MC-OFF-010 | Entidades criticas | Parcial | Estoque e venda citados como exemplos. | Confirmar lista oficial de entidades criticas. | P0 |
| MC-OFF-011 | Modo somente leitura | Parcial | Indicador e modo leitura informados. | Definir regras por modulo e mensagens ao usuario. | P1 |
| MC-OFF-012 | Indicador | Parcial | Indicador de conectividade informado. | Definir estados visuais, contadores e comportamento por dispositivo. | P1 |
| MC-OFF-013 | Auditoria | Parcial | Acao, UsuarioId, PayloadJson, timestamp e IP informados. | Definir payload mascarado, antes/depois, retencao e exportacao. | P0 |
| MC-OFF-014 | Anexos | Parcial | ArquivoId via GED informado. | Definir se operacoes offline podem anexar arquivos e como sincronizar. | P1 |
| MC-OFF-015 | APIs | Pendente | Dependencia com APIs idempotentes citada. | Definir endpoints, contratos, codigos de erro e versionamento. | P0 |
| MC-OFF-016 | Telas | Parcial | Lista, detalhe e painel gestor citados; EF acrescenta fila/conflito. | Detalhar campos, filtros, acoes e permissoes. | P1 |
| MC-OFF-017 | Relatorios | Parcial | Posicao geral e auditoria citadas. | Definir relatorios de sincronizacao, conflitos e falhas. | P1 |
| MC-OFF-018 | Testes | Parcial | Cenarios basicos informados. | Criar testes de replay, duplicidade, conflito, offline real, armazenamento e expurgo. | P0 |
| MC-OFF-019 | Seguranca | Incompleto | Tenant e LGPD citados. | Definir criptografia local, sessao offline, revogacao, bloqueio e wipe remoto se aplicavel. | P0 |
| MC-OFF-020 | Observabilidade | Incompleto | KPIs citados genericamente. | Definir metricas de fila, duracao, sucesso, falha, conflitos e abandono. | P1 |

## 4. Decisoes pendentes

| Codigo | Decisao | Motivo |
|---|---|---|
| D-OFF-001 | Confirmar modulos do MVP offline. | Define escopo de fila e conflitos. |
| D-OFF-002 | Definir operacoes permitidas offline por modulo. | Evita enfileirar operacoes inseguras. |
| D-OFF-003 | Definir padrao de chave idempotente. | Necessario para replay sem duplicidade. |
| D-OFF-004 | Definir tecnologia e politica de armazenamento local. | Necessario para seguranca e privacidade. |
| D-OFF-005 | Definir limites de fila local e payload. | Necessario para desempenho do dispositivo. |
| D-OFF-006 | Definir retry exponencial final. | Necessario para sincronizacao previsivel. |
| D-OFF-007 | Definir entidades criticas e politica de conflito. | Necessario para resolucao segura. |
| D-OFF-008 | Definir tela de resolucao de conflitos. | Necessario para operacao humana. |
| D-OFF-009 | Definir retencao e expurgo de dados locais. | Necessario para Compliance. |
| D-OFF-010 | Definir contratos de API idempotente com modulos donos. | Necessario para integracao. |

## 5. Riscos funcionais

| Risco | Impacto | Mitigacao proposta |
|---|---|---|
| Replay sem idempotencia. | Duplicidade de venda, estoque ou financeiro. | Exigir chave idempotente por item. |
| Operacao offline em modulo inadequado. | Dado inconsistente. | Politica por modulo e modo somente leitura. |
| Conflito automatico indevido. | Perda de dado correto. | Entidades criticas com resolucao manual. |
| Dados locais sem protecao. | Exposicao de informacao sensivel. | Criptografia, mascaramento, retencao e expurgo. |
| Retry sem limite. | Consumo de bateria, rede e servidor. | Limite de tentativas e erro definitivo. |
| Falhas invisiveis. | Usuario acredita que dado foi sincronizado. | Indicador, painel de fila e alertas. |

## 6. Proximo passo operacional

O submodulo `PLATAFORMA_COMPARTILHADA/OFFLINE_SHELL` foi processado e esta concluido como conteudo parcial-controlado. O proximo item da matriz principal e `PLATAFORMA_COMPARTILHADA/PLANEJAMENTO_IN_MEMORY`.
