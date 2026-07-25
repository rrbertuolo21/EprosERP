# EF 3 - PLATAFORMA COMPARTILHADA / SOA COLABORACAO V1

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

## 2. Objetivo funcional

O submodulo SOA_COLABORACAO define os recursos compartilhados de colaboracao do Epros para comunicacao entre usuarios, comentarios em entidades de negocio, timeline operacional, notas, lembretes, favoritos, busca global, notificacoes internas, templates de comunicacao e fila de envio.

A finalidade e permitir que os modulos do Epros tenham uma camada unica e padronizada para registrar interacoes, acompanhar historico, notificar usuarios, acionar comunicacoes e manter rastreabilidade operacional sem duplicar logica em cada modulo.

## 3. Escopo funcional

O submodulo contempla:

| Area | Descricao |
|---|---|
| Comentarios por entidade | Registro de comentarios vinculados a recursos de negocio, com leitura, exclusao controlada, anexos e eventos. |
| Mensagens diretas | Conversas entre usuarios, anexos, leitura, exclusao por participante, edicao e historico paginado. |
| Mensagens de equipe | Publicacao de mensagens para equipe, mensagens privadas por usuario e controle de leitura. |
| Presenca | Controle de usuarios online, transicoes de entrada/saida e validade temporal da presenca. |
| Favoritos e fixados | Contatos favoritos, conversas fixadas e recursos favoritos por usuario. |
| Timeline | Consolidacao de eventos visiveis conforme recurso, permissao e configuracao do modulo. |
| Notas | Notas publicas ou privadas com titulo, descricao, tags e anexos. |
| Lembretes | Lembretes por usuario e recurso, com vencimento, envio e exclusao de pendencias. |
| Busca global | Pesquisa por categorias com contagem, limite e filtros por permissao. |
| Templates | Templates de email e notificacao por idioma, com variaveis e conteudo editavel. |
| Notificacoes internas | Notificacoes exibidas no Epros, leitura em lote e retencao temporal. |
| Comunicacao por email | Mensagens, contas de entrada, anexos, fila de envio, bloqueios e tentativas. |
| Auditoria | Historico de acoes, usuario, horario, origem e payload quando aplicavel. |

## 4. Fora de escopo

| Item | Tratamento |
|---|---|
| Motor externo de tempo real | O Epros apenas parametriza e utiliza um provedor quando configurado. |
| Editor completo de campanha comercial | Apenas as capacidades funcionais encontradas para mensagens, templates, fila e controle de envio entram neste submodulo. |
| Modelo definitivo de permissao corporativa | O submodulo consome permissoes do Epros; a matriz final de perfis fica no modulo de seguranca. |
| Catalogo final de todas as entidades comentaveis | As entidades explicitadas entram neste documento; ampliacoes ficam na MC. |

## 5. Atores e responsabilidades

| Ator | Responsabilidades |
|---|---|
| Usuario autenticado | Enviar mensagens, comentar recursos, criar notas, criar lembretes, favoritar recursos e consultar busca conforme permissao. |
| Usuario remetente | Editar a propria mensagem, excluir sua visao da mensagem e acionar envio com texto ou anexo. |
| Usuario destinatario | Receber mensagens, marcar leitura, excluir sua visao da mensagem e receber notificacoes. |
| Usuario administrador | Gerenciar templates, apagar mensagens quando autorizado, consultar registros e administrar parametros. |
| Responsavel pelo recurso | Acompanhar comentarios, lembretes, timeline e eventos do recurso sob sua responsabilidade. |
| Processo automatico | Enviar emails, marcar lembretes enviados, criar notificacoes internas e remover registros vencidos. |

## 6. Conceitos funcionais

| Conceito | Definicao |
|---|---|
| Recurso | Entidade de negocio do Epros que pode receber comentario, nota, evento, lembrete ou favorito. |
| Conversa direta | Troca de mensagens entre dois usuarios. |
| Mensagem de equipe | Mensagem publicada para o conjunto de usuarios habilitados a participar do canal de equipe. |
| Timeline | Lista cronologica de eventos funcionais associados a um recurso ou usuario. |
| Tracking | Registro de leitura, exclusao, entrega ou visibilidade de um item para um usuario. |
| Template | Conteudo parametrizado usado para gerar email ou notificacao. |
| Variavel de template | Marcador substituido por valor funcional antes do envio. |
| Conta de entrada | Configuracao de caixa de email usada para captura de mensagens. |
| Fila de envio | Controle de itens pendentes, enviados, bloqueados ou em erro. |

## 7. Visao funcional integrada

O SOA_COLABORACAO atua como camada transversal. Quando um usuario interage com um recurso, o Epros pode registrar comentario, gerar evento de timeline, criar notificacao interna, disparar email, anexar arquivo e atualizar contadores de leitura.

O submodulo deve suportar dois padroes de comunicacao:

| Padrao | Comportamento |
|---|---|
| Interacao vinculada a recurso | Comentarios, notas, lembretes, eventos e favoritos vinculados a entidade de negocio. |
| Interacao entre usuarios | Mensagens diretas, mensagens de equipe, presenca, favoritos de contato e conversas fixadas. |

## 8. Capacidades funcionais

### 8.1 Comentarios por entidade

1. O Epros permite registrar comentarios em recursos aprovados para colaboracao.
2. Cada comentario deve possuir recurso, autor, texto, data de criacao e situacao.
3. O texto do comentario e obrigatorio.
4. Comentarios podem gerar evento funcional.
5. Comentarios podem gerar comunicacao por email quando a regra do recurso exigir.
6. Comentarios podem ter anexos quando o recurso permitir.
7. A abertura de comentarios de um recurso deve marcar como lidos os itens pendentes do usuario.
8. A exclusao deve respeitar permissao funcional.
9. O Epros deve preservar eventos de criacao e exclusao quando aplicavel.
10. Comentarios de tarefa preservam os campos funcionais `task_id`, `comment` e `user_id`.

### 8.2 Mensagens diretas

1. O Epros lista contatos disponiveis excluindo o proprio usuario.
2. A visibilidade de contatos deve respeitar limites de administracao, propriedade e vinculo funcional.
3. A caixa de entrada apresenta o ultimo item da conversa por contato.
4. A caixa de entrada calcula mensagens nao lidas recebidas do contato.
5. A abertura de uma conversa marca como lidas as mensagens recebidas exibidas.
6. O envio exige destinatario valido.
7. O texto e opcional quando houver anexo.
8. O texto, quando informado, deve respeitar limite de 1000 caracteres.
9. O anexo e opcional.
10. A mensagem nao pode ser enviada se nao houver texto nem anexo.
11. Falha no armazenamento do anexo impede o envio.
12. A mensagem nova inicia como nao lida pelo destinatario.
13. O historico considera os dois sentidos da conversa.
14. A paginacao padrao do historico e de 20 mensagens.
15. A consulta pode buscar registros em ordem interna decrescente e devolver a exibicao em ordem cronologica.
16. A marcacao de leitura automatica ocorre na primeira pagina da conversa.

### 8.3 Edicao e exclusao de mensagens

