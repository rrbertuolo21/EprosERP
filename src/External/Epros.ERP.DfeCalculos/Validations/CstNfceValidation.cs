namespace Epros.ERP.DfeCalculos.Validations
{
    public class CstNfceValidation
    {
        public static bool Validar(string cstIcms)
        {
            string[] nfceCstValidos = ["00", "20", "40", "41", "60", "90", "02", "15", "53", "61"];
            return nfceCstValidos.Contains(cstIcms);
        }
    }
}
