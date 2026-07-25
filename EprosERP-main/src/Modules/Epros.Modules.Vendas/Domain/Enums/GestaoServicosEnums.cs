using System.ComponentModel;

namespace Epros.Modules.Vendas.Domain.Enums
{
    // ============================================================================
    // Enums do submódulo Gestão de Serviços (VEN-GSV).
    // Fonte funcional: EF_7_VENDAS_GESTAO_DE_SERVICOS_V1 (seções 7, 11, 12, 19).
    // Enums locais do módulo (mesmo padrão de VendasEnums.cs). Tipados — nunca string solta.
    // ============================================================================

    /// <summary>Estado funcional da fatura de serviço. EF §7.1.</summary>
    public enum EServicoFaturaStatus
    {
        [Description("Rascunho")]
        Rascunho = 0,
        [Description("Confirmado")]
        Confirmado = 1,
        [Description("Faturado")]
        Faturado = 2,
        [Description("Cancelado")]
        Cancelado = 3,
        [Description("Estornado")]
        Estornado = 4
    }

    /// <summary>Tipo de imposto sobre valor agregado configurado para a empresa. EF §11.5–11.8.</summary>
    public enum ETipoImpostoServico
    {
        [Description("Exclusivo")]
        Exclusivo = 0,
        [Description("Inclusivo")]
        Inclusivo = 1
    }

    /// <summary>Natureza do lançamento financeiro esperado gerado pela fatura. EF §12 e §19.4.</summary>
    public enum ETipoLancamentoServicoFinanceiro
    {
        [Description("Cliente débito")]
        ClienteDebito = 0,
        [Description("Receita crédito")]
        ReceitaCredito = 1,
        [Description("Conta pagamento débito")]
        ContaPagamentoDebito = 2,
        [Description("Cliente crédito")]
        ClienteCredito = 3
    }

    /// <summary>Estado da integração financeira da referência de lançamento. EF §19.4.</summary>
    public enum EStatusIntegracaoServico
    {
        [Description("Pendente")]
        Pendente = 0,
        [Description("Integrado")]
        Integrado = 1,
        [Description("Estornado")]
        Estornado = 2,
        [Description("Erro")]
        Erro = 3
    }

    /// <summary>Entidade alterada registrada no histórico de serviço. EF §19.5.</summary>
    public enum EEntidadeHistoricoServico
    {
        [Description("Catálogo")]
        Catalogo = 0,
        [Description("Fatura")]
        Fatura = 1,
        [Description("Linha")]
        Linha = 2
    }
}
