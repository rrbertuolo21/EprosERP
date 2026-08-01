using Epros.ERP.DfeCalculos.Impostos.Abstracts;
using Epros.ERP.DfeCalculos.Models.Vendas;
using NFe.Classes.Informacoes.Emitente;

namespace Epros.ERP.DfeCalculos.Impostos.Icms.Csosns
{
    public class CalculaCsosn
    {
        public static VendaItemIcms ObterIcmsCsosn(Venda venda, VendaItem vendaItem)
        {
            var icms = new VendaItemIcms();
            if (vendaItem.RegimeTributario == CRT.SimplesNacional || vendaItem.RegimeTributario == CRT.SimplesNacionalMei)
            {
                CalculaIcmsCsosn? calculaIcmsCst = null;

                //if (!CstIcmsNfceValidation.CstIcmsValidar(vendaItem.Produto.CstIcms))
                //    throw new Exception("CST ICMS: " + vendaItem.Produto.CstIcms + " inválido!");

                icms.Csosn = "";
                switch (vendaItem.Csosn)
                {
                    case "101": calculaIcmsCst = new CalculaCsosn101(); break;
                    case "102": calculaIcmsCst = new CalculaCsosn102(); break;
                    case "103": calculaIcmsCst = new CalculaCsosn103(); break;
                    case "201": calculaIcmsCst = new CalculaCsosn201(); break;
                    case "202": calculaIcmsCst = new CalculaCsosn202(); break;
                    case "203": calculaIcmsCst = new CalculaCsosn203(); break;
                    case "300": calculaIcmsCst = new CalculaCsosn300(); break;
                    case "400": calculaIcmsCst = new CalculaCsosn400(); break;
                    case "500": calculaIcmsCst = new CalculaCsosn500(); break;
                    case "900": calculaIcmsCst = new CalculaCsosn900(); break;
                    default:
                        calculaIcmsCst = null;
                        {
                            vendaItem.AddNotification("Csosn", $"Csosn: {vendaItem.Csosn} do produto: {vendaItem.CodigoProduto} inválido");
                            return icms;
                        }
                }
                icms = calculaIcmsCst!.ObterIcmsCsosn(venda, vendaItem);
            }
            return icms;
        }
    }
}
