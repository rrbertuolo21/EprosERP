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
    public class CriarOrdemServicoManutencaoCommandHandler : ICommandHandler<CriarOrdemServicoManutencaoCommand>
    {
        private readonly ContextDMS _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public CriarOrdemServicoManutencaoCommandHandler(
            ContextDMS context,
            ITenantProvider tenantProvider,
            ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(CriarOrdemServicoManutencaoCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var ordem = new OrdemServicoManutencao(
                request.PessoaId,
                request.ProdutoId,
                request.VeiculoId,
                request.ChassiVin,
                request.Placa,
                request.QuilometragemEntrada,
                request.ConsultorId,
                request.UnidadeId,
                request.DataAbertura,
                request.PrevisaoEntrega,
                tenantId,
                usuario
            );

            if (!ordem.IsValid)
            {
                return CommandResult.Falha(ordem.Notifications.Select(n => n.Message));
            }

            _context.OrdensServicoManutencao.Add(ordem);
            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Ordem de serviço de manutenção criada com sucesso!", new { ordem.Id });
        }
    }
}
