using System;
using Epros.Modules.Estoque.Domain.Enums;
using Epros.Shared.Application.Contracts;

namespace Epros.Modules.Estoque.Application.Commands
{
    public record CriarBalancaCommand(
        string Nome,
        int QntDigitoIdentificador,
        int QntDigitoCodigoProduto,
        int QntDigitoValorProduto,
        int QntCasaDecimal,
        ETipoValorBalanca TipoValor
    ) : ICommand;

    public record AtualizarBalancaCommand(
        Guid Id,
        string Nome,
        int QntDigitoIdentificador,
        int QntDigitoCodigoProduto,
        int QntDigitoValorProduto,
        int QntCasaDecimal,
        ETipoValorBalanca TipoValor
    ) : ICommand;

    public record DeletarBalancaCommand(Guid Id) : ICommand;
}
