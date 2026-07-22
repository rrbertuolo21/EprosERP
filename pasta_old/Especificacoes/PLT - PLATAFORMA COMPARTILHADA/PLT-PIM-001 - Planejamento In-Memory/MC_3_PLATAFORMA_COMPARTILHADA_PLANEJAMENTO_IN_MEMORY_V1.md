# MC_3_PLATAFORMA_COMPARTILHADA_PLANEJAMENTO_IN_MEMORY_V1

**Projeto:** Epros  
**Empresa:** Siser  
**Modulo:** PLATAFORMA_COMPARTILHADA  
**Submodulo:** PLANEJAMENTO_IN_MEMORY  
**Documento:** Matriz de completude  
**Versao:** V1  
**Status:** Concluido  
**Ultima atualizacao:** 2026-06-09

## 1. Objetivo

Registrar o nivel de completude do submodulo Planejamento In Memory, separando capacidades comprovadas no material, estruturas funcionais criadas para implantacao e lacunas que dependem de validacao da Siser.

## 2. Resumo de cobertura

| Area | Status | Evidencia funcional consolidada |
|---|---|---|
| Escopo ATP/what-if | Parcial | Material informa ATP, cenarios what-if e alocacao tatica de estoque a pedidos. |
| Entidade principal | Parcial | Id, TenantId, Codigo, Status e ResponsavelId informados. |
| Workflow | Parcial | Rascunho, EmAnalise, Ativo, Inativo e Encerrado informados. |
| Historico | Parcial | Acao, UsuarioId, PayloadJson, timestamp e IP informados. |
| Anexos | Parcial | ArquivoId via GED informado. |
| Simulacao em memoria | Incompleto | Resultado nao persiste ate confirmacao informado, sem algoritmo ou tabelas. |
| Premissas | Parcial | Demanda, capacidade e lead time informados. |
| Comparacao | Parcial | Comparacao lado a lado informada. |
| Confirmacao | Parcial | Exportacao/publicacao ao confirmar informada. |
| APIs | Pendente | Endpoints finais nao informados. |
| Algoritmos | Pendente | ATP, capacidade, alocacao e priorizacao nao detalhados. |

## 3. Itens de completude

| Codigo | Area | Status | O que existe | O que falta | Prioridade |
|---|---|---|---|---|---|
| MC-PIM-001 | Escopo | Parcial | ATP, what-if, alocacao tatica, estoque, vendas e producao citados. | Confirmar escopo do MVP. | P0 |
| MC-PIM-002 | Cenario | Parcial | Id, TenantId, Codigo, Status e ResponsavelId. | Definir nome, horizonte, versao, tipo, destino e unicidade final. | P0 |
| MC-PIM-003 | Premissas | Parcial | Demanda, capacidade e lead time citados. | Definir campos, unidades, dominios, fontes e obrigatoriedade. | P0 |
| MC-PIM-004 | ATP | Incompleto | ATP citado como objetivo. | Definir algoritmo, fontes de saldo, reservas, pedidos e datas prometidas. | P0 |
| MC-PIM-005 | What-if | Parcial | Cenario what-if informado. | Definir parametros editaveis, limites, versionamento e persistencia de resultados. | P0 |
| MC-PIM-006 | Alocacao tatica | Parcial | Alocacao de estoque a pedidos citada. | Definir regra de prioridade, parcialidade, substituicao e local de estoque. | P0 |
| MC-PIM-007 | Capacidade | Parcial | Capacidade citada. | Definir recursos, calendario, gargalo, turno e unidade. | P0 |
| MC-PIM-008 | Lead time | Parcial | Lead time citado. | Definir origem, granularidade, calendario e tratamento de excecoes. | P1 |
| MC-PIM-009 | Resultado em memoria | Parcial | Resultado nao persiste ate confirmar. | Definir o que e temporario, o que fica auditado e prazo de retencao. | P0 |
| MC-PIM-010 | Comparacao | Parcial | Comparacao lado a lado informada. | Definir indicadores, limites de cenarios e visual final. | P1 |
| MC-PIM-011 | Confirmacao | Parcial | Exportacao ao confirmar informada. | Definir contrato com destino, idempotencia e rollback de falha. | P0 |
| MC-PIM-012 | Fronteira com destino | Parcial | Execucao real pertence aos modulos donos. | Formalizar contratos com Estoque, Vendas e Producao. | P0 |
| MC-PIM-013 | APIs | Pendente | Nenhum endpoint final informado. | Definir rotas, metodos, payloads, erros e versionamento. | P0 |
| MC-PIM-014 | Telas | Parcial | Lista, detalhe e painel gestor citados. | Detalhar tela de simulacao, comparacao e confirmacao. | P1 |
| MC-PIM-015 | Relatorios | Parcial | Posicao geral e auditoria citadas. | Definir relatorios de resultado, comparacao e confirmacoes. | P1 |
| MC-PIM-016 | Workflow | Parcial | Estados e transicoes informados. | Definir quando aprovacao e obrigatoria e quem aprova. | P1 |
| MC-PIM-017 | Auditoria | Parcial | Acao, UsuarioId, PayloadJson, timestamp e IP informados. | Definir payload mascarado, antes/depois, retencao e exportacao. | P0 |
| MC-PIM-018 | Testes | Parcial | Cenarios basicos informados; EF ampliou simulacao/comparacao/confirmacao. | Criar massas de dados, testes de algoritmo, desempenho e integracao. | P0 |
| MC-PIM-019 | Desempenho | Pendente | Motor em memoria citado. | Definir limites de tamanho, tempo maximo, concorrencia e cache. | P0 |
| MC-PIM-020 | Privacidade | Incompleto | LGPD citada genericamente. | Definir dados pessoais permitidos, mascaramento e retencao. | P1 |

