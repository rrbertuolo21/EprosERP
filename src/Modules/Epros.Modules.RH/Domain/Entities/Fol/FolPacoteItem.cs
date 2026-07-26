using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.RH.Domain.Entities
{
    /// <summary>Entidade portada da EF (submodulo RH-FOL). Fidelidade campo a campo.</summary>
    public partial class FolPacoteItem : EntidadeSaaSBase
    {
        public Guid PacoteSalarialId { get; private set; }
        public Guid RubricaId { get; private set; }
        public decimal Valor { get; private set; }
        public string? Narrativa { get; private set; }

        protected FolPacoteItem() { } // EF Core

        public FolPacoteItem(
            Guid pacoteSalarialId,
            Guid rubricaId,
            decimal valor,
            string? narrativa,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            PacoteSalarialId = pacoteSalarialId;
            RubricaId = rubricaId;
            Valor = valor;
            Narrativa = narrativa;
            Validar();
        }

        public void Validar()
        {
            var contract = new Contract<FolPacoteItem>().Requires();
            contract.AreNotEquals(PacoteSalarialId, Guid.Empty, nameof(PacoteSalarialId), "O campo PacoteSalarialId e obrigatorio.");
            contract.AreNotEquals(RubricaId, Guid.Empty, nameof(RubricaId), "O campo RubricaId e obrigatorio.");
            AddNotifications(contract);
        }

        public void MarcarAtualizado(string usuario) => MarcarAlterado(usuario);
    }
}
