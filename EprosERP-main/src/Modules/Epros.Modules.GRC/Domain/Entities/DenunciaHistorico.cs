using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.GRC.Domain.Entities
{
    /// <summary>
    /// GRC-DEN — Historico (grc_den_historico). Trilha auditavel do caso: usuario, IP, timestamp,
    /// payload e antes/depois. Fiel a EF_13_GRC_INVESTIGACOES_E_DENUNCIAS_V1 (secao 11.7).
    /// RN-DEN-008: alteracoes relevantes devem registrar usuario, IP, timestamp e payload.
    /// </summary>
    public class DenunciaHistorico : EntidadeSaaSBase
    {
        public Guid DenunciaId { get; private set; }
        public string Acao { get; private set; } = string.Empty;
        public Guid? UsuarioId { get; private set; }
        public string? Ip { get; private set; }
        public DateTime DataHora { get; private set; }
        public string? PayloadJson { get; private set; }
        public string? Justificativa { get; private set; }

        protected DenunciaHistorico() { } // EF Core

        public DenunciaHistorico(
            Guid denunciaId,
            string acao,
            Guid? usuarioId,
            string? ip,
            string? payloadJson,
            string? justificativa,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<DenunciaHistorico>()
                .Requires()
                .IsTrue(denunciaId != Guid.Empty, nameof(DenunciaId), "A denúncia do histórico é obrigatória.")
                .IsNotNullOrEmpty(acao, nameof(Acao), "A ação do histórico é obrigatória.")
            );

            DenunciaId = denunciaId;
            Acao = acao;
            UsuarioId = usuarioId;
            Ip = ip;
            PayloadJson = payloadJson;
            Justificativa = justificativa;
            DataHora = DateTime.UtcNow;
        }
    }
}
