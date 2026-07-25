using Epros.ERP.DfeCalculos.Impostos.Abstracts;
using Epros.ERP.DfeCalculos.Models.Vendas;
using Epros.ERP.Shared.Extensions;

namespace Epros.ERP.DfeCalculos.Impostos.Ipis
{
    public class CalculaIpi49 : CalculaIpi
    {
        public override VendaItemIpi ObterIpi(VendaItem vendaItem)
        {
            var ipi = new VendaItemIpi();
            ipi.ValorIpiOutros = vendaItem.ValorItem;
            ipi.Cst = vendaItem.CstIpi;
            ipi.Aliquota = vendaItem.ValorAliquotaIpi;
            ipi.ValorBaseDeCalculo = vendaItem.ValorItem; // + vendaItem.ObterAdicionaisAEmbutirBaseCalculoIpi();
            ipi.ValorImpostoDevido = (ipi.ValorBaseDeCalculo * (vendaItem.ValorAliquotaIpi / 100)).Arredonda2();
            return ipi;
        }
    }
}
