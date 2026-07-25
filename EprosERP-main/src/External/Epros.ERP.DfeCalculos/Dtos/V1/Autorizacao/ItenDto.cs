using System.ComponentModel.DataAnnotations;

namespace Epros.ERP.DfeCalculos.Dtos.V1.Autorizacao
{
    public class ItenDto
    {
        public ItenDto() { }
        public ItenDto(string? codigoProduto, string? nomeProduto, string? codigoBarras, string? ncm, int cfop, string? unidade, decimal quantidade, decimal valorUnitario, decimal valorDesconto, string? origem, string? cstCsosn, decimal valorAliquotaIcms, decimal valorReducaoIcmsPercentual, int tipoReducaoIcms, decimal valorBaseCalculoStRetidoOperacaoAnterior, decimal valorAlioquotaSt, decimal valorIcmsStRetidoOperacaoAnterior, decimal valorIcmsProprioSubstituto, string cstPisCofins, decimal valorAliquotaPis, decimal valorAliquotaPisReal, decimal valorAliquotaCofins, decimal valorAliquotaCofinsReal, string? cest, string? cstIbsCbs, string? cClassTrib, decimal qtdeBCMonoRetido, decimal valorAliquotaAdRemRetido, string? informacoesAdProduto, string? unidadeTributavel, decimal aliquotaDifalInterna, decimal aliquotaDifalInterestadual, string? codigoBeneficioFiscal, decimal valorCusto, VendaItemCombustivelDto? combustivel)
        {
            CodigoProduto = codigoProduto;
            NomeProduto = nomeProduto;
            CodigoBarras = codigoBarras;
            Ncm = ncm;
            Cfop = cfop;
            Unidade = unidade;
            Quantidade = quantidade;
            ValorUnitario = valorUnitario;
            ValorDesconto = valorDesconto;
            Origem = origem;
            CstCsosn = cstCsosn;
            ValorAliquotaIcms = valorAliquotaIcms;
            ValorReducaoIcmsPercentual = valorReducaoIcmsPercentual;
            TipoReducaoIcms = tipoReducaoIcms;
            ValorBaseCalculoStRetidoOperacaoAnterior = valorBaseCalculoStRetidoOperacaoAnterior;
            ValorAlioquotaSt = valorAlioquotaSt;
            ValorIcmsStRetidoOperacaoAnterior = valorIcmsStRetidoOperacaoAnterior;
            ValorIcmsProprioSubstituto = valorIcmsProprioSubstituto;
            CstPisCofins = cstPisCofins;
            ValorAliquotaPis = valorAliquotaPis;
            ValorAliquotaPisReal = valorAliquotaPisReal;
            ValorAliquotaCofins = valorAliquotaCofins;
            ValorAliquotaCofinsReal = valorAliquotaCofinsReal;
            Cest = cest;
            CstIbsCbs = cstIbsCbs;
            CClassTrib = cClassTrib;
            QtdeBCMonoRetido = qtdeBCMonoRetido;
            ValorAliquotaAdRemRetido = valorAliquotaAdRemRetido;
            InformacoesAdProduto = informacoesAdProduto;
            UnidadeTributavel = unidadeTributavel;
            AliquotaDifalInterna = aliquotaDifalInterna;
            AliquotaDifalInterestadual = aliquotaDifalInterestadual;
            CodigoBeneficioFiscal = codigoBeneficioFiscal;
            ValorCusto = valorCusto;
            Combustivel = combustivel;
        }

        public long ProdutoId { get; set; }////

        [Required(ErrorMessage = "{0}, obrigatório")]
        public string? CodigoProduto { get; set; }

        [Required(ErrorMessage = "{0}, obrigatório")]
        public string? NomeProduto { get; set; }

        [Required(ErrorMessage = "{0}, obrigatório")]
        public string? CodigoBarras { get; set; }

        [Required(ErrorMessage = "{0}, obrigatório")]
        public string? Ncm { get; set; }

        [Required(ErrorMessage = "{0}, obrigatório")]
        public int Cfop { get; set; }

        [Required(ErrorMessage = "{0}, obrigatório")]
        [MinLength(1, ErrorMessage = "{0}, deve conter entre 1 e 6 caractes")]
        [MaxLength(6, ErrorMessage = "{0}, deve conter entre 1 e 6 caractes")]
        public string? Unidade { get; set; }

