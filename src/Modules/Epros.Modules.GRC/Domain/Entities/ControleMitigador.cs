using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.GRC.Domain.Entities
{
    /// <summary>
    /// GRC-RIS — Controle mitigador (grc_ris_controle_mitigador). Vincula um risco a um
    /// controle interno (grc_cia_controle / ControleInterno) sem duplicar o cadastro.
    /// Fiel a EF_13_GRC_GESTAO_DE_RISCOS_CORPORATIVOS_V1 (secao 10.2/10.4).
    /// Unicidade: (RiscoId, ControleId).
    /// </summary>
    public class ControleMitigador : EntidadeSaaSBase
    {
        public Guid RiscoId { get; private set; }
        public Guid ControleId { get; private set; }
        public string? Efetividade { get; private set; } // Baixa, Media, Alta
        public string? Observacao { get; private set; }

        protected ControleMitigador() { } // EF Core

        public ControleMitigador(
            Guid riscoId,
            Guid controleId,
            string? efetividade,
            string? observacao,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<ControleMitigador>()
                .Requires()
                .IsTrue(riscoId != Guid.Empty, nameof(RiscoId), "O risco do vinculo e obrigatorio.")
                .IsTrue(controleId != Guid.Empty, nameof(ControleId), "O controle do vinculo e obrigatorio.")
                .IsTrue(efetividade == null || efetividade == "Baixa" || efetividade == "Media" || efetividade == "Alta",
                    nameof(Efetividade), "A efetividade deve ser 'Baixa', 'Media' ou 'Alta'.")
            );

            RiscoId = riscoId;
            ControleId = controleId;
            Efetividade = efetividade;
            Observacao = observacao;
        }
    }
}
