using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Shared.Domain.Events;
using Epros.Modules.RH.Application.Commands;
using Epros.Modules.RH.Domain.Entities;
using Epros.Modules.RH.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.RH.Application.Handlers
{
    public class ProcessarFolhaPagamentoCommandHandler : ICommandHandler<ProcessarFolhaPagamentoCommand>
    {
        private readonly ContextRH _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public ProcessarFolhaPagamentoCommandHandler(
            ContextRH context,
            ITenantProvider tenantProvider,
            ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(ProcessarFolhaPagamentoCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            // Buscar colaborador
            var colaborador = await _context.Colaboradores
                .FirstOrDefaultAsync(c => c.Id == request.ColaboradorId, cancellationToken);

            if (colaborador == null)
            {
                return CommandResult.Falha("Colaborador não encontrado.");
            }

            if (colaborador.Status != "Ativo")
            {
                return CommandResult.Falha("Não é possível processar folha de pagamento para um colaborador desligado.");
            }

            // Verificar se já existe folha processada nesta competência
            var folhaExiste = await _context.FolhasPagamento
                .AnyAsync(f => f.ColaboradorId == request.ColaboradorId
                            && f.MesCompetencia == request.MesCompetencia
                            && f.AnoCompetencia == request.AnoCompetencia, cancellationToken);

            if (folhaExiste)
            {
                return CommandResult.Falha($"Já existe uma folha de pagamento processada para este colaborador na competência {request.MesCompetencia:D2}/{request.AnoCompetencia}.");
            }

            // Instanciar a folha com o salário base como base inicial do Bruto
            var folha = new FolhaPagamento(
                colaborador.Id,
                request.MesCompetencia,
                request.AnoCompetencia,
                colaborador.SalarioBase,
                tenantId,
                usuario
            );

            // Adicionar as verbas informadas no comando
            if (request.Verbas != null)
            {
                foreach (var verba in request.Verbas)
                {
                    folha.AdicionarVerba(verba.Descricao, verba.Tipo, verba.Valor, usuario);
                }
            }

            if (!folha.IsValid)
            {
                return CommandResult.Falha(folha.Notifications.Select(n => n.Message));
            }

            _context.FolhasPagamento.Add(folha);

            // Enfileirar evento de folha processada para integração de contas a pagar no Financeiro
            var payload = new
            {
                FolhaPagamentoId = folha.Id,
                ColaboradorId = colaborador.Id,
                NomeColaborador = colaborador.Nome,
                CpfColaborador = colaborador.Cpf,
                MesCompetencia = folha.MesCompetencia,
                AnoCompetencia = folha.AnoCompetencia,
                SalarioLiquido = folha.SalarioLiquido,
                TenantId = tenantId
            };

            var payloadJson = JsonSerializer.Serialize(payload);
            var outboxMessage = new OutboxMessage(tenantId, "FolhaProcessada", payloadJson);
            _context.OutboxMessages.Add(outboxMessage);

            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Folha de pagamento processada com sucesso!", new { FolhaPagamentoId = folha.Id, SalarioLiquido = folha.SalarioLiquido });
        }
    }
}
