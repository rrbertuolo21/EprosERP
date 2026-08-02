using System.ComponentModel;

namespace Epros.Modules.Estoque.Domain.Enums
{
    /// <summary>Ciclo do alerta de estoque (EF Análise e Planejamento §16.3 `status_alerta`).</summary>
    public enum EStatusAlertaEstoque
    {
        [Description("Aberto")] Aberto = 0,
        [Description("Ignorado")] Ignorado = 1,
        [Description("Resolvido")] Resolvido = 2
    }
}
