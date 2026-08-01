using System.ComponentModel;

namespace Epros.ERP.Shared.Enums
{
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
