using System.ComponentModel;

namespace Epros.Modules.Estoque.Domain.Enums
{
    /// <summary>
    /// Tipo de inventário físico (EF Inventário Físico e Contagem Cíclica §9).
    /// Domínio consolidado para validação: inventário geral, cíclico (curva ABC/agenda),
    /// parcial e por recorte (produto/local).
    /// </summary>
    public enum ETipoInventario
    {
        [Description("Geral")]
        Geral = 0,

        [Description("Cíclico")]
        Ciclico = 1,

        [Description("Parcial")]
        Parcial = 2,

        [Description("Por produto")]
        PorProduto = 3,

        [Description("Por local")]
        PorLocal = 4
    }
}
