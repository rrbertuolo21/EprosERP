using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.GRC.Domain.Entities
{
    /// <summary>
    /// GRC-SOD — Registro de bypass de SoD (grc_sod_bypass_admin). D-SOD-04 / SOX §404:
    /// SoD vale ATÉ para o admin do tenant — não há bypass silencioso. Todo furo de SoD
    /// (inclusive pelo super-usuário) é registrado com quem, quando, motivo e o controle
    /// compensatório obrigatório, e dispara alerta (evento grc.sod.bypass_admin) + trilha central (T8).
    /// </summary>
    public class BypassSoD : EntidadeSaaSBase
    {
        public Guid RegraId { get; private set; }
        public Guid AtorId { get; private set; }
        public bool AtorEhAdmin { get; private set; }
        public string Motivo { get; private set; } = string.Empty;
        // D-SOD-02/04 — bypass exige controle compensatório (referência ou descrição).
        public Guid? ControleCompensatorioId { get; private set; }
        public string? ControleCompensatorio { get; private set; }
        public DateTime OcorridoEm { get; private set; }

        protected BypassSoD() { } // EF Core

        public BypassSoD(
            Guid regraId,
            Guid atorId,
            bool atorEhAdmin,
            string motivo,
            Guid? controleCompensatorioId,
            string? controleCompensatorio,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<BypassSoD>()
                .Requires()
                .IsTrue(regraId != Guid.Empty, nameof(RegraId), "A regra do bypass e obrigatoria.")
                .IsTrue(atorId != Guid.Empty, nameof(AtorId), "O ator do bypass e obrigatorio.")
                .IsNotNullOrEmpty(motivo, nameof(Motivo), "O motivo do bypass e obrigatorio.")
                // Nunca bypass silencioso e sem mitigação: controle compensatório obrigatório.
                .IsTrue(controleCompensatorioId != null || !string.IsNullOrWhiteSpace(controleCompensatorio),
                    nameof(ControleCompensatorio), "O bypass exige um controle compensatorio (referencia ou descricao).")
            );

            RegraId = regraId;
            AtorId = atorId;
            AtorEhAdmin = atorEhAdmin;
            Motivo = motivo;
            ControleCompensatorioId = controleCompensatorioId;
            ControleCompensatorio = controleCompensatorio;
            OcorridoEm = DateTime.UtcNow;
        }
    }
}
