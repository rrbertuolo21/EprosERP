using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.RH.Domain.Entities
{
    /// <summary>Entidade portada da EF (submodulo RH-SSO). Fidelidade campo a campo.</summary>
    public partial class SsoPpp : EntidadeSaaSBase
    {
        public Guid ColaboradorId { get; private set; }
        public string Observacao { get; private set; } = string.Empty;
        public string Status { get; private set; } = string.Empty;

        protected SsoPpp() { } // EF Core

        public SsoPpp(
            Guid colaboradorId,
            string observacao,
            string status,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            ColaboradorId = colaboradorId;
            Observacao = observacao;
            Status = status;
            Validar();
        }

        public void Validar()
        {
            var contract = new Contract<SsoPpp>().Requires();
            contract.AreNotEquals(ColaboradorId, Guid.Empty, nameof(ColaboradorId), "O campo ColaboradorId e obrigatorio.");
            contract.IsNotNullOrEmpty(Observacao, nameof(Observacao), "O campo Observacao e obrigatorio.");
            contract.IsNotNullOrEmpty(Status, nameof(Status), "O campo Status e obrigatorio.");
            AddNotifications(contract);
        }

        public void MarcarAtualizado(string usuario) => MarcarAlterado(usuario);
    }
}
