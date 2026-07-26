namespace Epros.ERP.DfeCalculos.Dtos.V1.Autorizacao
{
    public class EmitenteEnederecoDto
    {
        public EmitenteEnederecoDto() { }
        public EmitenteEnederecoDto(string uf, string logradouro, string numero, string? complemento, string bairro, int codMunicipioIbge, string nomeMunicipio, string cep)
        {
            Uf = uf;
            Logradouro = logradouro;
            Numero = numero;
            Complemento = complemento;
            Bairro = bairro;
            CodMunicipioIbge = codMunicipioIbge;
            NomeMunicipio = nomeMunicipio;
            Cep = cep;
        }

        public string Uf { get; set; } = null!;
        public string Logradouro { get; set; } = null!;
        public string Numero { get; set; } = null!;
        public string? Complemento { get; set; }
        public string Bairro { get; set; } = null!;
        public int CodMunicipioIbge { get; set; }
        public string NomeMunicipio { get; set; } = null!;
        public string Cep { get; set; } = null!;
    }
}