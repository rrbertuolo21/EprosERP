namespace Epros.ERP.DfeCalculos.Dtos.V1.Autorizacao
{
    public class DestinatatioEnederecoDto
    {
        public DestinatatioEnederecoDto() { }
        public DestinatatioEnederecoDto(string uf, string logradouro, string numero, string? complemento, string bairro, int codMunicipioIbge, string nomeMunicipio, string cep, int codPais, string nomePais)
        {
            Uf = uf;
            Logradouro = logradouro;
            Numero = numero;
            Complemento = complemento;
            Bairro = bairro;
            CodMunicipioIbge = codMunicipioIbge;
            NomeMunicipio = nomeMunicipio;
            Cep = cep;
            CodPais = codPais;
            NomePais = nomePais;
        }

        public string Uf { get; set; } = null!;
        public string Logradouro { get; set; } = null!;
        public string Numero { get; set; } = null!;
        public string? Complemento { get; set; }
        public string Bairro { get; set; } = null!;
        public int CodMunicipioIbge { get; set; }
        public string NomeMunicipio { get; set; } = null!;
        public string Cep { get; set; } = null!;
        public int CodPais { get; set; }
        public string NomePais { get; set; } = null!;
    }
}
