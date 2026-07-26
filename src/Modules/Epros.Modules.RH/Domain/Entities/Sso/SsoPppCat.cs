using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.RH.Domain.Entities
{
    /// <summary>Entidade portada da EF (submodulo RH-SSO). Fidelidade campo a campo.</summary>
    public partial class SsoPppCat : EntidadeSaaSBase
    {
        public Guid PppId { get; private set; }
        public Guid ColaboradorId { get; private set; }
        public DateTime? DataAcidente { get; private set; }
        public string? Descricao { get; private set; }
        public Guid? AfastamentoId { get; private set; }
        public string Status { get; private set; } = string.Empty;

        protected SsoPppCat() { } // EF Core

        public SsoPppCat(
            Guid pppId,
            Guid colaboradorId,
            DateTime? dataAcidente,
            string? descricao,
            Guid? afastamentoId,
            string status,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            PppId = pppId;
            ColaboradorId = colaboradorId;
            DataAcidente = dataAcidente;
            Descricao = descricao;
            AfastamentoId = afastamentoId;
            Status = status;
            Validar();
        }

        public void Validar()
        {
            var contract = new Contract<SsoPppCat>().Requires();
            contract.AreNotEquals(PppId, Guid.Empty, nameof(PppId), "O campo PppId e obrigatorio.");
            contract.AreNotEquals(ColaboradorId, Guid.Empty, nameof(ColaboradorId), "O campo ColaboradorId e obrigatorio.");
            contract.IsNotNullOrEmpty(Status, nameof(Status), "O campo Status e obrigatorio.");
            AddNotifications(contract);
        }

        public void MarcarAtualizado(string usuario) => MarcarAlterado(usuario);
    }
}
