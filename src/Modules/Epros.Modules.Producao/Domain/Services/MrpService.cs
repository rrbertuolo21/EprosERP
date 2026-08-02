using System;
using System.Collections.Generic;
using System.Linq;
using Epros.Modules.Producao.Domain.Entities;
using Epros.Modules.Producao.Domain.Enums;

namespace Epros.Modules.Producao.Domain.Services
{
    /// <summary>
    /// PRD-MRP — Motor de planejamento de necessidades de material (PD3 · DP-MRP-004/006/007/008/015/016/017).
    /// Explode a demanda pela BOM vigente multinível, agrega a necessidade bruta por item, faz o netting
    /// (bruta + estoque de segurança − disponibilidade − recebimentos programados = líquida), aplica lote
    /// mínimo/múltiplo, e gera sugestões (compra para item sem estrutura; produção para item com estrutura).
    /// BOM sem componente/estrutura → cálculo incompleto (DP-MRP-006/007/008), sem inventar demanda faltante.
    /// Motor puro de domínio, testável, reusando <see cref="BomExplosaoService"/>.
    /// </summary>
    public sealed class MrpService
    {
        public sealed record Demanda(Guid ItemId, decimal Quantidade, DateTime DataReferencia, Guid? VariacaoId = null);

        public sealed class ParametrosMrp
        {
            public Func<Guid, decimal> Disponibilidade { get; init; } = _ => 0m;
            public Func<Guid, decimal> RecebimentosProgramados { get; init; } = _ => 0m;
            public Func<Guid, decimal> EstoqueSeguranca { get; init; } = _ => 0m;
            public Func<Guid, decimal> LoteMinimo { get; init; } = _ => 0m;
            public Func<Guid, decimal> LoteMultiplo { get; init; } = _ => 0m;
        }

        public sealed record NecessidadeCalculada(
            Guid ItemId, int Nivel, decimal Bruta, decimal Disponibilidade,
            decimal Recebimentos, decimal EstoqueSeguranca, decimal Liquida);

        public sealed record SugestaoCalculada(Guid ItemId, ETipoSugestaoMrp Tipo, decimal Quantidade);

        public sealed class ResultadoMrp
        {
            public IReadOnlyList<NecessidadeCalculada> Necessidades { get; init; } = Array.Empty<NecessidadeCalculada>();
            public IReadOnlyList<SugestaoCalculada> Sugestoes { get; init; } = Array.Empty<SugestaoCalculada>();
            public bool CalculoIncompleto { get; init; }
            public string? MotivoIncompleto { get; init; }
        }

        private readonly BomExplosaoService _explosao = new();

        public ResultadoMrp Planejar(
            IEnumerable<Demanda> demandas,
            IReadOnlyCollection<BomEstrutura> catalogo,
            ParametrosMrp parametros,
            DateTime dataReferencia)
        {
            parametros ??= new ParametrosMrp();
            var demandaList = demandas?.ToList() ?? throw new ArgumentNullException(nameof(demandas));

            var brutaPorItem = new Dictionary<Guid, decimal>();
            var nivelPorItem = new Dictionary<Guid, int>();
            var incompleto = false;
            string? motivo = null;

            void Acumular(Guid item, decimal qtd, int nivel)
            {
                brutaPorItem[item] = brutaPorItem.TryGetValue(item, out var v) ? v + qtd : qtd;
                nivelPorItem[item] = nivelPorItem.TryGetValue(item, out var n) ? Math.Min(n, nivel) : nivel;
            }

            foreach (var d in demandaList)
            {
                // o próprio item demandado é necessidade bruta de nível 0
                Acumular(d.ItemId, d.Quantidade, 0);

                var estrutura = _explosao.SelecionarVigente(catalogo, d.ItemId, d.VariacaoId ?? Guid.Empty, d.DataReferencia);
                if (estrutura == null) continue; // item comprado/sem BOM: só necessidade própria

                var res = _explosao.Explodir(estrutura, d.Quantidade, catalogo, d.DataReferencia);
                if (res.PossuiCiclo)
                    return new ResultadoMrp { CalculoIncompleto = true, MotivoIncompleto = "Estrutura com ciclo (BOM-REG-019)." };
                if (res.CalculoIncompleto)
                {
                    incompleto = true;
                    motivo = res.MotivoIncompleto;
                }
                foreach (var item in res.Itens)
                    Acumular(item.ItemId, item.QuantidadeTotal, item.Nivel + 1);
            }

            var necessidades = new List<NecessidadeCalculada>();
            var sugestoes = new List<SugestaoCalculada>();

            foreach (var kv in brutaPorItem)
            {
                var item = kv.Key;
                var bruta = kv.Value;
                var disp = parametros.Disponibilidade(item);
                var receb = parametros.RecebimentosProgramados(item);
                var seg = parametros.EstoqueSeguranca(item);

                var liquida = bruta + seg - disp - receb;
                if (liquida < 0m) liquida = 0m;

                // lote mínimo / múltiplo (DP-MRP netting de tamanho de lote)
                if (liquida > 0m)
                {
                    var loteMin = parametros.LoteMinimo(item);
                    if (loteMin > 0m && liquida < loteMin) liquida = loteMin;

                    var mult = parametros.LoteMultiplo(item);
                    if (mult > 0m)
                    {
                        var passos = Math.Ceiling(liquida / mult);
                        liquida = passos * mult;
                    }
                }

                var nivel = nivelPorItem[item];
                necessidades.Add(new NecessidadeCalculada(item, nivel, bruta, disp, receb, seg, liquida));

                if (liquida > 0m)
                {
                    var temEstrutura = _explosao.SelecionarVigente(catalogo, item, Guid.Empty, dataReferencia) != null;
                    var tipo = temEstrutura ? ETipoSugestaoMrp.Producao : ETipoSugestaoMrp.Compra;
                    sugestoes.Add(new SugestaoCalculada(item, tipo, liquida));
                }
            }

            return new ResultadoMrp
            {
                Necessidades = necessidades.OrderBy(n => n.Nivel).ToList(),
                Sugestoes = sugestoes,
                CalculoIncompleto = incompleto,
                MotivoIncompleto = motivo
            };
        }
    }
}
