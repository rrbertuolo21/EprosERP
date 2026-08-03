using Epros.ERP.DfeCalculos.Impostos.Abstracts;
using Epros.ERP.DfeCalculos.Models.Vendas;
using NFe.Classes.Informacoes.Detalhe.Tributacao.Estadual;

namespace Epros.ERP.DfeCalculos.Impostos.Ipis
{
    public class CalculaIpis
    {
        public static VendaItemIpi ObterIpi(VendaItem vendaItem)
        {
            var ipi = new VendaItemIpi();

            if (string.IsNullOrEmpty(vendaItem.CstIpi)) return null!;

            CalculaIpi? calculaIpi = null;

            switch (vendaItem.CstIpi)
            {
                case "00": calculaIpi = new CalculaIpi00(); break;
                case "01": calculaIpi = new CalculaIpi01(); break;
                case "02": calculaIpi = new CalculaIpi02(); break;
                case "03": calculaIpi = new CalculaIpi03(); break;
                case "04": calculaIpi = new CalculaIpi04(); break;
                case "05": calculaIpi = new CalculaIpi05(); break;
                case "49": calculaIpi = new CalculaIpi49(); break;
                case "50": calculaIpi = new CalculaIpi50(); break;
                case "51": calculaIpi = new CalculaIpi51(); break;
                case "52": calculaIpi = new CalculaIpi52(); break;
                case "53": calculaIpi = new CalculaIpi53(); break;
                case "54": calculaIpi = new CalculaIpi54(); break;
                case "55": calculaIpi = new CalculaIpi55(); break;
                case "99": calculaIpi = new CalculaIpi99(); break;
                default:
                    calculaIpi = null;
                    {
                        vendaItem.AddNotification("CstIpi", $"CstIpi: {vendaItem.CstIpi} do produto: {vendaItem.CodigoProduto} inválido");
                        return ipi;
                    }
            }
            ipi = calculaIpi!.ObterIpi(vendaItem);

            return ipi;
        }
    }
}
