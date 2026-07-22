# EF 3 Plataforma Compartilhada — Analytics e Mobilidade V1

## 1. Identificacao

| Item | Valor |
|---|---|
| Sistema | Epros |
| Empresa | Siser |
| Modulo | Plataforma Compartilhada |
| Submodulo | Analytics e Mobilidade |
| Versao | V1 |
| Status | Especificacao funcional para validacao humana |
| Data | 2026-06-06 |

## 2. Objetivo funcional

O submodulo Analytics e Mobilidade fornece a camada compartilhada de paineis, indicadores, relatorios analiticos, estatisticas de uso, exportacoes auditadas e APIs moveis do Epros. Ele consolida dados dos modulos operacionais sem duplicar seus cadastros ou suas regras transacionais.

O submodulo permite criar paineis por tenant, perfil e usuario; consultar KPIs por periodo e dimensao; visualizar relatorios por area de negocio; controlar acesso aos relatorios; registrar estatisticas de acesso/download; exportar dados com governanca; e expor a mesma informacao para canais web e moveis.

## 3. Escopo

### 3.1 Dentro do escopo

| Capacidade | Descricao |
|---|---|
| Catalogo de indicadores | Definir KPIs, metricas, formulas, dimensoes, filtros e fontes de dados. |
| Paineis | Organizar widgets, graficos, tabelas e atalhos por tenant, perfil e usuario. |
| Relatorios analiticos | Disponibilizar relatorios de vendas, faturamento, estimativas, projetos, clientes, horas, financeiro, despesas e propostas quando houver fonte funcional. |
| Estatisticas | Registrar eventos de acesso, download, visualizacao, origem, navegador, sistema operacional, pais e usuario. |
| Mobilidade | Expor APIs para consulta movel, fila offline, sincronizacao e filtros compactos. |
| Exportacao | Exportar dados de relatorio com auditoria, permissao e controle de dados pessoais. |
| Permissao | Controlar modulo habilitado, perfil com acesso a relatorios, escopo de dados e permissao por painel/relatorio. |
| Filtros | Aplicar filtros por periodo, ano, cliente, projeto, categoria, status, membro da equipe e demais dimensoes do relatorio. |
| Cache | Guardar resultados agregados por tenant, periodo e parametros para reduzir custo de consulta. |
| Auditoria | Registrar consulta, exportacao, alteracao de definicao, alteracao de painel e acesso a dado sensivel. |
| Eventos | Publicar eventos de consulta/exportacao relevantes e consumir eventos dos modulos fonte. |

### 3.2 Fora do escopo

| Tema | Tratamento |
|---|---|
| Criar documentos de venda, compra, financeiro ou projeto | Pertence aos modulos transacionais. |
| Substituir contabilidade, faturamento ou estoque | Analytics apenas agrega e apresenta dados. |
| Definir regras tributarias ou financeiras | Pertence aos modulos donos. |
| Guardar dado mestre duplicado | Deve referenciar dados por chaves dos modulos fonte. |
| Construir relatorio sem fonte funcional conhecida | Deve permanecer como lacuna na MC. |

## 4. Dependencias e consumidores

### 4.1 Dependencias

| Dependencia | Uso |
|---|---|
| Identidade e Contexto Tenant | Tenant, usuario, perfil, empresa ativa e escopo. |
| Permissoes de Menu | Acesso a relatorios, paineis, exportacoes e KPIs. |
| Compliance e Privacidade | Mascaramento, base legal, retencao, exportacao e log de acesso. |
| Gestao Eletronica de Documentos | Armazenamento de exportacoes quando formalizadas como arquivo. |
| API Gateway | Versionamento, autenticacao e contratos de API. |
| Workflow | Aprovacao de publicacao de painel/indicador quando configurada. |
| Modulos de negocio | Fontes de fatos, dimensoes e indicadores. |

### 4.2 Consumidores

| Consumidor | Uso |
|---|---|
| Aplicativo | Paineis iniciais, indicadores do tenant e visao administrativa. |
| Financeiro | Relatorios de faturamento, recebimentos, pagamentos, despesas, saldos e resultado mensal. |
| Vendas | Indicadores de pedidos, clientes, propostas, estimativas, devolucoes, metas e PDV. |
| Compras | Indicadores de fornecedores, pedidos, compras, devolucoes e prazos. |
| Estoque | Indicadores de estoque, movimentos, armazens, rupturas, lotes e inventarios. |
| Projetos | Indicadores por projeto, cliente, status, horas, tarefas e despesas. |
| RH | Indicadores de horas, equipe, produtividade, ausencia e treinamento. |
| Qualidade | Indicadores de nao conformidade, inspeções e fornecedores. |
| Manutencao | Indicadores de ordens, paradas, SLA, pecas e disponibilidade. |
| Relatorios | Consome catalogos e execucao analitica compartilhada. |
| Mobilidade | Apps e canais moveis consomem paineis, KPIs e filas offline. |

## 5. Principios funcionais

| Codigo | Regra |
|---|---|
| REG-ANL-001 | Todo dado analitico tenantizado deve respeitar o tenant corrente. |
| REG-ANL-002 | Nenhuma API deve aceitar TenantId editavel para ampliar escopo. |
| REG-ANL-003 | Relatorios agregam dados dos modulos fonte e nao duplicam regras transacionais. |
| REG-ANL-004 | Cada indicador deve ter dono funcional, formula, fonte, granularidade, vigencia e versao. |
| REG-ANL-005 | Todo painel publicado deve ter permissao, escopo e status. |
| REG-ANL-006 | Exportacoes devem ser auditadas. |
| REG-ANL-007 | Dados pessoais em relatorios devem respeitar mascaramento, base legal e permissao. |
| REG-ANL-008 | Consultas pesadas devem usar cache ou processamento assincrono conforme volume. |
| REG-ANL-009 | Resultados de relatorios devem ser reproduziveis a partir de parametros, versao e periodo. |
| REG-ANL-010 | Relatorios sem fonte funcional confirmada permanecem desativados e aparecem como lacuna na MC. |