## 4. Decisoes pendentes

| Codigo | Decisao | Motivo |
|---|---|---|
| D-PIM-001 | Confirmar modulos e casos de uso do MVP. | Define dados, algoritmo e telas. |
| D-PIM-002 | Definir algoritmo de ATP. | Necessario para resultado confiavel. |
| D-PIM-003 | Definir regra de alocacao tatica de estoque. | Necessario para pedidos e disponibilidade. |
| D-PIM-004 | Definir tratamento de capacidade e lead time. | Necessario para simulacao consistente. |
| D-PIM-005 | Definir indicadores de comparacao. | Necessario para tela lado a lado. |
| D-PIM-006 | Definir contrato de confirmacao com modulos destino. | Necessario para aplicar plano. |
| D-PIM-007 | Definir idempotencia de confirmacao. | Evita duplicidade de plano. |
| D-PIM-008 | Definir limites de desempenho do motor em memoria. | Necessario para operacao segura. |
| D-PIM-009 | Definir retencao de cenarios e resultados. | Necessario para custo e auditoria. |
| D-PIM-010 | Definir endpoints finais. | Necessario para implementacao. |

## 5. Riscos funcionais

| Risco | Impacto | Mitigacao proposta |
|---|---|---|
| Simulacao confundida com execucao real. | Estoque, pedido ou producao podem ser comprometidos indevidamente. | Bloquear efeito definitivo ate confirmacao. |
| Algoritmo ATP indefinido. | Promessa de atendimento incorreta. | Validar regra oficial antes de construir. |
| Confirmacao duplicada. | Plano aplicado mais de uma vez. | Chave idempotente e bloqueio de reconfirmacao. |
| Fronteira mal definida com modulos destino. | Duplicidade de regras e inconsistencia. | Contratos formais por destino. |
| Cenario grande sem limite. | Lentidao ou indisponibilidade. | Limites de tempo, volume e concorrencia. |
| Resultado sem explicabilidade. | Usuario nao confia na simulacao. | Registrar restricoes e premissas usadas. |

## 6. Proximo passo operacional

O submodulo `PLATAFORMA_COMPARTILHADA/PLANEJAMENTO_IN_MEMORY` foi processado e esta concluido como conteudo parcial-controlado. O proximo item da matriz principal e `PLATAFORMA_COMPARTILHADA/SDK_EXTENSOES`.
