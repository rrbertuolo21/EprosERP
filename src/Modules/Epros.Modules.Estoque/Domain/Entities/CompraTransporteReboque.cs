using System;
using Epros.Shared.Domain.Enums;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Estoque.Domain.Entities
{
    /// <summary>
    /// Reboque do transporte da compra. Porte fiel do legado
    /// Epros.ERP.Domain.Entities.Compras.CompraTransporteReboque. VeiculoId é FK Guid (sem navegação cruzada).
    /// </summary>
    public class CompraTransporteReboque : EntidadeSaaSBase
    {
        public Guid CompraTransporteId { get; private set; }
        public Guid? VeiculoId { get; private set; }
        public string Placa { get; private set; } = string.Empty;
        public EEstado Uf { get; private set; }
        public string? Rntrc { get; private set; }

        // Navegação intra-módulo
        public CompraTransporte? Transporte { get; private set; }

        protected CompraTransporteReboque() { } // EF Core

        public CompraTransporteReboque(Guid compraTransporteId, Guid? veiculoId, string placa, EEstado uf, string? rntrc, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            CompraTransporteId = compraTransporteId;
            VeiculoId = veiculoId;
            Placa = placa ?? string.Empty;
            Uf = uf;
            Rntrc = rntrc;
            Validar();
        }

        public void Validar()
        {
            AddNotifications(new Contract<CompraTransporteReboque>()
                .Requires()
                .IsTrue(Enum.IsDefined(typeof(EEstado), Uf), nameof(Uf), "Uf não consta na lista [Origem: compra Transporte Reboque]")
                .IsLowerOrEqualsThan((Placa ?? "").Length, 8, nameof(Placa), "O campo Placa deve ter no máximo 8 caracteres [Origem: compra Transporte Reboque]")
                .IsLowerOrEqualsThan((Rntrc ?? "").Length, 14, nameof(Rntrc), "O campo Rntrc deve ter no máximo 14 caracteres [Origem: compra Transporte Reboque]")
            );
        }

        public void Alterar(Guid? veiculoId, string placa, EEstado uf, string? rntrc, string usuario)
        {
            VeiculoId = veiculoId;
            Placa = placa ?? string.Empty;
            Uf = uf;
            Rntrc = rntrc;
            MarcarAlterado(usuario);
        }
    }
}
