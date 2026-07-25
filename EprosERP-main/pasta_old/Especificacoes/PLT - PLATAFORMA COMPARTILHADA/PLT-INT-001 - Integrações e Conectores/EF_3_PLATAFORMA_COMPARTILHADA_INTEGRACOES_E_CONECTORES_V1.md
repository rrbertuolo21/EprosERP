# EF_3_PLATAFORMA_COMPARTILHADA_INTEGRACOES_E_CONECTORES_V1

**Projeto:** Epros  
**Empresa:** Siser  
**Tipo de documento:** Especificacao Funcional definitiva  
**Versao:** V1  
**Modulo:** PLATAFORMA_COMPARTILHADA  
**Submodulo:** INTEGRACOES_E_CONECTORES  
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

O submodulo Integracoes e Conectores do Epros deve centralizar a configuracao, seguranca, execucao, auditoria e monitoramento de conectores externos usados pela plataforma. O submodulo deve permitir que cada tenant configure integracoes de IA generativa, captcha, mensageria, webhooks, reunioes externas, pagamentos tecnicos, API de arquivos e rotinas de polling operacional sem espalhar credenciais, regras de envio e contratos de payload pelos modulos de negocio.

| Pergunta | Resposta |
|---|---|
| Para que o submodulo existe? | Para governar conectores externos, suas credenciais, eventos, payloads, estados, permissoes e rastreabilidade. |
| Que problema de negocio resolve? | Reduz risco operacional, duplicidade de configuracao e falta de auditoria em integracoes usadas por diferentes modulos do Epros. |
| Qual resultado operacional deve produzir? | Conectores configurados por tenant, eventos enviados com controle, callbacks recebidos com rastreabilidade, falhas registradas e lacunas visiveis para operacao. |
| Quais areas dependem dele? | Plataforma, Aplicativo, Financeiro, Vendas, Atendimento, GED, Fiscal, IA/ML, Workflow, Relatorios e Suporte. |

## 3. Escopo funcional

### 3.1 Dentro do escopo

| Capacidade | Descricao | Observacao |
|---|---|---|
| Catalogo de conectores | Manter conectores disponiveis, tipo, status, permissao e parametros esperados. | Material consolida multiplas familias de integracao. |
| Configuracao por tenant | Armazenar configuracoes tipadas, segregadas por tenant e por conector. | Material informa configuracoes por integracao. |
| Credenciais e segredos | Controlar chaves, tokens, URLs e parametros sensiveis. | Segredos devem ficar protegidos no servidor. |
| IA generativa | Configurar provedor, modelo, chave, prompt, idioma, tamanho maximo e criatividade. | Material informa assistente de IA e templates de prompt. |
| Captcha | Habilitar/desabilitar validacao, versao e credenciais de site. | Material informa versoes v2/v3 e verificacao remota. |
| Mensageria | Enviar mensagens por conectores de colaboracao, conversa e SMS. | Material informa Slack, Telegram e Twilio. |
| Webhooks de saida | Permitir que eventos do Epros acionem URLs externas configuradas. | Material informa cadastro, acao, metodo, status e payload. |
| Reunioes externas | Criar, listar, atualizar status e cancelar reunioes em provedor externo. | Material informa reunioes Zoom. |
| Pagamentos tecnicos | Configurar gateways, iniciar checkout, receber callback/webhook e registrar rastreabilidade tecnica. | Regra financeira final pertence ao modulo Financeiro.[^nota3] |
| API de arquivos | Permitir operacoes controladas de consulta, remocao, copia, movimentacao, download e upload de arquivos via chave autorizada. | Material informa acoes de API de arquivos. |
| Polling operacional | Expor consulta de pendencias operacionais, notificacoes, lembretes, mensagens e temporizadores. | Material informa cargas de retorno para polling. |
| Observabilidade | Registrar tentativas, respostas, falhas, deduplicacao, auditoria e historico. | Material informa lacunas e necessidade de monitoramento. |

### 3.2 Fora do escopo

| Item fora do escopo | Motivo | Destino correto |
|---|---|---|
| Regra contabila ou financeira de pagamento | Este submodulo controla o conector tecnico, nao a decisao financeira final. | Financeiro |
| Cadastro mestre de cliente, fornecedor ou usuario | Conectores consomem cadastros existentes. | Cadastros Base / Aplicativo |
| Conteudo final de eventos de dominio de cada modulo | O submodulo transporta eventos, mas cada modulo e dono do seu significado. | Modulo dono do evento |
| Motor completo de workflow | Conectores podem acionar workflow, mas nao substituem o submodulo Workflow. | Workflow |
| Regras de documento fiscal | Integracao tecnica pode enviar ou receber payload, mas regra fiscal pertence ao submodulo fiscal. | Faturamento Fiscal Eletronico |
| Motor de arquivos do GED | API de arquivos referencia arquivos, mas o reposititorio documental e controlado pelo GED. | GED |

## 4. Glossario e conceitos funcionais

| Termo | Definicao funcional | Observacoes |
|---|---|---|
| Conector | Capacidade tecnica configuravel para comunicar o Epros com servico externo. | Ex.: IA, captcha, mensageria, reuniao, pagamento, arquivo. |
| Configuracao tipada | Parametro com nome, tipo, obrigatoriedade, dominio e conector. | Evita configuracoes soltas e sem validacao. |
| Credencial | Chave, token, segredo, URL sensivel ou identificador usado para autenticacao externa. | Deve ser protegida, mascarada e auditada. |
| Evento | Ocorrencia interna do Epros capaz de disparar notificacao ou webhook. | O catalogo final de eventos ainda depende de validacao. |
| Template | Modelo de mensagem ou prompt com variaveis controladas. | Usado em IA e mensageria. |
| Webhook de saida | Chamada HTTP enviada pelo Epros para URL configurada. | Usa metodo, acao e payload padronizado. |
| Callback/webhook de entrada | Chamada recebida pelo Epros a partir de provedor externo. | Usado principalmente para pagamentos e conciliacao tecnica. |
| Deduplicacao | Regra que evita processar o mesmo evento externo mais de uma vez. | Material informa referencia de correspondencia. |
| Polling | Consulta periodica de dados operacionais pendentes. | Material informa notificacoes, lembretes, mensagens e timers. |

## 5. Atores, papeis e responsabilidades

| Ator/Papel | Responsabilidade | Permissoes esperadas | Restricoes |
|---|---|---|---|
| Administrador do tenant | Configurar conectores, credenciais, templates e status. | Criar, editar, ativar, inativar e testar conectores. | Deve possuir permissao especifica por conector. |
| Gestor de integracoes | Monitorar execucao, falhas, webhooks e callbacks. | Consultar logs, reenviar quando permitido e exportar evidencias. | Nao acessa segredo completo depois de gravado. |
| Usuario operacional | Consumir a funcionalidade integrada. | Usar IA, validar captcha, enviar mensagem ou criar reuniao conforme permissao. | Nao altera credenciais. |
| Sistema externo | Receber payload ou enviar callback. | Consumir endpoint autorizado ou chamar URL publicada. | Deve respeitar autenticacao, assinatura e formato. |
| Epros | Validar, executar, registrar, auditar e publicar eventos. | Automacao sistemica. | Nao executa conector inativo, incompleto ou sem permissao. |
| Financeiro | Validar efeitos financeiros dos callbacks de pagamento. | Consultar rastreabilidade tecnica e assumir regra de negocio financeira. | Nao armazena credencial tecnica fora do submodulo. |
| Suporte | Diagnosticar falhas e orientar configuracao. | Consultar estado, erro mascarado e trilhas de auditoria. | Nao visualiza segredo sensivel. |

## 6. Visao operacional do submodulo

O administrador habilita um conector para o tenant, preenche configuracoes obrigatorias, registra credenciais e define status. O Epros valida permissoes, plano, obrigatoriedade, dominio dos parametros e consistencia minima antes de permitir uso operacional. O conector ativo pode ser acionado por usuario, evento interno, polling ou callback externo.

