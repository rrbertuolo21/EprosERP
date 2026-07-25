# MC 3 Plataforma Compartilhada — Analytics e Mobilidade V1

## 1. Identificacao

| Item | Valor |
|---|---|
| Sistema | Epros |
| Empresa | Siser |
| Modulo | Plataforma Compartilhada |
| Submodulo | Analytics e Mobilidade |
| Versao | V1 |
| Data | 2026-06-06 |

## 2. Matriz de completude

| Area | Status | Evidencia funcional consolidada | Lacuna | Acao recomendada | Prioridade | Dono sugerido |
|---|---|---|---|---|---|---|
| Hub de analytics | Parcial | Hub, secoes, filtros, relatorios e permissao geral. | Contrato visual definitivo nao informado. | Validar experiencia web/mobile. | P1 | Plataforma |
| Permissao de relatorios | Parcial | Modulo habilitado, perfil com acesso e bloqueio de usuarios sem escopo. | Permissao parcial por metodo no material precisa saneamento. | Aplicar autorizacao em tela e API. | P0 | Plataforma/Seguranca |
| Relatorios de faturamento | Parcial | Visao geral, mes, cliente, categoria, pagamentos, saldo e totais. | Fonte final de dados por modulo precisa contrato. | Conectar com financeiro/vendas. | P0 | Financeiro/Vendas |
| Relatorios de estimativas | Parcial | Visao geral, mes, cliente e categoria. | Status e fonte final dependem do modulo comercial. | Validar com Vendas. | P1 | Vendas |
| Relatorios de projetos | Parcial | Status, tarefas, horas, despesas, documentos e pagamentos. | Categorias e projeto detalhado precisam fonte final. | Conectar com Projetos. | P1 | Projetos |
| Relatorios de clientes | Parcial | Projetos, documentos, estimativas e despesas por cliente. | Escopo de cliente externo nao definido. | Definir visao interna/externa. | P1 | Cadastros/Vendas |
| Relatorios de horas | Parcial | Equipe, cliente, projeto, faturado e nao faturado. | Fonte de apontamento final nao confirmada. | Integrar com RH/Projetos. | P1 | RH/Projetos |
| Resultado mensal | Parcial | Receita, despesa, 12 meses e resultado. | Formula de acumulado precisa validacao financeira. | Definir formula oficial. | P0 | Financeiro |
| Estatisticas de acesso/download | Parcial | Evento com data, recurso, pais, navegador, sistema, origem, usuario e IP protegido. | Retencao de eventos detalhados nao definida. | Definir politica de retencao. | P0 | Plataforma/Compliance |
| Graficos estatisticos | Parcial | 24h, 7d, 30d, 12m, pais, origem, navegador e sistema operacional. | Paleta, acessibilidade e padrao visual nao informados. | Validar design system. | P2 | Plataforma |
| Catalogo de layouts operacionais | Parcial | 132 layouts catalogados por dominio e visualizador generico. | Conteudo interno dos layouts nao detalhado campo a campo. | Validar layouts por dominio no modulo dono. | P2 | Relatorios |
| KPI e formula versionada | Incompleto | Requisito de catalogo KPI, formula e cache. | Nao ha modelo final implementado no material. | Construir catalogo KPI. | P0 | Plataforma/BI |
| Dashboard e widgets | Incompleto | Requisito de widgets configuraveis por tenant/perfil. | Posicionamento e personalizacao nao detalhados. | Definir grid e permissoes. | P1 | Plataforma |
| Cache de metricas | Incompleto | Necessidade de cache por serie e parametros. | TTL e invalidacao por evento nao definidos. | Definir estrategia de cache. | P0 | Plataforma |
| Exportacao auditada | Parcial | Exportacao com auditoria e controle de dados pessoais. | Formatos e limites de volume nao definidos. | Definir formatos e execucao assíncrona. | P0 | Plataforma |
| Mobilidade | Parcial | API movel, dados compactos e fila offline. | Escopo de acoes offline nao detalhado. | Definir contratos mobile. | P1 | Mobile/Plataforma |
| Relatorios sem fonte confirmada | Incompleto | Despesas por cliente/projeto e propostas aparecem como referencias. | Fonte funcional ausente. | Manter desativado ate fonte confirmada. | P1 | Plataforma |
| Testes automatizados | Parcial | Cenarios para permissao, filtros, status, exportacao e estatistica. | Suite completa nao comprovada. | Criar testes por CA. | P0 | QA |

## 3. Pendencias criticas P0

1. Garantir permissao em toda API de relatorio, exportacao, estatistica e mobile.
2. Definir formula oficial de resultado mensal e acumulado.
3. Conectar fontes finais para faturamento, pagamentos, despesas, projetos e horas.
4. Definir catalogo de KPIs com formula versionada.
5. Definir estrategia de cache e invalidacao por tenant/usuario/filtro.
6. Definir retencao de eventos estatisticos e protecao de IP.
7. Definir limites e processamento assincrono de exportacoes.
8. Cobrir os criterios de aceite com testes automatizados.

## 4. Perguntas para validacao humana

| Pergunta | Impacto |
|---|---|
| Quais relatorios entram no MVP: faturamento, estimativas, projetos, clientes, horas, financeiro, estatisticas ou todos? | Define escopo de construcao. |
| Resultado acumulado financeiro deve ser acumulado real ou resultado mensal independente? | Define formula oficial. |
| Usuarios externos podem acessar algum relatorio? | Define permissao e escopo. |
| Estatisticas detalhadas devem ser retidas por quanto tempo? | Define LGPD, custo e performance. |
| Exportacoes devem permitir PDF, CSV, XLSX e JSON no MVP? | Define formatos e bibliotecas. |
| A fila offline sera apenas consulta ou tambem acao operacional? | Define modelo mobile. |
| Layouts operacionais entram aqui ou ficam no modulo Relatorios? | Define fronteira entre analytics e relatorios operacionais. |

## 5. Itens de construcao

| Item | Entrega esperada | Prioridade |
|---|---|---|
| Autorizacao analitica | Permissao por hub, relatorio, exportacao, KPI e escopo. | P0 |
| Catalogo KPI | Indicador, versao, formula, fonte, dimensoes e dono. | P0 |
| Execucao de relatorios | Parametros, historico, paginacao, totais e cache. | P0 |
| Exportacao auditada | Solicitacao, processamento, arquivo e auditoria. | P0 |
| Estatisticas | Evento, agregado, graficos e retencao. | P0 |
| Cache | Chave, TTL, invalidacao e isolamento tenant. | P0 |
| Dashboards | Painel, widgets, layout e publicacao. | P1 |
| Mobilidade | APIs compactas e fila offline. | P1 |
| Layouts operacionais | Catalogo e validacao por dominio. | P2 |

## 6. Criterios de aceite de completude

| ID | Criterio |
|---|---|
| MC-ANL-001 | EF possui modelo de dados antes do dicionario. |
| MC-ANL-002 | Todos os campos do dicionario possuem tipo, tamanho/dominio, obrigatoriedade, relacao e regra/observacao. |
| MC-ANL-003 | Campos sem tamanho conhecido estao marcados como Nao informado no material. |
| MC-ANL-004 | Nenhum relatorio sem fonte confirmada e tratado como pronto. |
| MC-ANL-005 | Permissao e tenant sao obrigatorios nas APIs. |
| MC-ANL-006 | Exportacao e estatistica possuem auditoria e retencao pendente/definida. |

