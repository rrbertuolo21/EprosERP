using System;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Estoque.Application.Services;
using Epros.Modules.Estoque.Domain.Entities;
using Epros.Modules.Estoque.Domain.Enums;
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

            // Realiza a saída física do lote reprovado pelo MOTOR ÚNICO (kardex, D1). Se o saldo bloquear a
            // baixa (D8: default), a saída não é aplicada (paridade com o comportamento anterior, que
            // silenciosamente não persistia quando o saldo era insuficiente).
            var motor = new MotorMovimentacaoEstoque(_context, notification.TenantId, "system_quality");
            var fato = new FatoGeradorEstoque(null, null, null, EOrigemFatoGeradorEstoque.Avaria, notification.TenantId, "system_quality", referenciaExterna: $"Inspeção reprovada {notification.InspecaoLoteId}");
            _context.FatosGeradoresEstoque.Add(fato);

            var resSaida = await motor.AplicarSaidaAsync(MotorMovimentacaoEstoque.EmpresaPadrao, produto.Id, notification.QuantidadeLote, fato.Id, null, cancellationToken);
            if (resSaida.Sucesso)
            {
                await _context.SaveChangesAsync(cancellationToken);
            }
        }
    }
}
