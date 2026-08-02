using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Epros.Modules.Aplicativo.Application.Contracts
{
    /// <summary>
    /// PLT · CONECTORES — abstração de entrega HTTP de webhook. O envio real (cliente HTTP para o
    /// endpoint externo do tenant) é DEPENDÊNCIA DE AMBIENTE. Sem um dispatcher configurado, a
    /// implementação padrão devolve <see cref="ResultadoEntregaWebhook.NaoConfigurado"/> — a entrega
    /// fica pendente e nenhum estado falso é gravado.
    /// </summary>
    public interface IWebhookDispatchService
    {
        Task<ResultadoEntregaWebhook> EnviarAsync(string url, string payload, string? assinaturaHmac,
            IReadOnlyDictionary<string, string>? headers, CancellationToken cancellationToken = default);
    }

    public sealed record ResultadoEntregaWebhook(bool Configurado, bool Sucesso, int? CodigoHttp, string? Erro)
    {
        public static ResultadoEntregaWebhook Ok(int codigoHttp) => new(true, true, codigoHttp, null);
        public static ResultadoEntregaWebhook Falha(int? codigoHttp, string erro) => new(true, false, codigoHttp, erro);
        public static ResultadoEntregaWebhook NaoConfigurado() =>
            new(false, false, null, "Dispatcher de webhook não configurado (dependência de ambiente).");
    }
}
