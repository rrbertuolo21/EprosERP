using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.GestaoClientes.Domain.Entities
{
    public class ConversaoUnidade : EntidadeSaaSBase
    {
        public Guid UnidadeOrigemId { get; private set; }
        public Guid UnidadeDestinoId { get; private set; }
        public decimal Fator { get; private set; }

        protected ConversaoUnidade() { } // EF Core

        public ConversaoUnidade(
            Guid unidadeOrigemId,
            Guid unidadeDestinoId,
            decimal fator,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<ConversaoUnidade>()
                .Requires()
                .AreNotEquals(unidadeOrigemId, Guid.Empty, nameof(UnidadeOrigemId), "Unidade origem inválida.")
                .AreNotEquals(unidadeDestinoId, Guid.Empty, nameof(UnidadeDestinoId), "Unidade destino inválida.")
                .AreNotEquals(unidadeOrigemId, unidadeDestinoId, nameof(UnidadeDestinoId), "As unidades de origem e destino devem ser diferentes.")
                .IsGreaterThan(fator, 0.000000m, nameof(Fator), "Fator de conversão deve ser maior que zero.")
            );

            UnidadeOrigemId = unidadeOrigemId;
            UnidadeDestinoId = unidadeDestinoId;
            Fator = fator;
        }

        public void AtualizarFator(decimal fator, string alteradoPor)
        {
            AddNotifications(new Contract<ConversaoUnidade>()
                .Requires()
                .IsGreaterThan(fator, 0.000000m, nameof(Fator), "Fator de conversão deve ser maior que zero.")
            );

            if (IsValid)
            {
                Fator = fator;
                MarcarAlterado(alteradoPor);
            }
        }
    }
}
