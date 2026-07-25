using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Aplicativo.Domain.Entities
{
    public class MarketplaceSettings : EntidadeSaaSBase
    {
        public string Modulo { get; private set; } = string.Empty;
        public string SettingsJson { get; private set; } = "{}";

        protected MarketplaceSettings() { } // EF Core

        public MarketplaceSettings(string modulo, string settingsJson, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<MarketplaceSettings>()
                .Requires()
                .IsNotNullOrEmpty(modulo, nameof(Modulo), "O identificador do módulo é obrigatório")
            );

            Modulo = modulo;
            SettingsJson = string.IsNullOrEmpty(settingsJson) ? "{}" : settingsJson;
        }

        public void AtualizarSettings(string settingsJson, string alteradoPor)
        {
            SettingsJson = string.IsNullOrEmpty(settingsJson) ? "{}" : settingsJson;
            MarcarAlterado(alteradoPor);
        }
    }
}
