using Epros.ERP.DfeCalculos.Impostos.Abstracts;
using Epros.ERP.DfeCalculos.Impostos.IbsCbss;
using Epros.ERP.DfeCalculos.Models.Vendas;
using NFe.Classes.Informacoes.Detalhe.Tributacao;
using NFe.Classes.Informacoes.Emitente;

namespace Epros.ERP.DfeCalculos.Impostos.Icms
{
    public class CalculaIcms
    {
        public static VendaItemIcms ObterIcms(VendaItem vendaItem)
        {
            var icms = new VendaItemIcms();
            if (vendaItem.RegimeTributario == CRT.SimplesNacional || vendaItem.RegimeTributario == CRT.SimplesNacionalMei)
            {
                //if (!CsosnECFValidation.CsosnValidar(vendaItem.Produto.Csosn.Id))
                //    throw new Exception("CSOSN: " + vendaItem.Produto.Csosn + " inválido!");

                icms.Csosn = vendaItem.Csosn;
                icms.Origem = vendaItem.Origem;
                icms.Aliquota = vendaItem.ValorAliquotaIcms;
                icms.Cst = "";
            }
            else
            {
                CalculaIcmsCst? calculaIcmsCst = null;

                //if (!CstIcmsNfceValidation.CstIcmsValidar(vendaItem.Produto.CstIcms))
                //    throw new Exception("CST ICMS: " + vendaItem.Produto.CstIcms + " inválido!");

                icms.Csosn = "";
                switch (vendaItem.CstIcms)
                {
                    case "00": calculaIcmsCst = new CalculaIcmsCst00(); break;
                    case "10": calculaIcmsCst = new CalculaIcmsCst10(); break;
                    case "20": calculaIcmsCst = new CalculaIcmsCst20(); break;
                    case "30": calculaIcmsCst = new CalculaIcmsCst30(); break;
                    case "40": calculaIcmsCst = new CalculaIcmsCst40(); break;
                    case "41": calculaIcmsCst = new CalculaIcmsCst41(); break;
                    case "50": calculaIcmsCst = new CalculaIcmsCst50(); break;
                    case "51": calculaIcmsCst = new CalculaIcmsCst51(); break;
                    case "53": calculaIcmsCst = new CalculaIcmsCst53(); break;
                    case "60": calculaIcmsCst = new CalculaIcmsCst60(); break;
                    case "61": calculaIcmsCst = new CalculaIcmsCst61(); break;
                    case "70": calculaIcmsCst = new CalculaIcmsCst70(); break;
                    case "90": calculaIcmsCst = new CalculaIcmsCst90(); break;
                    default:
                        calculaIcmsCst = null;
                        {
                            vendaItem.AddNotification("CstIcms", $"CstIcms: {vendaItem.CstIcms} do produto: {vendaItem.CodigoProduto} inválido");
                            return icms;
                        }
                }
                icms = calculaIcmsCst!.ObterIcms(vendaItem);
            }
            return icms;
        }
    }
}
