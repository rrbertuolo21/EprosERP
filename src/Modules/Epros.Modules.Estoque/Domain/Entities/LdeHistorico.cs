using System;
using Epros.Modules.Estoque.Domain.Enums;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Estoque.Domain.Entities
{
    /// <summary>
    /// Histórico operacional e de auditoria da entrada (EF Logística de Entrada §15.8 `lde_historico`).
    /// LDE-021: histórico de transição é imutável (somente inserção).
    /// </summary>
    public class LdeHistorico : EntidadeSaaSBase
    {
        public Guid EntradaId { get; private set; }
        public string Evento { get; private set; } = string.Empty;
        public ESituacaoEntradaLogistica? SituacaoAnterior { get; private set; }
        public ESituacaoEntradaLogistica SituacaoNova { get; private set; }
        public string? Motivo { get; private set; }
        public string UsuarioId { get; private set; } = string.Empty;

        protected LdeHistorico() { } // EF Core

        public LdeHistorico(Guid entradaId, string evento, ESituacaoEntradaLogistica? situacaoAnterior, ESituacaoEntradaLogistica situacaoNova, string? motivo, string usuarioId, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            EntradaId = entradaId;
            Evento = evento ?? string.Empty;
            SituacaoAnterior = situacaoAnterior;
            SituacaoNova = situacaoNova;
            Motivo = motivo;
            UsuarioId = usuarioId ?? string.Empty;
            Validar();
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<LdeHistorico>()
                .Requires()
                .AreNotEquals(EntradaId, Guid.Empty, nameof(EntradaId), "A entrada do histórico é obrigatória [Origem: LdeHistorico]")
                .IsNotNullOrEmpty(Evento, nameof(Evento), "O evento do histórico é obrigatório [Origem: LdeHistorico]"));
        }
    }
}
