namespace Epros.ERP.DfeCalculos.Impostos.Icms
{
    public class VendaItemIcms
    {
        public VendaItemIcms() { }
        public VendaItemIcms(decimal valorBaseDeCalculo, decimal valorBaseDeCalculoSt, decimal valorBaseDeCalculoFcp, decimal valorBaseDeCalculoStFcp, decimal valorBaseDeCalculoDifal, decimal aliquota, decimal aliquotaSt, decimal aliquotaFcp, decimal aliquotaFcpSt, decimal aliquotaMva, decimal aliquotaDifalInterna, decimal aliquotaDifalInterestadual, decimal aliquotaReducaoPercentual, decimal aliquotaReducaoPercentualSt, decimal valorImpostoDevido, decimal valorCredito, decimal valorImpostoDevidoFcp, decimal valorImpostoDevidoFcpSt, decimal valorImpostoDevidoSt, decimal valorImpostoDevidoRecolherSt, decimal valorImpostoDevidoRecolherFcpSt, decimal valorImpostoDevidoDifal, string? cst, string? origem, string? csosn, decimal valorIcmsIsento, decimal valorIcmsOutros, string? icmsObservacao)
        {
            ValorBaseDeCalculo = valorBaseDeCalculo;
            ValorBaseDeCalculoSt = valorBaseDeCalculoSt;
            ValorBaseDeCalculoFcp = valorBaseDeCalculoFcp;
            ValorBaseDeCalculoStFcp = valorBaseDeCalculoStFcp;
            ValorBaseDeCalculoDifal = valorBaseDeCalculoDifal;
            Aliquota = aliquota;
            AliquotaSt = aliquotaSt;
            AliquotaFcp = aliquotaFcp;
            AliquotaFcpSt = aliquotaFcpSt;
            AliquotaMva = aliquotaMva;
            AliquotaDifalInterna = aliquotaDifalInterna;
            AliquotaDifalInterestadual = aliquotaDifalInterestadual;
            AliquotaReducaoPercentual = aliquotaReducaoPercentual;
            AliquotaReducaoPercentualSt = aliquotaReducaoPercentualSt;
            ValorImpostoDevido = valorImpostoDevido;
            ValorCredito = valorCredito;
            ValorImpostoDevidoFcp = valorImpostoDevidoFcp;
            ValorImpostoDevidoFcpSt = valorImpostoDevidoFcpSt;
            ValorImpostoDevidoSt = valorImpostoDevidoSt;
            ValorImpostoDevidoRecolherSt = valorImpostoDevidoRecolherSt;
            ValorImpostoDevidoRecolherFcpSt = valorImpostoDevidoRecolherFcpSt;
            ValorImpostoDevidoDifal = valorImpostoDevidoDifal;
            Cst = cst;
            Origem = origem;
            Csosn = csosn;
            ValorIcmsIsento = valorIcmsIsento;
            ValorIcmsOutros = valorIcmsOutros;
            IcmsObservacao = icmsObservacao;
        }

        // Qtdade

        public decimal QtdeBCMonoRetido { get; set; }

        // Base
        public decimal ValorBaseDeCalculo { get; set; }
        public decimal ValorBaseDeCalculoSt { get; set; }
        public decimal ValorBaseDeCalculoFcp { get; set; }
        public decimal ValorBaseDeCalculoStFcp { get; set; }
        public decimal ValorBaseDeCalculoDifal { get; set; }
        public int TipoCalculoBaseIcmsSt { get; set; }
        // Aliquotas
        public decimal Aliquota { get; set; }
        public decimal AliquotaSt { get; set; }
        public decimal AliquotaFcp { get; set; }
        public decimal AliquotaFcpSt { get; set; }
        public decimal AliquotaMva { get; set; }
        public decimal AliquotaDifalInterna { get; set; }
        public decimal AliquotaDifalInterestadual { get; set; }
        public decimal AliquotaReducaoPercentual { get; set; }
        public decimal AliquotaReducaoPercentualSt { get; set; }
        public decimal ValorUnitFixadoIcmsSt { get; set; }
        public decimal ValorAliquotaAdRemRetido { get; set; }
        // Impostos Devidos
        public decimal ValorImpostoDevido { get; set; }
        public decimal ValorCredito { get; set; }
        public decimal ValorImpostoDevidoFcp { get; set; }
        public decimal ValorImpostoDevidoFcpSt { get; set; }
        public decimal ValorImpostoDevidoSt { get; set; }
        public decimal ValorImpostoDevidoRecolherSt { get; set; }
        public decimal ValorImpostoDevidoRecolherFcpSt { get; set; }
        public decimal ValorImpostoDevidoDifal { get; set; }
        public decimal ValorImpostoMonoRetido { get; set; }
        // Outros
        public string? Cst { get; set; }
        public string? Origem { get; set; }
        public string? Csosn { get; set; }
        public decimal ValorIcmsIsento { get; set; }
        public decimal ValorIcmsOutros { get; set; }
        public string? IcmsObservacao { get; set; }
    }
}
