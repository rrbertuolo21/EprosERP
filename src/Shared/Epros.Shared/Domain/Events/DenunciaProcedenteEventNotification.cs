using System;
using MediatR;

namespace Epros.Shared.Domain.Events
{
    /// <summary>
    /// Evento publicado quando uma Denúncia anônima é julgada como Procedente.
    /// Consumido pelo próprio módulo de GRC para registrar automaticamente um incidente crítico de compliance.
    /// </summary>
    public record DenunciaProcedenteEventNotification(
        Guid DenunciaId,
        string Relato,
        string ParecerFinal,
        string TenantId
    ) : INotification;
}
