using System;

namespace Epros.Modules.Estoque.Infrastructure.Data
{
    /// <summary>
    /// Lookup (somente leitura) da entidade Servico, cujo dono é o módulo Fiscal (schema plataforma).
    /// Usado pela tela de compra (compras-dados/obter-servicos-por-ids). Referência cross-module por
    /// Guid/tabela, sem navegação de projeto entre módulos (convenção seção 6.4).
    /// </summary>
    public class ServicoLookup
    {
        public Guid Id { get; set; }
        public Guid UnidadeMedidaId { get; set; }
        public Guid CodigoServicoSefazId { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public string Descricao { get; set; } = string.Empty;
        public decimal Valor { get; set; }
        public string? InformacaoAdicional { get; set; }
        public bool ServicoAtivo { get; set; }
        public int Cnae { get; set; }
        public string CodigoNbs { get; set; } = string.Empty;
        public bool IndicadorIss { get; set; }
        public bool IndicadorIncentivo { get; set; }
        public string? CstIbsCbs { get; set; }
        public string? CClassTrib { get; set; }
        public decimal AliquotaIss { get; set; }
        public decimal AliquotaIssRetido { get; set; }
        public decimal AliquotaIrrfRetido { get; set; }
        public decimal AliquotaInss { get; set; }
        public decimal AliquotaPis { get; set; }
        public decimal AliquotaCofins { get; set; }
        public bool CalcularRetencao { get; set; }

        public string TenantId { get; set; } = string.Empty;
        public DateTime? DeletadoEm { get; set; }
    }
}
