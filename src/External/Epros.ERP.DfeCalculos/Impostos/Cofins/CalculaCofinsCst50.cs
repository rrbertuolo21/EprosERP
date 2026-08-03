using Epros.ERP.DfeCalculos.Impostos.Abstracts;
using Epros.ERP.DfeCalculos.Models.Vendas;
using Epros.ERP.Shared.Extensions;

namespace Epros.ERP.DfeCalculos.Impostos.Cofins
{
    public class CalculaCofinsCst50 : CalculaCofinsCst
    {
        public override VendaItemCofins ObterCofins(VendaItem vendaItem)
        {
            var cofins = new VendaItemCofins();

            // Regra CNAEs
            decimal valorCusto = decimal.Zero;
            if (CNAES.Contains(vendaItem.EmitenteCnae))
                valorCusto = vendaItem.ValorCompra;

            cofins.Cst = vendaItem.CstPisCofins;
            cofins.ValorBaseDeCalculo = ((vendaItem.ValorItem + vendaItem.ObterAdicionaisAEmbutirBaseCalculoIcms()) - vendaItem.ObterDescontosAEmbutirBaseCalculoIcms()) - vendaItem.Imposto.Icms!.ValorImpostoDevido - valorCusto;
            cofins.AliquotaPercetual = vendaItem.ValorAliquotaCofins;
            cofins.ValorImpostoDevido = (cofins.ValorBaseDeCalculo * (vendaItem.ValorAliquotaCofins / 100)).Arredonda2();
            return cofins;
        }
    }
}
