using System;
using Epros.Shared.Domain.Entities;
using Epros.Shared.Domain.Enums;
using Flunt.Notifications;
using Flunt.Validations;

namespace Epros.Modules.Vendas.Domain.Entities
{
    /// <summary>
    /// Porte fiel de VendaTransporteVeiculo. FK long -> Guid; VeiculoId referencia veículo
    /// de outro módulo por Guid FK. Herda EntidadeSaaSBase.
    /// </summary>
    public class VendaTransporteVeiculo : EntidadeSaaSBase
    {
        public Guid VendaTransporteId { get; private set; }
        public Guid? VeiculoId { get; private set; }
        public string Placa { get; private set; } = string.Empty;
        public EEstado Uf { get; private set; }
        public string? Rntrc { get; private set; }

        // Navegação intra-módulo
        public VendaTransporte Transporte { get; private set; } = null!;

        protected VendaTransporteVeiculo() { } // EF Core

        public VendaTransporteVeiculo(Guid vendaTransporteId, Guid? veiculoId, string placa, EEstado uf, string? rntrc,
            string tenantId, string criadoPor) : base(tenantId, criadoPor)
        {
            VendaTransporteId = vendaTransporteId;
            VeiculoId = veiculoId;
            Placa = placa;
            Uf = uf;
            Rntrc = rntrc;
            Validar();
        }

        public void Alterar(Guid? veiculoId, string placa, EEstado uf, string? rntrc, string alteradoPor)
        {
            VeiculoId = veiculoId;
            Placa = placa;
            Uf = uf;
            Rntrc = rntrc;
            MarcarAlterado(alteradoPor);
        }

        /// <summary>Porte fiel de VendaTransporteVeiculo.Validar.</summary>
        public void Validar()
        {
            AddNotifications(new Contract<Notification>()
                .Requires()
                .IsTrue(Enum.IsDefined(typeof(EEstado), Uf), "Uf", "Uf não consta na lista [Origem: Veiculo]")
                .IsLowerOrEqualsThan(8, (Placa ?? "").Length, "Placa", "O campo Placa deve ter no máximo 8 caracteres [Origem: Venda Transporte Veiculo]")
                .IsLowerOrEqualsThan(14, (Rntrc ?? "").Length, "Rntrc", "O campo Rntrc deve ter no máximo 14 caracteres [Origem: Venda Transporte Veiculo]")
            );
        }
    }
}
