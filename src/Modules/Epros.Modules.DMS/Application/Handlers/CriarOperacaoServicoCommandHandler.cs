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
    public class CriarOperacaoServicoCommandHandler : ICommandHandler<CriarOperacaoServicoCommand>
    {
        private readonly ContextDMS _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public CriarOperacaoServicoCommandHandler(
            ContextDMS context,
            ITenantProvider tenantProvider,
            ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(CriarOperacaoServicoCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var operacao = new OperacaoServico(
                request.TipoServicoId,
                request.Codigo,
                request.Descricao,
                request.TmoQuantidade,
                request.TmoUnidade,
                request.NaturezaPadrao,
                tenantId,
                usuario
            );

            if (!operacao.IsValid)
            {
                return CommandResult.Falha(operacao.Notifications.Select(n => n.Message));
            }

            _context.OperacoesServico.Add(operacao);
            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Operação de serviço criada com sucesso!", new { operacao.Id });
        }
    }
}
