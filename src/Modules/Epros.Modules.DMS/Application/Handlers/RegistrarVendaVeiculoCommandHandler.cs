using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Modules.DMS.Application.Commands;
using Epros.Modules.DMS.Domain.Entities;
using Epros.Modules.DMS.Infrastructure.Data;

namespace Epros.Modules.DMS.Application.Handlers
{
    public class RegistrarVendaVeiculoCommandHandler : ICommandHandler<RegistrarVendaVeiculoCommand>
    {
        private readonly ContextDMS _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public RegistrarVendaVeiculoCommandHandler(
            ContextDMS context,
            ITenantProvider tenantProvider,
            ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(RegistrarVendaVeiculoCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var venda = new VendaVeiculo(
                request.Chassi,
                request.Modelo,
                request.Marca,
                request.AnoModelo,
                request.PrecoVenda,
                request.ClienteNome,
                tenantId,
                usuario
            );

            if (!venda.IsValid)
            {
                return CommandResult.Falha(venda.Notifications.Select(n => n.Message));
            }

            _context.VendasVeiculos.Add(venda);
            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Venda de veículo registrada com sucesso!", new { VendaId = venda.Id, Status = venda.Status });
        }
    }
}
