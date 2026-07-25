using Epros.ERP.DfeCalculos.Models.Vendas;

namespace Epros.ERP.DfeCalculos.Impostos.Rateios
{
    public static class RateioItens
    {
        public static string[] CstCsosnTributaveis = ["00", "10", "20", "70", "90", "900"];
        public static decimal CalcularCorrecaoRateiosNosItens(decimal valorParaRateioTotal, decimal somaDoRateioNosItens)
        {
            if (somaDoRateioNosItens == valorParaRateioTotal) return valorParaRateioTotal;
            return valorParaRateioTotal - somaDoRateioNosItens;
        }

        public static decimal CalcularValorVendaSubTotal(IEnumerable<VendaItem> itens) => itens.Sum(x => x.Quantidade * x.ValorUnitario - x.ValorDesconto);

        public static decimal CalcularValorItemSubTotal(VendaItem item) => item.Quantidade * item.ValorUnitario - item.ValorDesconto;

        public static decimal CalcularPercentual(decimal valorVendaSubTotal, decimal valorItemSubTotal) => valorItemSubTotal * 100 / valorVendaSubTotal / 100;

        public static decimal CalcularValorASerRatiado(decimal percentualRateio, decimal ValorParaRateio) => ValorParaRateio * percentualRateio;
    }
}
