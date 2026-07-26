using System.ComponentModel;

namespace Epros.ERP.Shared.Enums
{
    public enum EStatusContasAReceber
    {

        [Description("Vencidos")]
        Vencidos = 1,

        [Description("À Vencer")]
        AVencer = 2,

        [Description("Vencendo Hoje")]
        VencendoHoje = 3,
    }
}
