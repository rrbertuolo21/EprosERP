# MC_3_PLATAFORMA_COMPARTILHADA_INTERFACE_ASSISTIDA_WIZARDS_V1

**Projeto:** Epros  
**Empresa:** Siser  
**Modulo:** PLATAFORMA_COMPARTILHADA  
**Submodulo:** INTERFACE_ASSISTIDA_WIZARDS  
**Documento:** Matriz de completude  
**Versao:** V1  
**Status:** Concluido  
**Ultima atualizacao:** 2026-06-09

## 1. Objetivo

Registrar o nivel de completude do submodulo Interface Assistida e Wizards, separando capacidades comprovadas no material, estruturas funcionais criadas para implantacao do Epros e lacunas que precisam de validacao da Siser antes da construcao final.

## 2. Resumo de cobertura

| Area | Status | Evidencia funcional consolidada |
|---|---|---|
| Formulario dinamico | Parcial | Material informa entidade de formulario, nome, codigo publico, status ativo, layout, owner e ator. |
| Campos dinamicos | Parcial | Material informa label, type, required, placeholder, options, order, owner e ator. |
| Respostas | Parcial | Material informa resposta por formulario com response_data JSON. |
| Conversao | Parcial | Material informa modulo, submodulo, status ativo e field_mappings. |
| Canal publico | Parcial | Material informa acesso por codigo publico e formulario ativo. |
| Validacao dinamica | Parcial | Material informa regras por tipo e obrigatoriedade. |
| Builder visual | Parcial | Material informa abas build/settings/style/embed e palette de campos. |
| Workflow | Parcial | Material informa Rascunho, EmAnalise, Ativo, Inativo e Encerrado. |
| Wizard multi-etapa | Incompleto | Material cita passos, rascunho, validacao cross-step e aplicacao idempotente, mas nao informa tabelas finais. |
| APIs | Pendente | Endpoints finais nao foram informados. |
| Seguranca publica | Incompleto | Material informa acesso publico, estilo e HTML; politica final de sanitizacao nao informada. |
| Relatorios | Parcial | Posicao geral, auditoria, respostas e telemetria de abandono citadas. |

## 3. Itens de completude

| Codigo | Area | Status | O que existe | O que falta | Prioridade |
|---|---|---|---|---|---|
| MC-WIZ-001 | Escopo | Parcial | Formularios dinamicos, wizards, captura publica, respostas e conversao. | Validar se primeira entrega inclui wizard multi-etapa completo ou apenas formularios dinamicos. | P0 |
| MC-WIZ-002 | Formulario | Parcial | id, name, code, is_active, default_layout, creator_id, created_by e timestamps. | Definir codigo funcional, versionamento, responsavel, status operacional e regras de unicidade finais. | P0 |
| MC-WIZ-003 | Campos | Parcial | label, type, required, placeholder, options, order e ownership. | Definir tipos oficiais, limites, mascara, validadores extras e suporte a arquivo. | P0 |
| MC-WIZ-004 | Tipos de campo | Parcial | text, email, number, tel, url, password, textarea, select, radio, checkbox, date, time, file, header e paragraph citados/consolidados. | Validar quais entram no produto inicial e regras por tipo. | P0 |
| MC-WIZ-005 | Layout | Parcial | single, two-column e card informados. | Definir responsividade, acessibilidade e padrao visual. | P1 |
| MC-WIZ-006 | Canal publico | Parcial | Codigo publico, formulario ativo e submissao sem backoffice. | Definir URL final, expiracao/revogacao de codigo, captcha e protecao contra abuso. | P0 |
| MC-WIZ-007 | Validacao dinamica | Parcial | Required/nullable, email, number, tel, url, date, time, checkbox, select/radio e string. | Definir mensagens oficiais, localizacao, limites de tamanho e validadores customizados. | P0 |
| MC-WIZ-008 | Respostas | Parcial | response_data JSON por field_id, busca, recencia e paginacao. | Definir exportacao, retencao, mascaramento, pesquisa estruturada e anexos. | P0 |
| MC-WIZ-009 | Conversao | Parcial | module_name, submodule_name, is_active, field_mappings, modulo ativo e permissao. | Definir catalogo oficial de destinos, contratos, idempotencia e reprocesso. | P0 |
| MC-WIZ-010 | Falha de conversao | Parcial | Falha nao bloqueia sucesso publico. | Definir fila, alerta, retry, status e tela de falhas. | P0 |
| MC-WIZ-011 | Builder visual | Parcial | Palette, drag/ordenacao, abas build/settings/style/embed. | Definir biblioteca final, comportamento mobile, acessibilidade e preview. | P1 |
| MC-WIZ-012 | Estilo | Parcial | CSS customizado informado. | Definir politica de sanitizacao, escopo de CSS e temas permitidos. | P0 |
| MC-WIZ-013 | Embed | Parcial | URL direta e iframe com dimensoes de referencia. | Definir dominios permitidos, CSP, parametros, responsividade e revogacao. | P0 |
| MC-WIZ-014 | Workflow | Parcial | Estados e transicoes principais informados. | Definir quando formulario exige aprovacao, motivos e papel aprovador. | P1 |
| MC-WIZ-015 | Wizard multi-etapa | Incompleto | Passos, rascunho, validacao cross-step e abandono citados. | Definir modelo final de passos, dependencias, rascunho, expiracao e aplicacao idempotente. | P0 |
| MC-WIZ-016 | Historico | Parcial | Acao, UsuarioId e PayloadJson informados. | Definir entidade final, payload mascarado, antes/depois e retencao. | P0 |
| MC-WIZ-017 | Anexos | Parcial | ArquivoId via GED informado. | Definir campo file, tamanho, tipos permitidos, antivirus e relacionamento com resposta. | P1 |
| MC-WIZ-018 | Permissoes | Parcial | Gestao, criar, editar, excluir, campos, respostas e conversao informadas. | Consolidar matriz oficial de permissoes por acao e escopo any/own. | P0 |
| MC-WIZ-019 | APIs | Pendente | Contratos funcionais inferidos, endpoints finais nao informados. | Definir rotas, metodos, payloads, codigos de erro e versionamento. | P0 |
| MC-WIZ-020 | Relatorios | Parcial | Posicao geral, auditoria, respostas e abandono citados. | Definir colunas, filtros, exportacao e indicadores oficiais. | P1 |
| MC-WIZ-021 | Privacidade | Incompleto | LGPD/retencao citadas genericamente. | Definir base legal por formulario, consentimento, minimizacao, retencao e anonimizacao. | P0 |
| MC-WIZ-022 | Testes | Parcial | Cenarios basicos informados e EF ampliou casos. | Criar massa de testes, testes publicos, conversao, seguranca, embed e carga. | P0 |

