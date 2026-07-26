using System.ComponentModel;

namespace Epros.Modules.Estoque.Domain.Enums
{
    /// <summary>
    /// Situação da entrada de mercadorias (EF Logística de Entrada §9 e §11).
    /// Estados consolidados a partir do fluxo disponível no material.
    /// </summary>
    public enum ESituacaoEntradaLogistica
    {
        [Description("Rascunho")]
        Rascunho = 0,

        [Description("Confirmado")]
        Confirmado = 1,

        [Description("Faturado")]
        Faturado = 2,

        [Description("Cancelado")]
        Cancelado = 3,

        [Description("Estornado")]
        Estornado = 4
    }
}
