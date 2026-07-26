using System.ComponentModel;

namespace Epros.ERP.Shared.Enums
{
    public enum EIndicadorPagamento
    {
        [Description("Pagamento à vista")]
        PagamentoAVista = 0,
        [Description("Pagamento à prazo")]
        PagamentoAPrazo = 1,
        Outros = 2
    }
}
