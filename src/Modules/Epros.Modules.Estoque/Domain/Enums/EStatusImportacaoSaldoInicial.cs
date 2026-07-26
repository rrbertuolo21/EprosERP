using System.ComponentModel;

namespace Epros.Modules.Estoque.Domain.Enums
{
    /// <summary>
    /// Situação do processamento de uma importação de saldo inicial.
    /// EF Movimentação Manual e Ajustes §7.6 e §10.4.
    /// </summary>
    public enum EStatusImportacaoSaldoInicial
    {
        [Description("Pendente")]
        Pendente = 0,

        [Description("Processando")]
        Processando = 1,

        [Description("Concluída")]
        Concluida = 2,

        [Description("Concluída com erros")]
        ConcluidaComErros = 3
    }
}
