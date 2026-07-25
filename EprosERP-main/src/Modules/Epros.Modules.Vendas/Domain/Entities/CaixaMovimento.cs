using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Vendas.Domain.Entities
{
    public class CaixaMovimento : EntidadeSaaSBase
    {
        public Guid CaixaId { get; private set; }
        public string Tipo { get; private set; } = string.Empty; // 'Suprimento', 'Sangria'
        public decimal Valor { get; private set; }
        public string? Observacao { get; private set; }

        protected CaixaMovimento() { } // EF Core

        public CaixaMovimento(Guid id, Guid syncId, Guid caixaId, string tipo, decimal valor, string? observacao, string tenantId, string criadoPor, DateTime criadoEm)
            : base(id, syncId, tenantId, criadoPor, criadoEm)
        {
            AddNotifications(new Contract<CaixaMovimento>()
                .Requires()
                .AreNotEquals(caixaId, Guid.Empty, nameof(CaixaId), "O ID do caixa é obrigatório.")
                .IsNotNullOrEmpty(tipo, nameof(Tipo), "O tipo de movimentação é obrigatório.")
                .IsGreaterThan(valor, 0m, nameof(Valor), "O valor da movimentação deve ser maior que zero.")
            );

            if (tipo != "Suprimento" && tipo != "Sangria")
            {
                AddNotification(nameof(Tipo), "O tipo de movimentação deve ser 'Suprimento' ou 'Sangria'.");
            }

            CaixaId = caixaId;
            Tipo = tipo;
            Valor = valor;
            Observacao = observacao;
        }
    }
}
