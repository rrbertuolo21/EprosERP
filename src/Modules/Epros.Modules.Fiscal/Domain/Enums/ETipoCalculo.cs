using System.ComponentModel;

namespace Epros.Modules.Fiscal.Domain.Enums
{
    public enum ETipoCalculo
    {
        [Description("Margem Agregada")]
        MargemAgregada = 0,

        [Description("Valor Fixo")]
        ValorFixo = 1
    }
}
