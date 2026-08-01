using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Estoque.Infrastructure.Data;
using Epros.Shared.Domain.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Estoque.Application.Handlers
{
    /// <summary>
    /// Consome o evento OrdemProducaoEncerrada publicado pelo outbox do módulo de Produção.
    /// Realiza a baixa de todos os insumos e a entrada do produto acabado no estoque.
    /// </summary>
    public class OrdemProducaoEncerradaEstoqueHandler : INotificationHandler<OrdemProducaoEncerradaEventNotification>
    {
        private readonly ContextEstoque _context;

        public OrdemProducaoEncerradaEstoqueHandler(ContextEstoque context)
        {
            _context = context;
        }

        public async Task Handle(OrdemProducaoEncerradaEventNotification notification, CancellationToken cancellationToken)
        {
            decimal custoTotalProducao = 0;

            // 1. Processar as baixas de cada insumo do estoque e calcular custo total
            foreach (var insumoConsumido in notification.InsumosConsumidos)
            {
                var insumo = await _context.Produtos
                    .FirstOrDefaultAsync(p => p.Sku == insumoConsumido.InsumoSku, cancellationToken);

                if (insumo != null)
                {
                    // Acumula o custo baseado no custo médio atual do insumo no estoque
                    custoTotalProducao += insumo.CustoMedio * insumoConsumido.QuantidadeConsumida;

                    // Lança a saída do estoque
                    insumo.LancarSaidaEstoque(insumoConsumido.QuantidadeConsumida, "system_production");

                    if (insumo.IsValid)
                    {
                        _context.Produtos.Update(insumo);
                    }
                }
            }

            // 2. Processar a entrada do produto acabado no estoque
            if (notification.QuantidadeProduzida > 0)
            {
                var produtoAcabado = await _context.Produtos
                    .FirstOrDefaultAsync(p => p.Sku == notification.ProdutoAcabadoSku, cancellationToken);

                if (produtoAcabado != null)
                {
                    // Custo unitário de fabricação do produto acabado
                    decimal precoUnitario = custoTotalProducao / notification.QuantidadeProduzida;

                    // Lança a entrada do produto acabado no estoque
                    produtoAcabado.LancarEntradaEstoque(notification.QuantidadeProduzida, precoUnitario, "system_production");

                    if (produtoAcabado.IsValid)
                    {
                        _context.Produtos.Update(produtoAcabado);
                    }
                }
            }

            // Salva as alterações no estoque
            await _context.SaveChangesAsync(cancellationToken);

            // 3. Atualizar o custo final acumulado diretamente na Ordem de Produção no banco
            if (_context.Database.IsRelational())
            {
                await _context.Database.ExecuteSqlRawAsync(
                    "UPDATE producao.ordens_producao SET \"CustoTotalProducao\" = {0} WHERE \"Id\" = {1}",
                    custoTotalProducao,
                    notification.OrdemProducaoId,
                    cancellationToken);
            }
        }
    }
}