1. Apenas o remetente pode editar uma mensagem.
2. A edicao exige texto informado.
3. O texto editado deve respeitar limite de 1000 caracteres.
4. A edicao altera apenas o corpo textual da mensagem.
5. A exclusao pode ser feita por remetente ou destinatario.
6. Quando o remetente exclui, a mensagem deixa de aparecer para o remetente.
7. Quando o destinatario exclui, a mensagem deixa de aparecer para o destinatario.
8. Quando ambos excluem, o registro pode ser removido definitivamente.
9. Mensagens ocultadas por participante nao devem aparecer no historico desse participante.

### 8.4 Favoritos e conversas fixadas

1. O usuario pode alternar um contato como favorito.
2. O contato favorito deve existir e estar visivel para o usuario.
3. O usuario pode alternar uma conversa como fixada.
4. A conversa fixada deve apontar para contato valido.
5. Cada usuario pode manter no maximo 3 conversas fixadas.
6. A listagem deve ordenar conversas fixadas antes das demais quando aplicavel.
7. A alternancia deve ser idempotente: se existir, remove; se nao existir, cria.

### 8.5 Presenca e tempo real

1. A presenca online do usuario possui validade de 300 segundos.
2. A entrada online gera evento apenas na transicao de offline para online.
3. A saida offline gera evento apenas na transicao de online para offline.
4. A lista de online deve respeitar a fronteira de contatos permitidos.
5. Eventos em tempo real sao usados quando o provedor estiver configurado.
6. A ausencia de configuracao de tempo real nao bloqueia o modulo.
7. Falha no envio em tempo real nao impede que a mensagem seja salva.
8. O Epros deve manter consulta periodica para novas mensagens quando necessario.
9. A interface pode usar envio de sinal de saida para marcar offline ao fechar a sessao.

### 8.6 Mensagens de equipe

1. Mensagens de equipe exigem habilitacao funcional do recurso.
2. Usuarios sem papel de equipe nao podem acessar a area de mensagens de equipe.
3. A publicacao de mensagem exige origem igual ao usuario autenticado.
4. O alvo deve ser equipe ou usuario existente.
5. O texto e obrigatorio para mensagem textual.
6. HTML nao deve ser aceito no texto da mensagem.
7. Mensagem para equipe cria tracking de leitura para todos os participantes, exceto o remetente.
8. Mensagem direta cria tracking de leitura para o destinatario.
9. A publicacao deve limpar o tracking de leitura do proprio remetente quando aplicavel.
10. Upload de arquivos cria mensagem do tipo arquivo e miniatura quando disponivel.
11. Um lote de anexos pode compartilhar o mesmo horario funcional.
12. Exclusao por participante deve ser tratada como ocultacao individual.
13. Exclusao administrativa remove a mensagem e os trackings relacionados.
14. O Epros deve informar mensagens excluidas pendentes e limpar esse controle apos consumo.
15. Contadores devem agrupar pendencias por destinatario.

### 8.7 Timeline e eventos

1. A timeline consolida eventos funcionais visiveis.
2. Eventos sem visibilidade habilitada nao devem aparecer.
3. A paginacao segue o limite parametrizado para o sistema.
4. A timeline do usuario filtra eventos criados pelo usuario quando solicitado.
5. Eventos de tarefas podem ser ocultados quando o usuario nao possuir acesso a tarefas.
6. Eventos de atendimento podem ser ocultados quando o usuario nao possuir acesso ao respectivo recurso.
7. Eventos financeiros podem ser ocultados quando o usuario nao possuir acesso financeiro.
8. Apenas modulos configurados para mostrar eventos em timeline devem produzir itens visiveis.

### 8.8 Notas

1. Notas podem ser associadas a recurso.
2. A consulta pode filtrar por tipo de recurso.
3. O titulo da nota e obrigatorio.
4. A descricao da nota e obrigatoria.
5. Tags nao devem aceitar HTML.
6. Notas podem ter anexos.
7. Anexos de nota devem poder ser listados, baixados e excluidos conforme permissao.
8. A nota pode ser publica ou privada.
9. A interface deve respeitar configuracao entre minhas notas e notas do recurso.
10. Exclusao de nota exige permissao.

### 8.9 Lembretes

1. Cada usuario pode ter um lembrete por recurso.
2. Ao gravar novo lembrete para o mesmo usuario e recurso, o anterior deve ser substituido.
3. A data e hora do lembrete nao podem estar no passado.
4. O titulo do lembrete e obrigatorio.
5. Lembretes podem ser exibidos em cartao ou painel lateral.
6. O topo do Epros deve listar lembretes vencidos do usuario.
7. A exclusao de todos os lembretes vencidos deve ser restrita ao proprio usuario.
8. O processo automatico marca lembretes como enviados apos comunicacao.
9. Recursos previstos para lembrete incluem projeto, cliente, tarefa, fatura, atendimento, oportunidade e proposta.

### 8.10 Favoritos por recurso

1. O usuario pode marcar recurso como favorito.
2. Tipos funcionais previstos: notas, clientes, projetos, tarefas, faturas, propostas, oportunidades e comentarios de projeto.
3. A alternancia deve ser idempotente.
4. A remocao pelo feed exclui o vinculo de favorito.
5. A ordenacao pode considerar titulo do projeto, atividade recente, data de fatura ou datas da oportunidade conforme o tipo.
6. Tipo desconhecido deve ser rejeitado.
7. O Epros deve carregar comentario mais recente para projeto favorito quando aplicavel.

### 8.11 Busca global

1. Busca vazia retorna estado inicial sem resultados.
2. Categorias previstas: clientes, contatos, projetos, tarefas, oportunidades, arquivos, anexos, atendimentos, contratos, propostas e base de conhecimento.
3. O usuario pode selecionar uma categoria ativa.
4. Cada categoria retorna contagem e lista propria.
5. Cada categoria respeita limite parametrizado.
6. A contagem total soma as categorias retornadas.
7. Resultados devem respeitar permissoes e papeis do usuario.
8. Autocomplete de formulario nao substitui a busca global.

### 8.12 Templates de email

1. A listagem de templates exige permissao de gerenciamento.
2. A listagem carrega idiomas disponiveis.
3. Deve ser possivel filtrar por nome e modulo funcional.
4. Deve haver ordenacao dinamica por campo permitido e direcao.
5. A paginacao padrao e de 10 registros.
6. O catalogo de modulos deve ser derivado dos templates existentes.
7. A edicao exige permissao propria.
8. O idioma padrao e `en`.
9. Conteudo por idioma e obtido por template pai e idioma.
10. Se o idioma solicitado nao existir, o Epros usa conteudo em `en` como referencia.
11. Para novo idioma, o Epros pode apresentar conteudo base sem gravar ate a confirmacao.
12. Atualizacao exige assunto, conteudo e idioma.
13. Variaveis devem ser herdadas do idioma `en`.
14. Gravacao por idioma deve ser idempotente por template e idioma.
15. Metadados do remetente sao atualizados em fluxo separado.
16. O campo de remetente e obrigatorio e aceita ate 255 caracteres.
17. Conteudo de email pode ser editado em formato HTML.
18. A interface deve exibir placeholders disponiveis.
19. Acao de edicao deve ficar indisponivel para usuario sem permissao.

