using System;
using Epros.Shared.Application.Contracts;

namespace Epros.Modules.GestaoClientes.Application.Commands
{
    public record ProcessarFaturamentoRecorrenteCommand(DateTime? Referencia = null) : ICommand;
}
