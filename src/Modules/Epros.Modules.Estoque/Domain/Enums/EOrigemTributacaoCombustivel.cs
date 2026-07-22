using System.ComponentModel;

namespace Epros.Modules.Estoque.Domain.Enums
{
    /// <summary>
    /// Origem da tributação de combustível. Porte fiel do legado Epros.ERP.Shared.Enums.EOrigemTributacaoCombustivel.
    /// </summary>
    public enum EOrigemTributacaoCombustivel
    {
        [Description("Não Utiliza")]
        NaoUtiliza = -1,
        Nacional = 0,
        Importado = 1
    }
}
