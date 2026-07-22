using System;
using Epros.Shared.Application.Contracts;

namespace Epros.Modules.Vendas.Application.Commands
{
    public record CancelarVendaCommand(Guid VendaId, string Motivo) : ICommand;
}
