using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.GestaoClientes.Domain.Entities
{
    public class ContratoItem : EntidadeSaaSBase
    {
        public Guid ContratoId { get; private set; }
        public string Descricao { get; private set; } = string.Empty;
        public int Quantidade { get; private set; }
        public decimal ValorUnitario { get; private set; }

        protected ContratoItem() { } // EF Core

        public ContratoItem(
            Guid contratoId,
            string descricao,
            int quantidade,
            decimal valorUnitario,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<ContratoItem>()
                .Requires()
                .AreNotEquals(contratoId, Guid.Empty, nameof(ContratoId), "ContratoId inválido")
                .IsNotNullOrEmpty(descricao, nameof(Descricao), "Descrição é obrigatória")
                .IsGreaterThan(quantidade, 0, nameof(Quantidade), "A quantidade deve ser maior que zero")
                .IsGreaterThan(valorUnitario, 0, nameof(ValorUnitario), "O valor unitário deve ser maior que zero")
            );

            ContratoId = contratoId;
            Descricao = descricao;
            Quantidade = quantidade;
            ValorUnitario = valorUnitario;
        }
    }
}
