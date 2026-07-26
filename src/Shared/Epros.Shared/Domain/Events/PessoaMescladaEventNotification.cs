using System;
using MediatR;

namespace Epros.Shared.Domain.Events
{
    /// <summary>
    /// Evento publicado após commit de um merge de duplicatas (CAD-PEM REG-PEM-160/161: pessoa.mesclada).
    /// PessoaSobreviventeId consolida o histórico; PessoaMescladaId fica marcada como mesclada.
    /// </summary>
    public record PessoaMescladaEventNotification(
        Guid PessoaSobreviventeId,
        Guid PessoaMescladaId,
        string TenantId,
        DateTime MescladoEm,
        string UserId
    ) : INotification;
}
