# EF_3_PLATAFORMA_COMPARTILHADA_PLANEJAMENTO_IN_MEMORY_V1

**Projeto:** Epros  
**Empresa:** Siser  
**Tipo de documento:** Especificacao Funcional definitiva  
**Versao:** V1  
**Modulo:** PLATAFORMA_COMPARTILHADA  
**Submodulo:** PLANEJAMENTO_IN_MEMORY  
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

O submodulo Planejamento In Memory do Epros deve executar simulacoes taticas de curto prazo em memoria, apoiando ATP, cenarios what-if, demanda, capacidade, lead time e alocacao tatica de estoque a pedidos. O resultado da simulacao nao deve gerar efeito operacional definitivo ate que usuario autorizado confirme o plano. A confirmacao deve exportar ou publicar o plano para o modulo dono da execucao.

| Pergunta | Resposta |
|---|---|
| Para que o submodulo existe? | Para simular cenarios taticos sem gravar operacoes definitivas ate a confirmacao. |
| Que problema de negocio resolve? | Permite avaliar disponibilidade, demanda, capacidade, lead time e alternativas de alocacao antes de comprometer estoque, vendas ou producao. |
| Qual resultado operacional deve produzir? | Cenario simulado, comparacao de cenarios, recomendacao/resultado calculado e plano confirmado quando aprovado. |
| Quais areas dependem dele? | Estoque, Vendas, Producao, Analytics, API Gateway e Workflow. |

## 3. Escopo funcional

### 3.1 Dentro do escopo

| Capacidade | Descricao | Observacao |
|---|---|---|
| Cenario what-if | Criar simulacao com premissas de demanda, capacidade e lead time. | Requisito informado. |
| ATP | Avaliar disponibilidade para promessa de atendimento. | Material informa ATP. |
| Alocacao tatica | Simular alocacao de estoque a pedidos no curto prazo. | Material informa alocacao tatica. |
| Simulacao em memoria | Processar resultado sem persistir efeito definitivo. | Material informa que resultado nao persiste ate confirmar. |
| Comparacao de cenarios | Comparar cenarios lado a lado. | Requisito informado. |
| Confirmacao de plano | Transformar resultado aprovado em plano enviado ao modulo destino. | Requisito informa exportacao ao confirmar. |
| Workflow de aprovacao | Controlar ciclo Rascunho, EmAnalise, Ativo, Inativo e Encerrado quando aplicavel. | Material informa maquina de estados. |
| Parametrizacao por tenant | Permitir regras por tenant sem novo deploy. | Requisito informado. |
| Historico e anexos | Registrar alteracoes e anexos via GED quando aplicavel. | Material informa historico e anexo. |
| Relatorios e KPIs | Exibir posicao, auditoria e indicadores de simulacao. | Material informa relatorios padronizados e KPIs. |

### 3.2 Fora do escopo

| Item fora do escopo | Motivo | Destino correto |
|---|---|---|
| Execucao real de ordem de producao | Este submodulo simula; a execucao pertence a Producao. | Producao |
| Movimentacao real de estoque | Este submodulo calcula disponibilidade e alocacao simulada. | Estoque |
| Geracao definitiva de pedido | Este submodulo avalia promessa e alternativas. | Vendas |
| Motor completo de MRP operacional persistido | O material delimita simulacao tatica e fronteira com planejamento operacional. | Producao/Estoque |
| Cadastro mestre de produto, cliente, recurso ou calendario | O submodulo referencia cadastros mestres. | Cadastros/Estoque/Producao |
| Modelo analitico preditivo | Indicadores podem consumir resultados, mas modelos pertencem a IA/ML ou Analytics. | IA/ML / Analytics |

## 4. Glossario e conceitos funcionais

| Termo | Definicao funcional | Observacoes |
|---|---|---|
| ATP | Avaliacao de disponibilidade para prometer atendimento. | Informado no material. |
| Cenario what-if | Simulacao com premissas alteraveis para avaliar impacto. | Informado no material. |
| In memory | Processamento temporario sem gerar efeito definitivo ate confirmacao. | Informado no material. |
| Demanda | Necessidade simulada de atendimento, venda ou consumo. | Requisito informado. |
| Capacidade | Limite simulado de atendimento, recurso ou producao. | Requisito informado. |
| Lead time | Tempo considerado para disponibilidade, reposicao ou atendimento. | Requisito informado. |
| Plano confirmado | Resultado aprovado e encaminhado ao modulo dono. | Requisito informado. |
| Fronteira operacional | Separacao entre simulacao e execucao real. | Essencial para nao duplicar regras dos modulos. |

## 5. Atores, papeis e responsabilidades