## 4. Decisoes pendentes

| Codigo | Decisao | Motivo |
|---|---|---|
| D-WIZ-001 | Confirmar se MVP sera formulario dinamico, wizard multi-etapa ou ambos. | Define modelo de dados, telas e testes. |
| D-WIZ-002 | Definir catalogo oficial de tipos de campo. | Necessario para validacao e renderizacao. |
| D-WIZ-003 | Definir URL publica, politica de codigo e revogacao. | Necessario para seguranca publica. |
| D-WIZ-004 | Definir se conversao permite um ou multiplos destinos por formulario. | Material informa uma conversao por formulario, mas o Epros pode precisar de multiplos destinos. |
| D-WIZ-005 | Definir destinos homologados para conversao. | Evita acoplamento indevido com modulos. |
| D-WIZ-006 | Definir politica de sanitizacao de CSS, HTML e embed. | Necessario para exposicao publica segura. |
| D-WIZ-007 | Definir retencao e mascaramento de respostas. | Respostas podem conter dados pessoais. |
| D-WIZ-008 | Definir matriz de permissoes any/own por acao. | Necessario para ownership e suporte. |
| D-WIZ-009 | Definir contratos finais de API. | Endpoints finais nao constam no material. |
| D-WIZ-010 | Definir telemetria de abandono de passo e sucesso de conversao. | Necessario para melhoria operacional. |

## 5. Riscos funcionais

| Risco | Impacto | Mitigacao proposta |
|---|---|---|
| Formulario publico sem protecao contra abuso. | Spam, carga excessiva e dados ruins. | Captcha, limite de taxa, auditoria e revogacao de codigo. |
| CSS/HTML publico sem sanitizacao. | Risco de conteudo inseguro. | Politica de sanitizacao e preview seguro. |
| Conversao sem idempotencia. | Duplicidade no modulo destino. | Chave de correlacao por resposta/conversao. |
| Falha de conversao invisivel. | Perda operacional apesar da resposta salva. | Registrar tentativa, status, erro e alerta. |
| Campos dinamicos sem limites. | Payload excessivo e baixa qualidade de dado. | Limites por tipo, tamanho, opcoes e anexos. |
| Respostas com dado pessoal sem retencao. | Risco de privacidade. | Base legal, retencao, mascaramento e anonimizacao. |

## 6. Proximo passo operacional

O submodulo `PLATAFORMA_COMPARTILHADA/INTERFACE_ASSISTIDA_WIZARDS` foi processado e esta concluido como conteudo parcial-controlado. O proximo item da matriz principal e `PLATAFORMA_COMPARTILHADA/OFFLINE_SHELL`.
