using Epros.ERP.DfeCalculos.Impostos.Abstracts;
using Epros.ERP.DfeCalculos.Impostos.Cofins;
using Epros.ERP.DfeCalculos.Models.Vendas;
using NFe.Classes.Informacoes.Detalhe.Tributacao.Estadual;
using NFe.Classes.Informacoes.Emitente;

namespace Epros.ERP.DfeCalculos.Impostos.Pis
{
    public class CalculaPis
    {
        public static VendaItemPis ObterPis(VendaItem vendaItem)
        {
            var pis = new VendaItemPis();
            CalculaPisCst? calculaPisCst = null;

            if (vendaItem.RegimeTributario == CRT.SimplesNacional || vendaItem.RegimeTributario == CRT.SimplesNacionalMei)
                pis.Cst = "49";
            else
            {
                //    if (!CstPisCofinsValidation.CstPisCofinsValidar(vendaItem.CstPisCofins))
                //        throw new Exception("CST PIS: " + vendaItem.CstPisCofins + " inválido!");
                switch (vendaItem.CstPisCofins)
                {
                    case "01": calculaPisCst = new CalculaPisCst01(); break;
                    case "02": calculaPisCst = new CalculaPisCst02(); break;
                    case "03": calculaPisCst = new CalculaPisCst03(); break;
                    case "04": calculaPisCst = new CalculaPisCst04(); break;
                    case "05": calculaPisCst = new CalculaPisCst05(); break;
                    case "06": calculaPisCst = new CalculaPisCst06(); break;
                    case "07": calculaPisCst = new CalculaPisCst07(); break;
                    case "08": calculaPisCst = new CalculaPisCst08(); break;
                    case "09": calculaPisCst = new CalculaPisCst09(); break;
                    case "49": calculaPisCst = new CalculaPisCst49(); break;
                    case "50": calculaPisCst = new CalculaPisCst50(); break;
                    case "51": calculaPisCst = new CalculaPisCst50(); break;
                    case "52": calculaPisCst = new CalculaPisCst50(); break;
                    case "53": calculaPisCst = new CalculaPisCst50(); break;
                    case "54": calculaPisCst = new CalculaPisCst50(); break;
                    case "55": calculaPisCst = new CalculaPisCst50(); break;
                    case "56": calculaPisCst = new CalculaPisCst50(); break;
                    case "57": calculaPisCst = new CalculaPisCst50(); break;
                    case "58": calculaPisCst = new CalculaPisCst50(); break;
                    case "59": calculaPisCst = new CalculaPisCst50(); break;
                    case "60": calculaPisCst = new CalculaPisCst50(); break;
                    case "61": calculaPisCst = new CalculaPisCst50(); break;
                    case "62": calculaPisCst = new CalculaPisCst50(); break;
                    case "63": calculaPisCst = new CalculaPisCst50(); break;
                    case "64": calculaPisCst = new CalculaPisCst50(); break;
                    case "65": calculaPisCst = new CalculaPisCst50(); break;
                    case "66": calculaPisCst = new CalculaPisCst50(); break;
                    case "67": calculaPisCst = new CalculaPisCst50(); break;
                    case "70": calculaPisCst = new CalculaPisCst07(); break;
                    case "71": calculaPisCst = new CalculaPisCst07(); break;
                    case "72": calculaPisCst = new CalculaPisCst07(); break;
                    case "73": calculaPisCst = new CalculaPisCst07(); break;
                    case "74": calculaPisCst = new CalculaPisCst07(); break;
                    case "75": calculaPisCst = new CalculaPisCst07(); break;
                    case "98": calculaPisCst = new CalculaPisCst99(); break;
                    case "99": calculaPisCst = new CalculaPisCst99(); break;
                    default:
                        calculaPisCst = null;
                        {
                            vendaItem.AddNotification("CstPisCofins", $"CstPisCofins: {vendaItem.CstPisCofins} do produto: {vendaItem.CodigoProduto} inválido");
                            return pis;
                        }
                }
                pis = calculaPisCst!.ObterPis(vendaItem);
            }
            return pis;
        }
    }
}
