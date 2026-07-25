using Epros.Shared.Application.Contracts;
using System;

namespace Epros.Modules.Fiscal.Application.Commands
{
    public record DeletarServicoCommand(Guid Id) : ICommand;
}
