using System.ComponentModel;

namespace Epros.Modules.Estoque.Domain.Enums
{
    /// <summary>
    /// Via de transporte da importação. Porte fiel do legado Epros.ERP.Shared.Enums.ETipoViaTransporte.
    /// </summary>
    public enum ETipoViaTransporte
    {
        [Description("Marítima")]
        Maritima = 1,

        [Description("Fluvial")]
        Fluvial,

        [Description("Lacustre")]
        Lacustre,

        [Description("Aérea")]
        Aerea,

        [Description("Postal")]
        Postal,

        [Description("Ferroviária")]
        Ferroviaria,

        [Description("Rodoviária")]
        Rodoviaria,

        [Description("Conduto / Rede de Transmissão")]
        CondutoRedeTransmissão,

        [Description("Meios próprios")]
        MeiosProprios,

        [Description("Entrada / Saída ficta")]
        EntradaSaidaficta,

        [Description("Courier")]
        Courier,

        [Description("Handcarry")]
        Handcarry,

        [Description("Por Reboque")]
        PorReboque
    }
}
