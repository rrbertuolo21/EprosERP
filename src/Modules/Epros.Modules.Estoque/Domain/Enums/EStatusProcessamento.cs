using System.ComponentModel;

namespace Epros.Modules.Estoque.Domain.Enums
{
    /// <summary>
    /// Status de processamento de uma etapa da importação. Porte fiel do legado
    /// Epros.ERP.Shared.Enums.EStatusProcessamento.
    /// </summary>
    public enum EStatusProcessamento
    {
        [Description("Não Processado")]
        NaoProcessado = 1,

        [Description("Processando")]
        Processando = 2,

        [Description("Finalizado")]
        Finalizado = 3,

        [Description("Com Erro")]
        Erro = 4
    }
}
