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

    /// <summary>
    /// Método de cálculo de depreciação (EF FIN-AFX §7.3). Fórmulas UNIVERSAIS de contabilidade
    /// (cita Negocio-acumulado/contabil — imobilizado/depreciação); a taxa/vida útil de referência
    /// (RFB IN 1.700/2017 Anexo III) é fato legal informado no ativo → // valida-contador.
    /// </summary>
    public enum ETipoDepreciacaoAtivo
    {
        [Description("Linear (cotas constantes)")] Linear = 0,
        [Description("Acelerada")] Acelerada = 1,
        [Description("Saldos decrescentes")] SaldoDecrescente = 2,
        [Description("Soma dos dígitos (SYD)")] SomaDosDigitos = 3
    }
}
