using Epros.ERP.DfeCalculos.Impostos.Abstracts;
using Epros.ERP.DfeCalculos.Models.Vendas;
using Epros.ERP.Shared.Enums;
using Epros.ERP.Shared.Extensions;

namespace Epros.ERP.DfeCalculos.Impostos.Icms
{
    public class CalculaIcmsCst20SAT : CalculaIcmsCst
    {
        public override VendaItemIcms ObterIcms(VendaItem vendaItem)
        {
            var icms = new VendaItemIcms();
            icms.Cst = vendaItem.CstIcms;
            icms.Aliquota = vendaItem.ValorAliquotaIcms;
            icms.Origem = vendaItem.Origem;
            icms.AliquotaReducaoPercentual = vendaItem.ValorReducaoIcmsPercentual;

            if (vendaItem.TipoReducaoIcms == ETipoReducaoBaseDeCalculo.Em)
                icms.AliquotaReducaoPercentual = (icms.Aliquota * (100 - icms.AliquotaReducaoPercentual) / 100).Arredonda2();
            else
                icms.AliquotaReducaoPercentual = (icms.Aliquota * (100 - (100 - icms.AliquotaReducaoPercentual)) / 100).Arredonda2();

            icms.Aliquota = icms.AliquotaReducaoPercentual;

            icms.ValorImpostoDevido = (vendaItem.ValorUnitario * (icms.AliquotaReducaoPercentual / 100)).Arredonda2();
            return icms;
        }
    }
}
