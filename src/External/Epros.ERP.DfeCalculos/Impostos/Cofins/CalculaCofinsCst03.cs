using Epros.ERP.DfeCalculos.Impostos.Abstracts;
using Epros.ERP.DfeCalculos.Models.Vendas;
using Epros.ERP.Shared.Extensions;

namespace Epros.ERP.DfeCalculos.Impostos.Cofins
{
    public class CalculaCofinsCst03 : CalculaCofinsCst
    {
        public override VendaItemCofins ObterCofins(VendaItem vendaItem)
        {
            return new VendaItemCofins
            {
                Cst = vendaItem.CstPisCofins,
                QuantidadeVendida = vendaItem.Quantidade,
                ValorBaseDeCalculo = vendaItem.Quantidade,
                AliquotaReal = vendaItem.ValorAliquotaCofinsReal,
                ValorImpostoDevido = (vendaItem.Quantidade * vendaItem.ValorAliquotaCofinsReal).Arredonda2()
            };
        }
    }
}
