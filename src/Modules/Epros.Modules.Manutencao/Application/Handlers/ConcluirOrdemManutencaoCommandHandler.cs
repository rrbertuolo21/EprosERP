using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Shared.Domain.Events;
using Epros.Modules.Manutencao.Application.Commands;
using Epros.Modules.Manutencao.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Manutencao.Application.Handlers
{
    public class ConcluirOrdemManutencaoCommandHandler : ICommandHandler<ConcluirOrdemManutencaoCommand>
    {
        private readonly ContextManutencao _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public ConcluirOrdemManutencaoCommandHandler(
            ContextManutencao context,
            ITenantProvider tenantProvider,
            ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(ConcluirOrdemManutencaoCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var ordem = await _context.OrdensManutencao
                .Include(o => o.Pecas)
                .FirstOrDefaultAsync(o => o.Id == request.OrdemManutencaoId, cancellationToken);

            if (ordem == null)
            {
                return CommandResult.Falha("Ordem de manutenção não encontrada.");
            }

            var equipamento = await _context.Equipamentos
                .FirstOrDefaultAsync(e => e.Id == ordem.EquipamentoId, cancellationToken);

            if (equipamento == null)
            {
                return CommandResult.Falha("Equipamento associado à OS não encontrado.");
            }

            ordem.ConcluirOrdem(request.DescricaoServicoExecutado, request.CustoMaoObra, usuario);

            if (!ordem.IsValid)
            {
                return CommandResult.Falha(ordem.Notifications.Select(n => n.Message));
            }

            // Retornar status do equipamento para Ativo
            equipamento.AlterarStatus("Ativo", usuario);

            // Enfileirar no outbox para que o Estoque processe a baixa física das peças consumidas
            var pecasPayload = ordem.Pecas.Select(p => new
            {
                ProdutoId = p.ProdutoId,
                Quantidade = p.Quantidade
            }).ToList();

            var payload = new
            {
                OrdemManutencaoId = ordem.Id,
                EquipamentoId = ordem.EquipamentoId,
                Pecas = pecasPayload,
                TenantId = tenantId
            };

            var payloadJson = JsonSerializer.Serialize(payload);
            var outboxMessage = new OutboxMessage(tenantId, "OrdemManutencaoConcluida", payloadJson);
            _context.OutboxMessages.Add(outboxMessage);

            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Ordem de serviço de manutenção concluída com sucesso!", new { OrdemManutencaoId = ordem.Id, Status = ordem.Status });
        }
    }
}
