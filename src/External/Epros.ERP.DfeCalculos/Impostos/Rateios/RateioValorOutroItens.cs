using Epros.ERP.DfeCalculos.Models.Vendas;
using Epros.ERP.Shared.Extensions;

namespace Epros.ERP.DfeCalculos.Impostos.Rateios
{
    public class RateioValorOutroItens
    {
        public static ICollection<VendaItem> Ratear(decimal valorTotalParaRateio, ICollection<VendaItem> itens)
        {
            var itensTributaveis = itens.Where(item =>
                RateioItens.CstCsosnTributaveis.Contains(item.CstIcms) ||
                RateioItens.CstCsosnTributaveis.Contains(item.Csosn)).ToList();

            if (itensTributaveis.Count() == 0)
                itensTributaveis = itens.ToList();

            if (valorTotalParaRateio <= decimal.Zero)
            {
                itensTributaveis.ToList().ForEach(item => item.DefinirValorOutro(decimal.Zero));
                return itensTributaveis;
            }

            decimal valorVendaSubTotal = RateioItens.CalcularValorVendaSubTotal(itensTributaveis);

            foreach (var item in itensTributaveis)
            {
                decimal subTotalItem = RateioItens.CalcularValorItemSubTotal(item);
                var porcentagemProporcional = RateioItens.CalcularPercentual(valorVendaSubTotal, subTotalItem);
                var valorProporcional = RateioItens.CalcularValorASerRatiado(porcentagemProporcional, valorTotalParaRateio);
                item.DefinirValorOutro(valorProporcional.Arredonda2());
            }

            var somaDoRateioNosItens = itensTributaveis.ToList().Sum(x => x.ValorOutroRateado);

            if (somaDoRateioNosItens == valorTotalParaRateio) return itens;

            var valorASerCorrigido = RateioItens.CalcularCorrecaoRateiosNosItens(valorTotalParaRateio, somaDoRateioNosItens);

            var ultimoItem = itensTributaveis.Last();

            ultimoItem.DefinirValorOutro(ultimoItem.ValorOutroRateado + valorASerCorrigido);

            return itens;
        }
    }
}