## 6. Regras funcionais detalhadas

### 6.1 Acesso e habilitacao

| Codigo | Regra |
|---|---|
| REG-ANL-011 | O tenant deve possuir modulo de relatorios/analytics habilitado para acessar paineis analiticos. |
| REG-ANL-012 | Usuario interno precisa permissao de visualizacao de relatorios para acessar o hub. |
| REG-ANL-013 | Usuario cliente externo nao pode acessar relatorios internos salvo permissao especifica e escopo contratado. |
| REG-ANL-014 | O menu de relatorios deve aparecer apenas para perfis autorizados. |
| REG-ANL-015 | Permissoes devem ser avaliadas tambem em chamadas de dados, nao apenas na tela. |
| REG-ANL-016 | Cada relatorio pode exigir permissao propria alem da permissao geral de analytics. |
| REG-ANL-017 | Exportacao exige permissao adicional quando incluir dados pessoais, financeiros ou sensiveis. |
| REG-ANL-018 | Escopo por usuario deve limitar clientes, projetos, empresas, equipes ou centros permitidos quando configurado. |

### 6.2 Hub e navegacao

| Codigo | Regra |
|---|---|
| REG-ANL-019 | O hub de analytics deve apresentar secoes por dominio: faturamento, estimativas, projetos, clientes, horas, financeiro, despesas e propostas. |
| REG-ANL-020 | O hub deve possuir pagina inicial com resumo e atalhos para relatorios disponiveis. |
| REG-ANL-021 | Secoes sem fonte funcional confirmada devem aparecer desabilitadas ou ocultas conforme configuracao. |
| REG-ANL-022 | Navegacao deve preservar filtros de periodo e contexto quando o usuario troca de relatorio da mesma familia. |
| REG-ANL-023 | O usuario pode favoritar relatorios e widgets quando a politica do tenant permitir. |
| REG-ANL-024 | Paginacao deve ser suportada em relatorios tabulares. |
| REG-ANL-025 | O limite padrao de pagina deve ser 25 quando o usuario nao informar outro valor. |
| REG-ANL-026 | Relatorios sem limite de pagina devem ser bloqueados ou convertidos para execucao assincrona quando houver risco de volume. |

### 6.3 Filtros de periodo e dimensoes

| Codigo | Regra |
|---|---|
| REG-ANL-027 | Filtro de periodo deve aceitar intervalo customizado, mes atual, mes anterior, ano atual e ano anterior. |
| REG-ANL-028 | Intervalo customizado exige data inicial e data final. |
| REG-ANL-029 | Relatorios mensais devem aceitar filtro de ano. |
| REG-ANL-030 | Quando o ano nao for informado, o ano corrente deve ser usado. |
| REG-ANL-031 | Filtros por cliente, projeto, categoria, status, membro da equipe e centro devem respeitar permissao do usuario. |
| REG-ANL-032 | Filtros aplicados devem ficar registrados no historico de execucao do relatorio. |
| REG-ANL-033 | A mesma combinacao de filtros deve produzir resultado consistente enquanto a versao da formula e os dados fonte nao mudarem. |

### 6.4 Relatorios de faturamento

| Codigo | Regra |
|---|---|
| REG-ANL-034 | Relatorio de faturamento deve agregar documentos, pagamentos, clientes, projetos e categorias. |
| REG-ANL-035 | Documentos em rascunho devem ser excluidos de relatorios financeiros oficiais. |
| REG-ANL-036 | Valor recebido deve ser calculado por soma dos pagamentos vinculados. |
| REG-ANL-037 | Saldo em aberto deve ser valor final menos pagamentos. |
| REG-ANL-038 | Visao geral deve totalizar valor antes de impostos, impostos, desconto, ajustes, valor final, pagamentos e saldo. |
| REG-ANL-039 | Visao mensal deve agrupar por mes e ano da data do documento. |
| REG-ANL-040 | Visao por cliente deve agrupar totais por cliente. |
| REG-ANL-041 | Visao por categoria deve agrupar totais por categoria do documento. |
| REG-ANL-042 | Relatorios por projeto e categoria de projeto ficam pendentes quando nao houver fonte funcional confirmada. |

### 6.5 Relatorios de estimativas e propostas

| Codigo | Regra |
|---|---|
| REG-ANL-043 | Relatorios de estimativas devem espelhar dimensoes de faturamento quando houver dados equivalentes. |
| REG-ANL-044 | Estimativas em rascunho devem ser excluidas de indicadores oficiais quando o modulo fonte diferenciar rascunho. |
| REG-ANL-045 | Estimativas devem suportar visao geral, mensal, por cliente e por categoria. |
| REG-ANL-046 | Propostas devem permanecer como capacidade pendente quando a fonte funcional nao estiver confirmada. |

### 6.6 Relatorios de projetos, clientes e horas

| Codigo | Regra |
|---|---|
| REG-ANL-047 | Relatorio de projetos deve contar projetos por status e agregar tarefas, horas, despesas, documentos e pagamentos. |
| REG-ANL-048 | Relatorio por cliente deve consolidar projetos pendentes/concluidos, documentos em aberto/pagos/vencidos, estimativas e despesas. |
| REG-ANL-049 | Relatorio por categoria de projeto deve agrupar projetos e valores por categoria. |
| REG-ANL-050 | Relatorio de horas deve agrupar por equipe, cliente e projeto. |
| REG-ANL-051 | Horas faturadas e nao faturadas devem ser separadas quando a fonte possuir esse indicador. |
| REG-ANL-052 | Relatorios de horas devem suportar filtro por membro da equipe, cliente e projeto. |

