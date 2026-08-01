using DFe.Classes.Entidades;
using Flunt.Notifications;

namespace Epros.ERP.DfeCalculos.Models.Vendas
{
    public class VendaTransporteTransportadora : Notifiable<Notification>
    {
        public VendaTransporteTransportadora() { }
        public VendaTransporteTransportadora(string cnpj, string cpf, string nome, string ie, string uf, string endereco, string municipio)
        {
            Enum.TryParse(uf, true, out Estado estado);

            Cnpj = string.IsNullOrEmpty(cnpj) ? null : cnpj;
            Cpf = string.IsNullOrEmpty(cpf) ? null : cpf;
            Nome = string.IsNullOrEmpty(nome) ? null : nome;
            Ie = string.IsNullOrEmpty(ie) ? null : ie;
            Uf = (estado == 0 ? null : estado);
            Endereco = string.IsNullOrEmpty(endereco) ? null : endereco;
            Municipio = string.IsNullOrEmpty(municipio) ? null : municipio;
        }

        public string? Cnpj { get; private set; }
        public string? Cpf { get; private set; }
        public string? Nome { get; private set; }
        public string? Ie { get; private set; }
        public Estado? Uf { get; private set; }
        public string? Endereco { get; private set; }
        public string? Municipio { get; private set; }
    }
}
