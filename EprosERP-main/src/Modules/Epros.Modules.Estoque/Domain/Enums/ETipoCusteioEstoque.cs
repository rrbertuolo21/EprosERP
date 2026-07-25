using System.ComponentModel;

namespace Epros.Modules.Estoque.Domain.Enums
{
    /// <summary>Tipo de custeio aplicado ao saldo de estoque do produto. Porte fiel do legado Epros.ERP.Shared.Enums.ETipoCusteioEstoque.</summary>
    public enum ETipoCusteioEstoque
    {
        /// <summary>Custo médio ponderado.</summary>
        [Description("Custo médio")]
        CustoMedio = 0,

        /// <summary>Primeiro que entra, primeiro que sai.</summary>
        [Description("Primeiro que entra primeiro que sai")]
        PEPS = 1,

        /// <summary>Último que entra, primeiro que sai.</summary>
        [Description("Ultimo que entra primeiro que sai")]
        UEPS = 2
    }
}
