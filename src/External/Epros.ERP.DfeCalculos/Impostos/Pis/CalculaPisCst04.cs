using Epros.ERP.DfeCalculos.Impostos.Abstracts;
using Epros.ERP.DfeCalculos.Models.Vendas;

namespace Epros.ERP.DfeCalculos.Impostos.Pis
{
    public class CalculaPisCst04 : CalculaPisCst
    {
        public override VendaItemPis ObterPis(VendaItem vendaItem)
        {
            return new VendaItemPis { Cst = vendaItem.CstPisCofins };
        }
    }
}
