using System;
using MediatR;

namespace Epros.Shared.Domain.Events
{
    /// <summary>
    /// Evento publicado quando um lançamento de compra é cancelado.
    /// Consumido pelo Financeiro para estornar/cancelar a contas a pagar correspondente.
    /// </summary>
    public record CompraCanceladaEventNotification(
        Guid CompraId,
        Guid FornecedorId,
        decimal ValorTotal,
        string NumeroNota,
        string TenantId,
        string UserId
    ) : INotification;
}
