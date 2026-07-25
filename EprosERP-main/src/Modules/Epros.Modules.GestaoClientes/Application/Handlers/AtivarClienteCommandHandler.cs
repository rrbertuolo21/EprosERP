using System.Threading;
using System.Threading.Tasks;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Modules.GestaoClientes.Application.Commands;
using Epros.Modules.GestaoClientes.Infrastructure.Data;

namespace Epros.Modules.GestaoClientes.Application.Handlers
{
    /// <summary>Ativa Cliente.</summary>
    public class AtivarClienteCommandHandler : ICommandHandler<AtivarClienteCommand>
    {
        private readonly ContextGestaoClientes _context;
        private readonly ICurrentUser _currentUser;

        public AtivarClienteCommandHandler(ContextGestaoClientes context, ICurrentUser currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(AtivarClienteCommand request, CancellationToken cancellationToken)
        {
            var cliente = await _context.Clientes.FindAsync(new object[] { request.ClienteId }, cancellationToken);

            if (cliente == null)
            {
                return CommandResult.Falha("Cliente não encontrado.");
            }

            var alteradoPor = _currentUser.GetUserId() ?? "system";
            cliente.Ativar(alteradoPor);

            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Cliente ativado com sucesso!");
        }
    }
}
