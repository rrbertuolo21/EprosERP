using System;
using System.Threading;
using System.Threading.Tasks;
using Epros.Shared.Application.Contracts;

namespace Epros.Modules.Aplicativo.Infrastructure.Services
{
    /// <summary>
    /// TRANSVERSAL T10 / PD-02 — implementação PADRÃO (fail-safe) da assinatura digital ICP.
    /// É o mecanismo central; o provedor de assinatura + certificado A1/A3 são DEPENDÊNCIA EXTERNA e
    /// entram por overlay. Sem eles, toda solicitação retorna "aguardando provedor" e o documento
    /// permanece pendente — NUNCA se forja validade jurídica (regra da transversal T10).
    /// Substituir este registro por uma implementação concreta quando o provedor estiver disponível.
    /// </summary>
    public class AssinaturaDigitalPendenteService : IAssinaturaDigitalService
    {
        public Task<ResultadoAssinatura> SolicitarAssinaturaAsync(
            Guid documentoId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(ResultadoAssinatura.AguardandoProvedor(
                "Provedor de assinatura ICP-Brasil não configurado (certificado A1/A3 + provedor de " +
                "assinatura = dependência externa). O documento permanece 'pendente de assinatura'."));
        }
    }
}
