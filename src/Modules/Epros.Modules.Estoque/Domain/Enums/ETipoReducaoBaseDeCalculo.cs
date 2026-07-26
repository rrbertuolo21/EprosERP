using System.ComponentModel;

namespace Epros.Modules.Estoque.Domain.Enums
{
    /// <summary>
    /// Tipo de redução da base de cálculo. Porte fiel do enum ETipoReducaoBaseDeCalculo (compartilhado com o módulo Fiscal),
    /// replicado aqui para manter o módulo Estoque autocontido (sem referência cruzada de projeto).
    /// </summary>
    public enum ETipoReducaoBaseDeCalculo
    {
        [Description("Não Utiliza")]
        NaoUtiliza = 0,

        [Description("Em = (-) subtração da base calculo")]
        Em = 1,

        [Description("Para = (*) multiplicação da base calculo")]
        Para = 2
    }
}
