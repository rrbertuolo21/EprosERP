using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.GestaoClientes.Domain.Entities
{
    public class HistoricoReajuste : EntidadeSaaSBase
    {
        public Guid ComposicaoId { get; private set; }
        public string Descricao { get; private set; } = string.Empty;
        public decimal ValorAtual { get; private set; }
        public decimal ValorNovo { get; private set; }
        public decimal PercentualReajuste { get; private set; }
        public string TipoReajuste { get; private set; } = string.Empty; // IGPM, IPCA, Acordo, etc.

        protected HistoricoReajuste() { } // EF Core

        public HistoricoReajuste(
            Guid composicaoId,
            string descricao,
            decimal valorAtual,
            decimal valorNovo,
            decimal percentualReajuste,
            string tipoReajuste,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<HistoricoReajuste>()
                .Requires()
                .AreNotEquals(composicaoId, Guid.Empty, nameof(ComposicaoId), "ComposicaoId é obrigatório")
                .IsNotNullOrEmpty(descricao, nameof(Descricao), "Descrição é obrigatória")
                .IsGreaterThan(valorNovo, 0, nameof(ValorNovo), "Novo valor deve ser maior que zero")
            );

            ComposicaoId = composicaoId;
            Descricao = descricao;
            ValorAtual = valorAtual;
            ValorNovo = valorNovo;
            PercentualReajuste = percentualReajuste;
            TipoReajuste = tipoReajuste;
        }
    }
}
