using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Fiscal.Application.Services;
using Epros.Modules.Fiscal.Domain.Entities;

namespace Epros.Modules.Fiscal.Infrastructure.Services
{
    /// <summary>
    /// Fallback honesto de <see cref="ICteFiscalService"/>. No legado o CT-e era TODO/mock; aqui NUNCA
    /// fabricamos chave/protocolo. Degrada de forma controlada até a biblioteca CT-e + certificado
    /// serem configurados no ambiente (homologação).
    /// </summary>
    public class CteFiscalServiceNaoConfigurado : ICteFiscalService
    {
        private const string Motivo =
            "Transmissão de CT-e não configurada. Integração com a biblioteca CT-e (Hercules/Zeus.Net.CTe) + " +
            "certificado A1 do emitente é específica do ambiente e entra na homologação. Nenhuma chave/protocolo é fabricada.";

        public Task<RetornoDfeTransporteDto> EmitirAsync(ConhecimentoTransporteEletronico cte, CancellationToken ct = default)
            => Task.FromResult(new RetornoDfeTransporteDto { Sucesso = false, StatusSefaz = 0, Motivo = Motivo });

        public Task<RetornoDfeTransporteDto> CancelarAsync(ConhecimentoTransporteEletronico cte, string justificativa, CancellationToken ct = default)
            => Task.FromResult(new RetornoDfeTransporteDto { Sucesso = false, StatusSefaz = 0, Motivo = Motivo });
    }

    /// <summary>
    /// Fallback honesto de <see cref="IMdfeFiscalService"/>. No legado o MDF-e era TODO/mock; aqui NUNCA
    /// fabricamos chave/protocolo. Degrada de forma controlada até a biblioteca MDF-e + certificado
    /// serem configurados no ambiente (homologação).
    /// </summary>
    public class MdfeFiscalServiceNaoConfigurado : IMdfeFiscalService
    {
        private const string Motivo =
            "Transmissão de MDF-e não configurada. Integração com a biblioteca MDF-e (Hercules/Zeus.Net.MDFe) + " +
            "certificado A1 do emitente é específica do ambiente e entra na homologação. Nenhuma chave/protocolo é fabricada.";

        public Task<RetornoDfeTransporteDto> EmitirAsync(ManifestoEletronicoDocumentosFiscais mdfe, CancellationToken ct = default)
            => Task.FromResult(new RetornoDfeTransporteDto { Sucesso = false, StatusSefaz = 0, Motivo = Motivo });

        public Task<RetornoDfeTransporteDto> EncerrarAsync(ManifestoEletronicoDocumentosFiscais mdfe, string municipioIbge, CancellationToken ct = default)
            => Task.FromResult(new RetornoDfeTransporteDto { Sucesso = false, StatusSefaz = 0, Motivo = Motivo });
    }
}
