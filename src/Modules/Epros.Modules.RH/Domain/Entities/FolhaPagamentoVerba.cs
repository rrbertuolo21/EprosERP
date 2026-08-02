using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.RH.Domain.Entities
{
    public class FolhaPagamentoVerba : EntidadeSaaSBase
    {
        public Guid FolhaPagamentoId { get; private set; }
        public string Descricao { get; private set; } = string.Empty;
        public string Tipo { get; private set; } = string.Empty; // Provento, Desconto, Encargo
        public decimal Valor { get; private set; }

        protected FolhaPagamentoVerba() { } // EF Core

        public FolhaPagamentoVerba(
            Guid folhaPagamentoId,
            string descricao,
            string tipo,
            decimal valor,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<FolhaPagamentoVerba>()
                .Requires()
                .AreNotEquals(folhaPagamentoId, Guid.Empty, nameof(FolhaPagamentoId), "O ID da folha de pagamento é obrigatório.")
                .IsNotNullOrEmpty(descricao, nameof(Descricao), "A descrição da verba é obrigatória.")
                .IsTrue(tipo == "Provento" || tipo == "Desconto" || tipo == "Encargo", nameof(Tipo), "O tipo deve ser 'Provento', 'Desconto' ou 'Encargo'.")
                .IsGreaterThan(valor, 0, nameof(Valor), "O valor deve ser maior que zero.")
            );

            FolhaPagamentoId = folhaPagamentoId;
            Descricao = descricao;
            Tipo = tipo;
            Valor = valor;
        }
    }
}
