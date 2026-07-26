using System.ComponentModel;

namespace Epros.Modules.Estoque.Domain.Enums
{
    /// <summary>
    /// Tipo de ajuste de estoque. Domínio observado no material; nomenclatura final na MC.
    /// EF Movimentação Manual e Ajustes §9 (Normal, Anormal).
    /// </summary>
    public enum ETipoAjusteEstoque
    {
        [Description("Normal")]
        Normal = 0,

        [Description("Anormal")]
        Anormal = 1
    }
}
