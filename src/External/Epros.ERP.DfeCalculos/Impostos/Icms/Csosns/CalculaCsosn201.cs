using Epros.ERP.DfeCalculos.Impostos.Abstracts;
using Epros.ERP.DfeCalculos.Models.Vendas;
using Epros.ERP.Shared.Extensions;
using NFe.Classes.Informacoes.Detalhe.Tributacao.Estadual.Tipos;

namespace Epros.ERP.DfeCalculos.Impostos.Icms.Csosns
{
    public class CalculaCsosn201 : CalculaIcmsCsosn
    {
        public override VendaItemIcms ObterIcmsCsosn(Venda venda, VendaItem vendaItem)
        {
            var icms = new VendaItemIcms();
            icms.Origem = vendaItem.Origem;
            icms.Csosn = vendaItem.Csosn;
            icms.Aliquota = (venda.Imposto != null ? (venda.Imposto.ValorAliquotaCreditoIcms).Arredonda2() : decimal.Zero);
            icms.ValorBaseDeCalculo = ((vendaItem.ValorItem + vendaItem.ObterAdicionaisAEmbutirBaseCalculoIcms()) - vendaItem.ObterDescontosAEmbutirBaseCalculoIcms());
            icms.ValorCredito = (venda.Imposto != null ? Math.Round(((vendaItem.ValorItem * venda.Imposto.ValorAliquotaCreditoIcms) / 100), 2) : decimal.Zero);
            icms.ValorImpostoDevido = (((vendaItem.ValorItem * vendaItem.ValorAliquotaIcms) / 100)).Arredonda2();

            // ST
            icms.AliquotaMva = vendaItem.ValorAliquotaMva;
            icms.AliquotaSt = vendaItem.ValorAliquotaSt;

            if (vendaItem.TipoCalculoBaseIcmsSt == DeterminacaoBaseIcmsSt.DbisPauta)
                icms.ValorBaseDeCalculoSt = ((vendaItem.Quantidade * vendaItem.ValorUnitFixadoIcmsSt)).Arredonda2();
            else
                icms.ValorBaseDeCalculoSt = (((vendaItem.ValorItem + vendaItem.ObterAdicionaisAEmbutirBaseCalculoIcms()) * (1 + (vendaItem.ValorAliquotaMva / 100)))).Arredonda2();

            icms.ValorImpostoDevidoSt = (icms.ValorBaseDeCalculoSt * (icms.AliquotaSt / 100)).Arredonda2();
            icms.ValorImpostoDevidoRecolherSt = (icms.ValorImpostoDevidoSt - icms.ValorImpostoDevido);
            // FCP
            icms.AliquotaFcp = vendaItem.ValorAliquotaFcp;
            icms.ValorBaseDeCalculoFcp = icms.ValorBaseDeCalculo;
            icms.ValorImpostoDevidoFcp = (icms.ValorBaseDeCalculoFcp * (vendaItem.ValorAliquotaFcp / 100)).Arredonda2();
            // FCP ST
            icms.AliquotaFcpSt = vendaItem.ValorAliquotaFcp;
            icms.ValorBaseDeCalculoStFcp = icms.ValorBaseDeCalculoSt;
            icms.ValorImpostoDevidoFcpSt = (icms.ValorBaseDeCalculoStFcp * (vendaItem.ValorAliquotaFcpSt / 100)).Arredonda2();
            icms.ValorImpostoDevidoRecolherFcpSt = (icms.ValorImpostoDevidoFcpSt - icms.ValorImpostoDevidoFcp);
            return icms;
        }
    }
}
