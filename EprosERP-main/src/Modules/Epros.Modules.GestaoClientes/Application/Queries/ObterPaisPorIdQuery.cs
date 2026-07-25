using System;
using Epros.Shared.Application.Contracts;

namespace Epros.Modules.GestaoClientes.Application.Queries
{
    public record ObterPaisPorIdQuery(Guid Id) : IQuery<PaisDto?>;
}
