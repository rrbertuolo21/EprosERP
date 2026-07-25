using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Modules.ESG.Application.Commands;
using Epros.Modules.ESG.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.ESG.Application.Handlers
{
    public class ConsolidarRelatorioESGCommandHandler : ICommandHandler<ConsolidarRelatorioESGCommand>
    {
        private readonly ContextESG _context;
        private readonly ICurrentUser _currentUser;

        public ConsolidarRelatorioESGCommandHandler(
            ContextESG context,
            ICurrentUser currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(ConsolidarRelatorioESGCommand request, CancellationToken cancellationToken)
        {
            var usuario = _currentUser.GetUserId() ?? "system";

            var relatorio = await _context.RelatoriosESG
                .FirstOrDefaultAsync(r => r.AnoFiscal == request.AnoFiscal, cancellationToken);

            if (relatorio == null)
            {
                return CommandResult.Falha($"Não foi encontrado rascunho de relatório ESG para o ano fiscal {request.AnoFiscal}. Crie um relatório primeiro.");
            }

            // Soma emissões por escopo
            var emissoes = await _context.EmissoesCarbono
                .Where(e => e.DataTransacao.Year == request.AnoFiscal)
                .ToListAsync(cancellationToken);

            var totalEscopo1 = emissoes.Where(e => e.Escopo == 1).Sum(e => e.TotalCo2e);
            var totalEscopo2 = emissoes.Where(e => e.Escopo == 2).Sum(e => e.TotalCo2e);
            var totalEscopo3 = emissoes.Where(e => e.Escopo == 3).Sum(e => e.TotalCo2e);

            relatorio.ConsolidarValores(totalEscopo1, totalEscopo2, totalEscopo3, usuario);

            if (!relatorio.IsValid)
            {
                return CommandResult.Falha(relatorio.Notifications.Select(n => n.Message));
            }

            _context.RelatoriosESG.Update(relatorio);
            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Relatório ESG anual consolidado com sucesso!", new
            {
                RelatorioId = relatorio.Id,
                Escopo1 = relatorio.TotalEscopo1,
                Escopo2 = relatorio.TotalEscopo2,
                Escopo3 = relatorio.TotalEscopo3,
                TotalCo2e = relatorio.TotalGeralCo2e
            });
        }
    }
}