### 6.7 Relatorio financeiro mensal

| Codigo | Regra |
|---|---|
| REG-ANL-053 | Relatorio financeiro mensal deve comparar receitas recebidas e despesas por mes. |
| REG-ANL-054 | O relatorio deve apresentar 12 meses para o ano filtrado. |
| REG-ANL-055 | Resultado do mes deve ser receita menos despesa. |
| REG-ANL-056 | Resultado acumulado deve seguir formula validada pela area financeira. |
| REG-ANL-057 | Mudanca de formula financeira exige nova versao do indicador. |

### 6.8 Estatisticas de acesso, download e uso

| Codigo | Regra |
|---|---|
| REG-ANL-058 | Evento estatistico deve registrar data/hora, referenciador, se referenciador e local, arquivo/recurso, pais, navegador, sistema operacional, IP, agente do usuario, URL base e usuario quando conhecido. |
| REG-ANL-059 | Estatistica pode contar apenas acessos unicos por IP, recurso e dia quando configurado. |
| REG-ANL-060 | Apos registrar acesso/download, o contador agregado do recurso deve ser atualizado. |
| REG-ANL-061 | O Epros deve identificar navegador, sistema operacional, pais e origem quando a informacao estiver disponivel. |
| REG-ANL-062 | Agentes conhecidos de download automatizado devem ser identificados para tratamento estatistico. |
| REG-ANL-063 | Estatisticas devem suportar graficos de ultimas 24 horas, 7 dias, 30 dias e 12 meses. |
| REG-ANL-064 | Estatisticas devem suportar graficos por pais, origem, navegador e sistema operacional. |
| REG-ANL-065 | Administrador pode ver graficos de uploads, usuarios, status de arquivos e tipos de arquivo quando o modulo de documentos estiver integrado. |
| REG-ANL-066 | Estatisticas detalhadas devem ter politica de retencao definida na MC. |

### 6.9 Relatorios operacionais e layouts

| Codigo | Regra |
|---|---|
| REG-ANL-067 | O Epros deve manter catalogo de layouts operacionais quando existirem relatorios documentais por dominio. |
| REG-ANL-068 | Cada layout deve possuir codigo, nome, dominio, formato, parametros, status e dono funcional. |
| REG-ANL-069 | Relatorios operacionais podem ser visualizados, impressos ou exportados conforme permissao. |
| REG-ANL-070 | Sub-relatorios devem declarar relatorio pai e parametros compartilhados. |
| REG-ANL-071 | Relatorios de boleto, financeiro, vendas, ordem de servico, fiscal e estoque devem ser catalogados sem misturar regras dos dominios fonte. |
| REG-ANL-072 | Layout sem uso confirmado deve permanecer em status EmValidacao ate decisao humana. |

### 6.10 Painel, widgets e KPIs

| Codigo | Regra |
|---|---|
| REG-ANL-073 | Painel pode ser global do tenant, por perfil ou pessoal do usuario. |
| REG-ANL-074 | Widget deve referenciar indicador, relatorio, grafico, tabela, atalho ou contador estatistico. |
| REG-ANL-075 | Widget deve possuir posicao, tamanho, configuracao visual, filtros e status. |
| REG-ANL-076 | Indicador deve possuir formula versionada, unidade, granularidade, fonte e regra de atualizacao. |
| REG-ANL-077 | Publicacao de indicador pode exigir workflow quando impactar indicador executivo. |
| REG-ANL-078 | Cache de indicador deve considerar tenant, indicador, periodo, parametros, versao e usuario quando houver escopo. |
| REG-ANL-079 | Indicador desativado nao deve aparecer em novos paineis, mas historico deve continuar consultavel quando autorizado. |

### 6.11 Mobilidade e offline

| Codigo | Regra |
|---|---|
| REG-ANL-080 | API movel deve fornecer paineis e indicadores compatíveis com tela reduzida. |
| REG-ANL-081 | Usuario movel deve receber apenas widgets autorizados para seu perfil e tenant. |
| REG-ANL-082 | Fila offline deve registrar acao, payload, status, tentativas, erro e data de sincronizacao. |
| REG-ANL-083 | Dados offline devem possuir validade para evitar decisao com indicador obsoleto. |
| REG-ANL-084 | Sincronizacao deve resolver conflito por versao, data e origem da alteracao. |
| REG-ANL-085 | APIs moveis devem registrar auditoria de consulta quando o dado for sensivel. |

### 6.12 Exportacao, auditoria e eventos

| Codigo | Regra |
|---|---|
| REG-ANL-086 | Toda exportacao deve registrar usuario, tenant, relatorio, parametros, formato, quantidade de registros e data/hora. |
| REG-ANL-087 | Exportacao com dados pessoais deve registrar base legal ou justificativa operacional quando exigido. |
| REG-ANL-088 | Exportacao grande deve ser executada em segundo plano. |
| REG-ANL-089 | Historico de alteracao deve guardar antes/depois de definicoes de KPI, painel, widget, layout e permissao. |
| REG-ANL-090 | Eventos minimos: analytics.relatorio.executado, analytics.exportacao.solicitada, analytics.exportacao.concluida, analytics.kpi.publicado e analytics.estatistica.registrada. |

## 7. Estados

