using System;
using System.Collections.Generic;
using MediatR;
using Epros.Shared.Application.Models;

namespace Epros.Modules.Estoque.Application.Commands
{
    /// <summary>
    /// Porte fiel de ProdutoController.UpdateImage (PUT api/v1/produtos/alterar-imagem).
    /// Recebe a imagem em base64 (data URI) e persiste no produto. A gravação física do arquivo
    /// era feita pelo legado em disco; aqui persistimos a string da imagem (base64/caminho) fielmente
    /// à entidade, mantendo o handler fino.
    /// </summary>
    public record AlterarImagemProdutoCommand(Guid ProdutoId, string Imagem) : IRequest<CommandResult>;

    /// <summary>
    /// Porte fiel de ProdutoController.DeleteImage (PUT api/v1/produtos/deletar-imagem-por-id-produto/{id}).
    /// Limpa a imagem do produto.
    /// </summary>
    public record DeletarImagemProdutoCommand(Guid ProdutoId) : IRequest<CommandResult>;

    /// <summary>
    /// Porte fiel de ProdutoController.DuplicarProduto (POST api/v1/produtos/duplicar-produto).
    /// Cria um novo produto a partir de um existente, com novo Código/Descrição, copiando os demais
    /// atributos, adicionais e o produto específico (combustível) com suas origens.
    /// </summary>
    public record DuplicarProdutoCommand(Guid ProdutoId, string Codigo, string Descricao) : IRequest<CommandResult>;

    /// <summary>
    /// Porte fiel de ProdutoController.ImportarProdutosCsv (POST api/v1/produtos/importar-csv).
    /// Importa produtos em massa a partir de um CSV. Só permite importar quando ainda não há produtos
    /// cadastrados (mesma trava do legado). O conteúdo do arquivo é lido no controller e passado como texto.
    /// </summary>
    public record ImportarProdutosCsvCommand(string ConteudoCsv) : IRequest<CommandResult>;

    /// <summary>
    /// Porte fiel de ProdutoController.ReajustarPrecos (PUT api/v1/produtos/reajustar-precos).
    /// Aplica um reajuste em massa (valor de venda ou de compra) a uma lista de produtos, gravando o
    /// histórico de reajuste (<c>ProdutoHistoricoReajuste</c>) para cada um. Delega ao domínio
    /// (<c>Produto.AlterarValorVenda/AlterarValorCompra</c>).
    /// </summary>
    /// <param name="Tipo">0 = Valor de venda, 1 = Valor de compra (ETipoReajuste).</param>
    /// <param name="Fator">Fator multiplicador aplicado ao valor atual (ex.: 1.10 = +10%). 0 ou 1 = sem fator.</param>
    /// <param name="ValorFixo">Valor fixo somado após o fator.</param>
    /// <param name="ProdutosIds">Ids dos produtos a reajustar. Vazio = todos os produtos ativos.</param>
    public record ReajustarPrecosCommand(
        int Tipo,
        decimal Fator,
        decimal ValorFixo,
        string? Motivo,
        List<Guid>? ProdutosIds
    ) : IRequest<CommandResult>;
}
