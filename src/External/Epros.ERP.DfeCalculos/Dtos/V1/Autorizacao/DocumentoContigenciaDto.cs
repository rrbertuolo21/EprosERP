namespace Epros.ERP.DfeCalculos.Dtos.V1.Autorizacao
{
    public class DocumentoContigenciaDto
    {
        public int TipoEmissao { get; set; }
        public string Justificativa { get; set; } = null!;
        public DateTimeOffset DataHora { get; set; }
    }
}