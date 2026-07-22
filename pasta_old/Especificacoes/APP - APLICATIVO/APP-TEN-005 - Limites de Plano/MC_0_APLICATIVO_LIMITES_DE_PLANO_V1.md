# MC 0_APLICATIVO LIMITES_DE_PLANO V1

**Projeto:** Epros  
**Empresa:** Siser  
**Modulo:** APLICATIVO  
**Submodulo:** LIMITES_DE_PLANO  
**ID funcional:** APP-TEN-005  
**Versao:** V1  
**Status:** Pronto para validacao humana  
**Data:** 2026-06-06

## 1. Objetivo

Esta matriz mede a completude funcional do submodulo Limites de Plano do Epros, cobrindo planos, quotas, modulos contratados, bloqueio por plano/assinatura/inadimplencia, faturas SaaS, PIX, area do cliente, backoffice Siser e contratos de integracao.

## 2. Legenda de status

| Status | Significado |
|---|---|
| Coberto | Capacidade possui regra, fluxo, entidade ou contrato suficiente para construcao inicial. |
| Parcial | Capacidade existe, mas precisa decisao, complemento ou validacao. |
| Lacuna | Capacidade citada ou esperada sem especificacao suficiente. |
| Decisao | Exige validacao humana antes de construcao. |

## 3. Matriz de completude

