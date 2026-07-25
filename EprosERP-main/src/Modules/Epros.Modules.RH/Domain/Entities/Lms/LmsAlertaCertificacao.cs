using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.RH.Domain.Entities
{
    /// <summary>Entidade portada da EF (submodulo RH-LMS, tabela rh_lms_alerta_certificacao). Fidelidade campo a campo.</summary>
    public partial class LmsAlertaCertificacao : EntidadeSaaSBase
    {
        public Guid CertificacaoId { get; private set; }
        public int DiasAntecedencia { get; private set; }
        public DateTime DataAlerta { get; private set; }
        public string Status { get; private set; } = string.Empty;

        protected LmsAlertaCertificacao() { } // EF Core

        public LmsAlertaCertificacao(
            Guid certificacaoId,
            int diasAntecedencia,
            DateTime dataAlerta,
            string status,
            string tenantId,
            string criadoPor
            )
            : base(tenantId, criadoPor)
        {
            CertificacaoId = certificacaoId;
            DiasAntecedencia = diasAntecedencia;
            DataAlerta = dataAlerta;
            Status = status;
            Validar();
        }

        public void Validar()
        {
            var contract = new Contract<LmsAlertaCertificacao>().Requires();
            contract.AreNotEquals(CertificacaoId, Guid.Empty, nameof(CertificacaoId), "O campo CertificacaoId e obrigatorio.");
            contract.IsNotNullOrEmpty(Status, nameof(Status), "O campo Status e obrigatorio.");
            AddNotifications(contract);
        }

        public void MarcarAtualizado(string usuario) => MarcarAlterado(usuario);
    }
}
