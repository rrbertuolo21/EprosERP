using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.DMS.Domain.Entities
{
    public class ReservaPeca : EntidadeSaaSBase
    {
        public Guid DemandaId { get; private set; }
        public Guid PecaId { get; private set; }
        public Guid LocalId { get; private set; }
        public decimal QuantidadeReservada { get; private set; }

        protected ReservaPeca() { } // EF Core

        public ReservaPeca(
            Guid demandaId,
            Guid pecaId,
            Guid localId,
            decimal quantidadeReservada,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<ReservaPeca>()
                .Requires()
                .AreNotEquals(demandaId, Guid.Empty, nameof(DemandaId), "A demanda é obrigatória.")
                .AreNotEquals(pecaId, Guid.Empty, nameof(PecaId), "A peça é obrigatória.")
                .AreNotEquals(localId, Guid.Empty, nameof(LocalId), "O local é obrigatório.")
                .IsGreaterThan(quantidadeReservada, 0, nameof(QuantidadeReservada), "A quantidade reservada deve ser maior que zero.")
            );

            DemandaId = demandaId;
            PecaId = pecaId;
            LocalId = localId;
            QuantidadeReservada = quantidadeReservada;
        }

        public void Cancelar(string usuario)
        {
            MarcarAlterado(usuario);
        }
    }
}
