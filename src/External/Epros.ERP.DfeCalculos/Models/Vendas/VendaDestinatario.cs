using Epros.ERP.Shared.Validations.Documentos;
using Flunt.Notifications;
using Flunt.Validations;
using NFe.Classes.Informacoes.Destinatario;
using NFe.Classes.Informacoes.Identificacao.Tipos;
using ProsisPDV.Domain.ValueObjects.Geral;

namespace Epros.ERP.DfeCalculos.Models.Vendas
{
    public class VendaDestinatario : Notifiable<Notification>
    {
        public VendaDestinatario() { }

        public VendaDestinatario(string documento, string nome, string nomeFantasia, string ie, string im, string inscricaoSuframa, indIEDest idIeDest, string email, string telefone, ConsumidorFinal consumidorFinal, string? idEstrangeiro, VendaDestinatarioEndereco endereco)
        {
            Documento = string.IsNullOrEmpty(documento) ? null : TextoHelper.RemoveMascaras(documento);
            Nome = string.IsNullOrEmpty(nome) ? null : nome;
            NomeFantasia = string.IsNullOrEmpty(nomeFantasia) ? null : nomeFantasia;
            Ie = string.IsNullOrEmpty(ie) ? null : TextoHelper.RemoveMascaras(ie);
            Im = string.IsNullOrEmpty(im) ? null : TextoHelper.RemoveMascaras(im);
            InscricaoSuframa = string.IsNullOrEmpty(inscricaoSuframa) ? null : TextoHelper.RemoveMascaras(inscricaoSuframa);
            IdIeDest = (idIeDest == 0 ? null : idIeDest);
            Email = string.IsNullOrEmpty(email) ? null : email;
            Telefone = string.IsNullOrEmpty(telefone) ? null : TextoHelper.RemoveMascaras(telefone);
            ConsumidorFinal = consumidorFinal;
            IdEstrangeiro = string.IsNullOrEmpty(idEstrangeiro) ? null : idEstrangeiro;
            Endereco = endereco;
            Validar();
        }

        public long Id { get; set; }
        public string? Documento { get; private set; }
        public string? Nome { get; private set; }
        public string? NomeFantasia { get; private set; }
        public string? Ie { get; private set; }
        public string? Im { get; private set; }
        public string? InscricaoSuframa { get; private set; }
        public indIEDest? IdIeDest { get; set; }
        public string? Email { get; private set; }
        public string? Telefone { get; private set; }
        public ConsumidorFinal ConsumidorFinal { get; private set; }
        public string? IdEstrangeiro { get; private set; }

        public VendaDestinatarioEndereco Endereco { get; private set; } = null!;

        public void Validar()
        {
            AddNotifications(new Contract<Notification>()

            .IsLowerOrEqualsThan(120, (Nome ?? "").Length, "Nome", "Nome destinatário pode conter no max 120 caracteres")

            .IsTrue(((Ie ?? "").Length == 0) || ((Ie ?? "").Length > 1 && (Ie ?? "").Length < 15), "Ie", "IE destinatário pode conter entre 2 e 14 caracteres")

            .IsLowerOrEqualsThan(15, (Im ?? "").Length, "Im", "IM destinatário pode conter no max 15 caracteres")

            .IsLowerOrEqualsThan(9, (InscricaoSuframa ?? "").Length, "InscricaoSuframa", "IsUf (Insc. Suframa) destinatário pode conter no max 9 caracteres")

            .IsLowerOrEqualsThan(60, (Email ?? "").Length, "Email", "E-mail destinatário pode conter no max 60 caracteres")

            .IsEmailOrEmpty(Email, "Email", "E-mail destinatário inválido")

            .Requires()
            .IsTrue((IdIeDest == indIEDest.ContribuinteICMS || IdIeDest == indIEDest.Isento || IdIeDest == indIEDest.NaoContribuinte), "IdIeDest", "IdIeDest destinatário inválido ops: [1, 2, 9]")
            );

            if (Endereco?.Notifications?.Count() > 0)
                AddNotifications(Endereco.Notifications);

            if (((Documento ?? "").Length > 0 && (Documento!.Length != 11 && Documento!.Length != 14)) && !CPFValidacao.Validar(Documento!))
                AddNotification("Documento", "CPF/CNPJ do destinatário c/ qtde caracteres inválido");

            if (((Documento ?? "").Length > 0 && Documento!.Length == 11) && !CPFValidacao.Validar(Documento!))
                AddNotification("Documento", "CPF do destinatário inválido");

            if (((Documento ?? "").Length > 0 && Documento!.Length == 14) && !CNPJValidacao.Validar(Documento!))
                AddNotification("Documento", "CNPJ do destinatário inválido");

            if (!Enum.IsDefined(typeof(indIEDest), IdIeDest!))
                AddNotification("IdIeDest", "ID IE do destinatário inválido ops: [1, 2, 9]");
        }
    }
}
