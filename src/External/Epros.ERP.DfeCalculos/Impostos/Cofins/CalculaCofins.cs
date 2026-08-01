using Epros.ERP.DfeCalculos.Impostos.Abstracts;
using Epros.ERP.DfeCalculos.Models.Vendas;
using NFe.Classes.Informacoes.Detalhe.Tributacao.Estadual;
using NFe.Classes.Informacoes.Emitente;

namespace Epros.ERP.DfeCalculos.Impostos.Cofins
{
    public class CalculaCofins
    {
        public static VendaItemCofins ObterCofins(VendaItem vendaItem)
        {
            var cofins = new VendaItemCofins();
            CalculaCofinsCst? calculaCofinsCst = null;

            if (vendaItem.RegimeTributario == CRT.SimplesNacional || vendaItem.RegimeTributario == CRT.SimplesNacionalMei)
                cofins.Cst = "49";
            else
            {
                //if (!CstPisCofinsValidation.CstPisCofinsValidar(vendaItem.CstPisCofins))
                //    throw new Exception("CST COFINS: " + vendaItem.Produto.CstIcms + " inválido!");

                switch (vendaItem.CstPisCofins)
                {
                    case "01": calculaCofinsCst = new CalculaCofinsCst01(); break;
                    case "02": calculaCofinsCst = new CalculaCofinsCst02(); break;
                    case "03": calculaCofinsCst = new CalculaCofinsCst03(); break;
                    case "04": calculaCofinsCst = new CalculaCofinsCst04(); break;
                    case "05": calculaCofinsCst = new CalculaCofinsCst05(); break;
                    case "06": calculaCofinsCst = new CalculaCofinsCst06(); break;
                    case "07": calculaCofinsCst = new CalculaCofinsCst07(); break;
                    case "08": calculaCofinsCst = new CalculaCofinsCst08(); break;
                    case "09": calculaCofinsCst = new CalculaCofinsCst09(); break;
                    case "49": calculaCofinsCst = new CalculaCofinsCst49(); break;
                    case "50": calculaCofinsCst = new CalculaCofinsCst50(); break;
                    case "51": calculaCofinsCst = new CalculaCofinsCst50(); break;
                    case "52": calculaCofinsCst = new CalculaCofinsCst50(); break;
                    case "53": calculaCofinsCst = new CalculaCofinsCst50(); break;
                    case "54": calculaCofinsCst = new CalculaCofinsCst50(); break;
                    case "55": calculaCofinsCst = new CalculaCofinsCst50(); break;
                    case "56": calculaCofinsCst = new CalculaCofinsCst50(); break;
                    case "57": calculaCofinsCst = new CalculaCofinsCst50(); break;
                    case "58": calculaCofinsCst = new CalculaCofinsCst50(); break;
                    case "59": calculaCofinsCst = new CalculaCofinsCst50(); break;
                    case "60": calculaCofinsCst = new CalculaCofinsCst50(); break;
                    case "61": calculaCofinsCst = new CalculaCofinsCst50(); break;
                    case "62": calculaCofinsCst = new CalculaCofinsCst50(); break;
                    case "63": calculaCofinsCst = new CalculaCofinsCst50(); break;
                    case "64": calculaCofinsCst = new CalculaCofinsCst50(); break;
                    case "65": calculaCofinsCst = new CalculaCofinsCst50(); break;
                    case "66": calculaCofinsCst = new CalculaCofinsCst50(); break;
                    case "67": calculaCofinsCst = new CalculaCofinsCst50(); break;
                    case "70": calculaCofinsCst = new CalculaCofinsCst07(); break;
                    case "71": calculaCofinsCst = new CalculaCofinsCst07(); break;
                    case "72": calculaCofinsCst = new CalculaCofinsCst07(); break;
                    case "73": calculaCofinsCst = new CalculaCofinsCst07(); break;
                    case "74": calculaCofinsCst = new CalculaCofinsCst07(); break;
                    case "75": calculaCofinsCst = new CalculaCofinsCst07(); break;
                    case "98": calculaCofinsCst = new CalculaCofinsCst99(); break;
                    case "99": calculaCofinsCst = new CalculaCofinsCst99(); break;
                    default:
                        calculaCofinsCst = null;
                        {
                            vendaItem.AddNotification("CstPisCofins", $"CstPisCofins: {vendaItem.CstPisCofins} do produto: {vendaItem.CodigoProduto} inválido");
                            return cofins;
                        }
                }
                cofins = calculaCofinsCst!.ObterCofins(vendaItem);
            }
            return cofins;
        }
    }
}
