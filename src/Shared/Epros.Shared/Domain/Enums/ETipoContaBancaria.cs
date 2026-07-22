using System.ComponentModel;

namespace Epros.Shared.Domain.Enums
{
    public enum ETipoContaBancaria
    {
        [Description("Conta Corrente")]
        ContaCorrente = 1,

        [Description("Conta Poupança")]
        ContaPoupanca = 2,

        [Description("Aplicações")]
        Aplicacoes = 3,

        [Description("Outras")]
        Outras = 4
    }
}