| Capacidade | Status | Evidencia funcional consolidada | Lacuna / risco | Acao recomendada | Prioridade | Dependencias |
|---|---|---|---|---|---|---|
| Cadastro de plano | Coberto | Plano possui Nome, DescricaoCurta, DescricaoCompleta, Valor, QtdeUsuarios, QtdeEmpresas, DataInicio, DataFim, Ativo e RecursosInclusos. | Dicionario completo de grupo de plano nao informado. | Detalhar plano_grupo. | P1 | Backoffice Siser |
| Modulos do plano | Coberto | modulo_geral e modulo_plano possuem campos, status e relacionamento. | Unicidade PlanoId+ModuloGeralId nao informada. | Definir constraint. | P1 | Permissoes/Menu |
| Plano ativo | Coberto | Plano/assinatura inativa bloqueia uso. | Mensagem final nao padronizada. | Definir mensagens por causa. | P0 | Identidade |
| Modulo ativo/contratado | Parcial | Validacao de modulo ativo e vinculado ao plano identificada. | Excecao para usuario interno precisa politica. | Aprovar regra de bypass interno. | P0 | Permissoes/Menu |
| Limite de usuarios | Parcial | QtdeUsuarios, MaximumUser, total_user e Tipo 1 identificados. | Uma fonte apenas exibe limite sem enforcement; comportamento de downgrade nao fechado. | Implementar enforcement obrigatorio e decidir downgrade. | P0 | Usuarios |
| Limite de empresas | Coberto | QtdeEmpresas e Tipo 0 identificados. | Dominio de empresa comercial Siser x empresa cliente precisa separacao fisica. | Confirmar tabelas/campos por dominio. | P0 | Onboarding/Empresas |
| Limite de produtos | Coberto | Contagem por tenant e MaximumProduct identificadas. | Periodo de contagem de produto aparece em outro trecho. | Definir se limite e total historico ou periodo da assinatura. | P1 | Estoque/Cadastros |
| Limite de clientes comerciais | Coberto | Contagem por tipo cliente e MaximumCustomer identificada. | Modelo final de pessoa/tipo fica no modulo dono. | Garantir integracao com Cadastros Base. | P1 | Cadastros Base |
| Limite de fornecedores | Coberto | Contagem por tipo fornecedor e MaximumSupplier identificada. | Modelo final de pessoa/tipo fica no modulo dono. | Garantir integracao com Cadastros Base. | P1 | Cadastros Base/Compras |
| Limite de faturas/documentos | Decisao | MaximumInvoice e contagem de documentos identificadas. | Material mistura faturas/documentos e compras. | Definir semantica: venda, compra, documento fiscal ou todos. | P0 | Vendas/Compras/Financeiro |
| Limite de locais | Parcial | Controle de locations e quota de locais identificado. | Entidade local e campo de plano final nao detalhados. | Definir tabela/campo e mensagem. | P1 | Cadastros Base |
| Limite de armazenamento | Parcial | storage_limit e conversao para unidade menor identificados. | Unidade oficial, consumo e escopo nao definidos. | Definir unidade, agregacao e rotina de medicao. | P0 | Plataforma/Arquivos |
| Recurso ilimitado | Decisao | Material usa `0` e `-1` como ilimitado. | Divergencia pode causar bloqueio indevido. | Escolher padrao unico e migrar todos os limites. | P0 | Todos os consumidores |
| Comparacao de limite | Parcial | Validacoes usam contagem atual contra limite. | Operador identificado pode permitir confusao se validacao ocorrer antes/depois da criacao. | Formalizar regra: bloquear quando a solicitacao fizer o total ultrapassar o limite. | P0 | QA |
| Mensagens de bloqueio | Lacuna | Mensagem unica aparece para plano inativo e limite excedido. | Usuario/suporte nao identificam causa. | Criar catalogo de mensagens por causa. | P0 | UX/Suporte |
| Severidade de bloqueio | Lacuna | Limite excedido aparece com severidade inadequada em trecho do material. | Pode sinalizar sucesso em falha. | Padronizar erro bloqueante. | P0 | UX/API |
| Fatura SaaS | Coberto | fatura possui vencimento, valor, status, comissoes, quitacao e pagamento. | Duplicidade Valor/ValorTotal e Status/StatusFatura em contratos. | Consolidar contrato final. | P1 | Financeiro Siser |
| Composicao de fatura | Parcial | fatura_composicao e gera_fatura_composicao identificadas. | Validacao efetiva de composicao obrigatoria precisa ser fechada. | Bloquear fatura sem composicao quando regra aplicar. | P0 | Financeiro Siser |
| Historico de reajuste | Coberto | ValorAtual, ValorNovo, PercentualReajuste e TipoReajuste obrigatorios. | TipoReajuste dominio nao informado. | Definir dominio. | P1 | Financeiro Siser |
| Duplicidade de fatura mensal | Parcial | Material indica necessidade de impedir duplicidade mes/ano. | Constraint/campos exatos nao definidos. | Definir chave funcional por cliente e competencia. | P0 | Financeiro Siser |
| PIX | Parcial | PaymentId, expiracao, TicketUrl, QrCode e QrCodeBase64 identificados. | Idempotencia e expiracao operacional nao definidas. | Definir regra de reuso/renovacao de cobranca. | P0 | Pagamentos |
| Webhook de pagamento | Parcial | Webhook identificado. | Contrato, idempotencia, autenticidade e retentativa nao detalhados. | Especificar contrato final. | P0 | Pagamentos |
| Bloqueio por inadimplencia | Coberto | Fatura aguardando pagamento com atraso superior a 15 dias bloqueia uso. | Parametrizacao do prazo nao informada. | Tornar prazo parametro Siser. | P0 | Area do Cliente |
| Area do cliente | Coberto | Minhas faturas, faturas vencidas, QR code PIX e planos identificados. | UX final e mensagens nao detalhadas. | Refinar prototipo/telas finais. | P1 | UX |
| Backoffice Siser | Coberto | Telas e campos de cliente, fatura, plano, modulo, revenda e vendedor preservados. | Permissoes finas por papel nao detalhadas. | Definir matriz de permissoes. | P0 | Permissoes |
| Registro de cliente novo | Parcial | Dados de cliente, endereco, composicoes e quantidades identificados. | Empresa/revenda/vendedor/plano padrao aparecem como parametrizacao pendente. | Criar parametros governados. | P0 | Onboarding |
| Token de sistema | Parcial | Token e contratos protegidos identificados. | Escopos, expiracao, rotacao e auditoria nao detalhados. | Especificar seguranca de token. | P0 | Seguranca |
| Tenant | Decisao | Duas estruturas de tenant aparecem com campos diferentes. | Risco de duplicidade e chave divergente. | Consolidar entidade tenant oficial. | P0 | Identidade/Isolamento |
| Vendedor | Parcial | Campos preservados. | TenantId aparece duplicado. | Remover duplicidade no modelo final. | P1 | Comercial |
| Testes automatizados | Lacuna | Cenarios foram listados, mas suite automatizada nao identificada. | Alto risco de regressao em bloqueios. | Criar suite obrigatoria. | P0 | QA |