        [MinLength(1, ErrorMessage = "{0}, deve conter entre 1 e 6 caractes")]
        [MaxLength(6, ErrorMessage = "{0}, deve conter entre 1 e 6 caractes")]
        public string? UnidadeTributavel { get; set; }

        [Required(ErrorMessage = "{0}, obrigatório")]
        [Range(0.0001, double.MaxValue, ErrorMessage = "{0}, deve ser maior que zero")]
        public decimal Quantidade { get; set; }

        [Required(ErrorMessage = "{0}, obrigatório")]
        [Range(0.0000000000, double.MaxValue, ErrorMessage = "{0}, deve ser maior ou igual zero")]
        public decimal ValorUnitario { get; set; }

        [Range(0.00, double.MaxValue, ErrorMessage = "{0}, deve ser maior ou igual zero")]
        public decimal ValorDesconto { get; set; }

        [Required(ErrorMessage = "{0}, obrigatório")]  // ****
        public string? Origem { get; set; }

        [Required(ErrorMessage = "{0}, obrigatório")]
        public string? CstCsosn { get; set; }

        [Range(0.00, double.MaxValue, ErrorMessage = "{0}, deve ser maior ou igual zero")]
        public decimal ValorAliquotaIcms { get; set; }

        [Range(0.00, double.MaxValue, ErrorMessage = "{0}, deve ser maior ou igual zero")]
        public decimal ValorReducaoIcmsPercentual { get; set; }
        public int TipoReducaoIcms { get; set; }

        [Range(0.00, double.MaxValue, ErrorMessage = "{0}, deve ser maior ou igual zero")]
        public decimal ValorBaseCalculoStRetidoOperacaoAnterior { get; set; }

        [Range(0.00, double.MaxValue, ErrorMessage = "{0}, deve ser maior ou igual zero")]
        public decimal ValorAlioquotaSt { get; set; }

        [Range(0.00, double.MaxValue, ErrorMessage = "{0}, deve ser maior ou igual zero")]
        public decimal ValorIcmsStRetidoOperacaoAnterior { get; set; }

        [Range(0.00, double.MaxValue, ErrorMessage = "{0}, deve ser maior ou igual zero")]
        public decimal ValorIcmsProprioSubstituto { get; set; }

        [Required(ErrorMessage = "{0}, obrigatório")]
        public string CstPisCofins { get; set; } = null!;

        [Range(0.00, double.MaxValue, ErrorMessage = "{0}, deve ser maior ou igual zero")]
        public decimal ValorAliquotaPis { get; set; }

        [Range(0.00, double.MaxValue, ErrorMessage = "{0}, deve ser maior ou igual zero")]
        public decimal ValorAliquotaPisReal { get; set; }

        [Range(0.00, double.MaxValue, ErrorMessage = "{0}, deve ser maior ou igual zero")]
        public decimal ValorAliquotaCofins { get; set; }

        [Range(0.00, double.MaxValue, ErrorMessage = "{0}, deve ser maior ou igual zero")]
        public decimal ValorAliquotaCofinsReal { get; set; }

        //[Required(ErrorMessage = "{0}, obrigatório")]
        //public int ValorDoItemCompoeTotalNF { get; set; }

        public string? EnquadramentoIpi { get; set; }
        public string? CstIpi { get; set; }

        [Range(0.00, double.MaxValue, ErrorMessage = "{0}, deve ser maior ou igual zero")]
        public decimal ValorAliquotaIpi { get; set; }

        [Range(0.00, double.MaxValue, ErrorMessage = "{0}, deve ser maior ou igual zero")]
        public decimal ValorReducaoIpiPercentual { get; set; }
        public int TipoReducaoIpi { get; set; }

        public string? Cest { get; set; }
        public string? CstIbsCbs { get; set; }
        public string? CClassTrib { get; set; }
        public decimal QtdeBCMonoRetido { get; set; }
        public decimal ValorAliquotaAdRemRetido { get; set; }

        public string? InformacoesAdProduto { get; set; }

        public decimal AliquotaDifalInterna { get; set; }
        public decimal AliquotaDifalInterestadual { get; set; }

        public string? CodigoBeneficioFiscal { get; set; }

        public decimal ValorCusto { get; set; }

        public VendaItemCombustivelDto? Combustivel { get; set; }
    }
}