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
    public class CriarTributarioGrupoCommandHandler : ICommandHandler<CriarTributarioGrupoCommand>
    {
        private readonly ContextFiscal _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public CriarTributarioGrupoCommandHandler(ContextFiscal context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        { _context = context; _tenantProvider = tenantProvider; _currentUser = currentUser; }

        public async Task<CommandResult> Handle(CriarTributarioGrupoCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var grupo = new TributarioGrupo(request.Descricao, tenantId, usuario);
            if (!grupo.IsValid)
                return CommandResult.Falha(grupo.Notifications.Select(n => n.Message), "Erro de validação do Grupo Tributário.");

            _context.TributarioGrupos.Add(grupo);
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Grupo Tributário criado com sucesso!", new { grupo.Id });
        }
    }

    public class AtualizarTributarioGrupoCommandHandler : ICommandHandler<AtualizarTributarioGrupoCommand>
    {
        private readonly ContextFiscal _context;
        private readonly ICurrentUser _currentUser;

        public AtualizarTributarioGrupoCommandHandler(ContextFiscal context, ICurrentUser currentUser)
        { _context = context; _currentUser = currentUser; }

        public async Task<CommandResult> Handle(AtualizarTributarioGrupoCommand request, CancellationToken cancellationToken)
        {
            var usuario = _currentUser.GetUserId() ?? "system";
            var grupo = await _context.TributarioGrupos.FirstOrDefaultAsync(g => g.Id == request.Id && g.DeletadoEm == null, cancellationToken);
            if (grupo == null)
                return CommandResult.Falha("Grupo Tributário não localizado.");

            grupo.Alterar(request.Descricao, usuario);
            if (!grupo.IsValid)
                return CommandResult.Falha(grupo.Notifications.Select(n => n.Message), "Erro de validação do Grupo Tributário.");

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Grupo Tributário atualizado com sucesso!", new { grupo.Id });
        }
    }

    public class DeletarTributarioGrupoCommandHandler : ICommandHandler<DeletarTributarioGrupoCommand>
    {
        private readonly ContextFiscal _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public DeletarTributarioGrupoCommandHandler(ContextFiscal context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        { _context = context; _tenantProvider = tenantProvider; _currentUser = currentUser; }

        public async Task<CommandResult> Handle(DeletarTributarioGrupoCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";
            var grupo = await _context.TributarioGrupos.FirstOrDefaultAsync(g => g.Id == request.Id && g.TenantId == tenantId && g.DeletadoEm == null, cancellationToken);
            if (grupo == null)
                return CommandResult.Falha("Grupo Tributário não encontrado.");

            grupo.Deletar(usuario);
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Grupo Tributário deletado com sucesso!");
        }
    }
}
