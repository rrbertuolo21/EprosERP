using System;
using MediatR;

namespace Epros.Shared.Domain.Events
{
    /// <summary>
    /// Evento publicado quando qualquer módulo submete um registro/comando para aprovação por alçada,
    /// através do motor de Workflow genérico (PLT-WF). Contrato reutilizável: o módulo dono informa
    /// qual entidade de negócio está sob controle e o valor de referência para a alçada, quando houver.
    /// </summary>
    public record AprovacaoSolicitadaEventNotification(
        Guid InstanciaId,
        Guid? DefinicaoId,
        string Modulo,
        string EntidadeTipo,
        string EntidadeId,
        string Descricao,
        decimal? ValorReferencia,
        string? CommandType,
        string? Payload,
        string SolicitadoPor,
        string TenantId,
        string UserId
    ) : INotification;
}
