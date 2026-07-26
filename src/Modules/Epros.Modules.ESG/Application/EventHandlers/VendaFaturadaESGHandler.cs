using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.ESG.Domain.Entities;
using Epros.Modules.ESG.Infrastructure.Data;
using Epros.Shared.Domain.Events;
using MediatR;

namespace Epros.Modules.ESG.Application.EventHandlers
{
    /// <summary>
    /// Captura o faturamento de vendas e calcula automaticamente a pegada logística downstream (Escopo 3).
    /// </summary>
    public class VendaFaturadaESGHandler : INotificationHandler<VendaFaturadaEventNotification>
    {
        private readonly ContextESG _context;

        public VendaFaturadaESGHandler(ContextESG context)
        {
            _context = context;
        }

        public async Task Handle(VendaFaturadaEventNotification notification, CancellationToken cancellationToken)
        {
            if (notification.Itens == null || !notification.Itens.Any()) return;

            decimal totalQuantidade = notification.Itens.Sum(i => i.Quantidade);
            // Intensidade média estimada de frete de entrega: 0.8 kg CO2e por peça transportada
            decimal fatorFretePadrao = 0.8m;

            var emissao = new EmissaoCarbono(
                fonteEmissao: $"Distribuição e Logística - Venda {notification.VendaId}",
                escopo: 3,
                categoriaGhg: "TransporteEDistribuicaoDownstream",
                quantidadeConsumo: totalQuantidade,
                unidadeMedida: "PC",
                fatorEmissao: fatorFretePadrao,
                dataTransacao: DateTime.UtcNow,
                tenantId: notification.TenantId,
                criadoPor: "system_esg"
            );

            if (emissao.IsValid)
            {
                _context.EmissoesCarbono.Add(emissao);
                await _context.SaveChangesAsync(cancellationToken);
            }
        }
    }
}
