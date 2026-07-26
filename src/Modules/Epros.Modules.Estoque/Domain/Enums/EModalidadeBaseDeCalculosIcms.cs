using System.ComponentModel;

namespace Epros.Modules.Estoque.Domain.Enums
{
    /// <summary>
    /// Modalidade de determinação da base de cálculo do ICMS. Porte fiel do legado Epros.ERP.Shared.Enums.EModalidadeBaseDeCalculosIcms.
    /// </summary>
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