Quando o conector e acionado, o Epros monta payload padronizado, aplica template, valida destino, executa chamada externa, registra tentativa, resposta, erro e identificador de correlacao. Quando a comunicacao externa retorna callback, o Epros valida autenticidade, assinatura ou confirmacao, registra payload bruto, aplica deduplicacao e entrega o evento tecnico ao modulo dono do processo.

Nenhum conector deve enviar dados quando estiver inativo, sem credencial obrigatoria, fora do tenant, sem permissao, sem plano habilitado ou sem mapeamento de evento. Falhas devem gerar trilha auditavel, com motivo, status, payload mascarado e possibilidade de reprocesso quando a regra do conector permitir.

## 7. Capacidades funcionais

### 7.1 Catalogo e configuracao de conectores

| Item | Especificacao |
|---|---|
| Objetivo | Manter lista de conectores, tipos, parametros e status por tenant. |
| Acionamento | Cadastro inicial, ativacao, edicao ou inativacao de conector. |
| Pre-condicoes | Tenant valido, usuario autenticado, plano habilitado e permissao de gestao. |
| Dados de entrada | Conector, tipo, status, parametros, credenciais, metodo, acao, URL, templates e observacoes. |
| Processamento | O Epros valida obrigatorios, dominios, permissao, propriedade do tenant e registra historico. |
| Resultado esperado | Conector salvo como ativo ou inativo, com configuracao auditavel. |
| Pos-condicoes | Conector ativo pode ser acionado conforme evento ou uso operacional. |
| Excecoes | Plano ausente, permissao ausente, credencial obrigatoria ausente, dominio invalido ou tenant divergente. |
| Auditoria | Criacao, edicao, ativacao, inativacao, teste, falha e rotacao de segredo. |

### 7.2 IA generativa e prompts

| Item | Especificacao |
|---|---|
| Objetivo | Permitir geracao de texto assistida por IA com prompt controlado por modulo, submodulo e tipo de campo. |
| Acionamento | Usuario solicita geracao ou processo autorizado invoca assistente. |
| Pre-condicoes | Plano habilitado, permissao, provedor, modelo e chave configurados. |
| Dados de entrada | Modulo, submodulo, tipo de campo, contexto, idioma, tamanho maximo e criatividade. |
| Processamento | O Epros seleciona prompt especifico por modulo/submodulo; se nao existir, usa prompt do modulo; se nao existir, usa prompt geral. Campos de identificador tecnico sao ignorados no contexto. |
| Resultado esperado | Resposta gerada conforme prompt, idioma, limite e criatividade. |
| Pos-condicoes | Resultado pode ser exibido ao usuario e registrado com rastreabilidade. |
| Excecoes | Configuracao ausente, chave ausente, prompt inexistente, contexto invalido ou retorno externo indisponivel. |
| Auditoria | Provedor, modelo, prompt aplicado, usuario, modulo, timestamp, status e erro mascarado. |

### 7.3 Captcha

| Item | Especificacao |
|---|---|
| Objetivo | Validar interacoes sensiveis contra automacao indevida. |
| Acionamento | Tela ou endpoint protegido envia token de captcha. |
| Pre-condicoes | Captcha habilitado, versao configurada e site/segredo preenchidos. |
| Dados de entrada | Versao v2/v3, chave do site, segredo, token de resposta e IP remoto quando disponivel. |
| Processamento | O Epros rejeita token vazio, rejeita segredo ausente e valida o token junto ao servico externo usando segredo, resposta e IP remoto. |
| Resultado esperado | Interacao aprovada ou bloqueada. |
| Pos-condicoes | Fluxo protegido continua apenas quando validado. |
| Excecoes | Captcha desabilitado, token vazio, segredo ausente, falha remota ou versao invalida. |
| Auditoria | Tela, acao, tenant, usuario/IP, versao, resultado e motivo. |

### 7.4 Mensageria por colaboracao, conversa e SMS

| Item | Especificacao |
|---|---|
| Objetivo | Enviar notificacoes externas por eventos internos do Epros. |
| Acionamento | Evento interno mapeado ou acao manual autorizada. |
| Pre-condicoes | Modulo ativo, tenant autorizado, conector ativo e credenciais completas. |
| Dados de entrada | Tipo de evento, template, idioma, destinatario, texto, URL de destino, token, chat, remetente, telefone e payload. |
| Processamento | O Epros seleciona template por tipo; se idioma nao existir, usa ingles; substitui variaveis; valida destino; envia ao provedor do canal. |
| Resultado esperado | Mensagem enviada ou falha registrada. |
| Pos-condicoes | Historico fica disponivel para suporte e auditoria. |
| Excecoes | Conector inativo, webhook URL ausente, token ausente, chat ausente, telefone ausente, destinatario inexistente, evento nao mapeado ou modulo inativo. |
| Auditoria | Evento, canal, destinatario mascarado, template, status, resposta externa e erro. |

### 7.5 Webhooks de saida

| Item | Especificacao |
|---|---|
| Objetivo | Enviar eventos do Epros para URLs externas configuradas pelo tenant. |
| Acionamento | Evento interno mapeado no catalogo do conector. |
| Pre-condicoes | Permissao de gestao para cadastro; conector ativo para envio; propriedade do tenant validada. |
| Dados de entrada | Metodo GET/POST, acao, URL, status ativo, criador, modulo, submodulo e extrator de dados. |
| Processamento | O Epros verifica evento, acao, status, metodo, URL e tenant; monta payload com `event`, `module`, `timestamp` e `data`; envia somente se houver mapeamento. |
| Resultado esperado | Payload entregue ou tentativa registrada como falha. |
| Pos-condicoes | Entrega fica disponivel para consulta, reprocesso e indicadores. |
| Excecoes | Evento sem mapeamento, URL invalida, metodo invalido, tenant divergente, conector inativo ou falha externa. |
| Auditoria | Criador, evento, payload mascarado, tentativa, resposta, status HTTP, tempo e erro. |

### 7.6 Reunioes externas

| Item | Especificacao |
|---|---|
| Objetivo | Criar e controlar reunioes externas a partir do Epros. |
| Acionamento | Usuario cria, altera, inicia, encerra ou cancela reuniao. |
| Pre-condicoes | Reunioes habilitadas, credenciais preenchidas, permissao e usuario autorizado. |
| Dados de entrada | Titulo, agenda, inicio, duracao, participantes, host, status, senha, opcoes de video, sala de espera e gravacao. |
| Processamento | O Epros cria a reuniao no provedor externo antes de persistir identificador, link de inicio e link de entrada. Atualizacao remota exige identificador externo e status permitido. |
| Resultado esperado | Reuniao registrada, com links e estado operacional. |
| Pos-condicoes | Participantes podem receber link conforme regra do modulo solicitante. |
| Excecoes | Conector desabilitado, credencial ausente, identificador externo ausente, status nao atualizavel ou falha remota. |
| Auditoria | Criacao, atualizacao, cancelamento, status, host, participantes e erro. |

### 7.7 Pagamentos tecnicos e callbacks

| Item | Especificacao |
|---|---|
| Objetivo | Configurar gateways, iniciar checkout e registrar callbacks tecnicos para posterior decisao financeira. |
| Acionamento | Contratacao, pedido, reserva, assinatura, evento de checkout ou callback externo. |
| Pre-condicoes | Gateway ativo, credenciais preenchidas, modo configurado, permissao e contexto transacional valido. |
| Dados de entrada | Gateway, modo, chave publica quando aplicavel, segredo, moeda, valor, referencia, URLs de retorno/cancelamento, sessao, identificador de pagamento e payload bruto. |
| Processamento | O Epros cria checkout tecnico, registra sessao para conciliacao, recebe callback, valida assinatura ou confirmacao, deduplica pela referencia e encaminha evento tecnico ao Financeiro. |
| Resultado esperado | Rastreabilidade tecnica completa do checkout/callback, sem assumir regra financeira fora do modulo dono. |
| Pos-condicoes | Financeiro pode confirmar situacao financeira e refletir no processo de negocio. |
| Excecoes | Credencial ausente, modo invalido, moeda fora do dominio, assinatura invalida, payload invalido, duplicidade ou callback nao confirmado. |
| Auditoria | Gateway, referencia, payload bruto protegido, assinatura, status tecnico, identificador externo e erro. |

