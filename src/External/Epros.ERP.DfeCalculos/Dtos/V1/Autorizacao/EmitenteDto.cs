namespace Epros.ERP.DfeCalculos.Dtos.V1.Autorizacao
{
    public class EmitenteDto
    {
        public EmitenteDto() { }
        public EmitenteDto(string documento, int regimeTributario, string razaoSocial, string? nomeFantasia, string ie, string? im, string? cnae, string? telefone, string? logoMarca)
        {
            Documento = documento;
            RegimeTributario = regimeTributario;
            RazaoSocial = razaoSocial;
            NomeFantasia = nomeFantasia;
            Ie = ie;
            Im = im;
            Cnae = cnae;
            Telefone = telefone;
            LogoMarca = logoMarca;
        }

        public string Documento { get; set; } = null!;
        public int RegimeTributario { get; set; }
        public string RazaoSocial { get; set; } = null!;
        public string? NomeFantasia { get; set; }
        public string Ie { get; set; } = null!;
        public string? Im { get; set; }
        public string? Cnae { get; set; }
        public string? Telefone { get; set; }
        public string? LogoMarca { get; set; }
        public EmitenteEnederecoDto Endereco { get; set; } = null!;
    }
}
