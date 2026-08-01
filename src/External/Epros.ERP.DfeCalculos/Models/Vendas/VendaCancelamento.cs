using Flunt.Notifications;
using Flunt.Validations;

namespace Epros.ERP.DfeCalculos.Models.Vendas
{
    public class VendaCancelamento : Notifiable<Notification>
    {
        public VendaCancelamento(string? descricao)
        {
            Descricao = descricao;
            Validar();
        }

        public string? Descricao { get; private set; }


        public void Validar()
        {
            AddNotifications(new Contract<Notification>()
                .Requires()
                .IsGreaterOrEqualsThan(5000, (Descricao ?? "").Length, "Descricao", "Descrição para cancelamento da NF é obrigatóri e pode conter até 5000 caracteres")
                );
        }
    }
}
