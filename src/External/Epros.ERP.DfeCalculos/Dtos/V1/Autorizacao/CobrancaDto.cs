namespace Epros.ERP.DfeCalculos.Dtos.V1.Autorizacao
{
    public class CobrancaDto
    {
        public CobrancaDto() { }

        public CobrancaDto(decimal valorOriginal, decimal valorDesconto, decimal valorLiquido, string? numeroFatura, CobrancaDuplicataDto[] duplicatas)
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
        public CobrancaDuplicataDto[] Duplicatas { get; set; } = null!;
    }
}
