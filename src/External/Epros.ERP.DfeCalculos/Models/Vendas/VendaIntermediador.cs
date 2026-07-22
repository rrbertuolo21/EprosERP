using Flunt.Notifications;

namespace Epros.ERP.DfeCalculos.Models.Vendas
{
    public class VendaIntermediador : Notifiable<Notification>
    {
        public VendaIntermediador() { }
        public VendaIntermediador(string documento, string idCadIntTran)
        {
            Documento = documento;
            IdCadIntTran = idCadIntTran;
        }

        public string Documento { get; private set; } = null!;
        public string IdCadIntTran { get; private set; } = null!;
    }
}
