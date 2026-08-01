
using System.ComponentModel;

namespace Epros.ERP.Shared.Enums
{
    public enum ETipoPessoa
    {
        [Description("Não Utiliza")]
        NaoUtiliza = -1,

        [Description("Pessoa Física")]
        PessoaFisica = 1,

        [Description("Pessoa Jurídica")]
        PessoaJuridica = 2,

        [Description("Pessoa Estrangeira")]
        PessoaEstrangeira = 3
    }
}
