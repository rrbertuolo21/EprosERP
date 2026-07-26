using System;
using MediatR;

namespace Epros.Shared.Domain.Events
{
    /// <summary>
    /// Evento publicado quando uma nota de compra de mercadorias é registrada.
    /// Consumido pelo Financeiro para geração automática de contas a pagar.
    /// </summary>
    public record CompraLancadaEventNotification(
        Guid CompraId,
        Guid FornecedorId,
        decimal ValorTotal,
        DateTime DataVencimento,
        string NumeroNota,
        string TenantId,
        string UserId,
        System.Collections.Generic.List<CompraLancadaItemNotification> Itens
    ) : INotification;

    public record CompraLancadaItemNotification(
        string Sku,
        string NomeProduto,
        decimal Quantidade,
        decimal PrecoUnitario
    );
}
