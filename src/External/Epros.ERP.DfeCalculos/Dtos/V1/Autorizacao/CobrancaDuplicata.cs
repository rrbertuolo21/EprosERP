namespace Epros.ERP.DfeCalculos.Dtos.V1.Autorizacao
{
    public class CobrancaDuplicata
    {
        public CobrancaDuplicata() { }
        public CobrancaDuplicata(string? numeroDuplicata, DateTime dataVencimento, decimal valorDuplicata)
        {
            NumeroDuplicata = numeroDuplicata;
            DataVencimento = dataVencimento;
            ValorDuplicata = valorDuplicata;
        }

        public string? NumeroDuplicata { get; set; }
        public DateTime DataVencimento { get; set; }
        public decimal ValorDuplicata { get; set; }
    }
}
