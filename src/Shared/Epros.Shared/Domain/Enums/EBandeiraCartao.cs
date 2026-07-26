using System.ComponentModel;

namespace Epros.Shared.Domain.Enums
{
    public enum EBandeiraCartao
    {
        [Description("Não utiliza")]
        NaoUtiliza = -1,

        [Description("Visa")]
        bcVisa = 1,

        [Description("Mastercard")]
        bcMasterCard = 2,

        [Description("American Express")]
        bcAmericanExpress = 3,

        [Description("Sorocred")]
        bcSorocred = 4,

        [Description("Diners Club")]
        bcDinersClub = 5,

        [Description("Elo")]
        Elo = 6,

        [Description("Hipercard")]
        Hipercard = 7,

        [Description("Aura")]
        Aura = 8,

        [Description("Cabal")]
        Cabal = 9,

        [Description("Alelo")]
        Alelo = 10,

        [Description("Banes Card")]
        BanesCard = 11,

        [Description("CalCard")]
        CalCard = 12,

        [Description("Credz")]
        Credz = 13,

        [Description("Discover")]
        Discover = 14,

        [Description("GoodCard")]
        GoodCard = 15,

        [Description("GreenCard")]
        GreenCard = 16,

        [Description("Hiper")]
        Hiper = 17,

        [Description("JcB")]
        JcB = 18,

        [Description("Mais")]
        Mais = 19,

        [Description("MaxVan")]
        MaxVan = 20,

        [Description("Policard")]
        Policard = 21,

        [Description("RedeCompras")]
        RedeCompras = 22,

        [Description("Sodexo")]
        Sodexo = 23,

        [Description("ValeCard")]
        ValeCard = 24,

        [Description("Verocheque")]
        Verocheque = 25,

        [Description("VR")]
        VR = 26,

        [Description("Ticket")]
        Ticket = 27,

        [Description("Outros")]
        bcOutros = 99
    }
}
