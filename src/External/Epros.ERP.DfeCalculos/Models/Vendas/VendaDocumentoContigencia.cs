using Flunt.Notifications;
using Flunt.Validations;
using NFe.Classes.Informacoes.Identificacao.Tipos;

namespace Epros.ERP.DfeCalculos.Models.Vendas
{
    public class VendaDocumentoContigencia : Notifiable<Notification>
    {
        protected VendaDocumentoContigencia(TipoEmissao teOffLine) { }
        public VendaDocumentoContigencia(TipoEmissao tipoEmissao, string justificativa, DateTimeOffset dataHora)
        {
            TipoEmissao = tipoEmissao;
            Justificativa = justificativa;
            DataHora = dataHora;
            Validar();
        }

        public TipoEmissao TipoEmissao { get; set; }
        public string Justificativa { get; set; } = null!;
        public DateTimeOffset DataHora { get; set; }

        public void Validar()
        {
            AddNotifications(new Contract<Notification>()
            .Requires()
            .IsBetween((Justificativa ?? "").Length, 15, 256, "Justificativa", "Justificativa da contingência obrigatório, deve conter entre 15 e 256 caractes")
            .IsFalse(DataHora == DateTime.MinValue, "Data", "Data da contingência inválida")
            );

            if (!Enum.IsDefined(typeof(TipoEmissao), TipoEmissao))
                AddNotification("TipoEmissao", $"Tipo emissão de contingência inválido");
        }
    }
}
