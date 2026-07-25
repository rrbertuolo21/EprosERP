using System.ComponentModel;

namespace Epros.Modules.Financeiro.Domain.Enums
{
    /// <summary>Estado do centro de custo (EF FIN-CMG §8.1 / §11.1).</summary>
    public enum EEstadoCentroCusto
    {
        [Description("Ativo")] Ativo = 0,
        [Description("Inativo")] Inativo = 1
    }

    /// <summary>Tipo de título alvo da alocação gerencial (EF FIN-CMG §11.2).</summary>
    public enum ETipoTituloAlocacao
    {
        [Description("Contas a Pagar")] ContasAPagar = 0,
        [Description("Contas a Receber")] ContasAReceber = 1
    }
}
