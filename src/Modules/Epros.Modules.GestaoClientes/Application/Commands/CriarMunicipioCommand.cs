using System;
using Epros.Shared.Application.Contracts;

namespace Epros.Modules.GestaoClientes.Application.Commands
{
    public record CriarMunicipioCommand(
        Guid PaisId,
        Guid SubdivisaoId,
        string Nome,
        long CodigoIbge,
        decimal? Latitude,
        decimal? Longitude
    ) : ICommand;
}
