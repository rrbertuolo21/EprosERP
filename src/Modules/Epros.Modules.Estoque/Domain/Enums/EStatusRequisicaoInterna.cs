using System.ComponentModel;

namespace Epros.Modules.Estoque.Domain.Enums
{
    /// <summary>
    /// Situação de uma requisição interna. Os valores não foram detalhados no material
    /// (EF Movimentação Manual e Ajustes §9); consolidado minimamente para controle de fluxo.
    /// </summary>
    public enum EStatusRequisicaoInterna
    {
        [Description("Rascunho")]
        Rascunho = 0,

        [Description("Confirmada")]
        Confirmada = 1,

        [Description("Atendida")]
        Atendida = 2,

        [Description("Cancelada")]
        Cancelada = 3
    }
}
