using System;
using Epros.Modules.Projetos.Domain.Enums;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Projetos.Domain.Entities.Risco
{
    /// <summary>
    /// Trilha de auditoria do risco/issue. Origem: EF PRJ-RSK 11.4 (prj_risco_historico).
    /// RN-RSK-016: acoes relevantes registram historico. Registro imutavel.
    /// </summary>
    public class HistoricoRisco : EntidadeSaaSBase
    {
        public Guid RiscoId { get; private set; }
        public EAcaoRisco Acao { get; private set; }
        public Guid UsuarioId { get; private set; }
        public string? PayloadJson { get; private set; }
        public string? Ip { get; private set; }
        public DateTime DataHora { get; private set; }

        protected HistoricoRisco() { } // EF Core

        public HistoricoRisco(
            Guid riscoId,
            EAcaoRisco acao,
            Guid usuarioId,
            string? payloadJson,
            string? ip,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<HistoricoRisco>()
                .Requires()
                .AreNotEquals(riscoId, Guid.Empty, nameof(RiscoId), "O risco e obrigatorio. [Origem: HistoricoRisco]"));

            RiscoId = riscoId;
            Acao = acao;
            UsuarioId = usuarioId;
            PayloadJson = payloadJson;
            Ip = ip;
            DataHora = DateTime.UtcNow;
        }
    }
}
