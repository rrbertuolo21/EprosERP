using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Epros.Modules.Fiscal.Application.Services
{
    /// <summary>
    /// Adapter de transmissão de NFS-e ao webservice municipal (provedores GISS/Ginfes/etc. via
    /// OpenAC.Net.NFSe no legado). Cada município tem provedor/URL próprios, então a implementação
    /// concreta é específica do ambiente. Porte fiel do contrato legado <c>IVendaNfseService</c>,
    /// com DTOs neutros (sem <c>ResultHttp</c>).
    ///
    /// Fallback (<c>NfseFiscalServiceNaoConfigurado</c>): quando não há provedor/certificado municipal
    /// configurado, retorna Sucesso=false com motivo claro — NUNCA fabrica um número de NFS-e.
    /// </summary>
    public interface INfseFiscalService
    {
        /// <summary>Obtém a configuração do provedor NFS-e do município (provedor, URLs, ambiente).</summary>
        RetornoNfseConfigDto ObterConfiguracao(int codigoMunicipioIbge);

        /// <summary>Envia um lote de RPS para a prefeitura (emissão). Síncrono ou assíncrono.</summary>
        Task<RetornoNfseDto> EmitirLoteAsync(NfseEmissaoInput input, CancellationToken ct = default);

        /// <summary>Consulta a situação de um lote pelo número/protocolo.</summary>
        Task<RetornoNfseDto> ConsultarLoteAsync(NfseConsultaLoteInput input, CancellationToken ct = default);

        /// <summary>Consulta a NFS-e gerada a partir de um RPS (número/série/tipo).</summary>
        Task<RetornoNfseDto> ConsultarPorRpsAsync(NfseConsultaRpsInput input, CancellationToken ct = default);

        /// <summary>Cancela uma NFS-e autorizada.</summary>
        Task<RetornoNfseDto> CancelarAsync(NfseCancelamentoInput input, CancellationToken ct = default);

        /// <summary>Gera o PDF (DANFSe) a partir de um XML de NFS-e já emitido.</summary>
        Task<RetornoNfsePdfDto> GerarPdfDeXmlAsync(Stream xmlStream, int? codigoMunicipioIbge, CancellationToken ct = default);
    }

    // ---- Inputs neutros (montados no handler a partir dos commands) ----

    public class NfsePrestadorInput
    {
        public string Documento { get; set; } = string.Empty;
        public int Crt { get; set; }
        public string? InscricaoMunicipal { get; set; }
        public string? RazaoSocial { get; set; }
        public int CodigoMunicipioIbge { get; set; }
        public string? Uf { get; set; }
    }

    public class NfseTomadorInput
    {
        public string Documento { get; set; } = string.Empty;
        public string? InscricaoMunicipal { get; set; }
        public string? RazaoSocial { get; set; }
    }

    public class NfseServicoInput
    {
        public string ItemListaServico { get; set; } = string.Empty;
        public string? CodigoCnae { get; set; }
        public string? CodigoTributacaoMunicipio { get; set; }
        public string? CodigoNbs { get; set; }
        public string? Discriminacao { get; set; }
        public int CodigoMunicipioIbge { get; set; }
        public int ExigibilidadeIss { get; set; } = 1;
        public int MunicipioIncidencia { get; set; }
        public decimal ValorServicos { get; set; }
        public decimal ValorDeducoes { get; set; }
        public decimal ValorIss { get; set; }
        public decimal ValorIssRetido { get; set; }
        public decimal AliquotaIss { get; set; }
        public decimal DescontoIncondicionado { get; set; }
        public decimal DescontoCondicionado { get; set; }
        public int IssRetido { get; set; } = 1;
    }

    public class NfseEmissaoInput
    {
        public int NumeroLote { get; set; }
        public bool Sincrono { get; set; }
        public int Ambiente { get; set; } = 2;
        public int NaturezaOperacao { get; set; } = 1;
        public int RegimeEspecialTributacao { get; set; }
        public bool OptanteSimplesNacional { get; set; }
        public bool IncentivoFiscal { get; set; }
        public System.DateTime? Competencia { get; set; }
        public string RpsNumero { get; set; } = string.Empty;
        public string RpsSerie { get; set; } = string.Empty;
        public int RpsTipo { get; set; } = 1;
        public NfsePrestadorInput Prestador { get; set; } = new();
        public NfseTomadorInput Tomador { get; set; } = new();
        public NfseServicoInput Servico { get; set; } = new();
    }

    public class NfseConsultaLoteInput
    {
        public int NumeroLote { get; set; }
        public string Protocolo { get; set; } = string.Empty;
        public int Ambiente { get; set; } = 2;
        public NfsePrestadorInput Prestador { get; set; } = new();
    }

    public class NfseConsultaRpsInput
    {
        public string NumeroRps { get; set; } = string.Empty;
        public string Serie { get; set; } = string.Empty;
        public int Tipo { get; set; } = 1;
        public int Ambiente { get; set; } = 2;
        public NfsePrestadorInput Prestador { get; set; } = new();
    }

    public class NfseCancelamentoInput
    {
        public string NumeroNfse { get; set; } = string.Empty;
        public string CodigoCancelamento { get; set; } = string.Empty;
        public string? Motivo { get; set; }
        public int Ambiente { get; set; } = 2;
        public NfsePrestadorInput Prestador { get; set; } = new();
    }

    // ---- Retornos neutros ----

    public class RetornoNfseConfigDto
    {
        public bool Sucesso { get; set; }
        public string Provedor { get; set; } = string.Empty;
        public int CodigoMunicipioIbge { get; set; }
        public string Motivo { get; set; } = string.Empty;
    }

    public class RetornoNfseDto
    {
        public bool Sucesso { get; set; }
        public int StatusPrefeitura { get; set; }
        public string Motivo { get; set; } = string.Empty;
        public string? NumeroNfse { get; set; }
        public string? CodigoVerificacao { get; set; }
        public string? Protocolo { get; set; }
        public string XmlEnvio { get; set; } = string.Empty;
        public string XmlRetorno { get; set; } = string.Empty;
    }

    public class RetornoNfsePdfDto
    {
        public bool Sucesso { get; set; }
        public byte[] Pdf { get; set; } = System.Array.Empty<byte>();
        public string NomeArquivo { get; set; } = "nfse.pdf";
        public string Motivo { get; set; } = string.Empty;
    }
}
