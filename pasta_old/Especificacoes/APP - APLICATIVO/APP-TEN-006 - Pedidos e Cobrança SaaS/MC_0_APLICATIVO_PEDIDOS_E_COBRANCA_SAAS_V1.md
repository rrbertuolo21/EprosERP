# MC 0_APLICATIVO PEDIDOS_E_COBRANCA_SAAS V1

**Projeto:** Epros  
**Empresa:** Siser  
**Modulo:** APLICATIVO  
**Submodulo:** PEDIDOS_E_COBRANCA_SAAS  
**ID funcional:** APP-TEN-006  
**Versao:** V1  
**Status:** Pronto para validacao humana  
**Data:** 2026-06-06

## 1. Objetivo

Esta matriz mede a completude funcional de pedidos e cobranca SaaS do Epros, incluindo pedido, cupom, checkout, fatura, PIX, webhook, transferencia/offline, comprovante, sessao de pagamento, pagamentos e bloqueio financeiro.

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
| Pedido SaaS | Parcial | orders registra valor, desconto, moeda e payment_status. | Campos completos nao detalhados. | Fechar dicionario fisico final de orders. | P0 | Assinatura |
| Cupom | Parcial | coupons e user_coupons identificados; desconto e uso registrados. | Tipo, validade, limites e unicidade nao detalhados. | Completar contrato de cupom. | P0 | Catalogos/Comercial |
| Checkout online | Parcial | payment_sessions pending/completed e gateways identificados. | Confirmacao, expiracao e retry nao detalhados. | Definir contrato de checkout. | P0 | Operacao Super Admin |
| Transferencia/offline | Parcial | bank_transfer_payments pending/approved/rejected. | Campos completos e fluxo de aprovacao precisam detalhar. | Criar formulario e regras de aprovacao. | P0 | Financeiro Siser |
| Comprovante | Parcial | proof_of_payments com tenant, valor, data, arquivo, status unread/read. | Retencao, tamanho, formatos e privacidade nao definidos. | Definir politica de anexos financeiros. | P0 | Compliance/Arquivos |
| Fatura SaaS | Coberto | fatura possui vencimento, valor, status, comissoes e quitacao. | Constraint de competencia pendente. | Definir cliente+competencia. | P0 | Limites |
| Pagamento de fatura | Coberto | fatura_pagamento possui tipo, data, valores, tarifa, PaymentId. | PaymentId unico/idempotencia nao fechado. | Definir chave idempotente. | P0 | Pagamentos |
| PIX | Parcial | Gera QR/link/PaymentId para fatura. | Reuso de cobranca, expiracao e reprocesso pendentes. | Definir idempotencia e expiracao. | P0 | Gateway |
| Webhook | Parcial | Webhook atualiza status. | Autenticidade, retry e duplicidade nao detalhados. | Especificar webhook seguro. | P0 | Gateway |
| Bloqueio financeiro | Coberto | Fatura aguardando pagamento >15 dias gera block. | Parametrizacao da tolerancia precisa confirmar. | Tornar parametro Siser. | P0 | Limites |
| Pagamento manual | Parcial | Baixa manual por backoffice com valor/data/forma. | Segregacao e justificativa nao detalhadas. | Exigir permissao e justificativa. | P0 | Financeiro |
| Gateway settings | Parcial | Chaves e habilitacao por gateway identificadas. | Cofre, mascaramento e rotacao nao definidos. | Definir segredo seguro. | P0 | Seguranca |
| Recorrencia | Parcial | Filas/rotinas com new/processing/failed/completed. | Agenda e retry nao detalhados. | Detalhar workflow. | P1 | Workflow |
| Backoffice faturas | Coberto | Campos de Faturas preservados. | FormaPagamento tem grafia inconsistente no material. | Sanear nome no modelo final. | P1 | UX/Dados |
| Testes automatizados | Lacuna | Nao identificados no material. | Alto risco financeiro. | Criar suite de pagamento. | P0 | QA |

## 4. Itens criticos para validacao humana

1. Fechar dicionario final de orders, coupons, user_coupons e bank_transfer_payments.
2. Definir status unico entre pedido, sessao, fatura, pagamento e assinatura.
3. Definir idempotencia por PaymentId e/ou chave de webhook.
4. Definir autenticidade, retry e logs de webhook.
5. Definir expiracao e reuso de cobranca PIX.
6. Definir constraint de fatura por cliente e competencia.
7. Definir politica de comprovantes: formato, tamanho, retencao, privacidade e rejeicao.
8. Definir cofre/mascaramento/rotacao de credenciais de gateway.
9. Parametrizar tolerancia de atraso.
10. Exigir justificativa para baixa manual e alteracao de fatura.

## 5. Backlog refinado

| Prioridade | Item | Justificativa |
|---|---|---|
| P0 | Criar maquina de estados financeira unica. | Evita divergencia pedido/fatura/assinatura. |
| P0 | Implementar idempotencia de webhook/PIX. | Evita baixa duplicada. |
| P0 | Criar suite automatizada de pagamentos. | Alto risco financeiro. |
| P0 | Criar modelo seguro de gateway. | Protege segredos. |
| P0 | Definir comprovante offline completo. | Necessario para aprovacao manual. |
| P0 | Definir constraint cliente+competencia da fatura. | Evita cobranca duplicada. |
| P1 | Criar relatorio de conciliacao. | Suporte ao financeiro. |
| P1 | Detalhar recorrencia e retries. | Confiabilidade. |

## 6. Controle de cobertura funcional

| Bloco funcional | Situacao | Conteudo incorporado | Pendencia de conferencia |
|---|---|---|---|
| Identificacao funcional | Incorporado | APP-TEN-006. | Nenhuma. |
| Pedido | Parcial | orders, valor, desconto, moeda, status. | Campos finais. |
| Cupom | Parcial | coupons, user_coupons, desconto. | Regras completas. |
| Fatura | Incorporado | fatura, composicao, pagamento, reajuste. | Competencia/duplicidade. |
| PIX/Webhook | Parcial | PaymentId, QR/link, webhook. | Idempotencia/seguranca. |
| Offline | Parcial | transferencia e comprovante. | Retencao/aprovacao. |
| Telas | Incorporado | Area cliente e backoffice faturas/cliente/plano. | UX final. |
| Integracoes | Parcial | Faturas, planos, token, webhook. | Contratos finais. |
| Testes | Lacuna | Ausencia identificada. | Suite automatizada. |

## 7. Notas de rodape

[^agente-001]: A maquina de estados financeira unica, relatorio de conciliacao, cofre de gateway e constraint cliente+competencia foram propostos pelo agente como encaminhamento de lacunas reais. Permanecem como decisao/backlog ate validacao humana.
