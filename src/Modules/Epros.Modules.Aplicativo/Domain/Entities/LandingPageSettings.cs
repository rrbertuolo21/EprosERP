using System;
using Epros.Shared.Domain.Entities;

namespace Epros.Modules.Aplicativo.Domain.Entities
{
    public class LandingPageSettings : EntidadeSaaSBase
    {
        public string SettingsJson { get; private set; } = "{}";

        protected LandingPageSettings() { } // EF Core

        public LandingPageSettings(string settingsJson, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            SettingsJson = string.IsNullOrEmpty(settingsJson) ? "{}" : settingsJson;
        }

        public void AtualizarSettings(string settingsJson, string alteradoPor)
        {
            SettingsJson = string.IsNullOrEmpty(settingsJson) ? "{}" : settingsJson;
            MarcarAlterado(alteradoPor);
        }
    }
}
