# EF_3_PLATAFORMA_COMPARTILHADA_INTERFACE_ASSISTIDA_WIZARDS_V1

**Projeto:** Epros  
**Empresa:** Siser  
**Tipo de documento:** Especificacao Funcional definitiva  
**Versao:** V1  
**Modulo:** PLATAFORMA_COMPARTILHADA  
**Submodulo:** INTERFACE_ASSISTIDA_WIZARDS  
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

O submodulo Interface Assistida e Wizards do Epros deve permitir a criacao, manutencao, execucao e monitoramento de experiencias declarativas de coleta guiada, formularios dinamicos, assistentes multi-etapa e conversoes controladas para outros modulos. O objetivo e reduzir formularios fixos duplicados, permitir captura publica segura por codigo unico, salvar rascunhos, validar campos dinamicamente, registrar respostas e acionar conversoes sem transferir para este submodulo as regras internas dos modulos de destino.

| Pergunta | Resposta |
|---|---|
| Para que o submodulo existe? | Para configurar e operar formularios dinamicos e wizards assistidos com campos, passos, validacoes, respostas, publicacao e conversao. |
| Que problema de negocio resolve? | Permite criar jornadas guiadas sem alterar codigo a cada novo formulario, onboarding, captura ou parametrizacao complexa. |
| Qual resultado operacional deve produzir? | Formularios/wizards publicados, respostas validadas e registradas, conversoes opcionais executadas, falhas auditadas e dados consultaveis. |
| Quais areas dependem dele? | Aplicativo, Cadastros, Vendas/CRM, Financeiro, Fiscal, GED, Workflow, IA/ML, Analytics e modulos com configuracoes complexas. |

## 3. Escopo funcional

### 3.1 Dentro do escopo

| Capacidade | Descricao | Observacao |
|---|---|---|
| Cadastro de formulario/wizard | Criar estrutura declarativa com nome, codigo publico, status, layout, owner e ator. | Material informa `forms`. |
| Builder de campos | Montar campos dinamicos, tipos, labels, obrigatoriedade, placeholder, opcoes e ordem. | Material informa `form_fields`. |
| Canal publico por codigo | Expor formulario ativo por codigo publico unico, sem exigir usuario autenticado quando configurado. | Material informa acesso por `code`. |
| Canal autenticado | Permitir gestao, respostas, conversao e operacoes administrativas com permissao. | Material informa permissoes granulares. |
| Validacao dinamica | Gerar regras de validacao conforme tipo e obrigatoriedade de cada campo. | Material informa regras por tipo. |
| Persistencia de respostas | Salvar respostas por formulario com dados indexados pelo campo. | Material informa `form_responses`. |
| Conversao declarativa | Mapear campos de resposta para modulo/submodulo de destino e acionar criacao minima quando ativo. | Material informa `form_conversions`. |
| Conversao tolerante a falhas | Falha de conversao nao deve impedir sucesso da submissao publica. | Material informa esta tolerancia. |
| Workflow de publicacao | Controlar ciclo Rascunho, EmAnalise, Ativo, Inativo e Encerrado quando aplicavel. | Material informa maquina de estados. |
| Estilo e embed | Permitir configuracao visual, URL direta e codigo de incorporacao. | Material informa abas build/settings/style/embed. |
| Respostas e relatorios | Consultar respostas, filtrar, paginar, exportar, ver contadores e auditoria. | Material informa contadores e tela de respostas. |
| Historico e anexos | Registrar alteracoes, usuario, payload e anexos por GED quando aplicavel. | Material informa historico e anexo. |

### 3.2 Fora do escopo

| Item fora do escopo | Motivo | Destino correto |
|---|---|---|
| Regra completa do modulo convertido | O wizard mapeia e aciona criacao minima; o modulo destino governa seu ciclo de vida. | Modulo destino |
| Cadastro mestre de pessoa, cliente, produto, titulo ou documento | O wizard referencia ou coleta dados, mas nao substitui cadastros mestres. | Cadastros/Base ou modulo dono |
| Motor de workflow corporativo completo | O wizard pode exigir aprovacao, mas nao substitui Workflow. | Workflow |
| Armazenamento documental | Anexos devem referenciar GED. | GED |
| Captcha corporativo | O wizard pode exigir captcha, mas a configuracao tecnica e do submodulo de integracoes. | Integracoes e Conectores |
| IA generativa | IA pode auxiliar preenchimento ou sugestao, mas seu governo pertence a IA/ML. | IA/ML |

## 4. Glossario e conceitos funcionais

| Termo | Definicao funcional | Observacoes |
|---|---|---|
| Formulario dinamico | Estrutura configuravel de campos, layout e publicacao. | Base comprovada no material. |
| Wizard | Experiencia guiada por passos, dependencias, validacoes e rascunho. | Parte do escopo de interface assistida.[^nota1] |
| Campo dinamico | Campo configuravel com label, tipo, obrigatoriedade, opcoes e ordem. | Tipos informados no material. |
| Codigo publico | Identificador unico usado para acessar formulario publicado. | Deve ser unico e nao sequencial. |
| Resposta | Conjunto de valores submetidos para um formulario. | Salvo em estrutura JSON indexada por campo. |
| Conversao | Configuracao que transforma uma resposta em criacao minima no modulo destino. | Opcional e tolerante a falhas. |
| Owner | Usuario ou conta dona do formulario no tenant. | Material informa owner/ator. |
| Ator | Usuario que executa a operacao. | Material informa criador e usuario logado. |
| Layout | Forma de apresentacao do formulario. | `single`, `two-column` e `card` informados. |
| Embed | Codigo para incorporar formulario em outra pagina. | Material informa iframe e URL direta. |

## 5. Atores, papeis e responsabilidades

| Ator/Papel | Responsabilidade | Permissoes esperadas | Restricoes |
|---|---|---|---|
| Administrador do tenant | Gerir formularios, campos, configuracoes, conversoes e publicacao. | Criar, editar, excluir, publicar, configurar conversao, ver respostas. | Deve respeitar ownership e tenant. |
| Gestor de formulario | Manter formulario, campos, estilo e respostas permitidas. | Editar formulario, campos, respostas e exportar. | Nao altera configuracoes globais. |
| Operador | Consultar lista, respostas e historico conforme permissao. | Visualizar e exportar. | Nao altera estrutura sem permissao. |
| Usuario publico | Preencher formulario ativo por codigo publico. | Enviar respostas. | Nao acessa lista nem respostas. |
| Modulo destino | Receber dados convertidos quando habilitado. | Criacao minima conforme contrato. | Regras internas permanecem no modulo destino. |
| Epros | Validar, salvar, converter, auditar e publicar eventos. | Automacao sistemica. | Nao converte sem mapeamento, modulo ativo e permissao. |

## 6. Visao operacional do submodulo

O administrador cria um formulario ou wizard no Epros, define nome, layout, status, codigo publico e ownership. Em seguida configura campos dinamicos com tipo, label, obrigatoriedade, placeholder, opcoes e ordem. O formulario pode ser gerido em canal autenticado e exposto em canal publico por codigo unico quando ativo. A listagem permite filtro por nome, status, paginacao, ordenacao e exibicao de contadores de campos e respostas.

Quando um usuario publico acessa o codigo, o Epros carrega somente formularios ativos, normaliza opcoes de campo e renderiza os campos na ordem configurada. Na submissao, o Epros monta validacoes dinamicas por tipo e obrigatoriedade, bloqueia payload vazio, salva resposta por campo e dispara evento de dominio. Se houver conversao ativa, o Epros aplica mapeamento campo a campo, verifica modulo ativo e permissao de criacao, e tenta criar o registro minimo no modulo destino. Falha de conversao deve ser registrada, mas nao deve desfazer uma submissao publica ja aceita.

