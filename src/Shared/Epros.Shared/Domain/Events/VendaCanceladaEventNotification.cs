using System;
using System.Collections.Generic;
using MediatR;

namespace Epros.Shared.Domain.Events
{
    public record VendaCanceladaEventNotification(
        Guid VendaId,
        string TenantId,
        decimal Total,
        DateTime CanceladoEm,
        List<VendaCanceladaItemNotification> Itens,
        string UserId
    ) : INotification;

    public record VendaCanceladaItemNotification(
        Guid ProdutoId,
        decimal Quantidade
    );
}
