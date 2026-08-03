using System.ComponentModel;

namespace Epros.ERP.Shared.Enums
{
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
