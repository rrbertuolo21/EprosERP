using Epros.ERP.DfeCalculos.Impostos.Abstracts;
using Epros.ERP.DfeCalculos.Models.Vendas;
using Epros.ERP.Shared.Extensions;

namespace Epros.ERP.DfeCalculos.Impostos.Cofins
{
    public class CalculaCofinsCst02 : CalculaCofinsCst
    {
        public override VendaItemCofins ObterCofins(VendaItem vendaItem)
        {
            var cofins = new VendaItemCofins();
            cofins.Cst = vendaItem.CstPisCofins;
            cofins.ValorBaseDeCalculo = ((vendaItem.ValorItem + vendaItem.ObterAdicionaisAEmbutirBaseCalculoIcms()) - vendaItem.ObterDescontosAEmbutirBaseCalculoIcms()) - vendaItem.Imposto.Icms!.ValorImpostoDevido;
            cofins.AliquotaPercetual = vendaItem.ValorAliquotaCofins;
            cofins.ValorImpostoDevido = (cofins.ValorBaseDeCalculo * (vendaItem.ValorAliquotaCofins / 100)).Arredonda2();
            return cofins;
        }
    }
}
