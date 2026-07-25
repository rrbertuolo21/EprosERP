using Flunt.Notifications;

namespace Epros.ERP.DfeCalculos.Models.Vendas
{
    public class VendaCobranca : Notifiable<Notification>
    {
        public VendaCobranca() { }
        public VendaCobranca(VendaCobrancaFatura fatura, ICollection<VendaCobrancaDuplicata> duplicatas)
        {
            Fatura = fatura;
            Duplicatas = duplicatas;
        }

        public VendaCobrancaFatura Fatura { get; private set; } = null!;
        public ICollection<VendaCobrancaDuplicata> Duplicatas { get; private set; } = null!;
    }
}
