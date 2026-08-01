using System.ComponentModel;

namespace Epros.ERP.Shared.Enums
{
    public enum EModalidadeBaseDeCalculosIcms
    {
        [Description("Margem Valor Agregado (%)")]
        MargemValorAgregado = 0,
        [Description("Pauta (Valor)")]
        PautaValor = 1,
        [Description("Preço Tabelado Máx. (valor)")]
        PrecoTabelado = 2,
        [Description("Valor da operação")]
        ValorOperacao = 3
    }
}
