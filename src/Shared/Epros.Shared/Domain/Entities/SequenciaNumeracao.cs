using Epros.Shared.Domain.Entities;

namespace Epros.Shared.Domain.Entities
{
    /// <summary>
    /// TRANSVERSAL T9 — armazenamento da sequência de numeração central por (tenant, tipo de documento).
    /// A concorrência/atomicidade é garantida no <see cref="Epros.Shared.Application.Contracts.INumeracaoService"/>
    /// (UPSERT atômico com RETURNING no Postgres), não por leitura-e-grava desta entidade — por isso o
    /// incremento NÃO deve passar pelo caminho de lock otimista (xmin) normal de leitura+save.
    /// Índice único (tenant_id, tipo_documento) garante uma linha por sequência.
    /// </summary>
    public class SequenciaNumeracao : EntidadeSaaSBase
    {
        /// <summary>Tipo/escopo da sequência (ex.: "pedido", "nfe", "contrato", "os").</summary>
        public string TipoDocumento { get; private set; } = string.Empty;

        /// <summary>Último valor JÁ entregue (o próximo entregue será ProximoValor + 1... ver serviço).</summary>
        public long UltimoValor { get; private set; }

        protected SequenciaNumeracao() { } // EF Core

        public SequenciaNumeracao(string tipoDocumento, long ultimoValor, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            TipoDocumento = tipoDocumento;
            UltimoValor = ultimoValor;
        }
    }
}
