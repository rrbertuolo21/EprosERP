
using System.ComponentModel;

namespace Epros.ERP.Shared.Enums
{
    public enum ETipoPix
    {
        [Description("Chave Aleatória")]
        ChaveAleatoria = 1,

        [Description("CPF/CNPJ")]
        CpfCnpj,

        [Description("E-mail")]
        Email,
        Telefone
    }
}
