using Epros.ERP.DfeCalculos.Impostos.Abstracts;
using Epros.ERP.DfeCalculos.Models.Vendas;
using Epros.ERP.Shared.Enums;
using Epros.ERP.Shared.Extensions;
using NFe.Classes.Informacoes.Detalhe.Tributacao.Estadual.Tipos;

namespace Epros.ERP.DfeCalculos.Impostos.Icms
{
    public class CalculaIcmsCst70 : CalculaIcmsCst
    {
        public override VendaItemIcms ObterIcms(VendaItem vendaItem)
        {
            // ICMS
            var icms = new VendaItemIcms();
            icms.Origem = vendaItem.Origem;
            icms.Cst = vendaItem.CstIcms;
            icms.Aliquota = vendaItem.ValorAliquotaIcms;
            icms.AliquotaReducaoPercentual = vendaItem.ValorReducaoIcmsPercentual;
            //icms.ValorBaseDeCalculo = ((vendaItem.ValorItem + vendaItem.ObterAdicionaisAEmbutirBaseCalculoIcms()) - vendaItem.ObterDescontosAEmbutirBaseCalculoIcms());            

            if (vendaItem.TipoReducaoIcms == ETipoReducaoBaseDeCalculo.Em)
            {
                icms.ValorBaseDeCalculo = (((vendaItem.ValorItem +
                    vendaItem.ObterAdicionaisAEmbutirBaseCalculoIcms()) +
                    (!vendaItem.IpiEmbutido ? decimal.Zero : (vendaItem.Imposto.Ipi == null || vendaItem.IpiEmbutido == false ? decimal.Zero : vendaItem.Imposto.Ipi.ValorImpostoDevido)) -
                    vendaItem.ObterDescontosAEmbutirBaseCalculoIcms()) * (1 - (icms.AliquotaReducaoPercentual / 100))).Arredonda2();
            }
            else if (vendaItem.TipoReducaoIcms == ETipoReducaoBaseDeCalculo.Para)
            {
                icms.ValorBaseDeCalculo = (((vendaItem.ValorItem +
                    vendaItem.ObterAdicionaisAEmbutirBaseCalculoIcms()) +
                    (!vendaItem.IpiEmbutido ? decimal.Zero : (vendaItem.Imposto.Ipi == null || vendaItem.IpiEmbutido == false ? decimal.Zero : vendaItem.Imposto.Ipi.ValorImpostoDevido)) -
                    vendaItem.ObterDescontosAEmbutirBaseCalculoIcms()) * (1 * (icms.AliquotaReducaoPercentual / 100))).Arredonda2();
            }

            icms.ValorImpostoDevido = (icms.ValorBaseDeCalculo * (vendaItem.ValorAliquotaIcms / 100)).Arredonda2();

            // ST
            icms.AliquotaMva = vendaItem.ValorAliquotaMva;
            icms.AliquotaSt = vendaItem.ValorAliquotaSt;
            icms.AliquotaReducaoPercentualSt = vendaItem.ValorReducaoIcmsPercentualSt;

            if (vendaItem.TipoCalculoBaseIcmsSt == DeterminacaoBaseIcmsSt.DbisPauta)
                icms.ValorBaseDeCalculoSt = ((vendaItem.Quantidade * vendaItem.ValorUnitFixadoIcmsSt)).Arredonda2();
            else
                icms.ValorBaseDeCalculoSt = (((vendaItem.ValorItem + (vendaItem.Imposto?.Ipi == null ? decimal.Zero : vendaItem.Imposto.Ipi.ValorImpostoDevido) + vendaItem.ObterAdicionaisAEmbutirBaseCalculoIcms()) * (1 + (vendaItem.ValorAliquotaMva / 100)))).Arredonda2();

            if (vendaItem.TipoReducaoIcmsSt == ETipoReducaoBaseDeCalculo.Em)
            {
                icms.ValorBaseDeCalculoSt = (icms.ValorBaseDeCalculoSt - (vendaItem.ValorReducaoIcmsPercentualSt / 100)).Arredonda2();
            }
            else if (vendaItem.TipoReducaoIcmsSt == ETipoReducaoBaseDeCalculo.Para)
            {
                icms.ValorBaseDeCalculoSt = (icms.ValorBaseDeCalculoSt * (vendaItem.ValorReducaoIcmsPercentualSt / 100)).Arredonda2();
            }

            icms.ValorImpostoDevidoRecolherSt = (((icms.ValorBaseDeCalculoSt * (icms.AliquotaSt / 100)) - icms.ValorImpostoDevido)).Arredonda2();

            if (vendaItem.ValorAliquotaFcp > decimal.Zero)
            {
                // FCP
                icms.AliquotaFcp = vendaItem.ValorAliquotaFcp;
                icms.ValorBaseDeCalculoFcp = icms.ValorBaseDeCalculo;
                icms.ValorImpostoDevidoFcp = (icms.ValorBaseDeCalculoFcp * (vendaItem.ValorAliquotaFcp / 100)).Arredonda2();

                // FCP ST
                icms.AliquotaFcpSt = vendaItem.ValorAliquotaFcpSt;
                icms.ValorBaseDeCalculoStFcp = icms.ValorBaseDeCalculoSt;
                icms.ValorImpostoDevidoFcpSt = (icms.ValorBaseDeCalculoStFcp * (vendaItem.ValorAliquotaFcpSt / 100)).Arredonda2();
                icms.ValorImpostoDevidoRecolherFcpSt = (icms.ValorImpostoDevidoFcpSt - icms.ValorImpostoDevidoFcp);
                icms.ValorImpostoDevidoDifal += icms.ValorImpostoDevidoFcp;
            }

            icms.ValorIcmsOutros = vendaItem.ValorItem;
            return icms;
        }
    }
}
