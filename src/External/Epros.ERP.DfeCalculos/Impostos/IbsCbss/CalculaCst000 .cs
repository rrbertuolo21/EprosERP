using Epros.ERP.DfeCalculos.Impostos.Abstracts;
using Epros.ERP.DfeCalculos.Impostos.IbsCbss;
using Epros.ERP.DfeCalculos.Models.Vendas;
using Epros.ERP.Shared.Extensions;

namespace Epros.ERP.DfeCalculos.Impostos.Ipis
{
    public class CalculaCst000 : CalculaIbsCbsCst
    {
        public override VendaItemIbsCbs ObterIbsCbs(VendaItem vendaItem)
        {
            VendaItemIbsCbs ibsCbs = new VendaItemIbsCbs();
            ibsCbs.Cst = vendaItem.CstIbsCbs;
            ibsCbs.CClassTrib = vendaItem.CClassTrib;
            switch (vendaItem.CClassTrib!.Substring(3))
            {
                case "001":
                case "003":
                case "004":
                    // CBS
                    ibsCbs.AliquotaCbs = aliquotaCbs;

                    var retirarValorImpostos = 
                        (vendaItem.Imposto?.Icms?.ValorImpostoDevido ?? decimal.Zero) +
                        (vendaItem.Imposto?.Icms?.ValorImpostoDevidoFcp ?? decimal.Zero) +
                        (vendaItem.Imposto?.Icms?.ValorImpostoDevidoDifal ?? decimal.Zero) +
                        (vendaItem.Imposto?.Icms?.ValorImpostoMonoRetido ?? decimal.Zero) +
                        (vendaItem.Imposto?.Pis?.ValorImpostoDevido ?? decimal.Zero) +
                        (vendaItem.Imposto?.Cofins?.ValorImpostoDevido ?? decimal.Zero);

                    ibsCbs.ValorBaseDeCalculo = vendaItem.ValorItem + 
                        vendaItem.ObterAdicionaisAEmbutirBaseCalculoIcms() - 
                        vendaItem.ObterDescontosAEmbutirBaseCalculoIcms() - 
                        retirarValorImpostos;

                    ibsCbs.ValorImpostoDevidoCbs = ((ibsCbs.ValorBaseDeCalculo * (aliquotaCbs / 100))).Arredonda2();

                    // IBS Estadual
                    ibsCbs.AliquotaEstadual = aliquotaIbsEstadual;
             

                    ibsCbs.ValorImpostoDevidoEstadual = (ibsCbs.ValorBaseDeCalculo * (aliquotaIbsEstadual / 100)).Arredonda2();

                    // IBS Municipal
                    ibsCbs.AliquotaMunicipal = aliquotaIbsMunicipal;
                    
                    ibsCbs.ValorImpostoDevidoMunicipal = (ibsCbs.ValorBaseDeCalculo * (aliquotaIbsMunicipal / 100)).Arredonda2();
                    return ibsCbs;
                default:
                    new Exception("CST IBS CBS inválido");
                    break;
            }

            return ibsCbs;
        }
    }
}
