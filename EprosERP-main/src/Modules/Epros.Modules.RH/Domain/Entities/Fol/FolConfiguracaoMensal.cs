using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.RH.Domain.Entities
{
    /// <summary>Entidade portada da EF (submodulo RH-FOL). Fidelidade campo a campo.</summary>
    public partial class FolConfiguracaoMensal : EntidadeSaaSBase
    {
        public string Competencia { get; private set; } = string.Empty;
        public Guid ColaboradorId { get; private set; }
        public Guid PacoteSalarialId { get; private set; }

        protected FolConfiguracaoMensal() { } // EF Core

        public FolConfiguracaoMensal(
            string competencia,
            Guid colaboradorId,
            Guid pacoteSalarialId,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            Competencia = competencia;
            ColaboradorId = colaboradorId;
            PacoteSalarialId = pacoteSalarialId;
            Validar();
        }

        public void Validar()
        {
            var contract = new Contract<FolConfiguracaoMensal>().Requires();
            contract.IsNotNullOrEmpty(Competencia, nameof(Competencia), "O campo Competencia e obrigatorio.");
            contract.AreNotEquals(ColaboradorId, Guid.Empty, nameof(ColaboradorId), "O campo ColaboradorId e obrigatorio.");
            contract.AreNotEquals(PacoteSalarialId, Guid.Empty, nameof(PacoteSalarialId), "O campo PacoteSalarialId e obrigatorio.");
            AddNotifications(contract);
        }

        public void MarcarAtualizado(string usuario) => MarcarAlterado(usuario);
    }
}