Quando o formulario for usado como wizard multi-etapa, o Epros deve permitir rascunho por usuario/tenant, validacao por passo, validacao entre passos, dependencias condicionais e aplicacao idempotente no processo de destino. O material nao informa tabelas fisicas definitivas para passos e rascunhos; esta EF inclui estrutura implantavel com nota de autoria.[^nota1]

## 7. Capacidades funcionais

### 7.1 Cadastro e listagem de formularios/wizards

| Item | Especificacao |
|---|---|
| Objetivo | Permitir gestao de formularios e wizards por tenant. |
| Acionamento | Usuario acessa lista, cria, edita, inativa ou exclui estrutura. |
| Pre-condicoes | Usuario autenticado, plano/modulo habilitado e permissao aplicavel. |
| Dados de entrada | Nome, codigo, status, layout, owner, ator, tipo, configuracoes e observacoes. |
| Processamento | O Epros valida permissao, escopo any/own, tenant, obrigatorios e unicidade do codigo. |
| Resultado esperado | Formulario/wizard salvo, listado e auditado. |
| Pos-condicoes | Campos podem ser configurados e formulario pode ser publicado. |
| Excecoes | Sem permissao, fora do escopo, codigo duplicado, tenant ausente ou nome ausente. |
| Auditoria | Criacao, edicao, exclusao, status, owner, ator, antes/depois e IP. |

### 7.2 Builder de campos

| Item | Especificacao |
|---|---|
| Objetivo | Montar estrutura declarativa de campos dinamicos. |
| Acionamento | Criacao, edicao individual, edicao em massa ou exclusao de campo. |
| Pre-condicoes | Formulario existente e usuario com permissao de editar campos. |
| Dados de entrada | Label, tipo, obrigatoriedade, placeholder, opcoes, ordem e metadados. |
| Processamento | O Epros valida tipos permitidos, serializa opcoes como JSON, recalcula ordem e salva campos. |
| Resultado esperado | Campos persistidos e ordenados. |
| Pos-condicoes | Formulario pode ser renderizado e validado dinamicamente. |
| Excecoes | Tipo invalido, opcoes invalidas, campo fora do formulario, permissao ausente. |
| Auditoria | Alteracao de estrutura e responsavel pela mudanca. |

### 7.3 Captura publica por codigo

| Item | Especificacao |
|---|---|
| Objetivo | Permitir que usuario externo envie resposta sem acessar backoffice. |
| Acionamento | Acesso a URL publica por codigo. |
| Pre-condicoes | Formulario ativo, codigo valido e contexto publico resolvido. |
| Dados de entrada | Codigo publico e dados dos campos. |
| Processamento | O Epros carrega formulario ativo, normaliza opcoes, monta estado inicial e valida submissao. |
| Resultado esperado | Resposta salva ou erro de validacao apresentado. |
| Pos-condicoes | Evento de resposta enviada e tentativa de conversao quando ativa. |
| Excecoes | Codigo inexistente, formulario inativo, payload vazio, validacao falha ou captcha falho quando exigido. |
| Auditoria | Codigo, tenant, IP, formulario, status e erro. |

### 7.4 Validacao dinamica

| Item | Especificacao |
|---|---|
| Objetivo | Validar respostas conforme metadados dos campos. |
| Acionamento | Submissao publica ou autenticada. |
| Pre-condicoes | Campos carregados e formulario ativo. |
| Dados de entrada | Tipo, obrigatoriedade, opcoes, valor e label. |
| Processamento | O Epros gera regra por campo, aplica mensagem usando label e bloqueia resposta vazia. |
| Resultado esperado | Dados validos persistidos. |
| Pos-condicoes | Resposta pode ser convertida. |
| Excecoes | Email invalido, numero invalido, URL invalida, data invalida, hora fora de HH:mm, opcao fora do dominio, checkbox invalido. |
| Auditoria | Erros de validacao por campo e formulario. |

### 7.5 Respostas

| Item | Especificacao |
|---|---|
| Objetivo | Salvar e consultar respostas de formularios/wizards. |
| Acionamento | Submissao aceita ou consulta de backoffice. |
| Pre-condicoes | Formulario valido e permissao para consulta quando backoffice. |
| Dados de entrada | Formulario, campos, valores, IP, usuario quando houver e contexto. |
| Processamento | O Epros salva dados indexados pelo identificador do campo e atualiza contadores. |
| Resultado esperado | Resposta consultavel, paginada e exportavel. |
| Pos-condicoes | Conversao e relatorios podem consumir a resposta. |
| Excecoes | Resposta sem formulario, exclusao sem permissao, busca fora do escopo. |
| Auditoria | Criacao, consulta sensivel, exclusao e exportacao. |

### 7.6 Conversao para modulos

| Item | Especificacao |
|---|---|
| Objetivo | Transformar resposta em registro minimo no modulo destino conforme mapeamento. |
| Acionamento | Submissao aceita ou reprocesso autorizado. |
| Pre-condicoes | Conversao ativa, modulo/submodulo de destino definido, modulo ativo e permissao de criacao. |
| Dados de entrada | Formulario, resposta, modulo, submodulo, field_mappings e valores fixos. |
| Processamento | O Epros aplica mapeamento campo destino -> campo de origem ou valor fixo, valida destino e chama contrato do modulo. |
| Resultado esperado | Registro minimo criado ou falha registrada. |
| Pos-condicoes | Modulo destino passa a governar o ciclo de vida do registro. |
| Excecoes | Conversao inativa, modulo inativo, permissao ausente, mapeamento incompleto, modulo nao suportado, erro do destino. |
| Auditoria | Resultado, destino, mapeamento aplicado, identificador destino e erro. |

### 7.7 Wizard multi-etapa

| Item | Especificacao |
|---|---|
| Objetivo | Guiar usuario por etapas, com rascunho e validacao incremental. |
| Acionamento | Usuario inicia processo assistido. |
| Pre-condicoes | Wizard ativo e usuario autorizado quando nao for publico. |
| Dados de entrada | Passos, campos por passo, dependencias, regras cross-step e payload parcial. |
| Processamento | O Epros salva rascunho por usuario/tenant, valida passo atual, controla avancar/voltar e aplica processo final de forma idempotente. |
| Resultado esperado | Processo guiado concluido ou rascunho preservado. |
| Pos-condicoes | Evento de conclusao e aplicacao no processo destino. |
| Excecoes | Passo invalido, dependencia nao satisfeita, rascunho expirado, conflito de versao ou aplicacao repetida. |
| Auditoria | Passo, abandono, retomada, validacao, conclusao e aplicacao. |

### 7.8 Estilo, URL direta e embed

| Item | Especificacao |
|---|---|
| Objetivo | Permitir apresentacao publica controlada. |
| Acionamento | Gestor altera estilo, consulta URL direta ou copia embed. |
| Pre-condicoes | Formulario existente e permissao de edicao. |
| Dados de entrada | CSS permitido, configuracoes visuais, URL direta, dimensoes de embed. |
| Processamento | O Epros sanitiza estilo, gera URL direta e codigo de incorporacao seguro. |
| Resultado esperado | Formulario incorporavel sem expor dados internos. |
| Pos-condicoes | Canal publico pode ser usado em pagina externa. |
| Excecoes | CSS invalido, formulario inativo, permissao ausente ou URL inexistente. |
| Auditoria | Alteracao de estilo e geracao de embed. |

## 8. Regras de negocio

