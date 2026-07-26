using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Shared.Domain.Events;
using Epros.Modules.Qualidade.Application.Commands;
using Epros.Modules.Qualidade.Domain.Entities;
using Epros.Modules.Qualidade.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Qualidade.Application.Handlers
{
    public class RegistrarResultadoInspecaoCommandHandler : ICommandHandler<RegistrarResultadoInspecaoCommand>
    {
        private readonly ContextQualidade _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public RegistrarResultadoInspecaoCommandHandler(
            ContextQualidade context,
            ITenantProvider tenantProvider,
            ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(RegistrarResultadoInspecaoCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var inspecao = await _context.InspecoesLote
                .FirstOrDefaultAsync(i => i.Id == request.InspecaoLoteId, cancellationToken);

            if (inspecao == null)
            {
                return CommandResult.Falha("Inspeção de lote não encontrada.");
            }

            inspecao.RegistrarResultado(request.Status, request.Responsavel, request.Observacoes, usuario);

            if (!inspecao.IsValid)
            {
                return CommandResult.Falha(inspecao.Notifications.Select(n => n.Message));
            }

            _context.InspecoesLote.Update(inspecao);

            // Se o lote for reprovado, gera automaticamente uma Não Conformidade (NCR)
            // e enfileira o evento no Outbox da Qualidade
            if (inspecao.Status == "Reprovado")
            {
                var ncr = new NaoConformidade(
                    inspecao.Id,
                    inspecao.Sku,
                    $"Lote Reprovado: {inspecao.NomeProduto}",
                    $"Inspeção realizada por {inspecao.Responsavel} em {inspecao.DataInspecao:g}. Observações: {inspecao.Observacoes}",
                    tenantId,
                    usuario
                );

                if (ncr.IsValid)
                {
                    _context.NaoConformidades.Add(ncr);
                }

                // Enfileira o evento InspecaoReprovada no Outbox da Qualidade
                var reprovadaPayload = new
                {
                    InspecaoLoteId = inspecao.Id,
                    TenantId = tenantId,
                    Sku = inspecao.Sku,
                    NomeProduto = inspecao.NomeProduto,
                    QuantidadeLote = inspecao.QuantidadeLote,
                    DataInspecao = inspecao.DataInspecao,
                    Responsavel = inspecao.Responsavel
                };

                var payloadJson = JsonSerializer.Serialize(reprovadaPayload);
                var outboxMessage = new OutboxMessage(tenantId, "InspecaoReprovada", payloadJson);
                _context.OutboxMessages.Add(outboxMessage);
            }

            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Resultado da inspeção registrado com sucesso!", new { InspecaoLoteId = inspecao.Id, Status = inspecao.Status });
        }
    }
}
