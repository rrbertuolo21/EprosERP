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
    public class FecharOrdemServicoDmsCommandHandler : ICommandHandler<FecharOrdemServicoDmsCommand>
    {
        private readonly ContextDMS _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public FecharOrdemServicoDmsCommandHandler(
            ContextDMS context,
            ITenantProvider tenantProvider,
            ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(FecharOrdemServicoDmsCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var os = await _context.OrdensServicoDms.FindAsync(request.Id);
            if (os == null)
            {
                return CommandResult.Falha("Ordem de serviço não encontrada.");
            }

            os.Fechar(usuario);

            if (!os.IsValid)
            {
                return CommandResult.Falha(os.Notifications.Select(n => n.Message));
            }

            if (os.ReclamacaoGarantia)
            {
                // Abre de forma integrada e automática o reembolso de garantia junto à montadora
                var garantia = new GarantiaMontadora(
                    ordemServicoDmsId: os.Id,
                    chassi: os.VeiculoChassi,
                    pecaReclamada: $"Revisão técnica sob OS {os.NumeroOs}",
                    valorReclamado: os.ValorPecas > 0 ? os.ValorPecas : 150m, // valor mínimo se for cortesia
                    tenantId: tenantId,
                    criadoPor: usuario
                );

                if (garantia.IsValid)
                {
                    _context.GarantiasMontadora.Add(garantia);
                }
            }

            _context.OrdensServicoDms.Update(os);
            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Ordem de serviço da oficina fechada com sucesso!", new { OsId = os.Id, Status = os.Status });
        }
    }
}
