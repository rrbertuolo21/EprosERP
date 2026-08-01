using Epros.ERP.Shared.Validations.Documentos;
using Flunt.Notifications;
using Flunt.Validations;
using NFe.Classes.Informacoes.Emitente;
using ProsisPDV.Domain.ValueObjects.Geral;

namespace Epros.ERP.DfeCalculos.Models.Vendas
{
    public class VendaEmitente : Notifiable<Notification>
    {
        public VendaEmitente() { }

        public VendaEmitente(string documento, CRT regimeTributario, string razaoSocial, string nomeFantasia, string ie, string im, string cnae, string telefone, string? linkLogoMarca, VendaEmitenteEndereco endereco)
        {
            Documento = (string.IsNullOrEmpty(documento) ? null : TextoHelper.RemoveMascaras(documento));
            RegimeTributario = regimeTributario;
            RazaoSocial = (string.IsNullOrEmpty(razaoSocial) ? null : razaoSocial);
            NomeFantasia = (string.IsNullOrEmpty(nomeFantasia) ? null : nomeFantasia);
            Ie = (string.IsNullOrEmpty(ie) ? null : TextoHelper.RemoveMascaras(ie));
            Im = (string.IsNullOrEmpty(im) ? null : TextoHelper.RemoveMascaras(im));
            Cnae = (string.IsNullOrEmpty(cnae) ? null : TextoHelper.RemoveMascaras(cnae));
            Telefone = (string.IsNullOrEmpty(telefone) ? null : TextoHelper.RemoveMascaras(telefone));
            LinkLogoMarca = (string.IsNullOrEmpty(linkLogoMarca) ? null : linkLogoMarca);
            Endereco = endereco;
            Validar();

        }

        public long Id { get; set; }
        public string? Documento { get; private set; }
        public CRT RegimeTributario { get; private set; }
        public string? RazaoSocial { get; private set; }
        public string? NomeFantasia { get; private set; }
        public string? Ie { get; private set; }
        public string? Im { get; private set; }
        public string? Cnae { get; private set; }
        public string? Telefone { get; private set; }
        public string? LinkLogoMarca { get; private set; }

        public VendaEmitenteEndereco Endereco { get; private set; } = null!;

        public void Validar()
        {
            AddNotifications(new Contract<Notification>()
                .Requires()
                .IsBetween((RazaoSocial ?? "").Length, 2, 60, "RazaoSocial", "Razão Social do emitente, deve conter entre 2 e 60 caractes")

                .IsLowerOrEqualsThan(60, (NomeFantasia ?? "").Length, "NomeFantasia", "Nome Fantasia emitente pode conter no max 60 caracteres")

                //.Requires()
                //.IsBetween((Ie ?? "").Length, 2, 14, "Ie", "IE emitente pode conter entre 2 e 14 caracteres")

                .IsLowerOrEqualsThan(15, (Im ?? "").Length, "Im", "IM emitente pode conter no max 15 caracteres")

                .IsLowerOrEqualsThan(7, (Cnae ?? "").Length, "Cnae", "CNAE emitente pode conter no max 7 caracteres")

                .IsTrue((Telefone ?? "").Length == 0 || ((Telefone ?? "").Length >= 6 && (Telefone ?? "").Length <= 14), "Telefone", "Telefone emitente pode conter entre 6 e 14 caracteres")
            );

            if (Endereco?.Notifications?.Count() > 0)
                AddNotifications(Endereco.Notifications);

            if (!Enum.TryParse(RegimeTributario.GetHashCode().ToString(), out CRT modeloDocumento))
                AddNotification("RegimeTributario", $"Regime tributario do emitente inválido");

            if ((Documento ?? "").Length != 11 && ((Documento ?? "").Length != 11 && (Documento ?? "").Length != 14) && !CPFValidacao.Validar(Documento!))
                AddNotification("Documento", "CPF/CNPJ do emitente c/ qtde caracteres inválido");

            if (((Documento ?? "").Length > 0 && (Documento ?? "").Length == 11) && !CPFValidacao.Validar(Documento!))
                AddNotification("Documento", "CPF do emitente inválido");

            if (((Documento ?? "").Length > 0 && (Documento ?? "").Length == 14) && !CNPJValidacao.Validar(Documento!))
                AddNotification("Documento", "CNPJ do emitente inválido");
        }
    }
}
