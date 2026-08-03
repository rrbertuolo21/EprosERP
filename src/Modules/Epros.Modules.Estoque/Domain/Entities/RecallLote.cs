using System;
using Epros.Modules.Estoque.Domain.Enums;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Estoque.Domain.Entities
{
    /// <summary>
    /// Evento de recall por lote (EF Rastreabilidade §7.7 `rlt_recall_lote`). RLT-008: lote em recall deve
    /// ser bloqueado para consumo; RLT-011: recall permite consultar documentos/clientes impactados.
    /// </summary>
    public class RecallLote : EntidadeSaaSBase
    {
        public Guid LoteId { get; private set; }
        public string? CodigoRecall { get; private set; }
        public string Motivo { get; private set; } = string.Empty;
        public EStatusRecallLote Status { get; private set; } = EStatusRecallLote.Aberto;
        public DateTime DataAbertura { get; private set; }
        public DateTime? DataEncerramento { get; private set; }

        protected RecallLote() { } // EF Core

        public RecallLote(Guid loteId, string? codigoRecall, string motivo, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            LoteId = loteId;
            CodigoRecall = codigoRecall;
            Motivo = motivo ?? string.Empty;
            Status = EStatusRecallLote.Aberto;
            DataAbertura = DateTime.UtcNow;
            Validar();
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<RecallLote>()
                .Requires()
                .IsNotEmpty(LoteId, nameof(LoteId), "O lote do recall é obrigatório [Origem: RecallLote]")
                .IsNotNullOrEmpty(Motivo, nameof(Motivo), "O motivo do recall é obrigatório [RLT-011] [Origem: RecallLote]"));
        }

        public void IniciarAndamento(string usuario) { Status = EStatusRecallLote.EmAndamento; MarcarAlterado(usuario); }
        public void Concluir(string usuario) { Status = EStatusRecallLote.Concluido; DataEncerramento = DateTime.UtcNow; MarcarAlterado(usuario); }
        public void Cancelar(string usuario) { Status = EStatusRecallLote.Cancelado; DataEncerramento = DateTime.UtcNow; MarcarAlterado(usuario); }
    }
}
