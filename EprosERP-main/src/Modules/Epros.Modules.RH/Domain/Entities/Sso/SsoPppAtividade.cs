using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.RH.Domain.Entities
{
    /// <summary>Entidade portada da EF (submodulo RH-SSO). Fidelidade campo a campo.</summary>
    public partial class SsoPppAtividade : EntidadeSaaSBase
    {
        public Guid PppId { get; private set; }
        public string DescricaoAtividade { get; private set; } = string.Empty;
        public DateTime? DataInicio { get; private set; }
        public DateTime? DataFim { get; private set; }

        protected SsoPppAtividade() { } // EF Core

        public SsoPppAtividade(
            Guid pppId,
            string descricaoAtividade,
            DateTime? dataInicio,
            DateTime? dataFim,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            PppId = pppId;
            DescricaoAtividade = descricaoAtividade;
            DataInicio = dataInicio;
            DataFim = dataFim;
            Validar();
        }

        public void Validar()
        {
            var contract = new Contract<SsoPppAtividade>().Requires();
            contract.AreNotEquals(PppId, Guid.Empty, nameof(PppId), "O campo PppId e obrigatorio.");
            contract.IsNotNullOrEmpty(DescricaoAtividade, nameof(DescricaoAtividade), "O campo DescricaoAtividade e obrigatorio.");
            AddNotifications(contract);
        }

        public void MarcarAtualizado(string usuario) => MarcarAlterado(usuario);
    }
}
