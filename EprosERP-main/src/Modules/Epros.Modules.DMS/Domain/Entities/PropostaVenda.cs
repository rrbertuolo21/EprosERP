using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.DMS.Domain.Entities
{
    public class PropostaVenda : EntidadeSaaSBase
    {
        public Guid OportunidadeId { get; private set; }
        public Guid ClienteId { get; private set; }
        public Guid? EstoqueVeiculoId { get; private set; }
        public int Versao { get; private set; } = 1;
        public DateTime ValidaAte { get; private set; }
        public decimal ValorVeiculo { get; private set; }
        public decimal Desconto { get; private set; }
        public decimal ValorFinal { get; private set; }
        public string Status { get; private set; } = "Emitida";

        protected PropostaVenda() { } // EF Core

        public PropostaVenda(
            Guid oportunidadeId,
            Guid clienteId,
            Guid? estoqueVeiculoId,
            DateTime validaAte,
            decimal valorVeiculo,
            decimal desconto,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<PropostaVenda>()
                .Requires()
                .AreNotEquals(oportunidadeId, Guid.Empty, nameof(OportunidadeId), "A oportunidade é obrigatória.")
                .AreNotEquals(clienteId, Guid.Empty, nameof(ClienteId), "O cliente é obrigatório.")
                .IsGreaterThan(valorVeiculo, 0, nameof(ValorVeiculo), "O valor do veículo deve ser maior que zero.")
                .IsGreaterThan(desconto, -0.001m, nameof(Desconto), "O desconto não pode ser negativo.")
            );

            OportunidadeId = oportunidadeId;
            ClienteId = clienteId;
            EstoqueVeiculoId = estoqueVeiculoId;
            Versao = 1;
            ValidaAte = validaAte;
            ValorVeiculo = valorVeiculo;
            Desconto = desconto;
            ValorFinal = valorVeiculo - desconto;
            Status = "Emitida";
        }

        public void Aceitar(string usuario)
        {
            if (Status != "Emitida")
            {
                AddNotification(nameof(Status), "Apenas propostas emitidas podem ser aceitas.");
                return;
            }

            Status = "Aceita";
            MarcarAlterado(usuario);
        }

        public void Expirar(string usuario)
        {
            Status = "Expirada";
            MarcarAlterado(usuario);
        }
    }
}
