using DFe.Classes.Entidades;
using Flunt.Notifications;
using Flunt.Validations;
using ProsisPDV.Domain.ValueObjects.Geral;

namespace Epros.ERP.DfeCalculos.Models.Vendas
{
    public class VendaDestinatarioEndereco : Notifiable<Notification>
    {
        public VendaDestinatarioEndereco() { }
        public VendaDestinatarioEndereco(Estado uf, string logradouro, string numero, string complemento, string bairro, int codMunicipioIbge, string nomeMunicipio, string cep, int codPais = 1058, string nomePais = "BRASIL")
        {
            Uf = uf;
            Logradouro = string.IsNullOrEmpty(logradouro) ? null : logradouro;
            Numero = string.IsNullOrEmpty(numero) ? null : numero;
            Complemento = string.IsNullOrEmpty(complemento) ? null : complemento;
            Bairro = string.IsNullOrEmpty(bairro) ? null : bairro;
            CodMunicipioIbge = codMunicipioIbge;
            NomeMunicipio = string.IsNullOrEmpty(nomeMunicipio) ? null : nomeMunicipio;
            Cep = string.IsNullOrEmpty(cep) ? null : TextoHelper.RemoveMascaras(cep);
            CodPais = codPais == 0 ? 1058 : codPais;
            NomePais = string.IsNullOrEmpty(nomePais) ? "BRASIL" : nomePais;
            Validar();
        }

        public Estado Uf { get; private set; }
        public string? Logradouro { get; private set; }
        public string? Numero { get; private set; }
        public string? Complemento { get; private set; }
        public string? Bairro { get; private set; }
        public int CodMunicipioIbge { get; private set; }
        public string? NomeMunicipio { get; private set; }
        public string? Cep { get; private set; }
        public int CodPais { get; private set; }
        public string? NomePais { get; private set; }

        public void Validar()
        {
            AddNotifications(new Contract<Notification>()
            .Requires()
            .IsBetween((Logradouro ?? "").Length, 2, 60, "Logradouro", "Logradouro do endereço do destinatario, deve conter entre 2 e 60 caractes")

            .Requires()
            .IsBetween((Numero ?? "").Length, 1, 60, "Numero", "Numero do endereço do destinatario, deve conter entre 1 e 60 caractes")

            .IsTrue((Complemento ?? "").Length == 0 || (Complemento ?? "").Length >= 1 && (Complemento ?? "").Length <= 60, "Complemento", "Complemento do endereço do destinatario, deve conter entre 1 e 60 caractes")

            .Requires()
            .IsBetween((Bairro ?? "").Length, 2, 60, "Bairro", "Bairro do endereço do destinatario, deve conter entre 2 e 60 caractes")

            .Requires()
            .IsBetween((NomeMunicipio ?? "").Length, 2, 60, "NomeMunicipio", "Municipio do endereço do destinatario, deve conter entre 2 e 60 caractes")

            .Requires()
            .IsTrue((Cep ?? "").Length == 8, "Cep", "CEP do endereço do destinatario, deve conter 8 caractes")

            .Requires()
            .IsBetween(CodPais.ToString().Length, 1, 4, "Cep", "Código do Pais do endereço do destinatario, deve conter entre 1 e 4 caractes")

            .Requires()
            .IsBetween((NomePais ?? "").Length, 2, 60, "NomePais", "Nome do Pais do endereço do destinatario, deve conter entre 2 e 60 caractes")

            );

            if (!Enum.TryParse(Uf.GetHashCode().ToString(), out Estado estado))
                AddNotification("Uf", $"UF do endereço do destinatario inválido");
        }
    }
}
