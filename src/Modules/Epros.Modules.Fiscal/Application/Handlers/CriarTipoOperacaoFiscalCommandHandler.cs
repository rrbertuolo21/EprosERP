using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Modules.Fiscal.Application.Commands;
using Epros.Modules.Fiscal.Domain.Entities;
using Epros.Modules.Fiscal.Infrastructure.Data;

namespace Epros.Modules.Fiscal.Application.Handlers
{
    public class CriarTipoOperacaoFiscalCommandHandler : ICommandHandler<CriarTipoOperacaoFiscalCommand>
    {
        private readonly ContextFiscal _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public CriarTipoOperacaoFiscalCommandHandler(
            ContextFiscal context,
            ITenantProvider tenantProvider,
            ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(CriarTipoOperacaoFiscalCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var operacao = new TipoOperacaoFiscal(
                request.TributarioGrupoId,
                request.CfopNfeId,
                request.CfopNfceId,
                request.Descricao,
                request.SobescreveTributacaoNcm,
                request.Finalidade,
                request.Atendimento,
                request.TipoFrete,
                request.TipoMovimento,
                tenantId,
                usuario
            );

            if (!operacao.IsValid)
            {
                return CommandResult.Falha(operacao.Notifications.Select(n => n.Message), "Erro de validação de domínio da Operação Fiscal.");
            }

            _context.TiposOperacoesFiscais.Add(operacao);
            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Operação Fiscal criada com sucesso!", new { Id = operacao.Id });
        }
    }
}
