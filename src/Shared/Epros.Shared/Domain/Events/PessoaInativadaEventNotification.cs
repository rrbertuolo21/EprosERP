using System;
using MediatR;

namespace Epros.Shared.Domain.Events
{
    /// <summary>
    /// Evento publicado após commit da inativação de uma Pessoa (CAD-PEM REG-PEM-160/161: pessoa.inativada).
    /// </summary>
    public record PessoaInativadaEventNotification(
        Guid PessoaId,
        string TenantId,
        string? Motivo,
        DateTime InativadoEm,
        string UserId
    ) : INotification;
}