| Ator/Papel | Responsabilidade | Permissoes esperadas | Restricoes |
|---|---|---|---|
| Planejador | Criar e comparar cenarios. | Criar, editar, simular, comparar e exportar resultados. | Nao confirma plano sem permissao. |
| Gestor | Aprovar ou confirmar plano. | Aprovar, confirmar, encerrar e exportar. | Deve respeitar fronteira com modulos donos. |
| Operador | Consultar cenarios e resultados. | Visualizar, filtrar e exportar quando permitido. | Nao altera premissas criticas. |
| Epros | Calcular, auditar, comparar, publicar evento e encaminhar plano confirmado. | Automacao sistemica. | Nao grava efeito definitivo sem confirmacao. |
| Modulo destino | Receber plano confirmado e aplicar suas regras. | Consumir evento/contrato de confirmacao. | Governa a execucao real. |
| Suporte | Diagnosticar falhas e historico. | Consultar logs e payload mascarado. | Nao altera cenarios aprovados sem autorizacao. |

## 6. Visao operacional do submodulo

O usuario cria um cenario de planejamento com codigo, tenant, responsavel, status, horizonte, premissas de demanda, capacidade e lead time. O Epros valida obrigatorios, registra historico e permite executar simulacao em memoria. A simulacao calcula resultados taticos de disponibilidade, atendimento, alocacao e restricoes sem gerar movimentacao real ou ordem operacional definitiva.

O usuario pode comparar cenarios lado a lado, ajustar premissas, anexar evidencias e submeter para aprovacao quando houver impacto operacional. Enquanto nao confirmado, o resultado permanece como simulacao. Quando usuario autorizado confirma o plano, o Epros publica evento e encaminha o plano ao modulo destino, que aplica suas proprias regras de negocio.

O material nao informa tabelas fisicas, endpoints finais, algoritmo de ATP, algoritmo de capacidade, horizonte padrao, unidades, prioridades nem contratos finais com modulos. Esta EF cria o modelo funcional implantavel necessario para que a Siser valide a construcao.[^nota1]

## 7. Capacidades funcionais

### 7.1 Cadastro de cenario

| Item | Especificacao |
|---|---|
| Objetivo | Registrar cenario de planejamento por tenant, codigo, status e responsavel. |
| Acionamento | Usuario cria ou edita cenario. |
| Pre-condicoes | Usuario autenticado, tenant definido e permissao. |
| Dados de entrada | Codigo, descricao, responsavel, horizonte, premissas, modulo destino e observacoes. |
| Processamento | O Epros valida obrigatorios, grava Rascunho e registra historico. |
| Resultado esperado | Cenario criado para simulacao. |
| Pos-condicoes | Cenario pode receber premissas, demandas e capacidade. |
| Excecoes | Tenant ausente, codigo ausente, responsavel ausente, horizonte invalido. |
| Auditoria | Usuario, acao, payload, timestamp e IP quando disponivel. |

### 7.2 Definicao de premissas

| Item | Especificacao |
|---|---|
| Objetivo | Informar demanda, capacidade, lead time e parametros do cenario. |
| Acionamento | Usuario configura premissas. |
| Pre-condicoes | Cenario em estado editavel. |
| Dados de entrada | Demanda, capacidade, lead time, produto, local, pedido, recurso, prioridade e horizonte quando aplicavel. |
| Processamento | O Epros valida dominio, vinculos e consistencia minima das premissas. |
| Resultado esperado | Premissas prontas para simulacao. |
| Pos-condicoes | Simulacao pode ser executada. |
| Excecoes | Cadastro referenciado inexistente, valor negativo, periodo invalido, parametro obrigatorio ausente. |
| Auditoria | Alteracao de premissas e responsavel. |

### 7.3 Execucao de simulacao em memoria

| Item | Especificacao |
|---|---|
| Objetivo | Calcular resultado tatico sem efeito definitivo. |
| Acionamento | Usuario solicita simular. |
| Pre-condicoes | Cenario e premissas validos. |
| Dados de entrada | Premissas, demanda, capacidade, lead time, estoque/ordens/pedidos referenciados. |
| Processamento | O Epros calcula disponibilidade, restricoes, alocacao e resultado simulado. |
| Resultado esperado | Resultado armazenado como simulacao. |
| Pos-condicoes | Cenario pode ser comparado, ajustado ou submetido. |
| Excecoes | Dados insuficientes, modulo de origem indisponivel, regra de fronteira sem contrato. |
| Auditoria | Inicio, termino, parametros, resultado resumido e erro. |

### 7.4 Comparacao de cenarios

