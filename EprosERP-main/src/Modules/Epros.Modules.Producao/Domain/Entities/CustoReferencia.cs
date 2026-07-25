using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Producao.Domain.Entities
{
    /// <summary>PRD-CST — Valores de custo por referência produtiva (prd_cst_custo_referencia).</summary>
    public class CustoReferencia : EntidadeSaaSBase
    {
        public Guid CustoProducaoId { get; private set; }
        public string TipoReferencia { get; private set; } = string.Empty;
        public Guid? ReferenciaId { get; private set; }
        public decimal? CustoPrevisto { get; private set; }
        public decimal? CustoRealizado { get; private set; }
        public decimal? CustoExtra { get; private set; }
        public string? TipoCustoProducao { get; private set; }
        public decimal? PercentualCustoProducao { get; private set; }
        public decimal? Desvio { get; private set; }

        protected CustoReferencia() { } // EF Core

        public CustoReferencia(
            Guid custoProducaoId,
            string tipoReferencia,
            string tenantId,
            string criadoPor,
            Guid? referenciaId = null,
            decimal? custoPrevisto = null,
            decimal? custoRealizado = null,
            decimal? custoExtra = null,
            string? tipoCustoProducao = null,
            decimal? percentualCustoProducao = null)
            : base(tenantId, criadoPor)
        {
            CustoProducaoId = custoProducaoId;
            TipoReferencia = tipoReferencia;
            ReferenciaId = referenciaId;
            CustoPrevisto = custoPrevisto;
            CustoRealizado = custoRealizado;
            CustoExtra = custoExtra;
            TipoCustoProducao = tipoCustoProducao;
            PercentualCustoProducao = percentualCustoProducao;
            RecalcularDesvio();

            AddNotifications(new Contract<CustoReferencia>()
                .Requires()
                .AreNotEquals(custoProducaoId, Guid.Empty, nameof(CustoProducaoId), "O custo de produção é obrigatório [Origem: CustoReferencia].")
                .IsNotNullOrEmpty(tipoReferencia, nameof(TipoReferencia), "O tipo da referência é obrigatório [Origem: CustoReferencia].")
            );
        }

        public void RecalcularDesvio()
        {
            if (CustoPrevisto.HasValue || CustoRealizado.HasValue)
            {
                Desvio = CustoRealizado.GetValueOrDefault(0m) - CustoPrevisto.GetValueOrDefault(0m);
            }
        }
    }
}
