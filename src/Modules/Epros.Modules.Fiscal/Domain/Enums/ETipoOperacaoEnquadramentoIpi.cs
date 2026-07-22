using System.ComponentModel;

namespace Epros.Modules.Fiscal.Domain.Enums
{
    public enum ETipoOperacaoEnquadramentoIpi
    {
        [Description("Imunidade")]
        Imunidade = 1,

        [Description("Suspensão")]
        Suspensao = 2,

        [Description("Isenção")]
        Isencao = 3,

        [Description("Redução")]
        Reducao = 4,

        [Description("Outros")]
        Outros = 5
    }
}