| Entidade | Estados |
|---|---|
| Indicador | Rascunho; EmAnalise; Aprovado; Ativo; Inativo; Encerrado |
| Painel | Rascunho; Publicado; Inativo |
| Widget | Rascunho; Ativo; Inativo |
| Relatorio | EmValidacao; Ativo; Inativo; Descontinuado |
| Exportacao | Solicitada; EmProcessamento; Concluida; Falha; Expirada |
| Fila offline | Pendente; Sincronizando; Sincronizada; Falha; Descartada |
| Candidato sem fonte | PendenteFonte; Rejeitado; AprovadoParaConstrucao |

## 8. Fluxos funcionais

### 8.1 Execucao de relatorio

1. Usuario acessa o hub de analytics.
2. O Epros valida modulo habilitado, permissao geral e permissao do relatorio.
3. Usuario seleciona secao e relatorio.
4. Usuario informa filtros.
5. O Epros valida escopo, filtros e permissao sobre dimensoes.
6. O Epros consulta cache ou executa consulta.
7. O Epros apresenta tabela/grafico, totais, paginacao e opcoes autorizadas.
8. O Epros registra execucao no historico.

### 8.2 Exportacao

1. Usuario solicita exportacao.
2. O Epros valida permissao de exportacao e regra de dados sensiveis.
3. O Epros registra solicitacao.
4. Exportacao pequena e entregue imediatamente; exportacao grande entra em processamento.
5. O Epros grava resultado em arquivo controlado quando aplicavel.
6. O Epros registra conclusao ou falha.

### 8.3 Publicacao de KPI

1. Analista cria indicador em Rascunho.
2. Analista informa formula, fonte, dimensoes, unidade, granularidade e dono.
3. O Epros valida fonte, parametros e escopo tenant.
4. Analista submete para analise.
5. Aprovador aprova ou rejeita.
6. Indicador aprovado pode ser ativado e usado em paineis.

### 8.4 Estatistica de acesso/download

1. Usuario ou visitante acessa recurso monitorado.
2. O Epros coleta metadados permitidos.
3. Se contagem unica estiver ativa, o Epros verifica duplicidade por IP, recurso e dia.
4. O Epros grava evento estatistico.
5. O Epros atualiza contador agregado.
6. Graficos passam a refletir o novo evento conforme cache.

### 8.5 Sincronizacao movel

1. Aplicativo solicita paineis autorizados.
2. O Epros retorna widgets compactos e validade dos dados.
3. Acoes offline entram na fila local.
4. Na sincronizacao, o Epros processa fila, valida versao e registra resultado.
5. Falhas permanecem com motivo e tentativas.

## 9. Telas e experiencia operacional

| ID | Tela | Funcao |
|---|---|---|
| TEL-ANL-001 | Hub de analytics | Selecionar secoes, acessar favoritos, paineis e relatorios. |
| TEL-ANL-002 | Painel executivo | Widgets e KPIs configurados por tenant/perfil/usuario. |
| TEL-ANL-003 | Catalogo de indicadores | Criar, versionar, aprovar, ativar e desativar KPIs. |
| TEL-ANL-004 | Catalogo de relatorios | Listar relatorios, status, fontes, parametros e permissao. |
| TEL-ANL-005 | Relatorio tabular | Filtros, tabela, totais, paginacao e exportacao. |
| TEL-ANL-006 | Relatorio grafico | Series, barras, pizza, linhas e comparativos por periodo. |
| TEL-ANL-007 | Estatisticas de recurso | Graficos de acesso, download, pais, origem, navegador e sistema operacional. |
| TEL-ANL-008 | Exportacoes | Solicitar, acompanhar, baixar e expirar exportacoes. |
| TEL-ANL-009 | Mobilidade | Lista compacta de paineis, indicadores e fila offline. |
| TEL-ANL-010 | Auditoria | Consultar execucoes, exportacoes, acessos sensiveis e alteracoes. |

## 10. APIs funcionais

**Base:** `/api/v1/analytics`

| Metodo | Rota | Funcao |
|---|---|---|
| GET | `/hub` | Retorna secoes e relatorios autorizados. |
| GET | `/dashboards` | Lista paineis autorizados. |
| POST | `/dashboards` | Cria painel. |
| PUT | `/dashboards/{id}` | Atualiza painel. |
| POST | `/dashboards/{id}/publicar` | Publica painel. |
| GET | `/dashboards/{id}/widgets` | Lista widgets do painel. |
| POST | `/dashboards/{id}/widgets` | Cria widget. |
| PUT | `/widgets/{id}` | Atualiza widget. |
| DELETE | `/widgets/{id}` | Inativa widget. |
| GET | `/kpis` | Lista indicadores. |
| POST | `/kpis` | Cria indicador. |
| PUT | `/kpis/{id}` | Atualiza indicador. |
| POST | `/kpis/{id}/submeter` | Submete indicador para analise. |
| POST | `/kpis/{id}/aprovar` | Aprova indicador. |
| POST | `/kpis/{id}/inativar` | Inativa indicador. |
| GET | `/kpis/{id}/serie` | Retorna serie de indicador por filtros. |
| GET | `/reports` | Lista relatorios autorizados. |
| POST | `/reports/{id}/executar` | Executa relatorio. |
| POST | `/reports/{id}/exportar` | Solicita exportacao. |
| GET | `/exports/{id}` | Consulta status de exportacao. |
| GET | `/exports/{id}/download` | Baixa exportacao autorizada. |
| POST | `/stats/track` | Registra estatistica de acesso/download. |
| GET | `/stats/recurso/{id}` | Consulta estatisticas de recurso. |
| GET | `/mobile/dashboards` | Retorna paineis compactos para mobile. |
| POST | `/mobile/offline/sync` | Sincroniza fila offline. |
| GET | `/audit` | Consulta auditoria analitica. |

