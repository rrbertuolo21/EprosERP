namespace Epros.ERP.DfeCalculos.Dtos.V1
{
    public record BaixaDocumentoContadorDto(string DocumentoEmitente, DateTime DataEmissaoInicial, DateTime DataEmissaoFinal)
    {
        public string? DocumentoDestinatario { get; set; } = string.Empty;
        public bool IncluiPdfs { get; set; } = false;
    }
}
