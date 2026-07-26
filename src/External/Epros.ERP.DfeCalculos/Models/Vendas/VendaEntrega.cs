using DFe.Classes.Entidades;
using Flunt.Notifications;
using ProsisPDV.Domain.ValueObjects.Geral;

namespace Epros.ERP.DfeCalculos.Models.Vendas
{
    public class VendaEntrega : Notifiable<Notification>
    {
        public VendaEntrega() { }

        public VendaEntrega(string nome, string fone, string email, string? iE, string? documento, string logradouro, string numero, string? complemento, string bairro, int codMunicipioIbge, string nomeMunicipio, Estado uf, string cep, int codPais, string? nomePais)
        {
            Nome = string.IsNullOrEmpty(nome) ? null! : nome;
            Fone = string.IsNullOrEmpty(fone) ? null! : fone;
            Email = string.IsNullOrEmpty(email) ? null! : email;
            IE = string.IsNullOrEmpty(iE) ? null : iE;
            Documento = string.IsNullOrEmpty(documento) ? null : documento;

            Uf = uf;
            Logradouro = string.IsNullOrEmpty(logradouro) ? null! : logradouro;
            Numero = string.IsNullOrEmpty(numero) ? null! : numero;
            Complemento = string.IsNullOrEmpty(complemento) ? null : complemento;
            Bairro = string.IsNullOrEmpty(bairro) ? null! : bairro;
            CodMunicipioIbge = codMunicipioIbge;
            NomeMunicipio = string.IsNullOrEmpty(nomeMunicipio) ? null! : nomeMunicipio;
            Cep = string.IsNullOrEmpty(cep) ? null! : TextoHelper.RemoveMascaras(cep);
            CodPais = codPais == 0 ? 1058 : codPais;
            NomePais = string.IsNullOrEmpty(nomePais) ? "BRASIL" : nomePais;
        }

        public string Nome { get; set; } = null!;
        public string Fone { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? IE { get; set; }
        public string? Documento { get; set; }
        public string Logradouro { get; set; } = null!;
        public string Numero { get; set; } = null!;
        public string? Complemento { get; set; }
        public string Bairro { get; set; } = null!;
        public int CodMunicipioIbge { get; set; }
        public string NomeMunicipio { get; set; } = null!;
        public Estado Uf { get; set; }
        public string Cep { get; set; } = null!;
        public int CodPais { get; set; }
        public string? NomePais { get; set; }
    }
}