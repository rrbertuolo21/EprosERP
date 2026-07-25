using System.ComponentModel;

namespace Epros.Modules.Estoque.Domain.Enums
{
    /// <summary>
    /// Classificação do tipo de estoque movimentado. Porte fiel do legado Epros.ERP.Shared.Enums.ETipoEstoque.
    /// </summary>
    public enum ETipoEstoque
    {
        [Description("Matéria-prima")]
        MateriaPrima,

        [Description("Produto em processo")]
        ProdutoEmProcesso,

        [Description("Produto acabado")]
        ProdutoAcabado,

        [Description("Material de consumo")]
        MaterialDeConsumo,

        [Description("Mercadoria para revenda")]
        MercadoriaParaRevenda,

        [Description("Produto em Geral")]
        Geral
    }
}
