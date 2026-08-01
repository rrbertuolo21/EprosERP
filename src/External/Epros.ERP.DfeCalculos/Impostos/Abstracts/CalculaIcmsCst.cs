using Epros.ERP.DfeCalculos.Impostos.Icms;
using Epros.ERP.DfeCalculos.Models.Vendas;

namespace Epros.ERP.DfeCalculos.Impostos.Abstracts
{
    public abstract class CalculaIcmsCst
    {
        public abstract VendaItemIcms ObterIcms(VendaItem vendaItem);
    }
}
