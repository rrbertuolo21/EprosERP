using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.GestaoClientes.Domain.Entities
{
    /// <summary>Preço/período por nível de usuário (APP-TEN-003 11.12).</summary>
    public class PrecoNivelUsuario : EntidadeSaaSBase
    {
        public Guid NivelUsuarioId { get; private set; }
        public string PricingLabel { get; private set; } = string.Empty;
        public string PackagePricingType { get; private set; } = "period";
        public string? Period { get; private set; }
        public long? DownloadAllowance { get; private set; }
        public decimal Price { get; private set; }

        protected PrecoNivelUsuario() { } // EF Core

        public PrecoNivelUsuario(
            Guid nivelUsuarioId,
            string pricingLabel,
            string packagePricingType,
            string? period,
            long? downloadAllowance,
            decimal price,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            NivelUsuarioId = nivelUsuarioId;
            PricingLabel = pricingLabel;
            PackagePricingType = string.IsNullOrWhiteSpace(packagePricingType) ? "period" : packagePricingType;
            Period = period ?? "1M";
            DownloadAllowance = downloadAllowance;
            Price = price;
            Validar();
        }

        public void Alterar(string pricingLabel, string packagePricingType, string? period, long? downloadAllowance, decimal price, string alteradoPor)
        {
            PricingLabel = pricingLabel;
            PackagePricingType = string.IsNullOrWhiteSpace(packagePricingType) ? "period" : packagePricingType;
            Period = period;
            DownloadAllowance = downloadAllowance;
            Price = price;
            MarcarAlterado(alteradoPor);
            Validar();
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<PrecoNivelUsuario>()
                .Requires()
                .AreNotEquals(NivelUsuarioId, Guid.Empty, nameof(NivelUsuarioId), "NivelUsuarioId é obrigatório [Origem: PrecoNivelUsuario]")
                .IsNotNullOrEmpty(PricingLabel, nameof(PricingLabel), "PricingLabel é obrigatório [Origem: PrecoNivelUsuario]")
                .HasMaxLen(PricingLabel ?? string.Empty, 50, nameof(PricingLabel), "PricingLabel deve ter no máximo 50 caracteres [Origem: PrecoNivelUsuario]")
                .HasMaxLen(PackagePricingType ?? string.Empty, 10, nameof(PackagePricingType), "PackagePricingType deve ter no máximo 10 caracteres [Origem: PrecoNivelUsuario]")
                .HasMaxLen(Period ?? string.Empty, 10, nameof(Period), "Period deve ter no máximo 10 caracteres [Origem: PrecoNivelUsuario]")
                .IsGreaterOrEqualsThan(Price, 0, nameof(Price), "Price deve ser maior ou igual a zero [Origem: PrecoNivelUsuario]")
            );
        }
    }
}
