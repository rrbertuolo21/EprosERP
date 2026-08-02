using System;
using System.Collections.Generic;
using System.Linq;
using Epros.Modules.Estoque.Domain.Enums;

namespace Epros.Modules.Estoque.Domain.Services
{
    /// <summary>
    /// Base de rateio de um item para o custo landed: valor, peso e quantidade já apurados (factuais).
    /// </summary>
    public sealed record RateioLandedItem(Guid ItemId, decimal Valor, decimal Peso, decimal Quantidade);

    /// <summary>Resultado do rateio por item: valor base + parcela landed apropriada = custo final.</summary>
    public sealed record RateioLandedResultado(Guid ItemId, decimal ValorBase, decimal ParcelaLanded, decimal CustoFinal);

    /// <summary>
    /// Calculadora ESTRUTURAL do custo landed da importação (CD1 / EF COMERCIO_EXTERIOR §6.2, NF-02).
    /// Distribui um montante JÁ APURADO (tributos + frete + despesas selecionados pela config) sobre os
    /// itens, segundo a chave escolhida (PorValor/PorPeso/PorQuantidade). É apenas apropriação matemática:
    /// não calcula tributo, base fiscal nem alíquota (isso é valida-contador). A última parcela absorve o
    /// resíduo de arredondamento para o total ratear exatamente o montante.
    /// </summary>
    public static class RateioLandedCalculadora
    {
        public static IReadOnlyList<RateioLandedResultado> Ratear(
            IReadOnlyList<RateioLandedItem> itens,
            decimal montanteLanded,
            ERateioLandedMetodo metodo)
        {
            if (itens == null || itens.Count == 0)
                return Array.Empty<RateioLandedResultado>();

            decimal Chave(RateioLandedItem i) => metodo switch
            {
                ERateioLandedMetodo.PorPeso => i.Peso,
                ERateioLandedMetodo.PorQuantidade => i.Quantidade,
                _ => i.Valor
            };

            var totalChave = itens.Sum(Chave);
            var resultado = new List<RateioLandedResultado>(itens.Count);

            // Chave total zero (ex.: sem peso informado) → distribui igualmente para não perder o montante.
            var usarIgual = totalChave <= 0;
            decimal acumulado = 0m;

            for (int idx = 0; idx < itens.Count; idx++)
            {
                var i = itens[idx];
                decimal parcela;
                if (idx == itens.Count - 1)
                {
                    // Última parcela = resíduo, para o rateio somar exatamente o montante.
                    parcela = Math.Round(montanteLanded - acumulado, 2, MidpointRounding.AwayFromZero);
                }
                else
                {
                    var proporcao = usarIgual ? 1m / itens.Count : Chave(i) / totalChave;
                    parcela = Math.Round(montanteLanded * proporcao, 2, MidpointRounding.AwayFromZero);
                    acumulado += parcela;
                }
                resultado.Add(new RateioLandedResultado(i.ItemId, i.Valor, parcela, Math.Round(i.Valor + parcela, 2, MidpointRounding.AwayFromZero)));
            }

            return resultado;
        }
    }
}
