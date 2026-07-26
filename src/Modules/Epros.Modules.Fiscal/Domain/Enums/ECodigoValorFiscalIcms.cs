using System.ComponentModel;

namespace Epros.Modules.Fiscal.Domain.Enums
{
    public enum ECodigoValorFiscalIcms
    {
        [Description("Não Utiliza")]
        NaoUtiliza = -1,
        Tributado = 0,
        Isento = 1,
        Outros = 2
    }
}
