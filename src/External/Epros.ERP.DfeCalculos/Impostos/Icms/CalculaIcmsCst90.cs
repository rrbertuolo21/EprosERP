using Epros.ERP.DfeCalculos.Impostos.Abstracts;
using Epros.ERP.DfeCalculos.Models.Vendas;
using Epros.ERP.Shared.Extensions;

namespace Epros.ERP.DfeCalculos.Impostos.Icms
{
    public class CalculaIcmsCst90 : CalculaIcmsCst
    {
        public override VendaItemIcms ObterIcms(VendaItem vendaItem)
        {
            var icms = new VendaItemIcms();
            icms.Origem = vendaItem.Origem;
            icms.Cst = vendaItem.CstIcms;
            icms.Aliquota = vendaItem.ValorAliquotaIcms;
            icms.ValorBaseDeCalculo = ((vendaItem.ValorItem +
                (!vendaItem.IpiEmbutido ? decimal.Zero : (vendaItem.Imposto.Ipi == null || vendaItem.IpiEmbutido == false ? decimal.Zero : vendaItem.Imposto.Ipi.ValorImpostoDevido)) +
                vendaItem.ObterAdicionaisAEmbutirBaseCalculoIcms()) - vendaItem.ObterDescontosAEmbutirBaseCalculoIcms());
            icms.ValorImpostoDevido = (icms.ValorBaseDeCalculo * (vendaItem.ValorAliquotaIcms / 100)).Arredonda2();

            if (vendaItem.ValorAliquotaFcp > decimal.Zero)
            {
                // FCP
                icms.AliquotaFcp = vendaItem.ValorAliquotaFcp;
                icms.ValorBaseDeCalculoFcp = icms.ValorBaseDeCalculo;
                icms.ValorImpostoDevidoFcp = (icms.ValorBaseDeCalculoFcp * (vendaItem.ValorAliquotaFcp / 100)).Arredonda2();
            }

            if (vendaItem.ValorAliquotaIcmsInterna > decimal.Zero)
            {
                // DIFAL
                icms.AliquotaDifalInterna = vendaItem.ValorAliquotaIcmsInterna;
                icms.AliquotaDifalInterestadual = vendaItem.ValorAliquotaIcmsInterestadual;

                if (vendaItem.DifalTipoCalculoPorDentro)
                {
                    // DIFAL - DENTRO
                    icms.ValorBaseDeCalculoDifal = (vendaItem.ValorItem / (1 - (icms.AliquotaDifalInterna / 100)));
                    var valorIcmsInterno = (icms.AliquotaDifalInterna / 100) * icms.ValorBaseDeCalculoDifal;
                    var valorIcmsInterestadual = (icms.AliquotaDifalInterestadual / 100) * icms.ValorBaseDeCalculo;
                    icms.ValorImpostoDevidoDifal = ((valorIcmsInterno - valorIcmsInterestadual)).Arredonda2();
                }
                else
                {
                    // DIFAL - FORA
                    icms.ValorBaseDeCalculoDifal = icms.ValorBaseDeCalculo;
                    icms.ValorImpostoDevidoDifal = (((icms.AliquotaDifalInterna - icms.AliquotaDifalInterestadual) / 100) * icms.ValorBaseDeCalculo).Arredonda2();
                }

                icms.ValorIcmsOutros = vendaItem.ValorItem;
            }
            return icms;
        }
    }
}
