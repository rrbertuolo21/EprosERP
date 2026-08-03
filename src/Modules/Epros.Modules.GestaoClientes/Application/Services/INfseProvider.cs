using System;
using System.Threading;
using System.Threading.Tasks;

namespace Epros.Modules.GestaoClientes.Application.Services
{
    /// <summary>
    /// 1.08J — Dados que o MECANISMO prepara e entrega ao provedor municipal para a emissão REAL da NFS-e
    /// da mensalidade. Contém apenas o que o mecanismo conhece (competência, tomador, valor base, ambiente).
    /// ⛔ NÃO carrega alíquota, subitem (1.05/1.03) nem imposto calculado — isso é responsabilidade do
    /// provedor/overlay (`negocio-siser` + contador), não do mecanismo.
    /// </summary>
    public sealed record NfseEmissaoDados(
        Guid NfseMensalidadeId,
        Guid FaturaId,
        Guid ClienteId,
        DateTime Competencia,
        decimal ValorBase,
        string TenantId,
        string Ambiente);

    /// <summary>Situação retornada por uma tentativa de emissão via <see cref="INfseProvider"/>.</summary>
    public enum NfseEmissaoSituacao
    {
        /// <summary>Provedor autorizou a NFS-e — <see cref="NfseEmissaoResultado.NumeroNfse"/> preenchido (REAL).</summary>
        Emitida,
        /// <summary>Nenhum provedor municipal / certificado / alíquota configurado — o registro segue Pendente.</summary>
        NaoConfigurado,
        /// <summary>Provedor tentou e falhou (rejeição, indisponibilidade) — registro em Erro com o motivo.</summary>
        Erro
    }

    /// <summary>Resultado de uma tentativa de emissão de NFS-e.</summary>
    public sealed record NfseEmissaoResultado(
        NfseEmissaoSituacao Situacao,
        string? NumeroNfse = null,
        string? Motivo = null)
    {
        public static NfseEmissaoResultado NaoConfigurado(string motivo)
            => new(NfseEmissaoSituacao.NaoConfigurado, null, motivo);

        public static NfseEmissaoResultado Emitida(string numeroNfse)
            => new(NfseEmissaoSituacao.Emitida, numeroNfse, null);

        public static NfseEmissaoResultado Erro(string motivo)
            => new(NfseEmissaoSituacao.Erro, null, motivo);
    }

    /// <summary>
    /// 1.08J — PORTA de provedor de NFS-e (adaptador municipal). O mecanismo depende DESTA abstração; a
    /// integração real (provedor da prefeitura, geração do XML/DPS, certificado A1/A3, cálculo de ISS/IBS/CBS)
    /// é uma implementação EXTERNA que só existirá quando o overlay `negocio-siser` estiver povoado e a infra
    /// (certificado + provedor municipal) provisionada. Até lá vige o <see cref="NfseProviderNaoConfigurado"/>.
    /// </summary>
    public interface INfseProvider
    {
        Task<NfseEmissaoResultado> EmitirAsync(NfseEmissaoDados dados, CancellationToken cancellationToken);
    }

    /// <summary>
    /// 1.08J — Implementação DEFAULT: não há provedor municipal, certificado nem alíquota configurados
    /// (overlay `negocio-siser` VAZIO). SEMPRE retorna <see cref="NfseEmissaoSituacao.NaoConfigurado"/> —
    /// não chama serviço externo, não gera XML/DPS, não inventa número nem alíquota. Deixa o registro em
    /// Pendente. É o adaptador seguro enquanto a emissão REAL for uma dependência (contador + infra).
    /// </summary>
    public sealed class NfseProviderNaoConfigurado : INfseProvider
    {
        public const string MotivoPadrao =
            "Emissão de NFS-e não configurada: falta provedor municipal, certificado e alíquota/subitem " +
            "(overlay negocio-siser vazio + contador). Registro mantido Pendente. [LC 116/2003 item 1.05/1.03]";

        public Task<NfseEmissaoResultado> EmitirAsync(NfseEmissaoDados dados, CancellationToken cancellationToken)
            => Task.FromResult(NfseEmissaoResultado.NaoConfigurado(MotivoPadrao));
    }
}