| Item | Especificacao |
|---|---|
| Objetivo | Comparar cenarios lado a lado para apoiar decisao. |
| Acionamento | Usuario seleciona dois ou mais cenarios. |
| Pre-condicoes | Cenarios simulados e usuario autorizado. |
| Dados de entrada | Lista de cenarios e indicadores comparaveis. |
| Processamento | O Epros apresenta diferencas de atendimento, atraso, uso de capacidade, disponibilidade e restricoes quando informadas. |
| Resultado esperado | Comparacao funcional de alternativas. |
| Pos-condicoes | Usuario pode escolher cenario para confirmacao. |
| Excecoes | Cenarios incompativeis, sem simulacao ou sem mesmo horizonte. |
| Auditoria | Cenarios comparados e usuario. |

### 7.5 Confirmacao de plano

| Item | Especificacao |
|---|---|
| Objetivo | Transformar resultado aprovado em plano enviado ao modulo destino. |
| Acionamento | Usuario autorizado confirma plano. |
| Pre-condicoes | Cenario simulado, aprovado quando necessario e contrato de destino definido. |
| Dados de entrada | Cenario, resultado, destino, usuario, justificativa e payload de confirmacao. |
| Processamento | O Epros valida estado, evita duplicidade, publica evento e envia plano ao destino. |
| Resultado esperado | Plano confirmado e rastreavel. |
| Pos-condicoes | Modulo destino aplica suas regras e passa a governar execucao. |
| Excecoes | Cenario nao aprovado, destino sem contrato, tentativa duplicada, falha do destino. |
| Auditoria | Confirmacao, payload, destino, usuario e status. |

### 7.6 Relatorios e indicadores

| Item | Especificacao |
|---|---|
| Objetivo | Disponibilizar visao operacional dos cenarios e auditoria. |
| Acionamento | Usuario consulta lista, painel ou relatorio. |
| Pre-condicoes | Usuario autorizado. |
| Dados de entrada | Status, periodo, responsavel, produto, local, modulo, cenario. |
| Processamento | O Epros filtra, pagina, consolida KPIs e exporta quando permitido. |
| Resultado esperado | Posicao geral, auditoria e indicadores. |
| Pos-condicoes | Informacoes podem alimentar Analytics. |
| Excecoes | Filtro invalido, permissao ausente, dado restrito. |
| Auditoria | Consulta e exportacao quando sensivel. |

## 8. Regras de negocio

| Regra | Descricao | Condicao | Resultado | Severidade | Observacoes |
|---|---|---|---|---|---|
| PIM-001 | Todo cenario deve possuir tenant. | Criacao, edicao, simulacao e confirmacao. | Operacao sem tenant e bloqueada. | Bloqueante | Material informa TenantId obrigatorio. |
| PIM-002 | Cenario deve possuir codigo, status e responsavel. | Cadastro e manutencao. | Ausencia bloqueia persistencia. | Bloqueante | Campos informados. |
| PIM-003 | Novo cenario nasce em Rascunho. | Criacao valida. | Status Rascunho. | Bloqueante | Fluxo informado. |
| PIM-004 | Rascunho pode ser submetido para EmAnalise por operador. | Submissao. | Status EmAnalise. | Normal | Fluxo informado. |
| PIM-005 | EmAnalise pode ser aprovado e tornar-se Ativo. | Aprovacao. | Status Ativo. | Bloqueante | Fluxo informado. |
| PIM-006 | EmAnalise pode ser rejeitado e voltar a Rascunho com motivo. | Rejeicao. | Status Rascunho. | Normal | Fluxo informado. |
| PIM-007 | Ativo pode ser inativado ou encerrado por gestor. | Gestao. | Status Inativo ou Encerrado. | Normal | Fluxo informado. |
| PIM-008 | Inativo pode ser reativado por gestor. | Reativacao. | Status Ativo. | Normal | Fluxo informado. |
| PIM-009 | Transicoes devem registrar usuario, timestamp e IP quando disponivel. | Alteracao de estado. | Historico gravado. | Bloqueante | Auditoria informada. |
| PIM-010 | Eventos de dominio devem ser publicados apos confirmacao transacional. | Persistencia concluida. | Evento publicado. | Normal | Material informa eventos apos commit. |
| PIM-011 | Simulacao deve considerar demanda, capacidade e lead time quando informados. | Execucao de cenario. | Resultado calculado com as premissas. | Bloqueante | Requisito informado. |
| PIM-012 | Resultado de simulacao nao deve gerar efeito definitivo antes da confirmacao. | Simulacao executada. | Nenhuma ordem, movimento ou alocacao real e criada. | Bloqueante | Requisito informado. |
| PIM-013 | Cenarios devem poder ser comparados lado a lado. | Consulta comparativa. | Diferencas e indicadores exibidos. | Normal | Requisito informado. |
| PIM-014 | Confirmacao de plano exige usuario autorizado. | Usuario confirma resultado. | Sem permissao, confirmacao bloqueada. | Bloqueante | Derivado do workflow. |
| PIM-015 | Confirmacao deve encaminhar o plano ao modulo destino. | Plano confirmado. | Evento/contrato de destino acionado. | Bloqueante | Requisito informado. |
| PIM-016 | Modulo destino deve governar a execucao real. | Plano confirmado. | Planejamento In Memory nao executa regra final. | Bloqueante | Fronteira funcional. |
| PIM-017 | Cenario confirmado nao deve ser confirmado novamente sem regra explicita. | Nova tentativa de confirmacao. | Tentativa duplicada bloqueada. | Bloqueante | Criado para evitar duplicidade.[^nota1] |
| PIM-018 | Cenarios devem referenciar cadastros mestres por identificador. | Premissas e resultados. | Nao duplica produto, pedido, recurso ou pessoa. | Bloqueante | Escopo informa nao duplicar cadastros. |
| PIM-019 | Anexos devem referenciar arquivo do GED. | Inclusao de anexo. | Anexo sem arquivo e bloqueado. | Bloqueante | Campo ArquivoId informado. |
| PIM-020 | Dados pessoais devem seguir privacidade, retencao e mascaramento. | Cenario com dados pessoais. | Dado protegido conforme Compliance. | Bloqueante | LGPD citada no material. |

