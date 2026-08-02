namespace Epros.Modules.Estoque.Domain.Enums
{
    /// <summary>
    /// Origem sobre a qual incide a aprovação por alçada (CD3 / EF SOURCING §5.8, §6.2).
    /// A alçada de compras é multi-nível e pode incidir sobre o pedido de compra, a compra
    /// (Gestão de Compras) ou o contrato de compra (GCC).
    /// </summary>
    public enum EOrigemAprovacaoCompra
    {
        PedidoCompra = 1,
        Compra = 2,
        ContratoCompra = 3
    }
}