### 7.8 API de arquivos

| Item | Especificacao |
|---|---|
| Objetivo | Expor operacoes controladas sobre arquivos para clientes autorizados. |
| Acionamento | Chamada de API com chave e usuario autorizado. |
| Pre-condicoes | Chave ativa, usuario autorizado e acao permitida. |
| Dados de entrada | Chave, usuario, acao, arquivo, servidor, caminho, pasta, operacao e conteudo quando upload/atualizacao. |
| Processamento | O Epros valida chave, usuario, permissao, propriedade do arquivo e nivel de administracao quando exigido. |
| Resultado esperado | Operacao executada com resposta JSON padronizada de sucesso ou erro. |
| Pos-condicoes | Auditoria registra leitura, alteracao, exclusao, copia, movimentacao ou upload. |
| Excecoes | Chave invalida, usuario inativo, arquivo inexistente, proprietario divergente, permissao insuficiente, metodo invalido ou acao invalida. |
| Auditoria | Acao, usuario, chave mascarada, arquivo, servidor, caminho, resultado e IP. |

### 7.9 Polling operacional

| Item | Especificacao |
|---|---|
| Objetivo | Retornar dados operacionais pendentes para atualizacao periodica de interface ou cliente autorizado. |
| Acionamento | Chamada autenticada de polling. |
| Pre-condicoes | Usuario autenticado e tenant valido. |
| Dados de entrada | Usuario, tenant, filtros, data de referencia e contexto de tela quando aplicavel. |
| Processamento | O Epros consolida notificacoes nao lidas, lembretes, mensagens e timers ativos. |
| Resultado esperado | Payload de atualizacao operacional com contadores e itens pendentes. |
| Pos-condicoes | Interface pode atualizar indicadores sem recarregar todo o contexto. |
| Excecoes | Usuario nao autenticado, tenant invalido ou modulo indisponivel. |
| Auditoria | Usuario, tenant, timestamp, duracao e payload resumido. |

## 8. Regras de negocio

| Regra | Descricao | Condicao | Resultado | Severidade | Observacoes |
|---|---|---|---|---|---|
| INT-001 | Todo conector deve pertencer a um tenant. | Criacao, edicao, envio ou callback. | Operacao sem tenant e bloqueada. | Bloqueante | Material informa isolamento por tenant e criador. |
| INT-002 | Conector ativo exige configuracoes obrigatorias preenchidas. | Ativacao ou uso. | Ativacao/uso bloqueado. | Bloqueante | Aplica-se a todos os conectores. |
| INT-003 | Usuario deve estar autenticado, possuir plano habilitado e permissao quando exigido. | Gestao ou uso. | Operacao negada. | Bloqueante | Material informa validacao de auth, plano e permissao. |
| INT-004 | Segredos devem ser gravados de forma protegida e exibidos apenas mascarados. | Cadastro, edicao e consulta. | Segredo completo nao e exposto. | Bloqueante | Regra de seguranca criada para implantacao.[^nota2] |
| INT-005 | Configuracao deve ser tipada por conector. | Inclusao ou alteracao. | Campo fora do dominio e rejeitado. | Bloqueante | Material aponta necessidade de substituir configuracao solta. |
| INT-006 | IA generativa exige provedor, modelo e chave configurados. | Geracao de conteudo. | Geracao bloqueada quando ausente. | Bloqueante | Material informa falha sem configuracao. |
| INT-007 | Criatividade da IA deve mapear baixo=0.3, medio=0.7 e alto=1.0. | Solicitacao de geracao. | Temperatura aplicada ao provedor. | Normal | Valores informados no material. |
| INT-008 | Contexto de IA deve ignorar identificadores tecnicos. | Montagem de prompt. | Campos de identificador nao entram no prompt. | Normal | Material informa ignorar id, campos com sufixos de identificador e listas de identificadores. |
| INT-009 | Prompt de IA deve seguir fallback especifico, modulo e geral. | Selecao de prompt. | Usa a configuracao mais especifica disponivel. | Normal | Material informa esta ordem. |
| INT-010 | Prompt de IA deve aplicar idioma e tamanho maximo. | Geracao. | Saida condicionada aos parametros. | Normal | Material informa idioma e maxLength. |
| INT-011 | Captcha habilitado exige versao, chave do site e segredo. | Configuracao e validacao. | Validacao bloqueada quando faltar dado. | Bloqueante | Material informa v2/v3, site e secret. |
| INT-012 | Token de captcha vazio deve falhar. | Validacao. | Interacao bloqueada. | Bloqueante | Material informa falha para token vazio. |
| INT-013 | Captcha deve validar segredo, resposta e IP remoto quando disponivel. | Verificacao externa. | Resultado de validacao registrado. | Bloqueante | Material informa envio desses parametros. |
| INT-014 | Mensageria externa so envia quando conector estiver ativo e credenciais estiverem completas. | Evento de mensagem. | Envio nao ocorre. | Bloqueante | Aplica-se a Slack, Telegram e Twilio. |
| INT-015 | Template de notificacao deve ser selecionado por tipo de evento. | Envio de mensagem. | Texto e montado pelo template. | Normal | Material informa templates por tipo. |
| INT-016 | Quando idioma do template nao existir, o Epros deve usar ingles. | Envio multilingue. | Mensagem usa fallback. | Normal | Material informa fallback para ingles. |
| INT-017 | Mensagem de colaboracao deve enviar payload com texto. | Envio por canal colaborativo. | Payload `text` e enviado. | Normal | Material informa JSON com texto. |
| INT-018 | Mensagem de conversa deve enviar chat, texto e modo HTML. | Envio por canal de conversa. | Payload de conversa e enviado. | Normal | Material informa `chat_id`, `text` e `parse_mode` HTML. |
| INT-019 | Mensagem SMS deve enviar remetente, destinatario e corpo. | Envio por SMS. | Payload de SMS e enviado. | Normal | Material informa From, To e Body. |
| INT-020 | Webhook de saida exige metodo GET ou POST e acao valida. | Cadastro e envio. | Cadastro/envio bloqueado se invalido. | Bloqueante | Material informa metodo e acao. |
| INT-021 | Edicao, exclusao e alternancia de webhook exigem propriedade do tenant. | Manutencao de webhook. | Operacao bloqueada se nao for do proprietario. | Bloqueante | Material informa isolamento por criador. |
| INT-022 | Evento nao mapeado para webhook deve ser ignorado. | Dispatch de evento. | Nenhuma chamada externa e feita. | Normal | Material informa eventos sem mapeamento ignorados. |
| INT-023 | Payload de webhook deve conter evento, modulo, timestamp e dados. | Envio de webhook. | Payload padronizado. | Bloqueante | Estrutura informada no material. |
| INT-024 | Webhook deve usar validacao TLS e politica de erro auditavel. | Envio externo. | Endpoint inseguro ou falha fica bloqueado/pendente conforme politica. | Bloqueante | Regra de seguranca criada para implantacao.[^nota2] |
| INT-025 | Reuniao externa so pode ser criada quando integracao estiver habilitada e credenciais estiverem completas. | Criacao de reuniao. | Criacao bloqueada. | Bloqueante | Material informa bloqueio quando desabilitado. |
| INT-026 | Criacao de reuniao deve ocorrer no provedor externo antes da persistencia final. | Criacao de reuniao. | Identificador e links externos sao salvos. | Bloqueante | Material informa criar remoto e depois persistir. |
| INT-027 | Atualizacao remota de reuniao exige status Scheduled e identificador externo. | Edicao de reuniao. | Atualizacao bloqueada sem condicao. | Bloqueante | Material informa atualizacao apenas em Scheduled. |
| INT-028 | Status de reuniao deve usar Scheduled, Started, Ended ou Cancelled. | Atualizacao de status. | Status fora do dominio e rejeitado. | Bloqueante | Estados informados no material. |
| INT-029 | Opcoes de video, sala de espera e gravacao devem ser normalizadas como booleano. | Criacao ou edicao. | Valor final verdadeiro/falso. | Normal | Material informa normalizacao. |
| INT-030 | Gateway de pagamento tecnico ativo exige credenciais e modo sandbox/live. | Checkout ou callback. | Operacao bloqueada se incompleto. | Bloqueante | Material informa modo e credenciais. |
| INT-031 | Contextos gratuitos ou cupons podem ativar fluxo sem gateway quando previsto. | Ativacao comercial. | Gateway nao e acionado. | Normal | Material informa cupons/plano gratuito. |
| INT-032 | Checkout tecnico deve registrar sessao ou identificador externo para conciliacao. | Criacao de checkout. | Referencia tecnica fica persistida. | Bloqueante | Material informa sessao e identificador. |
| INT-033 | Valores para checkout devem respeitar moeda permitida e unidade monetaria do provedor. | Criacao de checkout. | Valor invalido e bloqueado. | Bloqueante | Material informa whitelist de moeda e unidade menor. |
| INT-034 | Webhook de pagamento deve validar assinatura ou confirmacao equivalente. | Callback de pagamento. | Payload invalido retorna erro e nao processa. | Bloqueante | Material informa assinatura e confirmacao. |
| INT-035 | Tolerancia de assinatura de pagamento deve considerar janela de 30 minutos quando aplicavel. | Validacao de assinatura. | Evento fora da janela e rejeitado. | Bloqueante | Janela informada no material. |
| INT-036 | Callback de pagamento deve ser deduplicado por referencia de correspondencia. | Recebimento de callback. | Duplicidade nao gera novo efeito. | Bloqueante | Material informa matching_reference. |
| INT-037 | Webhook tecnico de pagamento deve evoluir de new para processing e completed. | Tratamento do callback. | Status tecnico rastreavel. | Normal | Estados informados no material. |
| INT-038 | Confirmacao de pagamento externo so e aceita quando o provedor retornar status confirmado. | Callback de pagamento. | Evento nao confirmado fica rejeitado ou pendente. | Bloqueante | Material informa VERIFIED/Completed para uma modalidade. |
| INT-039 | API de arquivos exige chave, usuario e acao. | Chamada de API. | Requisicao sem obrigatorios e rejeitada. | Bloqueante | Material informa parametros. |
| INT-040 | Usuario da API de arquivos deve estar ativo e autorizado. | Chamada de API. | Acesso negado. | Bloqueante | Material informa usuario ativo/admin. |
| INT-041 | Operacoes de arquivo devem validar propriedade quando aplicavel. | Info, update, delete, copy, move ou download. | Operacao bloqueada se proprietario divergente. | Bloqueante | Material informa checagem de owner. |
| INT-042 | Move, copy e acesso bruto exigem nivel administrativo quando configurado. | Operacoes sensiveis de arquivo. | Operacao negada sem nivel. | Bloqueante | Material informa nivel 20 para acoes sensiveis. |
| INT-043 | Upload e atualizacao de conteudo devem usar metodo POST quando aplicavel. | Upload/update. | Metodo invalido e rejeitado. | Bloqueante | Material informa POST only. |
| INT-044 | APIs de arquivos devem aplicar limite de taxa, TLS e auditoria. | Chamada externa. | Requisicao fora da politica e bloqueada/registrada. | Bloqueante | Regra de seguranca criada para implantacao.[^nota2] |
| INT-045 | Polling operacional exige usuario autenticado. | Consulta periodica. | Consulta bloqueada. | Bloqueante | Material informa autenticacao. |
| INT-046 | Polling deve retornar contadores e colecoes autorizadas por usuario. | Resposta de polling. | Payload contem somente dados permitidos. | Normal | Material informa notificacoes, lembretes, mensagens e timers. |
| INT-047 | Todas as tentativas externas devem registrar correlacao, status e erro. | Envio ou callback. | Auditoria completa. | Bloqueante | Necessario para suporte e operacao. |
| INT-048 | Dados sensiveis em payloads e logs devem ser mascarados quando exibidos. | Consulta operacional. | Exibicao segura. | Bloqueante | Regra de seguranca criada para implantacao.[^nota2] |

