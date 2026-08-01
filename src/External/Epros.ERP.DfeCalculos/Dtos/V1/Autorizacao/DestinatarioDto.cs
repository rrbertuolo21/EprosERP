namespace Epros.ERP.DfeCalculos.Dtos.V1.Autorizacao
{
    public class DestinatarioDto
    {
        public DestinatarioDto() { }
        public DestinatarioDto(string? documento, string? nome, string? nomeFantasia, string? ie, string? im, string? inscricaoSuframa, int? idIeDestinatario, string? email, string? telefone, int consumidorFinal, string? identificadorEstrangeiro)
        {
            Documento = documento;
            Nome = nome;
            NomeFantasia = nomeFantasia;
            Ie = ie;
            Im = im;
            InscricaoSuframa = inscricaoSuframa;
            IdIeDestinatario = idIeDestinatario;
            Email = email;
            Telefone = telefone;
            ConsumidorFinal = consumidorFinal;
            IdentificadorEstrangeiro = identificadorEstrangeiro;
        }

        public string? Documento { get; set; }
        public string? Nome { get; set; }
        public string? NomeFantasia { get; set; }
        public string? Ie { get; set; }
        public string? Im { get; set; }
        public string? InscricaoSuframa { get; set; }
        public int? IdIeDestinatario { get; set; }
        public string? Email { get; set; }
        public string? Telefone { get; set; }
        public int ConsumidorFinal { get; set; }
        public string? IdentificadorEstrangeiro { get; set; }

        public DestinatatioEnederecoDto? Endereco { get; set; }
    }
}
