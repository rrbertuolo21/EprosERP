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
    /// <summary>Handler de aplicação (VendaFaturadaEstoqueHandler).</summary>
    public class VendaFaturadaEstoqueHandler : INotificationHandler<VendaFaturadaEventNotification>
    {
        private readonly ContextEstoque _context;

        public VendaFaturadaEstoqueHandler(ContextEstoque context)
        {
            _context = context;
        }

        public async Task Handle(VendaFaturadaEventNotification notification, CancellationToken cancellationToken)
        {
            // Baixa de estoque pelo MOTOR ÚNICO (kardex, D1).
            var motor = new MotorMovimentacaoEstoque(_context, notification.TenantId, notification.UserId);

            foreach (var item in notification.Itens)
            {
                // 1. Validar idempotência
                var historicoIdentificador = $"Venda ID: {notification.VendaId}";
                var jaProcessado = await _context.MovimentosEstoque
                    .IgnoreQueryFilters()
                    .AnyAsync(m => m.TenantId == notification.TenantId &&
                                   m.ProdutoId == item.ProdutoId &&
                                   m.Tipo == "Saida" &&
                                   m.Historico.Contains(historicoIdentificador), cancellationToken);

                if (jaProcessado)
                {
                    continue; // Pula item já processado
                }

                // 2. Carregar o produto para atualizar o saldo físico
                var produto = await _context.Produtos
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(p => p.TenantId == notification.TenantId && p.Id == item.ProdutoId, cancellationToken);

                if (produto != null)
                {
                    // Efetua a saída do estoque pelo kardex (best-effort: preserva o registro físico mesmo
                    // quando o saldo bloqueia a baixa, mantendo a semântica anterior deste handler).
                    var fato = new FatoGeradorEstoque(notification.VendaId, null, null, EOrigemFatoGeradorEstoque.SaidaFiscal, notification.TenantId, notification.UserId, referenciaExterna: historicoIdentificador);
                    _context.FatosGeradoresEstoque.Add(fato);
                    await motor.AplicarSaidaAsync(MotorMovimentacaoEstoque.EmpresaPadrao, item.ProdutoId, item.Quantidade, fato.Id, null, cancellationToken);

                    // Cria o registro físico de movimentação de saída
                    var movimento = new MovimentoEstoque(
                        produtoId: item.ProdutoId,
                        quantidade: item.Quantidade,
                        tipo: "Saida",
                        historico: $"Saída de Venda Faturada ({historicoIdentificador})",
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
