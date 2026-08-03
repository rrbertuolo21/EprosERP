using System.ComponentModel;

namespace Epros.Modules.Estoque.Domain.Enums
{
    /// <summary>
    /// Máquina de estados do documento de inventário (EF Inventário Físico e Contagem Cíclica §11):
    /// Rascunho → EmContagem → EmConferencia → Aprovado → Ajustado; Cancelado a partir dos estados iniciais.
    /// </summary>
    public enum ESituacaoInventario
    {
        [Description("Rascunho")]
        Rascunho = 0,

        [Description("Em contagem")]
        EmContagem = 1,

        [Description("Em conferência")]
        EmConferencia = 2,

        [Description("Aprovado")]
        Aprovado = 3,

        [Description("Ajustado")]
        Ajustado = 4,

        [Description("Cancelado")]
        Cancelado = 5
    }
}
