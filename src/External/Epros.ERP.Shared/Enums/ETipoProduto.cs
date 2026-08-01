using System.ComponentModel;

namespace Epros.ERP.Shared.Enums
{
    public enum ETipoProduto
    {
        [Description("Mercadoria para Revenda")]
        MercadoriaParaRevenda = 0,

        [Description("Matéria-Prima")]
        MateriaPrima = 1,

        [Description("Embalagem")]
        Embalagem = 2,

        [Description("Produto em Processo")]
        ProdutoEmProcesso = 3,

        [Description("Produto Acabado")]
        ProdutoAcabado = 4,

        [Description("Subproduto")]
        Subproduto = 5,

        [Description("Produto Intermediário")]
        ProdutoIntermediário = 6,

        [Description("Material de Uso e Consumo")]
        MaterialdeUsoeConsumo = 7,

        [Description("Ativo Imobilizado")]
        AtivoImobilizado = 8,

        [Description("Serviços")]
        Servicos = 9,

        [Description("Outros insumos")]
        OutrosInsumos = 10,

        Outras = 99
    }
}
