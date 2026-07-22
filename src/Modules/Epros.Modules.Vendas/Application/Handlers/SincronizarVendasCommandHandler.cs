using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Modules.Vendas.Application.Commands;
using Epros.Modules.Vendas.Domain.Entities;
using Epros.Modules.Vendas.Infrastructure.Data;
using Epros.Shared.Domain.Events;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Vendas.Application.Handlers
{
    public class SincronizarVendasCommandHandler : ICommandHandler<SincronizarVendasCommand>
    {
        private readonly ContextVendas _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public SincronizarVendasCommandHandler(
            ContextVendas context,
            ITenantProvider tenantProvider,
            ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(SincronizarVendasCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            if (request.Vendas == null || !request.Vendas.Any())
            {
                return CommandResult.Ok("Nenhuma venda enviada para sincronização.");
            }

            int adicionadas = 0;
            int puladas = 0;

            foreach (var vendaInput in request.Vendas)
            {
                // Evita duplicidade baseando-se no SyncId ou Id
                var existe = await _context.Vendas
                    .IgnoreQueryFilters()
                    .AnyAsync(v => v.TenantId == tenantId && (v.SyncId == vendaInput.SyncId || v.Id == vendaInput.Id), cancellationToken);

                if (existe)
                {
                    puladas++;
                    continue;
                }

                var venda = new Venda(
                    vendaInput.Id == Guid.Empty ? Guid.NewGuid() : vendaInput.Id,
                    vendaInput.SyncId == Guid.Empty ? Guid.NewGuid() : vendaInput.SyncId,
                    vendaInput.CaixaId,
                    vendaInput.Total,
                    VendaStatusMapper.Mapear(vendaInput.Status),
                    tenantId,
                    usuario,
                    vendaInput.CriadoEm
                );

                if (!venda.IsValid)
                {
                    return CommandResult.Falha(venda.Notifications.Select(n => n.Message), $"Erro na validação da venda offline {vendaInput.SyncId}");
                }

                foreach (var itemInput in vendaInput.Itens)
                {
                    var item = new VendaItem(
                        venda.Id,
                        itemInput.ProdutoId,
                        itemInput.Quantidade,
                        itemInput.PrecoUnitario,
                        tenantId,
                        usuario
                    );

                    venda.AdicionarItem(item);
                }

                if (!venda.IsValid)
                {
                    return CommandResult.Falha(venda.Notifications.Select(n => n.Message), $"Erro na validação dos itens da venda {vendaInput.SyncId}");
                }

                _context.Vendas.Add(venda);

                // Enfileirar evento no Outbox de forma transacional
                var payloadObj = new
                {
                    VendaId = venda.Id,
                    TenantId = tenantId,
                    Total = venda.Total,
                    CriadoEm = venda.CriadoEm,
                    Itens = venda.Itens.Select(i => new
                    {
                        ProdutoId = i.ProdutoId,
                        Quantidade = i.Quantidade,
                        PrecoUnitario = i.PrecoUnitario
                    }).ToList()
                };

                var payloadJson = JsonSerializer.Serialize(payloadObj);
                var outboxMessage = new OutboxMessage(tenantId, "VendaFaturada", payloadJson);
                _context.OutboxMessages.Add(outboxMessage);

                adicionadas++;
            }

            if (adicionadas > 0)
            {
                await _context.SaveChangesAsync(cancellationToken);
            }

            return CommandResult.Ok($"Sincronização concluída com sucesso. Adicionadas: {adicionadas}, Puladas/Existentes: {puladas}", new { Adicionadas = adicionadas, Puladas = puladas });
        }
    }
}
