using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.GRC.Domain.Entities
{
    /// <summary>
    /// GRC-SOD — Violacao SoD (grc_sod_violacao). Registra um conflito detectado por simulacao
    /// ou reconciliacao. Fiel a EF_13_GRC_SEGREGACAO_DE_FUNCOES_SOD_V1 (secoes 10.2 e 12).
    /// </summary>
    public class ViolacaoSoD : EntidadeSaaSBase
    {
        public Guid? SimulacaoId { get; private set; }
        public Guid RegraId { get; private set; }
        public Guid? PerfilId { get; private set; }
        public Guid? UsuarioId { get; private set; }
        // Ativa, Mitigada, Resolvida, Vencida
        public string Status { get; private set; } = "Ativa";
        public DateTime DataDeteccao { get; private set; }

        protected ViolacaoSoD() { } // EF Core

        public ViolacaoSoD(
            Guid? simulacaoId,
            Guid regraId,
            Guid? perfilId,
            Guid? usuarioId,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<ViolacaoSoD>()
                .Requires()
                .IsTrue(regraId != Guid.Empty, nameof(RegraId), "A regra da violacao e obrigatoria.")
                .IsTrue(perfilId != null || usuarioId != null, nameof(PerfilId),
                    "A violacao deve estar associada a um perfil ou usuario.")
            );

            SimulacaoId = simulacaoId;
            RegraId = regraId;
            PerfilId = perfilId;
            UsuarioId = usuarioId;
            Status = "Ativa";
            DataDeteccao = DateTime.UtcNow;
        }

        /// <summary>Excecao vigente mitiga a violacao.</summary>
        public void Mitigar(string usuario)
        {
            if (Status != "Ativa")
            {
                AddNotification(nameof(Status), "Somente violacoes ativas podem ser mitigadas.");
                return;
            }
            Status = "Mitigada";
            MarcarAlterado(usuario);
        }

        /// <summary>Excecao expirada faz a violacao voltar a ativa.</summary>
        public void Reativar(string usuario)
        {
            if (Status != "Mitigada")
            {
                AddNotification(nameof(Status), "Somente violacoes mitigadas podem voltar a ativa.");
                return;
            }
            Status = "Ativa";
            MarcarAlterado(usuario);
        }

        public void Resolver(string usuario)
        {
            if (Status == "Resolvida")
            {
                AddNotification(nameof(Status), "A violacao ja esta resolvida.");
                return;
            }
            Status = "Resolvida";
            MarcarAlterado(usuario);
        }
    }
}
