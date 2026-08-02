using System;
using System.Linq;
using System.Text.Json;

namespace Epros.Modules.Agricultor.Domain.Services
{
    /// <summary>
    /// AGR-D05 — cálculo da área do talhão NO BACK-END a partir do polígono GeoJSON (não confiar no
    /// turf.js do cliente). Unidade canônica = m². Implementação: fórmula do "shoelace" (Gauss) sobre
    /// o anel exterior, com projeção equiretangular local (lon/lat → metros) usando o raio médio da
    /// Terra e a latitude média do polígono. É uma aproximação planar adequada a talhões (áreas
    /// pequenas); substituível por NetTopologySuite/geodésica sem mudar o contrato. Não é regra fiscal.
    /// </summary>
    public static class CalculadoraAreaTalhao
    {
        private const double RaioTerraMetros = 6_371_008.8; // raio médio IUGG

        /// <summary>
        /// Retorna a área em m² a partir de um GeoJSON (geometry Polygon ou Feature/FeatureCollection
        /// com Polygon). Retorna null quando não há polígono válido — o chamador decide o fallback.
        /// </summary>
        public static decimal? CalcularM2(string? geoJson)
        {
            if (string.IsNullOrWhiteSpace(geoJson)) return null;

            try
            {
                using var doc = JsonDocument.Parse(geoJson);
                var ring = LocalizarAnelExterior(doc.RootElement);
                if (ring == null) return null;

                var pontos = ring.Value.EnumerateArray()
                    .Select(p =>
                    {
                        var arr = p.EnumerateArray().ToArray();
                        return (lon: arr[0].GetDouble(), lat: arr[1].GetDouble());
                    })
                    .ToArray();

                if (pontos.Length < 3) return null;

                var latMedia = pontos.Average(p => p.lat) * Math.PI / 180.0;
                var cosLat = Math.Cos(latMedia);
                double metrosPorGrau = RaioTerraMetros * Math.PI / 180.0;

                // Projeta (lon,lat) → (x,y) em metros na tangente local.
                var xy = pontos.Select(p => (
                    x: p.lon * metrosPorGrau * cosLat,
                    y: p.lat * metrosPorGrau)).ToArray();

                double soma = 0;
                for (int i = 0; i < xy.Length; i++)
                {
                    var (x1, y1) = xy[i];
                    var (x2, y2) = xy[(i + 1) % xy.Length];
                    soma += (x1 * y2) - (x2 * y1);
                }

                var area = Math.Abs(soma) / 2.0;
                if (double.IsNaN(area) || double.IsInfinity(area)) return null;
                return Math.Round((decimal)area, 2);
            }
            catch
            {
                // GeoJSON malformado não deve quebrar o cadastro; cai no fallback do chamador.
                return null;
            }
        }

        private static JsonElement? LocalizarAnelExterior(JsonElement el)
        {
            // Feature → geometry
            if (el.ValueKind == JsonValueKind.Object && el.TryGetProperty("geometry", out var geom))
                return LocalizarAnelExterior(geom);

            // FeatureCollection → primeira feature
            if (el.ValueKind == JsonValueKind.Object && el.TryGetProperty("features", out var feats)
                && feats.ValueKind == JsonValueKind.Array && feats.GetArrayLength() > 0)
                return LocalizarAnelExterior(feats[0]);

            if (el.ValueKind == JsonValueKind.Object
                && el.TryGetProperty("type", out var tipo)
                && el.TryGetProperty("coordinates", out var coords))
            {
                var t = tipo.GetString();
                if (t == "Polygon" && coords.ValueKind == JsonValueKind.Array && coords.GetArrayLength() > 0)
                    return coords[0]; // anel exterior
                if (t == "MultiPolygon" && coords.ValueKind == JsonValueKind.Array && coords.GetArrayLength() > 0)
                    return coords[0][0];
            }

            return null;
        }
    }
}
