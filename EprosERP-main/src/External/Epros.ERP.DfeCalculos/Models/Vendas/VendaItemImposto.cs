using Epros.ERP.DfeCalculos.Impostos.Cofins;
using Epros.ERP.DfeCalculos.Impostos.IbsCbss;
using Epros.ERP.DfeCalculos.Impostos.Icms;
using Epros.ERP.DfeCalculos.Impostos.Ipis;
using Epros.ERP.DfeCalculos.Impostos.Pis;

namespace Epros.ERP.DfeCalculos.Models.Vendas
{
    public class VendaItemImposto
    {
        public VendaItemIcms? Icms { get; set; }
        public VendaItemPis? Pis { get; set; }
        public VendaItemCofins? Cofins { get; set; }
        public VendaItemIpi? Ipi { get; set; }
        public VendaItemIbsCbs? IbsCbs { get; set; }
    }
}
