using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Fiscal.Domain.Entities;

namespace Epros.Modules.Fiscal.Application.Services
{
    /// <summary>
    /// Adapter de transmissão de CT-e (modelo 57) à SEFAZ (Zeus.Net.CTe / Hercules.Net.CTe no legado).
    /// No legado o <c>CteController</c> era um TODO/mock; o contrato aqui é honesto: a implementação
    /// concreta (biblioteca CT-e + certificado) entra na homologação do ambiente do usuário.
    /// Fallback (<c>CteFiscalServiceNaoConfigurado</c>): Sucesso=false com motivo claro, sem fabricar chave.
    /// </summary>
    public interface ICteFiscalService
    {
        Task<RetornoDfeTransporteDto> EmitirAsync(ConhecimentoTransporteEletronico cte, CancellationToken ct = default);
        Task<RetornoDfeTransporteDto> CancelarAsync(ConhecimentoTransporteEletronico cte, string justificativa, CancellationToken ct = default);
    }

    /// <summary>
    /// Adapter de transmissão de MDF-e (modelo 58) à SEFAZ (Zeus.Net.MDFe / Hercules.Net.MDFe no legado).
    /// No legado o <c>MdfeController</c> era um TODO/mock; contrato honesto — implementação concreta na homologação.
    /// Fallback (<c>MdfeFiscalServiceNaoConfigurado</c>): Sucesso=false com motivo claro, sem fabricar chave.
    /// </summary>
    public interface IMdfeFiscalService
    {
        Task<RetornoDfeTransporteDto> EmitirAsync(ManifestoEletronicoDocumentosFiscais mdfe, CancellationToken ct = default);
        Task<RetornoDfeTransporteDto> EncerrarAsync(ManifestoEletronicoDocumentosFiscais mdfe, string municipioIbge, CancellationToken ct = default);
    }

    /// <summary>Retorno genérico de transmissão de DF-e de transporte (CT-e/MDF-e).</summary>
    public class RetornoDfeTransporteDto
    {
        public bool Sucesso { get; set; }
        public int StatusSefaz { get; set; }
        public string Motivo { get; set; } = string.Empty;
        public string ChaveAcesso { get; set; } = string.Empty;
        public string Protocolo { get; set; } = string.Empty;
        public string XmlEnvio { get; set; } = string.Empty;
        public string XmlRetorno { get; set; } = string.Empty;
    }
}
