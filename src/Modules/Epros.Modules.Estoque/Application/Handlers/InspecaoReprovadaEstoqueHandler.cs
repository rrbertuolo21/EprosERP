using System;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Estoque.Infrastructure.Data;
using Epros.Shared.Domain.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Estoque.Application.Handlers
{
    /// <summary>
    /// Consome o evento InspecaoReprovada publicado pelo outbox do módulo de Qualidade.
    /// Realiza a baixa do estoque correspondente ao lote que foi reprovado na inspeção.
    /// </summary>
    public class InspecaoReprovadaEstoqueHandler : INotificationHandler<InspecaoReprovadaEventNotification>
    {
        private readonly ContextEstoque _context;

        public InspecaoReprovadaEstoqueHandler(ContextEstoque context)
        {
            _context = context;
        }

        public async Task Handle(InspecaoReprovadaEventNotification notification, CancellationToken cancellationToken)
        {
            var produto = await _context.Produtos
                .FirstOrDefaultAsync(p => p.Sku == notification.Sku, cancellationToken);

            if (produto == null)
            {
                // Produto não existe no estoque (provavelmente erro cadastral)
                return;
            }

            // Realiza a saída física do lote reprovado do estoque disponível
            // Se o saldo for menor que a quantidade, a lógica interna do Produto adicionará a notificação de erro,
            // que deve ser tratada ou logada.
            produto.LancarSaidaEstoque(notification.QuantidadeLote, "system_quality");

            if (produto.IsValid)
            {
                _context.Produtos.Update(produto);
                await _context.SaveChangesAsync(cancellationToken);
            }
        }
    }
}
