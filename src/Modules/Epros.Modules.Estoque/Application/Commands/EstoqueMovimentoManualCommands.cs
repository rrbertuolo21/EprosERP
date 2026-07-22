using System;
using Epros.Modules.Estoque.Domain.Enums;
using Epros.Shared.Domain.Enums;
using Epros.Shared.Application.Contracts;

namespace Epros.Modules.Estoque.Application.Commands
{
    public record CriarEstoqueMovimentoManualCommand(
        Guid ProdutoId,
        ETipoEstoque TipoEstoque,
        ETipoMovimento TipoMovimento,
        decimal QuantidadeMovimentada,
        decimal ValorUnitario
    ) : ICommand;

    public record AtualizarEstoqueMovimentoManualCommand(
        Guid Id,
        Guid ProdutoId,
        ETipoEstoque TipoEstoque,
        ETipoMovimento TipoMovimento,
        decimal QuantidadeMovimentada,
        decimal ValorUnitario
    ) : ICommand;

    public record DeletarEstoqueMovimentoManualCommand(Guid Id) : ICommand;
}
