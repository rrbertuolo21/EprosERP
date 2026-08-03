using System;
using System.Linq;
using Epros.Shared.Application.Models;
using Flunt.Notifications;

namespace Epros.Shared.Domain.Entities
{
    public abstract class EntidadeSaaSBase : Notifiable<Notification>, ISyncable
    {
        public Guid Id { get; private set; }
        public Guid SyncId { get; private set; }
        public string TenantId { get; private set; } = string.Empty;
        public int SyncVersion { get; private set; }
        public DateTime CriadoEm { get; private set; }
        public DateTime? AlteradoEm { get; private set; }
        public DateTime? DeletadoEm { get; private set; }
        public string? CriadoPor { get; private set; }
        public string? AlteradoPor { get; private set; }

        protected EntidadeSaaSBase() { } // EF Core

        protected EntidadeSaaSBase(string tenantId, string criadoPor)
        {
            Id = Guid.NewGuid();
            SyncId = Guid.NewGuid();
            TenantId = tenantId;
            CriadoEm = DateTime.UtcNow;
            CriadoPor = criadoPor;
            SyncVersion = 0;
        }

        protected EntidadeSaaSBase(Guid id, Guid syncId, string tenantId, string criadoPor, DateTime criadoEm)
        {
            Id = id;
            SyncId = syncId;
            TenantId = tenantId;
            CriadoEm = criadoEm;
            CriadoPor = criadoPor;
            SyncVersion = 0;
        }

        public void MarcarAlterado(string alteradoPor)
        {
            AlteradoEm = DateTime.UtcNow;
            AlteradoPor = alteradoPor;
            SyncVersion++;
        }

        public void Deletar(string deletadoPor)
        {
            DeletadoEm = DateTime.UtcNow;
            AlteradoPor = deletadoPor;
        }

        // REG-016 — restauração GOVERNADA de soft-delete: limpa o carimbo de exclusão e registra a
        // auditoria de quem restaurou (mesma trilha de MarcarAlterado). Idempotente: se a entidade já
        // está ativa (DeletadoEm == null) é um no-op e retorna false; só restaura o que está deletado.
        // Retorna true quando efetivamente restaurou.
        public bool Restaurar(string restauradoPor)
        {
            if (DeletadoEm == null)
                return false;

            DeletadoEm = null;
            AlteradoEm = DateTime.UtcNow;
            AlteradoPor = restauradoPor;
            SyncVersion++;
            return true;
        }

        public bool EstaAtivo() => DeletadoEm == null;

        public new bool IsValid => !Notifications.Any();
    }
}