## 9. Parametros de configuracao

| Parametro | Finalidade | Tipo/formato | Valor padrao | Obrigatorio | Nivel | Quem pode alterar | Impacto |
|---|---|---|---|---|---|---|---|
| ConectorHabilitado | Ativar ou inativar conector por tenant. | Booleano | Nao informado no material | Sim | Tenant/conector | Administrador do tenant | Controla uso operacional. |
| ConectorTipo | Classificar conector. | Enum | Nao informado no material | Sim | Catalogo | Siser | Define parametros obrigatorios. |
| ProvedorIA | Definir provedor de IA generativa. | Texto/enum | Nao informado no material | Condicional | Tenant | Administrador do tenant | Necessario para geracao. |
| ModeloIA | Definir modelo de IA generativa. | Texto | Nao informado no material | Condicional | Tenant | Administrador do tenant | Necessario para geracao. |
| ChaveIA | Autenticar provedor de IA. | Segredo | Nao informado no material | Condicional | Tenant | Administrador do tenant | Sem chave, geracao falha. |
| CriatividadeIA | Controlar temperatura. | Enum baixo/medio/alto | Nao informado no material | Condicional | Tenant/uso | Usuario autorizado | Mapeia 0.3, 0.7 ou 1.0. |
| IdiomaPrompt | Definir idioma de resposta. | Texto/enum | Nao informado no material | Condicional | Uso | Usuario autorizado | Condiciona a resposta. |
| TamanhoMaximoPrompt | Limitar resposta. | Inteiro | Nao informado no material | Condicional | Uso | Usuario autorizado | Limita tamanho da geracao. |
| CaptchaVersao | Definir v2 ou v3. | Enum v2/v3 | Nao informado no material | Condicional | Tenant | Administrador do tenant | Controla validacao. |
| CaptchaSiteKey | Identificar site no captcha. | Texto | Nao informado no material | Condicional | Tenant | Administrador do tenant | Necessario quando habilitado. |
| CaptchaSecret | Validar resposta do captcha. | Segredo | Nao informado no material | Condicional | Tenant | Administrador do tenant | Necessario quando habilitado. |
| MensageriaWebhookUrl | URL de canal colaborativo. | URL | Nao informado no material | Condicional | Tenant | Administrador do tenant | Necessario para envio. |
| MensageriaBotToken | Token de canal de conversa. | Segredo | Nao informado no material | Condicional | Tenant | Administrador do tenant | Necessario para envio. |
| MensageriaChatId | Chat de destino padrao. | Texto | Nao informado no material | Condicional | Tenant | Administrador do tenant | Necessario quando canal usa chat. |
| MensageriaAccountSid | Identificador de conta SMS. | Texto | Nao informado no material | Condicional | Tenant | Administrador do tenant | Necessario para SMS. |
| MensageriaAuthToken | Token de autenticacao SMS. | Segredo | Nao informado no material | Condicional | Tenant | Administrador do tenant | Necessario para SMS. |
| MensageriaFrom | Remetente SMS. | Texto/telefone | Nao informado no material | Condicional | Tenant | Administrador do tenant | Necessario para SMS. |
| WebhookMetodo | Metodo de chamada externa. | Enum GET/POST | Nao informado no material | Sim | Webhook | Administrador do tenant | Define envio. |
| WebhookAcao | Acao catalogada. | Texto/enum | Nao informado no material | Sim | Webhook | Administrador do tenant | Define evento aceito. |
| WebhookUrl | URL externa de entrega. | URL | Nao informado no material | Sim | Webhook | Administrador do tenant | Destino da chamada. |
| ReuniaoApiKey | Credencial de reuniao externa. | Segredo | Nao informado no material | Condicional | Tenant | Administrador do tenant | Necessario quando habilitado. |
| ReuniaoApiSecret | Segredo de reuniao externa. | Segredo | Nao informado no material | Condicional | Tenant | Administrador do tenant | Necessario quando habilitado. |
| ReuniaoAccountId | Conta de reuniao externa. | Texto | Nao informado no material | Condicional | Tenant | Administrador do tenant | Necessario quando habilitado. |
| ReuniaoWebhookSecret | Validacao de callback de reuniao. | Segredo | Nao informado no material | Condicional | Tenant | Administrador do tenant | Protege callbacks. |
| GatewayClientId | Identificador de gateway de pagamento. | Segredo/texto | Nao informado no material | Condicional | Tenant | Administrador do tenant | Necessario para checkout. |
| GatewaySecret | Segredo de gateway de pagamento. | Segredo | Nao informado no material | Condicional | Tenant | Administrador do tenant | Necessario para checkout/callback. |
| GatewayMode | Ambiente de pagamento. | Enum sandbox/live | Nao informado no material | Condicional | Tenant | Administrador do tenant | Define endpoint externo. |
| GatewayCurrency | Moeda usada no checkout. | Enum | Nao informado no material | Condicional | Tenant/transacao | Administrador do tenant | Deve estar em dominio permitido. |
| GatewayWebhookSecret | Segredo de assinatura de webhook. | Segredo | Nao informado no material | Condicional | Tenant | Administrador do tenant | Valida callback. |
| ArquivoApiKey | Chave de acesso da API de arquivos. | Texto/segredo | Nao informado no material | Condicional | Usuario/tenant | Administrador | Autentica API. |
| PollingHabilitado | Permitir consulta periodica. | Booleano | Nao informado no material | Condicional | Tenant | Administrador | Controla polling. |

