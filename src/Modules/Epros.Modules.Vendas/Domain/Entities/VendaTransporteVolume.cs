using System;
using Epros.Shared.Domain.Entities;
using Flunt.Notifications;
using Flunt.Validations;

namespace Epros.Modules.Vendas.Domain.Entities
{
    /// <summary>
    /// Porte fiel de VendaTransporteVolume. FK long -> Guid; herda EntidadeSaaSBase.
    /// </summary>
    public class VendaTransporteVolume : EntidadeSaaSBase
    {
        public Guid VendaTransporteId { get; private set; }
        public int QuantidadeVolumes { get; private set; }
        public string? Especie { get; private set; }
        public string? NumeroVolumes { get; private set; }
        public decimal PesoLiquido { get; private set; }
        public decimal PesoBruto { get; private set; }
        public string? Marca { get; private set; }

        // Navegação intra-módulo
        public VendaTransporte VendaTransporte { get; private set; } = null!;

        protected VendaTransporteVolume() { } // EF Core

        public VendaTransporteVolume(Guid vendaTransporteId, int quantidadeVolumes, string? especie, string? numeroVolumes, decimal pesoLiquido, decimal pesoBruto, string? marca,
            string tenantId, string criadoPor) : base(tenantId, criadoPor)
        {
            VendaTransporteId = vendaTransporteId;
            QuantidadeVolumes = quantidadeVolumes;
            Especie = especie;
            NumeroVolumes = numeroVolumes;
            PesoLiquido = pesoLiquido;
            PesoBruto = pesoBruto;
            Marca = marca;
            Validar();
        }

        public void Alterar(int quantidadeVolumes, string? especie, string? numeroVolumes, decimal pesoLiquido, decimal pesoBruto, string? marca, string alteradoPor)
        {
            QuantidadeVolumes = quantidadeVolumes;
            Especie = especie;
            NumeroVolumes = numeroVolumes;
            PesoLiquido = pesoLiquido;
            PesoBruto = pesoBruto;
            Marca = marca;
            MarcarAlterado(alteradoPor);
            Validar();
        }

        /// <summary>Porte fiel de VendaTransporteVolume.Validar.</summary>
        public void Validar()
        {
            AddNotifications(new Contract<Notification>()
                .Requires()
                .IsLowerOrEqualsThan(60, (Especie ?? "").Length, "Especie", "O campo Especie pode conter no máximo 60 caracteres [Origem: VendaTransporteVolume]")
                .IsLowerOrEqualsThan(60, (Marca ?? "").Length, "Marca", "O campo Marca pode conter no máximo 60 caracteres [Origem: VendaTransporteVolume]")
                .IsLowerOrEqualsThan(60, (NumeroVolumes ?? "").Length, "NumeroVolumes", "O campo Numero de Volumes pode conter no máximo 60 caracteres [Origem: VendaTransporteVolume]")
            );
        }
    }
}
