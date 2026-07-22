namespace Epros.ERP.DfeCalculos.Dtos.V1.Autorizacao
{
    public class VendaItemCombustivelDto
    {
        public string CodigoAnp { get; set; } = null!;
        public string DescricaoAnp { get; set; } = null!;
        public decimal? PMixGN { get; set; }
        public decimal? PGLP { get; set; }
        public decimal? PGNn { get; set; }
        public decimal? PGNi { get; set; }
        public decimal? VPart { get; set; }
        public string? CODIF { get; set; }
        public decimal? QTemp { get; set; }
        public string UFCons { get; set; } = null!;
        public decimal? PBio { get; set; }

        public ICollection<VendaItemCombustivelOrigemCombustivelDto> Origens { get; set; } = null!;
    }
}