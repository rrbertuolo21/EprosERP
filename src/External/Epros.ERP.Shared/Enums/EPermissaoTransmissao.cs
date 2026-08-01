using System.ComponentModel;

namespace Epros.ERP.Shared.Enums
{
    public enum EPermissaoTransmissao
    {
        [Description("Não utiliza")]
        NaoUtiliza = -1,

        [Description("CPF")]
        Cpf = 1,

        [Description("CNPJ")]
        Cnpj = 2,

        [Description("Ambos")]
        Ambos = 3,
    }
}
