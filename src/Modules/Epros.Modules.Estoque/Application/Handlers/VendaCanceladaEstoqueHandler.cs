using System;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Estoque.Domain.Entities;
using Epros.Modules.Estoque.Infrastructure.Data;
using Epros.Shared.Domain.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Estoque.Application.Handlers
{
    /// <summary>Handler de aplicação (VendaCanceladaEstoqueHandler).</summary>
    public class VendaCanceladaEstoqueHandler : INotificationHandler<VendaCanceladaEventNotification>
    {
        private readonly ContextEstoque _context;

        public VendaCanceladaEstoqueHandler(ContextEstoque context)
        {
            _context = context;
        }

        public async Task Handle(VendaCanceladaEventNotification notification, CancellationToken cancellationToken)
        {
            foreach (var item in notification.Itens)
            {
                // 1. Validar idempotência antes de aplicar o estorno
                var historicoIdentificador = $"Venda ID: {notification.VendaId}";
                var jaProcessado = await _context.MovimentosEstoque
                    .IgnoreQueryFilters()
                    .AnyAsync(m => m.TenantId == notification.TenantId && 
                                   m.ProdutoId == item.ProdutoId && 
                                   m.Tipo == "Entrada" && 
                                   m.Historico.Contains(historicoIdentificador), cancellationToken);

                if (jaProcessado)
                {
                    continue; // Pula item já estornado anteriormente
                }

                // 2. Carregar o produto para atualizar o saldo físico
                var produto = await _context.Produtos
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(p => p.TenantId == notification.TenantId && p.Id == item.ProdutoId, cancellationToken);

                if (produto != null)
                {
                    // Restaura a quantidade física de estoque
                    produto.RestaurarEstoque(item.Quantidade, notification.UserId);

                    // Cria o registro físico de movimentação de entrada/devolução
                    var movimento = new MovimentoEstoque(
                        produtoId: item.ProdutoId,
                        quantidade: item.Quantidade,
                        tipo: "Entrada",
                        historico: $"Devolução de Venda Cancelada ({historicoIdentificador})",
                        tenantId: notification.TenantId,
                        criadoPor: notification.UserId
                    );

                    _context.MovimentosEstoque.Add(movimento);
                }
            }

            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
