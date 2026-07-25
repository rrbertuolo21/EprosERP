using System;
using Epros.Modules.Estoque.Domain.Enums;
using Epros.Shared.Domain.Enums;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Estoque.Domain.Entities
{
    /// <summary>
    /// Pagamento de uma compra. Porte fiel do legado Epros.ERP.Domain.Entities.Compras.CompraPagamento.
    /// O ValueObject CNPJ do intermediador foi achatado para a string CartaoCnpjIntermediadorFinanceira.
    /// </summary>
    public class CompraPagamento : EntidadeSaaSBase
    {
        public Guid CompraId { get; private set; }
        public decimal ValorTroco { get; private set; }
        public EIndicadorPagamento IndicadorPagamento { get; private set; }
        public ETipoPagamento TipoPagamento { get; private set; }
        public decimal ValorPagamento { get; private set; }  // 15,2
        public ETipoIntegracaoPagamentoCartao CartaoTipoIntegracao { get; private set; }
        public string? CartaoCnpjIntermediadorFinanceira { get; private set; }  // 14
        public EBandeiraCartao CartaoBandeira { get; private set; }
        public string? CartaoCodigoAutorizacaoOperacao { get; private set; }  // 20

        // Navegação intra-módulo
        public Compra? Compra { get; private set; }

        protected CompraPagamento() { } // EF Core

        public CompraPagamento(Guid compraId, decimal valorTroco, ETipoPagamento tipoPagamento, decimal valorPagamento, ETipoIntegracaoPagamentoCartao cartaoTipoIntegracao, string? cartaoCnpjIntermediadorFinanceira, EBandeiraCartao cartaoBandeira, string? cartaoCodigoAutorizacaoOperacao, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            CompraId = compraId;
            ValorTroco = valorTroco;
            TipoPagamento = tipoPagamento;
            ValorPagamento = valorPagamento;
            CartaoTipoIntegracao = cartaoTipoIntegracao;
            CartaoCnpjIntermediadorFinanceira = cartaoCnpjIntermediadorFinanceira;
            CartaoBandeira = cartaoBandeira;
            CartaoCodigoAutorizacaoOperacao = cartaoCodigoAutorizacaoOperacao;
            Validar();
            PreencherIndicadorPagamento();
        }

        public void Validar()
        {
            AddNotifications(new Contract<CompraPagamento>()
                .Requires()
                .IsGreaterThan(ValorPagamento, decimal.Zero, nameof(ValorPagamento), "Valor Pagamento informado na compra inválido")
                .IsLowerOrEqualsThan((CartaoCodigoAutorizacaoOperacao ?? "").Length, 20, nameof(CartaoCodigoAutorizacaoOperacao), "Cartão Código Autorização Operação pode conter no max 20 caracteres")
            );
        }

        public void PreencherIndicadorPagamento()
        {
            if (TipoPagamento == ETipoPagamento.Dinheiro) IndicadorPagamento = EIndicadorPagamento.PagamentoAVista;

            if (TipoPagamento == ETipoPagamento.Cheque) IndicadorPagamento = EIndicadorPagamento.PagamentoAPrazo;

            if (TipoPagamento == ETipoPagamento.CartaoCredito) IndicadorPagamento = EIndicadorPagamento.PagamentoAVista;

            if (TipoPagamento == ETipoPagamento.CartaoDebito) IndicadorPagamento = EIndicadorPagamento.PagamentoAVista;

            if (TipoPagamento == ETipoPagamento.CartaoDaLoja) IndicadorPagamento = EIndicadorPagamento.PagamentoAPrazo;

            if (TipoPagamento == ETipoPagamento.ValeAlimentacao) IndicadorPagamento = EIndicadorPagamento.PagamentoAVista;

            if (TipoPagamento == ETipoPagamento.ValeRefeicao) IndicadorPagamento = EIndicadorPagamento.PagamentoAVista;

            if (TipoPagamento == ETipoPagamento.ValePresente) IndicadorPagamento = EIndicadorPagamento.PagamentoAVista;

            if (TipoPagamento == ETipoPagamento.ValeCombustivel) IndicadorPagamento = EIndicadorPagamento.PagamentoAVista;

            if (TipoPagamento == ETipoPagamento.DuplicataMercantil) IndicadorPagamento = EIndicadorPagamento.PagamentoAPrazo;

            if (TipoPagamento == ETipoPagamento.BoletoBancario) IndicadorPagamento = EIndicadorPagamento.PagamentoAPrazo;

            if (TipoPagamento == ETipoPagamento.DepositoBancario) IndicadorPagamento = EIndicadorPagamento.PagamentoAVista;

            if (TipoPagamento == ETipoPagamento.PagamentoInstantaneoPixDinamico) IndicadorPagamento = EIndicadorPagamento.PagamentoAVista;

            if (TipoPagamento == ETipoPagamento.TransferenciaBancaria) IndicadorPagamento = EIndicadorPagamento.PagamentoAVista;

            if (TipoPagamento == ETipoPagamento.ProgramaDeFidelidade) IndicadorPagamento = EIndicadorPagamento.PagamentoAVista;

            if (TipoPagamento == ETipoPagamento.PagamentoInstantaneoPixEstatico) IndicadorPagamento = EIndicadorPagamento.PagamentoAVista;

            if (TipoPagamento == ETipoPagamento.CreditoEmLoja) IndicadorPagamento = EIndicadorPagamento.PagamentoAPrazo;

            if (TipoPagamento == ETipoPagamento.PagamentoEletronicoNaoInformado) IndicadorPagamento = EIndicadorPagamento.PagamentoAVista;

            if (TipoPagamento == ETipoPagamento.SemPagamento) IndicadorPagamento = EIndicadorPagamento.PagamentoAVista;

            if (TipoPagamento == ETipoPagamento.Outros) IndicadorPagamento = EIndicadorPagamento.PagamentoAVista;
        }

        public void Alterar(decimal valorTroco, ETipoPagamento tipoPagamento, decimal valorPagamento, ETipoIntegracaoPagamentoCartao cartaoTipoIntegracao, string? cartaoCnpjIntermediadorFinanceira, EBandeiraCartao cartaoBandeira, string? cartaoCodigoAutorizacaoOperacao, string usuario)
        {
            ValorTroco = valorTroco;
            TipoPagamento = tipoPagamento;
            ValorPagamento = valorPagamento;
            CartaoTipoIntegracao = cartaoTipoIntegracao;
            CartaoCnpjIntermediadorFinanceira = cartaoCnpjIntermediadorFinanceira;
            CartaoBandeira = cartaoBandeira;
            CartaoCodigoAutorizacaoOperacao = cartaoCodigoAutorizacaoOperacao;
            MarcarAlterado(usuario);
            Validar();
            PreencherIndicadorPagamento();
        }
    }
}