## 9. Parametros de configuracao

| Parametro | Finalidade | Tipo/formato | Valor padrao | Obrigatorio | Nivel | Quem pode alterar | Impacto |
|---|---|---|---|---|---|---|---|
| PlanejamentoInMemoryHabilitado | Ativar o submodulo no tenant. | Booleano | Nao informado no material | Sim | Tenant | Administrador | Permite criacao de cenarios. |
| HorizontePadrao | Definir periodo padrao da simulacao. | Periodo | Nao informado no material | Sim | Tenant | Gestor | Afeta calculo e comparacao. |
| ModulosDestinoPermitidos | Definir destinos permitidos para confirmacao. | Lista | Estoque, Vendas e Producao citados | Sim | Tenant | Administrador | Controla fronteira. |
| PoliticaAprovacao | Definir se confirmacao exige aprovacao. | Enum/booleano | Nao informado no material | Sim | Tenant/modulo | Gestor | Controla workflow. |
| ComparacaoMaximaCenarios | Limitar quantidade comparada lado a lado. | Inteiro | Nao informado no material | Nao | Tenant | Gestor | Afeta tela e desempenho. |
| RetencaoCenarios | Definir prazo de guarda de cenarios. | Periodo | Nao informado no material | Sim | Tenant | Compliance/Siser | Afeta auditoria e custo. |
| LimiteExecucaoSimulacao | Definir limite de tempo ou tamanho de simulacao. | Inteiro/duracao | Nao informado no material | Sim | Tenant | Siser | Protege desempenho. |

## 10. Modelo de dados funcional e implantavel

### 10.1 Visao geral do modelo

O material informa entidade principal com Id, TenantId, Codigo, Status e ResponsavelId, historico com Acao, UsuarioId e PayloadJson, anexo com ArquivoId, alem dos requisitos de cenario what-if, demanda, capacidade, lead time, resultado nao persistente ate confirmacao, comparacao lado a lado e exportacao ao confirmar. As entidades abaixo consolidam esses elementos em modelo funcional implantavel do Epros.[^nota1]

| Grupo de dados | Entidades/tabelas | Papel funcional | Observacoes |
|---|---|---|---|
| Cenario | `pim_cenario`, `pim_premissa` | Define cenario, horizonte e premissas. | Preserva campos raiz. |
| Entrada | `pim_demanda`, `pim_capacidade`, `pim_restricao` | Guarda dados de simulacao. | Criado a partir de requisitos. |
| Resultado | `pim_resultado`, `pim_resultado_item`, `pim_comparacao` | Registra resultado e comparacao. | Sem efeito definitivo ate confirmacao. |
| Confirmacao | `pim_plano_confirmado`, `pim_evento_destino` | Encaminha plano ao modulo destino. | Fronteira com modulos. |
| Auditoria | `pim_historico`, `pim_anexo` | Trilha e anexos. | Campos informados. |

### 10.2 Entidades, finalidade e cardinalidade

