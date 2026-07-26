namespace Epros.ERP.DfeCalculos.Dtos.V1.Autorizacao
{
    public class TransporteVeiculoDto
    {
        public TransporteVeiculoDto() { }

        public TransporteVeiculoDto(string placa, string? uf, string? rntc)
        {
            Placa = placa;
            Uf = uf;
            Rntc = rntc;
        }

        public string Placa { get; set; } = null!;
        public string? Uf { get; set; }
        public string? Rntc { get; set; }
    }
}