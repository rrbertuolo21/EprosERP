# Epros.Modules.Financeiro

Módulo **Financeiro** do EprosERP: contas a pagar/receber, plano de contas, fato gerador, bancos/contas/cartões e conciliação bancária (OFX). Consome eventos de outros módulos (ex.: `CompraLancada` de Estoque) via Outbox para gerar títulos automaticamente.

## Arquitetura

Clean Architecture + CQRS (MediatR), multi-tenant por `TenantId`.

```
Application/
  Commands/       Contratos de escrita (records)
  Queries/        Contratos de leitura
  Handlers/       Handlers de comando/consulta
  EventHandlers/  Consumidores de eventos de integração (CompraLancada, ProjetoFaturado, FolhaProcessada)
Domain/
  Entities/       ContasAPagar, ContasAReceber, PlanoDeContasFinanceiro(+Item), FatoGerador, Banco, CartaoDeCredito, ...
Infrastructure/
  Data/           ContextFinanceiro (EF Core / PostgreSQL)
  Jobs/           OutboxProcessorJob
Migrations/
```

## Fluxos principais

- **Contas a Pagar / Receber** — criação, alteração de itens (baixa parcial), baixa/estorno conciliado, cancelamento. Handlers em `ContasAPagarHandlers` / `ContasAReceberHandlers`.
- **Plano de Contas Financeiro** — `CriarPlanoDeContasFinanceiroCommandHandler` (+ itens em `PlanoDeContasFinanceiroItem`), associação a empresas e configuração de natureza (recebimento/pagamento). Um item exige plano existente.
- **Fato Gerador & OFX** — importação de extrato OFX, processamento e conciliação de transações contra contas a pagar/receber (`FatoGeradorFinanceiroAndOfxHandlers`).
- **Bancos / Contas / Cartões** — cadastro e faturas de cartão de crédito (`BancoAndContaBancariaAndCartaoHandlers`).

## Integração (Outbox / Eventos)

- `CompraLancadaEventHandler` — ao consumir `CompraLancada`, cadastra o fornecedor (se necessário) e cria `ContasAPagar`.
- `ProjetoFaturadoFinanceiroHandler`, `FolhaProcessadaFinanceiroHandler` — geram títulos a partir de eventos de Projetos e RH.
- `OutboxProcessorJob` processa a fila de mensagens de forma idempotente (marca `ProcessadoEm`/`Erro`).

## Testes

`tests/Epros.Tests/FinanceiroTests.cs` — entidades ContasAPagar/Receber (criação, baixa, estorno, cancelamento), o fluxo Outbox `CompraLancada → ContasAPagar` (com e sem fornecedor pré-cadastrado) e os handlers de Plano de Contas (criação de plano, rejeição de item sem plano, persistência de item de plano existente).
