using Epros.ERP.DfeCalculos.Impostos.Abstracts;
using Epros.ERP.DfeCalculos.Impostos.IbsCbss;
using Epros.ERP.DfeCalculos.Models.Vendas;
using Epros.ERP.Shared.Extensions;

namespace Epros.ERP.DfeCalculos.Impostos.Ipis
{
    public class CalculaCst200 : CalculaIbsCbsCst
    {
        public override VendaItemIbsCbs ObterIbsCbs(VendaItem vendaItem)
        {
            VendaItemIbsCbs ibsCbs = new VendaItemIbsCbs();
            ibsCbs.Cst = vendaItem.CstIbsCbs;
            ibsCbs.CClassTrib = vendaItem.CClassTrib;

            var valorBaseDeCalculoReducaoCbs = decimal.Zero;
            var valorBaseDeCalculoReducaoEstadual = decimal.Zero;
            var valorBaseDeCalculoReducaoMunicipal = decimal.Zero;

            switch (vendaItem.CClassTrib!.Substring(3))
            {
                case "002":
                case "003":
                case "004":
                case "005":
                case "006":
                case "007":
                case "008":
                case "009":
                case "010":
                case "011":
                case "012":
                case "013":
                case "014":
                case "015":
                case "022":
                case "023":
                case "024":
                    // Aliquotas
                    ibsCbs.AliquotaCbs = aliquotaCbs;
                    ibsCbs.AliquotaCbsReducao = 100;
                    ibsCbs.AliquotaEstadualReducao = 100;
                    ibsCbs.AliquotaMunicipalReducao = 100;

                    // CBS
                    ibsCbs.ValorBaseDeCalculo = vendaItem.ValorItem;

                    ibsCbs.ValorImpostoDevidoCbs = decimal.Zero;
                    // IBS Estadual
                    ibsCbs.AliquotaEstadual = aliquotaIbsEstadual; // decimal.Zero;
                    ibsCbs.ValorImpostoDevidoEstadual = decimal.Zero;
                    // IBS Municipal
                    ibsCbs.AliquotaMunicipal = decimal.Zero;
                    ibsCbs.ValorImpostoDevidoMunicipal = decimal.Zero;
                    return ibsCbs;
                case "030":
                case "031":
                case "032":
                case "033":
                case "034":
                case "035":
                case "036":
                case "037":
                case "038":
                case "039":
                case "042":
                case "043":
                case "044":
                    // Aliquotas
                    ibsCbs.AliquotaCbs = aliquotaCbs;
                    ibsCbs.AliquotaCbsReducao = 60;
                    ibsCbs.AliquotaEstadualReducao = 60;
                    ibsCbs.AliquotaMunicipalReducao = 60;

                    ibsCbs.ValorBaseDeCalculo = (((vendaItem.ValorItem + vendaItem.ObterAdicionaisAEmbutirBaseCalculoIcms()) - vendaItem.ObterDescontosAEmbutirBaseCalculoIcms())).Arredonda2();

                    // CBS
                    valorBaseDeCalculoReducaoCbs = ((((vendaItem.ValorItem + vendaItem.ObterAdicionaisAEmbutirBaseCalculoIcms()) - vendaItem.ObterDescontosAEmbutirBaseCalculoIcms()) * (1 - (ibsCbs.AliquotaCbsReducao / 100)))).Arredonda2();

                    ibsCbs.AliquotaEfetivaCbs = (ibsCbs.AliquotaCbs * (1 - (ibsCbs.AliquotaCbsReducao / 100))).Arredonda2();

                    ibsCbs.ValorImpostoDevidoCbs = ((valorBaseDeCalculoReducaoCbs * (ibsCbs.AliquotaCbs / 100))).Arredonda2();

                    // IBS Estadual
                    ibsCbs.AliquotaEstadual = aliquotaIbsEstadual;
                    valorBaseDeCalculoReducaoEstadual = ((((vendaItem.ValorItem + vendaItem.ObterAdicionaisAEmbutirBaseCalculoIcms()) - vendaItem.ObterDescontosAEmbutirBaseCalculoIcms()) * (1 - (ibsCbs.AliquotaEstadualReducao / 100)))).Arredonda2();
                    ibsCbs.ValorImpostoDevidoEstadual = ((valorBaseDeCalculoReducaoEstadual * (aliquotaIbsEstadual / 100))).Arredonda2();
                    ibsCbs.AliquotaEfetivaEstadual = (ibsCbs.AliquotaEstadual * (1 - (ibsCbs.AliquotaEstadualReducao / 100))).Arredonda2();

                    // IBS Municipal
                    ibsCbs.AliquotaMunicipal = aliquotaIbsMunicipal;
                    valorBaseDeCalculoReducaoMunicipal = ((((vendaItem.ValorItem + vendaItem.ObterAdicionaisAEmbutirBaseCalculoIcms()) - vendaItem.ObterDescontosAEmbutirBaseCalculoIcms()) * (1 - (ibsCbs.AliquotaMunicipalReducao / 100)))).Arredonda2();
                    ibsCbs.ValorImpostoDevidoMunicipal = ((valorBaseDeCalculoReducaoMunicipal * (aliquotaIbsMunicipal / 100))).Arredonda2();
                    ibsCbs.AliquotaEfetivaMunicipal = (ibsCbs.AliquotaMunicipal * (1 - (ibsCbs.AliquotaMunicipalReducao / 100))).Arredonda2();
                    return ibsCbs;
                case "046":
                    // Aliquotas
                    ibsCbs.AliquotaCbs = aliquotaCbs;
                    ibsCbs.AliquotaCbsReducao = 50;
                    ibsCbs.AliquotaEstadualReducao = 50;
                    ibsCbs.AliquotaMunicipalReducao = 50;

                    ibsCbs.ValorBaseDeCalculo = (((vendaItem.ValorItem + vendaItem.ObterAdicionaisAEmbutirBaseCalculoIcms()) - vendaItem.ObterDescontosAEmbutirBaseCalculoIcms())).Arredonda2();

                    // CBS
                    valorBaseDeCalculoReducaoCbs = ((((vendaItem.ValorItem + vendaItem.ObterAdicionaisAEmbutirBaseCalculoIcms()) - vendaItem.ObterDescontosAEmbutirBaseCalculoIcms()) * (1 - (ibsCbs.AliquotaCbsReducao / 100)))).Arredonda2();

                    ibsCbs.AliquotaEfetivaCbs = (ibsCbs.AliquotaCbs * (1 - (ibsCbs.AliquotaCbsReducao / 100))).Arredonda2();

                    ibsCbs.ValorImpostoDevidoCbs = ((valorBaseDeCalculoReducaoCbs * (ibsCbs.AliquotaCbs / 100))).Arredonda2();

                    // IBS Estadual
                    ibsCbs.AliquotaEstadual = aliquotaIbsEstadual;
                    valorBaseDeCalculoReducaoEstadual = ((((vendaItem.ValorItem + vendaItem.ObterAdicionaisAEmbutirBaseCalculoIcms()) - vendaItem.ObterDescontosAEmbutirBaseCalculoIcms()) * (1 - (ibsCbs.AliquotaEstadualReducao / 100)))).Arredonda2();
                    ibsCbs.ValorImpostoDevidoEstadual = ((valorBaseDeCalculoReducaoEstadual * (aliquotaIbsEstadual / 100))).Arredonda2();
                    ibsCbs.AliquotaEfetivaEstadual = (ibsCbs.AliquotaEstadual * (1 - (ibsCbs.AliquotaEstadualReducao / 100))).Arredonda2();

                    // IBS Municipal
                    ibsCbs.AliquotaMunicipal = aliquotaIbsMunicipal;
                    valorBaseDeCalculoReducaoMunicipal = ((((vendaItem.ValorItem + vendaItem.ObterAdicionaisAEmbutirBaseCalculoIcms()) - vendaItem.ObterDescontosAEmbutirBaseCalculoIcms()) * (1 - (ibsCbs.AliquotaMunicipalReducao / 100)))).Arredonda2();
                    ibsCbs.ValorImpostoDevidoMunicipal = ((valorBaseDeCalculoReducaoMunicipal * (aliquotaIbsMunicipal / 100))).Arredonda2();
                    ibsCbs.AliquotaEfetivaMunicipal = (ibsCbs.AliquotaMunicipal * (1 - (ibsCbs.AliquotaMunicipalReducao / 100))).Arredonda2();
                    return ibsCbs;
                case "047":
                case "048":
                    // Aliquotas
                    ibsCbs.AliquotaCbs = aliquotaCbs;
                    ibsCbs.AliquotaCbsReducao = 40;
                    ibsCbs.AliquotaEstadualReducao = 40;
                    ibsCbs.AliquotaMunicipalReducao = 40;

                    ibsCbs.ValorBaseDeCalculo = (((vendaItem.ValorItem + vendaItem.ObterAdicionaisAEmbutirBaseCalculoIcms()) - vendaItem.ObterDescontosAEmbutirBaseCalculoIcms())).Arredonda2();

                    // CBS
                    valorBaseDeCalculoReducaoCbs = ((((vendaItem.ValorItem + vendaItem.ObterAdicionaisAEmbutirBaseCalculoIcms()) - vendaItem.ObterDescontosAEmbutirBaseCalculoIcms()) * (1 - (ibsCbs.AliquotaCbsReducao / 100)))).Arredonda2();

                    ibsCbs.AliquotaEfetivaCbs = (ibsCbs.AliquotaCbs * (1 - (ibsCbs.AliquotaCbsReducao / 100))).Arredonda2();

                    ibsCbs.ValorImpostoDevidoCbs = ((valorBaseDeCalculoReducaoCbs * (ibsCbs.AliquotaCbs / 100))).Arredonda2();

                    // IBS Estadual
                    ibsCbs.AliquotaEstadual = aliquotaIbsEstadual;
                    valorBaseDeCalculoReducaoEstadual = ((((vendaItem.ValorItem + vendaItem.ObterAdicionaisAEmbutirBaseCalculoIcms()) - vendaItem.ObterDescontosAEmbutirBaseCalculoIcms()) * (1 - (ibsCbs.AliquotaEstadualReducao / 100)))).Arredonda2();
                    ibsCbs.ValorImpostoDevidoEstadual = ((valorBaseDeCalculoReducaoEstadual * (aliquotaIbsEstadual / 100))).Arredonda2();
                    ibsCbs.AliquotaEfetivaEstadual = (ibsCbs.AliquotaEstadual * (1 - (ibsCbs.AliquotaEstadualReducao / 100))).Arredonda2();

                    // IBS Municipal
                    ibsCbs.AliquotaMunicipal = aliquotaIbsMunicipal;
                    valorBaseDeCalculoReducaoMunicipal = ((((vendaItem.ValorItem + vendaItem.ObterAdicionaisAEmbutirBaseCalculoIcms()) - vendaItem.ObterDescontosAEmbutirBaseCalculoIcms()) * (1 - (ibsCbs.AliquotaMunicipalReducao / 100)))).Arredonda2();
                    ibsCbs.ValorImpostoDevidoMunicipal = ((valorBaseDeCalculoReducaoMunicipal * (aliquotaIbsMunicipal / 100))).Arredonda2();
                    ibsCbs.AliquotaEfetivaMunicipal = (ibsCbs.AliquotaMunicipal * (1 - (ibsCbs.AliquotaMunicipalReducao / 100))).Arredonda2();
                    return ibsCbs;
                default:
                    new Exception("CST IBS CBS inválido");
                    break;
            }

            return ibsCbs;
        }
    }
}
