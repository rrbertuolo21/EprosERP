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
    /// Captura o faturamento de vendas e registra a pegada logistica downstream (Escopo 3, cat. 9).
    ///
    /// NF-01/A-01 (esg-carbono, Regra #0): o fator NAO e mais constante em codigo. E lido do catalogo
    /// versionado esg.ghg_fator_emissao (codigo <see cref="GhgFatorCodigos.TransporteDistribuicaoDownstreamPorPeca"/>).
    /// Sem fator oficial vigente, a emissao entra como "pendente de fator" — sem numero inventado.
    /// </summary>
    public class VendaFaturadaESGHandler : INotificationHandler<VendaFaturadaEventNotification>
    {
        private const int EscopoCadeiaValor = 3;
        private const string CategoriaGhg = "TransporteEDistribuicaoDownstream";
        private const string Unidade = "PC";

        private readonly ContextESG _context;

        public VendaFaturadaESGHandler(ContextESG context)
        {
            _context = context;
        }

        public async Task Handle(VendaFaturadaEventNotification notification, CancellationToken cancellationToken)
        {
            if (notification.Itens == null || !notification.Itens.Any()) return;

            decimal totalQuantidade = notification.Itens.Sum(i => i.Quantidade);
            if (totalQuantidade <= 0) return;

            var dataTransacao = notification.CriadoEm == default ? DateTime.UtcNow : notification.CriadoEm;
            var fonte = $"Distribuição e Logística - Venda {notification.VendaId}";

            // NF-01: fator vem do catalogo versionado, nunca de constante. // valida-humano (MCTI-SIN/IPCC/DEFRA)
            var fator = await ResolvedorFatorEmissaoGee.ResolverVigenteAsync(
                _context, notification.TenantId, GhgFatorCodigos.TransporteDistribuicaoDownstreamPorPeca,
                dataTransacao, cancellationToken);

            var emissao = fator != null
                ? EmissaoCarbono.CalculadaComFator(
                    fonte, EscopoCadeiaValor, CategoriaGhg, totalQuantidade, Unidade,
                    fator, dataTransacao, notification.TenantId, "system_esg")
                // Regra #0: sem fator homologado, registra pendencia — NAO emite numero inventado.
                : EmissaoCarbono.PendenteDeFator(
                    fonte, EscopoCadeiaValor, CategoriaGhg, totalQuantidade, Unidade,
                    GhgFatorCodigos.TransporteDistribuicaoDownstreamPorPeca, dataTransacao,
                    notification.TenantId, "system_esg");

            if (emissao.IsValid)
            {
                _context.EmissoesCarbono.Add(emissao);
                await _context.SaveChangesAsync(cancellationToken);
            }
        }
    }
}