### 8.13 Templates de notificacao

1. A listagem exige permissao de gerenciamento.
2. O catalogo de tipos exclui tipo de email.
3. O tipo ativo vem da consulta do usuario ou do primeiro tipo ordenado.
4. A busca filtra por acao.
5. A ordenacao padrao e por identificador.
6. A paginacao padrao e de 10 registros.
7. A edicao exige permissao propria.
8. Conteudo por idioma segue fallback para `en`.
9. Quando nao existir base, o Epros cria representacao vazia em memoria para edicao.
10. O assunto e fixo a partir da acao e deve ser somente leitura.
11. Atualizacao exige conteudo e idioma.
12. Variaveis devem ser herdadas do idioma `en`.
13. Gravacao por idioma deve ser idempotente.
14. Conteudo e texto simples.

### 8.14 Envio por template

1. O envio seleciona template pelo nome funcional.
2. O idioma vem da empresa, quando configurado, ou de `en`.
3. Email so e enviado se a configuracao de envio estiver ativa para a empresa.
4. Assunto e conteudo devem ter variaveis substituidas antes do envio.
5. O retorno do envio deve padronizar sucesso e erro.
6. Preferencias de notificacao por email devem ser tratadas fora do cadastro do template.

### 8.15 Comunicacao por email e fila

1. Mensagens de email possuem identificador, datas de criacao e alteracao, assunto, tipo, status e correlacao de transporte.
2. O assunto e armazenado com limite de 255 caracteres.
3. Tipo e status devem seguir dominios controlados.
4. Mensagens podem relacionar-se a usuarios criador, alterador, responsavel e atribuido.
5. Mensagens podem relacionar-se a entidades de negocio.
6. Anexos e notas de email devem ser associados em estrutura propria.
7. Marcador de acompanhamento identifica itens sinalizados.
8. Status de resposta controla situacao da mensagem original.
9. Salvar mensagem exige permissao.
10. Destinatarios To, Cc e Bcc devem ser normalizados.
11. Tipo padrao e arquivado quando nao informado.
12. Templates devem ser processados somente quando a mensagem nao for rascunho.
13. Anexos devem ser tratados antes da gravacao final.
14. Envio solicitado deve resultar em status enviado ou erro de envio.
15. Relacionamentos anteriores de endereco devem ser invalidados e reconstruidos.
16. Mensagem salva deve sempre vincular o usuario atual.
17. Contas de entrada exigem servidor, usuario, senha, porta, servico e caixa.
18. Porta deve estar entre 110 e 65535.
19. Senha anterior deve ser preservada quando a alteracao for enviada em branco.
20. Criacao de caso pode ser acionada por parametrizacao.
21. Falha de conexao deve armazenar erro e retornar falha.
22. Opcoes da conta devem ser armazenadas em formato estruturado.
23. Importacao automatica cria pasta de grupo e ativa marcador proprio.
24. Desativacao de importacao automatica limpa inscricoes e pasta correspondente.
25. Sincronizacao deve manter nome e status da pasta alinhados com a conta.
26. Pastas removidas devem atualizar a selecao do usuario.
27. Fila de envio deve usar bloqueio por item em processamento e data de bloqueio.
28. Envio ignora fila sem campanha ou mensagem funcional associada.
29. Validacao de envio exige campanha, template e corpo nao vazio.
30. Endereco invalido deve registrar atividade e remover item da fila.
31. Destinatario sem email principal gera erro sem tentativa de envio.
32. Lista de bloqueio por endereco ou dominio impede envio e marca bloqueio.
33. Sucesso registra atividade de alvo e remove item da fila.
34. Falha incrementa tentativas e permite reprocessamento apos janela temporal.
35. O Epros pode criar email arquivado individual por destinatario quando parametrizado.

### 8.16 Notificacoes internas

1. Notificacao interna deve indicar usuario destinatario.
2. Data de criacao deve ser registrada.
3. Conteudo tem limite de 255 caracteres.
4. Icone tem limite de 255 caracteres.
5. Link tem limite de 255 caracteres.
6. Acao de clique tem limite de 255 caracteres.
7. Icone padrao e `info`.
8. A consulta deve carregar notificacoes recentes dos ultimos 14 dias.
9. A ordenacao e decrescente por data.
10. O usuario pode marcar todas as suas notificacoes como lidas.
11. A interface de topo exibe a lista no indicador de notificacoes.
12. Notificacoes internas podem ser geradas por processos automaticos e eventos funcionais.
13. Registros antigos devem ser removidos apos 90 dias.

## 9. Fluxos funcionais

### 9.1 Enviar mensagem direta

| Passo | Acao | Resultado |
|---|---|---|
| 1 | Usuario abre conversa | Epros carrega historico paginado e marca recebidas da primeira pagina como lidas. |
| 2 | Usuario informa texto e/ou anexo | Epros valida destinatario, tamanho do texto e existencia de conteudo. |
| 3 | Usuario envia | Epros grava mensagem como nao lida para o destinatario. |
| 4 | Ha anexo | Epros armazena arquivo; se falhar, cancela envio. |
| 5 | Tempo real configurado | Epros publica evento para destinatario. |
| 6 | Tempo real indisponivel | Epros mantem registro salvo e permite descoberta por consulta periodica. |

### 9.2 Comentar recurso

| Passo | Acao | Resultado |
|---|---|---|
| 1 | Usuario acessa recurso | Epros valida permissao de leitura. |
| 2 | Usuario abre comentarios | Epros lista comentarios e marca pendencias de leitura. |
| 3 | Usuario grava comentario | Epros exige texto, vincula autor e recurso. |
| 4 | Regra do recurso exige evento | Epros registra evento na timeline. |
| 5 | Regra do recurso exige comunicacao | Epros aciona template ou notificacao. |

### 9.3 Editar template por idioma

| Passo | Acao | Resultado |
|---|---|---|
| 1 | Usuario abre catalogo | Epros valida permissao e carrega idiomas. |
| 2 | Usuario escolhe idioma | Epros busca conteudo do idioma ou referencia `en`. |
| 3 | Usuario salva | Epros exige campos obrigatorios e grava por template e idioma. |
| 4 | Template e usado | Epros substitui variaveis antes do envio. |

### 9.4 Processar fila de email

| Passo | Acao | Resultado |
|---|---|---|
| 1 | Processo seleciona item | Epros bloqueia item com data de processamento. |
| 2 | Processo valida campanha e template | Item invalido e descartado ou marcado em erro conforme regra. |
| 3 | Processo valida destinatario | Email invalido ou bloqueado nao e enviado. |
| 4 | Processo envia | Sucesso registra atividade e remove item; falha incrementa tentativas. |

## 10. Estados funcionais

| Entidade | Estados |
|---|---|
| Registro colaborativo | Rascunho, Em analise, Ativo, Inativo, Encerrado |
| Mensagem | Enviada, Lida, Oculta pelo remetente, Oculta pelo destinatario, Removida |
| Presenca | Online, Offline |
| Lembrete | Pendente, Vencido, Enviado, Excluido |
| Template | Ativo, Inativo |
| Email | Rascunho, Arquivado, Enviado, Erro de envio |
| Fila de envio | Pendente, Em processamento, Bloqueada, Enviada, Erro |
| Notificacao interna | Nao lida, Lida |

