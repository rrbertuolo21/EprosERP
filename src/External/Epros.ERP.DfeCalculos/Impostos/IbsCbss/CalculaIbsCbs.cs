using Epros.ERP.DfeCalculos.Impostos.Abstracts;
using Epros.ERP.DfeCalculos.Impostos.Ipis;
using Epros.ERP.DfeCalculos.Models.Vendas;

namespace Epros.ERP.DfeCalculos.Impostos.IbsCbss
{
    public class CalculaIbsCbs
    {
        public static VendaItemIbsCbs ObterIbsCbs(VendaItem vendaItem)
        {
            var ibsCbs = new VendaItemIbsCbs();

            if (string.IsNullOrEmpty(vendaItem.CstIbsCbs)) return null!;

            CalculaIbsCbsCst? calculaIbsCbs = null;

            switch (vendaItem.CstIbsCbs)
            {
                case "000": calculaIbsCbs = new CalculaCst000(); break;
                case "200": calculaIbsCbs = new CalculaCst200(); break;
                case "410": calculaIbsCbs = new CalculaCst410(); break;
                case "510": calculaIbsCbs = new CalculaCst510(); break;
                default:
                    calculaIbsCbs = null;
                    {
                        vendaItem.AddNotification("CstIbsCbs", $"CstIbsCbs: {vendaItem.CstIbsCbs} do produto: {vendaItem.CodigoProduto} inválido");
                        return ibsCbs;
                    }
            }
            ibsCbs = calculaIbsCbs!.ObterIbsCbs(vendaItem);

            return ibsCbs;
        }
    }
}
