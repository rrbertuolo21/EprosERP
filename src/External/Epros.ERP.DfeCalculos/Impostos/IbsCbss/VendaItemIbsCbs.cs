namespace Epros.ERP.DfeCalculos.Impostos.IbsCbss
{
    public class VendaItemIbsCbs
    {
        public VendaItemIbsCbs() { }
        public VendaItemIbsCbs(string? cst, string? cClassTrib, decimal valorBaseDeCalculo, decimal aliquotaEstadual, decimal aliquotaMunicipal, decimal aliquotaCbs, decimal aliquotaEstadualReducao, decimal aliquotaMunicipalReducao, decimal aliquotaCbsReducao, decimal aliquotaEstadualDiferimento, decimal aliquotaMunicipalDiferimento, decimal aliquotaCbsDiferimento, decimal aliquotaEfetivaEstadual, decimal aliquotaEfetivaMunicipal, decimal aliquotaEfetivaCbs, decimal valorImpostoDevidoEstadual, decimal valorImpostoDevidoMunicipal, decimal valorImpostoDevidoCbs, VendaItemIbsCbsTributacaoRegular? tributacaoRegular)
        {
            Cst = cst;
            CClassTrib = cClassTrib;
            ValorBaseDeCalculo = valorBaseDeCalculo;
            AliquotaEstadual = aliquotaEstadual;
            AliquotaMunicipal = aliquotaMunicipal;
            AliquotaCbs = aliquotaCbs;
            AliquotaEstadualReducao = aliquotaEstadualReducao;
            AliquotaMunicipalReducao = aliquotaMunicipalReducao;
            AliquotaCbsReducao = aliquotaCbsReducao;
            AliquotaEstadualDiferimento = aliquotaEstadualDiferimento;
            AliquotaMunicipalDiferimento = aliquotaMunicipalDiferimento;
            AliquotaCbsDiferimento = aliquotaCbsDiferimento;
            AliquotaEfetivaEstadual = aliquotaEfetivaEstadual;
            AliquotaEfetivaMunicipal = aliquotaEfetivaMunicipal;
            AliquotaEfetivaCbs = aliquotaEfetivaCbs;
            ValorImpostoDevidoEstadual = valorImpostoDevidoEstadual;
            ValorImpostoDevidoMunicipal = valorImpostoDevidoMunicipal;
            ValorImpostoDevidoCbs = valorImpostoDevidoCbs;
            TributacaoRegular = tributacaoRegular;
        }

        public string? Cst { get; set; }
        public string? CClassTrib { get; set; }
        public decimal ValorBaseDeCalculo { get; set; }
        //public decimal ValorBaseDeCalculoReducaoEstadual { get; set; }
        //public decimal ValorBaseDeCalculoReducaoMunicipal { get; set; }
        //public decimal ValorBaseDeCalculoReducaoCbs { get; set; }

        public decimal AliquotaEstadual { get; set; }
        public decimal AliquotaMunicipal { get; set; }
        public decimal AliquotaCbs { get; set; }

        public decimal AliquotaEstadualReducao { get; set; }
        public decimal AliquotaMunicipalReducao { get; set; }
        public decimal AliquotaCbsReducao { get; set; }

        public decimal AliquotaEstadualDiferimento { get; set; }
        public decimal AliquotaMunicipalDiferimento { get; set; }
        public decimal AliquotaCbsDiferimento { get; set; }

        public decimal AliquotaEfetivaEstadual { get; set; }
        public decimal AliquotaEfetivaMunicipal { get; set; }
        public decimal AliquotaEfetivaCbs { get; set; }

        public decimal ValorImpostoDevidoEstadual { get; set; }
        public decimal ValorImpostoDevidoMunicipal { get; set; }
        public decimal ValorImpostoDevidoCbs { get; set; }

        public VendaItemIbsCbsTributacaoRegular? TributacaoRegular { get; set; }
    }
}
