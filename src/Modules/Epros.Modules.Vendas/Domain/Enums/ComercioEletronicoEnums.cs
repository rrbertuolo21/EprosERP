using System.ComponentModel;

namespace Epros.Modules.Vendas.Domain.Enums
{
    // ============================================================================
    // Enums do submódulo Comércio Eletrônico (VEN-ECM).
    // Fonte funcional: EF_7_VENDAS_COMERCIO_ELETRONICO_V1 (§7, §8). Enums locais do módulo.
    // Códigos fiéis ao material (ECO-016/ECO-026): status de pagamento 0..2, preparação 0..5.
    // ============================================================================

    /// <summary>Status de pagamento do pedido e-commerce. EF §8.4 (status_pagamento_codigo). ECO-016/ECO-024.</summary>
    public enum EEcoStatusPagamento
    {
        [Description("Criado sem pagamento")]
        CriadoSemPagamento = 0,
        [Description("Pagamento iniciado")]
        PagamentoIniciado = 1,
        [Description("Pagamento confirmado")]
        PagamentoConfirmado = 2
    }

    /// <summary>Status de preparação/logística do pedido. EF §8.4 (status_preparacao_codigo). ECO-026.</summary>
    public enum EEcoStatusPreparacao
    {
        [Description("Novo")]
        Novo = 0,
        [Description("Aprovado")]
        Aprovado = 1,
        [Description("Cancelado")]
        Cancelado = 2,
        [Description("Aguardando envio")]
        AguardandoEnvio = 3,
        [Description("Enviado")]
        Enviado = 4,
        [Description("Entregue")]
        Entregue = 5
    }

    /// <summary>Forma de pagamento do pedido. EF §8.4 (forma_pagamento). ECO-021..ECO-023/ECO-036.</summary>
    public enum EEcoFormaPagamento
    {
        [Description("Cartão")]
        Cartao = 0,
        [Description("Pix")]
        Pix = 1,
        [Description("Boleto")]
        Boleto = 2
    }

    /// <summary>Tipo de desconto do cupom. EF §8.7 (tipo). ECO-030.</summary>
    public enum EEcoTipoCupom
    {
        [Description("Percentual")]
        Percentual = 0,
        [Description("Fixo")]
        Fixo = 1
    }
}
