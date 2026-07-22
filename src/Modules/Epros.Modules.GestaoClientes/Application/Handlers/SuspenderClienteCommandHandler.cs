using System.Threading;
using System.Threading.Tasks;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Modules.GestaoClientes.Application.Commands;
using Epros.Modules.GestaoClientes.Infrastructure.Data;

namespace Epros.Modules.GestaoClientes.Application.Handlers
{
    /// <summary>Suspende Cliente.</summary>
    public class SuspenderClienteCommandHandler : ICommandHandler<SuspenderClienteCommand>
    {
        private readonly ContextGestaoClientes _context;
        private readonly ICurrentUser _currentUser;

        public SuspenderClienteCommandHandler(ContextGestaoClientes context, ICurrentUser currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(SuspenderClienteCommand request, CancellationToken cancellationToken)
        {
            var cliente = await _context.Clientes.FindAsync(new object[] { request.ClienteId }, cancellationToken);

            if (cliente == null)
            {
                return CommandResult.Falha("Cliente não encontrado.");
            }

            var alteradoPor = _currentUser.GetUserId() ?? "system";
            cliente.Inativar(alteradoPor);

            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Cliente suspenso com sucesso!");
        }
    }
}
