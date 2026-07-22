# MC 0_APLICATIVO DASHBOARD_E_LAYOUT V1

**Projeto:** Epros  
**Empresa:** Siser  
**Modulo:** APLICATIVO  
**Submodulo:** DASHBOARD_E_LAYOUT  
**ID funcional:** APP-TEN-011  
**Versao:** V1  
**Status:** Pronto para validacao humana  
**Data:** 2026-06-06

## 1. Objetivo

Esta matriz mede a completude funcional do submodulo Dashboard e Layout do Epros, separando capacidades suficientemente especificadas de lacunas, conflitos e decisoes necessarias antes de construcao e implantacao.

## 2. Legenda de status

| Status | Significado |
|---|---|
| Coberto | Capacidade possui regra, fluxo, tela, entidade ou contrato suficiente para construcao inicial. |
| Parcial | Capacidade existe, mas precisa decisao, complemento de dados ou validacao. |
| Lacuna | Capacidade citada ou esperada sem especificacao suficiente. |
| Decisao | Exige validacao humana antes de construcao. |

## 3. Matriz de completude

| Capacidade | Status | Evidencia funcional consolidada | Lacuna / risco | Acao recomendada | Prioridade | Dependencias |
|---|---|---|---|---|---|---|
| Separacao area publica e layout autenticado | Coberto | Material descreve experiencia anonima e autenticada. | Conteudo publico dinamico depende de catalogos/configuracao. | Validar governanca de conteudo publico. | P1 | CATALOGOS_GLOBAIS_SAAS; CONFIGURACAO |
| Validacao de licenca no layout | Parcial | Licenca expirada redireciona e informa periodo expirado. | Encerramento de sessao nao esta fechado. | Decidir se bloqueio tambem encerra sessao. | P0 | ASSINATURA_E_PLANOS |
| Painel Siser | Parcial | Widgets de usuarios, usuarios ativos, pedidos, valor de pedidos e planos. | Indicador Total Order possui conflito de fonte. | Corrigir fonte funcional de pedidos. | P0 | ASSINATURA_E_PLANOS; USUARIOS_E_PAPEIS |
| Painel operacional | Parcial | Receber, pagar, receita, despesa e transacoes recentes. | Conta funcional de despesa conflita no material. | Validar conta correta e regra contabil. | P0 | FINANCEIRO |
| Transacoes recentes | Coberto | Seis grupos: vendas, devolucoes de venda, compras, devolucoes de compra, pagamentos e recebimentos. | Campos exibidos por linha nao estao completos. | Definir colunas finais por aba. | P1 | VENDAS; COMPRAS; FINANCEIRO |
| Periodo inicial de filtros | Parcial | Material informa carga do dia anterior ate data atual com status todos. | Fuso, hora inicial/final e comportamento multiempresa precisam decisao. | Parametrizar periodo inicial. | P1 | CONFIGURACAO |
| Atalhos operacionais | Coberto | 12 cards de contagem identificados. | Regra final de visibilidade por menu/permissao precisa matriz detalhada. | Integrar com permissao de menu. | P0 | PERMISSOES_DE_MENU |
| Dashboards de vendas | Coberto | Totais, graficos, forma de pagamento, produtos, clientes, vendedores e relatorios. | Limites de ranking e status precisam padronizacao. | Definir parametros e mapa de status. | P1 | VENDAS |
| Dashboards de compras | Coberto | Totais, graficos, produtos, fornecedores e transportadoras. | Campos de detalhe e limites de ranking nao completos. | Fechar contrato de detalhe. | P1 | COMPRAS |
| Dashboards financeiros | Parcial | Aberto, baixado, inadimplencia, categoria e detalhe paginado. | Contagem de detalhe pode ser feita apos paginacao; estados precisam unificacao. | Corrigir regra de total filtrado e padronizar estados. | P0 | FINANCEIRO |
| Dashboard fiscal | Parcial | Indicadores por periodo, canceladas e modelo. | Filtro por ano e tipo pode gerar total incoerente. | Definir regra de periodo fiscal e filtros obrigatorios. | P0 | FATURAMENTO_FISCAL_ELETRONICO |
| Dashboard de estoque | Parcial | Entrada, saida, custo, preco e estoque atual. | Conversao/unidade fixa e campos de custo precisam validacao. | Definir unidade e formula oficial de custo/saldo. | P0 | ESTOQUE |
| Dashboard vendedor/meta | Parcial | Vendedor, mes, meta, vendas, falta e comissao. | Comissao sem regra funcional. | Definir formula ou retirar comissao do indicador. | P0 | VENDAS |
| Dashboard de servicos | Coberto | Servicos vendidos com preco, quantidade, desconto, acrescimo e total. | Fonte final do dominio de servicos precisa confirmacao. | Vincular a Gestao de Servicos. | P1 | VENDAS / gestao de servicos |
| Dashboard de cartoes | Parcial | Lista de cartoes identificada. | Mascaramento, permissao e campos exibidos nao informados. | Definir politica de dado sensivel. | P0 | FINANCEIRO; COMPLIANCE_LGPD_SOX_IFRS |
| Relatorios PDF de dashboard | Parcial | Detalhe de pagamento, venda por item/cliente e venda por cliente. | Auditoria, permissao de exportacao e layout final nao informados. | Criar contrato de relatorio. | P1 | RELATORIOS |
| Home por perfil | Parcial | Admin, equipe, cliente e afiliado aparecem como experiencias. | Precedencia entre papeis e rotas precisa decisao. | Definir matriz papel x homepage. | P0 | USUARIOS_E_PAPEIS |
| Busca global | Parcial | Busca global, busca rapida, itens recentes/favoritos e detalhes adicionais. | Campos pesquisaveis e seguranca por modulo nao completos. | Definir indice funcional de busca por modulo. | P1 | API_GATEWAY_E_OPENAPI; PERMISSOES_DE_MENU |
| Pesquisa salva | Lacuna | Capacidade de filtros salvos identificada. | Persistencia em payload opaco nao e suficiente para padrao implantavel. | Modelar estrutura tipada e auditavel. | P0 | CONFIGURACAO; COMPLIANCE_LGPD_SOX_IFRS |
| Feed e atividades | Parcial | Feed, atividades, eventos e blocos de home identificados. | Eventos/listeners e escopo de dados nao estao fechados. | Definir eventos oficiais e seguranca. | P1 | SOA_COLABORACAO; WORKFLOW |
| Conectores externos | Lacuna | Conectores e conteudo externo aparecem no material. | Nao ha contrato de seguranca, privacidade, autorizacao e auditoria. | Especificar em integracoes antes de liberar. | P0 | INTEGRACOES_E_CONECTORES; COMPLIANCE_LGPD_SOX_IFRS |
| Dashboard modular | Parcial | Resolucao por modulo ativo, menu, permissao, rota e fallback. | Precedencia e fallback final precisam validacao. | Fechar contrato com catalogo modular e permissao. | P0 | CATALOGOS_GLOBAIS_SAAS; PERMISSOES_DE_MENU |
| Modelo de dados funcional | Parcial | EF contem entidades, contratos, relacionamentos, constraints e diagrama. | Chaves/cardinalidades de home, feed, pesquisa e conectores nao informadas. | Completar com arquitetura de dados. | P0 | API_GATEWAY_E_OPENAPI |
| Dicionario de dados | Parcial | Campos informados preservados, inclusive indicadores e contratos. | Muitos tipos, tamanhos e obrigatoriedades ausentes. | Validar dicionario com time de dados. | P0 | Dados/Arquitetura |
| Testes automatizados | Lacuna | Material informa ausencia de testes automatizados identificados. | Alto risco de regressao em indicadores. | Criar suite minima de testes. | P0 | QA |
| Homologacao manual | Parcial | Material recomenda replay manual. | Roteiro e massa de dados nao informados. | Criar roteiro por dashboard e perfil. | P1 | Implantacao |

