using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Shared.Domain.Events;
using Epros.Modules.Projetos.Application.Commands;
using Epros.Modules.Projetos.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Projetos.Application.Handlers
{
    public class AtualizarProgressoTarefaCommandHandler : ICommandHandler<AtualizarProgressoTarefaCommand>
    {
        private readonly ContextProjetos _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public AtualizarProgressoTarefaCommandHandler(
            ContextProjetos context,
            ITenantProvider tenantProvider,
            ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(AtualizarProgressoTarefaCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var projeto = await _context.Projetos
                .Include(p => p.ItensWbs)
                .FirstOrDefaultAsync(p => p.Id == request.ProjetoId, cancellationToken);

            if (projeto == null)
            {
                return CommandResult.Falha("Projeto não encontrado.");
            }

            var (oldProgress, newProgress) = projeto.AtualizarProgressoTarefa(
                request.WbsItemId,
                request.PercentualConclusao,
                usuario
            );

            if (!projeto.IsValid)
            {
                return CommandResult.Falha(projeto.Notifications.Select(n => n.Message));
            }

            // Ativa o projeto automaticamente se o progresso iniciar (Rascunho -> Ativo). MM-a status canônico.
            if (oldProgress == 0 && newProgress > 0 && projeto.Status == Epros.Modules.Projetos.Domain.Enums.EProjetoWorkflowStatus.Rascunho)
            {
                projeto.Ativar(usuario);
            }

            // Verificar marcos de faturamento parciais (25%, 50%, 75%, 100%)
            var milestones = new[] { 25m, 50m, 75m, 100m };
            foreach (var milestone in milestones)
            {
                if (oldProgress < milestone && newProgress >= milestone)
                {
                    var valorFaturamento = projeto.OrcamentoTotal * 0.25m; // 25% do orçamento por marco
                    var payload = new
                    {
                        ProjetoId = projeto.Id,
                        NomeProjeto = projeto.Nome,
                        ClienteId = projeto.ClienteId,
                        Milestone = milestone,
                        ValorFaturamento = valorFaturamento,
                        TenantId = tenantId
                    };
                    var payloadJson = JsonSerializer.Serialize(payload);
                    var outboxMessage = new OutboxMessage(tenantId, "ProjetoFaturado", payloadJson);
                    _context.OutboxMessages.Add(outboxMessage);
                }
            }

            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Progresso atualizado com sucesso!", new { ProjetoId = projeto.Id, PercentualConclusao = projeto.PercentualConclusao, Status = projeto.Status });
        }
    }
}
