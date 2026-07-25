using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.DMS.Domain.Entities
{
    public class EstoqueVeiculo : EntidadeSaaSBase
    {
        public Guid VeiculoId { get; private set; }
        public string ChassiVin { get; private set; } = string.Empty; // VIN de 17 caracteres
        public Guid LocalId { get; private set; }
        public string Status { get; private set; } = "Livre";
        public decimal? Custo { get; private set; }
        public decimal? PrecoSugerido { get; private set; }
        public DateTime? DataEntrada { get; private set; }

        protected EstoqueVeiculo() { } // EF Core

        public EstoqueVeiculo(
            Guid veiculoId,
            string chassiVin,
            Guid localId,
            decimal? custo,
            decimal? precoSugerido,
            DateTime? dataEntrada,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<EstoqueVeiculo>()
                .Requires()
                .AreNotEquals(veiculoId, Guid.Empty, nameof(VeiculoId), "O veículo é obrigatório.")
                .IsNotNullOrEmpty(chassiVin, nameof(ChassiVin), "O chassi/VIN é obrigatório.")
                .AreEquals(chassiVin?.Length ?? 0, 17, nameof(ChassiVin), "O chassi/VIN deve possuir exatamente 17 caracteres.")
                .AreNotEquals(localId, Guid.Empty, nameof(LocalId), "O local é obrigatório.")
            );

            VeiculoId = veiculoId;
            ChassiVin = chassiVin?.ToUpperInvariant() ?? string.Empty;
            LocalId = localId;
            Custo = custo;
            PrecoSugerido = precoSugerido;
            DataEntrada = dataEntrada;
            Status = "Livre";
        }

        public void Reservar(string usuario)
        {
            if (Status != "Livre")
            {
                AddNotification(nameof(Status), "Apenas unidades livres podem ser reservadas.");
                return;
            }

            Status = "Reservado";
            MarcarAlterado(usuario);
        }

        public void Vender(string usuario)
        {
            if (Status != "Reservado")
            {
                AddNotification(nameof(Status), "Apenas unidades reservadas podem ser vendidas.");
                return;
            }

            Status = "Vendido";
            MarcarAlterado(usuario);
        }

        public void Liberar(string usuario)
        {
            Status = "Livre";
            MarcarAlterado(usuario);
        }
    }
}
