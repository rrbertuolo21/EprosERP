using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.RH.Domain.Entities
{
    /// <summary>Entidade portada da EF (submodulo RH-FOL). Fidelidade campo a campo.</summary>
    public partial class FolPagamentoDiarioItem : EntidadeSaaSBase
    {
        public Guid PagamentoDiarioId { get; private set; }
        public Guid ColaboradorId { get; private set; }
        public decimal? Valor { get; private set; }

        protected FolPagamentoDiarioItem() { } // EF Core

        public FolPagamentoDiarioItem(
            Guid pagamentoDiarioId,
            Guid colaboradorId,
            decimal? valor,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            PagamentoDiarioId = pagamentoDiarioId;
            ColaboradorId = colaboradorId;
            Valor = valor;
            Validar();
        }

        public void Validar()
        {
            var contract = new Contract<FolPagamentoDiarioItem>().Requires();
            contract.AreNotEquals(PagamentoDiarioId, Guid.Empty, nameof(PagamentoDiarioId), "O campo PagamentoDiarioId e obrigatorio.");
            contract.AreNotEquals(ColaboradorId, Guid.Empty, nameof(ColaboradorId), "O campo ColaboradorId e obrigatorio.");
            AddNotifications(contract);
        }

        public void MarcarAtualizado(string usuario) => MarcarAlterado(usuario);
    }
}
