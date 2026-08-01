using System.ComponentModel;

namespace Epros.ERP.Shared.Enums
{
    public enum ESituacao
    {
        Aberto = 1,
        Pago = 2,

        [Description("Pago Parcial")]
        PagoParcial = 3,
        Cancelado = 4
    }
}
