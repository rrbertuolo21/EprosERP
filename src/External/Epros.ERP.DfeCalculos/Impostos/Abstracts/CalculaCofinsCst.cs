using Epros.ERP.DfeCalculos.Impostos.Cofins;
using Epros.ERP.DfeCalculos.Models.Vendas;

namespace Epros.ERP.DfeCalculos.Impostos.Abstracts
{
    public abstract class CalculaCofinsCst
    {
        protected readonly string[] CNAES = ["4511101", "4511102", "4512902", "4530703", "4541204"];
        public abstract VendaItemCofins ObterCofins(VendaItem vendaItem);
    }
}
