using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Estoque.Domain.Entities
{
    /// <summary>
    /// Volume do transporte da compra. Porte fiel do legado
    /// Epros.ERP.Domain.Entities.Compras.CompraTransporteVolume.
    /// </summary>
    public class CompraTransporteVolume : EntidadeSaaSBase
    {
        public Guid CompraTransporteId { get; private set; }
        public int QuantidadeVolumes { get; private set; }
        public string? Especie { get; private set; }
        public string? NumeroVolumes { get; private set; }
        public decimal PesoLiquido { get; private set; }
        public decimal PesoBruto { get; private set; }
        public string? Marca { get; private set; }

        // Navegação intra-módulo
        public CompraTransporte? CompraTransporte { get; private set; }

        protected CompraTransporteVolume() { } // EF Core

        public CompraTransporteVolume(Guid compraTransporteId, int quantidadeVolumes, string? especie, string? numeroVolumes, decimal pesoLiquido, decimal pesoBruto, string? marca, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            CompraTransporteId = compraTransporteId;
            QuantidadeVolumes = quantidadeVolumes;
            Especie = especie;
            NumeroVolumes = numeroVolumes;
            PesoLiquido = pesoLiquido;
            PesoBruto = pesoBruto;
            Marca = marca;
            Validar();
        }

        public void Validar()
        {
            AddNotifications(new Contract<CompraTransporteVolume>()
                .Requires()
                .IsLowerOrEqualsThan((Especie ?? "").Length, 60, nameof(Especie), "O campo Especie pode conter no máximo 60 caracteres [Origem: CompraTransporteVolume]")
                .IsLowerOrEqualsThan((Marca ?? "").Length, 60, nameof(Marca), "O campo Marca pode conter no máximo 60 caracteres [Origem: CompraTransporteVolume]")
                .IsLowerOrEqualsThan((NumeroVolumes ?? "").Length, 60, nameof(NumeroVolumes), "O campo Numero de Volumes pode conter no máximo 60 caracteres [Origem: CompraTransporteVolume]")
            );
        }

        public void Alterar(int quantidadeVolumes, string? especie, string? numeroVolumes, decimal pesoLiquido, decimal pesoBruto, string? marca, string usuario)
        {
            QuantidadeVolumes = quantidadeVolumes;
            Especie = especie;
            NumeroVolumes = numeroVolumes;
            PesoLiquido = pesoLiquido;
            PesoBruto = pesoBruto;
            Marca = marca;
            MarcarAlterado(usuario);
            Validar();
        }
    }
}