## 11. Regras de permissao

| Codigo | Regra |
|---|---|
| SOA-PERM-001 | Gerenciar mensagens exige permissao funcional especifica. |
| SOA-PERM-002 | Enviar mensagens exige permissao de envio. |
| SOA-PERM-003 | Alternar favorito exige permissao para visualizar o contato ou recurso. |
| SOA-PERM-004 | Editar mensagem exige ser remetente. |
| SOA-PERM-005 | Excluir mensagem exige ser remetente, destinatario ou usuario autorizado. |
| SOA-PERM-006 | Gerenciar templates exige permissao de gerenciamento. |
| SOA-PERM-007 | Editar templates exige permissao de edicao. |
| SOA-PERM-008 | Excluir comentarios, notas e anexos exige permissao do recurso. |
| SOA-PERM-009 | Busca global deve filtrar resultados por permissao e papel do usuario. |
| SOA-PERM-010 | Lembretes vencidos em lote so podem ser removidos pelo proprio usuario. |

## 12. Parametros funcionais

| Parametro | Valor encontrado | Observacao |
|---|---|---|
| Validade da presenca online | 300 segundos | Usada para online/offline. |
| Tamanho maximo do texto de mensagem direta | 1000 caracteres | Aplicado em envio e edicao. |
| Quantidade padrao de mensagens no historico | 20 | Historico paginado. |
| Maximo de conversas fixadas por usuario | 3 | Limite por usuario. |
| Idioma padrao de templates | en | Usado como fallback. |
| Paginacao padrao de templates | 10 | Templates de email e notificacao. |
| Periodo de notificacoes recentes | 14 dias | Indicador de topo. |
| Retencao de notificacoes internas antigas | 90 dias | Limpeza automatica. |
| Porta minima de conta de entrada | 110 | Validacao funcional. |
| Porta maxima de conta de entrada | 65535 | Validacao funcional. |
| Limite de conteudo de notificacao interna | 255 caracteres | Conteudo, icone, link e acao. |

## 13. Modelo de dados funcional e implantavel

### 13.1 Visao geral das entidades

| Entidade | Finalidade |
|---|---|
| soa_configuracao | Parametros de colaboracao, timeline, busca, tempo real e comunicacao. |
| soa_comentario | Comentarios vinculados a recursos de negocio. |
| soa_mensagem | Mensagens diretas, de equipe, texto e arquivo. |
| soa_mensagem_tracking | Controle de leitura, ocultacao e notificacoes por participante. |
| soa_contato_preferencia | Favoritos e conversas fixadas por usuario. |
| soa_presenca_usuario | Estado online/offline e validade temporal. |
| soa_evento_timeline | Eventos visiveis em timeline. |
| soa_evento_tracking | Leitura de eventos por usuario e recurso. |
| soa_nota | Notas publicas ou privadas vinculadas a recurso. |
| soa_nota_tag | Tags associadas a notas. |
| soa_lembrete | Lembretes por usuario e recurso. |
| soa_favorito_recurso | Recursos favoritos por usuario. |
| soa_busca_configuracao | Categorias, limites e estado da busca global. |
| soa_template_email | Cadastro principal de templates de email. |
| soa_template_email_idioma | Conteudo de email por idioma. |
| soa_template_notificacao | Cadastro principal de templates de notificacao. |
| soa_template_notificacao_idioma | Conteudo de notificacao por idioma. |
| soa_notificacao_interna | Notificacoes internas por usuario. |
| soa_email_mensagem | Mensagens de email funcionais. |
| soa_email_destinatario | Destinatarios normalizados por mensagem. |
| soa_email_anexo | Anexos e notas vinculados a email. |
| soa_email_conta_entrada | Contas de entrada e captura de email. |
| soa_email_fila_envio | Fila de envio e retentativas. |
| soa_historico | Auditoria funcional do submodulo. |
| soa_anexo | Anexos compartilhados por comentarios, mensagens e notas. |

### 13.2 Relacionamentos principais

| Origem | Relacao | Destino |
|---|---|---|
| soa_comentario | pertence a | recurso funcional por tipo e identificador |
| soa_comentario | pode possuir | soa_anexo |
| soa_mensagem | possui | soa_mensagem_tracking |
| soa_mensagem | pode possuir | soa_anexo |
| soa_contato_preferencia | referencia | usuario e contato |
| soa_evento_timeline | pode gerar | soa_evento_tracking |
| soa_nota | pode possuir | soa_nota_tag |
| soa_nota | pode possuir | soa_anexo |
| soa_lembrete | pertence a | usuario e recurso |
| soa_favorito_recurso | pertence a | usuario e recurso |
| soa_template_email | possui | soa_template_email_idioma |
| soa_template_notificacao | possui | soa_template_notificacao_idioma |
| soa_email_mensagem | possui | soa_email_destinatario |
| soa_email_mensagem | possui | soa_email_anexo |
| soa_email_fila_envio | referencia | soa_email_mensagem |

### 13.3 Entidade soa_configuracao

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| id | inteiro | Nao informado no material | Sim | PK | Identificador da configuracao. |
| tenant_id | inteiro | Nao informado no material | Sim | FK empresa | Empresa da configuracao. |
| chave | texto | Nao informado no material | Sim | UK por tenant | Nome funcional do parametro. |
| valor | texto/json | Nao informado no material | Nao |  | Valor parametrizado. |
| ativo | booleano | true/false | Sim |  | Indica se a configuracao esta ativa. |
| criado_em | data-hora | Nao informado no material | Sim |  | Data de criacao. |
| atualizado_em | data-hora | Nao informado no material | Nao |  | Data de alteracao. |

### 13.4 Entidade soa_comentario

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| id | inteiro | Nao informado no material | Sim | PK | Identificador do comentario. |
| tenant_id | inteiro | Nao informado no material | Sim | FK empresa | Isolamento por empresa. |
| recurso_tipo | texto | projeto, oportunidade, cliente, tarefa, fatura, atendimento, proposta, outros previstos | Sim | indice composto | Tipo do recurso comentado. |
| recurso_id | inteiro/texto | Nao informado no material | Sim | indice composto | Identificador do recurso. |
| autor_usuario_id | inteiro | Nao informado no material | Sim | FK usuario | Usuario que criou o comentario. |
| comentario_texto | texto | Nao informado no material | Sim |  | Conteudo do comentario. |
| status | texto | ativo, excluido | Sim |  | Situacao do comentario. |
| lido_em | data-hora | Nao informado no material | Nao |  | Marcacao de leitura quando aplicavel. |
| task_id | misto | Nao informado no material | Nao | relacao tarefa | Campo preservado para comentarios de tarefa. |
| comment | misto | Nao informado no material | Nao |  | Campo funcional preservado quando a origem do dado usa nome generico. |
| user_id | misto | Nao informado no material | Nao | relacao usuario | Campo funcional preservado para autor em comentarios de tarefa. |
| criado_em | data-hora | Nao informado no material | Sim |  | Criacao do comentario. |
| atualizado_em | data-hora | Nao informado no material | Nao |  | Alteracao do comentario. |

