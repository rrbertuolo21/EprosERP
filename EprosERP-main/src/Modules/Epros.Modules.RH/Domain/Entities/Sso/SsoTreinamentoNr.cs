using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.RH.Domain.Entities
{
    /// <summary>Entidade portada da EF (submodulo RH-SSO). Fidelidade campo a campo.</summary>
    public partial class SsoTreinamentoNr : EntidadeSaaSBase
    {
        public Guid ColaboradorId { get; private set; }
        public string Norma { get; private set; } = string.Empty;
        public Guid? TreinamentoId { get; private set; }
        public bool Obrigatorio { get; private set; }
        public string Status { get; private set; } = string.Empty;

        protected SsoTreinamentoNr() { } // EF Core

        public SsoTreinamentoNr(
            Guid colaboradorId,
            string norma,
            Guid? treinamentoId,
            bool obrigatorio,
            string status,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            ColaboradorId = colaboradorId;
            Norma = norma;
            TreinamentoId = treinamentoId;
            Obrigatorio = obrigatorio;
            Status = status;
            Validar();
        }

        public void Validar()
        {
            var contract = new Contract<SsoTreinamentoNr>().Requires();
            contract.AreNotEquals(ColaboradorId, Guid.Empty, nameof(ColaboradorId), "O campo ColaboradorId e obrigatorio.");
            contract.IsNotNullOrEmpty(Norma, nameof(Norma), "O campo Norma e obrigatorio.");
            contract.IsNotNullOrEmpty(Status, nameof(Status), "O campo Status e obrigatorio.");
            AddNotifications(contract);
        }

        public void MarcarAtualizado(string usuario) => MarcarAlterado(usuario);
    }
}
