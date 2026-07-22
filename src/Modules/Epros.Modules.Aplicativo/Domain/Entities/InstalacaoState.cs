using System;
using Epros.Shared.Domain.Entities;

namespace Epros.Modules.Aplicativo.Domain.Entities
{
    public class InstalacaoState : EntidadeSaaSBase
    {
        public bool IsCompleted { get; private set; }
        public DateTime? CompletedAt { get; private set; }
        public string? CompletedBy { get; private set; }
        public bool DatabaseInitialized { get; private set; }
        public bool AdminCreated { get; private set; }
        public bool SystemSettingsSeeded { get; private set; }
        
        protected InstalacaoState() { } // EF Core
        
        public InstalacaoState(string tenantId, string criadoPor) : base(tenantId, criadoPor)
        {
            IsCompleted = false;
            DatabaseInitialized = false;
            AdminCreated = false;
            SystemSettingsSeeded = false;
        }

        public void MarcarPassoConcluido(bool dbInit, bool adminCreated, bool settingsSeeded, string alteradoPor)
        {
            DatabaseInitialized = dbInit;
            AdminCreated = adminCreated;
            SystemSettingsSeeded = settingsSeeded;
            MarcarAlterado(alteradoPor);
        }

        public void Concluir(string completadoPor)
        {
            if (IsCompleted)
                throw new InvalidOperationException("Instalação já concluída.");

            if (!DatabaseInitialized || !AdminCreated || !SystemSettingsSeeded)
                throw new InvalidOperationException("Passos obrigatórios pendentes.");

            IsCompleted = true;
            CompletedAt = DateTime.UtcNow;
            CompletedBy = completadoPor;
            MarcarAlterado(completadoPor);
        }
    }
}
