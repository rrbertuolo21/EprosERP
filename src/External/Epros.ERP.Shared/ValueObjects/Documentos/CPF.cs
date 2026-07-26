using Epros.ERP.Shared.Formatting.Documentos;
using Epros.ERP.Shared.Validations.Documentos;
using Flunt.Notifications;
using ProsisPDV.Domain.ValueObjects.Geral;

namespace Epros.ERP.Shared.ValueObjects.Documentos
{
    public class CPF : Notifiable<Notification>
    {
        protected CPF() { }

        public CPF(string numero)
        {
            Numero = (TextoHelper.RemoveMascaras(numero) ?? "").Trim();
            Validar();
        }

        public string Numero { get; private set; } = null!;
        public string NumeroFormatado { get { return FormatarNumero(); } }
        public void Validar()
        {
            if (!CPFValidacao.Validar(Numero))
                AddNotification("Numero", "CPF inválido");
        }

        private string FormatarNumero()
        {
            return string.IsNullOrEmpty(Numero) ? "" : CnpjCpfFormatacao.Formatar(Numero);
        }

        public override string ToString()
        {
            return Numero;
        }
    }
}
