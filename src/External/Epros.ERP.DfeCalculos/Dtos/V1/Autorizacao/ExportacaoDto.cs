namespace Epros.ERP.DfeCalculos.Dtos.V1.Autorizacao
{
    public class ExportacaoDto
    {
        public ExportacaoDto() { }

        public ExportacaoDto(string ufSaidaPais, string localExportacao, string? localDespacho)
        {
            UfSaidaPais = ufSaidaPais;
            LocalExportacao = localExportacao;
            LocalDespacho = localDespacho;
        }

        public string UfSaidaPais { get; set; } = null!;
        public string LocalExportacao { get; set; } = null!;
        public string? LocalDespacho { get; set; }
    }
}
