
using System.ComponentModel;

namespace Epros.ERP.Shared.Enums
{
    public enum ETipoContribuinte
    {
        [Description("Não Informado")]
        NaoInformado = 0,
        [Description("Simples Nacional")]
        SimplesNacional = 1,
        RPA,
        MEI
    }
}
