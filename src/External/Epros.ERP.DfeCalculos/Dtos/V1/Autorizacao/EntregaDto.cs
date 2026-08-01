namespace Epros.ERP.DfeCalculos.Dtos.V1.Autorizacao
{
    public class EntregaDto
    {
        public EntregaDto() { }
        public EntregaDto(string nome, string fone, string email, string iE, string? documento, string logradouro, string numero, string? complemento, string bairro, int codMunicipioIbge, string nomeMunicipio, string uf, string cep, int paisId, string? paisNome)
        {
            Nome = nome;
            Fone = fone;
            Email = email;
            //IE = iE;  // não vamos precisar por enquanto não descomentar ver com Luciano - Não existe essa tag no xml
            Documento = documento;
            Logradouro = logradouro;
            Numero = numero;
            Complemento = complemento;
            Bairro = bairro;
            CodMunicipioIbge = codMunicipioIbge;
            NomeMunicipio = nomeMunicipio;
            Uf = uf;
            Cep = cep;
            PaisId = paisId;
            PaisNome = paisNome;
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
        public string Uf { get; set; } = null!;
        public string Cep { get; set; } = null!;
        public int PaisId { get; set; }
        public string? PaisNome { get; set; }
    }
}
