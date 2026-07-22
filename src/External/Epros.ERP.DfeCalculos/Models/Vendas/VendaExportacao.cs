using Flunt.Notifications;

namespace Epros.ERP.DfeCalculos.Models.Vendas
{
    public class VendaExportacao : Notifiable<Notification>
    {
        public VendaExportacao() { }

        public VendaExportacao(string ufSaidaPais, string localExportacao, string localDespacho)
        {
            UfSaidaPais = ufSaidaPais;
            LocalExportacao = localExportacao;
            LocalDespacho = localDespacho;
        }

        public string UfSaidaPais { get; private set; } = null!;
        public string LocalExportacao { get; private set; } = null!;
        public string LocalDespacho { get; private set; } = null!;
    }

}