## 4. Itens criticos para validacao humana

1. Definir se licenca expirada encerra sessao ou apenas bloqueia navegacao operacional.
2. Corrigir fonte do indicador Total Order.
3. Confirmar conta funcional correta para despesas no grafico operacional.
4. Unificar status de vendas, documentos, fiscal e PDV.
5. Definir periodo inicial padrao com fuso, hora inicial/final e regra multiempresa.
6. Parametrizar limites de rankings.
7. Definir formula de comissao em meta de vendedor ou retirar o campo.
8. Definir unidade/conversao oficial do dashboard de estoque.
9. Fechar politica de mascaramento e permissao para cartoes e dados financeiros.
10. Modelar pesquisa salva de forma tipada e auditavel.
11. Definir seguranca e governanca de conectores externos.
12. Criar matriz papel x homepage x fallback.
13. Criar suite de testes automatizados para indicadores, filtros, permissoes e relatorios.

## 5. Backlog refinado

| Prioridade | Item | Justificativa |
|---|---|---|
| P0 | Corrigir Total Order para usar fonte de pedidos. | Evita indicador global incorreto. |
| P0 | Fechar conta funcional de despesa. | Evita grafico financeiro divergente. |
| P0 | Unificar status entre vendas, fiscal e PDV. | Evita totais conflitantes. |
| P0 | Corrigir regra de total filtrado em consultas paginadas. | Evita detalhe financeiro incorreto. |
| P0 | Definir modelo de pesquisa salva. | Necessario para seguranca, dados e UX. |
| P0 | Definir politica de conectores externos. | Necessario para LGPD e seguranca. |
| P0 | Fechar matriz de permissoes para dashboards e cards. | Evita exposicao indevida de dados. |
| P0 | Criar testes automatizados minimos. | Reduz risco de regressao nos indicadores. |
| P1 | Parametrizar rankings e periodo inicial. | Evita valores fixos como regra de produto. |
| P1 | Definir colunas de cada aba de transacoes recentes. | Melhora implantacao e validacao humana. |
| P1 | Criar roteiro de homologacao por perfil e dashboard. | Facilita validacao. |
| P2 | Padronizar mensagens e idioma dos widgets. | Melhora consistencia visual. |

