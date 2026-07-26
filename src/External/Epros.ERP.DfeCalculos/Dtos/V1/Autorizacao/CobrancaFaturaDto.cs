namespace Epros.ERP.DfeCalculos.Dtos.V1.Autorizacao
{
    public class CobrancaFaturaDto
    {
        public CobrancaFaturaDto() { }
        public CobrancaFaturaDto(decimal valorOriginal, decimal valorDesconto, decimal valorLiquido, string? numeroFatura, ICollection<CobrancaDuplicata> duplicatas)
        {
            ValorOriginal = valorOriginal;
            ValorDesconto = valorDesconto;
            ValorLiquido = valorLiquido;
            NumeroFatura = numeroFatura;
            Duplicatas = duplicatas;
        }

        public decimal ValorOriginal { get; set; }
        public decimal ValorDesconto { get; set; }
        public decimal ValorLiquido { get; set; }
        public string? NumeroFatura { get; set; }
        public ICollection<CobrancaDuplicata> Duplicatas { get; set; } = null!;
    }
}