| Regra | Descricao | Condicao | Resultado | Severidade | Observacoes |
|---|---|---|---|---|---|
| WIZ-001 | Toda operacao administrativa exige usuario autenticado. | Backoffice. | Operacao bloqueada sem autenticacao. | Bloqueante | Material informa rotas privadas com autenticacao. |
| WIZ-002 | Acesso administrativo exige plano/modulo habilitado. | Backoffice. | Operacao bloqueada. | Bloqueante | Material informa gate por modulo. |
| WIZ-003 | Listagem exige permissao de gestao. | Consulta administrativa. | Lista nao e exibida. | Bloqueante | Regra informada. |
| WIZ-004 | Escopo any lista formularios do owner do tenant. | Usuario com permissao ampla. | Lista filtrada por owner. | Bloqueante | Material informa owner. |
| WIZ-005 | Escopo own lista formularios do ator logado. | Usuario com permissao propria. | Lista filtrada por ator. | Bloqueante | Material informa ator. |
| WIZ-006 | Sem escopo valido, a lista nao retorna registros. | Listagem administrativa. | Resultado vazio. | Bloqueante | Material informa fallback sem resultados. |
| WIZ-007 | Listagem deve exibir contadores de campos e respostas. | Consulta administrativa. | Contadores exibidos. | Normal | Material informa withCount. |
| WIZ-008 | Listagem deve permitir filtro textual por nome. | Consulta com filtro. | Resultado filtrado. | Normal | Material informa filtro por name. |
| WIZ-009 | Listagem deve permitir filtro por ativo/inativo. | Consulta com status. | Resultado filtrado. | Normal | Material informa filtro is_active. |
| WIZ-010 | Listagem deve permitir ordenacao dinamica com fallback por criacao descendente. | Consulta. | Resultado ordenado. | Normal | Material informa fallback. |
| WIZ-011 | Listagem deve permitir paginacao parametrizavel com padrao 10. | Consulta. | Resultado paginado. | Normal | Material informa per_page 10. |
| WIZ-012 | Criacao exige permissao especifica. | Criar formulario. | Operacao bloqueada sem permissao. | Bloqueante | Regra informada. |
| WIZ-013 | Criacao deve aplicar validacao dedicada. | Criar formulario. | Dados invalidos sao rejeitados. | Bloqueante | Regra informada. |
| WIZ-014 | Codigo publico deve ser gerado automaticamente e ser unico. | Criar formulario. | Codigo unico salvo. | Bloqueante | Material informa code unique. |
| WIZ-015 | Novo formulario nasce ativo por padrao quando o material de origem assim configurar. | Criar formulario. | `is_active=true` se nao informado. | Normal | Regra informada. |
| WIZ-016 | Layout padrao deve ser `single`. | Criar/editar formulario. | Layout default aplicado. | Normal | Material informa default. |
| WIZ-017 | Formulario deve gravar owner e ator. | Criar/editar. | Rastreabilidade preservada. | Bloqueante | Material informa created_by e creator_id. |
| WIZ-018 | Campos podem ser criados em lote quando enviados. | Criacao/edicao. | Todos os campos validos sao salvos. | Normal | Regra informada. |
| WIZ-019 | Campo `required` assume falso por padrao. | Criacao de campo. | Campo opcional se nao informado. | Normal | Regra informada. |
| WIZ-020 | Opcoes de campo devem ser persistidas como JSON. | Campo com opcoes. | Opcoes serializadas. | Bloqueante | Regra informada. |
| WIZ-021 | Ordem de campo padrao e zero. | Campo sem ordem. | Ordem 0 aplicada. | Normal | Regra informada. |
| WIZ-022 | Campos herdam owner e ator da operacao. | Criacao/edicao de campo. | Rastreabilidade preservada. | Bloqueante | Regra informada. |
| WIZ-023 | Criacao, edicao e exclusao devem disparar evento de dominio. | Mudanca estrutural. | Evento publicado apos persistencia. | Normal | Material informa eventos. |
| WIZ-024 | Edicao exige permissao especifica e reaplica escopo any/own. | Editar formulario. | Fora do escopo e bloqueado. | Bloqueante | Regra informada. |
| WIZ-025 | Edicao deve alterar metadados centrais. | Atualizar formulario. | Nome, status e layout atualizados. | Normal | Regra informada. |
| WIZ-026 | Edicao de campos pode usar upsert por identificador do campo. | Atualizar campos. | Campo existente atualiza, novo campo cria. | Normal | Regra informada. |
| WIZ-027 | Opcoes invalidas ou ausentes devem ficar nulas em update. | Atualizar campo. | Campo salvo sem opcoes invalidas. | Normal | Regra informada. |
| WIZ-028 | Exclusao de formulario exige permissao, escopo e deve acionar evento antes da remocao. | Excluir formulario. | Formulario removido ou bloqueado. | Bloqueante | Regra informada. |
| WIZ-029 | Exclusao de formulario remove campos, respostas e conversao relacionados. | Excluir formulario. | Relacoes removidas em cascata. | Bloqueante | Material informa cascata. |
| WIZ-030 | Tela de edicao deve carregar campos do formulario. | Abrir edicao. | Campos disponiveis para alteracao. | Normal | Regra informada. |
| WIZ-031 | Opcoes devem ser normalizadas para array na edicao e no publico. | Renderizacao. | Opcoes invalidas viram lista vazia. | Normal | Regra informada. |
| WIZ-032 | Visualizar respostas exige permissao especifica. | Tela de respostas. | Consulta bloqueada sem permissao. | Bloqueante | Regra informada. |
| WIZ-033 | Respostas respeitam escopo any/own. | Consulta de respostas. | Dados fora do escopo nao aparecem. | Bloqueante | Regra informada. |
| WIZ-034 | Busca em respostas pode consultar dados serializados. | Consulta de respostas. | Resultado filtrado. | Normal | Material informa busca em JSON. |
| WIZ-035 | Respostas devem ser ordenadas por recencia e paginadas. | Consulta de respostas. | Lista paginada. | Normal | Regras informadas. |
| WIZ-036 | Exclusao de resposta exige permissao e pertencimento ao formulario. | Excluir resposta. | Exclusao bloqueada fora do formulario. | Bloqueante | Regra informada. |
| WIZ-037 | Edicao em massa de campos substitui estrutura anterior quando usada. | Atualizacao de campos. | Campos anteriores removidos e lista nova criada. | Bloqueante | Regra informada. |
| WIZ-038 | Exclusao individual de campo exige permissao especifica e resposta JSON padronizada. | Excluir campo. | Sucesso, 403 ou 404. | Normal | Regra informada. |
| WIZ-039 | Configuracao de conversao exige permissao de gerenciamento. | Criar/editar conversao. | Operacao bloqueada sem permissao. | Bloqueante | Regra informada. |
| WIZ-040 | Conversao respeita escopo any/own do formulario. | Operacoes de conversao. | Fora do escopo e bloqueado. | Bloqueante | Regra informada. |
| WIZ-041 | Conversao e upsert logico por formulario. | Salvar conversao. | Atualiza existente ou cria nova. | Normal | Material informa uma conversao por formulario. |
| WIZ-042 | Conversao deve persistir modulo e submodulo de destino. | Salvar conversao. | Destino registrado. | Bloqueante | Campos informados. |
| WIZ-043 | Conversao pode ser habilitada ou desabilitada. | Salvar conversao. | `is_active` controlado. | Normal | Regra informada. |
| WIZ-044 | Mapeamentos de conversao devem ser persistidos em JSON/array. | Salvar conversao. | Mapeamento salvo. | Bloqueante | Regra informada. |
| WIZ-045 | Conversao deve registrar owner e ator. | Salvar conversao. | Rastreabilidade preservada. | Bloqueante | Regra informada. |
| WIZ-046 | Dados de conversao devem expor catalogo de modulos e campos simplificados. | Configurar conversao. | Retorna modulo, usuarios e campos id/label/type. | Normal | Regra informada. |
| WIZ-047 | Canal publico dispensa autenticacao, mas exige contexto publico seguro. | Acesso publico. | Formulario renderizado sem backoffice. | Bloqueante | Regra informada. |
| WIZ-048 | Formulario publico so abre quando codigo existe e formulario esta ativo. | Acesso publico. | Ausente/inativo retorna erro. | Bloqueante | Regras informadas. |
| WIZ-049 | Submissao publica carrega formulario, campos e conversao. | Envio publico. | Dados disponiveis para validar e converter. | Bloqueante | Regra informada. |
| WIZ-050 | Validacao dinamica deve ser montada por campo. | Submissao. | Regras aplicadas. | Bloqueante | Regra informada. |
| WIZ-051 | Campo obrigatorio recebe required e mensagem com label. | Submissao. | Erro claro por campo. | Bloqueante | Regra informada. |
| WIZ-052 | Campo opcional recebe nullable. | Submissao. | Ausencia aceita. | Normal | Regra informada. |
| WIZ-053 | Email exige formato de email. | Submissao. | Valor invalido rejeitado. | Bloqueante | Regra informada. |
| WIZ-054 | Number exige valor numerico. | Submissao. | Valor invalido rejeitado. | Bloqueante | Regra informada. |
| WIZ-055 | Tel deve ser validado como texto. | Submissao. | Valor tratado como string. | Normal | Regra informada. |
| WIZ-056 | URL exige formato valido. | Submissao. | Valor invalido rejeitado. | Bloqueante | Regra informada. |
| WIZ-057 | Date exige data valida. | Submissao. | Valor invalido rejeitado. | Bloqueante | Regra informada. |
| WIZ-058 | Time exige formato HH:mm. | Submissao. | Valor invalido rejeitado. | Bloqueante | Regra informada. |
| WIZ-059 | Checkbox exige booleano e so conta como preenchido quando verdadeiro. | Submissao. | Valor invalido ou falso tratado conforme regra. | Normal | Regras informadas. |
| WIZ-060 | Select e radio restringem valores as opcoes configuradas. | Submissao. | Valor fora do dominio rejeitado. | Bloqueante | Regra informada. |
| WIZ-061 | Text, textarea e password usam validacao textual. | Submissao. | Valor textual validado. | Normal | Regra informada. |
| WIZ-062 | Submissao em branco deve ser bloqueada. | Submissao. | Resposta nao e salva. | Bloqueante | Regra informada. |
| WIZ-063 | Resposta deve salvar mapeamento por identificador de campo. | Submissao valida. | JSON por campo salvo. | Bloqueante | Regra informada. |
| WIZ-064 | Submissao deve disparar evento de resposta/conversao. | Submissao valida. | Evento publicado. | Normal | Regra informada. |
| WIZ-065 | Conversao pos-submissao deve tolerar falha sem bloquear sucesso publico. | Conversao. | Resposta permanece aceita. | Bloqueante | Regra informada. |
| WIZ-066 | Submissao bem-sucedida deve retornar feedback de sucesso. | Envio valido. | Usuario recebe confirmacao. | Normal | Regra informada. |
| WIZ-067 | Tela publica monta estado inicial por tipo. | Renderizacao publica. | Checkbox inicia falso; demais iniciam vazio. | Normal | Regra informada. |
| WIZ-068 | Layout publico deve usar `single`, `two-column` ou `card`. | Renderizacao publica. | Layout aplicado. | Normal | Material informa dominios. |
| WIZ-069 | Campos devem ser renderizados pela ordem configurada. | Renderizacao publica. | Ordem ascendente aplicada. | Normal | Regra informada. |
| WIZ-070 | Apos envio, formulario pode ser resetado e reposicionado no topo. | Sucesso publico. | Experiencia limpa para novo envio. | Normal | Regra informada. |
| WIZ-071 | Conversao so ocorre quando configuracao existe e esta ativa. | Submissao. | Sem conversao, apenas resposta e salva. | Normal | Regra informada. |
| WIZ-072 | Conversao exige modulo ativo para o tenant dono. | Submissao/conversao. | Conversao bloqueada se modulo inativo. | Bloqueante | Regra informada. |
| WIZ-073 | Conversao suporta apenas destinos previamente definidos e permitidos. | Conversao. | Destino nao suportado gera falha registrada. | Bloqueante | Regra informada. |
| WIZ-074 | Mapeamento aceita campo de origem ou valor fixo. | Conversao. | Valor final resolvido. | Normal | Regra informada. |
| WIZ-075 | Cada criacao no destino exige permissao especifica do criador. | Conversao. | Sem permissao, falha registrada. | Bloqueante | Regra informada. |
| WIZ-076 | Catalogo de destinos disponiveis deve filtrar modulo ativo e permissao de criacao. | Configurar conversao. | Lista mostra somente destinos validos. | Bloqueante | Regra informada. |
| WIZ-077 | Builder visual deve suportar abas construir, configuracoes, estilo e incorporacao. | Gestao visual. | Abas disponiveis. | Normal | Material informa tabs. |
| WIZ-078 | Palette de campos pode incluir campos customizados habilitados do dominio consumidor. | Builder. | Campos disponiveis para arrastar/selecionar. | Normal | Regra informada. |
| WIZ-079 | Tipos de campos customizados devem ser mapeados para tipos renderizaveis. | Builder. | Tipo final valido. | Normal | Material informa mapeamentos. |
| WIZ-080 | Dropdown deve usar opcoes vindas de payload JSON valido. | Campo select. | Opcoes renderizadas. | Normal | Regra informada. |
| WIZ-081 | Labels devem ser saneados para evitar quebra de renderizacao. | Builder/render. | Texto seguro. | Normal | Material informa limpeza de caracteres. |
| WIZ-082 | Configuracoes do formulario devem validar titulo, mensagem de agradecimento, texto do botao e status inicial quando aplicavel. | Aba settings. | Configuracao invalida e rejeitada. | Bloqueante | Regra informada. |
| WIZ-083 | O formulario pode gravar notificar admin, captcha e origem quando o dominio consumidor exigir. | Aba settings. | Metadados persistidos. | Normal | Regra informada. |
| WIZ-084 | Estilo customizado deve ser sanitizado antes de salvar. | Aba style. | CSS inseguro rejeitado ou limpo. | Bloqueante | Material informa limpeza; regra saneada.[^nota2] |
| WIZ-085 | URL direta e embed devem ser gerados a partir do codigo publico. | Aba embed. | URL/codigo de incorporacao disponiveis. | Normal | Regra informada. |
| WIZ-086 | Mensagem de agradecimento com HTML deve ser sanitizada. | Sucesso publico. | Evita script ou markup perigoso. | Bloqueante | Saneamento necessario.[^nota2] |
| WIZ-087 | Anexos devem ser tratados via GED quando o campo de arquivo for usado. | Campo file. | Arquivo registrado com referencia documental. | Bloqueante | Material cita anexos; GED e destino correto. |
| WIZ-088 | Wizard multi-etapa deve salvar rascunho por usuario e tenant. | Processo guiado. | Retomada possivel. | Bloqueante | Requisito informado no material. |
| WIZ-089 | Wizard multi-etapa deve validar passo e validacoes entre passos. | Avancar/concluir. | Avanco bloqueado quando dependencia falha. | Bloqueante | Requisito informado. |
| WIZ-090 | Aplicacao final de wizard deve ser idempotente. | Conclusao/reenvio. | Nao duplica efeito. | Bloqueante | Informado no README. |

