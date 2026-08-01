using Epros.ERP.DfeCalculos.Impostos.Abstracts;
using Epros.ERP.DfeCalculos.Models.Vendas;

namespace Epros.ERP.DfeCalculos.Impostos.Ipis
{
    public class CalculaIpi51 : CalculaIpi
    {
        public override VendaItemIpi ObterIpi(VendaItem vendaItem)
        {
            return new VendaItemIpi { Cst = vendaItem.CstIpi };
        }
    }
}
