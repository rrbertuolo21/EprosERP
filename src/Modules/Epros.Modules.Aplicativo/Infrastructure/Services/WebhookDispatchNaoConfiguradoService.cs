using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Aplicativo.Application.Contracts;

namespace Epros.Modules.Aplicativo.Infrastructure.Services
{
    /// <summary>
    /// PLT · CONECTORES — implementação PADRÃO (fail-safe) do dispatcher de webhook. O envio HTTP real
    /// ao endpoint externo é dependência de ambiente; sem ele, toda entrega retorna "não configurado"
    /// e permanece pendente (nenhum estado de entrega falso é gravado). // valida-ambiente
    /// </summary>
    public sealed class WebhookDispatchNaoConfiguradoService : IWebhookDispatchService
    {
        public Task<ResultadoEntregaWebhook> EnviarAsync(string url, string payload, string? assinaturaHmac,
            IReadOnlyDictionary<string, string>? headers, CancellationToken cancellationToken = default)
            => Task.FromResult(ResultadoEntregaWebhook.NaoConfigurado());
    }
}
