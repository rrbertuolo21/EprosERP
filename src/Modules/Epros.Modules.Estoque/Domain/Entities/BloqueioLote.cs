using System;
using Epros.Modules.Estoque.Domain.Enums;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Estoque.Domain.Entities
{
    /// <summary>
    /// Registro de bloqueio/desbloqueio de lote (EF Rastreabilidade §7.6 `rlt_bloqueio_lote`).
    /// RLT-012: bloqueio exige motivo e usuário; RLT-013: desbloqueio registra usuário e data.
    /// </summary>
    public class BloqueioLote : EntidadeSaaSBase
    {
        public Guid LoteId { get; private set; }
        public ETipoBloqueioLote TipoBloqueio { get; private set; }
        public string Motivo { get; private set; } = string.Empty;
        public DateTime DataBloqueio { get; private set; }
        public DateTime? DataDesbloqueio { get; private set; }
        public string? MotivoDesbloqueio { get; private set; }

        protected BloqueioLote() { } // EF Core

        public BloqueioLote(Guid loteId, ETipoBloqueioLote tipoBloqueio, string motivo, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            LoteId = loteId;
            TipoBloqueio = tipoBloqueio;
            Motivo = motivo ?? string.Empty;
            DataBloqueio = DateTime.UtcNow;
            Validar();
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<BloqueioLote>()
                .Requires()
                .IsNotEmpty(LoteId, nameof(LoteId), "O lote do bloqueio é obrigatório [Origem: BloqueioLote]")
                .IsNotNullOrEmpty(Motivo, nameof(Motivo), "O motivo do bloqueio é obrigatório [RLT-012] [Origem: BloqueioLote]"));
        }

        public void RegistrarDesbloqueio(string? motivo, string usuario)
        {
            DataDesbloqueio = DateTime.UtcNow;
            MotivoDesbloqueio = motivo;
            MarcarAlterado(usuario);
        }

        public bool Ativo => DataDesbloqueio == null;
    }
}
