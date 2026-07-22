using System;
using Epros.Shared.Application.Contracts;

namespace Epros.Modules.GestaoClientes.Application.Commands
{
    public record RegistrarCepManualCommand(
        string Cep,
        string Logradouro,
        string Bairro,
        Guid MunicipioId,
        string Uf,
        string Justificativa
    ) : ICommand;
}