| Entidade | Finalidade | Cardinalidade | Observacao |
|---|---|---|---|
| `pim_cenario` | Cadastro mestre do cenario. | 1 tenant possui N cenarios. | Preserva Id, TenantId, Codigo, Status e ResponsavelId. |
| `pim_premissa` | Parametros gerais do cenario. | 1 cenario possui N premissas. | Demanda/capacidade/lead time. |
| `pim_demanda` | Demanda considerada na simulacao. | 1 cenario possui N demandas. | Referencia pedidos/produtos quando aplicavel. |
| `pim_capacidade` | Capacidade considerada na simulacao. | 1 cenario possui N capacidades. | Referencia recurso/local. |
| `pim_restricao` | Restricao ou limitacao do cenario. | 1 cenario possui N restricoes. | Criado para explicabilidade. |
| `pim_resultado` | Resultado consolidado da simulacao. | 1 cenario possui N execucoes. | Simulado, nao definitivo. |
| `pim_resultado_item` | Linha detalhada de resultado. | 1 resultado possui N itens. | Alocacao/ATP/restricao por item. |
| `pim_comparacao` | Comparacao entre cenarios. | 1 comparacao possui N cenarios. | Requisito informado. |
| `pim_plano_confirmado` | Registro de confirmacao. | 1 cenario possui 0 ou 1 confirmacao ativa. | Evita duplicidade. |
| `pim_evento_destino` | Evento enviado ao modulo destino. | 1 confirmacao possui N eventos. | Rastreia fronteira. |
| `pim_historico` | Historico funcional. | N historicos por entidade. | Campos informados. |
| `pim_anexo` | Anexos via GED. | N anexos por entidade. | Campo ArquivoId. |

## 11. Dicionario de dados implantavel

### 11.1 `pim_cenario`

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| id | UUID/inteiro | Nao informado no material | Sim | PK | Identificador do cenario. |
| tenant_id | UUID/inteiro | Nao informado no material | Sim | FK tenant | Campo TenantId informado. |
| codigo | Texto | Nao informado no material | Sim | Unico por tenant | Campo Codigo informado. |
| nome | Texto | Nao informado no material | Sim |  | Nome funcional criado para operacao.[^nota1] |
| descricao | Texto | Nao informado no material | Nao |  | Descricao do cenario. |
| status | Enum | Rascunho/EmAnalise/Ativo/Inativo/Encerrado | Sim |  | Campo Status e fluxo informados. |
| responsavel_id | UUID/inteiro | Nao informado no material | Sim | FK usuario/pessoa | Campo ResponsavelId informado. |
| horizonte_inicio | Data | ISO 8601 | Condicional |  | Criado para horizonte de simulacao.[^nota1] |
| horizonte_fim | Data | ISO 8601 | Condicional |  | Criado para horizonte de simulacao.[^nota1] |
| modulo_destino | Texto | Estoque/Vendas/Producao/outros homologados | Condicional |  | Destino da confirmacao. |
| criado_em | Data/hora | ISO 8601 | Sim |  | Criacao. |
| atualizado_em | Data/hora | ISO 8601 | Sim |  | Atualizacao. |

### 11.2 `pim_premissa`

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| id | UUID/inteiro | Nao informado no material | Sim | PK | Identificador da premissa. |
| cenario_id | UUID/inteiro | Nao informado no material | Sim | FK pim_cenario | Cenario dono. |
| tipo | Enum | Demanda/Capacidade/LeadTime/Parametro | Sim |  | Tipos informados. |
| chave | Texto | Nao informado no material | Sim |  | Nome da premissa. |
| valor | Decimal/texto/JSON | Nao informado no material | Sim |  | Valor da premissa. |
| unidade | Texto | Nao informado no material | Nao |  | Unidade da premissa. |
| origem | Texto | Nao informado no material | Nao |  | Origem da informacao. |
| observacao | Texto | Nao informado no material | Nao |  | Comentarios. |

### 11.3 `pim_demanda`

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| id | UUID/inteiro | Nao informado no material | Sim | PK | Identificador da demanda. |
| cenario_id | UUID/inteiro | Nao informado no material | Sim | FK pim_cenario | Cenario. |
| produto_id | UUID/inteiro | Nao informado no material | Condicional | FK produto | Referencia cadastro mestre. |
| pedido_id | UUID/inteiro | Nao informado no material | Nao | FK pedido | Referencia Vendas quando aplicavel. |
| local_id | UUID/inteiro | Nao informado no material | Nao | FK local/estoque | Local considerado. |
| quantidade | Decimal | Nao informado no material | Sim |  | Quantidade demandada. |
| data_necessidade | Data | ISO 8601 | Condicional |  | Data de atendimento. |
| prioridade | Inteiro/texto | Nao informado no material | Nao |  | Prioridade tatica. |

