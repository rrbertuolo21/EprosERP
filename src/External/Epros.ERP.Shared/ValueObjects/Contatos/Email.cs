using Epros.ERP.Shared.Validations.Contatos;
using Flunt.Notifications;

namespace Epros.ERP.Shared.ValueObjects.Contatos
{
    public class Email : Notifiable<Notification>
    {
        protected Email() { }

        public Email(string endereco)
        {
            Endereco = (endereco ?? "").Trim();
            Validar();
        }

        public string Endereco { get; private set; } = null!;

        public void Validar()
        {
            if (!EmailValidacao.Validar(Endereco))
                AddNotification("Email", "E-mail inválido");
        }

        public override string ToString()
        {
            return Endereco;
        }
    }
}
