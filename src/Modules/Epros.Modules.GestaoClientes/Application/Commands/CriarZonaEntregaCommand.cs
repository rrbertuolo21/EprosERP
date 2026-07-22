using System;
using Epros.Shared.Application.Contracts;

namespace Epros.Modules.GestaoClientes.Application.Commands
{
    public record CriarZonaEntregaCommand(
        string Nome,
        string CepInicio,
        string CepFim
    ) : ICommand;
}
