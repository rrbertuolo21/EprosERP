using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.RH.Domain.Entities
{
    /// <summary>Entidade portada da EF (submodulo RH-SSO). Fidelidade campo a campo.</summary>
    public partial class SsoIndicadorAcidente : EntidadeSaaSBase
    {
        public string Periodo { get; private set; } = string.Empty;
        public Guid? ColaboradorId { get; private set; }
        public Guid? CatId { get; private set; }
        public int QuantidadeAcidentes { get; private set; }
        public string? Observacao { get; private set; }

        protected SsoIndicadorAcidente() { } // EF Core

        public SsoIndicadorAcidente(
            string periodo,
            Guid? colaboradorId,
            Guid? catId,
            int quantidadeAcidentes,
            string? observacao,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            Periodo = periodo;
            ColaboradorId = colaboradorId;
            CatId = catId;
            QuantidadeAcidentes = quantidadeAcidentes;
            Observacao = observacao;
            Validar();
        }

        public void Validar()
        {
            var contract = new Contract<SsoIndicadorAcidente>().Requires();
            contract.IsNotNullOrEmpty(Periodo, nameof(Periodo), "O campo Periodo e obrigatorio.");
            AddNotifications(contract);
        }

        public void MarcarAtualizado(string usuario) => MarcarAlterado(usuario);
    }
}
