using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.GRC.Domain.Entities
{
    /// <summary>
    /// GRC-POL — Aceite de politica (grc_pol_aceite). Registro imutavel de ciencia/adesao
    /// do publico-alvo a uma versao publicada. Fiel a EF_13_GRC_GESTAO_DE_POLITICAS_V1 (secao 11.4).
    /// RN-POL-006: aceite deve apontar para politica, versao, usuario, data/hora, IP e tenant.
    /// Unicidade funcional: (tenant_id, versao_id, usuario_id).
    /// </summary>
    public class PoliticaAceite : EntidadeSaaSBase
    {
        public Guid PoliticaId { get; private set; }
        public Guid VersaoId { get; private set; }
        public Guid UsuarioId { get; private set; }
        public DateTime DataHoraAceite { get; private set; }
        public string Ip { get; private set; } = string.Empty;
        public string? Origem { get; private set; } // Portal, Mobile, Integracao...
        public string? DeclaracaoAceite { get; private set; }
        public string Status { get; private set; } = "Aceito"; // Aceito, Revogado, Contestado

        protected PoliticaAceite() { } // EF Core

        public PoliticaAceite(
            Guid politicaId,
            Guid versaoId,
            Guid usuarioId,
            string ip,
            string? origem,
            string? declaracaoAceite,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<PoliticaAceite>()
                .Requires()
                .IsTrue(politicaId != Guid.Empty, nameof(PoliticaId), "A politica do aceite e obrigatoria.")
                .IsTrue(versaoId != Guid.Empty, nameof(VersaoId), "A versao do aceite e obrigatoria.")
                .IsTrue(usuarioId != Guid.Empty, nameof(UsuarioId), "O usuario do aceite e obrigatorio.")
                .IsNotNullOrEmpty(ip, nameof(Ip), "O IP do aceite e obrigatorio (RN-POL-006).")
            );

            PoliticaId = politicaId;
            VersaoId = versaoId;
            UsuarioId = usuarioId;
            Ip = ip;
            Origem = origem;
            DeclaracaoAceite = declaracaoAceite;
            DataHoraAceite = DateTime.UtcNow;
            Status = "Aceito";
        }
    }
}