## 9. Parametros de configuracao

| Parametro | Finalidade | Tipo/formato | Valor padrao | Obrigatorio | Nivel | Quem pode alterar | Impacto |
|---|---|---|---|---|---|---|---|
| WizardHabilitado | Habilitar submodulo para tenant. | Booleano | Nao informado no material | Sim | Tenant | Administrador | Bloqueia uso quando inativo. |
| LayoutPadrao | Definir layout inicial de formulario. | Enum | `single` | Sim | Tenant/formulario | Gestor | Afeta renderizacao. |
| PublicacaoPorCodigo | Permitir canal publico por codigo. | Booleano | Nao informado no material | Condicional | Formulario | Gestor | Exposicao publica. |
| CaptchaObrigatorio | Exigir captcha em formulario publico. | Booleano | Nao informado no material | Condicional | Formulario | Gestor | Protege envio publico. |
| ConversaoHabilitada | Ativar conversao por formulario. | Booleano | false | Sim | Formulario | Gestor | Aciona modulo destino. |
| DestinoModulo | Modulo alvo da conversao. | Texto/enum | Nao informado no material | Condicional | Conversao | Gestor | Define destino. |
| DestinoSubmodulo | Submodulo alvo da conversao. | Texto/enum | Nao informado no material | Nao | Conversao | Gestor | Refina destino. |
| FieldMappings | Mapeamento campo destino para campo origem ou valor fixo. | JSON | Nao informado no material | Condicional | Conversao | Gestor | Define transformacao. |
| CssCustomizado | Estilo do formulario publico. | Texto/CSS seguro | Nao informado no material | Nao | Formulario | Gestor | Afeta exibicao. |
| MensagemAgradecimento | Feedback apos envio. | Texto/HTML sanitizado | Nao informado no material | Nao | Formulario | Gestor | Afeta experiencia publica. |
| TextoBotaoEnvio | Texto do botao de envio. | Texto | Nao informado no material | Nao | Formulario | Gestor | Afeta experiencia publica. |
| RetencaoRespostas | Prazo de retencao das respostas. | Periodo | Nao informado no material | Sim | Tenant | Siser/Compliance | Afeta privacidade. |

