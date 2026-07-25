using System.ComponentModel;

namespace Epros.ERP.Shared.Enums
{
    public enum ERegimeApuracao
    {
        [Description("Não Utiliza")]
        NaoUtiliza = 0,

        [Description("Lucro Real")]
        LucroReal = 1,

        [Description("Lucro Presumido")]
        LucroPresumido = 2,
    }
}