namespace Epros.ERP.DfeCalculos.Dtos.V1.Autorizacao
{
    public class TransporteReboqueDto
    {
        public TransporteReboqueDto() { }

        public TransporteReboqueDto(string placa, string? uf, string? rntc)
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