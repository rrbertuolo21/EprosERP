namespace Epros.ERP.DfeCalculos.Impostos.Pis
{
    public class VendaItemPis
    {
        public VendaItemPis() { }
        public VendaItemPis(string cst, decimal quantidadeVendida, decimal valorBaseDeCalculo, decimal aliquotaPercetual, decimal aliquotaReal, decimal valorImpostoDevido)
        {
            Cst = cst;
            QuantidadeVendida = quantidadeVendida;
            ValorBaseDeCalculo = valorBaseDeCalculo;
            AliquotaPercetual = aliquotaPercetual;
            AliquotaReal = aliquotaReal;
            ValorImpostoDevido = valorImpostoDevido;
        }

        public string Cst { get; set; } = null!;
        public decimal QuantidadeVendida { get; set; }
        public decimal ValorBaseDeCalculo { get; set; }
        public decimal AliquotaPercetual { get; set; }
        public decimal AliquotaReal { get; set; }
        public decimal ValorImpostoDevido { get; set; }
    }
}
