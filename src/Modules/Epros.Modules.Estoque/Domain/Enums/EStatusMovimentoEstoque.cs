using System.ComponentModel;

namespace Epros.Modules.Estoque.Domain.Enums
{
    /// <summary>
    /// Situação de um movimento manual de estoque. Controla a reversão (estorno) conforme
    /// EF Movimentação Manual e Ajustes §9 e §11 (Rascunho, Aplicado, Cancelado, Estornado).
    /// </summary>
    public enum EStatusMovimentoEstoque
    {
        [Description("Rascunho")]
        Rascunho = 0,

        [Description("Aplicado")]
        Aplicado = 1,

        [Description("Cancelado")]
        Cancelado = 2,

        [Description("Estornado")]
        Estornado = 3
    }
}
