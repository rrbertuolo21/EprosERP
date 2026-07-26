using Epros.ERP.Shared.Enums;
using Epros.ERP.Shared.Formatting.Documentos;
using Epros.ERP.Shared.Validations.Documentos;
using Flunt.Notifications;
using ProsisPDV.Domain.ValueObjects.Geral;

namespace Epros.ERP.Shared.ValueObjects.Documentos
{
    public class Documento : Notifiable<Notification>
    {
        protected Documento() { }

        public Documento(string valor)
        {
            Valor = (TextoHelper.RemoveMascaras(valor) ?? "").Trim();
            Validar();
        }

        public string Valor { get; private set; } = null!;
        public string ValorFormatado { get { return FormatarNumero(); } }
        public ETipoPessoa TipoPessoa { get { return IdentificarTipoPessoa(); } }
        public void Validar()
        {
            if (!CNPJValidacao.Validar(Valor) && !CPFValidacao.Validar(Valor))
                AddNotification("Documento", "Documento inválido, documento não é CPF e nem CNPJ");
        }

        private string FormatarNumero()
        {
            if (!IsValid) return "";
            return string.IsNullOrEmpty(Valor) ? "" : CnpjCpfFormatacao.Formatar(Valor);
        }

        private ETipoPessoa IdentificarTipoPessoa()
        {
            ETipoPessoa tipoPessoa = 0;

            if (!IsValid) return tipoPessoa;

            if (Valor.Length == 11)
                tipoPessoa = ETipoPessoa.PessoaFisica;
            else if (Valor.Length == 14)
                tipoPessoa = ETipoPessoa.PessoaJuridica;
            return tipoPessoa;
        }

        public static bool EhMatrizEFilial(Documento matriz, Documento Filial)
        {
            if (matriz == null || Filial == null)
                return false;

            if (!matriz.IsValid || !Filial.IsValid)
                return false;

            return Filial.Valor.Substring(0, 8) == matriz.Valor.Substring(0, 8);
        }
    }
}
