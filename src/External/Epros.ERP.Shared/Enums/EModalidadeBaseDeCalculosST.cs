using System.ComponentModel;

namespace Epros.ERP.Shared.Enums
{
    public enum EModalidadeBaseDeCalculosST
    {
        [Description("Preço tabelado ou máximo sugerido")]
        PrecoTabelaOuMaximoSugerido = 0,
        [Description("Lista Negativa (valor)")]
        ListaNegativaValor = 1,
        [Description("Lista Positiva (valor)")]
        ListaPositivaValor = 2,
        [Description("Lista Neutra (valor)")]
        ListaNeutraValor = 3,
        [Description("Margem Valor Agregado (%)")]
        MargemValorAgregado = 4,
        [Description(" Pauta (valor)")]
        PautaValor = 5
    }
}
