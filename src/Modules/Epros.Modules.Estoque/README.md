# Epros.Modules.Estoque

Módulo de **produtos, estoque e compras (entrada de NF-e de fornecedor)** do EprosERP.
Porte do domínio de Produtos/Estoque/Compras do legado (`Epros.ERP.Domain/Entities/Cadastros/Produtos`,
`Entities/Estoque`, `Entities/Compras`) para a arquitetura modular CQRS/EF Core (PostgreSQL), com
`EntidadeSaaSBase` (PK/FK `Guid`, multi-tenant, soft delete, sincronização offline).

## Responsabilidades

- **Produtos** — `Produto` (agregado raiz, com saldo/custo-médio denormalizados), `CategoriaProduto`,
  `MarcaProduto`, `UnidadeMedidaComercial`, `UnidadeMedidaTributavel`, `ProdutoGrupo`,
  `ProdutoEspecifico` (+combustível), `Balanca`, `Adicionais`/`AdicionaisProduto`,
  `ProdutoHistoricoReajuste`.
- **Estoque** — `EstoqueProduto`, `EstoqueMovimentoManual`, `MovimentoEstoque`,
  `ProdutoFichaEstoqueEntrada/Saida`, `FatoGeradorEstoque`. Custo médio ponderado em
  `Produto.LancarEntradaEstoque`; baixa/restauração de saldo em `LancarSaidaEstoque`/`RestaurarEstoque`.
- **Compras** — `Compra` (agregado raiz) e sub-entidades portadas fiéis do legado: `CompraItem`
  (+imposto/IBS-CBS/combustível/importação), `CompraEmitente`/`CompraDestinatario` (+endereços),
  `CompraTransporte`, `CompraNfe`, `CompraTotal`/`CompraTotalIbsCbs`, `CompraFatura`,
  `CompraPagamento`, `CompraConfiguracao`, `CompraEntrega`, `CompraCobrancaEndereco`.
- **Integração por eventos** — handlers que reagem a eventos de outros módulos
  (`VendaFaturada`, `VendaCancelada`, `InspecaoReprovada`, `OrdemProducaoEncerrada`,
  `OrdemManutencaoConcluida`) ajustando estoque via Outbox.

## Estrutura

```
Domain/
  Entities/   entidades e agregados (~54)
  Enums/      enums fiscais/estoque tipados (porte fiel do legado)
Application/
  Commands/  Queries/  Handlers/
Infrastructure/
  Data/       ContextEstoque (56 DbSets)
Migrations/
```

## Padrões

- **CQRS**: comandos/consultas finos; invariantes no agregado (Flunt no construtor) e no handler.
- **Custo médio**: recalculado a cada entrada — `(saldo*custoAnterior + qtd*preço) / novoSaldo`.
- **Multi-tenant**: `TenantId` em toda entidade; `ChaveAcesso` da NF-e única por tenant (evita duplicidade).
- **Outbox transacional**: lançamento de compra grava evento `CompraLancada` no Outbox no mesmo `SaveChanges`.

## Fidelidade ao legado — agregados obrigatórios de Compra (D2)

Auditoria de `Compra` sobre os agregados `Configuracao` e `Entrega`:

- **`CompraEntrega`** já era **nullable** no legado (`CompraEntrega?`); nunca foi obrigatório.
- **`Configuracao`** era um tipo não-anulável na declaração do legado, mas **não** era exigido em
  `Compra.Validar()`, não era preenchido no construtor e era tratado como opcional em `DuplicarCompra`.

Decisão: **manter ambos opcionais** no modelo novo. O único `Validar()` obrigatório do legado cobre
`Emitente` e `Total`. Tornar `Configuracao`/`Entrega` obrigatórios seria **mais restritivo que o legado**
e quebraria o fluxo `LancarCompra` (importação de NF-e de fornecedor, que não fornece configuração
fiscal). Sem perda de dado — as sub-entidades continuam mapeadas (FK `CompraId`, cascade).

## Testes

`tests/Epros.Tests` — cobertura de domínio para custo-médio/saldo de `Produto` (`EstoqueTests`),
agregado fiscal de `Compra` (`CompraFiscalAgregadoTests`), CQRS de produtos/compras, Outbox de compra,
e as entidades de cadastro `CategoriaProduto`/`MarcaProduto`/`UnidadeMedidaComercial`/`Balanca`/
`Adicionais`/`ProdutoGrupo` (`EstoqueCadastrosTests`).
