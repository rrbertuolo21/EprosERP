using System;

namespace Epros.Shared.Domain.Events
{
    public abstract class DomainEvent
    {
        public Guid EventId { get; } = Guid.NewGuid();
        public DateTime OcorridoEm { get; } = DateTime.UtcNow;
        public string TenantId { get; protected set; } = string.Empty;
        public abstract string EventType { get; }
    }
}
