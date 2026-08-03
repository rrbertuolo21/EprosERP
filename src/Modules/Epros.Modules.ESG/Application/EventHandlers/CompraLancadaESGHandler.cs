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
    /// Captura o lancamento de compras e registra emissoes indiretas de Escopo 3 (cat. 1 — bens adquiridos).
    ///
    /// NF-01/A-01 (esg-carbono, Regra #0): o fator NAO e mais constante em codigo. E lido do catalogo
    /// versionado esg.ghg_fator_emissao (codigo <see cref="GhgFatorCodigos.BensAdquiridosPorPeca"/>).
    /// Sem fator oficial vigente, cada emissao entra como "pendente de fator" — sem numero inventado.
    /// </summary>
    public class CompraLancadaESGHandler : INotificationHandler<CompraLancadaEventNotification>
    {
        private const int EscopoCadeiaValor = 3;
        private const string CategoriaGhg = "BensEServicosAdquiridos";
        private const string Unidade = "PC";

        private readonly ContextESG _context;

        public CompraLancadaESGHandler(ContextESG context)
        {
            _context = context;
        }

        public async Task Handle(CompraLancadaEventNotification notification, CancellationToken cancellationToken)
        {
            if (notification.Itens == null) return;

            var dataTransacao = DateTime.UtcNow;

            // NF-01: fator do catalogo versionado (mesmo codigo para toda a categoria). // valida-humano (MCTI-SIN/IPCC/DEFRA)
            var fator = await ResolvedorFatorEmissaoGee.ResolverVigenteAsync(
                _context, notification.TenantId, GhgFatorCodigos.BensAdquiridosPorPeca,
                dataTransacao, cancellationToken);

            var houveAlteracao = false;
            foreach (var item in notification.Itens)
            {
                if (item.Quantidade <= 0) continue;

                var fonte = $"Compra de Insumo - {item.NomeProduto} ({item.Sku})";

                var emissao = fator != null
                    ? EmissaoCarbono.CalculadaComFator(
                        fonte, EscopoCadeiaValor, CategoriaGhg, item.Quantidade, Unidade,
                        fator, dataTransacao, notification.TenantId, "system_esg")
                    // Regra #0: sem fator homologado, registra pendencia — NAO emite numero inventado.
                    : EmissaoCarbono.PendenteDeFator(
                        fonte, EscopoCadeiaValor, CategoriaGhg, item.Quantidade, Unidade,
                        GhgFatorCodigos.BensAdquiridosPorPeca, dataTransacao,
                        notification.TenantId, "system_esg");

                if (emissao.IsValid)
                {
                    _context.EmissoesCarbono.Add(emissao);
                    houveAlteracao = true;
                }
            }

            if (houveAlteracao)
                await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
