using System;
using MediatR;

namespace Epros.Shared.Domain.Events
{
    public record ImpersonacaoIniciadaEventNotification(
        Guid SessaoImpersonacaoId,
        Guid UsuarioOriginalId,
        Guid UsuarioAlvoId,
        Guid? EmpresaId,
        string Motivo,
        string CriadoPor,
        string TenantId
    ) : INotification;
}
