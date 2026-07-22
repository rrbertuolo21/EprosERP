# MC_3_PLATAFORMA_COMPARTILHADA_INTEGRACAO_IOT_V1

**Projeto:** Epros  
**Empresa:** Siser  
**Modulo:** PLATAFORMA_COMPARTILHADA  
**Submodulo:** INTEGRACAO_IOT  
**Documento:** Matriz de completude  
**Versao:** V1  
**Status:** Concluido  
**Ultima atualizacao:** 2026-06-09

## 1. Objetivo

Registrar o nivel de completude do submodulo Integracao IoT, separando capacidades comprovadas no material, estruturas funcionais criadas para implantacao e lacunas que dependem de decisao ou levantamento.

## 2. Resumo de cobertura

| Area | Status | Evidencia funcional consolidada |
|---|---|---|
| Cadastro de dispositivo | Parcial | Material informa cadastro de dispositivo e credencial por tenant. |
| Entidade principal | Parcial | Id, TenantId, Codigo, Status e ResponsavelId informados. |
| Workflow | Parcial | Estados e transicoes principais informados. |
| Historico | Parcial | Acao, UsuarioId, PayloadJson, timestamp e IP informados. |
| Anexos | Parcial | ArquivoId via GED informado. |
| Topico segregado | Parcial | Padrao `/{tenantId}/equipamento/{id}` informado. |
| Ingestao | Incompleto | Arquitetura de ingestao citada, sem contrato final. |
| Leitura de condicao | Incompleto | Estrutura conceitual citada, sem campos completos. |
| Motor de regras | Incompleto | Motor de regras citado, sem regras finais. |
| Buffer offline e replay | Incompleto | Capacidade citada, sem janela, sequencia e regras completas. |
| Retencao serie temporal | Parcial | Retencao configuravel citada. |
| API final | Pendente | Endpoints nao informados no material. |

## 3. Itens de completude

| Codigo | Area | Status | O que existe | O que falta | Prioridade |
|---|---|---|---|---|---|
| MC-IOT-001 | Escopo | Parcial | Telemetria, dispositivos e integracao com manutencao/producao/operacoes. | Validar escopo da primeira entrega. | P0 |
| MC-IOT-002 | Dispositivo | Parcial | Id, TenantId, Codigo, Status e ResponsavelId. | Tipo, fabricante, serial, local, ativo/equipamento, unicidade e ciclo operacional. | P0 |
| MC-IOT-003 | Credencial | Parcial | Credencial por tenant citada. | Formato, rotacao, expiracao, segredo, revogacao e auditoria. | P0 |
| MC-IOT-004 | Topico | Parcial | Padrao com tenant e equipamento. | Validar padrao final, caracteres permitidos, versao e compatibilidade. | P0 |
| MC-IOT-005 | Ingestao | Incompleto | Arquitetura de ingestao citada. | Definir endpoint/broker, autenticacao, payload, erro, idempotencia e throughput. | P0 |
| MC-IOT-006 | Telemetria | Incompleto | Conceito de telemetria citado. | Definir campos obrigatorios, unidades, tipos, qualidade e validacoes. | P0 |
| MC-IOT-007 | Leitura de condicao | Incompleto | Leitura normalizada citada. | Definir dicionario completo, status, origem, historico e relacionamentos. | P0 |
| MC-IOT-008 | Motor de regras | Incompleto | Motor citado. | Definir sintaxe, operadores, limiares, periodicidade, severidade e prioridade. | P0 |
| MC-IOT-009 | Eventos | Parcial | Eventos de dominio citados. | Definir nomes, payloads, consumidores, retry, idempotencia e monitoramento. | P0 |
| MC-IOT-010 | Buffer offline | Incompleto | Buffer e replay citados. | Definir janela, sequencia, duplicidade, lote, atraso e comportamento parcial. | P1 |
| MC-IOT-011 | Retencao | Parcial | Retencao configuravel citada. | Definir prazos, expurgo, agregacao, anonimizacao e custo. | P0 |
| MC-IOT-012 | Workflow | Parcial | Estados e transicoes informados. | Definir motivos, SLA, aprovadores e bloqueios por referencia. | P1 |
| MC-IOT-013 | Telas | Parcial | Lista, formulario e painel gestor citados. | Definir campos, acoes, filtros, mensagens e permissoes. | P1 |
| MC-IOT-014 | Relatorios | Parcial | Posicao geral e auditoria citadas. | Definir indicadores, colunas, exportacao e retencao. | P1 |
| MC-IOT-015 | Manutencao | Parcial | Dependencia com manutencao citada. | Definir contrato com ativo, alerta, ordem, preditiva e trabalho. | P0 |
| MC-IOT-016 | Producao | Parcial | Dependencia com producao citada. | Definir contrato com equipamento, lote, ordem e execucao. | P1 |
| MC-IOT-017 | API Gateway | Pendente | Referencia citada. | Definir exposicao, autenticacao, limites e observabilidade. | P0 |
| MC-IOT-018 | Protocolos industriais | Pendente | MQTT citado; OPC-UA/telemetria detalhada nao informada. | Definir protocolos oficiais suportados e homologacao. | P1 |
| MC-IOT-019 | Seguranca | Incompleto | Tenant e credencial citados. | Definir criptografia, segredo, rotacao, permissao e segregacao. | P0 |
| MC-IOT-020 | Testes | Parcial | CT-IOT-001 a CT-IOT-006 informados; EF acrescenta topico e replay. | Criar massas de teste, simulador, carga, duplicidade e falhas. | P0 |

## 4. Decisoes pendentes

| Codigo | Decisao | Motivo |
|---|---|---|
| D-IOT-001 | Confirmar protocolos suportados na primeira entrega. | Define arquitetura e homologacao de dispositivos. |
| D-IOT-002 | Definir contrato de payload de telemetria. | Necessario para ingestao implantavel. |
| D-IOT-003 | Definir vinculo oficial entre dispositivo e ativo/equipamento. | Necessario para manutencao e producao. |
| D-IOT-004 | Definir regras de replay offline. | Necessario para consistencia temporal. |
| D-IOT-005 | Definir politica de retencao de serie temporal. | Necessario para custo e compliance. |
| D-IOT-006 | Definir motor de regras inicial. | Necessario para gerar eventos de dominio. |
| D-IOT-007 | Definir contratos com manutencao, producao e IA/ML. | Necessario para integracao entre modulos. |
| D-IOT-008 | Definir modelo de seguranca e rotacao de credenciais. | Necessario para operacao segura. |

## 5. Proximo passo operacional

O submodulo `PLATAFORMA_COMPARTILHADA/INTEGRACAO_IOT` foi processado e esta concluido como conteudo parcial-controlado. O proximo item da matriz principal e `PLATAFORMA_COMPARTILHADA/INTEGRACOES_E_CONECTORES`.