## 10. Modelo de dados funcional e implantavel

### 10.1 Visao geral do modelo

O material informa quatro entidades nucleares para formularios dinamicos: `forms`, `form_fields`, `form_responses` e `form_conversions`, alem de entidade generica com Id, TenantId, Codigo, Status, ResponsavelId, historico com Acao, UsuarioId, PayloadJson e anexo com ArquivoId. Esta EF preserva essas entidades em nomenclatura funcional do Epros e acrescenta estruturas para passos, rascunhos, publicacao, estilo, embed e auditoria, porque o escopo de wizard multi-etapa exige persistencia que nao esta detalhada fisicamente no material.[^nota1]

| Grupo de dados | Entidades/tabelas | Papel funcional | Observacoes |
|---|---|---|---|
| Definicao | `wizard_formulario`, `wizard_campo`, `wizard_passo` | Define formulario/wizard, campos e passos. | Campos de `forms` e `form_fields` preservados. |
| Execucao | `wizard_rascunho`, `wizard_resposta`, `wizard_resposta_item` | Salva rascunhos e respostas. | `response_data` preservado e detalhado. |
| Conversao | `wizard_conversao`, `wizard_conversao_execucao` | Configura e registra conversoes. | `form_conversions` preservado. |
| Publicacao | `wizard_publicacao`, `wizard_estilo_embed` | Controla codigo publico, URL, embed e CSS. | Derivado das abas style/embed. |
| Controle | `wizard_historico`, `wizard_anexo` | Auditoria e anexos GED. | Campos genericos informados. |

### 10.2 Entidades, finalidade e cardinalidade

| Entidade | Finalidade | Cardinalidade | Observacao |
|---|---|---|---|
| `wizard_formulario` | Cadastro mestre de formulario/wizard. | 1 tenant possui N formularios. | Preserva name, code, is_active, default_layout, creator_id e created_by. |
| `wizard_campo` | Campo dinamico do formulario. | 1 formulario possui N campos. | Preserva label, type, required, placeholder, options e order. |
| `wizard_passo` | Passo de wizard multi-etapa. | 1 formulario pode possuir N passos. | Criado para escopo de wizard. |
| `wizard_rascunho` | Estado parcial por usuario/tenant. | 1 usuario pode ter N rascunhos. | Criado para rascunho. |
| `wizard_resposta` | Cabecalho de resposta enviada. | 1 formulario possui N respostas. | Preserva form_id e response_data. |
| `wizard_resposta_item` | Valor por campo. | 1 resposta possui N itens. | Detalha JSON por campo para consulta. |
| `wizard_conversao` | Configuracao de destino e mapeamento. | 1 formulario possui 0 ou 1 conversao ativa por destino/regra. | Material informa restricao unica por formulario; MC pede validar multiplas conversoes. |
| `wizard_conversao_execucao` | Resultado de tentativa de conversao. | 1 resposta pode ter N tentativas. | Necessario para falhas toleradas. |
| `wizard_publicacao` | Controle de canal publico. | 1 formulario possui 0 ou 1 publicacao ativa. | Usa codigo publico. |
| `wizard_estilo_embed` | Estilo e incorporacao. | 1 formulario possui 0 ou 1 configuracao ativa. | Inclui CSS, URL direta e embed. |
| `wizard_historico` | Auditoria de alteracoes. | N historicos por entidade. | Campos informados. |
| `wizard_anexo` | Referencia de anexo GED. | N anexos por formulario/resposta. | Campo ArquivoId informado. |

## 11. Dicionario de dados implantavel

### 11.1 `wizard_formulario`

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| id | Bigint/UUID | Nao informado no material | Sim | PK | Identificador do formulario/wizard. |
| tenant_id | Bigint/UUID | Nao informado no material | Sim | FK tenant | Equivalente funcional do owner por tenant. |
| name | Texto | Nao informado no material | Sim |  | Nome obrigatorio informado. |
| code | Texto | UUID + timestamp ou codigo unico | Sim | Unico | Codigo publico unico. |
| tipo | Enum | Formulario/Wizard | Sim |  | Criado para distinguir formularios simples de wizards.[^nota1] |
| is_active | Booleano | true/false | Sim |  | Default true informado. |
| status | Enum | Rascunho/EmAnalise/Ativo/Inativo/Encerrado | Sim |  | Estados informados para ciclo principal. |
| default_layout | Enum | single/two-column/card | Sim |  | Default single informado. |
| responsavel_id | Bigint/UUID | Nao informado no material | Sim | FK usuario/pessoa | Campo generico informado. |
| creator_id | Bigint/UUID | Nao informado no material | Nao | FK usuario | Ator da criacao; nulo permitido no material. |
| created_by | Bigint/UUID | Nao informado no material | Nao | FK usuario/owner | Owner; material informa nulo com cascade. |
| title | Texto | Nao informado no material | Condicional |  | Usado em configuracoes publicas. |
| thankyou_message | Texto/HTML sanitizado | Nao informado no material | Nao |  | Mensagem de sucesso. |
| submit_text | Texto | Nao informado no material | Nao |  | Texto do botao. |
| notify_admin | Booleano | true/false | Nao |  | Metadado informado. |
| recaptcha_enabled | Booleano | true/false | Nao |  | Metadado informado. |
| source | Texto | Nao informado no material | Nao |  | Origem/canal quando aplicavel. |
| created_at | Data/hora | ISO 8601 | Sim |  | Timestamp informado. |
| updated_at | Data/hora | ISO 8601 | Sim |  | Timestamp informado. |