## 6. Controle de cobertura funcional

| Bloco funcional | Situacao | Conteudo incorporado | Pendencia de conferencia |
|---|---|---|---|
| Identificacao funcional | Incorporado | ID APP-TEN-011. | Nenhuma. |
| Area publica e layout | Incorporado | Separacao anonimo/autenticado, cabecalho, menu, usuario e licenca. | Fechar logout por licenca expirada. |
| Dashboard raiz | Incorporado | Painel Siser e painel operacional. | Corrigir Total Order e conta de despesa. |
| Atalhos | Incorporado | 12 cards de acesso rapido. | Matriz final de permissao. |
| BI vendas/compras | Incorporado | Totais, graficos, rankings e detalhes. | Parametros e status. |
| BI financeiro | Incorporado | Receber, pagar, aberto, baixado, inadimplencia e detalhe. | Paginacao, estados e conta correta. |
| BI estoque/fiscal | Incorporado | Estoque, custo, entradas, saidas, documentos e modelos. | Unidade, filtros fiscais e periodo. |
| Home/busca/feed | Incorporado | Home por perfil, buscas, pesquisas salvas, feed e dashlets. | Modelo de dados e seguranca. |
| Conectores | Incorporado como lacuna | Capacidade identificada. | Contrato de compliance e seguranca. |
| Modelo de dados | Incorporado | Entidades, contratos, relacionamentos e diagrama. | Chaves/cardinalidades ausentes. |
| Dicionario de dados | Incorporado | Campos de indicadores e contratos preservados. | Tipos/tamanhos ausentes. |
| Testes | Incorporado como lacuna | Ausencia identificada. | Criar automacao e roteiro manual. |

## 7. Notas de rodape

[^agente-001]: Itens de maturidade, seguranca, auditoria, performance, criterios de aceite e backlog refinado foram organizados pelo agente a partir do material disponivel. O que nao estava explicitamente informado foi marcado como lacuna, decisao ou `Nao informado no material`.

