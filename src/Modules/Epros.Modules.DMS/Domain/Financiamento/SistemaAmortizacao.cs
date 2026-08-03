namespace Epros.Modules.DMS.Domain.Financiamento
{
    /// <summary>
    /// Sistema de amortização de um financiamento.
    /// Fonte: skill Negocio-acumulado/financeiro/credito (Receitas A e B).
    /// - PRICE (francês): prestação constante; paga MAIS juros no total.
    /// - SAC: amortização constante; prestação decrescente; paga MENOS juros no total.
    /// </summary>
    public enum SistemaAmortizacao
    {
        Price = 0,
        Sac = 1
    }
}
