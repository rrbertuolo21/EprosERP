using Epros.ERP.DfeCalculos.Impostos.Abstracts;
using Epros.ERP.DfeCalculos.Models.Vendas;

namespace Epros.ERP.DfeCalculos.Impostos.Icms.Csosns
{
    public class CalculaCsosn102 : CalculaIcmsCsosn
    {
        public override VendaItemIcms ObterIcmsCsosn(Venda venda, VendaItem vendaItem)
        {
            var icms = new VendaItemIcms();
            icms.Origem = vendaItem.Origem;
            icms.Csosn = vendaItem.Csosn;
            return icms;
        }
    }
}
