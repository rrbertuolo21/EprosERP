namespace Epros.Modules.Fiscal.Application.Services
{
    /// <summary>
    /// Contrato limpo do módulo Fiscal para cálculo de impostos de um item/operação.
    /// Envelopa o motor legado Epros.ERP.DfeCalculos (adapter em Infrastructure/Services).
    /// ESCOPO: só cálculo (ICMS/ST/FCP/IPI/PIS/COFINS/IBS/CBS/IBPT) — sem transmissão SEFAZ.
    /// O DTO de entrada é NEUTRO: não referencia Vendas nem o namespace legado.
    /// </summary>
    public interface ICalculoFiscalService
    {
        CalculoFiscalResultado Calcular(CalculoFiscalRequest request);
    }

    /// <summary>
    /// Requisição de cálculo fiscal — dados do item/operação necessários ao motor.
    /// Espelha os campos que o motor legado exige, mas usando tipos primitivos/neutros
    /// (strings de CST/CSOSN, códigos de UF, ints de regime/modelo) para não vazar enums legados.
    /// </summary>
    public class CalculoFiscalRequest
    {
        // --- Operação / documento ---
        /// <summary>Regime tributário (CRT): 1=SimplesNacional, 2=SimplesNacionalExcessoSublimite, 3=RegimeNormal, 4=SimplesNacionalMei.</summary>
        public int RegimeTributario { get; set; } = 3;

        /// <summary>Modelo do documento: 55=NF-e, 65=NFC-e.</summary>
        public int ModeloDocumento { get; set; } = 55;

        /// <summary>CFOP da operação (ex.: 5102).</summary>
        public int Cfop { get; set; }

        /// <summary>UF de origem (emitente), ex.: "SP".</summary>
        public string UfOrigem { get; set; } = string.Empty;

        /// <summary>UF de destino (destinatário), ex.: "SP".</summary>
        public string UfDestino { get; set; } = string.Empty;

        // --- Produto ---
        public string? CodigoProduto { get; set; }
        public string? NomeProduto { get; set; }

        /// <summary>NCM (8 dígitos).</summary>
        public string Ncm { get; set; } = string.Empty;

        /// <summary>Origem da mercadoria (0 a 8).</summary>
        public string Origem { get; set; } = "0";

        public string? Unidade { get; set; }
        public decimal Quantidade { get; set; } = 1;
        public decimal ValorUnitario { get; set; }
        public decimal ValorDesconto { get; set; }

        // --- ICMS ---
        /// <summary>CST do ICMS (regime normal): "00", "10", "20"... Deixe vazio se Simples (usa Csosn).</summary>
        public string? CstIcms { get; set; }

        /// <summary>CSOSN (Simples Nacional): "101", "102"... Deixe vazio se regime normal (usa CstIcms).</summary>
        public string? Csosn { get; set; }

        public decimal AliquotaIcms { get; set; }
        public decimal ReducaoIcmsPercentual { get; set; }

        // --- ICMS ST ---
        public decimal AliquotaSt { get; set; }
        public decimal AliquotaMva { get; set; }
        public decimal ReducaoIcmsPercentualSt { get; set; }

        // --- FCP ---
        public decimal AliquotaFcp { get; set; }
        public decimal AliquotaFcpSt { get; set; }

        // --- DIFAL ---
        public decimal AliquotaIcmsInterna { get; set; }
        public decimal AliquotaIcmsInterestadual { get; set; }
        public bool DifalTipoCalculoPorDentro { get; set; }

        // --- IPI ---
        public string? CstIpi { get; set; }
        public decimal AliquotaIpi { get; set; }
        public string? EnquadramentoIpi { get; set; }
        public bool IpiEmbutido { get; set; }

        // --- PIS / COFINS ---
        public string? CstPisCofins { get; set; }
        public decimal AliquotaPis { get; set; }
        public decimal AliquotaPisReal { get; set; }
        public decimal AliquotaCofins { get; set; }
        public decimal AliquotaCofinsReal { get; set; }

        // --- IBS / CBS (Reforma Tributária) ---
        public string? CstIbsCbs { get; set; }
        public string? CClassTrib { get; set; }

        // --- IBPT (carga tributária aproximada) ---
        public decimal AliquotaIbptFederalNacional { get; set; }
        public decimal AliquotaIbptFederalImportado { get; set; }
        public decimal AliquotaIbptEstadual { get; set; }
        public decimal AliquotaIbptMunicipal { get; set; }
        public string? VersaoIbpt { get; set; }
    }

    /// <summary>Resultado do cálculo fiscal com os impostos apurados (base, alíquota e valor por tributo).</summary>
    public class CalculoFiscalResultado
    {
        public bool Sucesso { get; set; } = true;
        public List<string> Erros { get; set; } = new();

        public CalculoImpostoIcms Icms { get; set; } = new();
        public CalculoImpostoIpi Ipi { get; set; } = new();
        public CalculoImpostoPisCofins Pis { get; set; } = new();
        public CalculoImpostoPisCofins Cofins { get; set; } = new();
        public CalculoImpostoIbsCbs IbsCbs { get; set; } = new();
        public CalculoImpostoIbpt Ibpt { get; set; } = new();
    }

    public class CalculoImpostoIcms
    {
        public string? Cst { get; set; }
        public string? Csosn { get; set; }
        public string? Origem { get; set; }

        public decimal BaseCalculo { get; set; }
        public decimal Aliquota { get; set; }
        public decimal Valor { get; set; }

        // ST
        public decimal BaseCalculoSt { get; set; }
        public decimal AliquotaSt { get; set; }
        public decimal ValorSt { get; set; }

        // FCP
        public decimal BaseCalculoFcp { get; set; }
        public decimal AliquotaFcp { get; set; }
        public decimal ValorFcp { get; set; }
        public decimal ValorFcpSt { get; set; }

        // DIFAL
        public decimal BaseCalculoDifal { get; set; }
        public decimal ValorDifal { get; set; }

        public decimal ValorCredito { get; set; }
    }

    public class CalculoImpostoIpi
    {
        public string? Cst { get; set; }
        public decimal BaseCalculo { get; set; }
        public decimal Aliquota { get; set; }
        public decimal Valor { get; set; }
    }

    public class CalculoImpostoPisCofins
    {
        public string? Cst { get; set; }
        public decimal BaseCalculo { get; set; }
        public decimal Aliquota { get; set; }
        public decimal Valor { get; set; }
    }

    public class CalculoImpostoIbsCbs
    {
        public string? Cst { get; set; }
        public string? CClassTrib { get; set; }
        public decimal BaseCalculo { get; set; }

        public decimal AliquotaEstadual { get; set; }
        public decimal ValorEstadual { get; set; }

        public decimal AliquotaMunicipal { get; set; }
        public decimal ValorMunicipal { get; set; }

        public decimal AliquotaCbs { get; set; }
        public decimal ValorCbs { get; set; }
    }

    public class CalculoImpostoIbpt
    {
        public decimal AliquotaFederal { get; set; }
        public decimal AliquotaImportado { get; set; }
        public decimal AliquotaEstadual { get; set; }
        public decimal AliquotaMunicipal { get; set; }
        public decimal ValorAproximado { get; set; }
    }
}
