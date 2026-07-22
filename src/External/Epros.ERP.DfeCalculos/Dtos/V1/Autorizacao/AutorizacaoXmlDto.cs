namespace Epros.ERP.DfeCalculos.Dtos.V1.Autorizacao
{
    public class AutorizacaoXmlDto
    {
        public AutorizacaoXmlDto() { }

        public AutorizacaoXmlDto(string documento)
        {
            Documento = documento;
        }

        public string Documento { get; set; } = null!;
    }
}