### 13.5 Entidade soa_mensagem

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| id | inteiro | Nao informado no material | Sim | PK | Identificador da mensagem. |
| tenant_id | inteiro | Nao informado no material | Sim | FK empresa | Empresa da conversa. |
| remetente_usuario_id | inteiro | Nao informado no material | Sim | FK usuario | Usuario que enviou. |
| destinatario_usuario_id | inteiro | Nao informado no material | Nao | FK usuario | Obrigatorio para mensagem direta. |
| destinatario_grupo | texto | equipe ou Nao informado no material | Nao |  | Usado para mensagem de equipe. |
| tipo | texto | texto, arquivo | Sim |  | Tipo funcional da mensagem. |
| corpo | texto | maximo 1000 para mensagem direta | Nao |  | Obrigatorio quando nao houver anexo. |
| anexo_id | inteiro | Nao informado no material | Nao | FK soa_anexo | Obrigatorio quando nao houver corpo. |
| visualizada | booleano | true/false | Sim |  | Mensagem nova inicia false para destinatario. |
| apagada_remetente | booleano | true/false | Sim |  | Ocultacao para remetente. |
| apagada_destinatario | booleano | true/false | Sim |  | Ocultacao para destinatario. |
| horario_lote | data-hora | Nao informado no material | Nao |  | Agrupa anexos enviados no mesmo lote. |
| criado_em | data-hora | Nao informado no material | Sim | indice | Data de envio. |
| atualizado_em | data-hora | Nao informado no material | Nao |  | Data de edicao. |

### 13.6 Entidade soa_mensagem_tracking

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| id | inteiro | Nao informado no material | Sim | PK | Identificador do tracking. |
| mensagem_id | inteiro | Nao informado no material | Sim | FK soa_mensagem | Mensagem rastreada. |
| usuario_id | inteiro | Nao informado no material | Sim | FK usuario | Participante relacionado. |
| alvo | texto | usuario, equipe | Sim |  | Tipo de alvo do tracking. |
| lida | booleano | true/false | Sim |  | Controle de leitura. |
| excluida | booleano | true/false | Sim |  | Ocultacao para o participante. |
| notificar_exclusao | booleano | true/false | Nao |  | Indica exclusao pendente de consumo. |
| criado_em | data-hora | Nao informado no material | Sim |  | Criacao do tracking. |
| atualizado_em | data-hora | Nao informado no material | Nao |  | Atualizacao do tracking. |

### 13.7 Entidade soa_contato_preferencia

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| id | inteiro | Nao informado no material | Sim | PK | Identificador. |
| tenant_id | inteiro | Nao informado no material | Sim | FK empresa | Empresa. |
| usuario_id | inteiro | Nao informado no material | Sim | FK usuario | Dono da preferencia. |
| contato_usuario_id | inteiro | Nao informado no material | Sim | FK usuario | Contato alvo. |
| favorito | booleano | true/false | Sim | UK parcial | Alternancia de favorito. |
| fixado | booleano | true/false | Sim | UK parcial | Conversa fixada; maximo 3 por usuario. |
| criado_em | data-hora | Nao informado no material | Sim |  | Criacao. |
| atualizado_em | data-hora | Nao informado no material | Nao |  | Alteracao. |

### 13.8 Entidade soa_presenca_usuario

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| usuario_id | inteiro | Nao informado no material | Sim | PK/FK usuario | Usuario monitorado. |
| tenant_id | inteiro | Nao informado no material | Sim | FK empresa | Empresa. |
| status | texto | online, offline | Sim |  | Estado atual. |
| valido_ate | data-hora | +300 segundos | Sim | indice | Fim da validade online. |
| ultima_transicao_em | data-hora | Nao informado no material | Sim |  | Evita eventos duplicados. |

### 13.9 Entidade soa_evento_timeline

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| id | inteiro | Nao informado no material | Sim | PK | Identificador do evento. |
| tenant_id | inteiro | Nao informado no material | Sim | FK empresa | Empresa. |
| recurso_tipo | texto | Nao informado no material | Sim | indice | Tipo do recurso. |
| recurso_id | inteiro/texto | Nao informado no material | Sim | indice | Recurso associado. |
| criador_usuario_id | inteiro | Nao informado no material | Sim | FK usuario | Autor do evento. |
| acao | texto | Nao informado no material | Sim |  | Acao funcional. |
| descricao | texto | Nao informado no material | Nao |  | Texto de exibicao. |
| visivel | booleano | true/false | Sim | indice | Apenas visiveis aparecem. |
| modulo_exibe_timeline | booleano | true/false | Sim |  | Controla exibicao por modulo. |
| criado_em | data-hora | Nao informado no material | Sim | indice | Data do evento. |

### 13.10 Entidade soa_evento_tracking

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| id | inteiro | Nao informado no material | Sim | PK | Identificador. |
| evento_id | inteiro | Nao informado no material | Sim | FK soa_evento_timeline | Evento. |
| usuario_id | inteiro | Nao informado no material | Sim | FK usuario | Usuario. |
| recurso_tipo | texto | Nao informado no material | Nao | indice | Tipo para leitura por recurso. |
| recurso_id | inteiro/texto | Nao informado no material | Nao | indice | Identificador para leitura por recurso. |
| lido | booleano | true/false | Sim |  | Controle de leitura. |
| lido_em | data-hora | Nao informado no material | Nao |  | Momento da leitura. |

### 13.11 Entidade soa_nota

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| id | inteiro | Nao informado no material | Sim | PK | Identificador da nota. |
| tenant_id | inteiro | Nao informado no material | Sim | FK empresa | Empresa. |
| recurso_tipo | texto | Nao informado no material | Sim | indice | Tipo do recurso. |
| recurso_id | inteiro/texto | Nao informado no material | Sim | indice | Recurso. |
| autor_usuario_id | inteiro | Nao informado no material | Sim | FK usuario | Autor. |
| titulo | texto | Nao informado no material | Sim |  | Obrigatorio. |
| descricao | texto | Nao informado no material | Sim |  | Obrigatoria. |
| visibilidade | texto | publica, privada | Sim |  | Define acesso. |
| status | texto | ativa, excluida | Sim |  | Situacao. |
| criado_em | data-hora | Nao informado no material | Sim |  | Criacao. |
| atualizado_em | data-hora | Nao informado no material | Nao |  | Alteracao. |

### 13.12 Entidade soa_nota_tag

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| id | inteiro | Nao informado no material | Sim | PK | Identificador. |
| nota_id | inteiro | Nao informado no material | Sim | FK soa_nota | Nota. |
| tag | texto | sem HTML | Sim | indice | Tag funcional. |

