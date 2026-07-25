using System.ComponentModel;

namespace Epros.Modules.Estoque.Domain.Enums
{
    /// <summary>
    /// Indicador de intermediador/marketplace. Porte fiel do legado
    /// Epros.ERP.Shared.Enums.EIndicadorIntermediadorMarketplace.
    /// </summary>
    public enum EIndicadorIntermediadorMarketplace
    {
        [Description("Operação sem intermediador (em site ou plataforma própria")]
        OperacaoSemIntermediador = 0,

        [Description("Operação em site ou plataforma de terceiros(intermediadores/marketplace)")]
        OperacaoEmSiteOuPlataformaTerceiro = 1
    }
}
