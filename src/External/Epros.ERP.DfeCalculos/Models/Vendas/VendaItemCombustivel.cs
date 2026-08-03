using DFe.Classes.Entidades;
using Flunt.Notifications;

namespace Epros.ERP.DfeCalculos.Models.Vendas
{
    public class VendaItemCombustivel : Notifiable<Notification>
    {
        public VendaItemCombustivel() { }

        public string? CodigoAnp { get; private set; }
        public string? DescricaoAnp { get; private set; }
        public decimal QuantidadeCombustivelFaturada { get; private set; }
        public Estado UfConsumo { get; private set; }
        public decimal PercentualGlpDerivadoPetroleo { get; private set; }
        public decimal PercentualGasNaturalNacional { get; private set; }
        public decimal PercentualGasNaturalImportado { get; private set; }
        public decimal ValorPartida { get; private set; }

        public ICollection<VendaItenCombustivelOrigem> CombustivelOrigens { get; private set; } = null!;

        public void Validar()
        {
            //AddNotifications(new Contract<Notification>()

            //.IsLowerOrEqualsThan(120, (Nome ?? "").Length, "Nome", "Nome destinatário pode conter no max 120 caracteres")

            //.IsTrue(((Ie ?? "").Length == 0) || ((Ie ?? "").Length > 1 && (Ie ?? "").Length < 15), "Ie", "IE destinatário pode conter entre 2 e 14 caracteres")

            //.IsLowerOrEqualsThan(15, (Im ?? "").Length, "Im", "IM destinatário pode conter no max 15 caracteres")

            //.IsLowerOrEqualsThan(9, (InscricaoSuframa ?? "").Length, "InscricaoSuframa", "IsUf (Insc. Suframa) destinatário pode conter no max 9 caracteres")

            //.IsLowerOrEqualsThan(60, (Email ?? "").Length, "Email", "E-mail destinatário pode conter no max 60 caracteres")

            //.IsEmailOrEmpty(Email, "Email", "E-mail destinatário inválido")

            //.Requires()
            //.IsTrue((IdIeDest == indIEDest.ContribuinteICMS || IdIeDest == indIEDest.Isento || IdIeDest == indIEDest.NaoContribuinte), "IdIeDest", "IdIeDest destinatário inválido ops: [1, 2, 9]")
            //);

            //if (Endereco?.Notifications?.Count() > 0)
            //    AddNotifications(Endereco.Notifications);

            //if (((Documento ?? "").Length > 0 && (Documento.Length != 11 && Documento.Length != 14)) && !CPFValidacao.Validar(Documento))
            //    AddNotification("Documento", "CPF/CNPJ do destinatário c/ qtde caracteres inválido");

            //if (((Documento ?? "").Length > 0 && Documento.Length == 11) && !CPFValidacao.Validar(Documento))
            //    AddNotification("Documento", "CPF do destinatário inválido");

            //if (((Documento ?? "").Length > 0 && Documento.Length == 14) && !CNPJValidacao.Validar(Documento))
            //    AddNotification("Documento", "CNPJ do destinatário inválido");           

            //if (!Enum.IsDefined(typeof(indIEDest), IdIeDest))
            //    AddNotification("IdIeDest", "ID IE do destinatário inválido ops: [1, 2, 9]");
        }
    }

    public class VendaItenCombustivelOrigem : Notifiable<Notification>
    {
        public int IndicadorImportacao { get; private set; }
        public Estado UfOrigem { get; private set; }
        public decimal PercentualOrigem { get; private set; }
    }
}
