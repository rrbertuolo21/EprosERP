using Epros.ERP.DfeCalculos.Impostos.Ipis;
using Epros.ERP.DfeCalculos.Models.Vendas;

namespace Epros.ERP.DfeCalculos.Impostos.Abstracts
{
    public abstract class CalculaIpi
    {
        public abstract VendaItemIpi ObterIpi(VendaItem vendaItem);
    }
}
