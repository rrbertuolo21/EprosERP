using Epros.Shared.Application.Contracts;

namespace Epros.Modules.GestaoClientes.Application.Commands
{
    public record ReprocessarCepCommand(string Cep) : ICommand;
}
