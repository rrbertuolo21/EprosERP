using System;
using MediatR;

namespace Epros.Shared.Domain.Events
{
    public record UsuarioAtualizadoEventNotification(
        Guid UsuarioId,
        string Nome,
        string Email,
        string TenantId,
        Guid? PerfilUsuarioId,
        string Cargo,
        string Departamento,
        decimal LimiteDesconto,
        string AlteradoPor
    ) : INotification;
}
