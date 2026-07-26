using Epros.Shared.Application.Models;
using MediatR;

namespace Epros.Shared.Application.Contracts
{
    public interface ICommand : IRequest<CommandResult>
    {
    }

    public interface ICommandHandler<in TCommand> : IRequestHandler<TCommand, CommandResult>
        where TCommand : ICommand
    {
    }
}
