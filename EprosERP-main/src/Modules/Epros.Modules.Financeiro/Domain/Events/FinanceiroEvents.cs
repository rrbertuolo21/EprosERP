namespace Epros.Modules.Financeiro.Domain.Events
{
    /// <summary>
    /// Publicado quando uma compra é lançada pelo módulo Estoque.
    /// O Financeiro escuta esse evento para criar automaticamente um título a pagar (ContasAPagar fiel).
    /// </summary>
    public record CompraLancadaEvent(
        Guid CompraId,
        Guid FornecedorId,
        decimal ValorTotal,
        DateTime DataVencimento,
        string NumeroNota,
        string TenantId,
        string UserId
    );

    /// <summary>
    /// Publicado quando uma venda é faturada pelo módulo Vendas.
    /// O Financeiro escuta esse evento para criar automaticamente um título a receber (ContasAReceber fiel).
    /// </summary>
    public record VendaFaturadaEvent(
        Guid VendaId,
        Guid ClienteId,
        decimal ValorTotal,
        DateTime DataVencimento,
        string NumeroVenda,
        string TenantId,
        string UserId
    );
}
