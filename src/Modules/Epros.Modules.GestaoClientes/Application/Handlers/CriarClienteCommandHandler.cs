using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Modules.GestaoClientes.Application.Commands;
using Epros.Modules.GestaoClientes.Domain.Entities;
using Epros.Modules.GestaoClientes.Infrastructure.Data;

namespace Epros.Modules.GestaoClientes.Application.Handlers
{
    /// <summary>Cria Cliente.</summary>
    public class CriarClienteCommandHandler : ICommandHandler<CriarClienteCommand>
    {
        private readonly ContextGestaoClientes _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public CriarClienteCommandHandler(
            ContextGestaoClientes context,
            ITenantProvider tenantProvider,
            ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(CriarClienteCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var criadoPor = _currentUser.GetUserId() ?? "system";

            // Instancia a entidade rica com validações internas de domínio (Flunt)
            var cliente = new Cliente(
                request.RazaoSocial,
                request.Cnpj,
                request.Email,
                request.PlanoId,
                null, // RevendaId
                null, // VendedorId
                10, // DiaVencimento
                "Active", // StatusSaaS
                tenantId,
                criadoPor,
                request.Telefone,
                request.NomeContato,
                request.IsDemo,
                request.TokenAcesso
            );

            // Valida as invariantes do domínio (Flunt)
            if (!cliente.IsValid)
            {
                var erros = cliente.Notifications.Select(n => n.Message);
                return CommandResult.Falha(erros, "Invariantes de domínio do cliente não foram atendidas.");
            }

            // Persiste no banco usando o ContextGestaoClientes
            _context.Clientes.Add(cliente);
            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Cliente cadastrado com sucesso!", new { ClienteId = cliente.Id });
        }
    }
}
