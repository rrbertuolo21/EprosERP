using Epros.ERP.DfeCalculos.Impostos.Abstracts;
using Epros.ERP.DfeCalculos.Models.Vendas;

namespace Epros.ERP.DfeCalculos.Impostos.Icms.Csosns
{
    public class CalculaCsosn101 : CalculaIcmsCsosn
    {
        public override VendaItemIcms ObterIcmsCsosn(Venda venda, VendaItem vendaItem)
        {
            var icms = new VendaItemIcms();
            icms.Origem = vendaItem.Origem;
            icms.Csosn = vendaItem.Csosn;
            icms.Aliquota = (venda.Imposto != null ? Math.Round(venda.Imposto.ValorAliquotaCreditoIcms, 2) : decimal.Zero);
            icms.ValorBaseDeCalculo = vendaItem.ValorUnitario;
            icms.ValorCredito = (venda.Imposto != null ? Math.Round(((vendaItem.ValorItem * venda.Imposto.ValorAliquotaCreditoIcms) / 100), 2) : decimal.Zero);
            return icms;
        }
    }
}
