namespace Epros.Modules.Estoque.Domain.Enums
{
    /// <summary>
    /// Método de rateio (chave de apropriação) do custo landed sobre os itens da importação
    /// (CD1 / EF COMERCIO_EXTERIOR §6.2, NF-02). É a **chave estrutural** de distribuição — PorValor
    /// (proporcional ao valor do item), PorPeso ou PorQuantidade. NÃO define a BASE de cálculo dos
    /// tributos (isso é valida-contador): apenas como um montante já apurado é distribuído entre itens.
    /// </summary>
    public enum ERateioLandedMetodo
    {
        PorValor = 0,
        PorPeso = 1,
        PorQuantidade = 2
    }
}
