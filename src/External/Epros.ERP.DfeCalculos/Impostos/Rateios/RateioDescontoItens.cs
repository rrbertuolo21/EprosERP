using Epros.ERP.DfeCalculos.Models.Vendas;
using Epros.ERP.Shared.Extensions;

namespace Epros.ERP.DfeCalculos.Impostos.Rateios
{
    public class RateioDescontoItens
    {
        public static ICollection<VendaItem> Ratear(decimal valorTotalParaRateio, ICollection<VendaItem> itens)
        {
            if (valorTotalParaRateio <= decimal.Zero)
            {
                itens.ToList().ForEach(item => item.DefinirValorDescontoRatiado(decimal.Zero));
                return itens;
            }

            decimal valorVendaSubTotal = RateioItens.CalcularValorVendaSubTotal(itens);

            foreach (var item in itens)
            {
                decimal subTotalItem = RateioItens.CalcularValorItemSubTotal(item);
                var porcentagemProporcional = RateioItens.CalcularPercentual(valorVendaSubTotal, subTotalItem);
                var valorProporcional = RateioItens.CalcularValorASerRatiado(porcentagemProporcional, valorTotalParaRateio);
                item.DefinirValorDescontoRatiado(valorProporcional.Arredonda2());
            }

            var somaDoRateioNosItens = itens.ToList().Sum(x => x.ValorDescontoRateado);

            if (somaDoRateioNosItens == valorTotalParaRateio) return itens;

            var valorASerCorrigido = RateioItens.CalcularCorrecaoRateiosNosItens(valorTotalParaRateio, somaDoRateioNosItens);

            var ultimoItem = itens.Last();

            ultimoItem.DefinirValorDescontoRatiado(ultimoItem.ValorDescontoRateado + valorASerCorrigido);

            return itens;
        }
    }
}
