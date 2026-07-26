using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Modules.Manutencao.Application.Commands;
using Epros.Modules.Manutencao.Domain.Entities;
using Epros.Modules.Manutencao.Infrastructure.Data;

namespace Epros.Modules.Manutencao.Application.Handlers
{
    public class CriarEquipamentoCommandHandler : ICommandHandler<CriarEquipamentoCommand>
    {
        private readonly ContextManutencao _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public CriarEquipamentoCommandHandler(
            ContextManutencao context,
            ITenantProvider tenantProvider,
            ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(CriarEquipamentoCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var equipamento = new Equipamento(
                request.Nome,
                request.Codigo,
                request.Setor,
                request.DataAquisicao,
                request.Criticidade,
                tenantId,
                usuario
            );

            if (!equipamento.IsValid)
            {
                return CommandResult.Falha(equipamento.Notifications.Select(n => n.Message));
            }

            _context.Equipamentos.Add(equipamento);
            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Equipamento cadastrado com sucesso!", new { EquipamentoId = equipamento.Id });
        }
    }
}