## 10. Modelo de dados funcional e implantavel

### 10.1 Visao geral do modelo

O material informa tabelas e estruturas especificas para prompts de IA, modulos de webhook, webhooks, reunioes externas, configuracoes de pagamento, webhooks de pagamento e API de arquivos baseada em chave de usuario. Para tornar o Epros implantavel como sistema unico, esta EF consolida essas estruturas em um modelo funcional normalizado, mantendo os campos informados e criando entidades de governanca para parametros, credenciais, tentativas, auditoria e observabilidade.[^nota1]

| Grupo de dados | Entidades/tabelas | Papel funcional | Observacoes |
|---|---|---|---|
| Catalogo e configuracao | `integracao_conector`, `integracao_configuracao`, `integracao_credencial` | Define conectores, parametros e segredos por tenant. | Consolida configuracoes dispersas por integracao. |
| Eventos e templates | `integracao_evento_catalogo`, `integracao_template_mensagem`, `integracao_ai_prompt` | Controla eventos, notificacoes e prompts. | Mantem campos de prompt e template informados. |
| Webhooks | `integracao_webhook`, `integracao_webhook_entrega` | Cadastra destinos e registra tentativas de envio. | Mantem method, action, url, active e criador. |
| Captcha | `integracao_captcha_validacao` | Registra validacoes e resultado. | Estrutura criada para auditoria. |
| Reunioes | `integracao_reuniao_externa` | Controla reunioes e links externos. | Mantem campos informados de reuniao. |
| Pagamentos tecnicos | `integracao_pagamento_callback` | Registra checkout/callback tecnico e deduplicacao. | Regra financeira fica fora deste submodulo. |
| API de arquivos | `integracao_api_arquivo_operacao` | Registra operacoes de arquivo por chave autorizada. | Mantem parametros e acoes informadas. |
| Polling e auditoria | `integracao_polling_execucao`, `integracao_auditoria` | Registra consultas, eventos, erros e rastreabilidade. | Estrutura criada para operacao segura. |

### 10.2 Entidades, finalidade e cardinalidade

| Entidade | Finalidade | Cardinalidade | Observacao |
|---|---|---|---|
| `integracao_conector` | Cadastro mestre do conector por tenant. | 1 tenant possui N conectores. | Controla tipo e status. |
| `integracao_configuracao` | Parametros tipados do conector. | 1 conector possui N configuracoes. | Guarda valores nao sensiveis ou referencias. |
| `integracao_credencial` | Segredos e credenciais protegidas. | 1 conector possui N credenciais. | Segredo deve ser mascarado em consulta. |
| `integracao_evento_catalogo` | Catalogo de eventos acionaveis. | 1 conector pode consumir N eventos. | Material informa grande volume de eventos/listeners. |
| `integracao_template_mensagem` | Templates de mensagem por evento, canal e idioma. | 1 evento possui N templates. | Inclui fallback de idioma. |
| `integracao_ai_prompt` | Prompt por modulo, submodulo e tipo de campo. | 1 tenant possui N prompts. | Mantem campos module/submodule/field_type/prompt_template/status. |
| `integracao_webhook` | Destino externo cadastrado pelo tenant. | 1 usuario/tenant possui N webhooks. | Mantem method/action/url/is_active/creator. |
| `integracao_webhook_entrega` | Tentativa de envio de webhook. | 1 webhook possui N entregas. | Criada para observabilidade. |
| `integracao_captcha_validacao` | Resultado de validacao de captcha. | 1 tenant possui N validacoes. | Criada para auditoria. |
| `integracao_reuniao_externa` | Reuniao criada em provedor externo. | 1 tenant possui N reunioes. | Mantem meeting_id, senha, links, status e participantes. |
| `integracao_pagamento_callback` | Registro tecnico de checkout/callback. | 1 referencia pode possuir N callbacks. | Mantem gateway, tipo, referencia, payload e status. |
| `integracao_api_arquivo_operacao` | Registro de acao executada via API de arquivos. | 1 chave/usuario possui N operacoes. | Mantem acao, arquivo, caminho e resultado. |
| `integracao_polling_execucao` | Registro de consulta operacional periodica. | 1 usuario possui N execucoes. | Guarda contadores e tempo. |
| `integracao_auditoria` | Trilha comum de operacoes sensiveis. | N para cada entidade sensivel. | Guarda usuario, IP, acao e payload mascarado. |

## 11. Dicionario de dados implantavel

### 11.1 `integracao_conector`

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| id | UUID/inteiro | Nao informado no material | Sim | PK | Identificador do conector. |
| tenant_id | UUID/inteiro | Nao informado no material | Sim | FK tenant | Segrega dados por tenant. |
| codigo | Texto | Nao informado no material | Sim | Unico por tenant | Codigo funcional do conector. |
| nome | Texto | Nao informado no material | Sim |  | Nome exibido ao usuario. |
| tipo | Enum | IA, CAPTCHA, MENSAGERIA, WEBHOOK, REUNIAO, PAGAMENTO, ARQUIVO, POLLING | Sim |  | Tipo funcional do conector. |
| provedor | Texto | Nao informado no material | Condicional |  | Nome do provedor externo quando aplicavel. |
| status | Enum | Ativo/Inativo | Sim |  | Conector inativo nao executa. |
| modulo_origem | Texto | Nao informado no material | Nao |  | Modulo do Epros que usa o conector. |
| submodulo_origem | Texto | Nao informado no material | Nao |  | Submodulo do Epros que usa o conector. |
| plano_requerido | Texto/booleano | Nao informado no material | Nao |  | Usado para validar plano habilitado. |
| permissao_requerida | Texto | Nao informado no material | Condicional |  | Usado para gestao ou uso. |
| criado_por | UUID/inteiro | Nao informado no material | Sim | FK usuario | Criador do conector. |
| criado_em | Data/hora | ISO 8601 | Sim |  | Data de criacao. |
| atualizado_em | Data/hora | ISO 8601 | Sim |  | Ultima alteracao. |

### 11.2 `integracao_configuracao`

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| id | UUID/inteiro | Nao informado no material | Sim | PK | Identificador da configuracao. |
| conector_id | UUID/inteiro | Nao informado no material | Sim | FK integracao_conector | Vincula ao conector. |
| chave | Texto | Nao informado no material | Sim |  | Ex.: provider, model, recaptcha_version, enabled, mode, currency. |
| valor | Texto/JSON | Nao informado no material | Condicional |  | Valor nao sensivel. |
| tipo_valor | Enum | texto, booleano, inteiro, decimal, url, enum, json | Sim |  | Permite validacao tipada. |
| dominio | Texto/JSON | Nao informado no material | Nao |  | Lista de valores permitidos quando aplicavel. |
| obrigatorio_quando_ativo | Booleano | true/false | Sim |  | Bloqueia ativacao se ausente. |
| sensivel | Booleano | true/false | Sim |  | Se true, valor deve ir para credencial protegida. |
| status | Enum | Ativo/Inativo | Sim |  | Controla parametro. |

