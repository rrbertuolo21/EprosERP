using Epros.ERP.DfeCalculos.Impostos.Abstracts;
using Epros.ERP.DfeCalculos.Models.Vendas;
using Epros.ERP.Shared.Extensions;
using NFe.Classes.Informacoes.Detalhe.Tributacao.Estadual.Tipos;

namespace Epros.ERP.DfeCalculos.Impostos.Icms
{
    public class CalculaIcmsCst30 : CalculaIcmsCst
    {
        public override VendaItemIcms ObterIcms(VendaItem vendaItem)
        {
            var icms = new VendaItemIcms();
            icms.Origem = vendaItem.Origem;
            icms.Cst = vendaItem.CstIcms;
            icms.Aliquota = vendaItem.ValorAliquotaIcms;
            icms.ValorBaseDeCalculo = ((vendaItem.ValorItem + vendaItem.ObterAdicionaisAEmbutirBaseCalculoIcms()) - vendaItem.ObterDescontosAEmbutirBaseCalculoIcms());
            icms.ValorImpostoDevido = decimal.Zero;
            // ST
            icms.AliquotaMva = vendaItem.ValorAliquotaMva;
            icms.AliquotaSt = vendaItem.ValorAliquotaSt;

            icms.ValorUnitFixadoIcmsSt = vendaItem.ValorUnitFixadoIcmsSt;
            icms.TipoCalculoBaseIcmsSt = vendaItem.TipoCalculoBaseIcmsSt.GetHashCode();

            if (vendaItem.TipoCalculoBaseIcmsSt == DeterminacaoBaseIcmsSt.DbisPauta)
                icms.ValorBaseDeCalculoSt = ((vendaItem.Quantidade * vendaItem.ValorUnitFixadoIcmsSt)).Arredonda2();
            else
                icms.ValorBaseDeCalculoSt = (((vendaItem.ValorItem + (vendaItem.Imposto?.Ipi == null ? decimal.Zero : vendaItem.Imposto.Ipi.ValorImpostoDevido) + vendaItem.ObterAdicionaisAEmbutirBaseCalculoIcms()) * (1 + (vendaItem.ValorAliquotaMva / 100)))).Arredonda2();

            icms.ValorImpostoDevidoSt = (icms.ValorBaseDeCalculoSt * (icms.AliquotaSt / 100)).Arredonda2();
            icms.ValorImpostoDevidoRecolherSt = icms.ValorImpostoDevidoSt;

            if (vendaItem.ValorAliquotaFcp > decimal.Zero)
            {
                // FCP
                icms.AliquotaFcp = vendaItem.ValorAliquotaFcp;
                icms.ValorBaseDeCalculoFcp = decimal.Zero;
                icms.ValorImpostoDevidoFcp = (icms.ValorBaseDeCalculoFcp * (vendaItem.ValorAliquotaFcp / 100)).Arredonda2();
                // FCP ST
                icms.AliquotaFcpSt = vendaItem.ValorAliquotaFcp;
                icms.ValorBaseDeCalculoStFcp = icms.ValorBaseDeCalculoSt;
                icms.ValorImpostoDevidoFcpSt = (icms.ValorBaseDeCalculoStFcp * (vendaItem.ValorAliquotaFcpSt / 100)).Arredonda2();
                icms.ValorImpostoDevidoRecolherFcpSt = icms.ValorImpostoDevidoFcpSt;

                icms.ValorImpostoDevidoDifal += icms.ValorImpostoDevidoFcp;
            }

            icms.ValorIcmsIsento = vendaItem.ValorItem;

            return icms;
        }
    }
}
