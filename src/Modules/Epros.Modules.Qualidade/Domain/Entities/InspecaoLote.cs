using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Qualidade.Domain.Entities
{
    public class InspecaoLote : EntidadeSaaSBase
    {
        public Guid? CompraId { get; private set; }
        public string Sku { get; private set; } = string.Empty;
        public string NomeProduto { get; private set; } = string.Empty;
        public decimal QuantidadeLote { get; private set; }
        public string Status { get; private set; } = "Pendente";
        public DateTime? DataInspecao { get; private set; }
        public string? Responsavel { get; private set; }
        public string? Observacoes { get; private set; }

        protected InspecaoLote() { } // EF Core

        public InspecaoLote(Guid? compraId, string sku, string nomeProduto, decimal quantidadeLote, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<InspecaoLote>()
                .Requires()
                .IsNotNullOrEmpty(sku, nameof(Sku), "O SKU do produto é obrigatório.")
                .IsNotNullOrEmpty(nomeProduto, nameof(NomeProduto), "O Nome do produto é obrigatório.")
                .IsGreaterThan(quantidadeLote, 0, nameof(QuantidadeLote), "A quantidade do lote deve ser maior que zero.")
            );

            CompraId = compraId;
            Sku = sku;
            NomeProduto = nomeProduto;
            QuantidadeLote = quantidadeLote;
            Status = "Pendente";
        }

        public void RegistrarResultado(string status, string responsavel, string? observacoes, string usuario)
        {
            if (Status != "Pendente")
            {
                AddNotification(nameof(Status), "Este lote já foi inspecionado.");
                return;
            }

            if (status != "Aprovado" && status != "Reprovado")
            {
                AddNotification(nameof(Status), "Status de inspeção inválido. Deve ser 'Aprovado' ou 'Reprovado'.");
                return;
            }

            AddNotifications(new Contract<InspecaoLote>()
                .Requires()
                .IsNotNullOrEmpty(responsavel, nameof(Responsavel), "O Responsável pela inspeção é obrigatório.")
            );

            if (!IsValid) return;

            Status = status;
            Responsavel = responsavel;
            Observacoes = observacoes;
            DataInspecao = DateTime.UtcNow;

            MarcarAlterado(usuario);
        }
    }
}
