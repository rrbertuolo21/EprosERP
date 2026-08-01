using Epros.ERP.DfeCalculos.Impostos.Abstracts;
using Epros.ERP.DfeCalculos.Impostos.IbsCbss;
using Epros.ERP.DfeCalculos.Models.Vendas;

namespace Epros.ERP.DfeCalculos.Impostos.Ipis
{
    public class CalculaCst410 : CalculaIbsCbsCst
    {
        public override VendaItemIbsCbs ObterIbsCbs(VendaItem vendaItem)
        {
            VendaItemIbsCbs ibsCbs = new VendaItemIbsCbs();
            ibsCbs.Cst = vendaItem.CstIbsCbs;
            ibsCbs.CClassTrib = vendaItem.CClassTrib;

            switch (vendaItem.CClassTrib!.Substring(3))
            {
                case "001":
                case "002":
                case "003":
                case "004":
                case "005":
                case "006":
                case "007":
                case "008":
                case "009":
                case "011":
                case "012":
                case "013":
                case "014":
                case "016":
                case "017":
                case "023":
                case "024":
                case "025":
                case "999":
                    // Aliquotas
                    ibsCbs.AliquotaCbs = decimal.Zero;

                    // CBS
                    ibsCbs.ValorBaseDeCalculo = decimal.Zero; // vendaItem.ValorItem;

                    ibsCbs.ValorImpostoDevidoCbs = decimal.Zero;
                    // IBS Estadual
                    ibsCbs.AliquotaEstadual = decimal.Zero;
                    ibsCbs.ValorImpostoDevidoEstadual = decimal.Zero;
                    // IBS Municipal
                    ibsCbs.AliquotaMunicipal = decimal.Zero;
                    ibsCbs.ValorImpostoDevidoMunicipal = decimal.Zero;
                    return ibsCbs;
                default:
                    new Exception("CST IBS CBS inválido");
                    break;
            }

            return ibsCbs;
        }
    }
}
