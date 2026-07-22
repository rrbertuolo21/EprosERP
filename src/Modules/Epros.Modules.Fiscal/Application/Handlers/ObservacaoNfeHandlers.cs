using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Modules.Fiscal.Application.Commands;
using Epros.Modules.Fiscal.Domain.Entities;
using Epros.Modules.Fiscal.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Fiscal.Application.Handlers
{
    public class CriarObservacaoNfeCommandHandler : ICommandHandler<CriarObservacaoNfeCommand>
    {
        private readonly ContextFiscal _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public CriarObservacaoNfeCommandHandler(ContextFiscal context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        { _context = context; _tenantProvider = tenantProvider; _currentUser = currentUser; }

        public async Task<CommandResult> Handle(CriarObservacaoNfeCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var obs = new ObservacaoNfe(request.Descricao, tenantId, usuario);
            if (!obs.IsValid)
                return CommandResult.Falha(obs.Notifications.Select(n => n.Message), "Erro de validação da Observação NFe.");

            _context.ObservacoesNfe.Add(obs);
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Observação NFe criada com sucesso!", new { obs.Id });
        }
    }

    public class AtualizarObservacaoNfeCommandHandler : ICommandHandler<AtualizarObservacaoNfeCommand>
    {
        private readonly ContextFiscal _context;
        private readonly ICurrentUser _currentUser;

        public AtualizarObservacaoNfeCommandHandler(ContextFiscal context, ICurrentUser currentUser)
        { _context = context; _currentUser = currentUser; }

        public async Task<CommandResult> Handle(AtualizarObservacaoNfeCommand request, CancellationToken cancellationToken)
        {
            var usuario = _currentUser.GetUserId() ?? "system";
            var obs = await _context.ObservacoesNfe.FirstOrDefaultAsync(o => o.Id == request.Id && o.DeletadoEm == null, cancellationToken);
            if (obs == null)
                return CommandResult.Falha("Observação NFe não localizada.");

            obs.Alterar(request.Descricao, usuario);
            if (!obs.IsValid)
                return CommandResult.Falha(obs.Notifications.Select(n => n.Message), "Erro de validação da Observação NFe.");

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Observação NFe atualizada com sucesso!", new { obs.Id });
        }
    }

    public class DeletarObservacaoNfeCommandHandler : ICommandHandler<DeletarObservacaoNfeCommand>
    {
        private readonly ContextFiscal _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public DeletarObservacaoNfeCommandHandler(ContextFiscal context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        { _context = context; _tenantProvider = tenantProvider; _currentUser = currentUser; }

        public async Task<CommandResult> Handle(DeletarObservacaoNfeCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";
            var obs = await _context.ObservacoesNfe.FirstOrDefaultAsync(o => o.Id == request.Id && o.TenantId == tenantId && o.DeletadoEm == null, cancellationToken);
            if (obs == null)
                return CommandResult.Falha("Observação NFe não encontrada.");

            obs.Deletar(usuario);
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Observação NFe deletada com sucesso!");
        }
    }
}
