using System.ComponentModel;

namespace Epros.Modules.Financeiro.Domain.Enums
{
    /// <summary>Estado de consolidação: rascunho x oficializado (EF FIN-CON §7 / §10).</summary>
    public enum EEstadoConsolidacao
    {
        [Description("Provisório")] Provisorio = 0,
        [Description("Publicado")] Publicado = 1
    }
}
