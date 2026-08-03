using Epros.ERP.DfeCalculos.Impostos.Abstracts;
using Epros.ERP.DfeCalculos.Models.Vendas;

namespace Epros.ERP.DfeCalculos.Impostos.Ipis
{
    public class CalculaIpi99 : CalculaIpi
    {
        public override VendaItemIpi ObterIpi(VendaItem vendaItem)
        {
            return new VendaItemIpi { Cst = vendaItem.CstIpi, Enquadramento = vendaItem.EnquadramentoIpi!, ValorIpiOutros = vendaItem.ValorItem };
        }
        //public override VendaItemIpi ObterIpi(VendaItem vendaItem)
        //{
        //    var ipi = new VendaItemIpi();
        //    ipi.Cst = vendaItem.CstIpi;
        //    ipi.Aliquota = vendaItem.ValorAliquotaIpi;
        //    ipi.ValorBaseDeCalculo = vendaItem.ValorItem;// + vendaItem.ObterAdicionaisAEmbutirBaseCalculoIpi();
        //    ipi.ValorImpostoDevido = Math.Round(ipi.ValorBaseDeCalculo * (vendaItem.ValorAliquotaIpi / 100), 2);
        //    return ipi;
        //}
    }
}
