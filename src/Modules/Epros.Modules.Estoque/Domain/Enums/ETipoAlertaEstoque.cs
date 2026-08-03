using System.ComponentModel;

namespace Epros.Modules.Estoque.Domain.Enums
{
    /// <summary>Tipo de alerta de planejamento (EF Análise e Planejamento §16.3 `tipo_alerta`).</summary>
    public enum ETipoAlertaEstoque
    {
        [Description("Reposição")] Reposicao = 0,
        [Description("Excesso de máximo")] ExcessoMaximo = 1
    }
}
