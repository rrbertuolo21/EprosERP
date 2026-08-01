using Epros.ERP.DfeCalculos.Impostos.Abstracts;
using Epros.ERP.DfeCalculos.Models.Vendas;

namespace Epros.ERP.DfeCalculos.Impostos.Cofins
{
    public class CalculaCofinsCst06 : CalculaCofinsCst
    {
        public override VendaItemCofins ObterCofins(VendaItem vendaItem)
        {
            return new VendaItemCofins { Cst = vendaItem.CstPisCofins };
        }
    }
}
