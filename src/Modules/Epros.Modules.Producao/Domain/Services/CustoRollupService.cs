using System;
using System.Collections.Generic;
using System.Linq;
using Epros.Modules.Producao.Domain.Entities;
using Epros.Modules.Producao.Domain.Enums;

namespace Epros.Modules.Producao.Domain.Services
{
    /// <summary>
    /// PRD-CST — Motor de custeio com rollup multinível (PD8 · DP-CST-012 · DP-BOM-008/015/018).
    /// Apura o custo PREVISTO de uma estrutura subindo pela árvore da BOM: o custo de uma submontagem
    /// (material + MOD + CIF + extra) entra como insumo do nível-pai; folhas usam a valoração do Estoque.
    ///
    /// ⚠️ valida-contador (Regra #0 · DP-CST-005/006): a TAXA de mão-de-obra direta (MOD) e o CRITÉRIO/BASE
    /// de rateio dos custos indiretos de fabricação (CIF) NÃO se inventam — são parametrizados pelo contador.
    /// O motor é configurável; os defaults são 0 (nenhum valor fiscal presumido) e a base de CIF default é
    /// hora-máquina. Preencher via <see cref="ParametrosCusteio"/> após ratificação do contador.
    /// </summary>
    public sealed class CustoRollupService
    {
        public sealed class ParametrosCusteio
        {
            /// <summary>DP-CST-005 — taxa de MOD por hora-homem. valida-contador. Default 0.</summary>
            public decimal TaxaMaoDeObraPorHora { get; init; } = 0m;

            /// <summary>DP-CST-006 — taxa de CIF por hora-máquina (base default). valida-contador. Default 0.</summary>
            public decimal TaxaCifPorHoraMaquina { get; init; } = 0m;

            /// <summary>Valoração unitária de material de folha (custo médio móvel do Estoque D4). Default 0.</summary>
            public Func<Guid, decimal> CustoUnitarioMaterial { get; init; } = _ => 0m;

            /// <summary>Horas (MOD, máquina) por estrutura para MOD/CIF previstos (roteiro). Default (0,0).</summary>
            public Func<Guid, (decimal horasMod, decimal horasMaquina)> HorasPorEstrutura { get; init; } = _ => (0m, 0m);
        }

        public sealed class CustoEstrutura
        {
            public decimal CustoMaterial { get; init; }
            public decimal CustoMaoDeObra { get; init; }
            public decimal CustoIndireto { get; init; }
            public decimal CustoExtra { get; init; }
            public decimal Rendimento { get; init; }
            public bool PossuiCiclo { get; init; }
            public decimal CustoTotal => CustoMaterial + CustoMaoDeObra + CustoIndireto + CustoExtra;
            public decimal CustoUnitario => Rendimento > 0m ? CustoTotal / Rendimento : CustoTotal;
        }

        private readonly BomExplosaoService _explosao = new();

        /// <summary>
        /// DP-CST-012 — custo previsto por rollup multinível para 1 lote (rendimento = QuantidadeTotal) da estrutura raiz.
        /// Protege contra ciclo (BOM-REG-019) reaproveitando o guard da explosão.
        /// </summary>
        public CustoEstrutura CalcularPrevisto(
            BomEstrutura raiz,
            IReadOnlyCollection<BomEstrutura> catalogo,
            ParametrosCusteio parametros,
            DateTime dataReferencia)
        {
            if (raiz == null) throw new ArgumentNullException(nameof(raiz));
            parametros ??= new ParametrosCusteio();

            if (_explosao.PossuiCiclo(raiz, catalogo, dataReferencia))
                return new CustoEstrutura { PossuiCiclo = true, Rendimento = raiz.QuantidadeTotal };

            return Calcular(raiz, catalogo, parametros, dataReferencia, new HashSet<Guid>());
        }

        private CustoEstrutura Calcular(
            BomEstrutura estrutura,
            IReadOnlyCollection<BomEstrutura> catalogo,
            ParametrosCusteio p,
            DateTime dataReferencia,
            HashSet<Guid> caminho)
        {
            caminho.Add(estrutura.ProdutoId);
            var rendimento = estrutura.QuantidadeTotal > 0m ? estrutura.QuantidadeTotal : 1m;

            decimal material = 0m;
            foreach (var comp in estrutura.Componentes)
            {
                var qtd = comp.QuantidadeFinal ?? comp.Quantidade; // para o lote da estrutura
                var filha = BuscarSubmontagem(catalogo, comp.VariacaoComponenteId, dataReferencia);

                if (comp.EhFantasma && filha != null && !caminho.Contains(filha.ProdutoId))
                {
                    // fantasma: soma o custo dos componentes da submontagem (proporcional à quantidade)
                    var custoFantasma = Calcular(filha, catalogo, p, dataReferencia, caminho);
                    material += custoFantasma.CustoUnitario * qtd;
                }
                else if (filha != null && !caminho.Contains(filha.ProdutoId))
                {
                    var custoFilha = Calcular(filha, catalogo, p, dataReferencia, caminho);
                    material += custoFilha.CustoUnitario * qtd; // custo rolado da submontagem
                }
                else
                {
                    // folha: valoração do Estoque; se a linha já tem custo unitário próprio, usa-o
                    var unit = comp.CustoUnitarioComImpostos ?? p.CustoUnitarioMaterial(comp.VariacaoComponenteId);
                    material += qtd * unit;
                }
            }

            var (horasMod, horasMaq) = p.HorasPorEstrutura(estrutura.Id);
            var mod = horasMod * p.TaxaMaoDeObraPorHora;   // valida-contador
            var cif = horasMaq * p.TaxaCifPorHoraMaquina;  // valida-contador

            caminho.Remove(estrutura.ProdutoId);

            return new CustoEstrutura
            {
                CustoMaterial = material,
                CustoMaoDeObra = mod,
                CustoIndireto = cif,
                CustoExtra = estrutura.CustoExtra,
                Rendimento = rendimento
            };
        }

        private static BomEstrutura? BuscarSubmontagem(
            IReadOnlyCollection<BomEstrutura> catalogo, Guid componenteId, DateTime dataReferencia)
        {
            return catalogo
                .Where(e => e.Status == EStatusWorkflowProducao.Ativo)
                .Where(e => e.ProdutoId == componenteId || e.VariacaoId == componenteId)
                .Where(e => (!e.InicioVigencia.HasValue || e.InicioVigencia.Value.Date <= dataReferencia.Date)
                         && (!e.FimVigencia.HasValue || e.FimVigencia.Value.Date >= dataReferencia.Date))
                .OrderBy(e => e.FimVigencia.HasValue ? 0 : 1)
                .FirstOrDefault();
        }

        /// <summary>
        /// PD8/PD12 — desvio por linha (⚠️ valida-contador no tratamento fiscal do desvio): realizado − previsto.
        /// Mantém a convenção já usada em CustoProducao/CustoReferencia.
        /// </summary>
        public static decimal CalcularDesvio(decimal previsto, decimal realizado) => realizado - previsto;
    }
}
