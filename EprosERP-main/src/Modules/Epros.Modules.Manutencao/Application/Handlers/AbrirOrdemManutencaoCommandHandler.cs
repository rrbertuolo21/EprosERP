using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Modules.Manutencao.Application.Commands;
using Epros.Modules.Manutencao.Domain.Entities;
using Epros.Modules.Manutencao.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Manutencao.Application.Handlers
{
    public class AbrirOrdemManutencaoCommandHandler : ICommandHandler<AbrirOrdemManutencaoCommand>
    {
        private readonly ContextManutencao _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public AbrirOrdemManutencaoCommandHandler(
            ContextManutencao context,
            ITenantProvider tenantProvider,
            ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(AbrirOrdemManutencaoCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var equipamento = await _context.Equipamentos
                .FirstOrDefaultAsync(e => e.Id == request.EquipamentoId, cancellationToken);

            if (equipamento == null)
            {
                return CommandResult.Falha("Equipamento não encontrado.");
            }

            if (equipamento.Status == "Inativo")
            {
                return CommandResult.Falha("Não é possível abrir ordem de serviço para um equipamento Inativo.");
            }

            // Mudar status do equipamento para indicar que está sob intervenção
            equipamento.AlterarStatus("EmManutencao", usuario);

            var ordem = new OrdemManutencao(
                request.EquipamentoId,
                request.Tipo,
                request.DescricaoProblema,
                tenantId,
                usuario
            );

            if (!ordem.IsValid)
            {
                return CommandResult.Falha(ordem.Notifications.Select(n => n.Message));
            }

            _context.OrdensManutencao.Add(ordem);
            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Ordem de serviço de manutenção aberta com sucesso!", new { OrdemManutencaoId = ordem.Id });
        }
    }
}
