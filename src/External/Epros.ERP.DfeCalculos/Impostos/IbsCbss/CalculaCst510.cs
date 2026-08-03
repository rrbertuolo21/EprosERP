using Epros.ERP.DfeCalculos.Impostos.Abstracts;
using Epros.ERP.DfeCalculos.Impostos.IbsCbss;
using Epros.ERP.DfeCalculos.Models.Vendas;

namespace Epros.ERP.DfeCalculos.Impostos.Ipis
{
    public class CalculaCst510 : CalculaIbsCbsCst
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
                    // Aliquotas
                    ibsCbs.AliquotaCbs = decimal.Zero;
                    ibsCbs.AliquotaCbsDiferimento = aliquotaIbsDiferimento;
                    ibsCbs.AliquotaEstadualDiferimento = aliquotaIbsDiferimento;
                    ibsCbs.AliquotaMunicipalDiferimento = aliquotaIbsDiferimento;

                    // CBS
                    ibsCbs.ValorBaseDeCalculo = vendaItem.ValorItem;

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
