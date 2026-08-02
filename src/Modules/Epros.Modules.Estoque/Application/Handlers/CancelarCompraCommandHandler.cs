using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Shared.Domain.Events;
using Epros.Modules.Estoque.Application.Commands;
using Epros.Modules.Estoque.Application.Services;
using Epros.Modules.Estoque.Domain.Entities;
using Epros.Modules.Estoque.Domain.Enums;
using Epros.Modules.Estoque.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Estoque.Application.Handlers
{
    /// <summary>
    /// Handler para executar o cancelamento de uma compra no módulo de Estoque.
    /// Realiza a saída física do estoque (estorno) para cada item e dispara a integração.
    /// </summary>
    public class CancelarCompraCommandHandler : ICommandHandler<CancelarCompraCommand>
    {
        private readonly ContextEstoque _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public CancelarCompraCommandHandler(
            ContextEstoque context,
            ITenantProvider tenantProvider,
            ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(CancelarCompraCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            // 1. Localizar a compra ativa
            var compra = await _context.Compras
                .Include(c => c.Itens)
                .FirstOrDefaultAsync(c => c.Id == request.CompraId, cancellationToken);

            if (compra == null)
            {
                return CommandResult.Falha("Compra não encontrada.");
            }

            if (compra.Cancelada)
            {
                return CommandResult.Falha("Esta compra já está cancelada.");
            }

            // 2. Cancelar a compra no domínio
            compra.Cancelar(usuario);
            if (!compra.IsValid)
            {
                return CommandResult.Falha(compra.Notifications.Select(n => n.Message), "Erro ao atualizar status da compra.");
            }

            _context.Compras.Update(compra);

            // 3. Estornar cada item no estoque físico via MOTOR ÚNICO (kardex, D1).
            var motor = new MotorMovimentacaoEstoque(_context, tenantId, usuario);
            var fato = new FatoGeradorEstoque(null, compra.Id, null, EOrigemFatoGeradorEstoque.SaidaFiscal, tenantId, usuario, referenciaExterna: $"Estorno de compra NF {compra.NumeroNota}");
            _context.FatosGeradoresEstoque.Add(fato);

            foreach (var item in compra.Itens)
            {
                var produto = await _context.Produtos.FirstOrDefaultAsync(p => p.Id == item.ProdutoId, cancellationToken);
                if (produto == null)
                {
                    return CommandResult.Falha($"Produto com ID {item.ProdutoId} não encontrado.");
                }

                // Debitar a quantidade comprada do saldo (kardex)
                var resSaida = await motor.AplicarSaidaAsync(MotorMovimentacaoEstoque.EmpresaPadrao, produto.Id, item.Quantidade, fato.Id, null, cancellationToken);
                if (!resSaida.Sucesso)
                {
                    return CommandResult.Falha($"Erro ao estornar estoque para o produto {produto.Sku}: {resSaida.Erro}");
                }

                // Criar movimentação correspondente de Saída
                var movimento = new MovimentoEstoque(
                    produto.Id,
                    item.Quantidade,
                    "Saída",
                    $"Estorno de Compra - Cancelamento de NF-e nº {compra.NumeroNota}",
                    tenantId,
                    usuario
                );

                if (!movimento.IsValid)
                {
                    return CommandResult.Falha(movimento.Notifications.Select(n => n.Message), $"Erro ao registrar movimentação de estoque para o produto {produto.Sku}.");
                }

                _context.MovimentosEstoque.Add(movimento);
            }

            // 4. Enfileirar mensagem no Outbox
            var compraCanceladaEvent = new
            {
                CompraId = compra.Id,
                TenantId = tenantId,
                FornecedorCnpj = compra.FornecedorCnpj,
                NumeroNota = compra.NumeroNota,
                ValorTotal = compra.ValorTotal
            };

            var payloadJson = JsonSerializer.Serialize(compraCanceladaEvent);
            var outboxMessage = new OutboxMessage(tenantId, "CompraCancelada", payloadJson);
            _context.OutboxMessages.Add(outboxMessage);

            // Salvar tudo atomicamente em uma transação
            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Lançamento de compra cancelado com sucesso!");
        }
    }
}
