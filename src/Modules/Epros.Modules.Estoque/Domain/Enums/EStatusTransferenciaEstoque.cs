using System.ComponentModel;

namespace Epros.Modules.Estoque.Domain.Enums
{
    /// <summary>
    /// Situação de uma transferência de estoque entre locais.
    /// EF Movimentação Manual e Ajustes §9 e §11 (Rascunho, Confirmada, Recebida, Cancelada, Estornada).
    /// </summary>
    public enum EStatusTransferenciaEstoque
    {
        [Description("Rascunho")]
        Rascunho = 0,

        [Description("Confirmada")]
        Confirmada = 1,

        [Description("Recebida")]
        Recebida = 2,

        [Description("Cancelada")]
        Cancelada = 3,

        [Description("Estornada")]
        Estornada = 4
    }
}
