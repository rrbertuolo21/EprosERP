namespace Epros.ERP.Shared.Extensions
{
    public static class DecimalExtensions
    {
        public static decimal Arredonda2(this decimal valor)
        {
            return Math.Round(valor, 2, MidpointRounding.AwayFromZero);
        }

        public static decimal Arredonda(this decimal valor, int casas)
        {
            return Math.Round(valor, casas, MidpointRounding.AwayFromZero);
        }
    }

}