## 11. Modelo de dados funcional e implantavel

### 11.1 Visao geral

| Entidade | Papel | Cardinalidade principal |
|---|---|---|
| analytics_kpi | Catalogo de indicadores | Tenant 1:N |
| analytics_kpi_versao | Formula versionada | KPI 1:N |
| analytics_dashboard | Painel | Tenant 1:N |
| analytics_widget | Widget de painel | Dashboard 1:N |
| analytics_report | Catalogo de relatorios | Tenant/Global 1:N |
| analytics_report_parametro | Parametros do relatorio | Relatorio 1:N |
| analytics_report_execucao | Historico de execucao | Relatorio 1:N |
| analytics_exportacao | Exportacoes solicitadas | Execucao 1:N |
| analytics_metric_cache | Cache de resultado | KPI/Relatorio 1:N |
| analytics_stats_evento | Evento estatistico | Recurso 1:N |
| analytics_stats_agregado | Contador agregado | Recurso/periodo 1:N |
| analytics_layout_operacional | Catalogo de layout operacional | Dominio 1:N |
| analytics_layout_parametro | Parametros de layout | Layout 1:N |
| analytics_permissao_relatorio | Permissao e escopo | Relatorio 1:N |
| analytics_mobile_sessao | Sessao/consulta movel | Usuario 1:N |
| analytics_mobile_fila_offline | Fila offline | Sessao 1:N |
| analytics_auditoria | Auditoria analitica | Tenant 1:N |

### 11.2 Constraints e indices minimos

| Entidade | Constraint/indice |
|---|---|
| analytics_kpi | Unico por TenantId + Codigo; indice por Status e DonoFuncional. |
| analytics_kpi_versao | Unico por KpiId + Versao. |
| analytics_dashboard | Indice por TenantId, Escopo, PerfilId, UsuarioId e Status. |
| analytics_widget | Indice por DashboardId, TipoWidget, KpiId e ReportId. |
| analytics_report | Unico por Codigo quando global; indice por Dominio, Status e TenantId. |
| analytics_report_execucao | Indice por TenantId, ReportId, UsuarioId, DataExecucao. |
| analytics_exportacao | Indice por TenantId, Status, UsuarioId, DataSolicitacao. |
| analytics_metric_cache | Unico por ChaveCache; indice por ExpiraEm. |
| analytics_stats_evento | Indice por TenantId, RecursoId, DataEvento, IpHash. |
| analytics_stats_agregado | Unico por TenantId + RecursoId + Granularidade + PeriodoInicio + Dimensao. |
| analytics_auditoria | Indice por TenantId, Entidade, EntidadeId, DataEvento. |

## 12. Dicionario de dados implantavel

### 12.1 analytics_kpi

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| Id | uuid | uuid | Sim | PK | Gerado pelo Epros. |
| TenantId | texto | 200 | Condicional | Indice | Nulo apenas para KPI global. |
| Codigo | texto | Nao informado no material | Sim | Unico por tenant | Codigo funcional do indicador. |
| Nome | texto | Nao informado no material | Sim |  | Nome exibido. |
| Descricao | texto | Nao informado no material | Nao |  |  |
| Dominio | enum/texto | Financeiro/Vendas/Projetos/Clientes/Horas/Estatisticas/Outro | Sim | Indice | Familia de negocio. |
| Unidade | texto | Nao informado no material | Nao |  | Moeda, quantidade, percentual, horas. |
| Granularidade | enum | Dia/Semana/Mes/Ano/TempoReal | Sim |  |  |
| DonoFuncional | texto | Nao informado no material | Sim |  | Area responsavel. |
| Status | enum | Rascunho/EmAnalise/Aprovado/Ativo/Inativo/Encerrado | Sim | Indice | Ciclo de vida. |
| PermiteMobile | booleano | true/false | Sim |  | Disponivel no canal movel. |
| Sensivel | booleano | true/false | Sim |  | Exige controle adicional. |
| CriadoPorUsuarioId | uuid | uuid | Sim | FK usuario | Auditoria. |
| DataCriacao | data/hora | ISO 8601 | Sim |  | Auditoria. |
| AlteradoPorUsuarioId | uuid | uuid | Nao | FK usuario | Auditoria. |
| DataAlteracao | data/hora | ISO 8601 | Nao |  | Auditoria. |

### 12.2 analytics_kpi_versao

| Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|
| Id | uuid | uuid | Sim | PK |  |
| KpiId | uuid | uuid | Sim | FK analytics_kpi |  |
| Versao | inteiro | Nao informado no material | Sim | Unico por KPI |  |
| Formula | texto/json | Nao informado no material | Sim |  | Formula versionada. |
| FonteDados | texto/json | Nao informado no material | Sim |  | Modulo/tabela/visao/API fonte. |
| Dimensoes | json | Nao informado no material | Nao |  | Cliente, projeto, categoria, status etc. |
| FiltrosPadrao | json | Nao informado no material | Nao |  | Presets e filtros. |
| VigenciaInicio | data | ISO 8601 | Sim |  |  |
| VigenciaFim | data | ISO 8601 | Nao |  |  |
| Justificativa | texto | Nao informado no material | Nao |  | Obrigatoria em alteracao sensivel. |
| AprovadoPorUsuarioId | uuid | uuid | Nao | FK usuario |  |
| DataAprovacao | data/hora | ISO 8601 | Nao |  |  |

### 12.3 analytics_dashboard e analytics_widget

