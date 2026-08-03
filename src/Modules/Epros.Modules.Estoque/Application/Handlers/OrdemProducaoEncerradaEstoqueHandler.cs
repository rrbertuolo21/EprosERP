using System;
using System.Linq;
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

            // Movimentação pelo MOTOR ÚNICO (kardex, D1).
            var motor = new MotorMovimentacaoEstoque(_context, notification.TenantId, "system_production");
            var fato = new FatoGeradorEstoque(null, null, null, EOrigemFatoGeradorEstoque.Producao, notification.TenantId, "system_production", referenciaExterna: $"OP {notification.OrdemProducaoId}");
            _context.FatosGeradoresEstoque.Add(fato);

            // 1. Processar as baixas de cada insumo do estoque e calcular custo total
            foreach (var insumoConsumido in notification.InsumosConsumidos)
            {
                var insumo = await _context.Produtos
                    .FirstOrDefaultAsync(p => p.Sku == insumoConsumido.InsumoSku, cancellationToken);

                if (insumo != null)
                {
                    // Acumula o custo baseado no custo médio vigente do insumo no kardex
                    var custoMedioInsumo = await _context.EstoqueProdutos
                        .Where(e => e.EmpresaId == MotorMovimentacaoEstoque.EmpresaPadrao && e.ProdutoId == insumo.Id)
                        .Select(e => (decimal?)e.ValorCustoMedio)
                        .FirstOrDefaultAsync(cancellationToken) ?? insumo.CustoMedio;
                    custoTotalProducao += custoMedioInsumo * insumoConsumido.QuantidadeConsumida;

                    // Lança a saída do estoque (kardex)
                    await motor.AplicarSaidaAsync(MotorMovimentacaoEstoque.EmpresaPadrao, insumo.Id, insumoConsumido.QuantidadeConsumida, fato.Id, null, cancellationToken);
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

                    // Lança a entrada do produto acabado no estoque (kardex)
                    await motor.AplicarEntradaAsync(MotorMovimentacaoEstoque.EmpresaPadrao, produtoAcabado.Id, ETipoEstoque.Geral, notification.QuantidadeProduzida, precoUnitario, fato.Id, null, null, null, ETipoCusteioEstoque.CustoMedio, cancellationToken);
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
