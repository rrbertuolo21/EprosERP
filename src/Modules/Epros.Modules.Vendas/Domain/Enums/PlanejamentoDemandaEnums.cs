using System.ComponentModel;

namespace Epros.Modules.Vendas.Domain.Enums
{
    // ============================================================================
    // Enums do submódulo Planejamento de Demanda (VEN-PDM).
    // Fonte funcional: EF_7_VENDAS_PLANEJAMENTO_DE_DEMANDA_V1 (§8, §14). Enums locais do módulo.
    // ============================================================================

    /// <summary>Ciclo de vida da previsão de demanda. EF §8 / §14.1.</summary>
    public enum EDemandaStatus
    {
        [Description("Rascunho")]
        Rascunho = 0,
        [Description("Em revisão")]
        EmRevisao = 1,
        [Description("Aprovado")]
        Aprovado = 2,
        [Description("Substituído")]
        Substituido = 3,
        [Description("Cancelado")]
        Cancelado = 4
    }

    /// <summary>Estado da versão da previsão. EF §14.3 (status).</summary>
    public enum EDemandaVersaoStatus
    {
        [Description("Vigente")]
        Vigente = 0,
        [Description("Substituída")]
        Substituida = 1,
        [Description("Cancelada")]
        Cancelada = 2
    }

    /// <summary>Destino/origem da integração de demanda. EF §14.6 (destino).</summary>
    public enum EDemandaIntegracaoDestino
    {
        [Description("Histórico de vendas")]
        HistoricoVendas = 0,
        [Description("Estoque")]
        Estoque = 1,
        [Description("Produção")]
        Producao = 2,
        [Description("Cenários")]
        Cenarios = 3
    }

    /// <summary>Direção da integração. EF §14.6 (direcao).</summary>
    public enum EDemandaIntegracaoDirecao
    {
        [Description("Entrada")]
        Entrada = 0,
        [Description("Saída")]
        Saida = 1
    }

    /// <summary>Estado do processamento da integração. EF §14.6 (status).</summary>
    public enum EDemandaIntegracaoStatus
    {
        [Description("Pendente")]
        Pendente = 0,
        [Description("Processado")]
        Processado = 1,
        [Description("Erro")]
        Erro = 2
    }
}
