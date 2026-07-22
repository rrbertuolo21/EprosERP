using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Modules.ESG.Application.Commands;
using Epros.Modules.ESG.Domain.Entities;
using Epros.Modules.ESG.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.ESG.Application.Handlers
{
    public class CriarRelatorioESGCommandHandler : ICommandHandler<CriarRelatorioESGCommand>
    {
        private readonly ContextESG _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public CriarRelatorioESGCommandHandler(
            ContextESG context,
            ITenantProvider tenantProvider,
            ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(CriarRelatorioESGCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var jaExiste = await _context.RelatoriosESG
                .AnyAsync(r => r.AnoFiscal == request.AnoFiscal, cancellationToken);

            if (jaExiste)
            {
                return CommandResult.Falha($"Já existe um relatório consolidado ou rascunho para o ano fiscal {request.AnoFiscal}.");
            }

            var relatorio = new RelatorioESG(
                request.AnoFiscal,
                request.NomeRelatorio,
                tenantId,
                usuario
            );

            if (!relatorio.IsValid)
            {
                return CommandResult.Falha(relatorio.Notifications.Select(n => n.Message));
            }

            _context.RelatoriosESG.Add(relatorio);
            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Rascunho de relatório ESG anual criado com sucesso!", new { RelatorioId = relatorio.Id });
        }
    }
}
