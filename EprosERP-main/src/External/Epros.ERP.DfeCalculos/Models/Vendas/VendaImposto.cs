using Flunt.Notifications;

namespace Epros.ERP.DfeCalculos.Models.Vendas
{
    public class VendaImposto : Notifiable<Notification>
    {
        public VendaImposto() { }

        public VendaImposto(decimal valorAliquotaCreditoIcms)
        {
            ValorAliquotaCreditoIcms = valorAliquotaCreditoIcms;
        }

        public decimal ValorAliquotaCreditoIcms { get; private set; }
    }
}
