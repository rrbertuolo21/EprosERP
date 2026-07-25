using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.GRC.Domain.Entities
{
    /// <summary>
    /// GRC-RIS — Leitura de KRI (grc_ris_kri_leitura). Valor periodico do indicador para
    /// dashboard e alertas. Fiel a EF_13_GRC_GESTAO_DE_RISCOS_CORPORATIVOS_V1 (secao 10.2).
    /// </summary>
    public class KriLeitura : EntidadeSaaSBase
    {
        public Guid KriId { get; private set; }
        public decimal Valor { get; private set; }
        public DateTime DataLeitura { get; private set; }
        public string Origem { get; private set; } = "Manual"; // Manual, Agendada
        // Normal, Alerta, Critico (derivado dos limites do KRI, calculado no handler)
        public string? Situacao { get; private set; }

        protected KriLeitura() { } // EF Core

        public KriLeitura(
            Guid kriId,
            decimal valor,
            string origem,
            string? situacao,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<KriLeitura>()
                .Requires()
                .IsTrue(kriId != Guid.Empty, nameof(KriId), "O KRI da leitura e obrigatorio.")
                .IsTrue(origem == "Manual" || origem == "Agendada", nameof(Origem), "A origem deve ser 'Manual' ou 'Agendada'.")
            );

            KriId = kriId;
            Valor = valor;
            Origem = origem;
            Situacao = situacao;
            DataLeitura = DateTime.UtcNow;
        }
    }
}
