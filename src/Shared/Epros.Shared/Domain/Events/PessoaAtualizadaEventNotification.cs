using System;
using MediatR;

namespace Epros.Shared.Domain.Events
{
    /// <summary>
    /// Evento publicado após commit da atualização de uma Pessoa (CAD-PEM REG-PEM-160/161: pessoa.atualizada).
    /// </summary>
    public record PessoaAtualizadaEventNotification(
        Guid PessoaId,
        string TenantId,
        int TipoPessoa,
        DateTime AtualizadoEm,
        string UserId
    ) : INotification;
}
