using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.ESG.Domain.Entities
{
    /// <summary>Valor imutavel capturado de um indicador referenciado (EF RELATORIOS_ESG 11.4).</summary>
    public class SnapshotIndicador : EntidadeSaaSBase
    {
        public Guid IndicadorReferenciaId { get; private set; }
        public string OrigemVersao { get; private set; } = string.Empty;
        public DateTime DataCorte { get; private set; }
        public decimal? ValorNumerico { get; private set; }
        public string? ValorTexto { get; private set; }
        public string? Unidade { get; private set; }
        public string? Dimensoes { get; private set; }
        public string StatusOrigem { get; private set; } = string.Empty;
        public string HashConteudo { get; private set; } = string.Empty;

        protected SnapshotIndicador() { } // EF Core

        public SnapshotIndicador(
            Guid indicadorReferenciaId,
            string origemVersao,
            DateTime dataCorte,
            decimal? valorNumerico,
            string? valorTexto,
            string? unidade,
            string? dimensoes,
            string statusOrigem,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            IndicadorReferenciaId = indicadorReferenciaId;
            OrigemVersao = origemVersao;
            DataCorte = dataCorte;
            ValorNumerico = valorNumerico;
            ValorTexto = valorTexto;
            Unidade = unidade;
            Dimensoes = dimensoes;
            StatusOrigem = statusOrigem;
            HashConteudo = GerarHash();
            Validar();
        }

        private string GerarHash()
        {
            var conteudo = $"{IndicadorReferenciaId}|{OrigemVersao}|{DataCorte:O}|{ValorNumerico}|{ValorTexto}|{Unidade}";
            return conteudo.GetHashCode().ToString("X8");
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<SnapshotIndicador>()
                .Requires()
                .AreNotEquals(IndicadorReferenciaId, Guid.Empty, nameof(IndicadorReferenciaId), "O indicador referenciado e obrigatorio. [Origem: SnapshotIndicador]")
                .IsNotNullOrEmpty(OrigemVersao, nameof(OrigemVersao), "A versao de origem e obrigatoria. [Origem: SnapshotIndicador]")
                .IsTrue(ValorNumerico.HasValue || !string.IsNullOrWhiteSpace(ValorTexto), nameof(ValorNumerico),
                    "O snapshot deve ter valor numerico ou textual. [Origem: SnapshotIndicador]"));
        }
    }
}
