using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Producao.Domain.Entities
{
    public class BomItem : EntidadeSaaSBase
    {
        public Guid ListaMateriaisId { get; private set; }
        public string InsumoSku { get; private set; } = string.Empty;
        public decimal QuantidadeNecessaria { get; private set; }
        public string UnidadeMedida { get; private set; } = string.Empty;

        protected BomItem() { } // EF Core

        public BomItem(Guid listaMateriaisId, string insumoSku, decimal quantidadeNecessaria, string unidadeMedida, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<BomItem>()
                .Requires()
                .AreNotEquals(listaMateriaisId, Guid.Empty, nameof(ListaMateriaisId), "O ID da lista de materiais é obrigatório.")
                .IsNotNullOrEmpty(insumoSku, nameof(InsumoSku), "O SKU do insumo é obrigatório.")
                .IsGreaterThan(quantidadeNecessaria, 0, nameof(QuantidadeNecessaria), "A quantidade necessária deve ser maior que zero.")
                .IsNotNullOrEmpty(unidadeMedida, nameof(UnidadeMedida), "A unidade de medida é obrigatória.")
            );

            ListaMateriaisId = listaMateriaisId;
            InsumoSku = insumoSku;
            QuantidadeNecessaria = quantidadeNecessaria;
            UnidadeMedida = unidadeMedida;
        }
    }
}
