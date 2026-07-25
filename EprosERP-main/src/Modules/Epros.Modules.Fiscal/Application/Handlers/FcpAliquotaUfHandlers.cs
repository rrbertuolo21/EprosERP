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
    public class CriarFcpAliquotaUfCommandHandler : ICommandHandler<CriarFcpAliquotaUfCommand>
    {
        private readonly ContextFiscal _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public CriarFcpAliquotaUfCommandHandler(ContextFiscal context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        { _context = context; _tenantProvider = tenantProvider; _currentUser = currentUser; }

        public async Task<CommandResult> Handle(CriarFcpAliquotaUfCommand request, CancellationToken cancellationToken)
        {
            var usuario = _currentUser.GetUserId() ?? "system";

            var fcp = new FcpAliquotaUf(request.Uf, request.ValorAliquota, request.Observacao, usuario);
            if (!fcp.IsValid)
                return CommandResult.Falha(fcp.Notifications.Select(n => n.Message), "Erro de validação da Alíquota FCP.");

            _context.FcpAliquotasUf.Add(fcp);
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Alíquota FCP criada com sucesso!", new { fcp.Id });
        }
    }

    public class AtualizarFcpAliquotaUfCommandHandler : ICommandHandler<AtualizarFcpAliquotaUfCommand>
    {
        private readonly ContextFiscal _context;
        private readonly ICurrentUser _currentUser;

        public AtualizarFcpAliquotaUfCommandHandler(ContextFiscal context, ICurrentUser currentUser)
        { _context = context; _currentUser = currentUser; }

        public async Task<CommandResult> Handle(AtualizarFcpAliquotaUfCommand request, CancellationToken cancellationToken)
        {
            var usuario = _currentUser.GetUserId() ?? "system";
            var fcp = await _context.FcpAliquotasUf.FirstOrDefaultAsync(f => f.Id == request.Id && f.DeletadoEm == null, cancellationToken);
            if (fcp == null)
                return CommandResult.Falha("Alíquota FCP não localizada.");

            fcp.Alterar(request.Uf, request.ValorAliquota, request.Observacao, usuario);
            if (!fcp.IsValid)
                return CommandResult.Falha(fcp.Notifications.Select(n => n.Message), "Erro de validação da Alíquota FCP.");

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Alíquota FCP atualizada com sucesso!", new { fcp.Id });
        }
    }

    public class DeletarFcpAliquotaUfCommandHandler : ICommandHandler<DeletarFcpAliquotaUfCommand>
    {
        private readonly ContextFiscal _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public DeletarFcpAliquotaUfCommandHandler(ContextFiscal context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        { _context = context; _tenantProvider = tenantProvider; _currentUser = currentUser; }

        public async Task<CommandResult> Handle(DeletarFcpAliquotaUfCommand request, CancellationToken cancellationToken)
        {
            var usuario = _currentUser.GetUserId() ?? "system";
            var fcp = await _context.FcpAliquotasUf.FirstOrDefaultAsync(f => f.Id == request.Id && f.DeletadoEm == null, cancellationToken);
            if (fcp == null)
                return CommandResult.Falha("Alíquota FCP não encontrada.");

            fcp.Deletar(usuario);
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Alíquota FCP deletada com sucesso!");
        }
    }
}
