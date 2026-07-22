using Epros.ERP.DfeCalculos.Impostos.IbsCbss;
using Epros.ERP.DfeCalculos.Models.Vendas;

namespace Epros.ERP.DfeCalculos.Impostos.Abstracts
{
    public abstract class CalculaIbsCbsCst
    {
        protected readonly decimal aliquotaCbs = 0.90m;
        protected readonly decimal aliquotaIbsEstadual = 0.10m;
        protected readonly decimal aliquotaIbsMunicipal = decimal.Zero;
        protected readonly decimal aliquotaIbsImpostoSeletivo = decimal.Zero;
        protected readonly decimal aliquotaIbsDiferimento = 100m;
        public abstract VendaItemIbsCbs ObterIbsCbs(VendaItem vendaItem);
    }
}
