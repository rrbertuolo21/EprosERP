using System.ComponentModel;

namespace Epros.Shared.Domain.Enums
{
    /// <summary>
    /// Situação de um título financeiro (Contas a Pagar / Contas a Receber).
    /// Porte fiel do legado Epros.ERP.Shared.Enums.ESituacao.
    /// </summary>
    public enum ESituacao
    {
        Aberto = 1,
        Pago = 2,

        [Description("Pago Parcial")]
        PagoParcial = 3,
        Cancelado = 4
    }
}
