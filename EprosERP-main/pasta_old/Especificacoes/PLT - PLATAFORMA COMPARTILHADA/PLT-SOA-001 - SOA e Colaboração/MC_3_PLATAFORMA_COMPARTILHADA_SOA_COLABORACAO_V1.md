# MC 3 - PLATAFORMA COMPARTILHADA / SOA COLABORACAO V1

## 1. Controle do documento

| Item | Valor |
|---|---|
| Sistema | Epros |
| Empresa | Siser |
| Modulo | PLATAFORMA_COMPARTILHADA |
| Submodulo | SOA_COLABORACAO |
| Versao | V1 |
| Data | 2026-06-11 |
| Status | Concluido |
| Conteudo analisado | 12 documentos canonicos do submodulo |

## 2. Resumo de completude

| Area | Status | Evidencia funcional consolidada | Pendencia |
|---|---|---|---|
| Comentarios por entidade | Concluido | Regras de comentario, leitura, exclusao, evento, email e campos de tarefa foram especificadas. | Confirmar catalogo final de recursos comentaveis. |
| Mensagens diretas | Concluido | Contatos, envio, anexos, historico, leitura, edicao, exclusao, favoritos e fixados foram especificados. | Confirmar politica de retencao. |
| Mensagens de equipe | Concluido | Controle de equipe, tracking, arquivos, exclusao, contadores e feed foram especificados. | Confirmar limite operacional de anexos por lote. |
| Presenca | Concluido | Online/offline, validade de 300 segundos e eventos de transicao foram especificados. | Confirmar provedor final de tempo real. |
| Timeline | Concluido | Eventos visiveis, filtros, permissao e paginacao foram especificados. | Confirmar catalogo final de eventos por modulo. |
| Notas | Concluido | Titulo, descricao, tags, anexos, publico/privado e permissao foram especificados. | Confirmar retencao de notas excluidas. |
| Lembretes | Concluido | Um lembrete por usuario/recurso, data futura, envio e vencidos foram especificados. | Confirmar canal de notificacao padrao. |
| Favoritos por recurso | Concluido | Tipos, alternancia, remocao e ordenacao foram especificados. | Confirmar nomes finais dos tipos funcionais. |
| Busca global | Concluido | Categorias, estado inicial, contagens, limites e filtros por permissao foram especificados. | Confirmar categorias ativas por perfil. |
| Templates de email | Concluido | Idioma, fallback, variaveis, HTML, remetente, filtros e paginacao foram especificados. | Confirmar catalogo final de idiomas. |
| Templates de notificacao | Concluido | Tipos, acao, idioma, fallback, assunto fixo e texto simples foram especificados. | Confirmar tipos finais diferentes de email. |
| Notificacoes internas | Concluido | Campos, limites, recentes de 14 dias, leitura e retencao de 90 dias foram especificados. | Confirmar eventos geradores. |
| Email e fila | Concluido | Mensagens, destinatarios, anexos, contas, captura, fila, bloqueio e retentativas foram especificados. | Confirmar dominios de tipo/status e parametros de reprocessamento. |
| Modelo de dados | Concluido | 25 entidades funcionais e dicionario implantavel foram definidos. | Validar nomes fisicos antes da modelagem tecnica. |

## 3. Matriz de lacunas funcionais

