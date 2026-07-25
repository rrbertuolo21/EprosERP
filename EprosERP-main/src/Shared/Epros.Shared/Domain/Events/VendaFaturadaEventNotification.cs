using System;
using System.Collections.Generic;
using MediatR;

namespace Epros.Shared.Domain.Events
{
    public record VendaFaturadaEventNotification(
        Guid VendaId,
        string TenantId,
        decimal Total,
        DateTime CriadoEm,
        List<VendaFaturadaItemNotification> Itens,
        string UserId
    ) : INotification;

    public record VendaFaturadaItemNotification(
        Guid ProdutoId,
        decimal Quantidade,
        decimal PrecoUnitario
    );
}