### 11.2 `wizard_campo`

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| id | Bigint/UUID | Nao informado no material | Sim | PK | Identificador do campo. |
| formulario_id | Bigint/UUID | Nao informado no material | Sim | FK wizard_formulario | Campo pertence ao formulario. |
| passo_id | Bigint/UUID | Nao informado no material | Nao | FK wizard_passo | Usado quando for wizard. |
| label | Texto | Nao informado no material | Sim |  | Label obrigatorio informado. |
| name | Texto | Nao informado no material | Condicional |  | Nome tecnico para mapeamento. |
| type | Enum | text, email, number, tel, url, password, textarea, select, radio, checkbox, date, time, file, header, paragraph | Sim |  | Tipos informados/consolidados. |
| required | Booleano | true/false | Sim |  | Default false informado. |
| placeholder | Texto | Nao informado no material | Nao |  | Placeholder informado. |
| options | JSON | Lista de opcoes | Nao |  | Obrigatorio para select/radio quando houver dominio. |
| order | Inteiro | >= 0 | Sim |  | Default 0 informado. |
| default_value | Texto/JSON | Nao informado no material | Nao |  | Usado em wizard/valor fixo. |
| validation_schema | JSON | Nao informado no material | Nao |  | Criado para validacoes adicionais.[^nota1] |
| creator_id | Bigint/UUID | Nao informado no material | Nao | FK usuario | Ator informado. |
| created_by | Bigint/UUID | Nao informado no material | Nao | FK usuario/owner | Owner informado. |
| created_at | Data/hora | ISO 8601 | Sim |  | Timestamp informado. |
| updated_at | Data/hora | ISO 8601 | Sim |  | Timestamp informado. |

### 11.3 `wizard_passo`

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| id | Bigint/UUID | Nao informado no material | Sim | PK | Identificador do passo. |
| formulario_id | Bigint/UUID | Nao informado no material | Sim | FK wizard_formulario | Wizard dono. |
| codigo | Texto | Nao informado no material | Sim | Unico por formulario | Codigo do passo. |
| titulo | Texto | Nao informado no material | Sim |  | Titulo exibido. |
| descricao | Texto | Nao informado no material | Nao |  | Texto de apoio. |
| ordem | Inteiro | >= 0 | Sim |  | Sequencia do passo. |
| obrigatorio | Booleano | true/false | Sim |  | Define se passo pode ser pulado. |
| dependencias | JSON | Nao informado no material | Nao |  | Regras de exibicao/avance. |
| validation_schema | JSON | Nao informado no material | Nao |  | Validacoes cross-step. |
| status | Enum | Ativo/Inativo | Sim |  | Passo inativo nao renderiza. |

### 11.4 `wizard_rascunho`

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| id | Bigint/UUID | Nao informado no material | Sim | PK | Identificador do rascunho. |
| formulario_id | Bigint/UUID | Nao informado no material | Sim | FK wizard_formulario | Wizard/formulario. |
| tenant_id | Bigint/UUID | Nao informado no material | Sim | FK tenant | Segregacao. |
| usuario_id | Bigint/UUID | Nao informado no material | Condicional | FK usuario | Obrigatorio em fluxo autenticado. |
| codigo_publico | Texto | Nao informado no material | Condicional | FK publicacao | Usado em fluxo publico. |
| passo_atual_id | Bigint/UUID | Nao informado no material | Nao | FK wizard_passo | Passo atual. |
| payload_json | JSON | Nao informado no material | Sim |  | Dados parciais. |
| status | Enum | Aberto/Concluido/Expirado/Cancelado | Sim |  | Estados criados para rascunho.[^nota1] |
| versao_formulario | Inteiro | Nao informado no material | Nao |  | Detecta conflito de versao. |
| criado_em | Data/hora | ISO 8601 | Sim |  | Criacao. |
| atualizado_em | Data/hora | ISO 8601 | Sim |  | Ultima atualizacao. |

### 11.5 `wizard_resposta`

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| id | Bigint/UUID | Nao informado no material | Sim | PK | Identificador da resposta. |
| form_id | Bigint/UUID | Nao informado no material | Sim | FK wizard_formulario | Campo informado como `form_id`. |
| tenant_id | Bigint/UUID | Nao informado no material | Sim | FK tenant | Segregacao. |
| response_data | JSON | Dados indexados por field_id | Sim |  | Campo informado. |
| creator_id | Bigint/UUID | Nao informado no material | Nao | FK usuario | Campo informado. |
| created_by | Bigint/UUID | Nao informado no material | Nao | FK usuario/owner | Campo informado. |
| remote_ip | IP | IPv4/IPv6 | Nao |  | IP citado em material relacionado. |
| origem | Enum/texto | Publico/Autenticado/Embed | Sim |  | Origem da resposta. |
| status | Enum | Recebida/Convertida/ConversaoFalhou/Cancelada | Sim |  | Criado para operacao.[^nota1] |
| created_at | Data/hora | ISO 8601 | Sim |  | Timestamp informado. |
| updated_at | Data/hora | ISO 8601 | Sim |  | Timestamp informado. |

### 11.6 `wizard_resposta_item`

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| id | Bigint/UUID | Nao informado no material | Sim | PK | Identificador do item. |
| resposta_id | Bigint/UUID | Nao informado no material | Sim | FK wizard_resposta | Resposta dona. |
| campo_id | Bigint/UUID | Nao informado no material | Sim | FK wizard_campo | Campo respondido. |
| valor_texto | Texto | Nao informado no material | Nao |  | Valor textual. |
| valor_numero | Decimal | Nao informado no material | Nao |  | Valor numerico. |
| valor_data | Data | ISO 8601 | Nao |  | Valor de data. |
| valor_booleano | Booleano | true/false | Nao |  | Valor checkbox. |
| valor_json | JSON | Nao informado no material | Nao |  | Listas, anexos ou valores complexos. |
| valor_mascarado | Texto | Nao informado no material | Nao |  | Exibicao segura. |

### 11.7 `wizard_conversao`

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| id | Bigint/UUID | Nao informado no material | Sim | PK | Identificador da conversao. |
| form_id | Bigint/UUID | Nao informado no material | Sim | FK wizard_formulario | Campo informado. |
| module_name | Texto | Nao informado no material | Sim | Catalogo modulo | Campo informado. |
| submodule_name | Texto | Nao informado no material | Nao | Catalogo submodulo | Campo informado. |
| is_active | Booleano | true/false | Sim |  | Default false informado. |
| field_mappings | JSON | Campo destino -> campo origem/valor fixo | Sim |  | Campo informado. |
| creator_id | Bigint/UUID | Nao informado no material | Nao | FK usuario | Campo informado. |
| created_by | Bigint/UUID | Nao informado no material | Nao | FK usuario/owner | Campo informado. |
| created_at | Data/hora | ISO 8601 | Sim |  | Timestamp informado. |
| updated_at | Data/hora | ISO 8601 | Sim |  | Timestamp informado. |

