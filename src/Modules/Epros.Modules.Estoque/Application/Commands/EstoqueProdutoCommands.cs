using System;
using Epros.Modules.Estoque.Domain.Enums;
using Epros.Shared.Application.Contracts;

namespace Epros.Modules.Estoque.Application.Commands
{
    public record CriarEstoqueProdutoCommand(
        Guid EmpresaId,
        Guid ProdutoId,
        decimal QuantidadeSaldoEstoque,
        decimal QuantidadeEstoqueMinimo,
        decimal QuantidadeEstoqueMaximo,
        decimal QuantidadeEstoqueReservado,
        decimal ValorSaldo,
        ETipoCusteioEstoque TipoCusteioEstoque
    ) : ICommand;

    public record AtualizarEstoqueProdutoCommand(
        Guid Id,
        Guid EmpresaId,
        Guid ProdutoId,
        decimal QuantidadeSaldoEstoque,
        decimal QuantidadeEstoqueMinimo,
        decimal QuantidadeEstoqueMaximo,
        decimal QuantidadeEstoqueReservado,
        decimal ValorSaldo,
        ETipoCusteioEstoque TipoCusteioEstoque
    ) : ICommand;

    /// <summary>Porte fiel de EstoqueProduto.AlterarDadosAutorizados(): altera apenas mín/máx e tipo de custeio.</summary>
    public record AlterarDadosAutorizadosEstoqueProdutoCommand(
        Guid Id,
        Guid EmpresaId,
        Guid ProdutoId,
        decimal QuantidadeEstoqueMinimo,
        decimal QuantidadeEstoqueMaximo,
        ETipoCusteioEstoque TipoCusteioEstoque
    ) : ICommand;

    public record DeletarEstoqueProdutoCommand(Guid Id) : ICommand;
}
