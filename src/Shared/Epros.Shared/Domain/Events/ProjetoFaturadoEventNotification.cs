using System;
using MediatR;

namespace Epros.Shared.Domain.Events
{
    /// <summary>
    /// Evento publicado quando um projeto alcança um marco faturável.
    /// Consumido pelo módulo Financeiro para gerar a correspondente Conta a Receber.
    /// </summary>
    public record ProjetoFaturadoEventNotification(
        Guid ProjetoId,
        string NomeProjeto,
        Guid ClienteId,
        decimal Milestone,
        decimal ValorFaturamento,
        string TenantId
    ) : INotification;
}
