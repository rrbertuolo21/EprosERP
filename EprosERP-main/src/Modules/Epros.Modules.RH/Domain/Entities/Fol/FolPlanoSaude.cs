using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.RH.Domain.Entities
{
    /// <summary>Entidade portada da EF (submodulo RH-FOL). Fidelidade campo a campo.</summary>
    public partial class FolPlanoSaude : EntidadeSaaSBase
    {
        public Guid OperadoraPlanoSaudeId { get; private set; }
        public Guid ColaboradorId { get; private set; }
        public DateTime? DataInicio { get; private set; }
        public string Beneficiario { get; private set; } = string.Empty;

        protected FolPlanoSaude() { } // EF Core

        public FolPlanoSaude(
            Guid operadoraPlanoSaudeId,
            Guid colaboradorId,
            DateTime? dataInicio,
            string beneficiario,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            OperadoraPlanoSaudeId = operadoraPlanoSaudeId;
            ColaboradorId = colaboradorId;
            DataInicio = dataInicio;
            Beneficiario = beneficiario;
            Validar();
        }

        public void Validar()
        {
            var contract = new Contract<FolPlanoSaude>().Requires();
            contract.AreNotEquals(OperadoraPlanoSaudeId, Guid.Empty, nameof(OperadoraPlanoSaudeId), "O campo OperadoraPlanoSaudeId e obrigatorio.");
            contract.AreNotEquals(ColaboradorId, Guid.Empty, nameof(ColaboradorId), "O campo ColaboradorId e obrigatorio.");
            contract.IsNotNullOrEmpty(Beneficiario, nameof(Beneficiario), "O campo Beneficiario e obrigatorio.");
            AddNotifications(contract);
        }

        public void MarcarAtualizado(string usuario) => MarcarAlterado(usuario);
    }
}
