using System;
using Epros.Modules.Estoque.Domain.Enums;
using Epros.Shared.Application.Contracts;

namespace Epros.Modules.Estoque.Application.Commands
{
    public record CriarProdutoCommand(
        Guid? CategoriaId,
        Guid? MarcaProdutoId,
        Guid? UnidadeMedidaComercialId,
        string Codigo,
        string Descricao,
        string? Ean,
        decimal PesoLiquido,
        decimal PesoBruto,
        decimal ValorVenda,
        decimal ValorVendaPrazo,
        decimal ValorCompra,
        ETipoProduto? TipoProduto,
        bool Ativo,
        string Imagem,
        Guid? CestId,
        Guid? CodigoAnpId,
        Guid? BalancaId,
        bool UtilizaBalanca,
        string? CodigoProdutoBalanca,
        Guid? NcmId
    ) : ICommand;

    public record AtualizarProdutoCommand(
        Guid Id,
        Guid? CategoriaId,
        Guid? MarcaProdutoId,
        Guid? UnidadeMedidaComercialId,
        string Codigo,
        string Descricao,
        string? Ean,
        decimal PesoLiquido,
        decimal PesoBruto,
        decimal ValorVenda,
        decimal ValorVendaPrazo,
        decimal ValorCompra,
        ETipoProduto? TipoProduto,
        bool Ativo,
        Guid? CestId,
        Guid? CodigoAnpId,
        Guid? BalancaId,
        bool UtilizaBalanca,
        string? CodigoProdutoBalanca,
        Guid? NcmId
    ) : ICommand;

    public record DeletarProdutoCommand(Guid Id) : ICommand;
}
