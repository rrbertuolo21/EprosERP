using Epros.Shared.Application.Models;
using MediatR;

namespace Epros.Modules.GestaoClientes.Application.Queries
{
    public record ObterConfiguracaoGlobalQuery(string Chave) : IRequest<CommandResult>;
}