### 11.4 `pim_capacidade`

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| id | UUID/inteiro | Nao informado no material | Sim | PK | Identificador da capacidade. |
| cenario_id | UUID/inteiro | Nao informado no material | Sim | FK pim_cenario | Cenario. |
| recurso_id | UUID/inteiro | Nao informado no material | Condicional | FK recurso | Recurso produtivo/logistico. |
| local_id | UUID/inteiro | Nao informado no material | Nao | FK local | Local de capacidade. |
| capacidade | Decimal | Nao informado no material | Sim |  | Capacidade disponivel. |
| unidade | Texto | Nao informado no material | Nao |  | Unidade. |
| periodo_inicio | Data/hora | ISO 8601 | Condicional |  | Inicio do periodo. |
| periodo_fim | Data/hora | ISO 8601 | Condicional |  | Fim do periodo. |

### 11.5 `pim_resultado`

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| id | UUID/inteiro | Nao informado no material | Sim | PK | Identificador do resultado. |
| cenario_id | UUID/inteiro | Nao informado no material | Sim | FK pim_cenario | Cenario simulado. |
| tipo_resultado | Enum | ATP/WhatIf/Alocacao | Sim |  | Tipos informados/consolidados. |
| status | Enum | Simulado/Erro/Confirmado/Descartado | Sim |  | Estados criados para operacao.[^nota1] |
| resumo_json | JSON | Nao informado no material | Sim |  | Resultado consolidado. |
| score_atendimento | Decimal | Nao informado no material | Nao |  | Indicador criado para comparacao.[^nota1] |
| restricoes_json | JSON | Nao informado no material | Nao |  | Restricoes do resultado. |
| executado_por | UUID/inteiro | Nao informado no material | Sim | FK usuario | Usuario/processo. |
| executado_em | Data/hora | ISO 8601 | Sim |  | Data da simulacao. |

### 11.6 `pim_resultado_item`

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| id | UUID/inteiro | Nao informado no material | Sim | PK | Identificador do item. |
| resultado_id | UUID/inteiro | Nao informado no material | Sim | FK pim_resultado | Resultado dono. |
| demanda_id | UUID/inteiro | Nao informado no material | Nao | FK pim_demanda | Demanda atendida. |
| produto_id | UUID/inteiro | Nao informado no material | Condicional | FK produto | Produto. |
| quantidade_demandada | Decimal | Nao informado no material | Nao |  | Quantidade demandada. |
| quantidade_atendida | Decimal | Nao informado no material | Nao |  | Quantidade simulada como atendida. |
| data_promessa | Data | ISO 8601 | Nao |  | Data ATP simulada. |
| local_origem_id | UUID/inteiro | Nao informado no material | Nao | FK local | Origem sugerida. |
| restricao | Texto | Nao informado no material | Nao |  | Restricao identificada. |
| payload_json | JSON | Nao informado no material | Nao |  | Detalhe adicional. |

### 11.7 `pim_comparacao`

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| id | UUID/inteiro | Nao informado no material | Sim | PK | Identificador da comparacao. |
| tenant_id | UUID/inteiro | Nao informado no material | Sim | FK tenant | Segregacao. |
| nome | Texto | Nao informado no material | Sim |  | Nome da comparacao. |
| cenario_ids | JSON/lista | Nao informado no material | Sim | FK logica pim_cenario | Cenarios comparados. |
| resultado_json | JSON | Nao informado no material | Sim |  | Indicadores comparativos. |
| criado_por | UUID/inteiro | Nao informado no material | Sim | FK usuario | Usuario. |
| criado_em | Data/hora | ISO 8601 | Sim |  | Criacao. |

### 11.8 `pim_plano_confirmado`

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| id | UUID/inteiro | Nao informado no material | Sim | PK | Identificador da confirmacao. |
| cenario_id | UUID/inteiro | Nao informado no material | Sim | FK pim_cenario | Cenario confirmado. |
| resultado_id | UUID/inteiro | Nao informado no material | Sim | FK pim_resultado | Resultado confirmado. |
| modulo_destino | Texto | Nao informado no material | Sim |  | Modulo que recebe plano. |
| status | Enum | Pendente/Enviado/Aceito/Rejeitado | Sim |  | Estado de envio. |
| payload_confirmacao | JSON | Nao informado no material | Sim |  | Plano enviado. |
| confirmado_por | UUID/inteiro | Nao informado no material | Sim | FK usuario | Usuario. |
| confirmado_em | Data/hora | ISO 8601 | Sim |  | Data de confirmacao. |
| erro | Texto | Nao informado no material | Nao |  | Motivo de falha. |

