namespace Epros.Modules.DMS.Domain.Financiamento
{
    /// <summary>
    /// Uma linha da tabela de amortização (memória de cálculo).
    /// Fonte: skill Negocio-acumulado/financeiro/credito — "tabela de parcelas
    /// [k, prestação, juros, amortização, saldo devedor]".
    /// </summary>
    public sealed record ParcelaFinanciamento(
        int Numero,
        decimal Prestacao,
        decimal Juros,
        decimal Amortizacao,
        decimal SaldoDevedor);
}
