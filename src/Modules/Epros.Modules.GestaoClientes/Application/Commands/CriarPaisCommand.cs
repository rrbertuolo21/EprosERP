using System;
using Epros.Shared.Application.Contracts;

namespace Epros.Modules.GestaoClientes.Application.Commands
{
    public record CriarPaisCommand(
        string Nome,
        string CodigoIsoAlpha2,
        string CodigoIsoAlpha3,
        string CodigoNumerico,
        string? Capital,
        string? CodigoDiscagem
    ) : ICommand;
}