### 13.13 Entidade soa_lembrete

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| id | inteiro | Nao informado no material | Sim | PK | Identificador. |
| tenant_id | inteiro | Nao informado no material | Sim | FK empresa | Empresa. |
| usuario_id | inteiro | Nao informado no material | Sim | FK usuario | Dono do lembrete. |
| recurso_tipo | texto | projeto, cliente, tarefa, fatura, atendimento, oportunidade, proposta | Sim | UK por usuario/recurso | Tipo do recurso. |
| recurso_id | inteiro/texto | Nao informado no material | Sim | UK por usuario/recurso | Identificador do recurso. |
| titulo | texto | Nao informado no material | Sim |  | Obrigatorio. |
| lembrete_em | data-hora | maior ou igual ao momento atual | Sim | indice | Nao pode estar no passado. |
| enviado | booleano | true/false | Sim |  | Marcado por processo automatico. |
| enviado_em | data-hora | Nao informado no material | Nao |  | Momento do envio. |
| criado_em | data-hora | Nao informado no material | Sim |  | Criacao. |

### 13.14 Entidade soa_favorito_recurso

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| id | inteiro | Nao informado no material | Sim | PK | Identificador. |
| tenant_id | inteiro | Nao informado no material | Sim | FK empresa | Empresa. |
| usuario_id | inteiro | Nao informado no material | Sim | FK usuario | Dono do favorito. |
| recurso_tipo | texto | notas, clientes, projetos, tarefas, faturas, propostas, oportunidades, comentarios de projeto | Sim | UK por usuario/recurso | Tipo permitido. |
| recurso_id | inteiro/texto | Nao informado no material | Sim | UK por usuario/recurso | Recurso favorito. |
| criado_em | data-hora | Nao informado no material | Sim |  | Criacao. |

### 13.15 Entidade soa_busca_configuracao

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| id | inteiro | Nao informado no material | Sim | PK | Identificador. |
| tenant_id | inteiro | Nao informado no material | Sim | FK empresa | Empresa. |
| categoria | texto | clientes, contatos, projetos, tarefas, oportunidades, arquivos, anexos, atendimentos, contratos, propostas, base de conhecimento | Sim | UK por tenant | Categoria pesquisavel. |
| limite | inteiro | Nao informado no material | Sim |  | Limite por categoria. |
| ativa | booleano | true/false | Sim |  | Categoria habilitada. |

### 13.16 Entidade soa_template_email

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| id | inteiro | Nao informado no material | Sim | PK | Identificador. |
| tenant_id | inteiro | Nao informado no material | Sim | FK empresa | Empresa. |
| nome_funcional | texto | Nao informado no material | Sim | UK por tenant | Nome usado para selecao. |
| modulo_funcional | texto | Nao informado no material | Nao | indice | Modulo associado. |
| variaveis | texto/json | Nao informado no material | Nao |  | Variaveis disponiveis. |
| remetente | texto | 255 | Sim |  | Metadado de envio. |
| ativo | booleano | true/false | Sim |  | Permite uso. |
| criado_em | data-hora | Nao informado no material | Sim |  | Criacao. |

### 13.17 Entidade soa_template_email_idioma

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| id | inteiro | Nao informado no material | Sim | PK | Identificador. |
| template_email_id | inteiro | Nao informado no material | Sim | FK soa_template_email | Template pai. |
| idioma | texto | en ou idioma configurado | Sim | UK por template/idioma | Idioma do conteudo. |
| assunto | texto | Nao informado no material | Sim |  | Obrigatorio. |
| conteudo_html | texto | HTML | Sim |  | Obrigatorio para email. |
| criado_em | data-hora | Nao informado no material | Sim |  | Criacao. |
| atualizado_em | data-hora | Nao informado no material | Nao |  | Alteracao. |

### 13.18 Entidade soa_template_notificacao

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| id | inteiro | Nao informado no material | Sim | PK | Identificador. |
| tenant_id | inteiro | Nao informado no material | Sim | FK empresa | Empresa. |
| tipo | texto | diferente de email | Sim | indice | Tipo de notificacao. |
| acao | texto | Nao informado no material | Sim | UK por tenant/tipo | Acao funcional. |
| assunto_fixo | texto | Nao informado no material | Sim |  | Somente leitura na edicao. |
| variaveis | texto/json | Nao informado no material | Nao |  | Herdadas de `en`. |
| ativo | booleano | true/false | Sim |  | Permite uso. |

### 13.19 Entidade soa_template_notificacao_idioma

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| id | inteiro | Nao informado no material | Sim | PK | Identificador. |
| template_notificacao_id | inteiro | Nao informado no material | Sim | FK soa_template_notificacao | Template pai. |
| idioma | texto | en ou idioma configurado | Sim | UK por template/idioma | Idioma. |
| conteudo | texto | texto simples | Sim |  | Obrigatorio. |
| criado_em | data-hora | Nao informado no material | Sim |  | Criacao. |
| atualizado_em | data-hora | Nao informado no material | Nao |  | Alteracao. |

### 13.20 Entidade soa_notificacao_interna

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| id | inteiro | Nao informado no material | Sim | PK | Identificador. |
| tenant_id | inteiro | Nao informado no material | Sim | FK empresa | Empresa. |
| usuario_id | inteiro | Nao informado no material | Sim | indice/FK usuario | Destinatario. |
| data_criacao | data-hora | Nao informado no material | Sim | indice | Data da notificacao. |
| conteudo | texto | 255 | Sim |  | Texto exibido. |
| icone | texto | 255 | Nao |  | Padrao `info`. |
| link | texto | 255 | Nao |  | Destino funcional. |
| acao_clique | texto | 255 | Nao |  | Acao opcional. |
| lida | booleano | true/false | Sim |  | Controle de leitura. |
| lida_em | data-hora | Nao informado no material | Nao |  | Data da leitura. |

### 13.21 Entidade soa_email_mensagem

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| id | inteiro | Nao informado no material | Sim | PK | Identificador. |
| tenant_id | inteiro | Nao informado no material | Sim | FK empresa | Empresa. |
| assunto | texto | 255 | Sim | indice | Assunto da mensagem. |
| tipo | enum | dominio controlado de tipos de email | Sim |  | Tipo funcional. |
| status | enum | rascunho, arquivado, enviado, erro de envio | Sim | indice | Situacao. |
| message_id | texto | Nao informado no material | Nao | indice | Correlacao de transporte. |
| criador_usuario_id | inteiro | Nao informado no material | Sim | FK usuario | Usuario criador. |
| alterador_usuario_id | inteiro | Nao informado no material | Nao | FK usuario | Ultimo alterador. |
| responsavel_usuario_id | inteiro | Nao informado no material | Nao | FK usuario | Responsavel. |
| atribuido_usuario_id | inteiro | Nao informado no material | Nao | FK usuario | Usuario atribuido. |
| corpo | texto | Nao informado no material | Nao |  | Conteudo. |
| sinalizada | booleano | true/false | Sim |  | Acompanhamento. |
| status_resposta | texto | Nao informado no material | Nao |  | Controle de resposta. |
| criado_em | data-hora | Nao informado no material | Sim |  | Data de criacao. |
| atualizado_em | data-hora | Nao informado no material | Sim |  | Data de alteracao. |