### 11.8 `wizard_conversao_execucao`

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| id | Bigint/UUID | Nao informado no material | Sim | PK | Identificador da execucao. |
| conversao_id | Bigint/UUID | Nao informado no material | Sim | FK wizard_conversao | Conversao usada. |
| resposta_id | Bigint/UUID | Nao informado no material | Sim | FK wizard_resposta | Resposta convertida. |
| destino_modulo | Texto | Nao informado no material | Sim |  | Modulo de destino. |
| destino_submodulo | Texto | Nao informado no material | Nao |  | Submodulo de destino. |
| destino_registro_id | Bigint/UUID/texto | Nao informado no material | Nao |  | Registro criado. |
| payload_enviado | JSON | Nao informado no material | Sim |  | Dados mapeados. |
| status | Enum | Pendente/Sucesso/Falha/Ignorado | Sim |  | Criado para observabilidade. |
| erro | Texto | Nao informado no material | Nao |  | Falha tolerada. |
| criado_em | Data/hora | ISO 8601 | Sim |  | Data da execucao. |

### 11.9 `wizard_publicacao`

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| id | Bigint/UUID | Nao informado no material | Sim | PK | Identificador da publicacao. |
| formulario_id | Bigint/UUID | Nao informado no material | Sim | FK wizard_formulario | Formulario publicado. |
| code | Texto | UUID + timestamp ou codigo unico | Sim | Unico | Codigo publico. |
| direct_url | URL | Nao informado no material | Sim |  | URL direta informada. |
| embed_code | Texto | iframe seguro | Nao |  | Embed informado. |
| largura_embed | Inteiro | 650 informado como referencia | Nao |  | Dimensao informada em material. |
| altura_embed | Inteiro | 900 informado como referencia | Nao |  | Dimensao informada em material. |
| is_active | Booleano | true/false | Sim |  | Publicacao ativa/inativa. |
| criado_em | Data/hora | ISO 8601 | Sim |  | Criacao. |

### 11.10 `wizard_estilo_embed`

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| id | Bigint/UUID | Nao informado no material | Sim | PK | Identificador. |
| formulario_id | Bigint/UUID | Nao informado no material | Sim | FK wizard_formulario | Formulario. |
| css_customizado | Texto/CSS seguro | Nao informado no material | Nao |  | CSS deve ser sanitizado. |
| payload_builder | JSON | Nao informado no material | Nao |  | Payload de builder visual. |
| ativo | Booleano | true/false | Sim |  | Define configuracao ativa. |
| atualizado_por | Bigint/UUID | Nao informado no material | Sim | FK usuario | Usuario que atualizou. |
| atualizado_em | Data/hora | ISO 8601 | Sim |  | Ultima atualizacao. |

### 11.11 `wizard_historico`

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| id | Bigint/UUID | Nao informado no material | Sim | PK | Identificador. |
| formulario_id | Bigint/UUID | Nao informado no material | Condicional | FK wizard_formulario | Formulario relacionado. |
| entidade | Texto | Nao informado no material | Sim |  | Entidade afetada. |
| entidade_id | Bigint/UUID | Nao informado no material | Sim |  | Registro afetado. |
| acao | Texto | Nao informado no material | Sim |  | Campo informado. |
| usuario_id | Bigint/UUID | Nao informado no material | Sim | FK usuario | Campo informado. |
| payload_json | JSON | Nao informado no material | Sim |  | Campo informado; deve mascarar sensiveis. |
| ip | IP | IPv4/IPv6 | Nao |  | Auditoria informada. |
| criado_em | Data/hora | ISO 8601 | Sim |  | Data da acao. |

### 11.12 `wizard_anexo`

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| id | Bigint/UUID | Nao informado no material | Sim | PK | Identificador do anexo. |
| formulario_id | Bigint/UUID | Nao informado no material | Condicional | FK wizard_formulario | Formulario relacionado. |
| resposta_id | Bigint/UUID | Nao informado no material | Condicional | FK wizard_resposta | Resposta relacionada. |
| arquivo_id | Bigint/UUID | Nao informado no material | Sim | FK GED | Campo ArquivoId informado. |
| tipo | Enum | Estrutura/Resposta/Comprovante | Nao |  | Classificacao criada.[^nota1] |
| criado_por | Bigint/UUID | Nao informado no material | Sim | FK usuario | Usuario/processo. |
| criado_em | Data/hora | ISO 8601 | Sim |  | Data de inclusao. |

## 12. Fluxos e estados

### 12.1 Ciclo de vida principal

| Estado atual | Evento | Proximo estado | Permissao | Regra |
|---|---|---|---|---|
| Rascunho | Submeter | EmAnalise | Operador | Validar obrigatorios e estrutura minima. |
| EmAnalise | Aprovar | Ativo | Aprovador | Publicacao/uso permitido. |
| EmAnalise | Rejeitar | Rascunho | Aprovador | Exigir motivo. |
| Ativo | Inativar | Inativo | Gestor | Bloqueia uso publico e conversoes futuras. |
| Ativo | Encerrar | Encerrado | Gestor | Finaliza ciclo. |
| Inativo | Reativar | Ativo | Gestor | Reabilita uso quando valido. |

### 12.2 Fluxos operacionais

| Fluxo | Passos principais | Resultado esperado |
|---|---|---|
| Gestao de formulario | Listar, filtrar, criar, editar, ordenar, paginar, excluir. | Formulario governado por permissao e tenant. |
| Builder de campos | Adicionar, editar, remover, ordenar, salvar lote. | Estrutura dinamica salva. |
| Captura publica | Abrir por codigo, renderizar ativo, validar, salvar resposta. | Resposta aceita ou erro exibido. |
| Conversao | Configurar destino, mapear campos, ativar, converter apos resposta. | Criacao minima ou falha registrada. |
| Wizard multi-etapa | Iniciar, salvar rascunho, validar passo, concluir, aplicar. | Processo guiado concluido sem duplicidade. |
| Estilo/embed | Configurar CSS, gerar URL direta, copiar embed. | Publicacao visual controlada. |

## 13. APIs e contratos funcionais

| Contrato | Metodo funcional | Entrada | Saida | Observacoes |
|---|---|---|---|---|
| Listar formularios | Consulta administrativa | Filtros nome, ativo, ordenacao, pagina, tamanho | Lista com contadores | Endpoints finais nao informados no material. |
| Criar formulario | Escrita administrativa | Nome, layout, status, campos opcionais | Formulario com codigo publico | Exige permissao. |
| Atualizar formulario | Escrita administrativa | Metadados e campos | Formulario atualizado | Reaplica escopo. |
| Excluir formulario | Escrita administrativa | Identificador | Sucesso/erro | Remove relacoes. |
| Consultar respostas | Consulta administrativa | Formulario, busca, pagina, tamanho | Respostas paginadas | Exige permissao. |
| Excluir resposta | Escrita administrativa | Formulario e resposta | Sucesso/erro | Valida pertencimento. |
| Obter publico | Consulta publica | Codigo publico | Formulario ativo renderizavel | Sem backoffice. |
| Submeter publico | Escrita publica | Codigo e valores | Sucesso ou erros | Salva resposta. |
| Dados de conversao | Consulta administrativa | Formulario | Modulos, campos, usuarios e listas auxiliares | Filtra modulo ativo/permissao. |
| Salvar conversao | Escrita administrativa | Destino, status e mapeamentos | Conversao salva | Upsert logico. |
| Salvar estilo | Escrita administrativa | CSS seguro | Estilo salvo | Sanitizar. |
| Obter embed | Consulta administrativa | Formulario | URL direta e embed | Baseado no codigo publico. |

## 14. Telas, consultas e relatorios

