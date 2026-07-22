# Epros.Modules.Vendas

Módulo de **Vendas / PDV e Documento Fiscal** do EprosERP. Concentra o ciclo de venda do ponto de venda (abertura/fechamento de caixa, registro e cancelamento de vendas) e a composição do documento fiscal (NF-e / NFC-e), incluindo itens, impostos, transporte, pagamento e histórico de transmissão.

## Arquitetura

Segue Clean Architecture + CQRS (via MediatR) com isolamento multi-tenant (RLS por `TenantId`).

```
Application/
  Commands/    Comandos (records) — contratos de escrita
  Queries/     Consultas (records) — contratos de leitura
  Handlers/    Handlers de comando/consulta
  Mappers/     Tradução de status/enums do legado
Domain/
  Entities/    Venda + agregados fiscais (VendaItem, VendaImposto, VendaNfe, ...)
  Enums/       EVendaStatus, EModeloDocumento, EModalidadeFrete, EVendaOrigem, ...
Infrastructure/
  Data/        ContextVendas (EF Core / PostgreSQL) + configurações de mapeamento
Migrations/    Migrations EF Core
```

## Fluxos principais

- **Caixa** — `AbrirCaixaCommandHandler`, `FecharCaixaCommandHandler`, `RegistrarCaixaMovimentoCommandHandler`. Um operador só pode ter uma sessão aberta por vez; vendas exigem caixa aberto.
- **RegistrarVendaCommandHandler** — valida o caixa (existente e aberto), materializa a `Venda` com itens e, quando a venda é *emitida/contingência*, publica `VendaFaturadaEventNotification` (integração com Estoque/Financeiro).
- **CancelarVendaCommandHandler** — cancela venda do tenant ativo (idempotente: rejeita venda inexistente ou já cancelada) e enfileira a mensagem `VendaCancelada` no **Outbox** para estorno.
- **VendaFiscalHandlers** — CRUD do cabeçalho fiscal e itens (`CriarVendaFiscal`, `AtualizarVendaFiscal`, `AdicionarItemFiscal`, ...).
- **VendaAcoesHandlers** — ações fiscais (DANFE, cupom não fiscal, NF-e simplificada, transmissão).
- **Sincronização offline** — `SincronizarCaixasCommandHandler` / `SincronizarVendasCommandHandler` para o PDV offline (reconciliação por `SyncId`).

## Integração (Outbox / Eventos)

- Publica `VendaFaturadaEventNotification` no faturamento (consumido por Estoque para baixa e Financeiro para contas a receber).
- Grava `VendaCancelada` no Outbox no cancelamento, garantindo entrega transacional.

## Testes

`tests/Epros.Tests/VendasCqrsTests.cs` — cobre o fluxo feliz (abrir caixa → registrar → status → detalhe → listar → obter → fechar) e regras: rejeição por caixa inexistente/fechado, publicação (ou não) do evento de faturamento conforme o status, cancelamento com Outbox e idempotência, e criação de venda fiscal.
