using System;
using MediatR;

namespace Epros.Shared.Domain.Events
{
    /// <summary>
    /// Evento publicado quando uma solicitação de aprovação por alçada é concluída no motor de
    /// Workflow genérico (PLT-WF), seja por aprovação ou rejeição. Consumido pelo módulo dono
    /// para aplicar/retornar o efeito do comando retido.
    /// </summary>
    public record AprovacaoConcluidaEventNotification(
        Guid InstanciaId,
        Guid? DefinicaoId,
        string Modulo,
        string EntidadeTipo,
        string EntidadeId,
        bool Aprovado,
        string? Comentario,
        string? CommandType,
        string? Payload,
        string AprovadoPor,
        string TenantId,
        string UserId
    ) : INotification;
}
