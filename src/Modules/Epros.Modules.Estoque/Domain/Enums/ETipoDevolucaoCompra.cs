namespace Epros.Modules.Estoque.Domain.Enums
{
    /// <summary>
    /// Tipo da devolução de compra (CD4 / EF DEVOLUCAO_DE_COMPRA §6.1). Total = devolve integralmente a
    /// compra de origem; Parcial = devolve parte dos itens/quantidades. Informativo — a validação de
    /// quantidade (DEV-002) é feita item a item contra a compra de origem, independentemente do tipo.
    /// </summary>
    public enum ETipoDevolucaoCompra
    {
        Total = 0,
        Parcial = 1
    }
}
