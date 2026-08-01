namespace Epros.ERP.DfeCalculos.Impostos.NFses.Iss
{
    public class ServicoItemIss
    {
        public ServicoItemIss() { }

        public ServicoItemIss(decimal aliquota, decimal valorImpostoDevido)
        {
            Aliquota = aliquota;
            ValorImpostoDevido = valorImpostoDevido;
        }

        public decimal Aliquota { get; set; }
        public decimal ValorImpostoDevido { get; set; }
    }
}
