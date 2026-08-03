
using System.ComponentModel;

namespace Epros.ERP.Shared.Enums
{
    public enum ETipoContatoTelefonico
    {
        [Description("Não Utiliza")]
        NaoUtiliza = -1,
        Residencial = 0,
        Comercial = 2,
        Recado = 3,
        [Description("Emergência")]
        Emergencia = 4,
        Outros = 99

    }
}
