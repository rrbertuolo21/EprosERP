using Epros.ERP.DfeCalculos.Impostos.Abstracts;
using Epros.ERP.DfeCalculos.Models.Vendas;
using Epros.ERP.Shared.Extensions;

namespace Epros.ERP.DfeCalculos.Impostos.Pis
{
    public class CalculaPisCst02 : CalculaPisCst
    {
        public override VendaItemPis ObterPis(VendaItem vendaItem)
        {
            var pis = new VendaItemPis();
            pis.Cst = vendaItem.CstPisCofins;
            pis.ValorBaseDeCalculo = ((vendaItem.ValorItem + vendaItem.ObterAdicionaisAEmbutirBaseCalculoIcms()) - vendaItem.ObterDescontosAEmbutirBaseCalculoIcms()) - vendaItem.Imposto.Icms!.ValorImpostoDevido;
            pis.AliquotaPercetual = vendaItem.ValorAliquotaPis;
            pis.ValorImpostoDevido = (pis.ValorBaseDeCalculo * (vendaItem.ValorAliquotaPis / 100)).Arredonda2();
            return pis;
        }
    }
}
