using System.ComponentModel;

namespace Epros.Modules.Estoque.Domain.Enums
{
    /// <summary>
    /// Status de planejamento da posição (EF Análise e Planejamento §11). Derivado do saldo vs. mínimo/máximo:
    /// Normal, EmAlertaReposicao (saldo ≤ mínimo), AcimaMaximo (saldo &gt; máximo), SemPoliticaCompleta (sem parâmetros).
    /// </summary>
    public enum EStatusPlanejamentoEstoque
    {
        [Description("Normal")] Normal = 0,
        [Description("Em alerta de reposição")] EmAlertaReposicao = 1,
        [Description("Acima do máximo")] AcimaMaximo = 2,
        [Description("Sem política completa")] SemPoliticaCompleta = 3
    }
}
