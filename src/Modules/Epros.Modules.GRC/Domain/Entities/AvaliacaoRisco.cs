using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.GRC.Domain.Entities
{
    /// <summary>
    /// GRC-RIS — Avaliacao do risco (grc_ris_avaliacao). Registra probabilidade, impacto, score
    /// e justificativa por avaliacao (matriz probabilidade x impacto). Aditiva ao RiscoCorporativo
    /// ja existente. Fiel a EF_13_GRC_GESTAO_DE_RISCOS_CORPORATIVOS_V1 (secao 10.2).
    /// </summary>
    public class AvaliacaoRisco : EntidadeSaaSBase
    {
        public Guid RiscoId { get; private set; }
        public int Probabilidade { get; private set; } // 1 a 5
        public int Impacto { get; private set; } // 1 a 5
        public int Score { get; private set; } // Probabilidade * Impacto
        public string? Justificativa { get; private set; }
        public DateTime DataAvaliacao { get; private set; }

        protected AvaliacaoRisco() { } // EF Core

        public AvaliacaoRisco(
            Guid riscoId,
            int probabilidade,
            int impacto,
            string? justificativa,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<AvaliacaoRisco>()
                .Requires()
                .IsTrue(riscoId != Guid.Empty, nameof(RiscoId), "O risco da avaliacao e obrigatorio.")
                .IsTrue(probabilidade >= 1 && probabilidade <= 5, nameof(Probabilidade), "A probabilidade deve estar entre 1 e 5.")
                .IsTrue(impacto >= 1 && impacto <= 5, nameof(Impacto), "O impacto deve estar entre 1 e 5.")
            );

            RiscoId = riscoId;
            Probabilidade = probabilidade;
            Impacto = impacto;
            Score = probabilidade * impacto;
            Justificativa = justificativa;
            DataAvaliacao = DateTime.UtcNow;
        }
    }
}
