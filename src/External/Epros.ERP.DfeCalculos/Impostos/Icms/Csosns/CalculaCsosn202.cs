using Epros.ERP.DfeCalculos.Impostos.Abstracts;
using Epros.ERP.DfeCalculos.Models.Vendas;
using Epros.ERP.Shared.Extensions;
using NFe.Classes.Informacoes.Detalhe.Tributacao.Estadual.Tipos;

namespace Epros.ERP.DfeCalculos.Impostos.Icms.Csosns
{
    public class CalculaCsosn202 : CalculaIcmsCsosn
    {
        public override VendaItemIcms ObterIcmsCsosn(Venda venda, VendaItem vendaItem)
        {
            var icms = new VendaItemIcms();
            icms.Origem = vendaItem.Origem;
            icms.Csosn = vendaItem.Csosn;
            // ST
            icms.AliquotaMva = vendaItem.ValorAliquotaMva;
            icms.AliquotaSt = vendaItem.ValorAliquotaSt;

            if (vendaItem.TipoCalculoBaseIcmsSt == DeterminacaoBaseIcmsSt.DbisPauta)
                icms.ValorBaseDeCalculoSt = ((vendaItem.Quantidade * vendaItem.ValorUnitFixadoIcmsSt)).Arredonda2();
            else
                icms.ValorBaseDeCalculoSt = (((vendaItem.ValorItem + vendaItem.ObterAdicionaisAEmbutirBaseCalculoIcms()) * (1 + (vendaItem.ValorAliquotaMva / 100)))).Arredonda2();

            icms.ValorImpostoDevidoSt = (icms.ValorBaseDeCalculoSt * (icms.AliquotaSt / 100)).Arredonda2();
            icms.ValorImpostoDevidoRecolherSt = icms.ValorImpostoDevidoSt;
            // FCP
            icms.AliquotaFcp = vendaItem.ValorAliquotaFcp;
            icms.ValorBaseDeCalculoFcp = decimal.Zero;
            icms.ValorImpostoDevidoFcp = (icms.ValorBaseDeCalculoFcp * (vendaItem.ValorAliquotaFcp / 100)).Arredonda2();
            // FCP ST
            icms.AliquotaFcpSt = vendaItem.ValorAliquotaFcp;
            icms.ValorBaseDeCalculoStFcp = icms.ValorBaseDeCalculoSt;
            icms.ValorImpostoDevidoFcpSt = (icms.ValorBaseDeCalculoStFcp * (vendaItem.ValorAliquotaFcpSt / 100)).Arredonda2();
            icms.ValorImpostoDevidoRecolherFcpSt = icms.ValorImpostoDevidoFcpSt;
            return icms;
        }
    }
}
