using Epros.ERP.DfeCalculos.Impostos.Abstracts;
using Epros.ERP.DfeCalculos.Models.Vendas;
using Epros.ERP.Shared.Extensions;

namespace Epros.ERP.DfeCalculos.Impostos.Pis
{
    public class CalculaPisCst03 : CalculaPisCst
    {
        public override VendaItemPis ObterPis(VendaItem vendaItem)
        {
            return new VendaItemPis
            {
                Cst = vendaItem.CstPisCofins,
                QuantidadeVendida = vendaItem.Quantidade,
                ValorBaseDeCalculo = vendaItem.Quantidade,
                AliquotaReal = vendaItem.ValorAliquotaPisReal,
                ValorImpostoDevido = (vendaItem.Quantidade * vendaItem.ValorAliquotaPisReal).Arredonda2()
            };
        }
    }
}
