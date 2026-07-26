namespace Epros.ERP.DfeCalculos.Impostos.Ipis
{
    public class VendaItemIpi
    {
        public VendaItemIpi() { }
        public VendaItemIpi(decimal valorBaseDeCalculo, decimal aliquota, decimal valorImpostoDevido, decimal reducaoPercentual, string? cst, string enquadramento, decimal valorIpiIsento, decimal valorIpiOutros, string? ipiObservacao)
        {
            ValorBaseDeCalculo = valorBaseDeCalculo;
            Aliquota = aliquota;
            ValorImpostoDevido = valorImpostoDevido;
            ReducaoPercentual = reducaoPercentual;
            Cst = cst;
            Enquadramento = enquadramento;
            ValorIpiIsento = valorIpiIsento;
            ValorIpiOutros = valorIpiOutros;
            IpiObservacao = ipiObservacao;
        }

        public decimal ValorBaseDeCalculo { get; set; }
        public decimal Aliquota { get; set; }
        public decimal ValorImpostoDevido { get; set; }
        public decimal ReducaoPercentual { get; set; }
        public string? Cst { get; set; }
        public string Enquadramento { get; set; } = null!;
        public decimal ValorIpiIsento { get; set; }
        public decimal ValorIpiOutros { get; set; }
        public string? IpiObservacao { get; set; }
    }
}
