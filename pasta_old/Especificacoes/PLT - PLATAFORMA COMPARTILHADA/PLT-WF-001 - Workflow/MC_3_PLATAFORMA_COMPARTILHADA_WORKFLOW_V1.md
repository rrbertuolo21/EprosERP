# MC 3 - PLATAFORMA COMPARTILHADA / WORKFLOW V1

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

## 2. Resumo de completude

| Area | Status | Evidencia funcional consolidada | Pendencia |
|---|---|---|---|
| Ciclo de aprovacao | Concluido | Estados Rascunho, EmAnalise, Ativo, Inativo e Encerrado; transicoes e permissoes foram especificadas. | Validar variantes por modulo. |
| Solicitacao aprovada | Concluido | Campos de datas, motivo, anexo, status, aprovador, comentario e colaborador foram especificados. | Confirmar modulo dono e obrigatoriedade final. |
| Auditoria | Concluido | Historico com usuario, data/hora, IP, antes/depois e evento apos commit foi especificado. | Confirmar retencao e mascaramento por campo. |
| Agendamentos | Concluido | Expressao intervalar, agenda ativa e bloqueio de pendencia duplicada foram especificados. | Definir validador e calendario final. |
| Fila de jobs | Concluido | Sucesso, falha, retry, adiamento, falha final e contexto foram especificados. | Definir politica de tentativas. |
| Telas e relatorios | Concluido | Lista, detalhe, painel gestor, agendas, jobs, posicao geral e auditoria foram especificados. | Definir layout final e filtros completos. |
| APIs | Pendente | O material informa lacuna de APIs. | Especificar contratos REST/eventos. |
| Modelo de dados | Concluido | 13 entidades funcionais e dicionario implantavel foram definidos. | Validar nomes fisicos antes da modelagem tecnica. |

## 3. Matriz de lacunas funcionais

| ID | Capacidade esperada | Status | O que falta construir ou validar | Impacto se nao resolver |
|---|---|---|---|---|
| MC-WF-001 | Catalogo de modulos com workflow | Pendente | Definir quais modulos usam o motor na V1. | Motor pode nascer amplo demais ou insuficiente. |
| MC-WF-002 | Tipos de workflow | Pendente | Definir tipos padrao: aprovacao simples, solicitacao, tarefa, job e outros. | Dificulta parametrizacao por tenant. |
| MC-WF-003 | Alcadas | Pendente | Definir valores, papeis, niveis e criterios de aprovacao. | Aprovacoes financeiras podem ficar sem controle. |
| MC-WF-004 | Segregacao de funcoes | Pendente | Definir se criador pode aprovar, excecoes e papeis conflitantes. | Risco de aprovacao indevida. |
| MC-WF-005 | SLA de tarefas | Pendente | Definir prazo, vencimento, escalonamento e responsavel substituto. | Pendencias podem ficar sem tratamento. |
| MC-WF-006 | Notificacoes | Pendente | Definir eventos que geram aviso interno, email ou ambos. | Aprovadores podem nao ser avisados. |
| MC-WF-007 | Comentarios obrigatorios | Pendente | Definir quando rejeicao, aprovacao ou encerramento exigem comentario. | Historico pode ficar pobre para auditoria. |
| MC-WF-008 | Estados customizados | Pendente | Confirmar se cada tenant pode criar estados alem dos padroes. | Parametrizacao pode ficar limitada. |
| MC-WF-009 | Transicoes customizadas | Pendente | Confirmar se cada tenant pode alterar eventos e permissoes. | Motor pode nao atender fluxos reais. |
| MC-WF-010 | BPMN/desenho visual | Pendente | Decidir se fica fora da V1 ou entra como fase futura. | Expectativa de ferramenta visual pode ficar desalinhada. |
| MC-WF-011 | APIs finais | Pendente | Definir endpoints para definicao, instancia, tarefa, transicao, agenda e job. | Time tecnico nao tem contrato de integracao. |
| MC-WF-012 | Eventos de dominio | Pendente | Definir nomes, payloads, consumidores e idempotencia. | Modulos podem integrar de forma inconsistente. |
| MC-WF-013 | Retencao de historico | Pendente | Definir prazo, imutabilidade e expurgo permitido. | Risco de perda de auditoria ou excesso de dados. |
| MC-WF-014 | Mascaramento | Pendente | Definir campos sensiveis em payload antes/depois. | Dados pessoais podem aparecer em auditoria. |
| MC-WF-015 | Anexos | Pendente | Definir tipos, tamanho, obrigatoriedade e vinculo com GED. | Solicitacoes podem ficar sem evidencias. |
| MC-WF-016 | Jobs padrao | Pendente | Catalogar jobs padrao do Epros e sua reconstrucao. | Ambientes podem ficar sem rotinas essenciais. |
| MC-WF-017 | Politica de retry | Pendente | Definir maximo de tentativas, intervalo, backoff e falha final. | Jobs podem repetir demais ou parar cedo. |
| MC-WF-018 | Contexto de usuario em job | Pendente | Definir quando job executa como usuario, sistema ou tenant. | Auditoria pode ficar incorreta. |
| MC-WF-019 | Solicitacao de ausencia/licenca | Pendente | Confirmar se o caso preservado fica no workflow ou apenas no RH. | Funcionalidade pode ficar duplicada. |
| MC-WF-020 | Modelo fisico final | Pendente | Validar nomes de tabelas, indices, chaves e tipos. | Retrabalho na implementacao. |

## 4. Matriz de aderencia ao padrao de especificacao

| Requisito do padrao | Status | Observacao |
|---|---|---|
| Documento descreve o Epros no presente | Concluido | A EF foi escrita como fonte funcional do Epros. |
| Sem nomes de plataformas anteriores | Concluido | Nao ha referencia a sistemas externos de origem do levantamento. |
| Modelo de dados antes do dicionario | Concluido | A EF contem modelo funcional antes do dicionario. |
| Dicionario com campo, formato, tamanho, obrigatoriedade, chave e regra | Concluido | Estrutura aplicada nas entidades e campos transversais. |
| Campos desconhecidos marcados de forma explicita | Concluido | Foi usado `Nao informado no material`. |
| Lacunas separadas da especificacao | Concluido | Pendencias foram encaminhadas para esta MC. |
| Sem invencao de regra obrigatoria | Concluido | Itens criados para padronizacao foram indicados em nota da EF. |

## 5. Itens prontos para validacao humana

| Item | Status | Observacao |
|---|---|---|
| Maquina de estados principal | Concluido | Rascunho, EmAnalise, Ativo, Inativo e Encerrado. |
| Permissoes de transicao | Concluido | Operador, Aprovador e Gestor. |
| Solicitacao aprovada | Concluido | Campos preservados e status pending/approved/rejected. |
| Agendamentos | Concluido | Expressao intervalar e enfileiramento sem pendencia duplicada. |
| Jobs | Concluido | Sucesso, falha, retry, adiamento e falha final. |
| Modelo de dados | Concluido | Entidades funcionais suficientes para modelagem inicial. |
| Criterios de aceite | Concluido | Cenarios principais documentados. |

## 6. Status final do submodulo

| Indicador | Valor |
|---|---|
| Status do submodulo | Concluido |
| Classificacao de conteudo | Com conteudo |
| Arquivos canonicos processados | 11 |
| EF criada | Sim |
| MC criada | Sim |
| Requer retorno ao material canonico para validacao normal | Nao |
| Requer decisao humana antes de construir tudo | Sim, para lacunas listadas |
