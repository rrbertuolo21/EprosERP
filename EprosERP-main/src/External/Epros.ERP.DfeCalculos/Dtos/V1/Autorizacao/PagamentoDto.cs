namespace Epros.ERP.DfeCalculos.Dtos.V1.Autorizacao
{
    public class PagamentoDto
    {
        public PagamentoDto() { }
        public PagamentoDto(decimal valorPago, decimal valorTroco, int formaPagamento, int indicacaoPagamento, string? descricao, PagamentoCartaoDto? cartao)
        {
            ValorPago = valorPago;
            ValorTroco = valorTroco;
            FormaPagamento = formaPagamento;
            IndicacaoPagamento = indicacaoPagamento;
            Descricao = descricao;
            Cartao = cartao;
        }

        public decimal ValorPago { get; set; }
        public decimal ValorTroco { get; set; }
        public int FormaPagamento { get; set; }
        public int IndicacaoPagamento { get; set; }
        public string? Descricao { get; set; }
        public PagamentoCartaoDto? Cartao { get; set; }
    }
}