using System.ComponentModel;


namespace Epros.ERP.Shared.Enums
{
    public enum EIndicadorFinalidadeOperacao
    {
        [Description("Operação normal")]
        OperacaoSemConsumidorFinal = 0,
        [Description("Operação com Consumidor Final")]
        OperacaoComConsumidorFinal = 1
    }
}
