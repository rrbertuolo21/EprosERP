using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.GestaoClientes.Domain.Entities
{
    /// <summary>Item/composição de uma <c>Fatura</c> emitida (EF 5.9-item / 11.8): FaturaId + Descrição + Valor.</summary>
    public class FaturaItem : EntidadeSaaSBase
    {
        public Guid FaturaId { get; private set; }
        public string Descricao { get; private set; } = string.Empty;
        public decimal Valor { get; private set; }

        protected FaturaItem() { } // EF Core

        public FaturaItem(Guid faturaId, string descricao, decimal valor, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<FaturaItem>()
                .Requires()
                .AreNotEquals(faturaId, Guid.Empty, nameof(FaturaId), "FaturaId é obrigatório")
                .IsNotNullOrEmpty(descricao, nameof(Descricao), "Descrição do item é obrigatória")
                .IsMaxLength(descricao ?? string.Empty, 200, nameof(Descricao), "Descrição deve ter no máximo 200 caracteres")
                .IsGreaterOrEqualsThan(valor, 0, nameof(Valor), "Valor do item deve ser maior ou igual a zero")
            );

            FaturaId = faturaId;
            Descricao = descricao;
            Valor = valor;
        }

        public void Atualizar(string descricao, decimal valor, string alteradoPor)
        {
            AddNotifications(new Contract<FaturaItem>()
                .Requires()
                .IsNotNullOrEmpty(descricao, nameof(Descricao), "Descrição do item é obrigatória")
                .IsMaxLength(descricao ?? string.Empty, 200, nameof(Descricao), "Descrição deve ter no máximo 200 caracteres")
                .IsGreaterOrEqualsThan(valor, 0, nameof(Valor), "Valor do item deve ser maior ou igual a zero")
            );

            if (!IsValid) return;

            Descricao = descricao;
            Valor = valor;
            MarcarAlterado(alteradoPor);
        }
    }
}
