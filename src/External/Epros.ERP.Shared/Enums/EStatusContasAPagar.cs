using System.ComponentModel;

namespace Epros.ERP.Shared.Enums
{
    public enum EStatusContasAPagar
    {

        [Description("Vencidos")]
        Vencidos = 1,

        [Description("À Vencer")]
        AVencer = 2,

        [Description("Vencendo Hoje")]
        VencendoHoje = 3,
    }
}
