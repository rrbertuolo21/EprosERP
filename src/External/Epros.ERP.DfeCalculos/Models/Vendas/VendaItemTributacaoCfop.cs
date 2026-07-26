namespace Epros.ERP.DfeCalculos.Models.Vendas
{
    public class VendaItemTributacaoCfop
    {
        public VendaItemTributacaoCfop() { }
        public VendaItemTributacaoCfop(int cfop, int cfopCorrelacao, bool integraFaturamento, bool combustivel)
        {
            Cfop = cfop;
            CfopCorrelacao = cfopCorrelacao;
            IntegraFaturamento = integraFaturamento;
            Combustivel = combustivel;
        }

        public int Cfop { get; set; }
        public int CfopCorrelacao { get; set; }
        public bool IntegraFaturamento { get; set; }
        public bool Combustivel { get; set; }
    }
}