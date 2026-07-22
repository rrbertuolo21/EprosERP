namespace Epros.ERP.DfeCalculos.Dtos.V1.Autorizacao
{
    public class CobrancaDuplicataDto
    {
        public CobrancaDuplicataDto() { }

        public CobrancaDuplicataDto(string numeroDuplicata, DateTime dataVencimento, decimal valorDuplicata)
        {
            NumeroDuplicata = numeroDuplicata;
            DataVencimento = dataVencimento;
            ValorDuplicata = valorDuplicata;
        }

        public string NumeroDuplicata { get; set; } = null!;
        public DateTime DataVencimento { get; set; }
        public decimal ValorDuplicata { get; set; }
    }
}
