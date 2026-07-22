using Epros.Shared.Application.Models;
using MediatR;

namespace Epros.Modules.GestaoClientes.Application.Commands
{
    public record DefinirConfiguracaoGlobalCommand(
        string Chave,
        string Valor,
        bool EhSegredo,
        string Descricao
    ) : IRequest<CommandResult>;
}
