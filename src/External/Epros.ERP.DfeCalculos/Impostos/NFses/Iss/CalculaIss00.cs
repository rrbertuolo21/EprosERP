using Epros.ERP.DfeCalculos.Impostos.NFses.Abstracts;
using Epros.ERP.Shared.Extensions;

namespace Epros.ERP.DfeCalculos.Impostos.NFses.Iss
{
    public class CalculaIss00 : CalculaIss
    {
        public override ServicoItemIss ObterIss(decimal valorServico, decimal aliquotaServico)
        {
            return new ServicoItemIss(aliquotaServico, ((valorServico * aliquotaServico) / 100).Arredonda2());
        }
    }
}
