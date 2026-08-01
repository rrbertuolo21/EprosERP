using Epros.ERP.DfeCalculos.Impostos.NFses.Iss;

namespace Epros.ERP.DfeCalculos.Impostos.NFses.Abstracts
{
    public abstract class CalculaIss
    {
        public abstract ServicoItemIss ObterIss(decimal valorServico, decimal aliquotaServico);
    }
}
