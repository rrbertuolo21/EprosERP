using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.ESG.Domain.Entities;
using Epros.Modules.ESG.Infrastructure.Data;
using Epros.Shared.Domain.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.ESG.Application.EventHandlers
{
    /// <summary>
    /// Captura o lançamento de compras no estoque e gera automaticamente emissões indiretas de Escopo 3 (bens adquiridos).
    /// </summary>
    public class CompraLancadaESGHandler : INotificationHandler<CompraLancadaEventNotification>
    {
        private readonly ContextESG _context;

        public CompraLancadaESGHandler(ContextESG context)
        {
            _context = context;
        }

        public async Task Handle(CompraLancadaEventNotification notification, CancellationToken cancellationToken)
        {
            if (notification.Itens == null) return;

            foreach (var item in notification.Itens)
            {
                // Intensidade padrão de 1.5 kg CO2e por peça adquirida na cadeia de suprimentos
                decimal fatorEmissaoPadrao = 1.5m;

                var emissao = new EmissaoCarbono(
                    fonteEmissao: $"Compra de Insumo - {item.NomeProduto} ({item.Sku})",
                    escopo: 3,
                    categoriaGhg: "BensEServicosAdquiridos",
                    quantidadeConsumo: item.Quantidade,
                    unidadeMedida: "PC",
                    fatorEmissao: fatorEmissaoPadrao,
                    dataTransacao: DateTime.UtcNow,
                    tenantId: notification.TenantId,
                    criadoPor: "system_esg"
                );

                if (emissao.IsValid)
                {
                    _context.EmissoesCarbono.Add(emissao);
                }
            }

            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
