using System.ComponentModel;

namespace Epros.ERP.Shared.Enums
{
    public enum ETipoPagamento
    {
        Dinheiro = 1,
        Cheque = 2,
        [Description("Cartão de Crédito")]
        CartaoCredito = 3,

        [Description("Cartão de Débito")]
        CartaoDebito = 4,

        [Description("Cartão da Loja")]
        CartaoDaLoja = 5,

        [Description("Vale Alimentacão")]
        ValeAlimentacao = 10,

        [Description("Vale Refeição")]
        ValeRefeicao = 11,

        [Description("Vale Presente")]
        ValePresente = 12,

        [Description("Vale Combustível")]
        ValeCombustivel = 13,

        [Description("Duplicata Mercantil")]
        DuplicataMercantil = 14,

        [Description("Boleto Bancário")]
        BoletoBancario = 15,

        [Description("Deposito Bancário")]
        DepositoBancario = 16,

        [Description("Pagamento Instantâneo (Pix) Dinâmico")]
        PagamentoInstantaneoPixDinamico = 17,

        [Description("Transferência Bancária, Carteira Digital")]
        TransferenciaBancaria = 18,

        [Description("Porgrama de Fidelidade, Cashback, Crédito Virtual")]
        ProgramaDeFidelidade = 19,

        [Description("Pagamento Instantâneo (Pix) Estático")]
        PagamentoInstantaneoPixEstatico = 20,

        [Description("Crédito em Loja")]
        CreditoEmLoja = 21,

        [Description("Pagamento Eletrônico Não Informado")]
        PagamentoEletronicoNaoInformado = 22,

        [Description("Sem Pagamentos")]
        SemPagamento = 90,

        Outros = 99,

        //Para atender o contas a receber

        Desconto = 991,

        [Description("Acréscimo")]
        Acrescimo = 992,

        Juros = 993,

        Multa = 994,

        Troco = 995
    }
}
