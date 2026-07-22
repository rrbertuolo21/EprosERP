using System;
using MediatR;

namespace Epros.Shared.Domain.Events
{
    /// <summary>
    /// Evento publicado após commit da anonimização de uma Pessoa (CAD-PEM REG-PEM-160/161: pessoa.anonimizada).
    /// Respeita retenção legal fiscal/trabalhista/financeira (REG-PEM-154).
    /// </summary>
    public record PessoaAnonimizadaEventNotification(
        Guid PessoaId,
        string TenantId,
        DateTime AnonimizadoEm,
        string UserId
    ) : INotification;
}
