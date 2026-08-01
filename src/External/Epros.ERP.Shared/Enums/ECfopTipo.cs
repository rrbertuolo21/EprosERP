using System.ComponentModel;

namespace Epros.ERP.Shared.Enums
{
    public enum ECfopTipo
    {
        [Description("Não Definido")]
        NaoDefinido = 0,

        [Description("Dentro Estado Entrada")]
        DentroEstadoEntrada = 1,

        [Description("Dentro Estado Saída")]
        DentroEstadoSaida = 5,

        [Description("Fora Estado Entrada")]
        ForaEstadoEntrada = 2,

        [Description("Fora Estado Saída")]
        ForaEstadoSaida = 6,

        [Description("Exterior Importação")]
        ExteriorImportacao = 3,

        [Description("Exterior Exportação")]
        ExteriorExportacao = 7
    }
}
