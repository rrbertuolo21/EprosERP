namespace Epros.ERP.DfeCalculos.Dtos.V1.Autorizacao
{
    public class TransporteTransportadoraDto
    {
        public TransporteTransportadoraDto() { }

        public TransporteTransportadoraDto(string? cnpj, string? cpf, string nome, string ie, string uf, string endereco, string municipio)
        {
            Cnpj = cnpj;
            Cpf = cpf;
            Nome = nome;
            Ie = ie;
            Uf = uf;
            Endereco = endereco;
            Municipio = municipio;
        }

        public string? Cnpj { get; set; }
        public string? Cpf { get; set; }
        public string Nome { get; set; } = null!;
        public string? Ie { get; set; }
        public string Uf { get; set; } = null!;
        public string Endereco { get; set; } = null!;
        public string Municipio { get; set; } = null!;
    }
}
