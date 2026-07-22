# Tarefas: Alinhamento e Expansão do Backend C#

## 1. Módulo de Estoque (`Epros.Modules.Estoque`)
- `[x]` Criar entidades de suporte: `CategoriaProduto.cs`, `MarcaProduto.cs` e `UnidadeMedidaComercial.cs`
- `[x]` Expandir entidade `Produto.cs` com campos de balança, pesos, códigos auxiliares e regras de validação do legado
- `[x]` Mapear propriedades e novos relacionamentos no `ContextEstoque.cs`

## 2. Módulo Financeiro (`Epros.Modules.Financeiro`)
- `[x]` Criar entidades de Plano de Contas: `PlanoDeContasFinanceiro.cs` e `PlanoDeContasFinanceiroItem.cs`
- `[x]` Criar entidade de Naturezas Financeiras: `ConfiguracaoCodigoNaturezaFinanceira.cs`
- `[x]` Expandir `ContaPagar.cs` e `ContaReceber.cs` com os vínculos de Plano de Contas e Centro de Custo/Projetos
- `[x]` Mapear propriedades e relacionamentos no `ContextFinanceiro.cs`

## 3. Módulo de Vendas (`Epros.Modules.Vendas`)
- `[x]` Expandir `Venda.cs` e `VendaItem.cs` com dados fiscais de IBS/CBS, transporte e detalhes de destinatário/emitente
- `[x]` Mapear propriedades e relacionamentos no `ContextVendas.cs`

## 4. Módulo de Compras (`Epros.Modules.Estoque` - Notas de Entrada)
- `[x]` Expandir `Compra.cs` e `CompraItem.cs` com os campos adicionais de XML de Notas de Entrada
- `[x]` Mapear propriedades no Context de Compras/Estoque

## 5. Homologação e Banco de Dados
- `[x]` Gerar Migrations do EF Core para expandir o banco físico:
  - `ExpandProdutoEstoque` (Estoque)
  - `AddPlanoDeContasAndNatureza` (Financeiro)
  - `ExpandVendasAndItens` (Vendas)
  - `ExpandCompraEstoque` (Compras)
- `[x]` Executar compilação e suíte de testes unitários com `dotnet test` (312/312 passando!)
