using System.ComponentModel;

namespace Epros.Modules.Estoque.Domain.Enums
{
    /// <summary>Critério de baixa/seleção de camada na saída (EF Rastreabilidade §7.5 `criterio_baixa`).</summary>
    public enum ECriterioBaixaEstoque
    {
        [Description("PEPS")] PEPS = 0,
        [Description("UEPS")] UEPS = 1,
        [Description("FEFO")] FEFO = 2,
        [Description("Manual")] Manual = 3
    }
}