### 13.22 Entidade soa_email_destinatario

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| id | inteiro | Nao informado no material | Sim | PK | Identificador. |
| email_mensagem_id | inteiro | Nao informado no material | Sim | FK soa_email_mensagem | Mensagem. |
| tipo_destinatario | texto | to, cc, bcc | Sim | indice | Grupo do destinatario. |
| endereco_email | texto | Nao informado no material | Sim | indice | Endereco normalizado. |
| nome_exibicao | texto | Nao informado no material | Nao |  | Nome do destinatario. |
| valido | booleano | true/false | Sim |  | Resultado de validacao. |

### 13.23 Entidade soa_email_anexo

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| id | inteiro | Nao informado no material | Sim | PK | Identificador. |
| email_mensagem_id | inteiro | Nao informado no material | Sim | FK soa_email_mensagem | Mensagem. |
| anexo_id | inteiro | Nao informado no material | Sim | FK soa_anexo | Arquivo. |
| tipo | texto | anexo, nota | Sim |  | Classificacao. |

### 13.24 Entidade soa_email_conta_entrada

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| id | inteiro | Nao informado no material | Sim | PK | Identificador. |
| tenant_id | inteiro | Nao informado no material | Sim | FK empresa | Empresa. |
| nome | texto | Nao informado no material | Sim |  | Nome da conta. |
| servidor | texto | Nao informado no material | Sim |  | Servidor de entrada. |
| usuario_email | texto | Nao informado no material | Sim |  | Usuario da conta. |
| senha_armazenada | texto | Nao informado no material | Sim |  | Preserva senha anterior quando entrada em branco. |
| porta | inteiro | 110..65535 | Sim |  | Porta de conexao. |
| servico | texto | Nao informado no material | Sim |  | Servico usado. |
| caixa | texto | Nao informado no material | Sim |  | Caixa monitorada. |
| criar_caso | booleano | true/false | Nao |  | Cria atendimento/caso quando habilitado. |
| importacao_automatica | booleano | true/false | Sim |  | Controla captura automatica. |
| pasta_grupo_id | inteiro/texto | Nao informado no material | Nao |  | Pasta criada para importacao automatica. |
| opcoes | texto/json | Nao informado no material | Nao |  | Opcoes estruturadas. |
| erro_conexao | texto | Nao informado no material | Nao |  | Ultima falha. |
| ativa | booleano | true/false | Sim |  | Conta ativa. |

### 13.25 Entidade soa_email_fila_envio

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| id | inteiro | Nao informado no material | Sim | PK | Identificador. |
| tenant_id | inteiro | Nao informado no material | Sim | FK empresa | Empresa. |
| email_mensagem_id | inteiro | Nao informado no material | Nao | FK soa_email_mensagem | Mensagem a enviar. |
| campanha_id | inteiro/texto | Nao informado no material | Nao |  | Referencia funcional quando existir. |
| template_id | inteiro | Nao informado no material | Nao | FK template | Template usado. |
| destinatario_email | texto | Nao informado no material | Sim | indice | Destinatario. |
| status | texto | pendente, processando, bloqueado, enviado, erro | Sim | indice | Estado da fila. |
| em_processamento | booleano | true/false | Sim | indice | Bloqueio otimista. |
| bloqueado_em | data-hora | Nao informado no material | Nao |  | Data do bloqueio. |
| tentativas | inteiro | Nao informado no material | Sim |  | Incrementa em falha. |
| proxima_tentativa_em | data-hora | Nao informado no material | Nao |  | Janela de reprocessamento. |
| motivo_bloqueio | texto | Nao informado no material | Nao |  | Endereco, dominio ou regra. |
| criado_em | data-hora | Nao informado no material | Sim |  | Entrada na fila. |

### 13.26 Entidade soa_anexo

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| id | inteiro | Nao informado no material | Sim | PK | Identificador do anexo. |
| tenant_id | inteiro | Nao informado no material | Sim | FK empresa | Empresa. |
| origem_tipo | texto | mensagem, comentario, nota, email | Sim | indice | Origem funcional. |
| origem_id | inteiro/texto | Nao informado no material | Nao | indice | Registro de origem quando disponivel. |
| nome_arquivo | texto | Nao informado no material | Sim |  | Nome apresentado. |
| caminho_logico | texto | messenger ou Nao informado no material | Sim |  | Pasta logica. |
| mime_type | texto | Nao informado no material | Nao |  | Tipo do arquivo. |
| tamanho_bytes | inteiro | Nao informado no material | Nao |  | Tamanho do arquivo. |
| miniatura_id | inteiro | Nao informado no material | Nao | FK soa_anexo | Miniatura quando houver. |
| criado_por_usuario_id | inteiro | Nao informado no material | Sim | FK usuario | Autor do upload. |
| criado_em | data-hora | Nao informado no material | Sim |  | Data do upload. |

### 13.27 Entidade soa_historico

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| id | inteiro | Nao informado no material | Sim | PK | Identificador. |
| tenant_id | inteiro | Nao informado no material | Sim | FK empresa | Empresa. |
| entidade | texto | Nao informado no material | Sim | indice | Entidade auditada. |
| entidade_id | inteiro/texto | Nao informado no material | Sim | indice | Registro auditado. |
| acao | texto | Nao informado no material | Sim |  | Acao executada. |
| usuario_id | inteiro | Nao informado no material | Sim | FK usuario | Executor. |
| ip_origem | texto | Nao informado no material | Nao |  | Origem da acao. |
| payload_json | texto/json | Nao informado no material | Nao |  | Dados complementares. |
| criado_em | data-hora | Nao informado no material | Sim |  | Momento da acao. |

## 14. Dicionario de dados implantavel

### 14.1 Campos transversais

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| id | inteiro | Nao informado no material | Sim | PK | Identificador unico do registro. |
| tenant_id | inteiro | Nao informado no material | Sim | FK empresa | Deve existir em entidades operacionais multiempresa. |
| criado_em | data-hora | Nao informado no material | Sim |  | Data de criacao. |
| atualizado_em | data-hora | Nao informado no material | Nao |  | Data de alteracao. |
| status | texto/enum | Conforme entidade | Sim | indice quando aplicavel | Estado funcional. |
| usuario_id | inteiro | Nao informado no material | Sim quando houver acao de usuario | FK usuario | Usuario dono, autor ou destinatario conforme contexto. |
| recurso_tipo | texto | Conforme dominio da entidade | Sim quando vinculado a recurso | indice composto | Tipo funcional do recurso. |
| recurso_id | inteiro/texto | Nao informado no material | Sim quando vinculado a recurso | indice composto | Identificador do recurso. |

### 14.2 Campos de mensagem

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| remetente_usuario_id | inteiro | Nao informado no material | Sim | FK usuario | Autor da mensagem. |
| destinatario_usuario_id | inteiro | Nao informado no material | Sim para direta | FK usuario | Destino da conversa direta. |
| corpo | texto | ate 1000 em mensagem direta | Condicional |  | Obrigatorio quando nao houver anexo. |
| anexo_id | inteiro | Nao informado no material | Condicional | FK soa_anexo | Obrigatorio quando nao houver corpo. |
| visualizada | booleano | true/false | Sim |  | Inicia false para nova mensagem recebida. |
| apagada_remetente | booleano | true/false | Sim |  | Ocultacao individual. |
| apagada_destinatario | booleano | true/false | Sim |  | Ocultacao individual. |
| horario_lote | data-hora | Nao informado no material | Nao |  | Agrupamento de arquivos. |