| Entidade | Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|---|
| analytics_dashboard | Id | uuid | uuid | Sim | PK |  |
| analytics_dashboard | TenantId | texto | 200 | Sim | Indice |  |
| analytics_dashboard | Nome | texto | Nao informado no material | Sim |  |  |
| analytics_dashboard | Escopo | enum | Tenant/Perfil/Usuario | Sim |  |  |
| analytics_dashboard | PerfilId | uuid | uuid | Condicional | FK perfil | Obrigatorio quando escopo Perfil. |
| analytics_dashboard | UsuarioId | uuid | uuid | Condicional | FK usuario | Obrigatorio quando escopo Usuario. |
| analytics_dashboard | Status | enum | Rascunho/Publicado/Inativo | Sim |  |  |
| analytics_dashboard | Ordem | inteiro | Nao informado no material | Nao |  | Ordenacao. |
| analytics_widget | Id | uuid | uuid | Sim | PK |  |
| analytics_widget | DashboardId | uuid | uuid | Sim | FK dashboard |  |
| analytics_widget | TipoWidget | enum | Kpi/Grafico/Tabela/Atalho/Estatistica | Sim |  |  |
| analytics_widget | KpiId | uuid | uuid | Nao | FK kpi | Obrigatorio para widget KPI. |
| analytics_widget | ReportId | uuid | uuid | Nao | FK report | Obrigatorio para widget relatorio. |
| analytics_widget | Titulo | texto | Nao informado no material | Sim |  |  |
| analytics_widget | ConfiguracaoVisual | json | Nao informado no material | Sim |  | Tamanho, grafico, cores, colunas. |
| analytics_widget | Filtros | json | Nao informado no material | Nao |  |  |
| analytics_widget | PosicaoX | inteiro | Nao informado no material | Sim |  | Grid. |
| analytics_widget | PosicaoY | inteiro | Nao informado no material | Sim |  | Grid. |
| analytics_widget | Largura | inteiro | Nao informado no material | Sim |  | Grid. |
| analytics_widget | Altura | inteiro | Nao informado no material | Sim |  | Grid. |
| analytics_widget | Status | enum | Rascunho/Ativo/Inativo | Sim |  |  |

### 12.4 analytics_report e parametros

| Entidade | Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|---|
| analytics_report | Id | uuid | uuid | Sim | PK |  |
| analytics_report | TenantId | texto | 200 | Condicional | Indice | Nulo para relatorio global. |
| analytics_report | Codigo | texto | Nao informado no material | Sim | Unico |  |
| analytics_report | Nome | texto | Nao informado no material | Sim |  |  |
| analytics_report | Dominio | enum/texto | Faturamento/Estimativas/Projetos/Clientes/Horas/Financeiro/Despesas/Propostas/Estatisticas | Sim | Indice |  |
| analytics_report | TipoSaida | enum | Tabela/Grafico/Misto/LayoutOperacional | Sim |  |  |
| analytics_report | FonteDados | texto/json | Nao informado no material | Sim |  | Deve apontar fonte funcional. |
| analytics_report | Status | enum | EmValidacao/Ativo/Inativo/Descontinuado | Sim | Indice |  |
| analytics_report | PermiteExportar | booleano | true/false | Sim |  |  |
| analytics_report | PermiteMobile | booleano | true/false | Sim |  |  |
| analytics_report | Sensivel | booleano | true/false | Sim |  |  |
| analytics_report | PageLimitPadrao | inteiro | default 25 | Sim |  | REG-ANL-025. |
| analytics_report_parametro | Id | uuid | uuid | Sim | PK |  |
| analytics_report_parametro | ReportId | uuid | uuid | Sim | FK report |  |
| analytics_report_parametro | NomeParametro | texto | Nao informado no material | Sim |  |  |
| analytics_report_parametro | TipoParametro | enum | Data/Periodo/Ano/Cliente/Projeto/Categoria/Status/Usuario/Texto/Numero | Sim |  |  |
| analytics_report_parametro | Obrigatorio | booleano | true/false | Sim |  |  |
| analytics_report_parametro | ValorPadrao | texto/json | Nao informado no material | Nao |  |  |
| analytics_report_parametro | Ordem | inteiro | Nao informado no material | Sim |  |  |

### 12.5 execucao, exportacao e cache

| Entidade | Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|---|
| analytics_report_execucao | Id | uuid | uuid | Sim | PK |  |
| analytics_report_execucao | TenantId | texto | 200 | Sim | Indice |  |
| analytics_report_execucao | ReportId | uuid | uuid | Sim | FK report |  |
| analytics_report_execucao | UsuarioId | uuid | uuid | Sim | FK usuario |  |
| analytics_report_execucao | ParametrosJson | json | Nao informado no material | Sim |  | Filtros aplicados. |
| analytics_report_execucao | DataExecucao | data/hora | ISO 8601 | Sim | Indice |  |
| analytics_report_execucao | TempoMs | inteiro | Nao informado no material | Nao |  | Performance. |
| analytics_report_execucao | TotalRegistros | inteiro | Nao informado no material | Nao |  |  |
| analytics_report_execucao | Origem | enum | Web/Mobile/API/Agendada | Sim |  |  |
| analytics_exportacao | Id | uuid | uuid | Sim | PK |  |
| analytics_exportacao | ExecucaoId | uuid | uuid | Sim | FK execucao |  |
| analytics_exportacao | TenantId | texto | 200 | Sim | Indice |  |
| analytics_exportacao | UsuarioId | uuid | uuid | Sim | FK usuario |  |
| analytics_exportacao | Formato | enum | CSV/XLSX/PDF/JSON | Sim |  |  |
| analytics_exportacao | Status | enum | Solicitada/EmProcessamento/Concluida/Falha/Expirada | Sim |  |  |
| analytics_exportacao | ArquivoId | uuid | uuid | Nao | FK GED | Quando armazenada como arquivo. |
| analytics_exportacao | Justificativa | texto | Nao informado no material | Condicional |  | Exigida para dados sensiveis quando configurado. |
| analytics_exportacao | DataSolicitacao | data/hora | ISO 8601 | Sim |  |  |
| analytics_exportacao | DataConclusao | data/hora | ISO 8601 | Nao |  |  |
| analytics_metric_cache | Id | uuid | uuid | Sim | PK |  |
| analytics_metric_cache | TenantId | texto | 200 | Sim | Indice |  |
| analytics_metric_cache | ChaveCache | texto/hash | Nao informado no material | Sim | Unico |  |
| analytics_metric_cache | TipoOrigem | enum | KPI/Relatorio/Estatistica | Sim |  |  |
| analytics_metric_cache | OrigemId | uuid | uuid | Sim |  | Id do KPI/relatorio/recurso. |
| analytics_metric_cache | ParametrosHash | texto/hash | Nao informado no material | Sim |  |  |
| analytics_metric_cache | ResultadoJson | json | Nao informado no material | Sim |  |  |
| analytics_metric_cache | GeradoEm | data/hora | ISO 8601 | Sim |  |  |
| analytics_metric_cache | ExpiraEm | data/hora | ISO 8601 | Sim | Indice |  |

