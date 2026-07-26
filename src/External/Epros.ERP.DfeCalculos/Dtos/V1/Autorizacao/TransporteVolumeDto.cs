namespace Epros.ERP.DfeCalculos.Dtos.V1.Autorizacao
{
    public class TransporteVolumeDto
    {
        public TransporteVolumeDto() { }

        public TransporteVolumeDto(int quantidadeVolumes, string? especie, string? numeroVolumes, decimal pesoLiquido, decimal pesoBruto, string marca)
        {
            QuantidadeVolumes = quantidadeVolumes;
            Especie = especie;
            NumeroVolumes = numeroVolumes;
            PesoLiquido = pesoLiquido;
            PesoBruto = pesoBruto;
            Marca = marca;
        }

        public int QuantidadeVolumes { get; set; }
        public string? Especie { get; set; }
        public string? NumeroVolumes { get; set; }
        public decimal PesoLiquido { get; set; }
        public decimal PesoBruto { get; set; }
        public string? Marca { get; set; }
    }
}