## 4. Itens criticos para validacao humana

1. Escolher o padrao unico para recurso ilimitado: `0`, `-1` ou outro marcador.
2. Definir a semantica final de limite de fatura/documento/compra.
3. Confirmar que o limite deve bloquear quando a criacao solicitada fizer o total ultrapassar o limite.
4. Tornar o limite de usuarios efetivamente bloqueante em criacao e ativacao de usuario.
5. Decidir comportamento em downgrade de plano com usuarios acima do novo limite.
6. Consolidar a entidade tenant oficial.
7. Definir mensagens separadas para plano inativo, assinatura expirada, limite excedido e inadimplencia.
8. Definir idempotencia, autenticidade e retentativa do webhook de pagamento.
9. Definir chave funcional para impedir fatura duplicada por cliente e competencia.
10. Parametrizar empresa comercial Siser, revenda, vendedor e plano usados em onboarding.
11. Separar definitivamente empresa comercial Siser de empresa cliente operacional.
12. Definir matriz de permissoes do backoffice Siser.

## 5. Backlog refinado

| Prioridade | Item | Justificativa |
|---|---|---|
| P0 | Implementar servico central de verificacao de limite antes de qualquer criacao controlada. | Evita validacoes espalhadas e divergentes. |
| P0 | Implementar enforcement de limite de usuarios. | O material mostra lacuna critica nessa quota. |
| P0 | Criar catalogo de mensagens de bloqueio por causa. | Evita confusao operacional. |
| P0 | Criar parametro de recurso ilimitado. | Elimina divergencia 0 x -1. |
| P0 | Consolidar tenant oficial do submodulo. | Evita chaves divergentes. |
| P0 | Especificar webhook de pagamento com idempotencia. | Evita baixa duplicada. |
| P0 | Definir chave de fatura por cliente/competencia. | Evita duplicidade de cobranca mensal. |
| P0 | Criar suite de testes de limites, bloqueio financeiro e PIX. | Alto risco funcional. |
| P1 | Criar relatorio de consumo x limite por cliente. | Facilita suporte, implantacao e vendas. |
| P1 | Detalhar plano_grupo. | Completa modelo comercial. |
| P1 | Definir dominio de TipoReajuste. | Completa historico de reajuste. |
| P1 | Definir escopos e rotacao do token de sistema. | Aumenta seguranca. |

## 6. Controle de cobertura funcional

| Bloco funcional | Situacao | Conteudo incorporado | Pendencia de conferencia |
|---|---|---|---|
| Identificacao funcional | Incorporado | APP-TEN-005. | Nenhuma. |
| Regras de plano | Incorporado | Plano ativo, assinatura, modulo, limites e bloqueios. | Mensagens finais. |
| Modelo de dados | Incorporado | tenant, cliente, plano, modulo, fatura, pagamento, composicoes, permissao, revenda e vendedor. | Tenant oficial, plano_grupo e relacionamentos N:N. |
| Dicionario de dados | Incorporado | Campos, tipos, tamanhos, obrigatoriedade e relacionamentos quando informados. | Campos nao informados seguem marcados. |
| Fluxos | Incorporado | Contratacao, criacao de recurso, bloqueio financeiro, PIX. | UX final e estados completos. |
| Telas | Incorporado | Area do cliente, faturas, planos, clientes, modulos, revendas e vendedores. | Permissoes por papel. |
| Integracoes | Parcial | Faturas, planos, clientes, PIX, token e webhook. | Escopos, seguranca e idempotencia. |
| Testes | Parcial | Cenarios identificados. | Suite automatizada inexistente no material. |

## 7. Notas de rodape

[^agente-001]: A recomendacao de servico central de verificacao de limite, relatorio de consumo x limite, catalogo de mensagens, parametrizacao de ilimitado e controles de idempotencia foi criada pelo agente como encaminhamento de lacunas reais identificadas no material. Nenhuma dessas recomendacoes foi tratada como regra definitiva sem validacao humana.
