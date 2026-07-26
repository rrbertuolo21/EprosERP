using System.ComponentModel;

namespace Epros.Modules.Estoque.Domain.Enums
{
    /// <summary>
    /// Situação genérica de registros de estoque que seguem o ciclo aplicar/estornar
    /// (ajuste, avaria, saldo inicial). Consolidado para controle de reversão conforme
    /// EF Movimentação Manual e Ajustes §11.
    /// </summary>
    public enum EStatusRegistroEstoque
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
