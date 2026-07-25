using System.ComponentModel;

namespace Epros.Modules.Estoque.Domain.Enums
{
    /// <summary>
    /// Indicador de finalidade da operação (consumidor final). Porte fiel do legado
    /// Epros.ERP.Shared.Enums.EIndicadorFinalidadeOperacao.
    /// </summary>
    public enum EIndicadorFinalidadeOperacao
    {
        [Description("Operação normal")]
        OperacaoSemConsumidorFinal = 0,

        [Description("Operação com Consumidor Final")]
        OperacaoComConsumidorFinal = 1
    }
}
