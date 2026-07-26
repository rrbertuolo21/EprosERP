using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.DMS.Domain.Entities
{
    public class VendaVeiculo : EntidadeSaaSBase
    {
        public string Chassi { get; private set; } = string.Empty; // Chassi de 17 caracteres
        public string Modelo { get; private set; } = string.Empty;
        public string Marca { get; private set; } = string.Empty;
        public int AnoModelo { get; private set; }
        public decimal PrecoVenda { get; private set; }
        public string ClienteNome { get; private set; } = string.Empty;
        public string Status { get; private set; } = "Reservado"; // Reservado, Faturado, Entregue

        protected VendaVeiculo() { } // EF Core

        public VendaVeiculo(
            string chassi,
            string modelo,
            string marca,
            int anoModelo,
            decimal precoVenda,
            string clienteNome,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<VendaVeiculo>()
                .Requires()
                .IsNotNullOrEmpty(chassi, nameof(Chassi), "O número do chassi é obrigatório.")
                .AreEquals(chassi?.Length ?? 0, 17, nameof(Chassi), "O número de chassi deve possuir exatamente 17 caracteres.")
                .IsNotNullOrEmpty(modelo, nameof(Modelo), "O modelo do veículo é obrigatório.")
                .IsNotNullOrEmpty(marca, nameof(Marca), "A marca do veículo é obrigatória.")
                .IsGreaterThan(anoModelo, 1900, nameof(AnoModelo), "Ano de modelo inválido.")
                .IsGreaterThan(precoVenda, 0, nameof(PrecoVenda), "O preço de venda do veículo deve ser maior que zero.")
                .IsNotNullOrEmpty(clienteNome, nameof(ClienteNome), "O nome do cliente comprador é obrigatório.")
            );

            Chassi = chassi?.ToUpperInvariant() ?? string.Empty;
            Modelo = modelo;
            Marca = marca;
            AnoModelo = anoModelo;
            PrecoVenda = precoVenda;
            ClienteNome = clienteNome;
            Status = "Reservado";
        }

        public void Faturar(string usuario)
        {
            if (Status != "Reservado")
            {
                AddNotification(nameof(Status), "Apenas veículos reservados podem ser faturados.");
                return;
            }

            Status = "Faturado";
            MarcarAlterado(usuario);
        }

        public void Entregar(string usuario)
        {
            if (Status != "Faturado")
            {
                AddNotification(nameof(Status), "Apenas veículos faturados podem ser entregues ao cliente final.");
                return;
            }

            Status = "Entregue";
            MarcarAlterado(usuario);
        }
    }
}
