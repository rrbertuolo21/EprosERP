using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.DMS.Domain.Entities
{
    public class DemandaPeca : EntidadeSaaSBase
    {
        public Guid PecaId { get; private set; }
        public Guid LocalId { get; private set; }
        public string OrigemTipo { get; private set; } = string.Empty;
        public Guid OrigemId { get; private set; }
        public Guid ItemOrigemId { get; private set; }
        public decimal Quantidade { get; private set; }
        public DateTime Prazo { get; private set; }
        public string Prioridade { get; private set; } = "Normal";
        public string Status { get; private set; } = "Aberta";

        protected DemandaPeca() { } // EF Core

        public DemandaPeca(
            Guid pecaId,
            Guid localId,
            string origemTipo,
            Guid origemId,
            Guid itemOrigemId,
            decimal quantidade,
            DateTime prazo,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<DemandaPeca>()
                .Requires()
                .AreNotEquals(pecaId, Guid.Empty, nameof(PecaId), "A peça é obrigatória.")
                .AreNotEquals(localId, Guid.Empty, nameof(LocalId), "O local é obrigatório.")
                .IsNotNullOrEmpty(origemTipo, nameof(OrigemTipo), "O tipo de origem é obrigatório.")
                .AreNotEquals(origemId, Guid.Empty, nameof(OrigemId), "A origem é obrigatória.")
                .AreNotEquals(itemOrigemId, Guid.Empty, nameof(ItemOrigemId), "O item de origem é obrigatório.")
                .IsGreaterThan(quantidade, 0, nameof(Quantidade), "A quantidade deve ser maior que zero.")
            );

            PecaId = pecaId;
            LocalId = localId;
            OrigemTipo = origemTipo;
            OrigemId = origemId;
            ItemOrigemId = itemOrigemId;
            Quantidade = quantidade;
            Prazo = prazo;
            Prioridade = "Normal";
            Status = "Aberta";
        }

        public void Atender(string usuario)
        {
            if (Status == "Atendida")
            {
                AddNotification(nameof(Status), "Demanda já atendida.");
                return;
            }

            Status = "Atendida";
            MarcarAlterado(usuario);
        }

        public void Cancelar(string usuario)
        {
            Status = "Cancelada";
            MarcarAlterado(usuario);
        }
    }
}
