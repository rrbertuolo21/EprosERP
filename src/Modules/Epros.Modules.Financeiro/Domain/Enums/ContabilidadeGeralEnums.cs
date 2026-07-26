using System.ComponentModel;

namespace Epros.Modules.Financeiro.Domain.Enums
{
    /// <summary>Tipo contábil da conta (EF FIN-CGL §5.8 / §7). Saneamento funcional dos tipos internacionais.</summary>
    public enum ETipoContaContabil
    {
        [Description("Ativo")] Ativo = 0,
        [Description("Passivo")] Passivo = 1,
        [Description("Patrimônio")] Patrimonio = 2,
        [Description("Receita")] Receita = 3,
        [Description("Despesa")] Despesa = 4
    }

    /// <summary>Tipo de saldo/movimento (débito ou crédito) — EF FIN-CGL §5.9/§7.</summary>
    public enum ETipoSaldoContabil
    {
        [Description("Débito")] Debito = 0,
        [Description("Crédito")] Credito = 1
    }

    /// <summary>Estado do período contábil — EF FIN-CGL §9.3.</summary>
    public enum EEstadoPeriodoContabil
    {
        [Description("Aberto")] Aberto = 0,
        [Description("Em Fechamento")] EmFechamento = 1,
        [Description("Fechado")] Fechado = 2,
        [Description("Reaberto")] Reaberto = 3
    }

    /// <summary>Estado do lançamento contábil — EF FIN-CGL §9.4.</summary>
    public enum EEstadoLancamentoContabil
    {
        [Description("Rascunho")] Rascunho = 0,
        [Description("Confirmado")] Confirmado = 1,
        [Description("Estornado")] Estornado = 2,
        [Description("Cancelado")] Cancelado = 3
    }
}
