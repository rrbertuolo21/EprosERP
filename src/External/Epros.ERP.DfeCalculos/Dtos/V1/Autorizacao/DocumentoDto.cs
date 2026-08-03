namespace Epros.ERP.DfeCalculos.Dtos.V1.Autorizacao
{
    public class DocumentoNfceDto
    {
        public DocumentoNfceDto() { }
        public DocumentoNfceDto(int ambiente, long numeroNf, int serieNf, string certPath, string certPass, string csc, string cscId, DateTime? dataEmissao, DateTime? dataHoraSaida, DocumentoContigenciaDto? contigencia, DocumentoConfiguracaoDanfeNfceDto? configuracaoDanfeNfce)
        {
            Ambiente = ambiente;
            NumeroNf = numeroNf;
            SerieNf = serieNf;
            CertPath = certPath;
            CertPass = certPass;
            Csc = csc;
            CscId = cscId;
            DataEmissao = dataEmissao;
            DataHoraSaida = dataHoraSaida;
            Contigencia = contigencia;
            ConfiguracaoDanfeNfce = configuracaoDanfeNfce;
        }

        public int Ambiente { get; set; }
        public long NumeroNf { get; set; }
        public int SerieNf { get; set; }
        public string CertPath { get; set; } = null!;
        public string CertPass { get; set; } = null!;
        public string? Csc { get; set; }
        public string? CscId { get; set; }
        public DateTime? DataEmissao { get; set; }
        public DateTime? DataHoraSaida { get; set; }
        public int? TipoNfe { get; set; }
        public DocumentoContigenciaDto? Contigencia { get; set; }
        public DocumentoConfiguracaoDanfeNfceDto? ConfiguracaoDanfeNfce { get; set; }
    }

    public class DocumentoNfeDto
    {
        public DocumentoNfeDto() { }
        public DocumentoNfeDto(int tipoNfe, int ambiente, long numeroNf, int serieNf, string certPath, string certPass, int finalidadeNFe, DateTime? dataEmissao, DateTime? dataHoraSaida, DocumentoContigenciaDto? contigencia, ICollection<DocumentoNumeroReferenciadaNFeDto>? chavesReferenciadasNFe)
        {
            TipoNfe = tipoNfe;
            Ambiente = ambiente;
            NumeroNf = numeroNf;
            SerieNf = serieNf;
            CertPath = certPath;
            CertPass = certPass;
            Contigencia = contigencia;
            FinalidadeNFe = finalidadeNFe;
            DataEmissao = dataEmissao;
            DataHoraSaida = dataHoraSaida;
            ChavesReferenciadasNFe = chavesReferenciadasNFe;
        }

        public int TipoNfe { get; set; }
        public int Ambiente { get; set; }
        public long NumeroNf { get; set; }
        public int SerieNf { get; set; }
        public string CertPath { get; set; } = null!;
        public string CertPass { get; set; } = null!;
        public int FinalidadeNFe { get; set; }
        public DateTime? DataEmissao { get; set; }
        public DateTime? DataHoraSaida { get; set; }
        public DocumentoContigenciaDto? Contigencia { get; set; }
        public ICollection<DocumentoNumeroReferenciadaNFeDto>? ChavesReferenciadasNFe { get; set; }
    }
}
