using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;

namespace Epros.Modules.GestaoClientes.Application.Commands
{
    public record ProcessarWebhookPagamentoCommand(
        string Action,
        WebhookData Data,
        string? Signature = null
    ) : ICommand;

    public record WebhookData(string Id);
}