### 12.6 estatisticas

| Entidade | Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|---|
| analytics_stats_evento | Id | uuid | uuid | Sim | PK |  |
| analytics_stats_evento | TenantId | texto | 200 | Condicional | Indice | Nulo quando recurso publico permitir. |
| analytics_stats_evento | DataEvento | data/hora | ISO 8601 | Sim | Indice |  |
| analytics_stats_evento | RecursoId | uuid/texto | Nao informado no material | Sim | Indice | Arquivo, relatorio, download ou pagina. |
| analytics_stats_evento | TipoRecurso | enum/texto | Arquivo/Relatorio/Pagina/Download/Outro | Sim |  |  |
| analytics_stats_evento | Referer | texto/url | Nao informado no material | Nao |  |  |
| analytics_stats_evento | RefererIsLocal | booleano | true/false | Nao |  |  |
| analytics_stats_evento | Country | texto | Nao informado no material | Nao |  | Pais inferido. |
| analytics_stats_evento | BrowserFamily | texto | Nao informado no material | Nao |  |  |
| analytics_stats_evento | SistemaOperacional | texto | Nao informado no material | Nao |  |  |
| analytics_stats_evento | IpHash | texto/hash | Nao informado no material | Nao | Indice | IP deve ser protegido. |
| analytics_stats_evento | UserAgent | texto | Nao informado no material | Nao |  |  |
| analytics_stats_evento | BaseUrl | texto/url | Nao informado no material | Nao |  |  |
| analytics_stats_evento | UsuarioId | uuid | uuid | Nao | FK usuario |  |
| analytics_stats_evento | IsDownloadManager | booleano | true/false | Nao |  |  |
| analytics_stats_agregado | Id | uuid | uuid | Sim | PK |  |
| analytics_stats_agregado | TenantId | texto | 200 | Condicional | Indice |  |
| analytics_stats_agregado | RecursoId | uuid/texto | Nao informado no material | Sim | Indice |  |
| analytics_stats_agregado | Granularidade | enum | Hora/Dia/Semana/Mes/Ano | Sim |  |  |
| analytics_stats_agregado | PeriodoInicio | data/hora | ISO 8601 | Sim | Indice |  |
| analytics_stats_agregado | PeriodoFim | data/hora | ISO 8601 | Sim |  |  |
| analytics_stats_agregado | Dimensao | enum/texto | Pais/Origem/Navegador/SistemaOperacional/Total | Sim |  |  |
| analytics_stats_agregado | ValorDimensao | texto | Nao informado no material | Nao |  |  |
| analytics_stats_agregado | Quantidade | inteiro | Nao informado no material | Sim |  |  |

### 12.7 layout operacional, permissao, mobile e auditoria