| Interface | Objetivo | Campos/acoes minimas | Observacoes |
|---|---|---|---|
| Lista de formularios/wizards | Consultar estruturas existentes. | Nome, status, layout, campos, respostas, owner, ator, filtros, ordenacao, paginacao, novo, editar, excluir, respostas, conversao, copiar link. | Acoes condicionadas por permissao. |
| Criar formulario | Definir metadados iniciais. | Nome, layout, ativo, campos iniciais. | Codigo publico gerado. |
| Editar formulario | Manter metadados e campos. | Nome, status, layout, lista de campos, opcoes, ordem, remover campo. | Carrega campos existentes. |
| Builder visual | Montar campos por interface assistida. | Palette, arrastar/ordenar, build, settings, style, embed. | Material informa abas. |
| Respostas | Consultar respostas enviadas. | Busca, paginacao, colunas principais, detalhes, excluir, exportar. | Material informa limitar colunas principais aos primeiros campos. |
| Conversao | Configurar destino e mapeamentos. | Modulo, submodulo, ativo, campos do formulario, campos destino, valores fixos. | Deve bloquear mapeamento incompleto quando ativo. |
| Formulario publico | Capturar resposta. | Campos, mensagens, validacao, botao, agradecimento, reset. | Exibe apenas formulario ativo. |
| Estilo | Configurar CSS permitido. | Editor de CSS, salvar, preview. | Deve sanitizar. |
| Embed | Gerar incorporacao. | URL direta, iframe, dimensoes. | Material informa 650 x 900 como referencia. |
| Painel gestor | Monitorar uso e fila. | KPIs, aprovacao, respostas, conversoes, falhas. | Telas finais detalhadas na MC. |

| Relatorio | Descricao | Filtros | Observacoes |
|---|---|---|---|
| Posicao geral | Snapshot por status de formulario/wizard. | Tenant, status, periodo, responsavel. | Material informa REL-WIZ-001. |
| Auditoria de alteracoes | Trilha por periodo e usuario. | Formulario, usuario, acao, periodo. | Material informa REL-WIZ-002. |
| Respostas por formulario | Volume e detalhe de respostas. | Formulario, periodo, origem, status. | Derivado da tela de respostas. |
| Conversoes | Sucesso/falha por destino. | Formulario, destino, status, periodo. | Necessario porque falhas sao toleradas. |
| Abandono de passo | Monitorar abandono em wizard multi-etapa. | Wizard, passo, periodo. | Material cita telemetria de abandono. |

## 15. Seguranca, privacidade e auditoria

| Tema | Regra funcional |
|---|---|
| Tenant | Todos os registros administrativos devem respeitar tenant, owner e ator. |
| Canal publico | Deve expor apenas formularios ativos por codigo publico, sem acesso a listagem ou respostas. |
| Permissao | Gestao, campos, respostas, conversao e exclusoes exigem permissao especifica. |
| Dados pessoais | Respostas podem conter dados pessoais; retencao e mascaramento devem seguir Compliance. |
| HTML/CSS | Mensagens e estilos devem ser sanitizados antes de exibir publicamente. |
| Anexos | Campo de arquivo deve usar GED e politica de arquivos. |
| Auditoria | Mudancas, exclusoes, publicacao, resposta, conversao e falhas devem gerar historico. |
| Conversao | Falha tolerada deve ser registrada para suporte, nao escondida. |
| Publicacao | Codigo publico deve ser unico, nao sequencial e revogavel. |

## 16. Testes funcionais minimos

| Cenario | Dado/condicao | Resultado esperado |
|---|---|---|
| Listar sem permissao | Usuario sem gestao acessa lista. | Acesso negado. |
| Listar com escopo own | Usuario possui escopo proprio. | Apenas seus formularios aparecem. |
| Listar sem escopo | Usuario autenticado sem escopo valido. | Lista vazia. |
| Criar formulario valido | Nome informado, permissao ativa. | Formulario criado com codigo publico e layout single. |
| Criar sem nome | Nome ausente. | Erro de validacao. |
| Criar campo select sem opcoes validas | Campo de selecao sem dominio. | Erro ou campo salvo sem opcoes conforme regra validada. |
| Editar fora do escopo | Usuario tenta alterar formulario de outro owner. | Operacao bloqueada. |
| Excluir formulario | Usuario autorizado exclui formulario. | Campos, respostas e conversao relacionadas removidos. |
| Abrir publico inativo | Codigo existe, formulario inativo. | Erro/nao encontrado. |
| Submeter payload vazio | Nenhum campo preenchido. | Resposta bloqueada. |
| Validar email invalido | Campo email recebe texto invalido. | Erro por campo. |
| Validar time invalido | Campo time fora de HH:mm. | Erro por campo. |
| Validar select fora das opcoes | Valor nao esta no dominio. | Erro por campo. |
| Checkbox falso obrigatorio | Checkbox obrigatorio nao marcado. | Erro por campo. |
| Resposta valida | Todos obrigatorios validos. | Resposta salva com dados por campo. |
| Conversao inativa | Resposta salva, conversao off. | Nenhuma conversao executada. |
| Conversao ativa sem modulo ativo | Modulo destino inativo. | Resposta salva e falha de conversao registrada. |
| Conversao com mapeamento incompleto | Campo obrigatorio destino sem mapeamento. | Configuracao bloqueada ou conversao falha registrada. |
| CSS inseguro | Estilo contem conteudo nao permitido. | Estilo rejeitado ou sanitizado. |
| Embed | Formulario ativo. | URL direta e iframe seguro gerados. |
| Wizard rascunho | Usuario salva etapa parcial. | Rascunho retomavel. |
| Wizard reenvio final | Usuario repete conclusao. | Aplicacao idempotente sem duplicidade. |

## 17. Criterios de aceite

| Codigo | Criterio |
|---|---|
| CA-WIZ-001 | Deve ser possivel criar formulario/wizard com nome, codigo unico, status, layout, owner e ator. |
| CA-WIZ-002 | Deve ser possivel criar, editar, ordenar e excluir campos com tipos e opcoes permitidas. |
| CA-WIZ-003 | Listagem deve aplicar permissao, escopo any/own, filtros, ordenacao, paginacao e contadores. |
| CA-WIZ-004 | Canal publico deve abrir somente formulario ativo por codigo unico. |
| CA-WIZ-005 | Submissao deve validar campos dinamicamente por tipo e obrigatoriedade. |
| CA-WIZ-006 | Submissao vazia deve ser bloqueada. |
| CA-WIZ-007 | Resposta valida deve ser salva com mapeamento por campo. |
| CA-WIZ-008 | Respostas devem ser consultaveis por usuario autorizado com busca e paginacao. |
| CA-WIZ-009 | Conversao deve exigir destino, modulo ativo, permissao e mapeamento. |
| CA-WIZ-010 | Falha de conversao nao deve desfazer submissao publica aceita. |
| CA-WIZ-011 | Estilo e mensagem publica devem ser sanitizados. |
| CA-WIZ-012 | Wizard multi-etapa deve suportar rascunho, validacao por passo e aplicacao idempotente. |
| CA-WIZ-013 | Historico deve registrar usuario, acao, payload, IP e timestamps. |
| CA-WIZ-014 | Copia/URL de embed deve ser gerada sem expor dados administrativos. |

## 18. Notas de autoria e saneamento funcional

[^nota1]: As entidades e regras de `wizard_passo`, `wizard_rascunho`, `wizard_resposta_item`, `wizard_conversao_execucao`, status operacionais e validacoes cross-step foram criadas nesta EF para tornar o Epros implantavel como motor de wizard multi-etapa. O material comprova o escopo de wizard, rascunho, passos e aplicacao idempotente, mas nao informa tabelas fisicas definitivas para essas partes.
[^nota2]: As regras de sanitizacao de CSS, HTML de agradecimento, codigo de incorporacao seguro e exibicao mascarada foram incluidas como saneamento funcional necessario. O material aponta configuracoes de estilo, mensagem e incorporacao, alem de riscos de conteudo inseguro, mas nao fornece politica final de sanitizacao.
