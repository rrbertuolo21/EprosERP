using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.RH.Domain.Entities
{
    /// <summary>Entidade portada da EF (submodulo RH-REC). Fidelidade campo a campo.</summary>
    public partial class RecOnboardingCandidato : EntidadeSaaSBase
    {
        public Guid CandidatoId { get; private set; }
        public Guid ChecklistId { get; private set; }
        public DateTime DataInicio { get; private set; }
        public Guid? ResponsavelAcompanhamentoId { get; private set; }
        public string Status { get; private set; } = string.Empty;
        public Guid CriadoPorUsuarioId { get; private set; }
        public Guid DonoFuncionalId { get; private set; }

        protected RecOnboardingCandidato() { } // EF Core

        public RecOnboardingCandidato(
            Guid candidatoId,
            Guid checklistId,
            DateTime dataInicio,
            Guid? responsavelAcompanhamentoId,
            string status,
            Guid criadoPorUsuarioId,
            Guid donoFuncionalId,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            CandidatoId = candidatoId;
            ChecklistId = checklistId;
            DataInicio = dataInicio;
            ResponsavelAcompanhamentoId = responsavelAcompanhamentoId;
            Status = status;
            CriadoPorUsuarioId = criadoPorUsuarioId;
            DonoFuncionalId = donoFuncionalId;
            Validar();
        }

        public void Validar()
        {
            var contract = new Contract<RecOnboardingCandidato>().Requires();
            contract.AreNotEquals(CandidatoId, Guid.Empty, nameof(CandidatoId), "O campo CandidatoId e obrigatorio.");
            contract.AreNotEquals(ChecklistId, Guid.Empty, nameof(ChecklistId), "O campo ChecklistId e obrigatorio.");
            contract.IsNotNullOrEmpty(Status, nameof(Status), "O campo Status e obrigatorio.");
            contract.AreNotEquals(CriadoPorUsuarioId, Guid.Empty, nameof(CriadoPorUsuarioId), "O campo CriadoPorUsuarioId e obrigatorio.");
            contract.AreNotEquals(DonoFuncionalId, Guid.Empty, nameof(DonoFuncionalId), "O campo DonoFuncionalId e obrigatorio.");
            AddNotifications(contract);
        }

        public void MarcarAtualizado(string usuario) => MarcarAlterado(usuario);
    }
}
