using Epros.ERP.DfeCalculos.Impostos.Abstracts;
using Epros.ERP.DfeCalculos.Models.Vendas;
using Epros.ERP.Shared.Extensions;

namespace Epros.ERP.DfeCalculos.Impostos.Ipis
{
    public partial class CalculaIpi00 : CalculaIpi
    {
        public override VendaItemIpi ObterIpi(VendaItem vendaItem)
        {
            var ipi = new VendaItemIpi();
            ipi.Cst = vendaItem.CstIpi;
            ipi.Aliquota = vendaItem.ValorAliquotaIpi;
            ipi.ValorBaseDeCalculo = vendaItem.ValorItem;
            ipi.ValorImpostoDevido = (ipi.ValorBaseDeCalculo * (vendaItem.ValorAliquotaIpi / 100)).Arredonda2();
            return ipi;
        }
    }
}
