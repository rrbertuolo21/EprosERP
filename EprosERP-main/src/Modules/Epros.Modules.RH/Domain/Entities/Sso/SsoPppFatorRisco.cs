using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.RH.Domain.Entities
{
    /// <summary>Entidade portada da EF (submodulo RH-SSO). Fidelidade campo a campo.</summary>
    public partial class SsoPppFatorRisco : EntidadeSaaSBase
    {
        public Guid PppId { get; private set; }
        public string FatorRisco { get; private set; } = string.Empty;
        public string? Intensidade { get; private set; }
        public string? TecnicaMedicao { get; private set; }

        protected SsoPppFatorRisco() { } // EF Core

        public SsoPppFatorRisco(
            Guid pppId,
            string fatorRisco,
            string? intensidade,
            string? tecnicaMedicao,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            PppId = pppId;
            FatorRisco = fatorRisco;
            Intensidade = intensidade;
            TecnicaMedicao = tecnicaMedicao;
            Validar();
        }

        public void Validar()
        {
            var contract = new Contract<SsoPppFatorRisco>().Requires();
            contract.AreNotEquals(PppId, Guid.Empty, nameof(PppId), "O campo PppId e obrigatorio.");
            contract.IsNotNullOrEmpty(FatorRisco, nameof(FatorRisco), "O campo FatorRisco e obrigatorio.");
            AddNotifications(contract);
        }

        public void MarcarAtualizado(string usuario) => MarcarAlterado(usuario);
    }
}