| Entidade | Campo | Tipo/formato | Tamanho/dominio | Obrigatorio | Chave/relacao | Regra/observacao |
|---|---|---|---|---|---|---|
| analytics_layout_operacional | Id | uuid | uuid | Sim | PK |  |
| analytics_layout_operacional | Codigo | texto | Nao informado no material | Sim | Unico |  |
| analytics_layout_operacional | Nome | texto | Nao informado no material | Sim |  |  |
| analytics_layout_operacional | Dominio | enum/texto | Boleto/Financeiro/Vendas/Servico/Fiscal/Estoque/Outro | Sim |  | Catalogo de layouts. |
| analytics_layout_operacional | Formato | enum | A4/Termico/Digital/Outro | Sim |  |  |
| analytics_layout_operacional | Status | enum | EmValidacao/Ativo/Inativo/Descontinuado | Sim |  |  |
| analytics_layout_parametro | Id | uuid | uuid | Sim | PK |  |
| analytics_layout_parametro | LayoutId | uuid | uuid | Sim | FK layout |  |
| analytics_layout_parametro | NomeParametro | texto | Nao informado no material | Sim |  |  |
| analytics_layout_parametro | Obrigatorio | booleano | true/false | Sim |  |  |
| analytics_permissao_relatorio | Id | uuid | uuid | Sim | PK |  |
| analytics_permissao_relatorio | ReportId | uuid | uuid | Sim | FK report |  |
| analytics_permissao_relatorio | PerfilId | uuid | uuid | Nao | FK perfil |  |
| analytics_permissao_relatorio | UsuarioId | uuid | uuid | Nao | FK usuario |  |
| analytics_permissao_relatorio | PodeVer | booleano | true/false | Sim |  |  |
| analytics_permissao_relatorio | PodeExportar | booleano | true/false | Sim |  |  |
| analytics_permissao_relatorio | EscopoJson | json | Nao informado no material | Nao |  | Clientes/projetos/equipes permitidos. |
| analytics_mobile_sessao | Id | uuid | uuid | Sim | PK |  |
| analytics_mobile_sessao | TenantId | texto | 200 | Sim | Indice |  |
| analytics_mobile_sessao | UsuarioId | uuid | uuid | Sim | FK usuario |  |
| analytics_mobile_sessao | DispositivoId | texto | Nao informado no material | Sim |  |  |
| analytics_mobile_sessao | UltimaSincronizacao | data/hora | ISO 8601 | Nao |  |  |
| analytics_mobile_fila_offline | Id | uuid | uuid | Sim | PK |  |
| analytics_mobile_fila_offline | SessaoId | uuid | uuid | Sim | FK sessao |  |
| analytics_mobile_fila_offline | Acao | texto | Nao informado no material | Sim |  |  |
| analytics_mobile_fila_offline | PayloadJson | json | Nao informado no material | Sim |  |  |
| analytics_mobile_fila_offline | Status | enum | Pendente/Sincronizando/Sincronizada/Falha/Descartada | Sim |  |  |
| analytics_mobile_fila_offline | Tentativas | inteiro | Nao informado no material | Sim |  |  |
| analytics_mobile_fila_offline | MensagemErro | texto | Nao informado no material | Nao |  |  |
| analytics_mobile_fila_offline | DataSincronizacao | data/hora | ISO 8601 | Nao |  |  |
| analytics_auditoria | Id | uuid | uuid | Sim | PK |  |
| analytics_auditoria | TenantId | texto | 200 | Sim | Indice |  |
| analytics_auditoria | Entidade | texto | Nao informado no material | Sim |  |  |
| analytics_auditoria | EntidadeId | uuid | uuid | Sim | Indice |  |
| analytics_auditoria | Acao | texto | Nao informado no material | Sim |  |  |
| analytics_auditoria | UsuarioId | uuid | uuid | Sim | FK usuario |  |
| analytics_auditoria | DataEvento | data/hora | ISO 8601 | Sim | Indice |  |
| analytics_auditoria | Ip | texto/hash | Nao informado no material | Nao |  | Proteger IP. |
| analytics_auditoria | AntesJson | json | Nao informado no material | Nao |  |  |
| analytics_auditoria | DepoisJson | json | Nao informado no material | Nao |  |  |

## 13. Relatorios e indicadores iniciais

| ID | Nome | Dimensoes |
|---|---|---|
| REL-ANL-001 | Faturamento geral | Periodo, cliente, projeto, categoria, status. |
| REL-ANL-002 | Faturamento mensal | Ano, mes, status. |
| REL-ANL-003 | Faturamento por cliente | Periodo, cliente, categoria. |
| REL-ANL-004 | Estimativas | Periodo, cliente, categoria, status. |
| REL-ANL-005 | Projetos | Cliente, categoria, status, periodo. |
| REL-ANL-006 | Clientes | Cliente, projeto, documentos, despesas e estimativas. |
| REL-ANL-007 | Horas | Equipe, cliente, projeto, faturado/nao faturado. |
| REL-ANL-008 | Resultado mensal | Ano, receita, despesa, resultado. |
| REL-ANL-009 | Estatisticas de recurso | Periodo, pais, origem, navegador, sistema operacional. |
| REL-ANL-010 | Auditoria de exportacoes | Periodo, usuario, relatorio, formato e status. |

## 14. Criterios de aceite

| ID | Criterio |
|---|---|
| CA-ANL-001 | Usuario sem permissao nao acessa hub, API, relatorio nem exportacao. |
| CA-ANL-002 | Usuario ve somente relatorios autorizados e escopo permitido. |
| CA-ANL-003 | Filtro customizado exige data inicial e final. |
| CA-ANL-004 | Relatorio mensal usa ano corrente quando ano nao informado. |
| CA-ANL-005 | Relatorio de faturamento exclui documentos em rascunho. |
| CA-ANL-006 | Saldo em aberto e valor final menos pagamentos. |
| CA-ANL-007 | Page limit padrao e 25. |
| CA-ANL-008 | Exportacao registra usuario, parametros, formato e data. |
| CA-ANL-009 | Exportacao com dados sensiveis exige permissao adicional. |
| CA-ANL-010 | Estatistica unica nao duplica IP/recurso/dia quando configurada. |
| CA-ANL-011 | Graficos de 24h, 7d, 30d e 12m retornam serie valida. |
| CA-ANL-012 | KPI publicado possui formula, fonte, versao e dono funcional. |
| CA-ANL-013 | Widget desativado nao aparece no painel. |
| CA-ANL-014 | API movel retorna apenas paineis autorizados. |
| CA-ANL-015 | Fila offline registra falhas e tentativas. |
| CA-ANL-016 | Relatorio sem fonte funcional confirmada fica inativo ou em validacao. |
| CA-ANL-017 | Cache expira conforme configuracao e nao vaza dados entre tenants. |
| CA-ANL-018 | Auditoria registra alteracao de KPI, painel, widget e exportacao. |

## 15. Notas de rodape

[^1]: As entidades de KPI, dashboard, widget, cache, exportacao, permissao e mobile foram estruturadas a partir dos requisitos e lacunas funcionais do material, porque o levantamento informa relatorios, KPIs, filtros, estatisticas e mobilidade, mas nao apresenta modelo transacional final para essas capacidades.
[^2]: Relatorios sem fonte funcional confirmada foram mantidos como capacidade pendente na MC, sem criar regra de negocio transacional nova.

