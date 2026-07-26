using MediatR;
using Epros.Shared.Application.Models;

namespace Epros.Modules.Estoque.Application.Commands
{
    /// <summary>
    /// Registra um reajuste de valor (venda ou compra) de um produto, gravando o novo valor
    /// e registrando o histórico de reajuste. Porte fiel do fluxo legado de reajuste de produto:
    /// o novo valor é calculado como <c>ValorAtual * Fator + ValorFixo</c> quando <paramref name="ValorNovo"/>
    /// não é informado (0). Delega ao domínio (<c>Produto.AlterarValorVenda/AlterarValorCompra</c>),
    /// que anexa o <c>ProdutoHistoricoReajuste</c>.
    /// </summary>
    /// <param name="Tipo">0 = Valor de venda, 1 = Valor de compra (ETipoReajuste).</param>
    public record RegistrarProdutoReajusteCommand(
        System.Guid ProdutoId,
        int Tipo,
        decimal Fator,
        decimal ValorFixo,
        decimal ValorNovo,
        string? Motivo
    ) : IRequest<CommandResult>;
}
