using System;
using MediatR;

namespace Epros.Shared.Domain.Events
{
    public record UsuarioDeletadoEventNotification(
        Guid UsuarioId,
        string AlteradoPor
    ) : INotification;
}
