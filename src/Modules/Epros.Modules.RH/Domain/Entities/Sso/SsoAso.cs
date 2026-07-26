using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.RH.Domain.Entities
{
    /// <summary>Entidade portada da EF (submodulo RH-SSO). Fidelidade campo a campo.</summary>
    public partial class SsoAso : EntidadeSaaSBase
    {
        public Guid ColaboradorId { get; private set; }
        public Guid? PppId { get; private set; }
        public string TipoAso { get; private set; } = string.Empty;
        public DateTime? DataAso { get; private set; }
        public string Resultado { get; private set; } = string.Empty;
        public string? Observacao { get; private set; }

        protected SsoAso() { } // EF Core

        public SsoAso(
            Guid colaboradorId,
            Guid? pppId,
            string tipoAso,
            DateTime? dataAso,
            string resultado,
            string? observacao,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            ColaboradorId = colaboradorId;
            PppId = pppId;
            TipoAso = tipoAso;
            DataAso = dataAso;
            Resultado = resultado;
            Observacao = observacao;
            Validar();
        }

        public void Validar()
        {
            var contract = new Contract<SsoAso>().Requires();
            contract.AreNotEquals(ColaboradorId, Guid.Empty, nameof(ColaboradorId), "O campo ColaboradorId e obrigatorio.");
            contract.IsNotNullOrEmpty(TipoAso, nameof(TipoAso), "O campo TipoAso e obrigatorio.");
            contract.IsNotNullOrEmpty(Resultado, nameof(Resultado), "O campo Resultado e obrigatorio.");
            AddNotifications(contract);
        }

        public void MarcarAtualizado(string usuario) => MarcarAlterado(usuario);
    }
}
