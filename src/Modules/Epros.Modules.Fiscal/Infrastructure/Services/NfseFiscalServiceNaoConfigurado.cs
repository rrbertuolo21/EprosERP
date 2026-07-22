using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Fiscal.Application.Services;

namespace Epros.Modules.Fiscal.Infrastructure.Services
{
    /// <summary>
    /// Implementação padrão (fallback) de <see cref="INfseFiscalService"/> para ambientes que ainda
    /// NÃO têm o provedor municipal de NFS-e configurado (URL do webservice do município + certificado
    /// + credenciais GISS/Ginfes/etc. via OpenAC.Net.NFSe). Degrada de forma controlada: retorna
    /// Sucesso=false com motivo claro — NUNCA fabrica um número de NFS-e/protocolo.
    ///
    /// Este é o análogo dos controllers legados de CT-e/MDF-e (que também eram TODO/mock) e do
    /// <see cref="EmitenteFiscalProviderNaoConfigurado"/>. A implementação real (OpenAC.Net.NFSe +
    /// provedor por município) entra no ambiente do usuário na homologação.
    /// </summary>
    public class NfseFiscalServiceNaoConfigurado : INfseFiscalService
    {
        private const string MotivoPadrao =
            "Provedor municipal de NFS-e não configurado. Configure o webservice do município (provedor GISS/Ginfes/etc.), " +
            "o certificado digital e as credenciais antes de transmitir. (Integração OpenAC.Net.NFSe é específica do ambiente.)";

        public RetornoNfseConfigDto ObterConfiguracao(int codigoMunicipioIbge) => new()
        {
            Sucesso = false,
            CodigoMunicipioIbge = codigoMunicipioIbge,
            Provedor = "NaoConfigurado",
            Motivo = MotivoPadrao
        };

        public Task<RetornoNfseDto> EmitirLoteAsync(NfseEmissaoInput input, CancellationToken ct = default)
            => Task.FromResult(Falha());

        public Task<RetornoNfseDto> ConsultarLoteAsync(NfseConsultaLoteInput input, CancellationToken ct = default)
            => Task.FromResult(Falha());

        public Task<RetornoNfseDto> ConsultarPorRpsAsync(NfseConsultaRpsInput input, CancellationToken ct = default)
            => Task.FromResult(Falha());

        public Task<RetornoNfseDto> CancelarAsync(NfseCancelamentoInput input, CancellationToken ct = default)
            => Task.FromResult(Falha());

        public Task<RetornoNfsePdfDto> GerarPdfDeXmlAsync(Stream xmlStream, int? codigoMunicipioIbge, CancellationToken ct = default)
            => Task.FromResult(new RetornoNfsePdfDto { Sucesso = false, Motivo = MotivoPadrao });

        private static RetornoNfseDto Falha() => new()
        {
            Sucesso = false,
            StatusPrefeitura = 0,
            Motivo = MotivoPadrao
        };
    }
}
