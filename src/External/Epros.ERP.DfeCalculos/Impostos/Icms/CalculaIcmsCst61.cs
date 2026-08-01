using Epros.ERP.DfeCalculos.Impostos.Abstracts;
using Epros.ERP.DfeCalculos.Models.Vendas;

namespace Epros.ERP.DfeCalculos.Impostos.Icms
{
    public class CalculaIcmsCst61 : CalculaIcmsCst
    {
        public override VendaItemIcms ObterIcms(VendaItem vendaItem)
        {
            var icms = new VendaItemIcms();
            icms.Cst = vendaItem.CstIcms;
            icms.Origem = vendaItem.Origem;
            icms.ValorBaseDeCalculo = vendaItem.QtdeBCMonoRetido;
            icms.ValorAliquotaAdRemRetido = vendaItem.ValorAliquotaAdRemRetido;
            icms.QtdeBCMonoRetido = vendaItem.QtdeBCMonoRetido;
            icms.ValorImpostoMonoRetido = (icms.ValorBaseDeCalculo * icms.ValorAliquotaAdRemRetido);
            icms.ValorIcmsOutros = vendaItem.ValorItem;
            return icms;
        }
    }
}
