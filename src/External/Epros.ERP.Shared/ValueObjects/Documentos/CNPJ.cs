using Epros.ERP.Shared.Formatting.Documentos;
using Epros.ERP.Shared.Validations.Documentos;
using Flunt.Notifications;
using ProsisPDV.Domain.ValueObjects.Geral;

namespace Epros.ERP.Shared.ValueObjects.Documentos
{
    public class CNPJ : Notifiable<Notification>
    {
        protected CNPJ() { }

        public CNPJ(string valor)
        {
            Valor = (TextoHelper.RemoveMascaras(valor) ?? "").Trim();
            Validar();
        }

        public string Valor { get; private set; } = null!;
        public string ValorFormatado { get { return FormatarValor(); } }
        public void Validar()
        {
            if (!CNPJValidacao.Validar(Valor))
                AddNotification("Valor", "CNPJ inválido");
        }


        private string FormatarValor()
        {
            return string.IsNullOrEmpty(Valor) ? "" : CnpjCpfFormatacao.Formatar(Valor);
        }

        public override string ToString()
        {
            return Valor;
        }
    }
}
