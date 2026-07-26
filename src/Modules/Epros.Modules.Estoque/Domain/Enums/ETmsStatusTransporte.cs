namespace Epros.Modules.Estoque.Domain.Enums
{
    /// <summary>
    /// Estados funcionais do transporte da compra — EF Transporte e Frete TMS §8/§9.1.
    /// Rascunho → Confirmado → Faturado; Cancelado/Estornado por reflexo do documento principal.
    /// </summary>
    public enum ETmsStatusTransporte
    {
        Rascunho = 0,
        Confirmado = 1,
        Faturado = 2,
        Cancelado = 3,
        Estornado = 4
    }
}
