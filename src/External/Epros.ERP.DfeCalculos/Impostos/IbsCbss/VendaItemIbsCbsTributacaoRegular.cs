namespace Epros.ERP.DfeCalculos.Impostos.IbsCbss
{
    public class VendaItemIbsCbsTributacaoRegular
    {
        public VendaItemIbsCbsTributacaoRegular() { }

        public VendaItemIbsCbsTributacaoRegular(string cst, string cClassTrib, decimal aliquotaEfetivaIbsEstadual, decimal valorIbsEstadual, decimal aliquotaEfetivaIbsMunicipal, decimal valorIbsMunicipal, decimal aliquotaEfetivaCbs, decimal valorCbs)
        {
            Cst = cst;
            CClassTrib = cClassTrib;
            AliquotaEfetivaIbsEstadual = aliquotaEfetivaIbsEstadual;
            ValorIbsEstadual = valorIbsEstadual;
            AliquotaEfetivaIbsMunicipal = aliquotaEfetivaIbsMunicipal;
            ValorIbsMunicipal = valorIbsMunicipal;
            AliquotaEfetivaCbs = aliquotaEfetivaCbs;
            ValorCbs = valorCbs;
        }

        public string Cst { get; set; } = null!;
        public string CClassTrib { get; set; } = null!;
        public decimal AliquotaEfetivaIbsEstadual { get; set; }
        public decimal ValorIbsEstadual { get; set; }
        public decimal AliquotaEfetivaIbsMunicipal { get; set; }
        public decimal ValorIbsMunicipal { get; set; }
        public decimal AliquotaEfetivaCbs { get; set; }
        public decimal ValorCbs { get; set; }
    }
}
