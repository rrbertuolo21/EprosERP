using System.ComponentModel;

namespace Epros.Shared.Domain.Enums
{
    public enum ETipoMovimento
    {
        Entrada = 1,

        [Description("Saída")]
        Saida = 2
    }
}
