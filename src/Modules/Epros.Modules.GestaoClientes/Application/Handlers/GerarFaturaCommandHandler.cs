using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Modules.GestaoClientes.Application.Commands;
using Epros.Modules.GestaoClientes.Domain.Entities;
using Epros.Modules.GestaoClientes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.GestaoClientes.Application.Handlers
{
    /// <summary>Gera Fatura.</summary>
    public class GerarFaturaCommandHandler : ICommandHandler<GerarFaturaCommand>
    {
        private readonly ContextGestaoClientes _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public GerarFaturaCommandHandler(
            ContextGestaoClientes context,
            ITenantProvider tenantProvider,
            ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(GerarFaturaCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var criadoPor = _currentUser.GetUserId() ?? "system";

            // Verifica se o cliente existe sob as regras automáticas de multi-tenancy e soft delete
            var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.Id == request.ClienteId, cancellationToken);
            if (cliente == null)
            {
                return CommandResult.Falha("Cliente não encontrado para o inquilino atual.");
            }

            var fatura = new Fatura(request.ClienteId, request.Valor, request.DataVencimento, tenantId, criadoPor);

            if (!fatura.IsValid)
            {
                var erros = fatura.Notifications.Select(n => n.Message);
                return CommandResult.Falha(erros, "Invariantes de domínio da fatura não foram atendidas.");
            }

            _context.Faturas.Add(fatura);
            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Fatura gerada com sucesso!", new { FaturaId = fatura.Id });
        }
    }
}
