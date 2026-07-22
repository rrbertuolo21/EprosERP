using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Shared.Domain.Events;
using Epros.Modules.Producao.Application.Commands;
using Epros.Modules.Producao.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Producao.Application.Handlers
{
    public class EncerrarOrdemProducaoCommandHandler : ICommandHandler<EncerrarOrdemProducaoCommand>
    {
        private readonly ContextProducao _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public EncerrarOrdemProducaoCommandHandler(
            ContextProducao context,
            ITenantProvider tenantProvider,
            ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(EncerrarOrdemProducaoCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var ordem = await _context.OrdensProducao
                .Include(o => o.Apontamentos)
                .FirstOrDefaultAsync(o => o.Id == request.OrdemProducaoId, cancellationToken);

            if (ordem == null)
            {
                return CommandResult.Falha("Ordem de Produção não encontrada.");
            }

            if (ordem.Status == "Encerrada")
            {
                return CommandResult.Falha("A Ordem de Produção já está encerrada.");
            }

            // Obter a BOM ativa do produto
            var bom = await _context.ListasMateriais
                .Include(l => l.Itens)
                .FirstOrDefaultAsync(l => l.ProdutoAcabadoSku == ordem.ProdutoAcabadoSku && l.Ativa, cancellationToken);

            if (bom == null)
            {
                return CommandResult.Falha($"Não foi encontrada nenhuma ficha técnica (BOM) ativa para o SKU '{ordem.ProdutoAcabadoSku}' para realizar a baixa dos componentes.");
            }

            // Encerrar a OP localmente (o custo será calculado e atualizado pelo estoque)
            ordem.EncerrarOrdemProducao(0, usuario);

            if (!ordem.IsValid)
            {
                return CommandResult.Falha(ordem.Notifications.Select(n => n.Message));
            }

            // Gerar evento de integração de encerramento da OP no Outbox da Produção
            var totalProduzidoRefugado = ordem.QuantidadeProduzida + ordem.QuantidadeRefugada;
            var payload = new
            {
                OrdemProducaoId = ordem.Id,
                Codigo = ordem.Codigo,
                ProdutoAcabadoSku = ordem.ProdutoAcabadoSku,
                QuantidadeProduzida = ordem.QuantidadeProduzida,
                QuantidadeRefugada = ordem.QuantidadeRefugada,
                TenantId = tenantId,
                InsumosConsumidos = bom.Itens.Select(item => new
                {
                    item.InsumoSku,
                    QuantidadeConsumida = item.QuantidadeNecessaria * totalProduzidoRefugado
                }).ToList()
            };

            var payloadJson = JsonSerializer.Serialize(payload);
            var outboxMessage = new OutboxMessage(tenantId, "OrdemProducaoEncerrada", payloadJson);
            _context.OutboxMessages.Add(outboxMessage);

            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Ordem de Produção encerrada com sucesso! As baixas de estoque foram enviadas para processamento.", new { OrdemProducaoId = ordem.Id, Status = ordem.Status });
        }
    }
}
