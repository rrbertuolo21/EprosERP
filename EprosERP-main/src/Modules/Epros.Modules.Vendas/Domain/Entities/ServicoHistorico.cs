using System;
using Epros.Modules.Vendas.Domain.Enums;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Vendas.Domain.Entities
{
    /// <summary>
    /// Histórico/auditoria imutável do submódulo Gestão de Serviços (ven_servico_historico).
    /// Fonte: EF §13, §19.5 e §22. Registra criação, alteração, faturamento, cancelamento e estorno.
    /// </summary>
    public class ServicoHistorico : EntidadeSaaSBase
    {
        public EEntidadeHistoricoServico Entidade { get; private set; }
        public Guid EntidadeId { get; private set; }
        public DateTime DataHora { get; private set; }
        public string UsuarioId { get; private set; } = string.Empty;
        public string Evento { get; private set; } = string.Empty;
        public string? StatusAnterior { get; private set; }
        public string? StatusNovo { get; private set; }
        public string? ValoresAnteriores { get; private set; }
        public string? ValoresNovos { get; private set; }
        public string? TraceId { get; private set; }

        protected ServicoHistorico() { } // EF Core

        public ServicoHistorico(
            EEntidadeHistoricoServico entidade,
            Guid entidadeId,
            string usuarioId,
            string evento,
            string? statusAnterior,
            string? statusNovo,
            string? valoresAnteriores,
            string? valoresNovos,
            string? traceId,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            Entidade = entidade;
            EntidadeId = entidadeId;
            DataHora = DateTime.UtcNow;
            UsuarioId = usuarioId;
            Evento = evento;
            StatusAnterior = statusAnterior;
            StatusNovo = statusNovo;
            ValoresAnteriores = valoresAnteriores;
            ValoresNovos = valoresNovos;
            TraceId = traceId;

            AddNotifications(new Contract<ServicoHistorico>()
                .Requires()
                .AreNotEquals(entidadeId, Guid.Empty, nameof(EntidadeId), "O identificador da entidade é obrigatório. [Origem: ServicoHistorico]")
                .IsNotNullOrEmpty(evento, nameof(Evento), "O evento do histórico é obrigatório. [Origem: ServicoHistorico]"));
        }
    }
}
