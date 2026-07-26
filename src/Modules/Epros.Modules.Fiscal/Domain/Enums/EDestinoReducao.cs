using System.ComponentModel;

namespace Epros.Modules.Fiscal.Domain.Enums
{
    public enum EDestinoReducao
    {
        [Description("Não Utiliza")]
        NaoUtiliza = -1,
        Isento = 1,
        Outras = 2
    }
}
