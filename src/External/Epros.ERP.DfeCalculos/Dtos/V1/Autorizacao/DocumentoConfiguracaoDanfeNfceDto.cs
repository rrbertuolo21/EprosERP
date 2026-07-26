namespace Epros.ERP.DfeCalculos.Dtos.V1.Autorizacao
{
    public class DocumentoConfiguracaoDanfeNfceDto
    {
        public int? DetalheVendaNormal { get; set; } = 3;
        public int? DetalheVendaContigencia { get; set; } = 3;
        public bool? ImprimeDescontoItem { get; set; } = true;
        public bool? ImprimeFoneEmitente { get; set; } = false;
        public float? MargemEsquerda { get; set; } = 5;
        public float? MargemDireita { get; set; } = 0;
        public int? ModoImpressao { get; set; } = 1;
        public int? NfceLayoutQrCode { get; set; } = 0;
        public int? VersaoQrCode { get; set; } = 100;
        public bool? SegundaViaContingencia { get; set; } = false;
        public string? TextoRodape { get; set; } = string.Empty;
    }
}