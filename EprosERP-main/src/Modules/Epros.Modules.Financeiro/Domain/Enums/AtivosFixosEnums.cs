using System.ComponentModel;

namespace Epros.Modules.Financeiro.Domain.Enums
{
    /// <summary>Estado funcional do ativo fixo (EF FIN-AFX §10.3 / §9).</summary>
    public enum EStatusAtivoFixo
    {
        [Description("Ativo")] Ativo = 0,
        [Description("Baixado")] Baixado = 1,
        [Description("Totalmente Depreciado")] TotalmenteDepreciado = 2,
        [Description("Excluído")] Excluido = 3
    }

    /// <summary>Tipo de movimentação patrimonial do ativo (EF FIN-AFX §10.5 afx_movimentacao).</summary>
    public enum ETipoMovimentacaoAtivo
    {
        [Description("Baixa")] Baixa = 0,
        [Description("Alienação")] Alienacao = 1,
        [Description("Depreciação")] Depreciacao = 2,
        [Description("Vistoria")] Vistoria = 3
    }

    /// <summary>Tipo de cálculo de depreciação (EF FIN-AFX §7.3).</summary>
    public enum ETipoDepreciacaoAtivo
    {
        [Description("Linear")] Linear = 0,
        [Description("Acelerada")] Acelerada = 1
    }
}
