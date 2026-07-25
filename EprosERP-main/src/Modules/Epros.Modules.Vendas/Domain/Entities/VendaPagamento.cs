using System;
using Epros.Modules.Vendas.Domain.Enums;
using Epros.Shared.Domain.Entities;
using Epros.Shared.Domain.Enums;
using Flunt.Notifications;
using Flunt.Validations;

namespace Epros.Modules.Vendas.Domain.Entities
{
    /// <summary>
    /// Porte fiel de VendaPagamento. FK long -> Guid; VO CNPJ -> string?; herda EntidadeSaaSBase.
    /// </summary>
    public class VendaPagamento : EntidadeSaaSBase
    {
        public Guid VendaId { get; private set; }
        public decimal ValorTroco { get; private set; }
        public EIndicadorPagamento IndicadorPagamento { get; private set; }
        public ETipoPagamento TipoPagamento { get; private set; }
        public decimal ValorPagamento { get; private set; }
        public ETipoIntegracaoPagamentoCArtao CartaoTipoIntegracao { get; private set; }
        public string? CartaoCnpjIntermediadorFinanceira { get; private set; }
        public EBandeiraCartao CartaoBandeira { get; private set; }
        public string? CartaoCodigoAutorizacaoOperacao { get; private set; }

        // Navegação intra-módulo
        public Venda Venda { get; private set; } = null!;

        protected VendaPagamento() { } // EF Core

        public VendaPagamento(Guid vendaId, decimal valorTroco, ETipoPagamento tipoPagamento, decimal valorPagamento, ETipoIntegracaoPagamentoCArtao cartaoTipoIntegracao, string? cartaoCnpjIntermediadorFinanceira, EBandeiraCartao cartaoBandeira, string? cartaoCodigoAutorizacaoOperacao,
            string tenantId, string criadoPor) : base(tenantId, criadoPor)
        {
            VendaId = vendaId;
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

        public void Alterar(decimal valorTroco, ETipoPagamento tipoPagamento, decimal valorPagamento, ETipoIntegracaoPagamentoCArtao cartaoTipoIntegracao, string? cartaoCnpjIntermediadorFinanceira, EBandeiraCartao cartaoBandeira, string? cartaoCodigoAutorizacaoOperacao, string alteradoPor)
        {
            ValorTroco = valorTroco;
            TipoPagamento = tipoPagamento;
            ValorPagamento = valorPagamento;
            CartaoTipoIntegracao = cartaoTipoIntegracao;
            CartaoCnpjIntermediadorFinanceira = cartaoCnpjIntermediadorFinanceira;
            CartaoBandeira = cartaoBandeira;
            CartaoCodigoAutorizacaoOperacao = cartaoCodigoAutorizacaoOperacao;
            MarcarAlterado(alteradoPor);
            Validar();
            PreencherIndicadorPagamento();
        }

        /// <summary>Porte fiel de VendaPagamento.Validar.</summary>
        public void Validar()
        {
            AddNotifications(new Contract<Notification>()
                .Requires()
                .IsLowerOrEqualsThan(ValorPagamento, decimal.Zero, "ValorPagamento", "Valor Pagamento informado na venda inválido")
                .IsLowerOrEqualsThan(20, (CartaoCodigoAutorizacaoOperacao ?? "").Length, "CartaoCodigoAutorizacaoOperacao", "Cartão Código Autorização Operação pode conter no max 20 caracteres")
            );

            if (!Enum.IsDefined(typeof(ETipoPagamento), TipoPagamento))
                AddNotification("TipoPagamento", "Tipo Pagamento informado na venda inválido");

            if (!Enum.IsDefined(typeof(ETipoIntegracaoPagamentoCArtao), CartaoTipoIntegracao))
                AddNotification("CartaoTipoIntegracao", "Cartão Tipo Integração informado na venda inválido");

            if (!Enum.IsDefined(typeof(EBandeiraCartao), CartaoBandeira))
                AddNotification("CartaoBandeira", "Cartão Bandeira informado na venda inválido");
        }

        /// <summary>Porte fiel de VendaPagamento.PreencherIndicadorPagamento.</summary>
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

        /// <summary>Porte fiel de VendaPagamento.Duplicar (novo Id/FK).</summary>
        public VendaPagamento Duplicar(Guid novaVendaId, string criadoPor)
            => new(novaVendaId, ValorTroco, TipoPagamento, ValorPagamento, CartaoTipoIntegracao,
                   CartaoCnpjIntermediadorFinanceira, CartaoBandeira, CartaoCodigoAutorizacaoOperacao, TenantId, criadoPor);
    }
}