### 11.9 `pim_historico`

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| id | UUID/inteiro | Nao informado no material | Sim | PK | Identificador. |
| tenant_id | UUID/inteiro | Nao informado no material | Sim | FK tenant | Segregacao. |
| entidade | Texto | Nao informado no material | Sim |  | Entidade afetada. |
| entidade_id | UUID/inteiro/texto | Nao informado no material | Sim |  | Registro afetado. |
| acao | Texto | Nao informado no material | Sim |  | Campo Acao informado. |
| usuario_id | UUID/inteiro | Nao informado no material | Sim | FK usuario | Campo UsuarioId informado. |
| payload_json | JSON | Nao informado no material | Sim |  | Campo PayloadJson informado; mascarar sensiveis. |
| ip | IP | IPv4/IPv6 | Nao |  | Auditoria de transicao. |
| criado_em | Data/hora | ISO 8601 | Sim |  | Data do historico. |

### 11.10 `pim_anexo`

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| id | UUID/inteiro | Nao informado no material | Sim | PK | Identificador. |
| entidade | Texto | Nao informado no material | Sim |  | Cenario, resultado ou confirmacao. |
| entidade_id | UUID/inteiro/texto | Nao informado no material | Sim |  | Registro relacionado. |
| arquivo_id | UUID/inteiro | Nao informado no material | Sim | FK GED | Campo ArquivoId informado. |
| criado_por | UUID/inteiro | Nao informado no material | Sim | FK usuario | Usuario. |
| criado_em | Data/hora | ISO 8601 | Sim |  | Data de inclusao. |

## 12. Fluxos e estados

### 12.1 Ciclo de vida do cenario

| Estado atual | Evento | Proximo estado | Permissao | Regra |
|---|---|---|---|---|
| Rascunho | Submeter | EmAnalise | Operador | Validar obrigatorios e premissas minimas. |
| EmAnalise | Aprovar | Ativo | Aprovador | Cenario pode ser usado como referencia. |
| EmAnalise | Rejeitar | Rascunho | Aprovador | Exigir motivo. |
| Ativo | Inativar | Inativo | Gestor | Bloqueia nova confirmacao. |
| Ativo | Encerrar | Encerrado | Gestor | Finaliza ciclo. |
| Inativo | Reativar | Ativo | Gestor | Reabilita cenario. |

### 12.2 Estados do resultado

| Estado | Significado | Proxima acao |
|---|---|---|
| Simulado | Resultado calculado sem efeito definitivo. | Comparar, ajustar ou confirmar. |
| Erro | Simulacao nao foi concluida. | Corrigir premissas ou integracao. |
| Confirmado | Resultado foi escolhido para envio ao destino. | Aguardar aceite/rejeicao do destino. |
| Descartado | Resultado nao sera usado. | Arquivar historico. |

### 12.3 Fluxos principais

| Fluxo | Passos principais | Resultado esperado |
|---|---|---|
| Criar cenario | Criar, informar premissas, salvar Rascunho. | Cenario pronto para simulacao. |
| Simular | Validar premissas, calcular em memoria, salvar resultado simulado. | Resultado sem efeito definitivo. |
| Comparar | Selecionar cenarios, comparar indicadores, exibir diferencas. | Apoio a decisao. |
| Confirmar | Validar permissao, evitar duplicidade, enviar ao destino. | Plano confirmado e rastreavel. |

## 13. APIs e contratos funcionais

| Contrato | Direcao | Entrada | Saida | Observacoes |
|---|---|---|---|---|
| Criar cenario | Cliente para Epros | Codigo, responsavel, horizonte, modulo destino | Cenario Rascunho | Endpoint final nao informado. |
| Configurar premissas | Cliente para Epros | Demanda, capacidade, lead time, parametros | Premissas salvas | Contrato final pendente. |
| Executar simulacao | Cliente para Epros | Cenario e premissas | Resultado simulado | Algoritmo nao informado. |
| Comparar cenarios | Cliente para Epros | Lista de cenarios | Comparacao | Requisito informado. |
| Confirmar plano | Cliente para Epros | Resultado, destino, justificativa | Evento/plano enviado | Modulo destino aplica regras finais. |
| Consultar historico | Cliente para Epros | Entidade, periodo, usuario | Trilha de auditoria | Material informa auditoria. |

## 14. Telas, consultas e relatorios

| Interface | Objetivo | Campos/acoes minimas | Observacoes |
|---|---|---|---|
| Lista de cenarios | Consultar cenarios. | Status, periodo, responsavel, novo, exportar. | Material informa lista. |
| Detalhe do cenario | Manter dados, historico, anexos e aprovacao. | Dados, premissas, historico, anexos, aprovacao. | Material informa abas. |
| Simulacao | Executar ATP/what-if. | Demanda, capacidade, lead time, executar, resultado. | Criada a partir dos requisitos. |
| Comparacao lado a lado | Comparar cenarios. | Cenarios, indicadores, diferencas, escolher. | Requisito informado. |
| Confirmacao | Confirmar plano. | Resultado, destino, justificativa, confirmar. | Deve bloquear duplicidade. |
| Painel gestor | KPIs e fila de aprovacao. | Cenarios por status, simulacoes, confirmados, erros. | Material informa painel gestor. |