### 11.3 `integracao_credencial`

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| id | UUID/inteiro | Nao informado no material | Sim | PK | Identificador da credencial. |
| conector_id | UUID/inteiro | Nao informado no material | Sim | FK integracao_conector | Vincula credencial ao conector. |
| tipo | Enum/texto | api_key, secret, token, client_id, webhook_secret, account_id, from, chat_id | Sim |  | Tipos citados no material. |
| identificador_publico | Texto | Nao informado no material | Condicional |  | Valor nao secreto quando aplicavel. |
| segredo_protegido | Segredo | Nao informado no material | Condicional |  | Nunca exibido integralmente. |
| mascara_exibicao | Texto | Nao informado no material | Nao |  | Valor mascarado para suporte. |
| valido_ate | Data/hora | ISO 8601 | Nao |  | Nao informado no material. |
| status | Enum | Ativo/Inativo/Revogado | Sim |  | Credencial inativa nao executa. |
| rotacionado_em | Data/hora | ISO 8601 | Nao |  | Usado para governanca. |

### 11.4 `integracao_ai_prompt`

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| id | UUID/inteiro | Nao informado no material | Sim | PK | Identificador do prompt. |
| tenant_id | UUID/inteiro | Nao informado no material | Sim | FK tenant | Segrega prompt por tenant. |
| module | Texto | Nao informado no material | Sim |  | Campo informado no material. |
| submodule | Texto | Nao informado no material | Nao |  | Campo informado no material; pode ser vazio para fallback do modulo. |
| field_type | Texto | Nao informado no material | Sim |  | Tipo de campo do prompt. |
| prompt_template | Texto longo | Nao informado no material | Sim |  | Template usado na geracao. |
| status | Enum | Ativo/Inativo | Sim |  | Prompt inativo nao e selecionado. |
| idioma | Texto/enum | Nao informado no material | Nao |  | Idioma quando configurado. |
| criado_em | Data/hora | ISO 8601 | Sim |  | Criacao. |

### 11.5 `integracao_evento_catalogo`

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| id | UUID/inteiro | Nao informado no material | Sim | PK | Identificador do evento. |
| codigo_evento | Texto | Nao informado no material | Sim | Unico por modulo | Codigo funcional do evento. |
| modulo | Texto | Nao informado no material | Sim |  | Modulo emissor. |
| submodulo | Texto | Nao informado no material | Nao |  | Submodulo emissor quando aplicavel. |
| tipo | Enum/texto | notificacao, webhook, pagamento, sistema | Sim |  | Classificacao funcional. |
| descricao | Texto | Nao informado no material | Sim |  | Descricao para gestao. |
| payload_schema | JSON | Nao informado no material | Condicional |  | Esquema esperado para envio. |
| ativo | Booleano | true/false | Sim |  | Evento inativo nao dispara. |

### 11.6 `integracao_template_mensagem`

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| id | UUID/inteiro | Nao informado no material | Sim | PK | Identificador do template. |
| evento_id | UUID/inteiro | Nao informado no material | Sim | FK integracao_evento_catalogo | Evento associado. |
| canal | Enum | colaboracao, conversa, sms | Sim |  | Canais informados no material. |
| idioma | Texto/enum | Nao informado no material | Sim |  | Fallback para ingles quando ausente. |
| assunto | Texto | Nao informado no material | Nao |  | Usado quando canal suportar. |
| corpo_template | Texto longo | Nao informado no material | Sim |  | Texto com placeholders. |
| placeholders | JSON | Nao informado no material | Nao |  | Lista de variaveis aceitas. |
| ativo | Booleano | true/false | Sim |  | Template inativo nao e selecionado. |

### 11.7 `integracao_webhook`

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| id | UUID/inteiro | Nao informado no material | Sim | PK | Identificador do webhook. |
| tenant_id | UUID/inteiro | Nao informado no material | Sim | FK tenant | Segregacao por tenant. |
| method | Enum | GET/POST | Sim |  | Campo informado no material. |
| action | Texto/enum | Nao informado no material | Sim | FK/catalogo | Campo informado no material. |
| url | URL | Nao informado no material | Sim |  | URL externa de entrega. |
| is_active | Booleano | true/false | Sim |  | Campo informado no material. |
| creator_id | UUID/inteiro | Nao informado no material | Condicional | FK usuario | Campo informado no material. |
| created_by | UUID/inteiro/texto | Nao informado no material | Sim | FK usuario | Campo informado no material. |
| headers_config | JSON | Nao informado no material | Nao |  | Parametros adicionais quando aprovados. |
| payload_schema | JSON | Nao informado no material | Nao |  | Esquema versionado recomendado. |
| criado_em | Data/hora | ISO 8601 | Sim |  | Criacao. |

### 11.8 `integracao_webhook_entrega`

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| id | UUID/inteiro | Nao informado no material | Sim | PK | Identificador da tentativa. |
| webhook_id | UUID/inteiro | Nao informado no material | Sim | FK integracao_webhook | Webhook acionado. |
| event | Texto | Nao informado no material | Sim |  | Campo do payload padrao. |
| module | Texto | Nao informado no material | Sim |  | Campo do payload padrao. |
| timestamp | Data/hora | ISO 8601 | Sim |  | Campo do payload padrao. |
| data_payload | JSON | Nao informado no material | Sim |  | Campo `data` do payload padrao. |
| status_envio | Enum | Pendente/Enviado/Falha/Ignorado | Sim |  | Resultado da tentativa. |
| http_status | Inteiro | 100-599 | Nao |  | Retorno externo. |
| resposta_externa | Texto/JSON | Nao informado no material | Nao |  | Deve mascarar dados sensiveis. |
| erro | Texto | Nao informado no material | Nao |  | Motivo de falha. |
| duracao_ms | Inteiro | Nao informado no material | Nao |  | Tempo de chamada. |

### 11.9 `integracao_reuniao_externa`

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| id | UUID/inteiro | Nao informado no material | Sim | PK | Identificador interno. |
| tenant_id | UUID/inteiro | Nao informado no material | Sim | FK tenant | Segregacao. |
| meeting_id | Texto | Nao informado no material | Condicional | Identificador externo | Campo informado no material. |
| meeting_password | Texto/segredo | Nao informado no material | Nao |  | Campo informado no material. |
| start_url | URL | Nao informado no material | Condicional |  | Campo informado no material. |
| join_url | URL | Nao informado no material | Condicional |  | Campo informado no material. |
| start_time | Data/hora | ISO 8601 | Sim |  | Campo informado no material. |
| duration | Inteiro | Minutos | Sim |  | Campo informado no material. |
| status | Enum | Scheduled/Started/Ended/Cancelled | Sim |  | Estados informados no material. |
| participants | JSON/lista | Nao informado no material | Nao |  | Campo informado no material. |
| host_id | UUID/inteiro/texto | Nao informado no material | Sim | FK usuario/host | Campo informado no material. |
| created_by | UUID/inteiro | Nao informado no material | Sim | FK usuario | Campo informado no material. |
| host_video | Booleano | true/false | Nao |  | Deve ser normalizado. |
| participant_video | Booleano | true/false | Nao |  | Deve ser normalizado. |
| waiting_room | Booleano | true/false | Nao |  | Deve ser normalizado. |
| recording | Booleano | true/false | Nao |  | Deve ser normalizado. |

