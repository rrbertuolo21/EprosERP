using Epros.ERP.DfeCalculos.Impostos.Abstracts;
using Epros.ERP.DfeCalculos.Models.Vendas;

namespace Epros.ERP.DfeCalculos.Impostos.Icms
{
    public class CalculaIcmsCst40 : CalculaIcmsCst
    {
        public override VendaItemIcms ObterIcms(VendaItem vendaItem)
        {
            return new VendaItemIcms { Cst = vendaItem.CstIcms, Origem = vendaItem.Origem, ValorIcmsIsento = vendaItem.ValorItem };
        }
    }
}
