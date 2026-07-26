using System.ComponentModel;

namespace Epros.Modules.Estoque.Domain.Enums
{
    /// <summary>
    /// Origem funcional do fato gerador de estoque. Taxonomia normalizada para o Epros conforme
    /// EF Movimentação Manual e Ajustes §9 (Domínios e listas de valores).
    /// </summary>
    public enum EOrigemFatoGeradorEstoque
    {
        [Description("Movimento manual")]
        MovimentoManual = 0,

        [Description("Entrada fiscal")]
        EntradaFiscal = 1,

        [Description("Saída fiscal")]
        SaidaFiscal = 2,

        [Description("Entrada consumidor")]
        EntradaConsumidor = 3,

        [Description("Saída consumidor")]
        SaidaConsumidor = 4,

        [Description("Contas a pagar manual")]
        ContasAPagarManual = 5,

        [Description("Contas a receber manual")]
        ContasAReceberManual = 6,

        [Description("Ajuste de estoque")]
        AjusteEstoque = 7,

        [Description("Transferência")]
        Transferencia = 8,

        [Description("Avaria")]
        Avaria = 9,

        [Description("Saldo inicial")]
        SaldoInicial = 10
    }
}
