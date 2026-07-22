using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Estoque.Domain.Entities;
using Epros.Modules.Estoque.Infrastructure.Data;
using Epros.Shared.Domain.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Estoque.Application.Handlers
{
    /// <summary>
    /// Consome o evento OrdemManutencaoConcluida enviado pelo outbox do módulo de Manutenção.
    /// Deduz as peças utilizadas do inventário físico e loga a movimentação de saída.
    /// </summary>
    public class OrdemManutencaoConcluidaEstoqueHandler : INotificationHandler<OrdemManutencaoConcluidaEventNotification>
    {
        private readonly ContextEstoque _context;

        public OrdemManutencaoConcluidaEstoqueHandler(ContextEstoque context)
        {
            _context = context;
        }

        public async Task Handle(OrdemManutencaoConcluidaEventNotification notification, CancellationToken cancellationToken)
        {
            var historicoIdentificador = $"Baixa OS {notification.OrdemManutencaoId}";

            // 1. Evitar reprocessamento (idempotência)
            var jaProcessado = await _context.MovimentosEstoque
                .IgnoreQueryFilters()
                .AnyAsync(m => m.TenantId == notification.TenantId &&
                                m.Tipo == "Saida" &&
                                m.Historico.Contains(historicoIdentificador), cancellationToken);

            if (jaProcessado)
            {
                return;
            }

            // 2. Processar a saída de cada peça utilizada
            if (notification.Pecas != null)
            {
                foreach (var peca in notification.Pecas)
                {
                    var produto = await _context.Produtos
                        .IgnoreQueryFilters()
                        .FirstOrDefaultAsync(p => p.TenantId == notification.TenantId && p.Id == peca.ProdutoId, cancellationToken);

                    if (produto != null)
                    {
                        produto.LancarSaidaEstoque(peca.Quantidade, "system_maintenance");

                        var movimento = new MovimentoEstoque(
                            produtoId: peca.ProdutoId,
                            quantidade: peca.Quantidade,
                            tipo: "Saida",
                            historico: $"{historicoIdentificador} (Equipamento: {notification.EquipamentoId})",
                            tenantId: notification.TenantId,
                            criadoPor: "system_maintenance"
                        );

                        if (produto.IsValid)
                        {
                            _context.Produtos.Update(produto);
                            _context.MovimentosEstoque.Add(movimento);
                        }
                    }
                }

                await _context.SaveChangesAsync(cancellationToken);
            }
        }
    }
}
