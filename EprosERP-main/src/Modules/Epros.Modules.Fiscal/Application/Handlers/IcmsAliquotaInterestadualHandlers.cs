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
    public class CriarIcmsAliquotaInterestadualCommandHandler : ICommandHandler<CriarIcmsAliquotaInterestadualCommand>
    {
        private readonly ContextFiscal _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public CriarIcmsAliquotaInterestadualCommandHandler(ContextFiscal context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        { _context = context; _tenantProvider = tenantProvider; _currentUser = currentUser; }

        public async Task<CommandResult> Handle(CriarIcmsAliquotaInterestadualCommand request, CancellationToken cancellationToken)
        {
            var usuario = _currentUser.GetUserId() ?? "system";

            var icms = new IcmsAliquotaInterestadual(request.UfOrigem, request.UfDestino, request.ValorAliquota, usuario);
            if (!icms.IsValid)
                return CommandResult.Falha(icms.Notifications.Select(n => n.Message), "Erro de validação da Alíquota ICMS Interestadual.");

            _context.IcmsAliquotasInterestaduais.Add(icms);
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Alíquota ICMS Interestadual criada com sucesso!", new { icms.Id });
        }
    }

    public class AtualizarIcmsAliquotaInterestadualCommandHandler : ICommandHandler<AtualizarIcmsAliquotaInterestadualCommand>
    {
        private readonly ContextFiscal _context;
        private readonly ICurrentUser _currentUser;

        public AtualizarIcmsAliquotaInterestadualCommandHandler(ContextFiscal context, ICurrentUser currentUser)
        { _context = context; _currentUser = currentUser; }

        public async Task<CommandResult> Handle(AtualizarIcmsAliquotaInterestadualCommand request, CancellationToken cancellationToken)
        {
            var usuario = _currentUser.GetUserId() ?? "system";
            var icms = await _context.IcmsAliquotasInterestaduais.FirstOrDefaultAsync(i => i.Id == request.Id && i.DeletadoEm == null, cancellationToken);
            if (icms == null)
                return CommandResult.Falha("Alíquota ICMS Interestadual não localizada.");

            icms.Alterar(request.UfOrigem, request.UfDestino, request.ValorAliquota, usuario);
            if (!icms.IsValid)
                return CommandResult.Falha(icms.Notifications.Select(n => n.Message), "Erro de validação da Alíquota ICMS Interestadual.");

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Alíquota ICMS Interestadual atualizada com sucesso!", new { icms.Id });
        }
    }

    public class DeletarIcmsAliquotaInterestadualCommandHandler : ICommandHandler<DeletarIcmsAliquotaInterestadualCommand>
    {
        private readonly ContextFiscal _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public DeletarIcmsAliquotaInterestadualCommandHandler(ContextFiscal context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        { _context = context; _tenantProvider = tenantProvider; _currentUser = currentUser; }

        public async Task<CommandResult> Handle(DeletarIcmsAliquotaInterestadualCommand request, CancellationToken cancellationToken)
        {
            var usuario = _currentUser.GetUserId() ?? "system";
            var icms = await _context.IcmsAliquotasInterestaduais.FirstOrDefaultAsync(i => i.Id == request.Id && i.DeletadoEm == null, cancellationToken);
            if (icms == null)
                return CommandResult.Falha("Alíquota ICMS Interestadual não encontrada.");

            icms.Deletar(usuario);
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Alíquota ICMS Interestadual deletada com sucesso!");
        }
    }
}
