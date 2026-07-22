# MC_3_PLATAFORMA_COMPARTILHADA_INTEGRACOES_E_CONECTORES_V1

**Projeto:** Epros  
**Empresa:** Siser  
**Modulo:** PLATAFORMA_COMPARTILHADA  
**Submodulo:** INTEGRACOES_E_CONECTORES  
**Documento:** Matriz de completude  
**Versao:** V1  
**Status:** Concluido  
**Ultima atualizacao:** 2026-06-09

## 1. Objetivo

Registrar o nivel de completude do submodulo Integracoes e Conectores, separando capacidades comprovadas no material, estruturas funcionais saneadas para implantacao e lacunas que dependem de decisao da Siser antes de construcao final.

## 2. Resumo de cobertura

| Area | Status | Evidencia funcional consolidada |
|---|---|---|
| Catalogo de conectores | Parcial | Material consolida familias de IA, captcha, mensageria, webhook, reuniao, pagamento, API de arquivos e polling. |
| Configuracao por tenant | Parcial | Material informa configuracoes por integracao e isolamento por criador/tenant. |
| Permissao e plano | Parcial | Material informa autenticacao, plano e permissao para varias operacoes. |
| Credenciais | Parcial | Material informa chaves, tokens, secrets, client id, account id, chat id, from e webhook URL. |
| IA generativa | Parcial | Provedor, modelo, chave, prompts, fallback, idioma, maxLength e criatividade informados. |
| Captcha | Parcial | v2/v3, enabled, site key, secret, token vazio e validacao remota informados. |
| Mensageria | Parcial | Slack, Telegram e Twilio informados com templates, credenciais, flags e payloads. |
| Webhooks de saida | Parcial | Metodo, acao, URL, status, criador, payload padrao e eventos dinamicos informados. |
| Reunioes externas | Parcial | Credenciais, criacao remota, links, status e operacoes principais informados. |
| Pagamentos tecnicos | Parcial | Gateways, modo, checkout, callbacks, assinatura, deduplicacao e status tecnico informados. |
| API de arquivos | Parcial | Chave, usuario, acao, arquivo, permissao, propriedade e JSON success/error informados. |
| Polling operacional | Parcial | Contadores de notificacoes, lembretes, mensagens e timers informados. |
| Observabilidade | Incompleto | Material aponta necessidade de historico, healthcheck e sucesso/falha, sem modelo completo. |
| Seguranca operacional | Incompleto | Material aponta lacunas de TLS, limite de taxa, auditoria e validacao robusta. |

## 3. Itens de completude

| Codigo | Area | Status | O que existe | O que falta | Prioridade |
|---|---|---|---|---|---|
| MC-INT-001 | Escopo | Parcial | Familias de conectores e fronteiras gerais. | Validar quais conectores entram na primeira entrega. | P0 |
| MC-INT-002 | Catalogo | Parcial | Conectores e eventos citados. | Definir catalogo oficial, nomes, tipos, owners e status. | P0 |
| MC-INT-003 | Configuracao tipada | Parcial | Chaves de configuracao por conector informadas. | Criar dominio definitivo de parametros, tipos, obrigatoriedade e validadores. | P0 |
| MC-INT-004 | Credenciais | Parcial | Api key, secret, token, SID, chat id, account id, client id e webhook secret informados. | Definir cofre, mascaramento, rotacao, expiracao e trilha de acesso. | P0 |
| MC-INT-005 | Plano/permissao | Parcial | Validacoes de plano e permissao citadas. | Consolidar matriz de permissoes por conector e acao. | P0 |
| MC-INT-006 | IA generativa | Parcial | Provider, model, api key, prompts, idioma, maxLength e criatividade. | Definir provedores homologados, limites, custo, retencao e politica de dados. | P0 |
| MC-INT-007 | Prompts | Parcial | module, submodule, field_type, prompt_template e status informados. | Definir versionamento, aprovacao, variaveis permitidas e historico de mudanca. | P1 |
| MC-INT-008 | Captcha | Parcial | v2/v3, site key, secret, enabled, token vazio e verificacao remota. | Definir telas protegidas, score minimo para v3, mensagens e retencao. | P0 |
| MC-INT-009 | Mensageria | Parcial | Canais, templates, payloads e credenciais informados. | Definir catalogo oficial dos 83 eventos, destinatarios e regras de opt-out. | P0 |
| MC-INT-010 | Templates | Parcial | Templates por tipo e fallback para ingles informados. | Definir schema de placeholders e validacao por canal. | P1 |
| MC-INT-011 | Webhooks de saida | Parcial | method, action, url, is_active, criador, payload e mapeamento. | Definir retry, backoff, timeout, assinatura, headers permitidos e idempotencia. | P0 |
| MC-INT-012 | Eventos dinamicos | Parcial | Eventos/listeners e ignorar nao mapeados informados. | Validar catalogo completo de eventos publicaveis. | P0 |
| MC-INT-013 | Reunioes externas | Parcial | Credenciais, links, status e booleans informados. | Definir timezone, participantes obrigatorios, convites, webhook de status e permissao detalhada. | P1 |
| MC-INT-014 | Pagamentos tecnicos | Parcial | Checkout, modo, moeda, assinatura, IPN/confirmacao, callbacks e deduplicacao. | Validar fronteira final com Financeiro, retencao de payload e eventos financeiros oficiais. | P0 |
| MC-INT-015 | Moeda e valores | Parcial | Whitelist de moeda e unidade menor informadas. | Definir moedas oficiais, arredondamento e validacao por gateway. | P0 |
| MC-INT-016 | Callback pagamento | Parcial | Eventos, payload bruto, assinatura, tolerancia e status tecnico. | Definir resposta padrao, filas, reprocesso e conciliacao tecnica. | P0 |
| MC-INT-017 | API de arquivos | Parcial | key, username, action, file_id, server_id, file_path, upload e update. | Definir contrato final, escopos de chave, limite de taxa, TLS e retencao de logs. | P0 |
| MC-INT-018 | Acoes de arquivo | Parcial | list, info, delete, move, copy, rawget, rawdelete, upload e update informadas. | Validar nomes oficiais, permissao por acao e comportamento de erro. | P1 |
| MC-INT-019 | Polling | Parcial | Notificacoes, lembretes, mensagens e timers informados. | Definir endpoint, frequencia, limites, cache e payload final. | P1 |
| MC-INT-020 | Observabilidade | Incompleto | Necessidade de healthcheck, historico e sucesso/falha citada. | Definir modelo final de tentativa, correlacao, indicadores e alertas. | P0 |
| MC-INT-021 | Seguranca | Incompleto | Riscos de TLS, validacao, limite de taxa e logs citados. | Formalizar politica de TLS, assinatura, mascaramento, rate limit e bloqueio. | P0 |
| MC-INT-022 | Testes | Parcial | Cenarios de IA, captcha, mensageria, webhook, reuniao e pagamento citados. | Criar massas, mocks externos, testes de falha, duplicidade, assinatura e carga. | P0 |
| MC-INT-023 | Telas | Parcial | Telas de configuracao, monitoramento e checkout citadas. | Definir wireframes, campos obrigatorios, mensagens e permissoes por tela. | P1 |
| MC-INT-024 | Relatorios | Incompleto | Material solicita healthcheck, historico, taxa de sucesso e monitor. | Definir relatorios oficiais e indicadores por area. | P1 |
| MC-INT-025 | Retencao | Pendente | Payload bruto e logs citados. | Definir prazos por tipo de dado, mascaramento e expurgo. | P0 |

