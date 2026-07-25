using System;
using System.Collections.Generic;
using Epros.Modules.Estoque.Domain.Enums;
using Epros.Shared.Application.Contracts;

namespace Epros.Modules.Estoque.Application.Commands
{
    /// <summary>Linha de ajuste. Quantidade positiva = entrada; negativa = saída controlada (EF §7.8).</summary>
    public record AjusteEstoqueItemInput(
        Guid ProdutoId,
        decimal Quantidade,
        decimal ValorUnitario,
        string? Lote = null
    );

    /// <summary>
    /// Cria e aplica um ajuste de estoque (MVM-025). Observação (motivo) é obrigatória.
    /// EmpresaId é necessário para localizar saldo e gravar fichas.
    /// </summary>
    public record CriarAjusteEstoqueCommand(
        Guid EmpresaId,
        Guid? LocalId,
        DateTime DataAjuste,
        ETipoAjusteEstoque TipoAjuste,
        decimal? ValorRecuperado,
        string Observacao,
        List<AjusteEstoqueItemInput> Itens
    ) : ICommand;
}
