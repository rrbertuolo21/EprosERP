namespace Epros.ERP.DfeCalculos.Dtos.V1.Autorizacao
{
    public class PagamentoCartaoDto
    {
        public int? TipoIntegracaoPagamento { get; set; }
        public int? BandeiraCartao { get; set; }
        public string? NumeroAutorizacaoOperaCartao { get; set; }
    }
}
