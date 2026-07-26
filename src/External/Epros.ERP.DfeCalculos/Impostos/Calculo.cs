using Epros.ERP.DfeCalculos.Impostos.Cofins;
using Epros.ERP.DfeCalculos.Impostos.IbsCbss;
using Epros.ERP.DfeCalculos.Impostos.Icms;
using Epros.ERP.DfeCalculos.Impostos.Icms.Csosns;
using Epros.ERP.DfeCalculos.Impostos.Ipis;
using Epros.ERP.DfeCalculos.Impostos.Pis;
using Epros.ERP.DfeCalculos.Models.Vendas;
using NFe.Classes.Informacoes.Emitente;

namespace Epros.ERP.DfeCalculos.Impostos
{
    public class Calculo
    {
        public static VendaItemIcms CalcularICMS(Venda venda, VendaItem vendaItem)
        {
            VendaItemIcms? icms = null;

            if (vendaItem.RegimeTributario == CRT.SimplesNacional || vendaItem.RegimeTributario == CRT.SimplesNacionalMei)
            {
                icms = CalculaCsosn.ObterIcmsCsosn(venda, vendaItem);
            }
            else if (vendaItem.RegimeTributario == CRT.SimplesNacionalExcessoSublimite || vendaItem.RegimeTributario == CRT.RegimeNormal)
            {
                icms = CalculaIcms.ObterIcms(vendaItem);
            }

            return icms!;
        }

        public static VendaItemPis CalcularPIS(VendaItem item)
        {
            var pis = CalculaPis.ObterPis(item);
            return pis;
        }

        public static VendaItemCofins CalcularCOFINS(VendaItem item)
        {
            var cofins = CalculaCofins.ObterCofins(item);
            return cofins;
        }

        public static VendaItemIpi CalcularIPI(VendaItem item)
        {
            VendaItemIpi? ipi = null;
            if ((!string.IsNullOrEmpty(item.CstIpi)) && item.RegimeTributario != CRT.SimplesNacional && item.RegimeTributario != CRT.SimplesNacionalMei)
            {
                ipi = CalculaIpis.ObterIpi(item);
            }
            return ipi!;
        }

        public static VendaItemIbsCbs CalcularIBSCBS(VendaItem item)
        {
            VendaItemIbsCbs? ibsCbs = null;
            //if (!string.IsNullOrEmpty(item.CstIbsCbs) && item.RegimeTributario != CRT.SimplesNacional && item.RegimeTributario != CRT.SimplesNacionalMei)
            //{
            ibsCbs = CalculaIbsCbs.ObterIbsCbs(item);
            //}
            return ibsCbs;
        }
    }
}


