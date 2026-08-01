
using System.ComponentModel;

namespace Epros.ERP.Shared.Enums
{
    public enum ETipoContatoEmail
    {
        [Description("Não Utiliza")]
        NaoUtiliza = -1,

        [Description("Envio NF-e")]
        EnvioNFe = 0,

        [Description("Envio NFS-e")]
        EnvioNfse,

        Contador
    }
}
