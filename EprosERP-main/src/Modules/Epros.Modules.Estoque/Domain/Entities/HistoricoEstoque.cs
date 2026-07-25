using System;
using Epros.Shared.Domain.Entities;

namespace Epros.Modules.Estoque.Domain.Entities
{
    /// <summary>
    /// Histórico de alterações e transições de registros de estoque (EF Movimentação Manual e Ajustes §15.15).
    /// Registro append-only de auditoria de eventos e mudanças de situação.
    /// </summary>
    public class HistoricoEstoque : EntidadeSaaSBase
    {
        public string Entidade { get; private set; } = string.Empty;
        public Guid EntidadeId { get; private set; }
        public string Evento { get; private set; } = string.Empty;
        public string? SituacaoAnterior { get; private set; }
        public string? SituacaoNova { get; private set; }
        public string? Motivo { get; private set; }
        public string? UsuarioId { get; private set; }

        protected HistoricoEstoque() { } // EF Core

        public HistoricoEstoque(string entidade, Guid entidadeId, string evento, string? situacaoAnterior, string? situacaoNova, string? motivo, string? usuarioId, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            Entidade = entidade ?? string.Empty;
            EntidadeId = entidadeId;
            Evento = evento ?? string.Empty;
            SituacaoAnterior = situacaoAnterior;
            SituacaoNova = situacaoNova;
            Motivo = motivo;
            UsuarioId = usuarioId;
        }

        public void Validar() { }
    }
}