## 4. Decisoes pendentes

| Codigo | Decisao | Motivo |
|---|---|---|
| D-INT-001 | Confirmar conectores homologados para primeira entrega. | Define escopo, testes e suporte. |
| D-INT-002 | Definir catalogo oficial de eventos acionaveis. | Necessario para mensageria e webhooks. |
| D-INT-003 | Definir cofre, rotacao e mascaramento de credenciais. | Necessario para seguranca empresarial. |
| D-INT-004 | Definir provedores de IA, limites, custo e politica de dados. | Necessario para uso seguro de IA. |
| D-INT-005 | Definir telas protegidas por captcha e regras v3. | Necessario para validacao operacional. |
| D-INT-006 | Definir retry, timeout, assinatura e idempotencia de webhooks. | Necessario para confiabilidade de integracao. |
| D-INT-007 | Validar fronteira de pagamento com Financeiro. | Evita duplicidade de regra financeira. |
| D-INT-008 | Definir contrato final da API de arquivos. | Necessario para exposicao segura. |
| D-INT-009 | Definir limites de polling e payload final. | Necessario para desempenho. |
| D-INT-010 | Definir retencao de payloads, logs e callbacks. | Necessario para privacidade, auditoria e custo. |

## 5. Riscos funcionais

| Risco | Impacto | Mitigacao proposta |
|---|---|---|
| Credenciais expostas em tela ou log. | Vazamento de segredo e risco operacional. | Mascarar segredos, usar armazenamento protegido e auditar acesso. |
| Webhooks sem retry/idempotencia. | Evento perdido ou processado em duplicidade. | Definir tentativa, deduplicacao, backoff e correlacao. |
| Pagamento tecnico assumindo regra financeira. | Baixa ou ativacao incorreta. | Manter efeito financeiro no modulo Financeiro. |
| Evento sem catalogo oficial. | Mensagem inconsistente e integracao instavel. | Criar catalogo versionado de eventos. |
| API de arquivos sem limite de taxa. | Abuso ou indisponibilidade. | Aplicar limite, TLS, escopo de chave e auditoria. |
| Prompt sem validacao de contexto. | Envio de dado indevido a provedor externo. | Validar campos, excluir identificadores e aplicar politica de dados. |

## 6. Proximo passo operacional

O submodulo `PLATAFORMA_COMPARTILHADA/INTEGRACOES_E_CONECTORES` foi processado e esta concluido como conteudo parcial-controlado. O proximo item da matriz principal e `PLATAFORMA_COMPARTILHADA/INTERFACE_ASSISTIDA_WIZARDS`.
