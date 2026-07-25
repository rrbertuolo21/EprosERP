using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.ESG.Domain.Entities
{
    /// <summary>Vinculo de um item a um dado de outro dominio, sem duplicar o mestre (EF RELATORIOS_ESG 11.4).</summary>
    public class IndicadorReferencia : EntidadeSaaSBase
    {
        public Guid ItemId { get; private set; }
        public string OrigemDominio { get; private set; } = string.Empty;
        public string OrigemEntidade { get; private set; } = string.Empty;
        public string OrigemId { get; private set; } = string.Empty;
        public string CodigoIndicador { get; private set; } = string.Empty;
        public string? RegraComposicao { get; private set; }

        protected IndicadorReferencia() { } // EF Core

        public IndicadorReferencia(
            Guid itemId,
            string origemDominio,
            string origemEntidade,
            string origemId,
            string codigoIndicador,
            string? regraComposicao,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            ItemId = itemId;
            OrigemDominio = origemDominio;
            OrigemEntidade = origemEntidade;
            OrigemId = origemId;
            CodigoIndicador = codigoIndicador;
            RegraComposicao = regraComposicao;
            Validar();
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<IndicadorReferencia>()
                .Requires()
                .AreNotEquals(ItemId, Guid.Empty, nameof(ItemId), "O item do relatorio e obrigatorio. [Origem: IndicadorReferencia]")
                .IsNotNullOrEmpty(OrigemDominio, nameof(OrigemDominio), "O dominio de origem e obrigatorio. [Origem: IndicadorReferencia]")
                .IsNotNullOrEmpty(OrigemId, nameof(OrigemId), "A chave externa de origem e obrigatoria. [Origem: IndicadorReferencia]")
                .IsNotNullOrEmpty(CodigoIndicador, nameof(CodigoIndicador), "O codigo do indicador e obrigatorio. [Origem: IndicadorReferencia]"));
        }
    }
}
