using System.ComponentModel;

namespace Epros.Modules.Estoque.Domain.Enums
{
    /// <summary>
    /// Indicador de pagamento (à vista/à prazo). Porte fiel do legado
    /// Epros.ERP.Shared.Enums.EIndicadorPagamento.
    /// </summary>
    public enum EIndicadorPagamento
    {
        [Description("Pagamento à vista")]
        PagamentoAVista = 0,

        [Description("Pagamento à prazo")]
        PagamentoAPrazo = 1,

        Outros = 2
    }
}
