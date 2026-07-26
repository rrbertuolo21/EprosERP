using System.ComponentModel;

namespace Epros.Modules.Financeiro.Domain.Enums
{
    /// <summary>Tipo de movimento na transação de conta financeira (EF FIN-TS §11.6 — credit/debit).</summary>
    public enum ETipoTransacaoConta
    {
        [Description("Crédito")] Credito = 0,
        [Description("Débito")] Debito = 1
    }

    /// <summary>Situação do cheque (EF FIN-TS §11.7 — domínio 1..5).</summary>
    public enum ESituacaoCheque
    {
        [Description("Emitido")] Emitido = 0,
        [Description("Compensado")] Compensado = 1,
        [Description("Devolvido")] Devolvido = 2,
        [Description("Cancelado")] Cancelado = 3,
        [Description("Repassado")] Repassado = 4
    }

    /// <summary>Status do caixa operacional (EF FIN-TS §11.10 — open/close).</summary>
    public enum EStatusCaixaOperacional
    {
        [Description("Aberto")] Aberto = 0,
        [Description("Fechado")] Fechado = 1
    }
}
