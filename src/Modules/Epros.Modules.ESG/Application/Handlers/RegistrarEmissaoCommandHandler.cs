using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Modules.ESG.Application.Commands;
using Epros.Modules.ESG.Domain.Entities;
using Epros.Modules.ESG.Infrastructure.Data;

namespace Epros.Modules.ESG.Application.Handlers
{
    public class RegistrarEmissaoCommandHandler : ICommandHandler<RegistrarEmissaoCommand>
    {
        private readonly ContextESG _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public RegistrarEmissaoCommandHandler(
            ContextESG context,
            ITenantProvider tenantProvider,
            ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(RegistrarEmissaoCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var emissao = new EmissaoCarbono(
                request.FonteEmissao,
                request.Escopo,
                request.CategoriaGhg,
                request.QuantidadeConsumo,
                request.UnidadeMedida,
                request.FatorEmissao,
                request.DataTransacao,
                tenantId,
                usuario
            );

            if (!emissao.IsValid)
            {
                return CommandResult.Falha(emissao.Notifications.Select(n => n.Message));
            }

            _context.EmissoesCarbono.Add(emissao);
            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Emissão de carbono registrada com sucesso!", new { EmissaoId = emissao.Id, TotalCo2e = emissao.TotalCo2e });
        }
    }
}