| Relatorio | Descricao | Filtros | Observacoes |
|---|---|---|---|
| Posicao geral | Snapshot por status. | Status, periodo, responsavel. | Material informa REL-PIM-001. |
| Auditoria de alteracoes | Trilha por periodo. | Usuario, acao, periodo. | Material informa REL-PIM-002. |
| Resultado de simulacao | Resultado de cenarios executados. | Cenario, produto, local, periodo. | Criado para operacao.[^nota1] |
| Comparacao de cenarios | Indicadores lado a lado. | Cenarios, periodo, responsavel. | Requisito informado. |
| Confirmacoes | Planos confirmados e destino. | Destino, status, periodo. | Necessario para rastreabilidade. |

## 15. Seguranca, privacidade e auditoria

| Tema | Regra funcional |
|---|---|
| Tenant | Todo cenario, premissa, resultado e confirmacao deve possuir tenant. |
| Permissao | Criar, simular, aprovar, confirmar e exportar exigem permissao. |
| Fronteira | Resultado simulado nao pode gerar efeito definitivo antes de confirmacao. |
| Idempotencia | Confirmacao deve evitar envio duplicado do mesmo resultado. |
| Dados pessoais | Premissas com pessoa, cliente ou responsavel devem seguir Compliance. |
| Payload | Historico deve mascarar dados sensiveis em consulta. |
| Anexos | Arquivos devem ser referenciados pelo GED. |
| Auditoria | Simulacao, comparacao e confirmacao devem ser rastreaveis. |

## 16. Testes funcionais minimos

| Cenario | Dado/condicao | Resultado esperado |
|---|---|---|
| Criar cenario valido | Tenant, codigo, status e responsavel informados. | Status Rascunho. |
| Criar sem obrigatorios | Falta tenant/codigo/status/responsavel. | Erro de validacao. |
| Aprovar cenario | Registro EmAnalise e aprovador autorizado. | Status Ativo. |
| Integracao rejeita rascunho | Cenario ainda Rascunho. | Nenhum evento de plano. |
| Inativar com referencia | Cenario ativo em uso. | Bloqueio ou inativacao conforme politica. |
| Mascaramento LGPD | Payload com dado pessoal. | Campo oculto em consulta. |
| Simular com premissas | Demanda, capacidade e lead time preenchidos. | Resultado Simulado. |
| Simular sem premissas minimas | Dados insuficientes. | Erro funcional. |
| Comparar cenarios | Dois cenarios simulados. | Comparacao lado a lado. |
| Confirmar sem permissao | Usuario sem permissao confirma. | Operacao bloqueada. |
| Confirmar resultado | Resultado simulado e destino definido. | Plano enviado ao destino. |
| Confirmar duas vezes | Mesmo resultado ja confirmado. | Segunda confirmacao bloqueada. |

## 17. Criterios de aceite

| Codigo | Criterio |
|---|---|
| CA-PIM-001 | Cenario deve possuir tenant, codigo, status e responsavel. |
| CA-PIM-002 | Workflow Rascunho, EmAnalise, Ativo, Inativo e Encerrado deve funcionar com auditoria. |
| CA-PIM-003 | Cenario deve permitir premissas de demanda, capacidade e lead time. |
| CA-PIM-004 | Simulacao deve gerar resultado sem efeito definitivo. |
| CA-PIM-005 | Cenarios simulados devem poder ser comparados lado a lado. |
| CA-PIM-006 | Confirmacao deve exigir permissao e destino definido. |
| CA-PIM-007 | Confirmacao deve encaminhar plano ao modulo destino sem duplicidade. |
| CA-PIM-008 | Modulo destino deve governar a execucao real. |
| CA-PIM-009 | Historico deve registrar usuario, acao, payload, timestamp e IP quando disponivel. |
| CA-PIM-010 | Anexos devem referenciar GED. |

## 18. Notas de autoria e saneamento funcional

[^nota1]: O modelo de cenario, premissa, demanda, capacidade, resultado, comparacao e confirmacao foi criado nesta EF para tornar o Epros implantavel. O material comprova ATP, what-if, demanda, capacidade, lead time, resultado nao persistente ate confirmacao, comparacao lado a lado e exportacao ao confirmar, mas nao informa tabelas fisicas, algoritmos, payloads ou contratos finais.
