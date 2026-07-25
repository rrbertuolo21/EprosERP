using System;
using Epros.Shared.Domain.Entities;

namespace Epros.Modules.Estoque.Domain.Entities
{
    /// <summary>
    /// Auditoria funcional da subcontratação (EF Subcontratação §6 `sub_historico`). SUB-012: registra usuário,
    /// data, antes e depois. Modelo proposto por autoria (§16).
    /// </summary>
    public class SubHistorico : EntidadeSaaSBase
    {
        public Guid OrdemId { get; private set; }
        public string Evento { get; private set; } = string.Empty;
        public string? DadosAnteriores { get; private set; }
        public string? DadosNovos { get; private set; }
        public Guid? UsuarioId { get; private set; }
        public DateTime DataEvento { get; private set; }

        protected SubHistorico() { } // EF Core

        public SubHistorico(Guid ordemId, string evento, string? dadosAnteriores, string? dadosNovos, Guid? usuarioId, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            OrdemId = ordemId;
            Evento = evento;
            DadosAnteriores = dadosAnteriores;
            DadosNovos = dadosNovos;
            UsuarioId = usuarioId;
            DataEvento = DateTime.UtcNow;
        }
    }
}
