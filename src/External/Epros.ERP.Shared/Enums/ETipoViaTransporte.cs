using System.ComponentModel;

namespace Epros.ERP.Shared.Enums
{
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
