using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Shared.Domain.Events;
using Epros.Modules.Estoque.Application.Commands;
using Epros.Modules.Estoque.Domain.Entities;
using Epros.Modules.Estoque.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Estoque.Application.Handlers
{
    /// <summary>Lança Compra.</summary>
    public class LancarCompraCommandHandler : ICommandHandler<LancarCompraCommand>
    {
        private readonly ContextEstoque _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public LancarCompraCommandHandler(
            ContextEstoque context,
            ITenantProvider tenantProvider,
            ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(LancarCompraCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            // 1. Evitar lançamento em duplicidade de nota fiscal de compra (ChaveAcesso única por tenant)
            var notaExistente = await _context.Compras.AnyAsync(c => c.ChaveAcesso == request.ChaveAcesso, cancellationToken);
            if (notaExistente)
            {
                return CommandResult.Falha("Esta nota fiscal já foi lançada para este inquilino.");
            }

            // 2. Instanciar agregado raiz de Compra
            var compra = new Compra(
                request.FornecedorCnpj,
                request.FornecedorNome,
                request.NumeroNota,
                request.ChaveAcesso,
                request.ValorTotal,
                request.DataEmissao,
                tenantId,
                usuario
            );

            if (!compra.IsValid)
            {
                return CommandResult.Falha(compra.Notifications.Select(n => n.Message), "Dados de cabeçalho da nota fiscal são inválidos.");
            }

            // 3. Processar cada item da nota
            foreach (var itemInput in request.Itens)
            {
                // Buscar produto por SKU no tenant ativo
                var produto = await _context.Produtos.FirstOrDefaultAsync(p => p.Sku == itemInput.Sku, cancellationToken);
                if (produto == null)
                {
                    // Registro automático do produto a partir da nota
                    produto = new Produto(itemInput.Sku, itemInput.NomeProduto, 0m, tenantId, usuario);
                    if (!produto.IsValid)
                    {
                        return CommandResult.Falha(produto.Notifications.Select(n => n.Message), $"Erro ao criar produto com SKU {itemInput.Sku}.");
                    }
                    _context.Produtos.Add(produto);
                }

                // Incrementar estoque e recalcular custo médio
                produto.LancarEntradaEstoque(itemInput.Quantidade, itemInput.PrecoUnitario, usuario);
                if (!produto.IsValid)
                {
                    return CommandResult.Falha(produto.Notifications.Select(n => n.Message), $"Erro ao lançar entrada de estoque para o SKU {itemInput.Sku}.");
                }

                // Gerar registro de movimentação física do estoque
                var movimento = new MovimentoEstoque(
                    produto.Id,
                    itemInput.Quantidade,
                    "Entrada",
                    $"Lançamento de compra - NF-e nº {request.NumeroNota}",
                    tenantId,
                    usuario
                );

                if (!movimento.IsValid)
                {
                    return CommandResult.Falha(movimento.Notifications.Select(n => n.Message), $"Erro ao criar movimentação de estoque para o SKU {itemInput.Sku}.");
                }
                _context.MovimentosEstoque.Add(movimento);

                // Associar o item à compra
                compra.AdicionarItem(produto.Id, itemInput.Quantidade, itemInput.PrecoUnitario, itemInput.ValorIms, itemInput.ValorIpi, usuario);
            }

            // Propagar notificações de itens de compra inválidos
            foreach (var item in compra.Itens)
            {
                if (!item.IsValid)
                {
                    compra.AddNotifications(item.Notifications);
                }
            }

            if (!compra.IsValid)
            {
                return CommandResult.Falha(compra.Notifications.Select(n => n.Message), "Invariantes de domínio da nota fiscal não foram atendidas.");
            }

            // 4. Persistir a compra
            _context.Compras.Add(compra);

            // 5. Enfileirar evento no Outbox de forma transacional
            var compraLancadaEvent = new
            {
                CompraId = compra.Id,
                TenantId = tenantId,
                FornecedorCnpj = compra.FornecedorCnpj,
                FornecedorNome = compra.FornecedorNome,
                NumeroNota = compra.NumeroNota,
                ValorTotal = compra.ValorTotal,
                Itens = request.Itens.Select(i => new { i.Sku, i.NomeProduto, i.Quantidade, i.PrecoUnitario, ValorTotal = i.Quantidade * i.PrecoUnitario }).ToList()
            };

            var payloadJson = JsonSerializer.Serialize(compraLancadaEvent);
            var outboxMessage = new OutboxMessage(tenantId, "CompraLancada", payloadJson);
            _context.OutboxMessages.Add(outboxMessage);

            // Salvar todas as modificações atomicamente
            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Nota fiscal lançada com sucesso!", new { CompraId = compra.Id });
        }
    }
}
