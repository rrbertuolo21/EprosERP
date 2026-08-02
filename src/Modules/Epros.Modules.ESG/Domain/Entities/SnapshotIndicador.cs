using System;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
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

        // TE-02 (RN-REL-012/013): hash SHA-256 sobre payload canonico
        // (valor+unidade+dimensoes+origem+versao+data). Deterministico e verificavel — garante
        // imutabilidade do snapshot. GetHashCode() NAO serve (nao e estavel entre processos).
        private string GerarHash()
        {
            var conteudo = string.Join("|", new[]
            {
                IndicadorReferenciaId.ToString(),
                OrigemVersao ?? string.Empty,
                DataCorte.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture),
                ValorNumerico?.ToString(CultureInfo.InvariantCulture) ?? string.Empty,
                ValorTexto ?? string.Empty,
                Unidade ?? string.Empty,
                Dimensoes ?? string.Empty,
                StatusOrigem ?? string.Empty
            });

            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(conteudo));
            var sb = new StringBuilder(bytes.Length * 2);
            foreach (var b in bytes) sb.Append(b.ToString("x2"));
            return sb.ToString();
        }

        /// <summary>RN-REL-013: recomputa o hash do conteudo atual e confirma que bate com o gravado.</summary>
        public bool HashConfere() => HashConteudo == GerarHash();

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
