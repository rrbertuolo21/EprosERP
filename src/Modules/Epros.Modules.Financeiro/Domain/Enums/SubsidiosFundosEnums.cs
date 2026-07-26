using System.ComponentModel;

namespace Epros.Modules.Financeiro.Domain.Enums
{
    /// <summary>Estado do programa de subsídio/fundo (EF FIN-SBF §11.1 / §12).</summary>
    public enum EEstadoProgramaSubsidio
    {
        [Description("Vigente")] Vigente = 0,
        [Description("Encerrado")] Encerrado = 1,
        [Description("Prestação de Contas")] PrestacaoContas = 2
    }
}
