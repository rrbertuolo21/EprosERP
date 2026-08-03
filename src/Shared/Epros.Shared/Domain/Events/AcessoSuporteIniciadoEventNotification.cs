using System;
using MediatR;

namespace Epros.Shared.Domain.Events
{
    /// <summary>
    /// Notificação de auditoria de segurança para o acesso de suporte da Siser a um cliente
    /// (formalização da impersonação como "acesso por perfil de suporte"). Emitida via outbox,
    /// na mesma trilha da impersonação clássica (REG-033).
    /// </summary>
    public record AcessoSuporteIniciadoEventNotification(
        Guid SessaoImpersonacaoId,
        Guid UsuarioOriginalId,
        Guid UsuarioAlvoId,
        Guid? EmpresaId,
        string Motivo,
        string CriadoPor,
        string TenantAlvo,
        string PerfilSuporte
    ) : INotification;
}
