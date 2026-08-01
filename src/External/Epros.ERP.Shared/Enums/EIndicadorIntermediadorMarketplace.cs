using System.ComponentModel;


namespace Epros.ERP.Shared.Enums
{
    public enum EIndicadorIntermediadorMarketplace
    {
        [Description("Operação sem intermediador (em site ou plataforma própria")]
        OperacaoSemIntermediador = 0,
        [Description("Operação em site ou plataforma de terceiros(intermediadores/marketplace)")]
        OperacaoEmSiteOuPlataformaTerceiro = 1
    }
}