### 11.10 `integracao_pagamento_callback`

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| id | UUID/inteiro | Nao informado no material | Sim | PK | Identificador do registro tecnico. |
| tenant_id | UUID/inteiro | Nao informado no material | Sim | FK tenant | Segregacao. |
| gateway_name | Texto/enum | PayPal/Stripe/outros homologados | Sim |  | Material informa esses gateways. |
| type | Texto/enum | Nao informado no material | Sim |  | Tipo de evento recebido. |
| payment_type | Enum | subscription/onetime | Condicional |  | Campo informado no material. |
| payment_transactionid | Texto | Nao informado no material | Condicional |  | Identificador externo. |
| matching_reference | Texto | Nao informado no material | Sim | Indice de deduplicacao | Campo informado no material. |
| payload | JSON/texto bruto | Nao informado no material | Sim |  | Payload bruto protegido. |
| status | Enum | new/processing/completed/failed | Sim |  | Estados tecnicos informados com extensao de falha.[^nota1] |
| assinatura_valida | Booleano | true/false | Condicional |  | Necessaria quando provedor assina callback. |
| confirmado_externamente | Booleano | true/false | Condicional |  | Necessario quando provedor exige confirmacao remota. |
| recebido_em | Data/hora | ISO 8601 | Sim |  | Data de recebimento. |
| processado_em | Data/hora | ISO 8601 | Nao |  | Data de processamento tecnico. |
| erro | Texto | Nao informado no material | Nao |  | Motivo de rejeicao ou falha. |

### 11.11 `integracao_api_arquivo_operacao`

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| id | UUID/inteiro | Nao informado no material | Sim | PK | Identificador da operacao. |
| tenant_id | UUID/inteiro | Nao informado no material | Sim | FK tenant | Segregacao. |
| api_key_mascarada | Texto | 32 caracteres informados para chave de usuario | Sim |  | Material informa chave em usuario com varchar(32). |
| username | Texto | Nao informado no material | Condicional | FK usuario | Parametro informado. |
| action | Enum/texto | list, info, delete, move, copy, rawget, rawdelete, upload, update | Sim |  | Acoes informadas no material. |
| file_id | UUID/inteiro | Nao informado no material | Condicional | FK arquivo | Parametro informado. |
| server_id | UUID/inteiro | Nao informado no material | Condicional |  | Parametro informado. |
| file_path | Texto | Nao informado no material | Condicional |  | Parametro informado. |
| folder_id | UUID/inteiro | Nao informado no material | Condicional | FK pasta | Informado para upload. |
| metodo_http | Enum | GET/POST | Sim |  | Upload/update exigem POST. |
| resultado | Enum | success/error | Sim |  | Resposta JSON padronizada. |
| erro | Texto | Nao informado no material | Nao |  | Motivo de erro. |
| criado_em | Data/hora | ISO 8601 | Sim |  | Data da operacao. |

### 11.12 `integracao_captcha_validacao`

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| id | UUID/inteiro | Nao informado no material | Sim | PK | Identificador da validacao. |
| tenant_id | UUID/inteiro | Nao informado no material | Sim | FK tenant | Segregacao. |
| versao | Enum | v2/v3 | Sim |  | Versoes informadas. |
| acao | Texto | Nao informado no material | Condicional |  | Tela/acao protegida. |
| remote_ip | IP | IPv4/IPv6 | Nao |  | Enviado quando disponivel. |
| token_presente | Booleano | true/false | Sim |  | Token vazio falha. |
| sucesso | Booleano | true/false | Sim |  | Resultado da validacao. |
| erro | Texto | Nao informado no material | Nao |  | Motivo de falha. |
| criado_em | Data/hora | ISO 8601 | Sim |  | Data da validacao. |

### 11.13 `integracao_polling_execucao`

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| id | UUID/inteiro | Nao informado no material | Sim | PK | Identificador da consulta. |
| tenant_id | UUID/inteiro | Nao informado no material | Sim | FK tenant | Segregacao. |
| usuario_id | UUID/inteiro | Nao informado no material | Sim | FK usuario | Usuario autenticado. |
| unread_notifications | Inteiro | >= 0 | Nao |  | Contador informado no material. |
| reminders_count | Inteiro | >= 0 | Nao |  | Contador informado no material. |
| messages_count | Inteiro | >= 0 | Nao |  | Contador informado no material. |
| active_timer_status | Texto/JSON | Nao informado no material | Nao |  | Timer ativo informado no material. |
| payload_resumo | JSON | Nao informado no material | Nao |  | Resumo autorizado. |
| duracao_ms | Inteiro | Nao informado no material | Nao |  | Tempo da consulta. |
| criado_em | Data/hora | ISO 8601 | Sim |  | Data da consulta. |

### 11.14 `integracao_auditoria`

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| id | UUID/inteiro | Nao informado no material | Sim | PK | Identificador da auditoria. |
| tenant_id | UUID/inteiro | Nao informado no material | Sim | FK tenant | Segregacao. |
| entidade | Texto | Nao informado no material | Sim |  | Entidade afetada. |
| entidade_id | UUID/inteiro | Nao informado no material | Sim |  | Registro afetado. |
| acao | Texto | Nao informado no material | Sim |  | Criar, editar, ativar, enviar, receber, falhar, rotacionar. |
| usuario_id | UUID/inteiro | Nao informado no material | Condicional | FK usuario | Usuario ou processo. |
| ip | IP | IPv4/IPv6 | Nao |  | Origem quando disponivel. |
| payload_mascarado | JSON | Nao informado no material | Nao |  | Sem segredos expostos. |
| resultado | Enum | Sucesso/Falha | Sim |  | Resultado. |
| criado_em | Data/hora | ISO 8601 | Sim |  | Data do evento. |

## 12. Contratos de payload e integracoes

| Contrato | Direcao | Campos obrigatorios | Resultado esperado | Observacoes |
|---|---|---|---|---|
| Payload de webhook de saida | Epros para externo | `event`, `module`, `timestamp`, `data` | Evento entregue ou falha registrada. | Estrutura informada no material. |
| Mensagem colaborativa | Epros para externo | `text` | Mensagem enviada ao canal. | Material informa JSON com texto. |
| Mensagem de conversa | Epros para externo | `chat_id`, `text`, `parse_mode` | Mensagem enviada com HTML. | Material informa HTML. |
| SMS | Epros para externo | `From`, `To`, `Body` | SMS enviado. | Material informa HTTP Basic e campos. |
| IA generativa | Epros para provedor | provedor, modelo, chave, prompt, contexto | Conteudo gerado. | Campos de identificador tecnico sao excluidos do contexto. |
| Captcha | Epros para provedor | segredo, resposta, IP remoto quando disponivel | Token validado. | Token vazio ou segredo ausente falha. |
| Reuniao externa | Epros para provedor | credenciais, titulo, horario, duracao, participantes | Identificador e links retornados. | Criacao remota antecede persistencia final. |
| Checkout tecnico | Epros para gateway | valor, moeda, referencia, retorno, cancelamento | Sessao/ordem criada. | Financeiro define efeito final. |
| Callback de pagamento | Externo para Epros | payload bruto, assinatura/confirmacao, referencia | Registro tecnico deduplicado. | Payload invalido deve retornar erro. |
| API de arquivos | Externo para Epros | chave, usuario, acao e parametros da acao | JSON success/error. | Acoes sensiveis exigem permissao adicional. |
| Polling | Cliente para Epros | usuario autenticado, tenant | Contadores e itens pendentes. | Retorna somente dados autorizados. |

## 13. Telas, consultas e relatorios

| Interface | Objetivo | Campos/acoes minimas | Observacoes |
|---|---|---|---|
| Catalogo de conectores | Listar conectores por tipo, status e tenant. | Tipo, provedor, status, ultima execucao, ultima falha, editar, ativar, inativar, testar. | Deve ocultar segredos. |
| Configuracao de IA | Configurar provedor, modelo, chave, prompts e parametros. | Provider, model, api_key, prompt, idioma, criatividade, tamanho maximo. | Deve validar obrigatorios. |
| Geracao assistida | Solicitar conteudo por modulo/submodulo. | Modulo, submodulo, tipo de campo, contexto, resultado. | Deve registrar prompt aplicado. |
| Configuracao de captcha | Ativar, escolher v2/v3 e informar chaves. | Enabled, recaptcha_version, site key, secret. | Deve permitir teste de validacao. |
| Configuracao de mensageria | Configurar canais externos. | Webhook URL, notification flag, bot token, chat id, SID, token, from. | Deve exibir status por canal. |
| Gestao de webhooks | Criar e manter URLs externas. | Metodo, acao, URL, ativo, testar, remover, alternar status. | Edicao exige propriedade do tenant. |
| Monitor de entregas | Consultar envios externos. | Evento, conector, status, HTTP, duracao, erro, reenviar quando permitido. | Material indica necessidade de healthcheck/historico. |
| Configuracao de reunioes | Configurar credenciais e habilitacao. | Api key, secret, account id, enabled, webhook secret. | Deve validar whitelist. |
| Agenda de reunioes | Listar, criar, editar, iniciar, encerrar e cancelar. | Titulo, inicio, duracao, participantes, status, links. | Lista propria ou geral conforme permissao. |
| Configuracao de pagamentos | Configurar gateway tecnico. | Client id, secret, mode, enabled, moeda, webhook secret. | Nao deve armazenar cartao. |
| Monitor de callbacks | Consultar callbacks de pagamento. | Gateway, referencia, status, assinatura, payload, erro, datas. | Efeito financeiro fica no Financeiro. |
| API de arquivos | Monitorar acessos e erros. | Usuario, chave mascarada, acao, arquivo, resultado, IP. | Deve mostrar operacoes sensiveis. |
| Polling operacional | Diagnosticar consultas periodicas. | Usuario, contadores, timers, duracao, erro. | Usado por suporte. |

