using System;
using Epros.Modules.Agricultor.Domain.Services;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Agricultor.Domain.Entities
{
    /// <summary>
    /// Talhão (parcela) de uma propriedade rural. Reaproveitado do Agris (Fields).
    /// AGR-D05: a área é recalculada NO BACK a partir do polígono GeoJSON (unidade canônica m²;
    /// hectare derivado = area_m2 / 10000). Nunca confiar no valor calculado no cliente. Área > 0 bloqueante.
    /// Talhão/safra NÃO atravessam a fronteira do LCDPR (o arquivo agrega por COD_IMOVEL).
    /// </summary>
    public class Talhao : EntidadeSaaSBase
    {
        public Guid PropriedadeId { get; private set; }
        public Guid? CulturaId { get; private set; }   // cultura fixa (legado Agris); default N/A
        public string Nome { get; private set; } = string.Empty;
        public decimal AreaM2 { get; private set; }
        public string? PoligonoGeoJson { get; private set; }

        /// <summary>Hectare DERIVADO da área canônica em m² (AGR-D05).</summary>
        public decimal AreaHectares => AreaM2 / 10000m;

        protected Talhao() { } // EF Core

        public Talhao(
            string nome,
            Guid? culturaId,
            string? poligonoGeoJson,
            decimal? areaM2Informada,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            Nome = nome;
            CulturaId = culturaId;
            PoligonoGeoJson = poligonoGeoJson;
            // AGR-D05: recomputa a área no back a partir do polígono; só cai no valor informado
            // quando não há polígono (cadastro manual sem desenho).
            AreaM2 = CalculadoraAreaTalhao.CalcularM2(poligonoGeoJson) ?? areaM2Informada ?? 0m;
            Validar();
        }

        public void VincularAPropriedade(Guid propriedadeId) => PropriedadeId = propriedadeId;

        public void AtualizarPoligono(string? poligonoGeoJson, decimal? areaM2Informada, string usuario)
        {
            PoligonoGeoJson = poligonoGeoJson;
            AreaM2 = CalculadoraAreaTalhao.CalcularM2(poligonoGeoJson) ?? areaM2Informada ?? AreaM2;
            Validar();
            if (!IsValid) return;
            MarcarAlterado(usuario);
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<Talhao>()
                .Requires()
                .IsNotNullOrEmpty(Nome, nameof(Nome),
                    "O nome do talhão é obrigatório. [Origem: Talhao]")
                .IsGreaterThan(AreaM2, 0m, nameof(AreaM2),
                    "A área do talhão deve ser maior que zero (m²). [Origem: Talhao] (AGR-D05)"));
        }
    }
}
