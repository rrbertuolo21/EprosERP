using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Vendas.Application.Commands;
using Epros.Modules.Vendas.Domain.Entities;
using Epros.Modules.Vendas.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Shared.Domain.Events;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Vendas.Application.Handlers
{
    /// <summary>
    /// Cancela uma venda existente do tenant ativo. Rejeita se a venda não existir ou já estiver
    /// cancelada; caso contrário marca a venda como cancelada e enfileira, de forma transacional,
    /// a mensagem "VendaCancelada" no Outbox para estorno de estoque/financeiro.
    /// </summary>
    public class CancelarVendaCommandHandler : ICommandHandler<CancelarVendaCommand>
    {
        private readonly ContextVendas _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public CancelarVendaCommandHandler(
            ContextVendas context,
            ITenantProvider tenantProvider,
            ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        /// <summary>
        /// Processa o comando de cancelamento de venda, aplicando as regras de idempotência
        /// (venda inexistente / já cancelada) e gravando a mensagem de integração no Outbox.
        /// </summary>
        public async Task<CommandResult> Handle(CancelarVendaCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var userId = _currentUser.GetUserId() ?? "system";

            // Busca a venda e seus itens com isolamento do tenant ativo
            var venda = await _context.Vendas
                .Include(v => v.Itens)
                .FirstOrDefaultAsync(v => v.TenantId == tenantId && v.Id == request.VendaId, cancellationToken);

            if (venda == null)
            {
                return CommandResult.Falha(new[] { "Venda não encontrada." });
            }

            if (venda.Cancelada)
            {
                return CommandResult.Falha(new[] { "Esta venda já se encontra cancelada." });
            }

            // Altera o estado da venda para cancelada
            venda.Cancelar(userId);

            if (!venda.IsValid)
            {
                return CommandResult.Falha(venda.Notifications.Select(n => n.Message));
            }

            // Prepara o payload de integração
            var payload = new
            {
                VendaId = venda.Id,
                TenantId = tenantId,
                Total = venda.Total,
                CanceladoEm = DateTime.UtcNow,
                Itens = venda.Itens.Select(i => new
                {
                    ProdutoId = i.ProdutoId,
                    Quantidade = i.Quantidade
                }).ToList()
            };

            var payloadJson = JsonSerializer.Serialize(payload);
            
            // Grava a mensagem na fila do Outbox de forma transacional
            var outboxMessage = new OutboxMessage(tenantId, "VendaCancelada", payloadJson);
            _context.OutboxMessages.Add(outboxMessage);

            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Venda cancelada com sucesso.", new { VendaId = venda.Id });
        }
    }
}
