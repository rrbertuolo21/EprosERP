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
    public class CriarCestCommandHandler : ICommandHandler<CriarCestCommand>
    {
        private readonly ContextFiscal _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public CriarCestCommandHandler(ContextFiscal context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context; _tenantProvider = tenantProvider; _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(CriarCestCommand request, CancellationToken cancellationToken)
        {
            var usuario = _currentUser.GetUserId() ?? "system";

            var cest = new Cest(request.Codigo, request.Descricao, usuario);
            if (!cest.IsValid)
                return CommandResult.Falha(cest.Notifications.Select(n => n.Message), "Erro de validação do CEST.");

            _context.Cests.Add(cest);
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("CEST criado com sucesso!", new { cest.Id });
        }
    }

    public class AtualizarCestCommandHandler : ICommandHandler<AtualizarCestCommand>
    {
        private readonly ContextFiscal _context;
        private readonly ICurrentUser _currentUser;

        public AtualizarCestCommandHandler(ContextFiscal context, ICurrentUser currentUser)
        {
            _context = context; _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(AtualizarCestCommand request, CancellationToken cancellationToken)
        {
            var usuario = _currentUser.GetUserId() ?? "system";
            var cest = await _context.Cests.FirstOrDefaultAsync(c => c.Id == request.Id && c.DeletadoEm == null, cancellationToken);
            if (cest == null)
                return CommandResult.Falha("CEST não localizado.");

            cest.Alterar(request.Codigo, request.Descricao, usuario);
            if (!cest.IsValid)
                return CommandResult.Falha(cest.Notifications.Select(n => n.Message), "Erro de validação do CEST.");

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("CEST atualizado com sucesso!", new { cest.Id });
        }
    }

    public class DeletarCestCommandHandler : ICommandHandler<DeletarCestCommand>
    {
        private readonly ContextFiscal _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public DeletarCestCommandHandler(ContextFiscal context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context; _tenantProvider = tenantProvider; _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(DeletarCestCommand request, CancellationToken cancellationToken)
        {
            var usuario = _currentUser.GetUserId() ?? "system";
            var cest = await _context.Cests.FirstOrDefaultAsync(c => c.Id == request.Id && c.DeletadoEm == null, cancellationToken);
            if (cest == null)
                return CommandResult.Falha("CEST não encontrado.");

            cest.Deletar(usuario);
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("CEST deletado com sucesso!");
        }
    }
}
