using System;

namespace Epros.Shared.Domain.Events
{
    public class OutboxMessage
    {
        public Guid Id { get; private set; }
        public string TenantId { get; private set; } = string.Empty;
        public string EventType { get; private set; } = string.Empty;
        public string Payload { get; private set; } = string.Empty;
        public DateTime CriadoEm { get; private set; }
        public DateTime? ProcessadoEm { get; private set; }
        public string? Erro { get; private set; }
        public int Tentativas { get; private set; }

        protected OutboxMessage() { } // EF Core

        public OutboxMessage(string tenantId, string eventType, string payload)
        {
            Id = Guid.NewGuid();
            TenantId = tenantId;
            EventType = eventType;
            Payload = payload;
            CriadoEm = DateTime.UtcNow;
            Tentativas = 0;
        }

        public void MarcarProcessado()
        {
            ProcessadoEm = DateTime.UtcNow;
        }

        public void RegistrarFalha(string erro)
        {
            Tentativas++;
            Erro = erro;
        }
    }
}