| ID | Capacidade esperada | Status | O que falta construir ou validar | Impacto se nao resolver |
|---|---|---|---|---|
| MC-SOA-001 | Catalogo final de recursos comentaveis | Pendente | Validar se os recursos alem de projeto, oportunidade, cliente, tarefa, fatura, atendimento e proposta devem entrar na V1. | Comentarios podem ficar indisponiveis em recursos esperados pelos usuarios. |
| MC-SOA-002 | Comentarios de atendimento | Pendente | Decidir se atendimento entra no mesmo fluxo de comentarios gerais ou exige tratamento proprio. | Duplicidade de experiencia ou ausencia de comentarios em atendimento. |
| MC-SOA-003 | Politica de retencao de mensagens | Pendente | Definir prazo para mensagens diretas, mensagens de equipe, anexos e registros apagados por participante. | Crescimento de base e indefinicao de privacidade. |
| MC-SOA-004 | Limite de anexos em mensagem | Pendente | Definir quantidade maxima, tamanho maximo e tipos permitidos. | Risco operacional de arquivos excessivos. |
| MC-SOA-005 | Provedor de tempo real | Pendente | Definir provedor, credenciais, canais e modo de degradacao operacional. | Experiencia de conversa pode depender apenas de consulta periodica. |
| MC-SOA-006 | Politica de presenca | Pendente | Confirmar se 300 segundos vale para todos os perfis e dispositivos. | Indicador online pode ficar impreciso. |
| MC-SOA-007 | Permissoes por tela | Pendente | Detalhar permissoes finais por caixa de mensagem, equipe, comentarios, notas, templates e busca. | Exposicao indevida ou bloqueio de usuarios corretos. |
| MC-SOA-008 | Eventos de timeline por modulo | Pendente | Definir quais eventos cada modulo pode publicar e quais sao visiveis. | Timeline incompleta ou poluida. |
| MC-SOA-009 | Regras de ocultacao de eventos | Pendente | Validar todas as combinacoes de permissao para tarefa, atendimento e financeiro. | Usuario pode ver evento sem acesso ao recurso ou deixar de ver evento permitido. |
| MC-SOA-010 | Retencao de notas | Pendente | Definir se notas excluidas ficam ocultas, auditadas ou removidas. | Perda de historico ou excesso de dados. |
| MC-SOA-011 | Visibilidade de notas privadas | Pendente | Confirmar se nota privada e visivel apenas ao autor ou tambem a administradores. | Divergencia de privacidade. |
| MC-SOA-012 | Canal de lembrete | Pendente | Confirmar se lembrete vencido gera somente indicador interno, email ou ambos. | Usuario pode nao receber alerta esperado. |
| MC-SOA-013 | Tipos de favoritos por recurso | Pendente | Normalizar nomes finais de tipos permitidos e sinonimos. | Favoritos podem duplicar o mesmo tipo com nomes diferentes. |
| MC-SOA-014 | Ordenacao de favoritos | Pendente | Validar regra de ordenacao por tipo com areas donas dos recursos. | Feed de favoritos pode ficar pouco util. |
| MC-SOA-015 | Categorias da busca global | Pendente | Confirmar categorias ativas, limites e templates de resultado por perfil. | Busca global pode retornar excesso ou falta de resultados. |
| MC-SOA-016 | Indexacao da busca | Pendente | Definir origem de dados e atualizacao dos indices de cada categoria. | Resultado de busca pode ficar lento ou desatualizado. |
| MC-SOA-017 | Idiomas suportados | Pendente | Definir catalogo corporativo de idiomas e regra de ativacao por empresa. | Templates podem ficar sem traducao esperada. |
| MC-SOA-018 | Variaveis de template | Pendente | Catalogar variaveis permitidas por template funcional. | Envio pode falhar por marcador sem valor. |
| MC-SOA-019 | Remetente de email | Pendente | Definir se remetente e global, por empresa, por modulo ou por template. | Inconsistencia na origem das mensagens. |
| MC-SOA-020 | Preferencias de notificacao por email | Pendente | Especificar onde o usuario habilita ou desabilita comunicacoes por acao. | Usuarios podem receber notificacoes indesejadas. |
| MC-SOA-021 | Eventos geradores de notificacao interna | Pendente | Listar eventos que criam notificacoes internas e conteudo padrao. | Indicador interno pode ficar incompleto. |
| MC-SOA-022 | Dominios de email | Pendente | Definir dominios finais de tipo, status e status de resposta. | Dificulta integracao e relatorios de comunicacao. |
| MC-SOA-023 | Contas de entrada | Pendente | Confirmar servicos suportados, seguranca da senha e frequencia de captura. | Captura de email pode ficar insegura ou incompleta. |
| MC-SOA-024 | Reprocessamento de fila | Pendente | Definir janelas, maximo de tentativas e criterio de descarte. | Fila pode acumular erros ou reenviar indevidamente. |
| MC-SOA-025 | Lista de bloqueio | Pendente | Definir fonte e manutencao de bloqueios por endereco e dominio. | Risco de envio para destinatarios bloqueados. |
| MC-SOA-026 | Email arquivado por destinatario | Pendente | Confirmar quando gerar copia individual arquivada por destinatario. | Historico individual pode ficar ausente ou duplicado. |
| MC-SOA-027 | Auditoria e payload | Pendente | Confirmar quais acoes exigem payload detalhado e IP de origem. | Auditoria pode ser insuficiente para rastreabilidade. |
| MC-SOA-028 | Modelo fisico final | Pendente | Validar nomes de tabelas, indices e chaves antes da implantacao tecnica. | Retrabalho na implementacao. |

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
| Fluxo de mensagem direta | Concluido | Inclui envio, anexo, leitura, historico, edicao, exclusao, favoritos e fixados. |
| Fluxo de mensagem de equipe | Concluido | Inclui tracking, arquivos, exclusao e contadores. |
| Fluxo de comentario por recurso | Concluido | Inclui leitura, eventos, email e exclusao. |
| Fluxo de template por idioma | Concluido | Inclui fallback, variaveis e gravacao idempotente. |
| Fluxo de fila de email | Concluido | Inclui bloqueio, validacao, sucesso, falha e retentativa. |
| Modelo de dados | Concluido | Entidades funcionais suficientes para modelagem inicial. |
| Criterios de aceite | Concluido | Cenarios principais documentados. |

## 6. Status final do submodulo

| Indicador | Valor |
|---|---|
| Status do submodulo | Concluido |
| Classificacao de conteudo | Com conteudo |
| Arquivos canonicos processados | 12 |
| EF criada | Sim |
| MC criada | Sim |
| Requer retorno ao material canonico para validacao normal | Nao |
| Requer decisao humana antes de construir tudo | Sim, para lacunas listadas |