### 14.3 Campos de templates

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| nome_funcional | texto | Nao informado no material | Sim | UK | Seleciona template para envio. |
| modulo_funcional | texto | Nao informado no material | Nao | indice | Filtro de catalogo. |
| idioma | texto | en ou idioma configurado | Sim | UK composta | `en` e fallback padrao. |
| assunto | texto | Nao informado no material | Sim em email |  | Obrigatorio em template de email. |
| conteudo_html | texto | HTML | Sim em email |  | Corpo de email. |
| conteudo | texto | texto simples | Sim em notificacao |  | Corpo da notificacao. |
| variaveis | texto/json | Nao informado no material | Nao |  | Herdadas do idioma `en`. |
| remetente | texto | 255 | Sim em email |  | Metadado do remetente. |

### 14.4 Campos de notificacao interna

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| conteudo | texto | 255 | Sim |  | Texto exibido ao usuario. |
| icone | texto | 255 | Nao |  | Padrao `info`. |
| link | texto | 255 | Nao |  | Destino. |
| acao_clique | texto | 255 | Nao |  | Acao opcional. |
| lida | booleano | true/false | Sim | indice | Controle de leitura. |
| data_criacao | data-hora | ultimos 14 dias na consulta recente | Sim | indice | Base para exibicao e retencao. |

### 14.5 Campos de email e fila

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| assunto | texto | 255 | Sim | indice | Assunto do email. |
| tipo | enum | dominio controlado | Sim |  | Tipo da mensagem. |
| status | enum | rascunho, arquivado, enviado, erro de envio | Sim | indice | Situacao da mensagem. |
| message_id | texto | Nao informado no material | Nao | indice | Correlacao de transporte. |
| tipo_destinatario | texto | to, cc, bcc | Sim | indice | Grupo de destinatario. |
| endereco_email | texto | Nao informado no material | Sim | indice | Normalizado. |
| porta | inteiro | 110..65535 | Sim |  | Conta de entrada. |
| em_processamento | booleano | true/false | Sim | indice | Bloqueio da fila. |
| tentativas | inteiro | Nao informado no material | Sim |  | Incrementa em falhas. |
| motivo_bloqueio | texto | Nao informado no material | Nao |  | Endereco, dominio ou regra. |

## 15. Regras de integracao

| Integracao | Regra |
|---|---|
| GED/anexos | Comentarios, mensagens, notas e emails podem registrar anexos por referencia comum. |
| Workflow | Registros colaborativos podem seguir estados Rascunho, Em analise, Ativo, Inativo e Encerrado quando forem cadastros controlados. |
| Timeline | Comentarios, mensagens, alteracoes e eventos de recursos podem alimentar timeline conforme configuracao. |
| Comunicacao | Templates e notificacoes podem gerar email e notificacao interna. |
| Seguranca | Todas as consultas devem respeitar permissoes, papeis e fronteira de visibilidade. |
| Busca | Categorias retornam resultados filtrados e contados por permissao. |
| Processos automaticos | Lembretes, emails, notificacoes internas e limpeza de retencao devem ser executados por processos programados. |

## 16. Relatorios e telas funcionais

| Tela/visao | Conteudo esperado |
|---|---|
| Caixa de mensagens | Contatos, ultima mensagem, nao lidas, online, favoritos e fixadas. |
| Conversa | Historico paginado, envio de texto, envio de anexo, edicao e exclusao. |
| Mensagens de equipe | Feed de equipe, mensagens privadas, arquivos, contadores e excluidas. |
| Comentarios do recurso | Lista, inclusao, leitura, exclusao e anexos. |
| Timeline | Eventos por recurso ou usuario, filtros e paginacao. |
| Notas | Lista, criacao, edicao, tags, anexos, publico/privado. |
| Lembretes | Criacao, substituicao por recurso, vencidos e exclusao. |
| Favoritos | Recursos favoritos, ordenacao e remocao. |
| Busca global | Categorias, contagens, resultados e estado inicial. |
| Templates de email | Catalogo, filtro, idioma, assunto, conteudo HTML, variaveis e remetente. |
| Templates de notificacao | Catalogo por tipo, idioma, conteudo simples e assunto fixo. |
| Notificacoes internas | Indicador de topo, lista recente e marcar todas como lidas. |
| Email e fila | Mensagens, anexos, destinatarios, contas de entrada e fila de envio. |

## 17. Criterios de aceite

| Codigo | Criterio |
|---|---|
| SOA-CA-001 | Mensagem direta nao pode ser enviada sem texto e sem anexo. |
| SOA-CA-002 | Texto de mensagem direta com mais de 1000 caracteres deve ser rejeitado. |
| SOA-CA-003 | Mensagem com anexo deve ser cancelada quando o armazenamento do arquivo falhar. |
| SOA-CA-004 | Historico de conversa deve ocultar mensagens apagadas pelo participante atual. |
| SOA-CA-005 | Conversa fixada deve respeitar limite de 3 por usuario. |
| SOA-CA-006 | Presenca online deve expirar apos 300 segundos sem renovacao. |
| SOA-CA-007 | Comentario de recurso deve exigir texto. |
| SOA-CA-008 | Nota deve exigir titulo e descricao. |
| SOA-CA-009 | Lembrete nao pode ser criado no passado. |
| SOA-CA-010 | Template de email por idioma deve exigir assunto, conteudo e idioma. |
| SOA-CA-011 | Template de notificacao deve manter assunto somente leitura e editar apenas conteudo por idioma. |
| SOA-CA-012 | Envio por template deve substituir variaveis antes de enviar. |
| SOA-CA-013 | Notificacao interna recente deve considerar ultimos 14 dias. |
| SOA-CA-014 | Limpeza de notificacoes internas deve remover registros com mais de 90 dias. |
| SOA-CA-015 | Fila de email deve bloquear item em processamento e incrementar tentativas em falha. |
| SOA-CA-016 | Busca global deve respeitar permissoes e limites por categoria. |

## 18. Pontos de decisao encaminhados para MC

1. Confirmar catalogo final de recursos comentaveis.
2. Confirmar se comentarios de atendimento devem ser incluidos no mesmo recurso de comentarios gerais.
3. Confirmar dominios definitivos de tipo e status de email.
4. Confirmar lista final de categorias da busca global por modulo implantado.
5. Confirmar politica de retencao para mensagens, comentarios, notas e anexos.
6. Confirmar provedor e parametros de tempo real.
7. Confirmar politica final de permissao por papel para cada tela.

## 19. Notas de elaboracao

[^1]: Foram criados nomes funcionais padronizados para tabelas do Epros com prefixo `soa_`, pois o material informa campos, regras e relacionamentos, mas nao apresenta nomenclatura fisica definitiva para a nova base.
[^2]: O catalogo de recursos foi mantido apenas com os tipos citados no material; qualquer expansao deve ser validada antes de implantacao.
