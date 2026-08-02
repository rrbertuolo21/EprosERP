using System;
using System.Threading;
using System.Threading.Tasks;

namespace Epros.Shared.Application.Contracts
{
    /// <summary>
    /// TRANSVERSAL T10 / PD-02 — abstração ÚNICA de assinatura digital ICP-Brasil.
    /// O provedor de assinatura + certificado A1/A3 são DEPENDÊNCIA EXTERNA e ficam ATRÁS desta
    /// interface. O mecanismo (solicitar/confirmar) é central e compartilhado; a implementação real
    /// entra por overlay quando o provedor/certificado estiverem disponíveis.
    ///
    /// Sem provedor configurado, a implementação padrão devolve <see cref="ResultadoAssinatura.Pendente"/>
    /// — o documento permanece "pendente de assinatura" e NUNCA se forja validade jurídica.
    /// </summary>
    public interface IAssinaturaDigitalService
    {
        /// <summary>
        /// Solicita a assinatura ICP de um documento do GED (<c>DocumentoGed</c>).
        /// Retorna o resultado do provedor; quando não há provedor/certificado, retorna Pendente.
        /// </summary>
        Task<ResultadoAssinatura> SolicitarAssinaturaAsync(
            Guid documentoId,
            CancellationToken cancellationToken = default);
    }

    /// <summary>Resultado de uma solicitação de assinatura ao provedor ICP.</summary>
    public sealed record ResultadoAssinatura(bool Assinado, bool Pendente, string? Detalhe, string? EvidenciaHash)
    {
        public static ResultadoAssinatura Ok(string evidenciaHash) =>
            new(true, false, null, evidenciaHash);

        /// <summary>Sem provedor/certificado — documento fica pendente (dependência externa).</summary>
        public static ResultadoAssinatura AguardandoProvedor(string detalhe) =>
            new(false, true, detalhe, null);

        public static ResultadoAssinatura Falha(string detalhe) =>
            new(false, false, detalhe, null);
    }
}