## 14. Relatorios e indicadores

| Indicador/relatorio | Descricao | Filtros | Periodicidade |
|---|---|---|---|
| Saude dos conectores | Percentual de conectores ativos, configurados e com ultima chamada bem-sucedida. | Tenant, tipo, provedor, periodo. | Diario/tempo real. |
| Falhas por conector | Volume de erros por tipo, status HTTP e provedor. | Tenant, conector, erro, periodo. | Diario. |
| Entregas de webhook | Tentativas, sucesso, falha e ignorados. | Evento, metodo, URL, periodo. | Diario. |
| Uso de IA | Geracoes por modulo, usuario, prompt e status. | Modulo, submodulo, usuario, periodo. | Diario/mensal. |
| Validacoes de captcha | Sucesso, falha, token vazio e erro remoto. | Tela, acao, versao, periodo. | Diario. |
| Mensagens enviadas | Envio por canal, evento e template. | Canal, evento, status, periodo. | Diario. |
| Reunioes externas | Criadas, iniciadas, encerradas e canceladas. | Host, status, periodo. | Diario. |
| Callbacks de pagamento | Recebidos, deduplicados, invalidos, confirmados e pendentes. | Gateway, referencia, status, periodo. | Diario. |
| API de arquivos | Acoes executadas, negadas e sensiveis. | Usuario, acao, arquivo, periodo. | Diario. |
| Polling operacional | Volume, duracao media e falhas. | Usuario, tenant, periodo. | Diario. |

## 15. Seguranca, privacidade e auditoria

| Tema | Regra funcional |
|---|---|
| Segregacao | Todo dado deve possuir tenant ou contexto equivalente, e consultas devem respeitar o tenant. |
| Permissao | Gestao de conector, segredo, webhook, reuniao, pagamento e API de arquivos exige permissao especifica. |
| Segredos | Chaves, tokens e secrets devem ser protegidos, mascarados e auditados. |
| TLS | Chamadas externas e APIs devem exigir transporte seguro. |
| Assinatura | Callbacks de pagamento devem validar assinatura ou confirmacao equivalente. |
| Deduplicacao | Eventos externos devem ser deduplicados antes de gerar efeito tecnico. |
| Logs | Logs devem conter correlacao, status, erro e payload mascarado. |
| Retencao | Prazos de retencao de payloads e logs nao estao informados no material e devem ser definidos na MC. |
| Dados pessoais | Telefone, chat, IP, usuario, payload e participantes devem seguir regras de privacidade e minimizacao. |

## 16. Testes funcionais minimos

| Cenario | Dado/condicao | Resultado esperado |
|---|---|---|
| Conector sem plano | Usuario tenta configurar conector sem plano habilitado. | Operacao negada. |
| Conector sem permissao | Usuario sem permissao tenta editar. | Operacao negada. |
| Conector ativo sem credencial | Usuario tenta ativar conector incompleto. | Ativacao bloqueada. |
| IA sem chave | Usuario solicita geracao sem chave. | Geracao falha com motivo. |
| IA com prompt especifico | Existe prompt para modulo/submodulo/tipo. | Prompt especifico e selecionado. |
| IA sem prompt especifico | Nao existe prompt especifico. | Epros usa fallback por modulo ou geral. |
| Captcha token vazio | Token nao enviado. | Validacao falha. |
| Captcha segredo ausente | Segredo nao configurado. | Validacao falha. |
| Mensageria inativa | Evento mapeado, conector inativo. | Nenhuma mensagem e enviada. |
| Mensageria credencial incompleta | Canal ativo sem token/destino. | Envio nao ocorre e erro e registrado. |
| Webhook sem propriedade | Usuario tenta alterar webhook de outro tenant. | Operacao bloqueada. |
| Webhook evento nao mapeado | Evento sem acao. | Chamada externa ignorada. |
| Webhook payload padrao | Evento mapeado e ativo. | Payload contem event, module, timestamp e data. |
| Reuniao desabilitada | Usuario tenta criar reuniao. | Criacao bloqueada. |
| Reuniao Scheduled | Usuario atualiza reuniao agendada. | Atualizacao remota e persistida. |
| Reuniao nao Scheduled | Usuario tenta alterar reuniao encerrada/cancelada. | Alteracao bloqueada. |
| Checkout sem credencial | Gateway ativo incompleto. | Checkout bloqueado. |
| Callback com assinatura invalida | Provedor envia payload invalido. | Retorna erro e nao processa. |
| Callback duplicado | Mesmo matching_reference recebido novamente. | Nao gera novo efeito tecnico. |
| API arquivo sem chave | Chamada sem chave. | JSON de erro. |
| API arquivo sem propriedade | Usuario tenta alterar arquivo de outro proprietario. | Operacao bloqueada. |
| Polling sem autenticacao | Cliente consulta sem sessao valida. | Consulta bloqueada. |

## 17. Criterios de aceite

| Codigo | Criterio |
|---|---|
| CA-INT-001 | Deve ser possivel cadastrar conector por tenant com tipo, status, parametros e credenciais obrigatorias. |
| CA-INT-002 | O Epros deve impedir uso de conector inativo, incompleto, sem plano ou sem permissao. |
| CA-INT-003 | IA deve selecionar prompt por modulo/submodulo, depois modulo e depois geral. |
| CA-INT-004 | Captcha deve bloquear token vazio e segredo ausente. |
| CA-INT-005 | Mensageria deve enviar somente quando canal estiver ativo e credenciais completas. |
| CA-INT-006 | Webhook deve respeitar metodo, acao, URL, status ativo, tenant e payload padrao. |
| CA-INT-007 | Reuniao externa deve persistir identificador, links e status apos criacao remota bem-sucedida. |
| CA-INT-008 | Callback de pagamento deve validar autenticidade, registrar payload bruto protegido e deduplicar referencia. |
| CA-INT-009 | API de arquivos deve validar chave, usuario, acao, propriedade e permissao sensivel. |
| CA-INT-010 | Polling deve retornar somente dados autorizados do usuario autenticado. |
| CA-INT-011 | Segredos nao podem ser exibidos integralmente em consulta ou log. |
| CA-INT-012 | Todas as execucoes devem gerar rastreabilidade funcional suficiente para suporte. |

## 18. Notas de autoria e saneamento funcional

[^nota1]: O modelo funcional consolidado foi criado nesta EF para tornar o Epros implantavel. O material informa campos e estruturas parciais por familia de conector, mas nao informa um modelo unico definitivo para catalogo, credenciais, entregas, auditoria e polling.
[^nota2]: Regras de protecao de segredo, TLS, limite de taxa, auditoria, mascaramento e logs seguros foram incluidas como saneamento funcional necessario para padrao de produto empresarial. O material aponta riscos e lacunas nessas areas, sem especificar politica final.
[^nota3]: A fronteira de pagamentos foi saneada nesta EF: Integracoes e Conectores controla configuracao tecnica, checkout, callback, assinatura, payload e rastreabilidade; o modulo Financeiro deve controlar efeito financeiro, conciliacao contabil, baixa, inadimplencia, assinatura e reflexos de cobranca.
