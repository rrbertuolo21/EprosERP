using Epros.ERP.Shared.Formatting.Documentos;
using Flunt.Notifications;
using ProsisPDV.Domain.ValueObjects.Geral;

namespace Epros.ERP.Shared.ValueObjects.Enderecos
{
    public class CEP : Notifiable<Notification>
    {
        protected CEP() { }

        public CEP(string numero)
        {
            Numero = (TextoHelper.RemoveMascaras(numero) ?? "").Trim();
            Validar();
        }

        public string Numero { get; private set; } = null!;
        public string NumeroFormatado { get { return FormatarNumero(); } }
        public void Validar()
        {
            if (Numero.Length != 8)
                AddNotification("Numero", "CEP inválido");
        }
        private string FormatarNumero()
        {
            return string.IsNullOrEmpty(Numero) ? "" : CepFormatacao.Formatar(Numero);
        }
    }
}
