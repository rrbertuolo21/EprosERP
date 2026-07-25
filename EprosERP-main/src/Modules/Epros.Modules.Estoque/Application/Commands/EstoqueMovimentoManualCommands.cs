using System;
using Epros.Modules.Estoque.Domain.Enums;
using Epros.Shared.Domain.Enums;
using Epros.Shared.Application.Contracts;

namespace Epros.Modules.Estoque.Application.Commands
{
    public record CriarEstoqueMovimentoManualCommand(
        Guid EmpresaId,
        Guid ProdutoId,
        ETipoEstoque TipoEstoque,
        ETipoMovimento TipoMovimento,
        decimal QuantidadeMovimentada,
        decimal ValorUnitario,
        string? Motivo = null,
        Guid? LocalId = null,
        string? Lote = null,
        DateTime? DataValidade = null
    ) : ICommand;

    public record AtualizarEstoqueMovimentoManualCommand(
        Guid Id,
        Guid ProdutoId,
        ETipoEstoque TipoEstoque,
        ETipoMovimento TipoMovimento,
        decimal QuantidadeMovimentada,
        decimal ValorUnitario,
        string? Motivo = null
    ) : ICommand;

    public record DeletarEstoqueMovimentoManualCommand(Guid Id) : ICommand;

    /// <summary>Estorno controlado de movimento já aplicado (MVM-030). Exige motivo.</summary>
    public record EstornarEstoqueMovimentoManualCommand(
        Guid Id,
        Guid EmpresaId,
        string Motivo
    ) : ICommand;
}
