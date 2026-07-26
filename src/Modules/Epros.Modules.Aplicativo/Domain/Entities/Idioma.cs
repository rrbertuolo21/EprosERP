using System;
using Epros.Shared.Domain.Entities;

namespace Epros.Modules.Aplicativo.Domain.Entities
{
    public class Idioma : EntidadeSaaSBase
    {
        public string Code { get; private set; } = string.Empty;
        public string Name { get; private set; } = string.Empty;
        public string CountryCode { get; private set; } = string.Empty;
        public bool Enabled { get; private set; }

        protected Idioma() { } // EF Core

        public Idioma(
            string tenantId,
            string code,
            string name,
            string countryCode,
            bool enabled,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            if (string.IsNullOrWhiteSpace(code))
                AddNotification(nameof(Code), "O código do idioma é obrigatório.");
            if (code != null && code.Length > 10)
                AddNotification(nameof(Code), "O código do idioma deve ter no máximo 10 caracteres.");
            if (string.IsNullOrWhiteSpace(name))
                AddNotification(nameof(Name), "O nome do idioma é obrigatório.");
            if (string.IsNullOrWhiteSpace(countryCode))
                AddNotification(nameof(CountryCode), "O código do país é obrigatório.");
            if (countryCode != null && countryCode.Length != 2)
                AddNotification(nameof(CountryCode), "O código do país deve ter exatamente 2 caracteres.");

            Code = code?.ToLowerInvariant().Trim() ?? string.Empty;
            Name = name ?? string.Empty;
            CountryCode = countryCode?.ToUpperInvariant().Trim() ?? string.Empty;
            Enabled = enabled;
        }

        public void Ativar(string alteradoPor)
        {
            Enabled = true;
            MarcarAlterado(alteradoPor);
        }

        public void Desativar(string alteradoPor)
        {
            if (Code == "en")
            {
                AddNotification(nameof(Enabled), "O idioma inglês (en) é o idioma base e não pode ser desativado.");
                return;
            }

            Enabled = false;
            MarcarAlterado(alteradoPor);
        }
    }
}
