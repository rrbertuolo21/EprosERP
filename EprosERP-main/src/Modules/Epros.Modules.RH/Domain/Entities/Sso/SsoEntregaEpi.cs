using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.RH.Domain.Entities
{
    /// <summary>Entidade portada da EF (submodulo RH-SSO). Fidelidade campo a campo.</summary>
    public partial class SsoEntregaEpi : EntidadeSaaSBase
    {
        public Guid ColaboradorId { get; private set; }
        public string EpiDescricao { get; private set; } = string.Empty;
        public DateTime? DataEntrega { get; private set; }
        public DateTime? DataDevolucao { get; private set; }
        public string Status { get; private set; } = string.Empty;

        protected SsoEntregaEpi() { } // EF Core

        public SsoEntregaEpi(
            Guid colaboradorId,
            string epiDescricao,
            DateTime? dataEntrega,
            DateTime? dataDevolucao,
            string status,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            ColaboradorId = colaboradorId;
            EpiDescricao = epiDescricao;
            DataEntrega = dataEntrega;
            DataDevolucao = dataDevolucao;
            Status = status;
            Validar();
        }

        public void Validar()
        {
            var contract = new Contract<SsoEntregaEpi>().Requires();
            contract.AreNotEquals(ColaboradorId, Guid.Empty, nameof(ColaboradorId), "O campo ColaboradorId e obrigatorio.");
            contract.IsNotNullOrEmpty(EpiDescricao, nameof(EpiDescricao), "O campo EpiDescricao e obrigatorio.");
            contract.IsNotNullOrEmpty(Status, nameof(Status), "O campo Status e obrigatorio.");
            AddNotifications(contract);
        }

        public void MarcarAtualizado(string usuario) => MarcarAlterado(usuario);
    }
}
